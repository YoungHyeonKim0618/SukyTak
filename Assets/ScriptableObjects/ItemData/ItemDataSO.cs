
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

    // 이 아이템을 만들 수 있는 조합법들 (조합법은 다른 ItemDataSO의 List 형태)
    public List<ItemRecipe> Recipes;

    public virtual string GetString()
    {
        return "";
    }
}

[Serializable]
public struct ItemRecipe
{
    public List<ItemDataSO> materials;
}