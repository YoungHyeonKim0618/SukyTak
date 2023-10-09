using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ItemData/MedicalData")]
public class MedicalData : ItemDataSO
{
    [Space(20)]
    public int Recovery;

    public override string GetString()
    {
        return $"체력 + {Recovery}";
    }
}