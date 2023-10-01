
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    public string ID;
    public Sprite HeadSprite;

    public float MaxHP;
    public float MinDamage;
    public float MaxDamage;
    public float CritChance;
    
    public List<MonsterHitInfo> HitInfos;
    public List<int> ExpPerDifficulty;
}

[Serializable]
public struct MonsterHitInfo
{
    public Sprite hitSprite;
    [Header("대미지 배율 (%)")]
    // 대미지 배율
    public float dmgCoefficient;
    [Header("공격 적중 확률 (%)")]
    // 타격 확률
    public float hitChance;
}