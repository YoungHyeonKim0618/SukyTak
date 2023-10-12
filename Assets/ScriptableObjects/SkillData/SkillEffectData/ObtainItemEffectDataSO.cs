
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Effect/ObtainItem")]
public class ObtainItemEffectDataSO : SkillEffectDataSO
{
    public ItemDataSO ItemData;
    public ItemDataChannelSO Channel;
    public override void OnUse()
    {
        Channel.OnRaise(ItemData);
    }
}