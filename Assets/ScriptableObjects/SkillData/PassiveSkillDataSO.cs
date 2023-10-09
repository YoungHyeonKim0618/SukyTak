
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * 패시브 스킬은 액티브와 다르게 사용자가 발동 시점을 지정하지 않는다.
 * 때문에 게임 시작 시 명시적으로 효과 발동 시점을 이벤트에 등록해야 함.
 */
[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Passive")]
public class PassiveSkillDataSO : SkillDataSO
{
    [Tooltip("이벤트 등록")]
    public List<SkillRegisterDataSO> RegisterDataList;
    [Tooltip("발동 시 효과")]
    public List<SkillEffectDataSO> EffectDataList;

    // 게임 시작 시 호출되는 메서드.
    public void Register()
    {
        for (int i = 0; i < RegisterDataList.Count; i++)
        {
            RegisterDataList[i].Register(EffectDataList[i]);
        }
    }
}