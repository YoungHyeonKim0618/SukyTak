

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Register/StartGame")]
public class GameStartRegisterDataSO : SkillRegisterDataSO
{
    public VoidChannelSO OnStartGameChannel;
    public override void Register(SkillEffectDataSO effect)
    {
        OnStartGameChannel.OnEventRaised += effect.OnUse;
    }
}