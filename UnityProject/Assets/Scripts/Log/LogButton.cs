using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogButton : MonoBehaviour
{
    [SerializeField] LogEventComponent self;
    [SerializeField] GameObject checkbox;

    public void Click()
    {
        LogManager.Instance.OnSelectQuestion(self);
    }

    private void Awake()
    {
        checkbox.SetActive(false);
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
        if (e.groupID != -1 && e.groupID != self.groupID) return;
        bool activate = e.selfID == self.selfID;
        checkbox.SetActive(activate);
    }
}