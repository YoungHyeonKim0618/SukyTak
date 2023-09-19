
using System;
using UnityEngine;

public class PlayerStatus
{
 private ExpPerLevelDataSO _expData;
 /*
  * Constructor
  */
 public PlayerStatus(ExpPerLevelDataSO data)
 {
  _expData = data;
 }
 // ------------------------------------------------------------------------
 // 현재 상태 정보
 // ------------------------------------------------------------------------
 
 private float _curHp;
 private float _curSatiety;
 private int _curLevel;
 private float _curExp;
 
 /*
  * Properties
  */
 public float CurHp
 {
  get { return _curHp; }
  set { _curHp = value; }
 }

 public float CurSatiety
 {
  get { return _curSatiety; }
  set { _curSatiety = value; }
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
}