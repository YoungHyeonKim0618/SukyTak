
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * 게임 시작 전 스킬 선택 창의 스킬 UI 클래스.
 */
public class SkillUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SkillDataSO _skill;
    [SerializeField] private MainSceneManager _manager;
    [SerializeField] private Image _background;
    [SerializeField] private Color _selectedColor;
    
    public SkillDataSO Skill => _skill;

    public void Init()
    {
        GetComponent<Image>().sprite = _skill.Sprite;
        GetComponent<Button>().onClick.AddListener(() => _manager.SelectSkill(_skill));
    }

    /*
     * Passive와 Active 스킬들 중 각각 하나씩만 선택되어야 하기 때문에 선택 시
     * 해당 SkillUI만 true, 나머지 모든 SkillUI는 false로 호출한다.
     */
    public void SetState(bool selected)
    {
        _background.color = selected ? _selectedColor : Color.black;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 스킬 설명 디스플레이
        _manager.DisplaySkill(_skill, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 스킬 설명 닫기
        _manager.HideSkill();
    }
}