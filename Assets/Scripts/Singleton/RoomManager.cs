
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    [SerializeField] private Transform _buildingRoot;

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
        SetMonsterCanvasSize();
        
        // 랜덤하게 방 구조 생성
        FindSideRoomDataSOs();
        InitBuilding();
        
        // 랜덤 적 생성
        SpawnMonsters();
        
        
        CloseElevatorPanel();
        
    }


    // ------------------------------------------------------------------------
    // 방들의 참조 정보
    // ------------------------------------------------------------------------
    [Header("방들의 참조")] 
    [SerializeField] 
    private Floor p_floor;
    [SerializeField] 
    private Floor p_bottomFloor, p_topFloor;
    
    private Dictionary<RoomPosition, Room> _roomsDictionary = new Dictionary<RoomPosition, Room>();

    
    // 1층부터 99층까지 엘리베이터의 작동 여부
    private bool[] _areElevatorsWorking;
    // 5층부터 95층까지 두꺼비집의 작동 여부
    private bool[] _areFuseBoxesWorking;
    
    private void CreateBuilding()
    {
        int maxFloor = GameConstantsSO.Instance.MaxFloor;
        float roomHeight = GameConstantsSO.Instance.RoomHeight;
        
        // 1층
        var bottomFloor = Instantiate(p_bottomFloor, Vector3.zero, Quaternion.identity,_buildingRoot);
        _roomsDictionary.Add(new RoomPosition(0,RoomDirection.CENTER),bottomFloor.Center);

        // 중간 층들
        for (int i = 1; i < maxFloor - 1; i++)
        {
            var floor = Instantiate(p_floor, new Vector3(0, roomHeight * i, 0), Quaternion.identity,_buildingRoot);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.LEFT),floor.Left);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.CENTER),floor.Center);
            _roomsDictionary.Add(new RoomPosition(i,RoomDirection.RIGHT),floor.Right);
        }

        // 마지막 층
        var topFloor = Instantiate(p_topFloor, new Vector3(0,roomHeight * (maxFloor-1), 0), Quaternion.identity,_buildingRoot);
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
            _customMapSeed = seed;
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

                bool elevatorWorking = Random.Range(0, 100) < GameConstantsSO.Instance.ElevatorWorkingChance;
                
                centerRoom.SetCenterRoom(i+1,elevatorWorking);
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
    
    // ------------------------------------------------------------------------
    // 랜덤 몬스터 생성
    // ------------------------------------------------------------------------
    [Header("Monster")]
    [SerializeField] private RectTransform _monsterCanvas;

    [SerializeField] private Monster p_zombie;

    /*
     * 몬스터의 부모가될 MonsterCanvas의 크기를 빌딩에 맞추는 메서드.
     */
    private void SetMonsterCanvasSize()
    {
        var data = GameConstantsSO.Instance;
        _monsterCanvas.sizeDelta = new Vector2(data.FloorWidth, data.RoomHeight * data.MaxFloor);
        _monsterCanvas.position = new Vector3(0, 0, 0);
    }

    /*
     * 랜덤으로 몬스터를 생성하고 배치하는 메서드.
     */
    private void SpawnMonsters()
    {
        //TODO : 디버그용, 랜덤 알고리즘
        
        //SpawnMonster(p_zombie,GetRoomFromPosition(new RoomPosition(96,RoomDirection.CENTER)));
        //SpawnMonster(p_zombie,GetRoomFromPosition(new RoomPosition(97,RoomDirection.RIGHT)));
        //SpawnMonster(p_zombie,GetRoomFromPosition(new RoomPosition(98,RoomDirection.LEFT)));
    }

    private void SpawnMonster(Monster monsterPrefab, Room room)
    {
        Monster monster = Instantiate(monsterPrefab,room.MonsterPos,Quaternion.identity, _monsterCanvas);
        monster.InitMonster();
        room.PlaceMonster(monster);
    }

    /*
     * 층별로 몬스터를 배치하기 위한 메서드
     */
    private Monster GetMonsterFromFloor(int floor)
    {
        return p_zombie;
    }
    
    
    // ------------------------------------------------------------------------
    // 엘리베이터
    // ------------------------------------------------------------------------
    [Header("엘리베이터")] [SerializeField]
    private GameObject _elevatorPanelRoot;

    // 가장 아래 위치한 요소의 인덱스 [0] 부터 시작.
    [SerializeField]private List<Button> _elevatorPanelButtons;
    [SerializeField]private List<TextMeshProUGUI> _elevatorPanelButtonTmps;
    
    /*
     * 고장나지 않은 엘리베이터를 클릭했을 때 UI를 표시하는 메서드.
     */
    public void OpenElevatorPanel()
    {
        _elevatorPanelRoot.SetActive(true);
        SetPrimaryElevatorPanel();
    }

    public void CloseElevatorPanel()
    {
        _elevatorPanelRoot.SetActive(false);
    }


    /*
     * 엘리베이터 패널의 버튼들을 관리하는 메서드.
     * 처음 엘리베이터 패널을 열면 이 메서드가 호출됨.
     */
    private void SetPrimaryElevatorPanel()
    {
        for (int i = 0; i < 10; i++)
        {
            _elevatorPanelButtonTmps[i].text = $"{Mathf.Clamp(10*i,1,99)} - {10*i + 9}";
            _elevatorPanelButtons[i].onClick.RemoveAllListeners();
            
            // 람다식이 i를 캡쳐해서 원하지 않은 값을 파라미터로 이용하지 않게 함
            int index = i;
            _elevatorPanelButtons[i].onClick.AddListener(() => SetSecondaryElevatorPanel(index));
            
        }
        _elevatorPanelButtonTmps[10].text = "VIP";
    }

    /*
     * 엘리베이터 패널에서 10층 단위의 버튼을 클릭했을 때 실행되는 메서드.
     * 최소 0 ~ 최대 9를 파라미터로 받는다.
     */
    private void SetSecondaryElevatorPanel(int floorTens)
    {
        for (int i = 0; i < 10; i++)
        {
            _elevatorPanelButtonTmps[i].text = $"{Mathf.Clamp(10*floorTens + i,1,99)}";
        }
    }
    
    // ------------------------------------------------------------------------
    // 게임 변수 (임시)
    // ------------------------------------------------------------------------

    // 일반적인 경우에 DataManager가 있다면 그 값을 반환, 없다면 임시 값을 반환함 (디버그용)
    public int GetSeed()
    {
        if (DataManager.Instance != null)
        {
            return DataManager.Instance.MapSeed;
        }
        return _customMapSeed;
    }

    public GameDifficulty GetDifficulty()
    {
        if (DataManager.Instance != null)
        {
            return DataManager.Instance.Difficulty;
        }
        return _customDifficulty;
    }
}