
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Active")]
public class ActiveSkillDataSO : SkillDataSO
{
    [Tooltip("최대 사용 횟수 (0 : Infinity)")]
    public int MaxCount;

    [Tooltip("사용 시 다음 사용까지 필요한 이동 횟수")]
    public int Cooldown;
    [Tooltip("시작 시 쿨다운")]
    public int StartingCooldown;

    [Tooltip("사용 시 효과")]
    public SkillEffectDataSO OnUseData;

    // 액티브 스킬 버튼을 눌렀을 때 호출되는 메서드.
    public void Use()
    {
        if(OnUseData != null)
            OnUseData.OnUse();
    }
}