
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Effect/Heal")]
public class HealEffectDataSO : SkillEffectDataSO
{
    public float HealAmount;
    // 플레이어와 이어주는 이벤트 채널 SO
    public FloatChannelSO Channel;
    
    public override void OnUse()
    {
        Channel.OnRaise(HealAmount);
    }
}