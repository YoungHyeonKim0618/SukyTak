
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/GameConstants")]
public class GameConstantsSO : ScriptableObject
{
    /*
     * MonoBehaviour 대신 ScriptableObject를 이용해 씬간 종속성을 없앴다.
     */
    private static GameConstantsSO _instance;
    public static GameConstantsSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConstantsSO>("Data/GameConstantsSO_Instance");
                if(_instance == null)
                    Debug.LogError("Failed to load GameConstantsSO!");
            }

            return _instance;
        }
    }

    [Header("게임 설정")]
    public int MaxFloor;
    public float CenterRoomWidth;
    public float SideRoomWidth;
    public float FloorWidth => 2 * SideRoomWidth + CenterRoomWidth;
    public float RoomHeight;

    public int MaxSatiety;

    [Header("가운데 방 설정")]
    public float ElevatorWorkingChance;
    public float FuseboxWorkingChance;
    public float FuseboxFixChance;

    [Header("플레이어 기본 스탯")] 
    public float DefaultMaxHP;

    public float DefaultCritChance;
    public float DefaultAdditionalAttackChance;
    public float DefaultDodgeChance;
    public float DefaultDamageReduction;
    
    [Header("밸런스")]
    public float RottenFoodDmg;
    [Tooltip("몬스터가 선공을 가할 확률 (%)")]
    public float MonsterAttackFirstChance;

    [Header("난이도별 변수")]
    [SerializeField] private List<float> RootChancePerDifficulty;

    public float GetRootChanceFromDifficulty(GameDifficulty difficulty) => RootChancePerDifficulty[(int)difficulty];


    [SerializeField] private List<float> MonsterChancePerDifficulty;

    public float GetMonsterChanceFromDifficulty(GameDifficulty difficulty) => MonsterChancePerDifficulty[(int)difficulty];

    [Header("글로벌 에셋")]
    public Sprite InteractaleActivatedSprite;
    
    [Header("아이템 참조")]
    [SerializeField] private List<ItemDataSO> _rootableItems;
    public ItemDataSO GetRandomRootableItem()
    {
        int index = Random.Range(0, _rootableItems.Count);
        return _rootableItems[index];
    }

    [SerializeField] private List<ItemDataSO> _itemsToFixFusebox;

    public ItemDataSO GetRandomItemToFixFusebox()
    {
        int index = Random.Range(0, _itemsToFixFusebox.Count);
        return _itemsToFixFusebox[index];
    }
}