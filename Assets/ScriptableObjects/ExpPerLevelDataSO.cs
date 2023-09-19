
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ExpPerLevel")]
public class ExpPerLevelDataSO : ScriptableObject
{
    /*
     * 레벨별 필요한 최대 경험치를 저장하는 리스트.
     * 넉넉잡아 약 20레벨 정도까지 지정해놓자
     */
    public List<float> expPerLevelList;
}