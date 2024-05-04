using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BetterOutline : MonoBehaviour
{
    private LineRenderer _lr;
    private RectTransform _rt;
    private Vector2 _scaleOffset;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _lr = gameObject.AddComponent<LineRenderer>();
        _lr.positionCount = 4;

        _scaleOffset = new Vector2(
            _rt.rect.width * 0.5f, 
            _rt.rect.height * 0.5f);
    }

    private void Update()
    {
        SetBoundaries();
    }

    private void SetBoundaries()
    {
        float minX = _rt.position.x - _scaleOffset.x * _rt.localScale.x;
        float maxX = _rt.position.x + _scaleOffset.x * _rt.localScale.x;
        float minY = _rt.position.y - _scaleOffset.y * _rt.localScale.y;
        float maxY = _rt.position.y + _scaleOffset.y * _rt.localScale.y;

        _lr.SetPosition(0, new Vector3(minX, minY, 0));
        _lr.SetPosition(1, new Vector3(minX, maxY, 0));
        _lr.SetPosition(2, new Vector3(maxX, maxY, 0));
        _lr.SetPosition(3, new Vector3(maxX, minY, 0));
    }
}