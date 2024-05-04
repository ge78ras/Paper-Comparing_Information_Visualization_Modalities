using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    private static LogManager _instance;
    public static LogManager Instance { get { return _instance; } }

    private int _groupCount = 41;
    private string[] _selectedValues;
    private string[] _otherValues;

    void Awake()
    {
        if (_instance) { Destroy(gameObject); return; }
        _instance = this;

        _selectedValues = new string[_groupCount];
        _otherValues = new string[_groupCount];
    }

    private void Start()
    {
        ResetSelections();
        LanguageEnglish(true);
    }

    public event Action<LogEventComponent> onSelectQuestion;

    public void OnSelectQuestion(LogEventComponent e)
    {
        onSelectQuestion?.Invoke(e);

        _selectedValues[e.groupID] = e.value;
        CheckFinished(e.groupID);
    }

    public void SetOtherText(LogEventComponent e)
    {
        _otherValues[e.groupID] = e.value;
        LogEventComponent e2 = new LogEventComponent(e.groupID, e.selfID, "Other");
        OnSelectQuestion(e2);
        CheckFinished(e.groupID);
    }

    public void ResetSelections()
    {
        // -1 is broadcast ID
        LogEventComponent e = new LogEventComponent(-1, -1, "");
        onSelectQuestion?.Invoke(e);

        for (int i = 0; i < _selectedValues.Length; i++)
        {
            _selectedValues[i] = null;
            _otherValues[i] = null;
        }

        _finishedTask.SetActive(false);
        _finishedUEQ.SetActive(false);
        _finishedDemographic.SetActive(false);
        _finishedTaskGerman.SetActive(false);
        _finishedUEQGerman.SetActive(false);
        _finishedDemographicGerman.SetActive(false);

        DeactivateAllVisuals();
    }

    public string GetValue(int groupID)
    {
        string value = _selectedValues[groupID];
        if (value != null && value.ToLower().Contains("other")) value = _otherValues[groupID];
        return value;
    }

    public bool AllValuesSet(int from, int to) // refering to groupID, from inclusive, to exclusive
    {
        if (from < 0) from = 0;
        if (to > _selectedValues.Length) to = _selectedValues.Length;
        
        for (int i = from; i < to; i++)
        {
            string val = GetValue(i);
            if (val == null || val == "") return false;
        }
        
        return true;
    }

    public enum Variant {TASK_LAUNDRY, TASK_SAY_HI, UEQ_WORLD, UEQ_SCREEN, DEMOGRAPHIC}

    public void ActivateVisuals(Variant variant)
    {
        _pivotBackground.SetActive(true);

        switch(variant)
        {
            case Variant.TASK_LAUNDRY:
                _pivotTask.SetActive(true);
                _pivotHeaderLaundry.SetActive(true);
                _pivotHeaderSayHi.SetActive(false);
                _textTask1.text = "<b>Task 1</b>\nCount the number of final states that explicitly contain the phrase \"<b>do laundry</b>\" (but <b>not</b> \"Don't do laundry\"). Write down this <b>number</b> <i>(e.g. 42)</i>.";
                _textTask2.text = "<b>Task 2</b>\nFind the sequence of decisions that will arrive at the final state \"<b>Buy beer</b>\". Tick the corresponding decision nodes for this <b>sequence</b> <i>(e.g. No, Yes, Yes, No)</i>.";
                _textTask1German.text = "<b>Task 1</b>\nZähle die Endzustände, die explizit den Satz \"<b>do laundry</b>\" beinhalten (aber <b>nicht</b> \"Don’t do laundry\"). Schreibe diese <b>Zahl</b> auf <i>(z.B. 42)</i>.";
                _textTask2German.text = "<b>Task 2</b>\nFinde die Sequenz von Entscheidungen, die bei dem Endzustand ankommt: \"<b>Buy beer</b>\". Kreuze die entsprechenden Entscheidungen für diese <b>Sequenz</b> an <i>(z.B. No, Yes, Yes, No)</i>.";
                break;
            case Variant.TASK_SAY_HI:
                _pivotTask.SetActive(true);
                _pivotHeaderLaundry.SetActive(false);
                _pivotHeaderSayHi.SetActive(true);
                _textTask1.text = "<b>Task 1</b>\nCount the number of final states that explicitly contain the phrase \"<b>say hi</b>\" (but <b>not</b> \"Don't say hi\"). Write down this <b>number</b> <i>(e.g. 42)</i>.";
                _textTask2.text = "<b>Task 2</b>\nFind the sequence of decisions that will arrive at the final state \"<b>Share the money in case one of you gets caught</b>\". Tick the corresponding decision nodes for this <b>sequence</b> <i>(e.g. No, Yes, Yes, No)</i>.";
                _textTask1German.text = "<b>Task 1</b>\nZähle die Endzustände, die explizit den Satz \"<b>say hi</b>\" beinhalten (aber <b>nicht</b> \"Don't say hi\"). Schreibe diese <b>Zahl</b> auf <i>(z.B. 42)</i>.";
                _textTask2German.text = "<b>Task 2</b>\nFinde die Sequenz von Entscheidungen, die bei dem Endzustand ankommt: \"<b>Share the money in case one of you gets caught</b>\". Kreuze die entsprechenden Entscheidungen für diese <b>Sequenz</b> an <i>(z.B. No, Yes, Yes, No)</i>.";
                break;
            case Variant.UEQ_WORLD:
                _pivotUEQ.SetActive(true);
                _textHeaderUEQ.text = "Questionnaire: <b>World Space</b>";
                _textHeaderUEQGerman.text = "Fragebogen: <b>World Space</b>";
                break;
            case Variant.UEQ_SCREEN:
                _pivotUEQ.SetActive(true);
                _textHeaderUEQ.text = "Questionnaire: <b>Screen Space</b>";
                _textHeaderUEQGerman.text = "Fragebogen: <b>Screen Space</b>";
                break;
            case Variant.DEMOGRAPHIC:
                _pivotDemographic.SetActive(true);
                break;
            default: break;
        }
    }

    private void DeactivateAllVisuals()
    {
        _scrollbarUEQ.value = 1;
        _scrollbarDemographic.value = 1;
        _pivotTask.SetActive(false);
        _pivotUEQ.SetActive(false);
        _pivotDemographic.SetActive(false);
        _pivotBackground.SetActive(false);
    }

    public void DeactivateTasks()
    {
        _pivotTask.SetActive(false);
    }



    private Vector2Int _logsTask = new Vector2Int(0, 5);
    private Vector2Int _logsUEQ = new Vector2Int(5, 31);
    private Vector2Int _logsDemographic = new Vector2Int(31, 41);
    [SerializeField] Button _finishedTask;
    [SerializeField] Button _finishedUEQ;
    [SerializeField] Button _finishedDemographic;
    [SerializeField] Button _finishedTaskGerman;
    [SerializeField] Button _finishedUEQGerman;
    [SerializeField] Button _finishedDemographicGerman;
    [SerializeField] GameObject _pivotTask;
    [SerializeField] GameObject _pivotUEQ;
    [SerializeField] GameObject _pivotDemographic;

    [SerializeField] GameObject _pivotHeaderLaundry;
    [SerializeField] GameObject _pivotHeaderSayHi;
    [SerializeField] TMPro.TextMeshProUGUI _textTask1;
    [SerializeField] TMPro.TextMeshProUGUI _textTask2;
    [SerializeField] TMPro.TextMeshProUGUI _textHeaderUEQ;
    [SerializeField] TMPro.TextMeshProUGUI _textTask1German;
    [SerializeField] TMPro.TextMeshProUGUI _textTask2German;
    [SerializeField] TMPro.TextMeshProUGUI _textHeaderUEQGerman;

    [SerializeField] GameObject _pivotBackground;
    [SerializeField] Scrollbar _scrollbarUEQ;
    [SerializeField] Scrollbar _scrollbarDemographic;

    private void CheckFinished(int groupID)
    {
        Vector2Int range;
        Button button;

        if (groupID < _logsTask.y)
        {
            range = _logsTask;
            button = IsEnglish() ? _finishedTask : _finishedTaskGerman;
        }
        else if (groupID >= _logsDemographic.x)
        {
            range = _logsDemographic;
            button = IsEnglish() ? _finishedDemographic : _finishedDemographicGerman;
        }
        else
        {
            range = _logsUEQ;
            button = IsEnglish() ? _finishedUEQ : _finishedUEQGerman;
        }

        bool finished = AllValuesSet(range.x, range.y);
        if (finished) button.SetActive(true);
    }

    [SerializeField] private GameObject _englishTask1;
    [SerializeField] private GameObject _germanTask1;
    [SerializeField] private GameObject _englishTask2;
    [SerializeField] private GameObject _germanTask2;
    [SerializeField] private GameObject _englishUEQ;
    [SerializeField] private GameObject _germanUEQ;
    [SerializeField] private GameObject _englishDemographic;
    [SerializeField] private GameObject _germanDemographic;

    private void SetEnglish(bool active)
    {
        _englishTask1.SetActive(active);
        _englishTask2.SetActive(active);
        _englishUEQ.SetActive(active);
        _englishDemographic.SetActive(active);
    }

    private void SetGerman(bool active)
    {
        _germanTask1.SetActive(active);
        _germanTask2.SetActive(active);
        _germanUEQ.SetActive(active);
        _germanDemographic.SetActive(active);
    }

    private void LanguageEnglish(bool english)
    {
        SetEnglish(english);
        SetGerman(!english);
    }

    public void SwitchLanguage()
    {
        LanguageEnglish(!_englishUEQ.activeSelf);
    }

    public bool IsEnglish()
    {
        return _englishUEQ.activeSelf;
    }
}