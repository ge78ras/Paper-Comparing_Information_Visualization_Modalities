using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchRotate : MonoBehaviour
{
    private float _rotationSpeed = 10.0f;
    private float _rotateDuration = 0.2f;
    private ButtonManager _buttonManager;
    private bool _isBackgroundVisible;
    private bool _isLookAtMeActive;
    [SerializeField] private Renderer _rendererBackground;
    [SerializeField] private Transform _parentPivot;

    private void Awake()
    {
        _buttonManager = FindObjectOfType<ButtonManager>();
        PreserveAspectRatio(_buttonManager.GetMainTexture());
    }

	private void Update()
    {
        if ((ButtonManager._logTimerWhole == 0 || !_buttonManager.TaskActive()) && Input.touchCount == 1) HandleRotate();
        HandleVisibilityCheck();
    }

    private void HandleRotate()
    {
        Touch touch = Input.GetTouch(0);
        float delta = touch.deltaPosition.y;
        float rotateFactor = delta * _rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.right, rotateFactor);
    }

    public void LookAtMe()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        StartCoroutine(SmoothRotate(targetRotation));
        ButtonManager._logButtonLookAtMe++;
    }

    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float i = 0;

        Quaternion initialRotation = transform.rotation;
        
        while (i < _rotateDuration)
        {
            i += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, Mathf.SmoothStep(0, 1, i / _rotateDuration));
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    public void Reset()
    {
        transform.localRotation = Quaternion.identity;
    }
    
    private void HandleVisibilityCheck()
    {
        // if the back of the plane is visible, the LookAtMe-Button is highlighted
        bool visible = CheckBackgroundVisible();
        if (visible != _isBackgroundVisible) SetHighlight(visible);
        if (visible && !_buttonManager.TaskActive() && ButtonManager._logTimerWhole != 0) ButtonManager._logTimerBehindAbs += Time.deltaTime;
        
        // if the plane is not visible at all, the LookAtMe-Button is greyed out
        bool active = _rendererBackground.isVisible;
        if (active != _isLookAtMeActive)
        {
            if (!active && !_buttonManager.TaskActive() && ButtonManager._logTimerWhole != 0) ButtonManager._logTimerUnseenTimes++;
            ActivateLookAtMe(active);
        }
        if (!active && !_buttonManager.TaskActive() && ButtonManager._logTimerWhole != 0) ButtonManager._logTimerUnseenAbs += Time.deltaTime;
    }

    private bool CheckBackgroundVisible()
    {
        Vector3 lookDir = Camera.main.transform.position - transform.position;
        float dot = Vector3.Dot(transform.forward, lookDir);
        return dot > 0 && _rendererBackground.isVisible;
    }

    private void SetHighlight(bool visible, bool setup = false)
    {
        _buttonManager.HighlightButton(Buttons.LOOK_AT_ME_AR, visible);
        _isBackgroundVisible = visible;
        if (visible && !setup) ButtonManager._logTimerBehindTimes++;
    }

    public void SetupHighlight()
    {
        SetHighlight(CheckBackgroundVisible(), true);
    }

    private void ActivateLookAtMe(bool active)
    {
        _buttonManager.SetButtonActive(Buttons.LOOK_AT_ME_AR, active);
        _isLookAtMeActive = active;
    }

    public void SetupLookAtMe()
    {
        ActivateLookAtMe(_rendererBackground.isVisible);
    }

    public void PreserveAspectRatio(Texture2D tex)
    {
        float texWidth = tex.width;
        float texHeight = tex.height;

        if (texWidth > texHeight)
        {
            transform.localScale = new Vector3(1, texHeight / texWidth, 1);
        }
        else if (texWidth < texHeight)
        {
            transform.localScale = new Vector3(texWidth / texHeight, 1, 1);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public bool IsVisible()
    {
        return _rendererBackground.isVisible;
    }

    public float GetRotation()
    {
        //return Vector3.Angle(transform.forward, _parentPivot.forward);
        //return Quaternion.FromToRotation(Vector3.up, transform.forward - _parentPivot.forward).eulerAngles.z;

        float angle = Vector3.Angle(transform.forward, _parentPivot.forward);
        return (Vector3.Angle(transform.up, _parentPivot.forward) > 90f) ? angle : 360f - angle;
    }
}