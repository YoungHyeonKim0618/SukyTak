using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
        Init();
    }

    // ------------------------------------------------------------------------
    // 초기화
    // ------------------------------------------------------------------------
    private void Init()
    {
        //TODO : 이전 씬에서 선택한 패시브/액티브 특성 효과 적용
        
        // 이벤트 채널 등록
        _onTryMoveChannel.OnEventRaised += TryMove;
        
        // 위치 초기화
        InitPlayerPosition();
    }

    private void InitPlayerPosition()
    {
        MoveRoomInstantly(new RoomPosition(GameConstantsSO.Instance.MaxFloor - 1, RoomDirection.CENTER));
    }
    
    
    // ------------------------------------------------------------------------
    // 저장 데이터로부터 로드
    // ------------------------------------------------------------------------
    
    // ------------------------------------------------------------------------
    // 현재 상태 (스탯) 정보
    // ------------------------------------------------------------------------
    
    
    // ------------------------------------------------------------------------
    // 위치 정보
    // ------------------------------------------------------------------------
    [Header("현재 위치 정보")] [SerializeField, DisableInInspector]
    private RoomPosition _currentRoomPosition;
    
    public RoomPosition CurrentRoomPosition()
    {
        return _currentRoomPosition;
    }

    private Room CurrentRoom => RoomManager.Instance.GetRoomFromPosition(_currentRoomPosition);

    
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
    private RoomPositionEventChannelSO _onMoveChannel;

    private Coroutine _moveRoomLoopCoroutine;
    private Coroutine _moveRoomCoroutine;

    private Queue<Vector2Int> _routesQueue = new Queue<Vector2Int>();

    /*
     * 현재 방이 아닌 다른 방 클릭 시 발생되는 이벤트로부터 Invoke되는 메서드.
     * 전투 중이거나 잠긴 방, 중간에 막힌 방이 있는지 등을 판단하고
     * 가능한 만큼 실제로 이동한다.
     */
    private void TryMove(Room destination)
    {
        // 이전의 요청에 따라 이동 중이었다면 초기화함.
        ResetMovingCoroutines();
        
        // 목적지와 현재 위치가 다를 때에만 작동
        if (destination.GetRoomPosition() != _currentRoomPosition)
        {
            // 목적지까지의 경로를 큐에 집어넣음.
            FindRoutes(destination.GetRoomPosition());
            
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
    private void FindRoutes(RoomPosition destination)
    {
        _routesQueue.Clear();
        
        // 이동할 수 있는 가운데 방으로 향하는 경로
        switch (_currentRoomPosition.direction)
        {
            case RoomDirection.LEFT:
                _routesQueue.Enqueue(new Vector2Int(1,0));
                break;
            case RoomDirection.CENTER:
                break;
            case RoomDirection.RIGHT:
                _routesQueue.Enqueue(new Vector2Int(-1,0));
                break;
        }

        // 가운데 방에서 높이 차이에 따른 경로
        for (int i = 0; i < Mathf.Abs(destination.floor - _currentRoomPosition.floor); i++)
        {
            if(destination.floor > _currentRoomPosition.floor)
                _routesQueue.Enqueue(new Vector2Int(0,1));
            else _routesQueue.Enqueue(new Vector2Int(0,-1));
        }
        
        // 목적지 층의 가운데 방에서 목적지 방까지의 경로
        switch (destination.direction)
        {
            case RoomDirection.LEFT:
                _routesQueue.Enqueue(new Vector2Int(-1,0));
                break;
            case RoomDirection.CENTER:
                break;
            case RoomDirection.RIGHT:
                _routesQueue.Enqueue(new Vector2Int(1,0));
                break;
        }
    }


    private IEnumerator MoveRoomLoop()
    {
        Vector2Int direction;
        while (_routesQueue.TryDequeue(out direction))
        {
            _moveRoomCoroutine = StartCoroutine(MoveRoom(direction));
            yield return _moveRoomCoroutine;
        }
    }

    private IEnumerator MoveRoom(Vector2Int direction)
    {
        RoomPosition nextRoomPos = new RoomPosition(_currentRoomPosition.floor + direction.y,
            _currentRoomPosition.direction + direction.x);

        // 만약 LEFT에서 왼쪽으로 가거나 RIGHT에서 오른쪽으로 가는 등 잘못된 연산 시 중지
        if (!Enum.IsDefined(typeof(RoomDirection), nextRoomPos.direction) || nextRoomPos.floor < 0
                || nextRoomPos.floor >= GameConstantsSO.Instance.MaxFloor)
        {
            Debug.LogError("Invalid room position value to move!");
            yield break;
        }
        
        Room nextRoom = RoomManager.Instance.GetRoomFromPosition(nextRoomPos);
        
        // 잠겨있을 때
        if (nextRoom.Locked)
        {
            //TODO : 열쇠 있는지 확인 후 있다면 소모 후 Unlock, 없다면 멈춤
        }
        
        // 현재 방에 적이 있을 때
        if (CurrentRoom.HasAliveMonsters)
        {
            //TODO : 적에게 공격 기회를 준 후 이동
        }

        yield return new WaitForSeconds(0.25f);
        ChangeRoom(nextRoomPos);
        yield return new WaitForSeconds(0.25f);
        
        // 이동한 방에 적이 있을 때
        if (CurrentRoom.HasAliveMonsters)
        {
            //TODO : 새로운 전투 시작
        }
        
    }

    /*
     * 시작 시 혹은 특정 이벤트로 바로 순간이동해 위치를 바꾸는 메서드.
     */
    private void MoveRoomInstantly(RoomPosition destination)
    {
        var room = CurrentRoom;
        if(room != null)
            room.Exit();
        
        _currentRoomPosition = destination;
        SyncPlayerPosWithRoom();
        
        CurrentRoom.Enter();
    }

    private void ChangeRoom(RoomPosition nextRoomPos)
    {
        CurrentRoom.Exit();
        // 현재 방을 이동함
        _currentRoomPosition = nextRoomPos;
        // 현재 플레이어 위치도 이동함
        SyncPlayerPosWithRoom();
        CurrentRoom.Enter();
    }

    /*
     * 잠긴 문, 몬스터 등 때문에 진행이 불가능해졌을 때 멈추는 메서드.
     * 코루틴들을 멈추고 초기화한다.
     */
    private void StopMovingRoom()
    {
        ResetMovingCoroutines();
    }

    private void ResetMovingCoroutines()
    {
        if(_moveRoomLoopCoroutine != null)
            StopCoroutine(_moveRoomLoopCoroutine);
        _moveRoomLoopCoroutine = null;
        
        //TODO : _moveRoomCoroutine도 Stop해야 하나?
    }

    private void SyncPlayerPosWithRoom()
    {
        transform.position = RoomManager.Instance.GetRoomFromPosition(_currentRoomPosition).transform.position;
    }
    
    
}
