using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMPro.TMP_InputField))]
public class LogOtherText : MonoBehaviour
{
    [SerializeField] LogEventComponent self;
    TMPro.TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMPro.TMP_InputField>();
    }

    public void Click()
    {
        self.value = inputField.text;
        LogManager.Instance.SetOtherText(self);
    }

    private void Start()
    {
        LogManager.Instance.onSelectQuestion += SelectedQuestion;
    }

    private void OnDestroy()
    {
        LogManager.Instance.onSelectQuestion -= SelectedQuestion;
    }

    private void SelectedQuestion(LogEventComponent e)
    {
        if (e.groupID != -1 || e.selfID != -1) return;
        Reset();
    }

    private void Reset()
    {
        self.value = null;
        inputField.text = "";
    }
}
