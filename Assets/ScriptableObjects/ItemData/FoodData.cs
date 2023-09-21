using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/FoodData")]
public class FoodData : ItemDataSO
{
    [Space(20)]
    public int Satiety;
    public bool IsRotten;

    public override string GetString()
    {
        string ret =  $"배고픔 + {Satiety}";
        if (IsRotten) ret += $"\n<color=red>50% 확률로 {GameConstantsSO.Instance.RottenFoodDmg} 피해 입음</color>";

        return ret;
    }
}