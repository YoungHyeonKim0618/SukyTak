
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Effect/Satiety")]
public class SatietyEffectDataSO : SkillEffectDataSO
{
    public float SatietyAmount;
    public FloatChannelSO Channel;
    public override void OnUse()
    {
        Channel.OnRaise(SatietyAmount);
    }
}