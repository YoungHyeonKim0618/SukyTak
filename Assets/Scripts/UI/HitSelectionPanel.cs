
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * 적을 클릭했을 때 어떤 부위를 공격할 지 선택하는 패널.
 */
public class HitSelectionPanel : MonoBehaviour
{
    [SerializeField] private List<HitSelectionButton> _hitSelectionRoots;
    // 최대 3개까지 HitInfo를 가질 수 있으므로 이미지는 3개를 준비한다. (in Horizontal Layout Group)
    public void SetMonster(Monster monster)
    {
        int numHitSelects = Mathf.Clamp(monster.Data.HitInfos.Count,1,3);


        for (int i = 0; i < 3; i++)
        {
            // 데이터의 HitSelection 수만큼 활성화하고 설정해줌.
            if(i <numHitSelects)
            {
                MonsterHitInfo info = monster.Data.HitInfos[i];
                _hitSelectionRoots[i].SetHitInfo(info);
                _hitSelectionRoots[i].gameObject.SetActive(true);
            }
            else 
                _hitSelectionRoots[i].gameObject.SetActive(false);
        }
    }
}