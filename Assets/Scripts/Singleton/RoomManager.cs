
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        
        // Room과 Floor들의 오브젝트 생성
        CreateBuilding();
        
        // 랜덤하게 방 구조 생성
        FindSideRoomDataSOs();
        InitBuilding();
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
    
    
    // ------------------------------------------------------------------------
    // 방의 구조 정보
    // ------------------------------------------------------------------------
    
    [Header("방의 구조 정보")]
    /*
     * Left 혹은 Right 방의 모든 구조.
     */
    [SerializeField,DisableInInspector]
    private RoomDataSO[] _sideRoomDataSOs;

    private void FindSideRoomDataSOs()
    {
        string path = "Data/SideRoomDataSO";
        _sideRoomDataSOs =  Resources.LoadAll<RoomDataSO>(path);
    }
    
    // ------------------------------------------------------------------------
    // 랜덤 맵 생성
    // ------------------------------------------------------------------------

    [Header("랜덤 맵 생성")]

    
    /*
     * Game 씬에서 바로 실행 시 쓰이는 에디터용 변수들
     */
    [SerializeField]
    private int _customMapSeed;
    [SerializeField] private GameDifficulty _customDifficulty;
    
    
    /*
     * 모든 방들의 구조/ 오브젝트/ 적 등을 임의로 생성하는 메서드.
     * Main 씬으로부터 불러와진다면 시드값을 받아 그대로 생성하지만,
     * Game 씬부터 Play하면 랜덤한 시드값으로 생성한다.
     */
    public void InitBuilding()
    {
        int seed;
        GameDifficulty difficulty;
        
        // DataManager 인스턴스가 있다면 받아오고, 없다면 설정한 값을 씀
        if (DataManager.Instance != null)
        {
            seed = DataManager.Instance.MapSeed;
            difficulty = DataManager.Instance.Difficulty;
        }
        else
        {
            if (_customMapSeed == -1) seed = Random.Range(Int32.MinValue, Int32.MaxValue);
            else seed = _customMapSeed;
            difficulty = _customDifficulty;
        }
        
        Random.InitState(seed);
        
        InitSideRooms();
        InitCenterRooms();
        InitTopRoom();
        InitBottomRoom();
    }

    private void InitSideRooms()
    {
        for (int i = 1; i < GameConstantsSO.Instance.MaxFloor - 1; i++)
        {
            bool left = _roomsDictionary.TryGetValue(new RoomPosition(i, RoomDirection.LEFT), out Room leftRoom);
            bool right = _roomsDictionary.TryGetValue(new RoomPosition(i, RoomDirection.RIGHT), out Room rightRoom);

            if (left && right)
            {
                int roomDataNumber = _sideRoomDataSOs.Length;
                int leftIndex = Random.Range(0, roomDataNumber);
                int rightIndex = Random.Range(0, roomDataNumber);
                
                leftRoom.InitRoom();
                leftRoom.SetRoomData(_sideRoomDataSOs[leftIndex]);
                rightRoom.InitRoom();
                rightRoom.SetRoomData(_sideRoomDataSOs[rightIndex]);
                
            }
        }
    }

    private void InitCenterRooms()
    {
        for (int i = 1; i < GameConstantsSO.Instance.MaxFloor - 1; i++)
        {
            bool center = _roomsDictionary.TryGetValue(new RoomPosition(i, RoomDirection.CENTER), out Room centerRoom);
            if (center)
            {
                centerRoom.InitRoom();
            }
        }
    }

    private void InitTopRoom()
    {
        _roomsDictionary[new RoomPosition(GameConstantsSO.Instance.MaxFloor-1,RoomDirection.CENTER)].InitRoom();
    }

    private void InitBottomRoom()
    {
        _roomsDictionary[new RoomPosition(0,RoomDirection.CENTER)].InitRoom();
    }
}