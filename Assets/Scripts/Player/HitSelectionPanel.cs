
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * 적을 클릭했을 때 어떤 부위를 공격할 지 선택하는 패널.
 */
public class HitSelectionPanel : MonoBehaviour
{
    [SerializeField] private List<RectTransform> _hitSelectionRoots;
    // 최대 3개까지 HitInfo를 가질 수 있으므로 이미지는 3개를 준비한다. (in Horizontal Layout Group)
    [SerializeField] private List<Image> _hitSelectionImages;
    // 각각 위의 Image에 자식으로 붙어 있는 
    [SerializeField] private List<TextMeshProUGUI> _hitSelectionTmps;
    public void SetMonster(Monster monster)
    {
        int numHitSelects = Mathf.Clamp(monster.data.HitInfos.Count,1,3);


        for (int i = 0; i < 3; i++)
        {
            // 데이터의 HitSelection 수만큼 활성화하고 설정해줌.
            if(i <numHitSelects)
            {
                MonsterHitInfo info = monster.data.HitInfos[i];
                _hitSelectionRoots[i].gameObject.SetActive(true);
                _hitSelectionImages[i].sprite = info.hitSprite;
                
                float coef = info.dmgCoefficient;
                float chance = info.hitChance;
                Vector2 dmg = Player.Instance.GetWeaponDamage() * coef * 0.01f;
                _hitSelectionTmps[i].text = $"{Mathf.RoundToInt(chance)}%\n<color=red>대미지: {dmg.x:0.##} - {dmg.y:0.##}</color>";
            }
            else 
                _hitSelectionRoots[i].gameObject.SetActive(false);
        }
    }
}