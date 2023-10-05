
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataSO : ScriptableObject
{
    public string ID;
    public string Name;
    public Sprite Sprite;
    public ItemType Type;
    public bool Stackable;


    public virtual string GetString()
    {
        return "";
    }
}
