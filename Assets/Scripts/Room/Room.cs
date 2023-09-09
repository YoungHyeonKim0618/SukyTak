
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/*
 * 실제 게임 내에서 클릭할 수 있는 방 클래스.
 * 월드 스페이스 캔버스를 컴포넌트 혹은 차일드로 가진다.
 */
public class Room : MonoBehaviour
{
    

    private void Start()
    {
        InitRoom();
    }

    public virtual void InitRoom()
    {
        SetRoomPosition();
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
    
    /*
     * 방 전체를 뒤덮는 버튼 오브젝트.
     * 플레이어가 다른 방에서 클릭 시 해당 방으로 이동하려고 시도한다.
     * 만약 플레이어가 해당 방 안에 있다면, 비활성화된다.
     */
    [SerializeField] private GameObject entireButton;

    /*
     * 뒤덮는 버튼의 액티브 상태를 지정하는 메서드.
     * 플레이어가 해당 방에 들어갈 때 false, 나갈 때 true로 지정해준다.
     */
    public void SetEntireButtonEnabled(bool enable)
    {
        entireButton.gameObject.SetActive(enable);
    }
    
    // ------------------------------------------------------------------------
    // 몬스터 정보
    // ------------------------------------------------------------------------
    [Header("몬스터 정보")]
    private List<Monster> _aliveMonsters = new List<Monster>();

    public bool HasAliveMonsters => _aliveMonsters.Count > 0;
    
    
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
}