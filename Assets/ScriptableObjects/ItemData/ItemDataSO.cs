
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
}

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/WeaponData")]
public class WeaponDataSO : ItemDataSO
{
    [Space(20)]
    public float MinDmg;
    public float MaxDmg;

    public OnHitEffect OnHitEffect;
}

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/AccessoryData")]
public class AccessoryDataSO : ItemDataSO
{
    [Space(20)]
    public StatusEffect StatusEffect;
}

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/MedicalData")]
public class MedicalData : ItemDataSO
{
    [Space(20)]
    public int Recovery;
}

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/FoodData")]
public class FoodData : ItemDataSO
{
    [Space(20)]
    public int Satiety;
    public bool IsRotten;
}

[Serializable]
public struct ItemRecipe
{
    public List<ItemDataSO> materials;
}