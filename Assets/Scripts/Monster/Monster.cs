
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class Monster : MonoBehaviour
{
    [Header("몬스터 데이터")]
    [SerializeField]
    private MonsterDataSO _data;
    public MonsterDataSO Data => _data;
    
    // ------------------------------------------------------------------------
    // 스탯 정보
    // ------------------------------------------------------------------------

    [SerializeField]
    private bool _alive;

    public bool Alive => _alive;
    
    
    private float _hp;
    private float Hp
    {
        get => _hp;
        set
        {
            // 0.0 단위까지 표기되기 때문에 표기된 값과 다른 결과를 내는 것 방지
            if (value <= 0.1f - float.Epsilon)
            {
                StartDie();
            }
            else _hp = value;
        }
    }

    // 적 처치시 획득하는 경험치
    private int RewardExp => _data.ExpPerDifficulty[(int)RoomManager.Instance.GetDifficulty()];
    
    public void InitMonster()
    {
        _alive = true;
        _hp = _data.MaxHP;
        _rooted = false;
    }

    public float GetDamage()
    {
        return Random.Range(_data.MinDamage, _data.MaxDamage);
    }

    // ------------------------------------------------------------------------
    // 애니메이션
    // ------------------------------------------------------------------------
    
    [Header("애니메이션")] [SerializeField] 
    private SkeletonAnimation _skeletonAnimation;
    public SkeletonAnimation SkeletonAnimation => _skeletonAnimation;

    public void SetFlipX(bool flip)
    {
        _skeletonAnimation.skeleton.ScaleX = flip ? -1 : 1;
    }
    public void StartAttack()
    {
        _skeletonAnimation.AnimationName = "Attack";
    }

    public void EndAttack()
    {
        _skeletonAnimation.loop = true;
        _skeletonAnimation.AnimationName = "Idle";
    }

    public void SetDamage(float dmg)
    {
        Hp -= dmg;
        _skeletonAnimation.loop = false;
        _skeletonAnimation.AnimationName = "BeAttacked";
    }

    /*
     * 몬스터가 죽으면 아예 사라지지는 않지만 다음 효과를 가진다.
     * - "MonsterDie" 애니메이션을 실행한다.
     * = PlayerBattleManager에서 "EndMonsterDie" 이벤트를 감지한다.
     * = 이후 EndBattle(), EnableInteractables() 등등...
     */
    private void StartDie()
    {
        _alive = false;
        _skeletonAnimation.loop = false;
        _skeletonAnimation.AnimationName = "MonsterDie";
        
        //TODO : 상호작용 비활성화
        
        
    }

    /*
     * PlayerBattleManager에서 "EndMonsterDie"를 감지하고 호출하는 메서드.
     */
    public void Die()
    {
        
        //TODO : 상호작용 활성화
        //TODO : 클릭 시 아이템 루팅
    }
    
    // ------------------------------------------------------------------------
    // 상호작용
    // ------------------------------------------------------------------------
    [Header("상호작용")][DisableInInspector]
    [SerializeField] private bool _clickable;
    [SerializeField] private bool _rooted;

    private List<ItemDataSO> _rootables = new List<ItemDataSO>();
    
    public void SetClickable(bool clickable)
    {
        _clickable = clickable;
    }

    public void AddRootable(ItemDataSO itemData)
    {
        _rootables.Add(itemData);
    }
    public void OnClick()
    {
        if (_clickable)
        {
            // 살아있을 때 클릭 (공격)
            if (_alive)
            {
                Player.Instance.OpenHitSelectionPanel(this);
            }
            // 죽었을 때 클릭 (파밍)
            else
            {
                if (_rootables.Count > 0 && !_rooted)
                    StartCoroutine(Root());
                _rooted = true;
            }
        }
    }

    private IEnumerator Root()
    {
        foreach (var data in _rootables)
        {
            Player.Instance.ObtainItem(data,transform.position);
            yield return new WaitForSeconds(0.3f);
        }
    }
}