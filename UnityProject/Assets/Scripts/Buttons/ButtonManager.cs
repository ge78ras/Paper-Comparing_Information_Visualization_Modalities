using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class ButtonManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private List<Button> _buttons;
    private List<Button> _hiddenButtons;
    private bool _hiddenButtonsActive;

    [Header("Diagrams")]
    // REQUIREMENT
    //  | 0 -> Mona Lisa
    //  | 1 -> Laundry
    //  | 2 -> Say Hi
    [SerializeField] private Texture2D[] _diagrams;
    private int _currentDiagram;
    [SerializeField] private Image _imageScreenSpace;
    [SerializeField] private Material _materialWorldSpace;

    [Header("Pivots")]
    [SerializeField] private GameObject _screenSpacePivot;
    [SerializeField] private GameObject _debugPivot;
    private bool _screenSpaceActive;
    private bool _debugActive;
    [SerializeField] private GameObject _switchMethodPivot;
    [SerializeField] private TMPro.TextMeshProUGUI _switchMethodText;
    [SerializeField] private GameObject _infoPivot;
    [SerializeField] private GameObject _pivotTasks;
    [SerializeField] private GameObject _taskButtonPivot;

    [Header("Other Scripts")]
    private TouchMoveScale _touchScale;
    private ImageTracker _imageTracker;
    private Minimap _minimap;

    [Header("Loggings")]
    [SerializeField] private TMPro.TextMeshProUGUI _textCurrentRun;
    [SerializeField] private TMPro.TextMeshProUGUI _textPath;
    [SerializeField] private TMPro.TextMeshProUGUI _textID;
    [SerializeField] private TMPro.TextMeshProUGUI _logTextTimerWhole;
    private int _logID;
    public static float _logTimerWhole = 0;
    public static float _logTimerWhole2 = 0;
    public static float _logTimerWork = 0;
    public static float _logTimerTask = 0;
    public static float _logTimerTask1Whole = 0;
    public static float _logTimerTask1Work = 0;
    public static float _logTimerTask1Task = 0;
    public static float _logTimerTask2Whole = 0;
    public static float _logTimerTask2Work = 0;
    public static float _logTimerTask2Task = 0;
    public static float _logTimerBehindAbs = 0;
    public static int _logTimerBehindTimes = 0;
    public static float _logTimerUnseenAbs = 0;
    public static int _logTimerUnseenTimes = 0;
    public static float _logTimerBeforeScan = 0;
    public static int _logButtonCenter = 0;
    public static int _logButtonLastPosition = 0;
    public static int _logButtonDoubleTap = 0;
    public static int _logButtonLookAtMe = 0;
    private float _logPollTimer = 0;
    private float _logPollInterval = 1;

    private void Awake()
    {
        _touchScale = FindObjectOfType<TouchMoveScale>();
        _imageTracker = FindObjectOfType<ImageTracker>();
        _minimap = FindObjectOfType<Minimap>();

        _touchScale.onZoomOut += CenterUndo;
        _touchScale.onZoomIn += CenterReset;
        _touchScale.onLeftInitialState += ActivateCenterButton;

        SetLogID(PlayerPrefs.GetInt("identifier", -1) + 1);
    }

    private void OnDisable()
    {
        _touchScale.onZoomOut -= CenterUndo;
        _touchScale.onZoomIn -= CenterReset;
        _touchScale.onLeftInitialState -= ActivateCenterButton;
    }

    private void Start()
    {
        SetupButtons();
        GetButton(Buttons.CENTER_UI).SetActive(false);
        GetButton(Buttons.LOOK_AT_ME_AR).SetActive(false);

        _screenSpaceActive = true;
        DeactivateWorldSpace();
        ActivateScreenSpace();

        _debugActive = false;
        DeactivateDebug();

        _currentDiagram = 0;
        SetDiagram(_currentDiagram);

        ShowSwitchMethod();

        SetupLogPoll();
        _textPath.text = "Persistent Filepath: " + Application.persistentDataPath;

        _taskButtonPivot.SetActive(false);
    }

    private void Update()
    {
        if (CheckHiddenButtons()) SetHiddenButtons(!_hiddenButtonsActive);
        if (_logTimerWhole > 0) HandleLogging();
        _textCurrentRun.text = "Run: " + _logCurrentRun;
    }

    private void SetupButtons()
    {
        _buttons = _buttons
            .OrderBy(x => x.GetID())
            .Distinct()
            .ToList();

        if (_buttons.Count != GetEnumLength(typeof(Buttons)))
        {
            Debug.LogError("Not all buttons have been assigned (see Buttons.cs enum)!");
        }

        _hiddenButtons = _buttons
            .Where(x => x.IsHidden())
            .ToList();
        SetHiddenButtons(false);
    }

    private int GetEnumLength(System.Type name)
    {
        return System.Enum.GetNames(name).Length;
    }

    private Button GetButton(Buttons id)
    {
        return _buttons[(int) id];
    }

    private bool CheckHiddenButtons()
    {
        return 
            Input.touchCount == 2 && 
            Input.GetTouch(0).tapCount == 2 && 
            Input.GetTouch(1).tapCount == 2 && 
                (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began || 
                Input.GetTouch(1).phase == UnityEngine.TouchPhase.Began);
    }

    private void SetHiddenButtons(bool active)
    {
        _hiddenButtonsActive = active;
        foreach (Button button in _hiddenButtons)
        {
            button.gameObject.SetActive(_hiddenButtonsActive);
        }
        if (active) HideSwitchMethod(); else if (_logTimerWhole == 0) ShowSwitchMethod();

        _infoPivot.SetActive(active);
    }

    // Buttons.SWITCH_SPACE
    public void SwitchSpace()
    {
        if (_screenSpaceActive)
        {
            DeactivateScreenSpace();
            ActivateWorldSpace();
        }
        else
        {
            DeactivateWorldSpace();
            ActivateScreenSpace();
        }

        _screenSpaceActive = !_screenSpaceActive;
    }

    private void ActivateWorldSpace()
    {
        GetButton(Buttons.SWITCH_SPACE).SetVariant(1);
        GetButton(Buttons.LOOK_AT_ME_AR).gameObject.SetActive(true);
        _imageTracker.Activate();
        _switchMethodText.text = "Active Method:\nWORLD SPACE";
    }

    private void DeactivateWorldSpace()
    {
        GetButton(Buttons.LOOK_AT_ME_AR).gameObject.SetActive(false);
        _imageTracker.Deactivate();
    }

    private void ActivateScreenSpace()
    {
        GetButton(Buttons.SWITCH_SPACE).SetVariant(0);
        GetButton(Buttons.CENTER_UI).gameObject.SetActive(true);
        _screenSpacePivot.SetActive(true);
        _switchMethodText.text = "Active Method:\nSCREEN SPACE";
    }

    private void DeactivateScreenSpace()
    {
        GetButton(Buttons.CENTER_UI).gameObject.SetActive(false);
        _screenSpacePivot.SetActive(false);
    }

    private void ShowSwitchMethod()
    {
        _switchMethodPivot.SetActive(true);
    }

    private void HideSwitchMethod()
    {
        _switchMethodPivot.SetActive(false);
    }

    // Buttons.DEBUG
    public void SwitchDebug()
    {
        if (_debugActive)
        {
            DeactivateDebug();
        }
        else
        {
            ActivateDebug();
        }

        _debugActive = !_debugActive;
    }

    private void ActivateDebug()
    {
        GetButton(Buttons.DEBUG).SetVariant(1);
        HighlightButton(Buttons.DEBUG, true);
        _debugPivot.SetActive(true);
    }

    private void DeactivateDebug()
    {
        _debugPivot.SetActive(false);
        return; // TODO falls ich Debug je wieder brauche
        GetButton(Buttons.DEBUG).SetVariant(0);
        HighlightButton(Buttons.DEBUG, false);
    }

    // Buttons.CENTER_UI
    public void CenterUI()
    {
        _touchScale?.Center();
    }

    // event TouchScale.onResetLastTransform
    private void CenterReset()
    {
        GetButton(Buttons.CENTER_UI).SetVariant(0);
    }

    // event TouchScale.onSaveLastTransform
    private void CenterUndo()
    {
        GetButton(Buttons.CENTER_UI).SetVariant(1);
    }

    private void ActivateCenterButton()
    {
        GetButton(Buttons.CENTER_UI).SetActive(true);
    }

    // Buttons.LOOK_AT_ME_AR
    public void LookAtMeAR()
    {
        _imageTracker?.LookAtMe();
    }

    // Buttons.SWITCH_DIAGRAM
    public void SwitchDiagram()
    {
        _currentDiagram = (_currentDiagram + 1) % _diagrams.Length;
        SetDiagram(_currentDiagram);
    }

    private void SetDiagram(int index)
    {
        _currentDiagram = index;
        Texture2D tex = _diagrams[index];
        _imageScreenSpace.sprite = TextureToSprite(tex);
        _materialWorldSpace.mainTexture = tex;
        _imageTracker?.PreserveAspectRatio(tex);
        _touchScale?.PreserveAspectRatio(tex);
        _minimap?.PreserveAspectRatio(tex);
    }

    private Sprite TextureToSprite(Texture2D tex)
    {
        Rect rect = new Rect(0, 0, tex.width, tex.height); // Uses the texture's dimensions
        Vector2 pivot = new Vector2(0.5f, 0.5f); // Values between 0 and 1, center of image
        return Sprite.Create(tex, rect, pivot);
    }

    public Texture2D GetMainTexture()
    {
        return _diagrams[_currentDiagram];
    }

    public void HighlightButton(Buttons id, bool highlightOn)
    {
        Button button = GetButton(id);
        string trigger = highlightOn ? "HighlightOn" : "HighlightOff";
        button.SetTrigger(trigger);
    }

    public void SetButtonActive(Buttons id, bool active)
    {
        GetButton(id).SetActive(active);
    }

    // Buttons.START_LAUNDRY
    public void StartLaundry()
    {
        if (_logTimerWhole == 0)
        {
            GetButton(Buttons.START_LAUNDRY).SetVariant(1);
            SetDiagram(1);
            StartLogging();
            LogManager.Instance.ActivateVisuals(LogManager.Variant.TASK_LAUNDRY);
            _pivotTasks.SetActive(false);
        }
        else
        {
            CancelLogging();
        }
    }

    // Buttons.START_SAY_HI
    public void StartSayHi()
    {
        if (_logTimerWhole == 0)
        {
            GetButton(Buttons.START_SAY_HI).SetVariant(1);
            SetDiagram(2);
            StartLogging();
            LogManager.Instance.ActivateVisuals(LogManager.Variant.TASK_SAY_HI);
            _pivotTasks.SetActive(false);
        }
        else
        {
            CancelLogging();
        }
    }

    private void StartLogging()
    {
        ResetEnvironment();
        ResetLogging();
        _logTimerWhole = Time.deltaTime;
        _logTimerWork = Time.deltaTime;
        _logTimerTask1Whole = Time.deltaTime;
        _logTimerTask1Work = Time.deltaTime;
        SetHiddenButtons(false);
        _taskButtonPivot.SetActive(true);
    }

    private void CancelLogging()
    {
        GetButton(Buttons.START_LAUNDRY).SetVariant(0);
        GetButton(Buttons.START_SAY_HI).SetVariant(0);
        DisplayLogging();
        SetDiagram(0);
        ResetLogging();
        ResetEnvironment();
        LogManager.Instance.ResetSelections();
        _taskButtonPivot.SetActive(false);
    }

    private void StopLogging()
    {
        PlayerPrefs.SetInt("identifier", _logID);
        CancelLogging();
        ShowSwitchMethod();
        _logCurrentRun++;
    }

    private void ResetEnvironment()
    {
        GetButton(Buttons.CENTER_UI).SetVariant(0);
        GetButton(Buttons.CENTER_UI).SetActive(false);
        _touchScale?.Reset();

        if (_screenSpaceActive) return;
        _imageTracker?.Reset();
    }

    private void DisplayLogging()
    {
        WriteLoggingGeneral();

        if (_screenSpaceActive)
        {
            WriteLoggingScale();
        }
        else
        {
            WriteLoggingRotation();
            WriteLoggingCamera();
            WriteLoggingDistance();
        }
    }

    private void WriteLoggingGeneral()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_logs_" + _logCurrentRun + (_screenSpaceActive ? "_screen_" : "_world_") + _currentDiagram +  ".txt";

        List<string> content = new List<string>();

        content.Add("##########################################");
        content.Add("General");
        content.Add("##########################################\n");

        content.Add("ID:\n" + _logID + "\n");
        content.Add("Method:\n" + (_screenSpaceActive ? "SCREEN SPACE" : "WORLD SPACE") + "\n");
        content.Add("Diagram:\n" + (_currentDiagram == 1 ? "Laundry" : "Say Hi") + "\n");
        content.Add("Run:\n" + _logCurrentRun + "\n");
        
        content.Add("\n##########################################");
        content.Add("Stats");
        content.Add("##########################################\n");

        content.Add("TimerWhole:\n" + _logTimerWhole2 + "\n");
        content.Add("TimerWorking:\n" + _logTimerWork + "\n");
        content.Add("TimerTaskSheet:\n" + _logTimerTask + "\n");
        
        if (_screenSpaceActive)
        {
            content.Add("ButtonCenter:\n" + _logButtonCenter + "\n");
            content.Add("ButtonLastPosition:\n" + _logButtonLastPosition + "\n");
            content.Add("ButtonDoubleTap:\n" + _logButtonDoubleTap + "\n");
        }
        else
        {
            if (_logTimerBehindTimes >= 2) _logTimerBehindTimes -= 2;
            content.Add("TimerBehindAbs:\n" + _logTimerBehindAbs + "\n");
            content.Add("TimerBehindTimes:\n" + _logTimerBehindTimes + "\n");
            content.Add("TimerBehindAvg:\n" + (_logTimerBehindAbs / _logTimerBehindTimes) + "\n");
            content.Add("TimerUnseenAbs:\n" + _logTimerUnseenAbs + "\n");
            content.Add("TimerUnseenTimes:\n" + _logTimerUnseenTimes + "\n");
            content.Add("TimerUnseenAvg:\n" + (_logTimerUnseenAbs / _logTimerUnseenTimes) + "\n");
            content.Add("TimerBeforeScan:\n" + _logTimerBeforeScan + "\n");

            content.Add("ButtonLookAtMe:\n" + _logButtonLookAtMe + "\n");
        }

        content.Add("\n##########################################");
        content.Add("Task results");
        content.Add("##########################################\n");

        content.Add("##########################################");
        content.Add("Task 1\n");

        content.Add("CompletionTimerWhole:\n" + _logTimerTask1Whole + "\n");
        content.Add("CompletionTimerWorking:\n" + _logTimerTask1Work + "\n");
        content.Add("CompletionTimerTaskSheet:\n" + _logTimerTask1Task + "\n");

        string result = LogManager.Instance.GetValue(0);
        content.Add(result);
        bool correct = false;
        if (int.TryParse(result, out int val)) correct = val == 3;
        content.Add("" + correct + "\n");

        content.Add("##########################################");
        content.Add("Task 2\n");

        content.Add("CompletionTimerWhole:\n" + _logTimerTask2Whole + "\n");
        content.Add("CompletionTimerWorking:\n" + _logTimerTask2Work + "\n");
        content.Add("CompletionTimerTaskSheet:\n" + _logTimerTask2Task + "\n");

        result = LogManager.Instance.GetValue(1);
        content.Add(result);
        if (_currentDiagram == 1) correct = result == "No"; else correct = result == "Yes";
        content.Add("" + correct + "\n");

        result = LogManager.Instance.GetValue(2);
        content.Add(result);
        correct = result == "No";
        content.Add("" + correct + "\n");

        result = LogManager.Instance.GetValue(3);
        content.Add(result);
        if (_currentDiagram == 1) correct = result == "No"; else correct = result == "Yes";
        content.Add("" + correct + "\n");

        result = LogManager.Instance.GetValue(4);
        content.Add(result);
        correct = result == "Yes";
        content.Add("" + correct + "\n");

        content.Add("\n##########################################");
        content.Add("UEQ results");
        content.Add("##########################################\n");

        for (int i = 5; i < 31; i++)
        {
            content.Add(LogManager.Instance.GetValue(i));
        }

        File.WriteAllLines(filePath, content);
    }

    private void WriteLoggingScale()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_logs_" + _logCurrentRun + "_scale.txt";

        List<string> content = new List<string>();

        content.Add("Stepsize:\n1/" + _logPollScaleDiscretizeSteps);
        content.Add("Max Value:\n" + _logPollScaleMaxValue);
        content.Add("Logged Time in [s]:\n" + _logPollScaleLoggedTimeSteps * _logPollInterval + "\n");

        for (int i = 0; i < _logPollScaleValues.Length; i++)
        {
            //float scaleValue = (float) i / _logPollDiscretizeSteps;
            //float intensity = _logPollScaleValues[i];
            content.Add("" + _logPollScaleValues[i]);
        }

        File.WriteAllLines(filePath, content);
    }

    private void WriteLoggingRotation()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_logs_" + _logCurrentRun + "_rotation.txt";

        List<string> content = new List<string>();

        content.Add("Stepsize in [degree]:\n1");
        content.Add("Max Value:\n360");
        content.Add("Logged Time in [s]:\n" + _logPollRotationLoggedTimeSteps * _logPollInterval + "\n");

        for (int i = 0; i < _logPollRotationValues.Length; i++)
        {
            content.Add("" + _logPollRotationValues[i]);
        }

        File.WriteAllLines(filePath, content);
    }

    private void WriteLoggingCamera()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_logs_" + _logCurrentRun + "_camera.txt";

        List<string> content = new List<string>();

        content.Add("Stepsize:\n1/" + _logPollCameraDiscretizeSteps);
        content.Add("Values:\n[-1;+1]");
        content.Add("Logged Time in [s]:\n" + _logPollCameraLoggedTimeSteps * _logPollInterval + "\n");

        for (int i = 0; i < _logPollCameraValues.Length; i++)
        {
            content.Add("" + _logPollCameraValues[i]);
        }

        File.WriteAllLines(filePath, content);
    }

    private void WriteLoggingDistance()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_logs_" + _logCurrentRun + "_distance.txt";

        List<string> content = new List<string>();

        content.Add("Stepsize:\n1/" + _logPollDistanceDiscretizeSteps);
        content.Add("Max Value:\n" + _logPollDistanceMaxValue);
        content.Add("Logged Time in [s]:\n" + _logPollDistanceLoggedTimeSteps * _logPollInterval + "\n");

        for (int i = 0; i < _logPollDistanceValues.Length; i++)
        {
            content.Add("" + _logPollDistanceValues[i]);
        }

        File.WriteAllLines(filePath, content);
    }

    private void WriteDemographic()
    {
        string filePath = Application.persistentDataPath + "/" + _logID + "_demographic.txt";

        List<string> content = new List<string>();

        content.Add("Age");
        content.Add(LogManager.Instance.GetValue(31) + "\n");

        content.Add("Gender");
        content.Add(LogManager.Instance.GetValue(32) + "\n");

        content.Add("Degree of education");
        content.Add(LogManager.Instance.GetValue(33) + "\n");

        content.Add("English");
        content.Add(LogManager.Instance.GetValue(34) + "\n");

        content.Add("AR in last 6 months");
        content.Add(LogManager.Instance.GetValue(35) + "\n");

        content.Add("AR device");
        content.Add(LogManager.Instance.GetValue(36) + "\n");

        content.Add("How often mobile games");
        content.Add(LogManager.Instance.GetValue(37) + "\n");

        content.Add("Shoulder height");
        content.Add(LogManager.Instance.GetValue(38) + "\n");

        content.Add("Table height");
        content.Add(LogManager.Instance.GetValue(39) + "\n");

        content.Add("Field of work/study");
        content.Add(LogManager.Instance.GetValue(40) + "\n");

        content.Add("Language\n" + (LogManager.Instance.IsEnglish() ? "English\n" : "German\n"));

        File.WriteAllLines(filePath, content);
    }

    private void ResetLogging()
    {
        _logTimerWhole = 0;
        _logTimerWhole2 = 0;
        _logTimerWork = 0;
        _logTimerTask = 0;
        _logTimerTask1Whole = 0;
        _logTimerTask1Work = 0;
        _logTimerTask1Task = 0;
        _logTimerTask2Whole = 0;
        _logTimerTask2Work = 0;
        _logTimerTask2Task = 0;
        _logTimerBehindAbs = 0;
        _logTimerBehindTimes = 0;
        _logTimerUnseenAbs = 0;
        _logTimerUnseenTimes = 0;
        _logTimerBeforeScan = 0;
        _logButtonCenter = 0;
        _logButtonLastPosition = 0;
        _logButtonDoubleTap = 0;
        _logButtonLookAtMe = 0;
        _logPollTimer = 0;
        _logPollScaleLoggedTimeSteps = 0;
        _logPollRotationLoggedTimeSteps = 0;
        _logPollCameraLoggedTimeSteps = 0;

        _pivotTasks.SetActive(true);
        
        // Scale
        for (int i = 0; i < _logPollScaleValues.Length; i++)
        {
            _logPollScaleValues[i] = 0;
        }

        // Rotation
        for (int i = 0; i < _logPollRotationValues.Length; i++)
        {
            _logPollRotationValues[i] = 0;
        }

        // Camera
        for (int i = 0; i < _logPollCameraValues.Length; i++)
        {
            _logPollCameraValues[i] = 0;
        }

        // Distance
        for (int i = 0; i < _logPollDistanceValues.Length; i++)
        {
            _logPollDistanceValues[i] = 0;
        }
    }

    private void HandleLogging()
    {
        _logTimerWhole += Time.deltaTime;
        if (TaskActive()) _logTimerTask += Time.deltaTime; else _logTimerWork += Time.deltaTime;

        if (_logTimerTask2Whole == 0)
        {
            _logTimerTask1Whole += Time.deltaTime;
            if (TaskActive()) _logTimerTask1Task += Time.deltaTime; else _logTimerTask1Work += Time.deltaTime;
        }
        else
        {
            _logTimerTask2Whole += Time.deltaTime;
            if (TaskActive()) _logTimerTask2Task += Time.deltaTime; else _logTimerTask2Work += Time.deltaTime;
        }

        _logPollTimer += Time.deltaTime;
        if (_logPollTimer >= _logPollInterval)
        {
            _logPollTimer -= _logPollInterval;

            if (_pivotTasks.activeSelf) return;

            if (_screenSpaceActive)
            {
                // Scale
                int val = Mathf.RoundToInt(_touchScale.GetAbsoluteScale() * _logPollScaleDiscretizeSteps);
                if (val >= _logPollScaleValues.Length) val = _logPollScaleValues.Length - 1;
                _logPollScaleValues[val]++;
                _logPollScaleLoggedTimeSteps++;
            }
            else
            {
                if (!_imageTracker.IsActive()) return;
                if (!_imageTracker.IsVisible()) return;

                // Rotation
                int val = Mathf.RoundToInt(_imageTracker.GetRotation()) % 360;
                _logPollRotationValues[val]++;
                _logPollRotationLoggedTimeSteps++;

                // Camera
                float dot = Vector3.Dot(_imageTracker.GetTransform().forward, Camera.main.transform.forward);
                val = Mathf.RoundToInt(_logPollCameraDiscretizeSteps * (dot + 1));
                val = Mathf.Clamp(val, 0, _logPollCameraValues.Length - 1);
                _logPollCameraValues[val]++;
                _logPollCameraLoggedTimeSteps++;

                // Distance
                val = Mathf.RoundToInt(_imageTracker.GetDistance() * _logPollDistanceDiscretizeSteps);
                if (val >= _logPollDistanceValues.Length) val = _logPollDistanceValues.Length - 1;
                _logPollDistanceValues[val]++;
                _logPollDistanceLoggedTimeSteps++;
            }
        }
    }

    // Scale
    private int _logPollScaleDiscretizeSteps = 10; // steps per 1 whole number
    private float[] _logPollScaleValues;
    private float _logPollScaleMaxValue;
    private int _logPollScaleLoggedTimeSteps;

    // Rotation
    private float[] _logPollRotationValues;
    private int _logPollRotationLoggedTimeSteps;

    // Camera
    private int _logPollCameraDiscretizeSteps = 100; // steps per 1 whole number
    private float[] _logPollCameraValues;
    private int _logPollCameraLoggedTimeSteps;

    // Distance
    private int _logPollDistanceDiscretizeSteps = 100; // steps per 1 whole number
    private float[] _logPollDistanceValues;
    private float _logPollDistanceMaxValue = 2;
    private int _logPollDistanceLoggedTimeSteps;

    private int _logCurrentRun = 1; // increments every runthrough of logging

    private void SetupLogPoll()
    {
        // Scale
        _logPollScaleMaxValue = TouchMoveScale.MAX_SCALE;
        int size = Mathf.RoundToInt(_logPollScaleMaxValue * _logPollScaleDiscretizeSteps) + 2; // "+1" for one extra slot for the zero and another "+1" for emergency buffer
        _logPollScaleValues = new float[size];

        // Rotation
        _logPollRotationValues = new float[360];

        // Camera
        size = _logPollCameraDiscretizeSteps * 2 + 1; // "*2" so that values from -1 to +1 and "+1" for the zero
        _logPollCameraValues = new float[size];

        // Distance
        size = Mathf.RoundToInt(_logPollDistanceMaxValue * _logPollDistanceDiscretizeSteps) + 2; // "+1" for one extra slot for the zero and another "+1" for emergency buffer
        _logPollDistanceValues = new float[size];
    }

    private void SetLogID(int val)
    {
        if (val < 0) return;
        _logID = val;
        _textID.text = "ID: " + _logID;
    }

    public void IncreaseID()
    {
        SetLogID( _logID + 1);
    }

    public void DecreaseID()
    {
        SetLogID(_logID - 1);
    }

    public void FinishSelection(int id)
    {
        // Tasks
        if (id == 0)
        {
            _logTimerWhole2 = _logTimerWhole;
            _logTimerWhole = 0;

            _taskButtonPivot.SetActive(false);
            LogManager.Instance.DeactivateTasks();

            if (_screenSpaceActive)
                LogManager.Instance.ActivateVisuals(LogManager.Variant.UEQ_SCREEN);
            else
                LogManager.Instance.ActivateVisuals(LogManager.Variant.UEQ_WORLD);
        }

        // UEQ
        if (id == 1)
        {
            StopLogging();
        }

        // Demographic
        if (id == 2)
        {
            WriteDemographic();
            LogManager.Instance.ResetSelections();
        }
    }

    // Buttons.DEMOGRAPHIC
    public void StartDemographic()
    {
        SetHiddenButtons(false);
        LogManager.Instance.ActivateVisuals(LogManager.Variant.DEMOGRAPHIC);
    }

    // Buttons.HIDE_TASKS
    public void HideTasks()
    {
        bool active = _pivotTasks.activeSelf;
        _pivotTasks.SetActive(!active);
    }

    public bool TaskActive()
    {
        return _pivotTasks.activeSelf;
    }

    public void Task1Completed()
    {
        if (_logTimerTask2Whole != 0) return;
        _logTimerTask2Whole = Time.deltaTime;
        _logTimerTask2Task = Time.deltaTime;
    }

    // Buttons.LANGUAGE
    public void SwitchLanguage()
    {
        LogManager.Instance.SwitchLanguage();
        int variant = LogManager.Instance.IsEnglish() ? 0 : 1;
        GetButton(Buttons.LANGUAGE).SetVariant(variant);
    }
}