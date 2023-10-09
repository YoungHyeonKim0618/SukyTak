
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Effect/MaxHp")]
public class MaxHpEffectDataSO : SkillEffectDataSO
{
    public float Value;
    public FloatChannelSO Channel;


    public override void OnUse()
    {
        Channel.OnRaise(Value);
    }
}