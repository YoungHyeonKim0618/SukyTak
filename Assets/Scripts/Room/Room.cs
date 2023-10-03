
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/*
 * 실제 게임 내에서 클릭할 수 있는 방 클래스.
 * 월드 스페이스 캔버스를 컴포넌트 혹은 차일드로 가진다.
 */
public class Room : MonoBehaviour
{
    [Header("기본 멤버")] [SerializeField]
    private RectTransform _interactableRootRectTransform;


    public virtual void InitRoom()
    {
        SetRoomPosition();
        SetRoomSpots();
        
        // 아직 방문하지 않은 방을 어둡게 함.
        SetDarkImageAlpha(100);
        
        // MonsterSpot 초기화
        InitMonsterSpot();
    }
    
    
    // ------------------------------------------------------------------------
    // 방 위치 정보
    // ------------------------------------------------------------------------
    
    [SerializeField,DisableInInspector]
    private int _floorNumber;
    [SerializeField,DisableInInspector]
    private RoomDirection _roomDirection;

    /// 방의 위치를 반환. 게임에서 n층일 경우에 floor는 n-1임에 주의할 것!
    public RoomPosition GetRoomPosition()
    {
        return new RoomPosition(_floorNumber,_roomDirection);
    }
    
    private void SetRoomPosition()
    {
        Vector2 pos = transform.position;
        float roomHeight = GameConstantsSO.Instance.RoomHeight;

        _floorNumber = Mathf.RoundToInt(pos.y / roomHeight);

        switch (Mathf.RoundToInt(pos.x))
        {
            case < 0:
                _roomDirection = RoomDirection.LEFT;
                break;
            case 0:
                _roomDirection = RoomDirection.CENTER;
                break;
            case > 0:
                _roomDirection = RoomDirection.RIGHT;
                break;
        }
    }


    // ------------------------------------------------------------------------
    // 방 이동 & RoomSpot
    // ------------------------------------------------------------------------

    [Header("방 이동")] [SerializeField] private RoomSpot _leftRoomSpot;
    [SerializeField] private RoomSpot _rightRoomSpot, _centerRoomSpot_0, _centerRoomSpot_1, _centerRoomSpot_2;

    /*
     * 방의 위치 기준점들.
     * Side Room은 LEFT, RIGHT에 따라 각각 하나씩만을 가지고
     * Center Room은 기준점/ 계단 시작/ 계단 중간 순서로 3개를 가짐.
     */
    public List<RoomSpot> spots;

    private void SetRoomSpots()
    {
        switch (_roomDirection)
        {
            case RoomDirection.LEFT:
                spots.Add(_leftRoomSpot);
                break;
            case RoomDirection.CENTER:
                spots.Add(_centerRoomSpot_0);
                spots.Add(_centerRoomSpot_1);
                spots.Add(_centerRoomSpot_2);
                break;
            case RoomDirection.RIGHT:
                spots.Add(_rightRoomSpot);
                break;
        }

        foreach (var spot in spots)
        {
            spot.Host = this;
        }
    }
    
    
    // ------------------------------------------------------------------------
    // 클릭 시 이동 이벤트
    // ------------------------------------------------------------------------
    
    [Header("클릭 이벤트")]
    [SerializeField] private RoomEventChannelSO _channel;
    public void OnClickRoom()
    {
        _channel.OnEventRaised.Invoke(this);
    }
    
    
    // ------------------------------------------------------------------------
    // 상호작용
    // ------------------------------------------------------------------------
    
    [SerializeField,DisableInInspector]
    // 한 번이라도 방문한 적 있는지 여부. 게임 로딩 시 필요함
    private bool _hasVisited;
    
    
    /*
     * 방 전체를 뒤덮는 버튼 오브젝트.
     * 플레이어가 다른 방에서 클릭 시 해당 방으로 이동하려고 시도한다.
     * 만약 플레이어가 해당 방 안에 있다면, 비활성화된다.
     */
    [SerializeField] private GameObject _entireButton;



    public void Enter()
    {
        _hasVisited = true;
        SetEntireButtonEnabled(false);
        SetDarkImageAlpha(0);
    }

    public void Exit()
    {
        SetEntireButtonEnabled(true);
        SetDarkImageAlpha(0.3f);
    }
    /*
     * 
     * 뒤덮는 버튼의 액티브 상태를 지정하는 메서드.
     * 플레이어가 해당 방에 들어갈 때 false, 나갈 때 true로 지정해준다.
     */
    private void SetEntireButtonEnabled(bool enable)
    {
        _entireButton.gameObject.SetActive(enable);
    }

    /*
     * 상호작용 가능한 Interactable들을 활성화하는 메서드.
     * 몬스터가 없는 방에 들어가거나, 몬스터를 죽일 때 호출된다.
     */
    public void ActivateInteractables()
    {
        foreach (var interactable in _interactables)
        {
            interactable.Activate();
        }
    }

    public void DeactivateInteractables()
    {
        foreach (var interactable in _interactables)
        {
            interactable.Deactivate();
        }
    }
    
    
    
    // ------------------------------------------------------------------------
    // Initiation (Side Rooms)
    // ------------------------------------------------------------------------
    [Header("초기화")]

    private List<Interactable> _interactables = new List<Interactable>();
    
    private RoomDataSO _dataSO;
    public void SetRoomData(RoomDataSO dataSO)
    {
        _dataSO = dataSO;
        _background.sprite = dataSO.Background;

        foreach (var vo in dataSO.VisualObjects)
        {
            GameObject newGameObject = new GameObject();
            Interactable interactable = newGameObject.AddComponent<Interactable>();
            interactable.transform.localScale = Vector3.one;
            interactable.transform.SetParent(_interactableRootRectTransform);
            interactable.InitInteractable(vo.interactableSo);
            
            // 어떤 아이템을 가지고 있는지를 정해줌.
            // 미획득/ 획득 중 획득 시에는 추가로 20% 확률로 2개 아이템을 가짐.
            bool rootable = Random.Range(0, 100) <
                            GameConstantsSO.Instance.GetRootChanceFromDifficulty(RoomManager.Instance.GetDifficulty());
            if (rootable)
            {
                int rootingNum = Random.Range(0, 100) < 20 ? 2 : 1;
                for (int i = 0; i < rootingNum; i++)
                {
                    ItemDataSO obtaining = null;
                    // 만약 정해진 아이템 풀이 있다면 그 중 획득, 없다면 모든 풀 중에서 랜덤 획득
                    if (vo.interactableSo.PossibleItems.Count > 0)
                    {
                        int index = Random.Range(0, vo.interactableSo.PossibleItems.Count);
                        obtaining = vo.interactableSo.PossibleItems[index];
                    }
                    else
                    {
                        obtaining = GameConstantsSO.Instance.GetRandomRootableItem();
                    }
                    interactable.AddRootable(obtaining);
                }
            }

            interactable.transform.localPosition = new Vector3(vo.position.x, -vo.position.y);
            interactable.transform.localScale = vo.scale;
            _interactables.Add(interactable);
        }
    }
    
    
    
    // ------------------------------------------------------------------------
    // Initiation (Center Rooms)
    // ------------------------------------------------------------------------

    [Header("가운데 방 초기화 (Side Room은 참조할 필요 없음)")]
    [SerializeField] private Button _elevator;
    [SerializeField] private Sprite _elevatorWorkingSprite, _elevatorBrokenSprite;

    [SerializeField] private Button _fusebox;
    [SerializeField] private Sprite _fuseboxWorkingSprite, _fuseboxBrokenSprite;
    [SerializeField] private TextMeshProUGUI _floorTmp;

    public void SetCenterRoom(int floor, bool elevator)
    {
        SetFloorText(floor);
        SetElevator(elevator);
    }
    private void SetFloorText(int floor)
    {
        _floorTmp.text = $"{floor}";
    }

    /*
     * 엘레베이터가 사용 가능한지 여부 설정
     */
    private void SetElevator(bool working)
    {
        _elevator.interactable = working;
        _elevator.image.sprite = working ? _elevatorWorkingSprite : _elevatorBrokenSprite;
        // 엘리베이터 클릭 시 이벤트에 패널을 여는 메서드를 등록
        if(working)
            _elevator.onClick.AddListener(() => RoomManager.Instance.OpenElevatorPanel(_floorNumber));
    }

    /*
     * 두꺼비집이 멀쩡한지 여부 설정 (5층 단위 층에서만 호출됨)
     */
    public void SetFusebox(bool working)
    {
        _fusebox.interactable = !working;
        _fusebox.image.sprite = working ? _fuseboxWorkingSprite : _fuseboxBrokenSprite;
        
        if(!working)
            _fusebox.onClick.AddListener(() => RoomManager.Instance.OpenFuseboxPanel(GetRoomPosition().floor));
    }

    public void DisableFusebox()
    {
        _fusebox.gameObject.SetActive(false);
    }
    
    // ------------------------------------------------------------------------
    // 비주얼
    // ------------------------------------------------------------------------
    [Header("비주얼")]
    /*
     * Side Room의 경우엔 RoomDataSO의 sprite를 적용시키지만,
     * Center Room은 프리팹 자체에 이미 sprite를 가지며 변경하지 않음.
     */
    [SerializeField] private Image _background;
    
    /*
     * 방의 밝기를 조절하는 이미지.
     * 아직 들어가보지 않았을 때, 들어가봤지만 현재 다른 방일 때, 현재 방일 때 각각 다른 알파 값을 가진다.
     */
    [SerializeField] private Image _darkImage;
    
    // 전원이 꺼진 방을 어둡게 보이게 하는 이미지.
    [SerializeField] private Image _blackOutImage;
    
    private void SetDarkImageAlpha(float value)
    {
        _darkImage.color = new Color(0, 0, 0, value);
    }

    public void TurnLightOn()
    {
        _blackOutImage.gameObject.SetActive(false);
    }
    
    // ------------------------------------------------------------------------
    // 몬스터 정보
    // ------------------------------------------------------------------------
    [Header("몬스터 정보")] [SerializeField]
    private Vector2 _monsterPos;
    private List<Monster> _aliveMonsters = new List<Monster>();

    public Vector2 MonsterPos => _monsterPos;
    public bool MonsterExists => _aliveMonsters.Count > 0;
    public Monster Monster => MonsterExists ? _aliveMonsters[0] : null;
    
    
    private void InitMonsterSpot()
    {
        if (GetRoomPosition().direction == RoomDirection.LEFT)
        {
            _monsterPos = transform.position + new Vector3(-1.5f, 0, 0);
        }
        else if(GetRoomPosition().direction == RoomDirection.RIGHT)
            _monsterPos = transform.position + new Vector3(1.5f, 0, 0);
        else
            _monsterPos = transform.position + new Vector3(1.2f, 0, 0);
    }
    
    public void PlaceMonster(Monster monster)
    {
        _aliveMonsters.Add(monster);
        monster.SetFlipX(GetRoomPosition().direction != RoomDirection.LEFT);

    }
    
    /*
     * 적을 죽여서 방 정보로부터 없애는 메서드.
     */
    public void RemoveMonster(Monster monster)
    {
        if (_aliveMonsters.Contains(monster))
        {
            _aliveMonsters.Remove(monster);
        }
    }
    
    // ------------------------------------------------------------------------
    // 기타 정보
    // ------------------------------------------------------------------------
    [DisableInInspector]
    public bool Locked;
    
    
}

public enum RoomDirection
{
    LEFT = -1,
    CENTER = 0,
    RIGHT = 1
}

[Serializable]
public struct RoomPosition
{
    public int floor;
    public RoomDirection direction;

    public RoomPosition(int roomFloor, RoomDirection roomDirection)
    {
        floor = roomFloor;
        direction = roomDirection;
    }

    /*
     * 비교 연산자 오버라이딩
     */
    public static bool operator ==(RoomPosition a, RoomPosition b)
    {
        return a.floor == b.floor && a.direction == b.direction;
    }

    public static bool operator !=(RoomPosition a, RoomPosition b)
    {
        return a.floor != b.floor || a.direction != b.direction;
    }

    public override string ToString()
    {
        return $"({floor}, {direction})";
    }
}