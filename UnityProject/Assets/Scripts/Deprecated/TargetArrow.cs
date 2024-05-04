using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TargetArrow : MonoBehaviour
{
    private bool _isActive = false;
    private Renderer _targetRenderer;
    [SerializeField] private GameObject _targetArrow;
    [SerializeField] private Camera _localCamera;

    void Awake()
    {
       EnableTargetArrow(false);
    }

    public void SetTarget(Renderer renderer)
    {
        _targetRenderer = renderer;
    }

    void Update()
    {
        if (!_isActive) return;

        if (_targetRenderer) HandleTargetIndicator();
        else EnableTargetArrow(false);
    }

    private void HandleTargetIndicator()
    {
        if (!EnableTargetArrow(!_targetRenderer.isVisible)) return;

        SetRotation();
    }

    private bool EnableTargetArrow(bool enable)
    {
        if (_targetArrow.activeSelf == !enable)
        {
            _targetArrow.SetActive(enable);
        }
        return enable;
    }

    private void SetRotation()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(_targetRenderer.transform.position);
        Vector3 newWorldPos = _localCamera.ViewportToWorldPoint(viewportPos);
        Quaternion newRot = Quaternion.LookRotation(Vector3.Normalize(newWorldPos - _targetArrow.transform.position), -Camera.main.transform.forward);
        _targetArrow.transform.rotation = newRot;
    }

    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        EnableTargetArrow(false);
    }
}