using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TargetIndicator : MonoBehaviour
{
    private bool _isActive = false;
    private float _offset = 25;
    [SerializeField] private Image _targetIndicator;
    private Renderer _targetRenderer;
    private RectTransform _canvas;
    private Vector3 _canvasCenter;

    void Awake()
    {
        _canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        _canvasCenter = new Vector3(_canvas.rect.width * 0.5f, _canvas.rect.height * 0.5f, 0f) * _canvas.localScale.x;
       EnableTargetIndicator(false);
    }

    public void SetTarget(Renderer renderer)
    {
        _targetRenderer = renderer;
    }

    void Update()
    {
        if (!_isActive) return;

        if (_targetRenderer) HandleTargetIndicator();
        else EnableTargetIndicator(false);
    }

    private void HandleTargetIndicator()
    {
        if (!EnableTargetIndicator(!_targetRenderer.isVisible)) return;

        SetPositionAndRotation();
    }

    private void SetPositionAndRotation()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(_targetRenderer.transform.position);
        if (pos.z < 0) pos *= -1; // Flip if object is behind camera
        pos = GetPosition(pos);
        _targetIndicator.rectTransform.position = pos;
        _targetIndicator.rectTransform.rotation = GetRotation(pos);
    }

    private bool EnableTargetIndicator(bool enable)
    {
        if (_targetIndicator.isActiveAndEnabled == !enable)
        {
            _targetIndicator.enabled = enable;
        }
        return enable;
    }

    private Vector3 GetPosition(Vector3 pos)
    {
        // Project onto 2D canvas coordinate space
        pos.z = 0;
        pos -= _canvasCenter;

        // Relative distance between canvas center and target
        float relativeX = (_canvas.rect.width * 0.5f - _offset) / Mathf.Abs(pos.x);
        float relativeY = (_canvas.rect.height * 0.5f - _offset) / Mathf.Abs(pos.y);

        // TargetIndicator is fixed on the x-border of the canvas
        if (relativeX < relativeY)
        {
            float angle = Vector3.SignedAngle(Vector3.right, pos, Vector3.forward);
            pos.x = Mathf.Sign(pos.x) * (_canvas.rect.width * 0.5f - _offset) * _canvas.localScale.x;
            pos.y = Mathf.Tan(Mathf.Deg2Rad * angle) * pos.x;
        }
        // TargetIndicator is fixed on the y-border of the canvas
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, pos, Vector3.forward);
            pos.y = Mathf.Sign(pos.y) * (_canvas.rect.height / 2f - _offset) * _canvas.localScale.y;
            pos.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * pos.y;
        }

        pos += _canvasCenter;
        return pos;
    }

    private Quaternion GetRotation(Vector3 pos)
    {
        float angle = Vector3.SignedAngle(Vector3.up, pos - _canvasCenter, Vector3.forward);
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return rotation;
    }

    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        EnableTargetIndicator(false);
    }
}