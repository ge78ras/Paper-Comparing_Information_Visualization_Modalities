using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] RectTransform _outerFrame;
    [SerializeField] RectTransform _innerFrame;
    [SerializeField] RectTransform _screenProportions;
    [SerializeField] RectTransform _target;

    private Vector2 _outerScaleOffset;
    private Vector2 _canvasCenter;

    private float _minX;
    private float _maxX;
    private float _minY;
    private float _maxY;

    private void Awake()
    {
        SetupValues();
        RefactureThis();
    }

    private void SetupValues()
    {
        RectTransform canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        
        _outerScaleOffset = new Vector2(
            _outerFrame.rect.width * 0.5f, 
            _outerFrame.rect.height * 0.5f);

        _canvasCenter = new Vector2(
            canvas.rect.width * 0.5f, 
            canvas.rect.height * 0.5f);
    }

    private void LateUpdate()
    {
        SetPosition();
        SetScale();
    }

    public void PreserveAspectRatio(Texture2D tex)
    {
        float texWidth = tex.width;
        float texHeight = tex.height;

        if (texWidth > texHeight)
        {
            _outerFrame.localScale = new Vector3(1, texHeight / texWidth, 1);
        }
        else if (texWidth < texHeight)
        {
            _outerFrame.localScale = new Vector3(texWidth / texHeight, 1, 1);
        }
        else
        {
            _outerFrame.localScale = Vector3.one;
        }

        SetBoundaries();
    }

    private void RefactureThis() // TODO this should not aim at _innerFrame as the scale is overwritten in SetScale() aaaaaaa
    {
        float texWidth = _canvasCenter.x * 2;
        float texHeight = _canvasCenter.y * 2; // TODO this is so stupid

        if (texWidth > texHeight)
        {
            _screenProportions.localScale = new Vector3(texWidth / texHeight, 1, 1);
        }
        else if (texWidth < texHeight)
        {
            _screenProportions.localScale = new Vector3(1, texHeight / texWidth, 1);
        }
        else
        {
            _screenProportions.localScale = Vector3.one;
        }
    }

    private void SetBoundaries()
    {
        _minX = _outerFrame.anchoredPosition.x - _outerScaleOffset.x * _outerFrame.localScale.x;
        _maxX = _outerFrame.anchoredPosition.x + _outerScaleOffset.x * _outerFrame.localScale.x;
        _minY = _outerFrame.anchoredPosition.y - _outerScaleOffset.y * _outerFrame.localScale.y;
        _maxY = _outerFrame.anchoredPosition.y + _outerScaleOffset.y * _outerFrame.localScale.y;
    }

    private void SetPosition()
    {
        Vector2 pos = -_target.anchoredPosition;
        pos.x = RemapInterval(pos.x, -_canvasCenter.x, _canvasCenter.x, _minX, _maxX);
        pos.y = RemapInterval(pos.y, -_canvasCenter.y, _canvasCenter.y, _minY, _maxY);
        
        _innerFrame.anchoredPosition = pos;
    }

    private void SetScale()
    {
        _innerFrame.localScale = new Vector3(1f/_target.localScale.z, 1f/_target.localScale.z, 1f/_target.localScale.z);
    }

    private float RemapInterval(float value, float from1, float to1, float from2, float to2)
    {
        return (((value - from1) / (to1 - from1)) * (to2 - from2)) + from2;
    }
}