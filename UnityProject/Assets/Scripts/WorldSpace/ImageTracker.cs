using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracker : MonoBehaviour
{
    [SerializeField] private GameObject _worldSpaceDiagramPrefab;
    private TouchRotate _activeWorldSpaceDiagram;
    private ARTrackedImageManager _trackedImageManager;
    private TargetIndicator _targetIndicator;
    [SerializeField] private GameObject _scanMarkerPrompt;

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        _targetIndicator = FindObjectOfType<TargetIndicator>();
        _scanMarkerPrompt?.SetActive(false);
    }

    private void Start()
    {
        _targetIndicator.Activate();
    }

    private void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs e)
    {
        if (e.added.Count > 0 && !_activeWorldSpaceDiagram)
        {
            _activeWorldSpaceDiagram = Instantiate(_worldSpaceDiagramPrefab, e.added[0].transform).GetComponentInChildren<TouchRotate>();
            _activeWorldSpaceDiagram.SetupLookAtMe();
            _activeWorldSpaceDiagram.SetupHighlight();
            _targetIndicator?.SetTarget(_activeWorldSpaceDiagram.GetComponentInChildren<Renderer>());
            _scanMarkerPrompt?.SetActive(false);
        }
        if (e.updated.Count > 0 && !_activeWorldSpaceDiagram.isActiveAndEnabled)
        {
            if (e.updated[0].trackingState != TrackingState.Tracking) return;
            _activeWorldSpaceDiagram.gameObject.SetActive(true);
            _activeWorldSpaceDiagram.Reset();
            _activeWorldSpaceDiagram.SetupLookAtMe();
            _activeWorldSpaceDiagram.SetupHighlight();
            _scanMarkerPrompt?.SetActive(false);
            _targetIndicator?.Activate();
        }
        if (e.removed.Count > 0 && _activeWorldSpaceDiagram)
        {
            Destroy(_activeWorldSpaceDiagram.gameObject);
            _scanMarkerPrompt?.SetActive(true);
        }
    }

    public void Activate()
    {
        _scanMarkerPrompt?.SetActive(true);
        _activeWorldSpaceDiagram?.SetupLookAtMe();
        _activeWorldSpaceDiagram?.SetupHighlight();
        _trackedImageManager.enabled = true;
    }

    public void Deactivate()
    {
        if (_activeWorldSpaceDiagram) _activeWorldSpaceDiagram.gameObject.SetActive(false);
        _trackedImageManager.enabled = false;
        _targetIndicator?.Deactivate();
        _scanMarkerPrompt?.SetActive(false);
    }

    public void LookAtMe()
    {
        _activeWorldSpaceDiagram?.LookAtMe();
    }

    public void PreserveAspectRatio(Texture2D tex)
    {
        _activeWorldSpaceDiagram?.PreserveAspectRatio(tex);
    }

    private void Update()
    {
        if (ButtonManager._logTimerWhole > 0 && !IsActive()) ButtonManager._logTimerBeforeScan += Time.deltaTime;
    }

    public void Reset()
    {
        _scanMarkerPrompt?.SetActive(true);
        _targetIndicator?.Deactivate();
        _activeWorldSpaceDiagram?.gameObject.SetActive(false);
        _activeWorldSpaceDiagram?.SetupLookAtMe();
        _activeWorldSpaceDiagram?.SetupHighlight();
    }

    public Transform GetTransform()
    {
        if (_activeWorldSpaceDiagram) return _activeWorldSpaceDiagram.transform; else return null;
    }

    public float GetRotation()
    {
        if (_activeWorldSpaceDiagram) return _activeWorldSpaceDiagram.GetRotation();
        else return float.NaN;
    }

    public bool IsActive()
    {
        return _activeWorldSpaceDiagram != null && _activeWorldSpaceDiagram.isActiveAndEnabled;
    }

    public bool IsVisible()
    {
        if (_activeWorldSpaceDiagram) return _activeWorldSpaceDiagram.IsVisible();
        else return false;
    }

    public float GetDistance()
    {
        if (!_activeWorldSpaceDiagram) return 0;
        return Vector3.Distance(Camera.main.transform.position, _activeWorldSpaceDiagram.transform.position);
    }
}