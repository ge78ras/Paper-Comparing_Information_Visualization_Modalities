using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ButtonVariant
{
    [TextArea] public string text;
    public Sprite icon;

    public ButtonVariant()
    {
        text = "";
        icon = null;

    }

    public ButtonVariant(string text, Sprite icon)
    {
        this.text = text;
        this.icon = icon;
    }
}