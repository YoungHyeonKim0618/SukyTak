
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        
        CreateBuilding();
    }

    private void Start()
    {
    }

    // ------------------------------------------------------------------------
    // 방들의 참조 정보
    // ------------------------------------------------------------------------
    [Header("방들의 참조")] 
    [SerializeField] 
    private Floor floorPrefab;
    [SerializeField] 
    private Floor bottomFloorPrefab, topFloorPrefab;
    
    private Dictionary<RoomPosition, Room> _roomsDictionary = new Dictionary<RoomPosition, Room>();

    private void CreateBuilding()
    {
        int maxFloor = GameConstantsSO.Instance.MaxFloor;
        float roomHeight = GameConstantsSO.Instance.RoomHeight;
        
        // 1층
        var bottomFloor = Instantiate(bottomFloorPrefab, Vector3.zero, Quaternion.identity);
        _roomsDictionary.Add(new RoomPosition(0,RoomDirection.CENTER),bottomFloor.Center);

        // 중간 층들
        for (int i = 1; i < maxFloor - 1; i++)
        {
            var floor = Instantiate(floorPrefab, new Vector3(0, roomHeight * i, 0), Quaternion.identity);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.LEFT),floor.Left);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.CENTER),floor.Center);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.RIGHT),floor.Right);
        }

        // 마지막 층
        var topFloor = Instantiate(topFloorPrefab, new Vector3(0,roomHeight * (maxFloor-1), 0), Quaternion.identity);
        _roomsDictionary.Add(new RoomPosition(maxFloor-1,RoomDirection.CENTER),topFloor.Center);
    }

    public Room GetRoomFromPosition(RoomPosition pos)
    {
        if (_roomsDictionary.TryGetValue(pos, out Room room))
            return room;
        return null;
    }
}