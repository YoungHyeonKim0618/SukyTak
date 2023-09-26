
using System;
using System.Collections;
using Spine;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerBattleManager : MonoBehaviour
{

    /*
     * 플레이어나 몬스터의 공격 애니메이션 중 다른 행동을 하지 못하게 하는 상위 레이어의 투명한 이미지.
     */
    [SerializeField] private Image _raycastBlocker;
    

    private void Start()
    {
        UnblockRaycast();
        CloseHitSelectionPanel();
        HideEquippedWeapons();
    }

    public void BlockRaycast()
    {
        _raycastBlocker.gameObject.SetActive(true);
    }
    public void UnblockRaycast()
    {
        _raycastBlocker.gameObject.SetActive(false);
    }
    
    // ------------------------------------------------------------------------
    // 전투 상태
    // ------------------------------------------------------------------------

    [Header("전투 상태")][DisableInInspector]
    [SerializeField] private bool _battleOngoing;
    
    // 가방을 여는 버튼
    [SerializeField] private GameObject _inventoryButton;
    
    // 현재 마주하고 있는 몬스터.
    [SerializeField, DisableInInspector]
    private Monster _facingMonster;

    // 살아있는 몬스터가 있는 방에 진입할 때 호출되는 메서드
    public void EncounterMonster(Monster monster)
    {
        _facingMonster = monster;
        DisplayEquippedWeapons();
        RegisterMonsterSpineEvent(monster);
        
        _facingMonster.SkeletonAnimation.loop = true;
        _facingMonster.SkeletonAnimation.AnimationName = "Idle";
        
        //TODO : 이동을 완료한 후에 공격당함.
        CheckMonsterAttackFirst();
    }

    // 살아있는 몬스터가 있는 방을 나가거나, 모든 몬스터를 죽였을 때 호출되는 메서드
    public void LeaveMonster()
    {
        HideEquippedWeapons();
        RemoveMonsterSpineEvent(_facingMonster);
        _facingMonster.SkeletonAnimation.ClearState();
        
        // 현재 위의 Spine 관련 코드가 바로 적용되지 않아서 NullException이 나옴
        //_facingMonster = null;
    }
    
    public void BeginBattle()
    {
        if(!_battleOngoing)
        {
            _battleOngoing = true;
            _inventoryButton.SetActive(false);
        }
    }

    public void EndBattle()
    {
        if(_battleOngoing)
        {
            _battleOngoing = false;
            _inventoryButton.SetActive(true);
            LeaveMonster();
        }
    }

    public IEnumerator RunFromMonster()
    {
        // 전투 중일 때에만 기다림
        if (_battleOngoing)
        {
            MonsterAttack();
            var track = _facingMonster.SkeletonAnimation.state.GetCurrent(0);
            yield return new WaitForSpineAnimationComplete(track);
            EndBattle();
        }
    }
    // ------------------------------------------------------------------------
    // 전투
    // ------------------------------------------------------------------------

    [Header("전투")] 

    [SerializeField]
    private MonsterHitInfo _selectedHitInfo;
    
    public void PlayerAttack(MonsterHitInfo hitInfo)
    {
        // 전투 시작 
        BeginBattle();
        
        // 공격 선택 패널 비활성화
        CloseHitSelectionPanel();
        _selectedHitInfo = hitInfo;
        
        // 플레이어의 공격 애니메이션 실행
        Player.Instance.SetAnimationLoop(false);
        //TODO : 현재 선택한 무기별로 다른 애니메이션 실행
        Player.Instance.SetAnimationState("AttackFist");
    }

    /*
     * 상대 몬스터에게 실제로 피해를 입히는 메서드
     */
    private void PlayerHit()
    {
        bool hit = Random.Range(0, 100) < _selectedHitInfo.hitChance;
        print($"chance : {_selectedHitInfo.hitChance}, hit : {hit}");

        // 공격 적중시
        if (hit)
        {
            Vector2 weaponDmg = Player.Instance.GetWeaponDamage();
            float coef = _selectedHitInfo.dmgCoefficient * 0.01f;
            float dmg = Random.Range(weaponDmg.x * coef, weaponDmg.y * coef);
            _facingMonster.SetDamage(dmg);

            Vector3 vec3 = _facingMonster.transform.position + new Vector3(0, 1, 0);
            
            bool crit = Random.Range(0,100) < Player.Instance.Status.CritChance;
            dmg = crit ? dmg * 1.5f : dmg;
            vec3.z = crit ? 1 : -1;
            _channel.OnEventRaised(dmg, vec3);
        }
        // 공격 미적중시
        else
        {
            //TODO : 비주얼
            _channel.OnEventRaised(-1, _facingMonster.transform.position + new Vector3(0,1,0));
        }
    }

    private void MonsterAttack()
    {
        Player.Instance.SetMovable(false);
        _facingMonster.SetClickable(false);
        _facingMonster.StartAttack();
    }

    /*
     * 플레이어에게 피해를 입히는 메서드
     */
    private void MonsterHit()
    {
        Vector3 vec3 = Player.Instance.transform.position + new Vector3(0, 1, 0);
        
        float dmg = _facingMonster.GetDamage();
        bool crit = Random.Range(0, 100) < _facingMonster.Data.CritChance;
        dmg = crit ? dmg * 1.5f : dmg;

        vec3.z = crit ? 1 : -1;
        
        Player.Instance.ModifyHp(-dmg);
        _channel.OnEventRaised(dmg, vec3);
    }

    /*
     * 적이 있는 방에 들어올 때 호출되는 메서드로, 적이 선공을 가할 것인지 판단하고
     * 만약 그렇다면 전투를 시작하고 공격한다.
     */
    public void CheckMonsterAttackFirst()
    {
        if (Random.Range(0, 100) < GameConstantsSO.Instance.MonsterAttackFirstChance)
        {
            BeginBattle();
            MonsterAttack();
        }
        else
        {
            _facingMonster.SetClickable(true);
        }
    }
    
    // ------------------------------------------------------------------------
    // 이벤트
    // ------------------------------------------------------------------------
    [Header("이벤트")] 
    // 대미지를 표기하고 싶을 때 호출하는 채널.
    [SerializeField] private FloatVector3ChannelSO _channel;
    
    /*
     * 몬스터의 스파인 이벤트에 리스너를 등록하는 메서드.
     * 적과 전투를 시작할 때 실행된다. 
     */
    private void RegisterMonsterSpineEvent(Monster monster)
    {
        monster.SkeletonAnimation.AnimationState.Event += OnSpineEventRaised;
    }

    /*
     * 몬스터의 스파인 이벤트에서 리스너를 제거하는 메서드.
     * 적과 전투를 끝낼 때 실행된다.
     */
    private void RemoveMonsterSpineEvent(Monster monster)
    {
        monster.SkeletonAnimation.AnimationState.Event -= OnSpineEventRaised;
    }

    /*
     * 등록된 SpineAnimation에서 이벤트가 일어나면 실행되는 메서드.
     * e.Data로부터 어떤 이벤트인지를 파악하고 그에 맞게 작동한다.
     */
    public void OnSpineEventRaised(TrackEntry entry, Spine.Event e)
    {
        // 플레이어의 공격 이벤트
        if (e.Data.Name == "PlayerHit")
        {
            PlayerHit();
        }
        // 몬스터의 공격 이벤트
        else if (e.Data.Name == "MonsterHit")
        {
            MonsterHit();
        }
        // 플레이어 공격 애니메이션의 시작. 다른 방이나 몬스터와의 상호작용을 비활성화한다.
        else if (e.Data.Name == "PlayerStartAttack")
        {
            Player.Instance.SetMovable(false);
            _facingMonster.SetClickable(false);
        }
        // 플레이어 공격 애니메이션의 끝. 다른 방이나 몬스터와의 상호작용을 활성화한다.
        else if (e.Data.Name == "PlayerEndAttack")
        {
            print("Player End Attack!");
            Player.Instance.SetAnimationLoop(true);
            Player.Instance.SetAnimationState("Idle");
            // 몬스터가 이미 사망했다면 몬스터 사망 애니메이션이 끝날 때 활성화됨.
            if(_facingMonster.Alive)
            {
                print("Enemy still alive");
                Player.Instance.SetMovable(true);
                _facingMonster.SetClickable(true);
            
                // 플레이어 공격이 끝난 후 적의 공격
                //TODO : 확률적으로 추가 공격 차례
                MonsterAttack();
            }
        }
        // 몬스터 공격 애니메이션의 끝.
        else if (e.Data.Name == "MonsterEndAttack")
        {
            print("Monster End Attack!");
            Player.Instance.SetMovable(true);
            _facingMonster.SetClickable(true);
            
            Player.Instance.SetAnimationLoop(true);
            Player.Instance.SetAnimationState("Idle");
            
            _facingMonster.EndAttack();
        }
        // 몬스터의 사망 애니메이션의 끝.
        else if (e.Data.Name == "MonsterDie")
        {
            print("Monster Die!");
            Player.Instance.SetMovable(true);
            _facingMonster.SetClickable(true);
            _facingMonster.Die();
            EndBattle();
        }
    }
    //TODO : 플레이어와 적 사망 시...
    
    
    // ------------------------------------------------------------------------
    // UI
    // ------------------------------------------------------------------------
    [Header("UI"), SerializeField]
    private GameObject _equippedWeaponsRoot;
    [SerializeField]private HitSelectionPanel _hitSelectionPanel;

    private void DisplayEquippedWeapons()
    {
        _equippedWeaponsRoot.SetActive(true);
    }

    private void HideEquippedWeapons()
    {
        _equippedWeaponsRoot.SetActive(false);
    }
    
    public void OpenHitSelectionPanel(Monster monster)
    {
        _hitSelectionPanel.gameObject.SetActive(true);
        _hitSelectionPanel.SetMonster(monster);
    }

    public void CloseHitSelectionPanel()
    {
        _hitSelectionPanel.gameObject.SetActive(false);
    }

}