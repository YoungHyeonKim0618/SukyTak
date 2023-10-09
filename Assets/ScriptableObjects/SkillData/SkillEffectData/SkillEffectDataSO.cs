
using UnityEngine;

/*
 * 각각의 SkillDataSO마다 Use()를 재정의하는 대신, 필요한 SkillEffectDataSO를 가지도록 한다.
 */
public abstract class SkillEffectDataSO : ScriptableObject
{
    public abstract void OnUse();
}