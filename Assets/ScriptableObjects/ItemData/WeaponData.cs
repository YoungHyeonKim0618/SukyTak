using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/WeaponData")]
public class WeaponData : ItemDataSO
{
    [Space(20)]
    public float MinDmg;
    public float MaxDmg;

    public OnHitEffect OnHitEffect;

    public override string GetString()
    {
        return $"대미지 : {MinDmg} - {MaxDmg}";
    }
}