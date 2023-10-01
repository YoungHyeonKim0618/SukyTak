
using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField]
    private ExpPerLevelDataSO _expData;
    // ------------------------------------------------------------------------
    // 현재 상태 정보
    // ------------------------------------------------------------------------
 
    private float _curHp;
    private int _curSatiety;
    private int _curLevel;
    private float _curExp;
    
    /*
     * Properties
     */
    public float CurHp
    {
        get { return _curHp; }
        set
        {
            DisplayHpModification(value - _curHp);
            _curHp = value;
            if(_curHp <= 0 )Player.Instance.Die();
            SyncHpAndSatietyWithUi();
        }
    } 
    public int CurSatiety
    {
        get { return _curSatiety; }
        set
        {
            /*
             * 현재 포만도가 0인 상태에서 포만도 감소 시 체력을 2배만큼 제거함
             */
            if (value < 0)
            {
                int rest = Mathf.Abs(value - _curSatiety);
                _curSatiety = 0;
                CurHp -= rest * 2;
            }
            else
            {
                DisplaySatietyModification(value - _curSatiety);    
                _curSatiety = value;
            }
            SyncHpAndSatietyWithUi();  
        }
    }
    public int CurLevel
    {
        get { return _curLevel; }
    }
    public float CurExp
    {
        get { return _curExp; }
        set
        {
            try
            {
                float tempExp = value;
                float maxExp = _expData.expPerLevelList[CurLevel];
                while (tempExp >= maxExp)
                {
                    tempExp -= maxExp;
                    LevelUp();
                    maxExp = _expData.expPerLevelList[CurLevel];
                }
            }
            catch(IndexOutOfRangeException e)
            {
                Debug.Log("No matching exp index in data!");
            }
        }
    }
    public void LevelUp()
    {
        _curLevel++;
    }
    // ------------------------------------------------------------------------
    // Status
    // ------------------------------------------------------------------------
 
    // 최대 체력
    private float _maxHp;  
    // 치명타 확률 (1.5배 대미지)
    private float _critChance; 
    // 추가 공격 차례 확률
    private float _additionalAttackChance;
    // 회피율    
    private float _dodge;       
    // 받는 피해 감소
    private float _dmgReduction;
 
    /*
     * Modifications
     * 특성이나 아이템 등이 수정한 값을 따로 저장한다.
     * 저장 시에는 이 값들은 무시하고 저장함.
     */
    private float _maxHpModification;
    private float _critChanceModification;
    private float _additionalAttackChanceModification;
    private float _dodgeModification;
    private float _dmgReductionModification;
    
    /*
     * Properties
     */


    public float MaxHp
    {
        get { return _maxHp + _maxHpModification; }
        set { _maxHpModification = value - _maxHp; }
    }

    public float CritChance
    {
        get { return _critChance + _critChanceModification; }
        set { _critChanceModification = value - _critChance; }
    }

    public float AdditionalAttackChance
    {
        get { return _additionalAttackChance + _additionalAttackChanceModification; }
        set { _additionalAttackChanceModification = value - _additionalAttackChance; }
    }

    public float Dodge
    {
        get { return _dodge + _dodgeModification; }
        set { _dodgeModification = value - _dodge; }
    }

    public float DmgReduction
    {
        get { return _dmgReduction + _dmgReductionModification; }
        set { _dmgReductionModification = value - _dmgReduction; }
    } 
    
    public void InitStatus()
    {
        _maxHp = GameConstantsSO.Instance.DefaultMaxHP;
        _curHp = _maxHp;
        _curSatiety = 50;

        _critChance = GameConstantsSO.Instance.DefaultCritChance;
        _additionalAttackChance = GameConstantsSO.Instance.DefaultAdditionalAttackChance;
        _dodge = GameConstantsSO.Instance.DefaultDodgeChance;
        _dmgReduction = GameConstantsSO.Instance.DefaultDamageReduction;
        
        SyncHpAndSatietyWithUi();
    }
    
    
    // ------------------------------------------------------------------------
    // UI와 동기화
    // ------------------------------------------------------------------------
    
    [Header("UI")][SerializeField]
    private List<Image> _hpBarImages;
    [SerializeField] 
    private List<Image> _satietyBarImages;

    [SerializeField] private List<TextMeshProUGUI> _hpTmps;
    [SerializeField] private List<TextMeshProUGUI> _satietyTmps;

    [SerializeField] private TextMeshProUGUI p_modTmp;
    
    [SerializeField] private Transform _modTmpParentCanvas;
    // 값이 변경되었을 때 생성되는 tmp의 시작 위치
    [SerializeField] private Transform _hpTmpStartingPos, _satietyTmpStartingPos;
    // 값이 변경되었을 때 두근거리는 효과를 받을 이미지
    [SerializeField] private Image _hpIcon, _satietyIcon;
    
    //TODO : 포만도와 체력에 따라 얼굴 sprite 바꿔주기

    private void SyncHpAndSatietyWithUi()
    {
        foreach (var image in _hpBarImages)
        {
            image.fillAmount = CurHp / MaxHp;
        }
        foreach (var image in _satietyBarImages)
        {
            image.fillAmount = (float) CurSatiety / GameConstantsSO.Instance.MaxSatiety;
        }
        foreach (var tmp in _hpTmps)
        {
            tmp.text = $"{CurHp:0.#} / {MaxHp:0.#}";
        }
        foreach (var tmp in _satietyTmps)
        {
            tmp.text = $"{CurSatiety} / {GameConstantsSO.Instance.MaxSatiety}";
        }
    }

    /*
     * 각각 체력과 포만도가 변경되었을 때 아이콘이 두근대는 효과와 텍스트 효과를 표기하는 메서드.
     */
    private void DisplayHpModification(float value)
    {
        // 두근거림 효과
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_hpIcon.transform.DOScale(new Vector3(1.2f, 1.2f, 1),0.2f));
        sequence.Append(_hpIcon.transform.DOScale(Vector3.one,0.2f));
        sequence.Play();
        
        // 텍스트 효과
        var tmp = Instantiate(p_modTmp, _hpTmpStartingPos.position, Quaternion.identity, _modTmpParentCanvas);
        tmp.text = value >= 0 ? $"<color=green>+ {value:0.#}</color>" : $"<color=red>{value:0.#}</color>";
        tmp.transform.DOLocalMoveY(tmp.transform.localPosition.y + 40f, 0.5f);
        Destroy(tmp.gameObject,0.75f);
    }

    private void DisplaySatietyModification(float value)
    {
        // 두근거림 효과
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_satietyIcon.transform.DOScale(new Vector3(1.2f, 1.2f, 1),0.2f));
        sequence.Append(_satietyIcon.transform.DOScale(Vector3.one,0.2f));
        sequence.Play();
        
        // 텍스트 효과
        var tmp = Instantiate(p_modTmp, _satietyTmpStartingPos.position, Quaternion.identity, _modTmpParentCanvas);
        tmp.text = value >= 0 ? $"<color=green>+ {value:0.#}</color>" : $"<color=red>{value:0.#}</color>";
        tmp.transform.DOLocalMoveY(tmp.transform.localPosition.y - 40f, 0.5f);
        Destroy(tmp.gameObject,0.75f);
    }
}