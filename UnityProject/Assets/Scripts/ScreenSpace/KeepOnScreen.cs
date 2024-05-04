using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class KeepOnScreen : MonoBehaviour
{
    private float _stayVisible = 0.2f; // percentage of screen (min of width, height) that the image will still be visible in
    private RectTransform _rt;
    private Vector2 _scaleOffset; // Half Width and Height of the RectTransform-Component
    private Vector2 _canvasCenter; // Half Width and Height of the Canvas
    private float _visibilityOffset; // Absolute pixel value of the percentage @_stayVisible

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        SetupValues();
    }

    private void SetupValues()
    {
        RectTransform canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();

        _scaleOffset = new Vector2(
            _rt.rect.width * 0.5f, 
            _rt.rect.height * 0.5f);

        _canvasCenter = new Vector2(
            canvas.rect.width * 0.5f, 
            canvas.rect.height * 0.5f);

        _visibilityOffset = _stayVisible * Mathf.Min(
            canvas.rect.width, 
            canvas.rect.height);
    }
    
    private void LateUpdate()
    {
        HandleScreenClamping();
    }

    private void HandleScreenClamping()
    {
        // Left
        if (_rt.anchoredPosition.x + _scaleOffset.x * transform.localScale.x < -_canvasCenter.x + _visibilityOffset)
        {
            _rt.anchoredPosition = new Vector2(
                -_canvasCenter.x + _visibilityOffset - _scaleOffset.x * transform.localScale.x, 
                _rt.anchoredPosition.y);
        }

        // Right
        if (_rt.anchoredPosition.x - _scaleOffset.x * transform.localScale.x > _canvasCenter.x - _visibilityOffset)
        {
            _rt.anchoredPosition = new Vector2(
                _canvasCenter.x - _visibilityOffset + _scaleOffset.x * transform.localScale.x, 
                _rt.anchoredPosition.y);
        }

        // Bottom
        if (_rt.anchoredPosition.y + _scaleOffset.y * transform.localScale.y < -_canvasCenter.y + _visibilityOffset)
        {
            _rt.anchoredPosition = new Vector2(
                _rt.anchoredPosition.x, 
                -_canvasCenter.y + _visibilityOffset - _scaleOffset.y * transform.localScale.y);
        }

        // Top
        if (_rt.anchoredPosition.y - _scaleOffset.y * transform.localScale.y > _canvasCenter.y - _visibilityOffset)
        {
            _rt.anchoredPosition = new Vector2(
                _rt.anchoredPosition.x, 
                _canvasCenter.y - _visibilityOffset + _scaleOffset.y * transform.localScale.y);
        }
    }
}