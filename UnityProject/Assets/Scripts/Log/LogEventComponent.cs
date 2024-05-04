using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LogEventComponent
{
    public int groupID;
    public int selfID;
    public string value;

    public LogEventComponent(int groupID, int selfID, string value)
    {
        this.groupID = groupID;
        this.selfID = selfID;
        this.value = value;
    }
}