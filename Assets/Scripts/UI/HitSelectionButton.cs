
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HitSelectionButton : MonoBehaviour
{
    private MonsterHitInfo _hitInfo;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _tmp;
    public void SetHitInfo(MonsterHitInfo hitInfo)
    {
        _hitInfo = hitInfo;
        _image.sprite = hitInfo.hitSprite;
                
        float coef = hitInfo.dmgCoefficient;
        float chance = hitInfo.hitChance;
        Vector2 dmg = Player.Instance.GetWeaponDamage() * coef * 0.01f;
        
        _tmp.text = $"{Mathf.RoundToInt(chance)}%\n<color=red>대미지: {dmg.x:0.##} - {dmg.y:0.##}</color>";
    }

    public void OnClick()
    {
        Player.Instance.PlayerAttack(_hitInfo);
    }
}