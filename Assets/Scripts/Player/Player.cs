using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;
    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Init();
    }

    // ------------------------------------------------------------------------
    // 초기화
    // ------------------------------------------------------------------------
    private void Init()
    {
        _status = new PlayerStatus(_expPerLevelData);
        //TODO : 이전 씬에서 선택한 패시브/액티브 특성 효과 적용
        
        // 이벤트 채널 등록
        _onTryMoveChannel.OnEventRaised += TryMove;
        
        // 위치 초기화
        InitPlayerPosition();
        
        //TODO : 디버그용 메서드 삭제
        _onMoveChannel.OnEventRaised += PrintOnEnterRoom;
    }

    private void InitPlayerPosition()
    {
        _currentRoom = RoomManager.Instance
            .GetRoomFromPosition(new RoomPosition(GameConstantsSO.Instance.MaxFloor-1, RoomDirection.CENTER));
        MoveRoomInstantly(CurrentRoomPosition);
    }

    
    // ------------------------------------------------------------------------
    // 저장 데이터로부터 로드
    // ------------------------------------------------------------------------
        
    // ------------------------------------------------------------------------
    // 현재 상태 (스탯) 정보
    // ------------------------------------------------------------------------

    [Header("스탯")] private PlayerStatus _status;
    [SerializeField] private ExpPerLevelDataSO _expPerLevelData;

    public void ModifyHp(float value)
    {
        _status.CurHp += value;
    }

    public void ModifySatiety(float value)
    {
        _status.CurSatiety += value;
    }

    public void EarnExp(float value)
    {
        _status.CurExp += value;
    }
    
    // ------------------------------------------------------------------------
    // 인벤토리 정보
    // ------------------------------------------------------------------------
    
    [Header("인벤토리")][SerializeField] private PlayerInventory _inventory;

    public void ObtainItem(ItemDataSO data, Vector2 pos = new Vector2())
    {
        _inventory.ObtainItem(data);
        DisplayObtainItem(data, pos);
    }

    public void DropItem(Item item)
    {
        _inventory.DropItem(item);
        DisplayDropItem(item);
    }

    public void DisplayItem(Item item, Vector2 pos)
    {
        _inventory.DisplayItem(item,pos);
    }

    public void CloseDisplay()
    {
        _inventory.CloseDisplay();
    }

    public void CheckItemUiBelow(ItemUI caller, PointerEventData eventData)
    {
        _inventory.CheckItemUiBelow(caller,eventData);
    }

    public Vector2 GetWeaponDamage()
    {
        return new Vector2(_inventory.CurWeaponData.MinDmg,_inventory.CurWeaponData.MaxDmg);
    }
    
    
    // ------------------------------------------------------------------------
    // 위치 정보
    // ------------------------------------------------------------------------
    [Header("현재 위치 정보")] [SerializeField, DisableInInspector]
    private Room _currentRoom;
    private RoomPosition CurrentRoomPosition => _currentRoom.GetRoomPosition();

    
    // ------------------------------------------------------------------------
    // FSM
    // ------------------------------------------------------------------------
    private enum PlayerState
    {
        MOVE,
        BATTLE
    }
    private PlayerState _currentState;
    
    
    
    
    // ------------------------------------------------------------------------
    // 이동
    // ------------------------------------------------------------------------
    [Header("이동")]
    
    [Tooltip("방 클릭 시 Invoke되는 이벤트 채널"), SerializeField]
    private RoomEventChannelSO _onTryMoveChannel;
    [Tooltip("이동 시 Invoke되는 이벤트 채널"), SerializeField]
    private RoomEventChannelSO _onMoveChannel;

    [Tooltip("이동 속도")]
    [SerializeField] private float _moveSpeed;

    // 이동 전체 코루틴
    private Coroutine _moveRoomLoopCoroutine;
    // 다음 RoomSpot까지 이동하는 코루틴
    private Coroutine _moveCoroutine;
    
    // 진행 방향을 Vector2Int 형태로 저장하는 큐.
    private Queue<RoomSpot> _routesQueue = new Queue<RoomSpot>();

    // 현재 향하고 있는 RoomSpot (이동 중이 아니라면 null)
    [SerializeField, DisableInInspector]
    private RoomSpot _headingRoomSpot;

    /*
     * 현재 방이 아닌 다른 방 클릭 시 발생되는 이벤트로부터 Invoke되는 메서드.
     * 전투 중이거나 잠긴 방, 중간에 막힌 방이 있는지 등을 판단하고
     * 가능한 만큼 실제로 이동한다.
     */
    private void TryMove(Room destination)
    {
        // 목적지와 현재 위치가 다를 때에만 작동
        //TODO : 이동 중이라면 위치가 같더라도 되돌아올 수 있게 하기.
        if (destination.GetRoomPosition() != CurrentRoomPosition)
        {
            // 목적지까지의 경로를 큐에 집어넣음.
            FindRoutes(destination);
            
            // 만약 이미 경로 이동 중이라면 멈추고 새로운 코루틴을 시작한다.
            // 다만 현재 목표로 하는 RoomSpot에 도착한 후 새로운 경로로 이동하기 시작한다.
            if (_moveRoomLoopCoroutine != null)
            {
                StopCoroutine(_moveRoomLoopCoroutine);
                _moveRoomLoopCoroutine = null;
            }
            
            // 새로운 이동 루프 코루틴을 실행함.
            _moveRoomLoopCoroutine = StartCoroutine(MoveRoomLoop());
        }
        else
        {
            // 멈춰있다면 아무일도 안 일어남, 이동 중이라면 현재 방에 멈춤
            
        }
    }
    
    /*
     * 현재 위치와 목적지 위치로부터 _routesQueue를 만드는 메서드
     */
    private void FindRoutes(Room destination)
    {
        _routesQueue.Clear();

        // 현재 계단을 오르내리는 중인지 파악
        float roomHeight = GameConstantsSO.Instance.RoomHeight;
        float y = transform.position.y;
        float h = CurrentRoomPosition.floor * roomHeight;
        float yOffset = y - h;

        bool underStair = yOffset > float.Epsilon && yOffset < 0.5f * roomHeight;
        bool upperStair = yOffset >= 0.5f * roomHeight && yOffset < roomHeight;
        

        // 층이 다르다면 중앙으로 이동한 후 계단을 걸음.
        if (destination.GetRoomPosition().floor != CurrentRoomPosition.floor)
        {
            int floorDiff = destination.GetRoomPosition().floor - CurrentRoomPosition.floor;
            int floorOffset = floorDiff > 0 ? 1 : -1;
            
            // 만약 이동 중이고 곧바로 다음 층으로 향하는 경우 첫 번째 방 이동을 무시해야 함.
            bool ignoreFirstRoom = false;
            
            // 계단 이동 중이 아닐 때
            if(_headingRoomSpot == null || (!underStair && !upperStair))
            {
                // 중앙으로 이동
                _routesQueue.Enqueue(RoomManager.Instance.GetRoomFromPosition(new RoomPosition(CurrentRoomPosition.floor,RoomDirection.CENTER)).spots[1]);
            }
            // 이동 중일 때 (Center Room에 있는 것이 보장됨)
            else
            {
                // Heading RoomSpot [1]
                if (_headingRoomSpot == _currentRoom.spots[1])
                {
                    // 위층으로 올라가려면 현재 방의 RoomSpot [2] -> 다음 방의 RoomSpot [1]
                    if (floorOffset == 1)
                    {
                        _routesQueue.Enqueue(_currentRoom.spots[2]);
                        _routesQueue.Enqueue(RoomManager.Instance.GetRoomFromPosition(new RoomPosition(CurrentRoomPosition.floor + 1,RoomDirection.CENTER)).spots[1]);
                        //다음 방으로 가므로 ignoreFirstRoom을 참으로 만듦.
                        ignoreFirstRoom = true;
                    }
                    // 아니라면 기준 위치로 가고 있으므로 아무것도 하지 않는다.
                }
                // Heading RoomSpot [2]
                else if (_headingRoomSpot == _currentRoom.spots[2])
                {
                    if (floorOffset == 1)
                    {
                        _routesQueue.Enqueue(RoomManager.Instance.GetRoomFromPosition(new RoomPosition(CurrentRoomPosition.floor + 1,RoomDirection.CENTER)).spots[1]);
                        ignoreFirstRoom = true;
                    }
                    else
                    {
                        _routesQueue.Enqueue(_currentRoom.spots[1]);
                    }
                }
                // Heading next RoomSpot [1]
                else
                {
                    // 어차피 위층으로 올라갈 것이므로
                    if (floorOffset == 1)
                        ignoreFirstRoom = true;
                    else
                    {
                        _routesQueue.Enqueue(_currentRoom.spots[2]);
                        _routesQueue.Enqueue(_currentRoom.spots[1]);
                    }
                }
            }

            for (int i = 0; i < Mathf.Abs(floorDiff); i++)
            {
                if(i == 0 && ignoreFirstRoom) continue;
                
                Room now = RoomManager.Instance.GetRoomFromPosition(new RoomPosition(CurrentRoomPosition.floor + i * floorOffset,RoomDirection.CENTER));
                Room next = RoomManager.Instance.GetRoomFromPosition(
                    new RoomPosition(CurrentRoomPosition.floor + (i + 1) * floorOffset, RoomDirection.CENTER));
                
                // 위층을 향해 가고 있을 때
                if (floorDiff > 0)
                {
                    _routesQueue.Enqueue(now.spots[2]);
                    _routesQueue.Enqueue(next.spots[1]);
                }
                // 아래층을 향해 가고 있을 때
                else
                {
                    _routesQueue.Enqueue(next.spots[2]);
                    _routesQueue.Enqueue(next.spots[1]);
                }
            }
        }
        // 층이 같은데 계단에 있다면 
        else if (CurrentRoomPosition.direction == RoomDirection.CENTER && (underStair || upperStair))
        {
            // 위층의 RoomSpot [1]로 향할 때
            if (!_currentRoom.spots.Contains(_headingRoomSpot))
            {
                _routesQueue.Enqueue(_currentRoom.spots[2]);
                _routesQueue.Enqueue(_currentRoom.spots[1]);
            }
            // 현재 층의 RoomSpot [2]로 향할 때
            else if (_headingRoomSpot.IsStair)
                _routesQueue.Enqueue(_currentRoom.spots[1]);
        }
        
        // 같은 층에 도달한 뒤 각각의 기준 위치로 바로 이동
        _routesQueue.Enqueue(destination.spots[0]);
        
    }


    private IEnumerator MoveRoomLoop()
    {
        // 현재 RoomSpot을 향해 움직이고 있다면, 도착할 때까지 기다림.
        while (_headingRoomSpot != null)
        {
            yield return null;
        }
        // 위치를 방의 기준점으로 초기화
        //SyncPlayerPosWithRoom();
        
        RoomSpot direction;
        while (_routesQueue.TryDequeue(out direction))
        {
            _moveCoroutine = StartCoroutine(MoveToward(direction));
            yield return _moveCoroutine;
            _moveCoroutine = null;
        }

        _moveRoomLoopCoroutine = null;
    }

    /*
     * 시작 시 혹은 특정 이벤트로 바로 순간이동해 위치를 바꾸는 메서드.
     */
    private void MoveRoomInstantly(RoomPosition destination)
    {
        var room = _currentRoom;
        if(room != null)
            room.Exit();
        
        _currentRoom = RoomManager.Instance.GetRoomFromPosition(destination);
        SyncPlayerPosWithRoom();
        
        _currentRoom.Enter();
    }

    /*
     * 목표(바로 다음의) RoomSpot까지 이동하는 메서드.
     * 파라미터의 spot은 TryMove()에서 정해주기 때문에 여기서 잠김이나 적 존재 여부를 파악할 필요 없음.
     */
    private IEnumerator MoveToward(RoomSpot spot)
    {
        _headingRoomSpot = spot;
        
        //TODO : 애니메이션과 x축 반전 설정
        
        // Divide by 0을 막기 위해 최소값을 지정함
        float spd = Mathf.Max(_moveSpeed, 0.01f);
        
        Vector2 startingPos = transform.position;
        Vector2 destination = (Vector2)spot.transform.position;
        Vector2 toGo = destination - startingPos;
        float movingTime = toGo.magnitude / spd;

        
        //TODO : 게임 배속을 spd에 적용시키기

        yield return transform.DOMove(destination, movingTime).SetEase(Ease.Linear).WaitForCompletion();
        _headingRoomSpot = null;
    }

    /*
     * RoomEnterDetector와 충돌했을 때 호출되는 메서드.
     */
    public void Detected(RoomEnterDetector detector)
    {
        // 다른 방의 RoomSpot에 간다면 방을 바꿈.
        if (detector.Host != _currentRoom)
        {
            ChangeRoom(detector.Host);
        }
    }
    /*
     * 플레이어가 위치한 방을 바꾸는 메서드.
     * 방이 가진 Enter Detector에 접촉 시 호출된다.
     */
    private void ChangeRoom(Room room)
    {
        _currentRoom.Exit();
        room.Enter();

        _currentRoom = room;
            
        _onMoveChannel.OnRaise(room);
    }

    /*
     * 잠긴 문, 몬스터 등 때문에 진행이 불가능해졌을 때 멈추는 메서드.
     * 코루틴들을 멈추고 초기화한다.
     */
    private void StopMovingRoom()
    {
    }


    private void SyncPlayerPosWithRoom()
    {
        transform.position = _currentRoom.spots[0].transform.position;
    }
    
    // ------------------------------------------------------------------------
    // 초기화
    // ------------------------------------------------------------------------
    [Header("UI")]
    [SerializeField] private Canvas _playerCanvas;
    // 아이템 획득/상실 시에 UI 생성을 위한 이미지 프리팹.
    [SerializeField] private Image p_ItemImage;
    [SerializeField] private float _itemImageOffsetY;

    private void DisplayObtainItem(ItemDataSO itemData, Vector2 pos)
    {
        // pos가 (0,0)이라면 기본 값이라고 간주, 플레이어 포지션으로 생각함.
        Vector2 startingPos = pos == Vector2.zero ? transform.position + new Vector3(0,1,0) : pos;
        
        var newImage = Instantiate(p_ItemImage,startingPos,Quaternion.identity,_playerCanvas.transform);
        newImage.sprite = itemData.Sprite;
        newImage.rectTransform.DOMoveY(startingPos.y + 1, 0.5f);
        Destroy(newImage.gameObject, 0.75f);
    }

    private void DisplayDropItem(Item item)
    {
        Vector2 startingPos =transform.position + new Vector3(0,1,0);
        
        var newImage = Instantiate(p_ItemImage,startingPos,Quaternion.identity,_playerCanvas.transform);
        newImage.sprite = item.Data.Sprite;
        newImage.rectTransform.DOMoveY(startingPos.y + 1, 0.5f);
        Destroy(newImage.gameObject, 0.75f);
    }
    
    
    
    // ------------------------------------------------------------------------
    // 디버그
    // ------------------------------------------------------------------------

    [Header("디버그")] [SerializeField] private CircleCollider2D _collider2D;
    [SerializeField, DisableInInspector] private bool _isMovingCoroutine;
    
    // 디버그용 프린트 메서드
    private void PrintOnEnterRoom(Room room)
    {
        //print($"Entered room : floor {room.GetRoomPosition().floor}, direction {room.GetRoomPosition().direction}");
    }

    private void Update()
    {
        _isMovingCoroutine = _moveCoroutine != null;
    }

    private void OnDrawGizmos()
    {
        if (_collider2D != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + (Vector3)_collider2D.offset,_collider2D.radius);
        }
    }
}
