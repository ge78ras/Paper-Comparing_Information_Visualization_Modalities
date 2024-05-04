using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchMoveScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // State (for more info see the DFA in ReadMe)
    private enum States {RESET, RESET_MOVED, RESET_ZOOMED, MOVED, ZOOMED};
    private enum Actions {IDLE, MOVING, SCALING, SLIDING, CENTER, DOUBLETAP};
    private States _state = States.RESET;
    private Actions _action = Actions.IDLE;

    // Pinch Scaling
    private float _initialDistance;
    private Vector3 _initialScale;
    public static float MAX_SCALE = 20f;

    // Double Tap Zoom
    private float _zoomDuration = 0.4f;
    private float _zoomAmount = 2.5f;
    private Vector3 _lastPosition;
    private Vector3 _lastScale;

    // Move with one Finger
    private Vector3 _offset;
    private Vector3 _slideVelocity = Vector3.zero;
    private float _slideMinVelocity = 0.1f;
    private float _slideDamping = 0.82f;
    
    // Other
    private Vector2 _scaleOffset;
    private RectTransform _rt;
    private RectTransform _canvasRect;

    // State machine
    private void SetAction(Actions action)
    {
        _slideVelocity = Vector3.zero; // Every action cancels out sliding

        _action = action;

        States oldState = _state;
        _state = GetState();

        // Reflexive edge
        if (oldState == _state) return;

        if (_state == States.RESET_MOVED || _state == States.RESET_ZOOMED) OnZoomOut();
        if (oldState == States.RESET_MOVED || oldState == States.RESET_ZOOMED) OnZoomIn();
        if (oldState == States.RESET) OnLeftInitialState();
    }

    // For more info see DFA
    private States GetState()
    {
        switch (_action)
        {
            case Actions.IDLE:
                return _state;
            case Actions.MOVING:
                if (_state == States.ZOOMED) return _state;
                return States.MOVED;
            case Actions.SCALING:
                return States.ZOOMED;
            case Actions.SLIDING:
                return _state;
            case Actions.CENTER:
                if (_state == States.MOVED) return States.RESET_MOVED;
                if (_state == States.ZOOMED) return States.RESET_ZOOMED;
                if (_state == States.RESET_MOVED) return States.MOVED;
                if (_state == States.RESET_ZOOMED) return States.ZOOMED;
                return _state;
            case Actions.DOUBLETAP:
                if (_state == States.ZOOMED) return States.RESET_ZOOMED;
                return States.ZOOMED;
            default:
                return _state;
        }
    }

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _canvasRect = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
    }

    private void Start()
    {
        _lastPosition = transform.localPosition;
        _lastScale = transform.localScale;

        _scaleOffset = new Vector2(
            _rt.rect.width * 0.5f, 
            _rt.rect.height * 0.5f);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (DoNotInterruptAction()) return;

        int fingersDown = GetActiveTouches(); // Always >= 1 because OnPointerDown()

        if (fingersDown == 2) InitiateScale();

        if (fingersDown != 1) return;

        Touch touch = Input.GetTouch(0);
        if (touch.tapCount == 2) DoubleTap(touch.position);
        if (touch.tapCount == 1) InitiateMove();
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (DoNotInterruptAction()) return;

        int fingersDown = GetActiveTouches();

        SetAction(Actions.IDLE);

        if (fingersDown == 1) InitiateMove();
        if (fingersDown == 0) EndMove();
    }
    
	private void Update()
    {
        if (Input.touchCount > 2) return;
        if (_action == Actions.SCALING) HandleScale(); // Input.touchCount == 2 holds
        if (_action == Actions.MOVING) HandleMove(); // Input.touchCount == 1 holds
        if (_action == Actions.SLIDING) HandleSlide(); // Input.touchCount == 0 holds
    }

    private int GetActiveTouches()
    {
        int counter = 0;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if ((touch.phase == TouchPhase.Began 
            || touch.phase == TouchPhase.Moved 
            || touch.phase == TouchPhase.Stationary)
            && IsOnRectTransform(touch.position))
            {
                counter++;
            }
        }
        
        return counter;
    }

    private bool IsOnRectTransform(Vector2 posScreenSpace)
    {
        Vector2 posCanvasSpace;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, posScreenSpace, null, out posCanvasSpace);

        return posCanvasSpace.x <= _rt.anchoredPosition.x + _scaleOffset.x * transform.localScale.x
            && posCanvasSpace.x >= _rt.anchoredPosition.x - _scaleOffset.x * transform.localScale.x
            && posCanvasSpace.y <= _rt.anchoredPosition.y + _scaleOffset.y * transform.localScale.y
            && posCanvasSpace.y >= _rt.anchoredPosition.y - _scaleOffset.y * transform.localScale.y;
    }

    private void InitiateScale()
    {
        _initialDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
        _initialScale = transform.localScale;
        SetAction(Actions.SCALING);
    }

    private void InitiateMove()
    {
        _offset = transform.position - GetFirstActiveTouch();
        SetAction(Actions.MOVING);
    }

    private void EndMove()
    {
        if (Input.touchCount == 0 || Mathf.Approximately(Time.deltaTime, 0f)) return;

        SetAction(Actions.SLIDING);
        _slideVelocity = Input.GetTouch(0).deltaPosition / Time.deltaTime;
    }

    private void HandleSlide()
    {
        if (_slideVelocity.sqrMagnitude < _slideMinVelocity)
        {
            SetAction(Actions.IDLE);
            return;
        }

        transform.position += _slideVelocity * Time.deltaTime;
        _slideVelocity *= _slideDamping;
    }

    private void HandleMove()
    {
        transform.position = GetFirstActiveTouch() + _offset;
    }

    private Vector3 GetFirstActiveTouch()
    {
        Touch touch = Input.GetTouch(0);

        if ((touch.phase == TouchPhase.Ended
        || touch.phase == TouchPhase.Canceled
        || !IsOnRectTransform(touch.position))
        && Input.touchCount == 2)
        {
            touch = Input.GetTouch(1);
        }

        return touch.position;
    }

    private void HandleScale()
    {
        Touch touch0 = Input.GetTouch(0); 
        Touch touch1 = Input.GetTouch(1);

        float currentDistance = Vector2.Distance(touch0.position, touch1.position);
        if (Mathf.Approximately(_initialDistance, 0)) return; // Avoid DivisionByZero
        float scaleFactor = currentDistance / _initialDistance;

        // Scale
        Vector3 pivot = GetPivot(touch0, touch1);
        Vector3 newScale = _initialScale * scaleFactor;
        // Clamp
        if (newScale.z > MAX_SCALE)
        {
            float factor = newScale.z / MAX_SCALE;
            newScale /= factor;
        }
        ScaleAround(transform, pivot, newScale);

        // Move with two fingers
        transform.position += GetPivotDelta(touch0, touch1);
    }

    // Pivot point for scaling
    private Vector2 GetPivot(Touch touch0, Touch touch1)
    {
        return WeightedValue(touch0, touch1, touch0.position, touch1.position);
    }

    // Pivot point for moving with two fingers
    private Vector3 GetPivotDelta(Touch touch0, Touch touch1)
    {
        return WeightedValue(touch0, touch1, touch0.deltaPosition, touch1.deltaPosition);
    }

    // Interpolates between two values, according to the deltaMovement of two touches
    private Vector2 WeightedValue(Touch touch0, Touch touch1, Vector3 value0, Vector3 value1)
    {
        // the exact magnitude is not needed since the 2 touches just need to be comparable and sqrMagnitude doesn't perform the expensive root-operation
        float delta0 = touch0.deltaPosition.sqrMagnitude;
        float delta1 = touch1.deltaPosition.sqrMagnitude;

        if (delta0 > delta1)
        {
            return LerpTouches(delta1, value1, delta0, value0);
        }
        if (delta1 > delta0)
        {
            return LerpTouches(delta0, value0, delta1, value1);
        }

        // Both deltas are equal (maybe 0) -> chose midpoint
        return (value0 + value1) * 0.5f;
    }

    private Vector2 LerpTouches(float deltaSmaller, Vector2 posSmaller, float deltaBigger, Vector2 posBigger)
    {
        // deltaBigger > deltaSmaller and deltaSmaller >= 0 hold -> no DivisionByZero possible
        float factor = deltaSmaller / deltaBigger;
        factor *= 0.5f;
        // Example Use Case: Get pivot point between two touches for scaling
        //      Both touches have moved the same -> chose midpoint between touches (Lerp(0.5))
        //      One touch has not moved at all -> chose stationary touch (Lerp(0))
        //      As factor is bound between 0 and 0.5, any Lerp(>0.5) does not need to be considered
        Vector2 pivot = Vector2.Lerp(posSmaller, posBigger, factor);
        return pivot;
    }

    private void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
    {
        Vector3 pivotDelta = target.position - pivot;
        Vector3 scaleFactor = new Vector3(newScale.x / target.localScale.x, newScale.y / target.localScale.y, newScale.z / target.localScale.z );
        pivotDelta.Scale(scaleFactor);

        target.localScale = newScale;
        target.position = pivot + pivotDelta;
    }

    private bool DoNotInterruptAction()
    {
        return _action == Actions.CENTER || _action == Actions.DOUBLETAP;
    }

    public void Center()
    {
        if (DoNotInterruptAction()) return;

        SetAction(Actions.CENTER);

        if (_state == States.RESET_MOVED || _state == States.RESET_ZOOMED)
        {
            // Zoom Out
            StartCoroutine(SmoothZoom(Vector3.zero, GetResetScale(), false));
            ButtonManager._logButtonCenter++;
        }
        if (_state == States.MOVED || _state == States.ZOOMED)
        {
            // Zoom In
            StartCoroutine(SmoothZoom(_lastPosition, _lastScale, false));
            ButtonManager._logButtonLastPosition++;
        }
    }

    private void DoubleTap(Vector2 pivot)
    {
        if (DoNotInterruptAction()) return;

        SetAction(Actions.DOUBLETAP);

        if (_state == States.RESET_ZOOMED)
        {
            // Zoom Out
            StartCoroutine(SmoothZoom(Vector3.zero, GetResetScale(), false));
        }
        if (_state == States.ZOOMED)
        {
            // Zoom In
            Vector3 targetScale = transform.localScale + transform.localScale * _zoomAmount;
            StartCoroutine(SmoothZoom(pivot, targetScale, true));
        }
        ButtonManager._logButtonDoubleTap++;
    }

    // @scaleAroundTargetPosition
    //  true    -> Change scale,                scale around @targetPosition as pivot
    //  false   -> Change scale and position,   scale around center as pivot
    private IEnumerator SmoothZoom(Vector3 targetPosition, Vector3 targetScale, bool scaleAroundTargetPosition)
    {
        float i = 0;

        Vector3 initialPosition = transform.localPosition;
        Vector3 initialScale = transform.localScale;

        while (i < _zoomDuration)
        {
            i += Time.deltaTime;

            if (scaleAroundTargetPosition)
            {
                ScaleAround(transform, targetPosition, Vector3.Slerp(initialScale, targetScale, Mathf.SmoothStep(0, 1, i / _zoomDuration)));
            }
            else
            {
                transform.localPosition = Vector3.Slerp(initialPosition, targetPosition, Mathf.SmoothStep(0, 1, i / _zoomDuration));
                transform.localScale = Vector3.Slerp(initialScale, targetScale, Mathf.SmoothStep(0, 1, i / _zoomDuration));
            }

            yield return null;
        }

        if (scaleAroundTargetPosition)
        {
            ScaleAround(transform, targetPosition, targetScale);
        }
        else
        {
        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
        }

        SetAction(Actions.IDLE);
    }

    public event Action onZoomOut;

    private void OnZoomOut()
    {
        _lastPosition = transform.localPosition;
        _lastScale = transform.localScale;

        if (onZoomOut != null) onZoomOut();
    }

    public event Action onZoomIn;

    private void OnZoomIn()
    {
        if (onZoomIn != null) onZoomIn();
    }

    public event Action onLeftInitialState;

    private void OnLeftInitialState()
    {
        if (onLeftInitialState != null) onLeftInitialState();
    }

    // x and y components of localScale may vary due to different aspect ratios of the main texture sprite
    public float GetAbsoluteScale()
    {
        return transform.localScale.z;
    }

    public Vector3 GetResetScale()
    {
        return new Vector3(
            transform.localScale.x / GetAbsoluteScale(), 
            transform.localScale.y / GetAbsoluteScale(), 
            transform.localScale.z / GetAbsoluteScale());
    }

    public void PreserveAspectRatio(Texture2D tex)
    {
        float texWidth = tex.width;
        float texHeight = tex.height;

        if (texWidth > texHeight)
        {
            transform.localScale = new Vector3(
                GetAbsoluteScale(), 
                GetAbsoluteScale() * (texHeight / texWidth), 
                GetAbsoluteScale());
        }
        else if (texWidth < texHeight)
        {
            transform.localScale = new Vector3(
                GetAbsoluteScale() * (texWidth / texHeight), 
                GetAbsoluteScale(), 
                GetAbsoluteScale());
        }
        else
        {
            transform.localScale = GetAbsoluteScale() * Vector3.one;
        }
    }

    public void Reset()
    {
        _state = States.RESET;
        _action = Actions.IDLE;
        _slideVelocity = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = GetResetScale();
    }
}