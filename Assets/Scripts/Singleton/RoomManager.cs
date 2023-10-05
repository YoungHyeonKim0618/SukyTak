
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
        
        InitRoomPowers();
        
        // 랜덤 적 생성
        SpawnMonsters();
        
        // UI 이미지 닫기
        CloseElevatorPanel();
        CloseFuseboxPanel();
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
    private Dictionary<int,bool> _areFuseboxesWorking;
    // 고장난 두꺼비집을 수리하는데 필요한 아이템 정보
    private Dictionary<int, ItemDataSO> _itemsNeedToFixFusebox;
    
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
        _areElevatorsWorking = new bool[GameConstantsSO.Instance.MaxFloor - 1];
        _areFuseboxesWorking = new Dictionary<int, bool>();
        _itemsNeedToFixFusebox = new Dictionary<int, ItemDataSO>();
        
        for (int i = 1; i < GameConstantsSO.Instance.MaxFloor - 1; i++)
        {
            bool center = _roomsDictionary.TryGetValue(new RoomPosition(i, RoomDirection.CENTER), out Room centerRoom);
            if (center)
            {
                centerRoom.InitRoom();

                // 엘리베이터 작동 설정
                bool elevatorWorking = Random.Range(0, 100) < GameConstantsSO.Instance.ElevatorWorkingChance;
                _areElevatorsWorking[i] = elevatorWorking;
                centerRoom.SetCenterRoom(i+1,elevatorWorking);
                
                // 5층 단위마다 두꺼비집 작동 설정
                if ((i + 1) % 5 == 0)
                {
                    bool fuseBoxWorking = Random.Range(0, 100) < GameConstantsSO.Instance.FuseboxWorkingChance;
                    _areFuseboxesWorking.Add(i,fuseBoxWorking);
                    // 고장난 두꺼비집을 고치기 위한 아이템 설정
                    if(!fuseBoxWorking)
                        _itemsNeedToFixFusebox.Add(i, GameConstantsSO.Instance.GetRandomItemToFixFusebox());
                    
                    centerRoom.SetFusebox(fuseBoxWorking);
                }
                else centerRoom.DisableFusebox();
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

    /*
     * 기본적으로 활성화되어있는 Blackout Image들을 조건에 맞는 방들만 비활성화하는 메서드.
     *
     * - 꼭대기층부터 96층까지는 무조건 비활성화한다.
     * - 이후는 5층 단위로 두꺼비집이 멀쩡하다면 모두 비활성화한다.
     */
    private void InitRoomPowers()
    {
        for (int i = 99; i > 0; i -= 5)
        {
            if (i == 99 || _areFuseboxesWorking[i])
            {
                for (int j = i == 99 ? 1 : 0; j < 5; j++)
                {
                    _roomsDictionary[new RoomPosition(i - j,RoomDirection.LEFT)].TurnLightOn();
                    _roomsDictionary[new RoomPosition(i - j,RoomDirection.CENTER)].TurnLightOn();
                    _roomsDictionary[new RoomPosition(i - j,RoomDirection.RIGHT)].TurnLightOn();
                }
            }
            else
            {
                break;
            }
        }
    }

    private void TurnLightsOn(int floor)
    {
        for (int j = 0; j < 5; j++)
        {
            _roomsDictionary[new RoomPosition(floor - j,RoomDirection.LEFT)].TurnLightOn();
            _roomsDictionary[new RoomPosition(floor - j,RoomDirection.CENTER)].TurnLightOn();
            _roomsDictionary[new RoomPosition(floor - j,RoomDirection.RIGHT)].TurnLightOn();
        }
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
    [SerializeField] private List<Button> _elevatorPanelButtons;
    [SerializeField] private List<TextMeshProUGUI> _elevatorPanelButtonTmps;

    [SerializeField] private Color _yellowColor, _greenColor, _redColor;
    
    /*
     * 고장나지 않은 엘리베이터를 클릭했을 때 UI를 표시하는 메서드.
     * 현재 층 이상의 층에 전력이 들어오지 않았다면 사용할 수 없다.
     */
    public void OpenElevatorPanel(int floor)
    {
        if (IsElevatorAvailable(floor))
        {
            _elevatorPanelRoot.SetActive(true);
            SetPrimaryElevatorPanel();
        }
        else
        {
            // 플레이어 대사 출력
            Player.Instance.SetDialogue("여기나 더 위의 전력이 나갔어...");
        }
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
            
            /*
             * 10층 단위로 엘리베이터가 갈 수 있는지 여부는 다음과 같다.
             * 1. Player가 10*(n+1)층 이하로 내려가 봤고
             * 2. 10*n층을 포함한 위의 모든 두꺼비집이 고장나지 않아야 함
             */
            bool available = Player.Instance.LowestFloorVisited < 10 * (i+1) && IsElevatorAvailable(10 * (i+1) - 1);
            
            if(available)
            {
                // 람다식이 i를 캡쳐해서 원하지 않은 값을 파라미터로 이용하지 않게 함
                int index = i;
                _elevatorPanelButtons[i].image.color = _greenColor;
                _elevatorPanelButtonTmps[i].color = Color.black;
                _elevatorPanelButtons[i].onClick.AddListener(() => SetSecondaryElevatorPanel(index));
            }
            else
            {
                _elevatorPanelButtons[i].image.color = _redColor;
                _elevatorPanelButtonTmps[i].color = Color.white;
            }
            
        }
        _elevatorPanelButtons[10].image.color = _yellowColor;
        _elevatorPanelButtonTmps[10].text = "VIP";
        _elevatorPanelButtonTmps[10].color = Color.black;
        
        _elevatorPanelButtons[10].onClick.RemoveAllListeners();
        _elevatorPanelButtons[10].onClick.AddListener(() => MoveRoomByElevator(GameConstantsSO.Instance.MaxFloor-1));
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
            _elevatorPanelButtons[i].onClick.RemoveAllListeners();

            int curFloor = 10 * floorTens + i - 1;

            if (_areElevatorsWorking[curFloor] && IsElevatorAvailable(curFloor))
            {
                _elevatorPanelButtons[i].image.color = _greenColor;
                _elevatorPanelButtonTmps[i].color = Color.black;

                // 다른 층으로 이동
                if(curFloor != Player.Instance.CurrentRoom.GetRoomPosition().floor)
                    _elevatorPanelButtons[i].onClick.AddListener(() => MoveRoomByElevator(curFloor));
            }
            else
            {
                _elevatorPanelButtons[i].image.color = _redColor;
                _elevatorPanelButtonTmps[i].color = Color.white;
            }
        }
        _elevatorPanelButtons[10].image.color = Color.clear;
        _elevatorPanelButtonTmps[10].text = "돌아가기";
        _elevatorPanelButtonTmps[10].color = _greenColor;
        
        _elevatorPanelButtons[10].onClick.RemoveAllListeners();
        _elevatorPanelButtons[10].onClick.AddListener(SetPrimaryElevatorPanel);
    }

    private void MoveRoomByElevator(int floor)
    {
        CloseElevatorPanel();
        Player.Instance.MoveRoomByElevator(floor);
    }

    /*
     * 해당 층 이상에 고장난 두꺼비집이 하나라도 있다면 false
     */
    private bool IsElevatorAvailable(int floor)
    {
        if (floor >= GameConstantsSO.Instance.MaxFloor - 5) return true;
        
        foreach (var pair in _areFuseboxesWorking)
        {
            if (pair.Key >= floor && pair.Value == false) return false;
        }
        return true;
    }
        
        
    // ------------------------------------------------------------------------
    // 두꺼비집
    // ------------------------------------------------------------------------

    [Header("두꺼비집")] [SerializeField]
    private GameObject _fuseBoxPanelRoot;

    [SerializeField] private Image _itemToFixFuseboxImage;
    [SerializeField] private TextMeshProUGUI _itemToFixFuseboxTmp;
    [SerializeField] private TextMeshProUGUI _tryFixTmp;
    [SerializeField] private Button _fuseboxYesButton, _fuseboxNoButton;

    public void OpenFuseboxPanel(int floor)
    {
        // 현재 층보다 높은 위치의 두꺼비집이 고쳐지지 않았다면 고칠 수 없음
        if (!IsElevatorAvailable(floor + 1)) return;
        
        
        _fuseBoxPanelRoot.SetActive(true);

        if (_itemsNeedToFixFusebox.TryGetValue(floor, out ItemDataSO data))
        {
            _itemToFixFuseboxImage.sprite = data.Sprite;
            _itemToFixFuseboxTmp.text = data.Name;
            
            // 필요한 아이템을 가지고 있을 때
            if (Player.Instance.Inventory.IsExists(data))
            {
                _tryFixTmp.text = "";
                _fuseboxYesButton.onClick.RemoveAllListeners();
                _fuseboxNoButton.onClick.RemoveAllListeners();
                _fuseboxYesButton.onClick.AddListener(() => FixFusebox(floor));
                _fuseboxNoButton.onClick.AddListener(CloseFuseboxPanel);
            }
            else
            {
                _tryFixTmp.text = "혹은 아무거나 이용해서 수리를 시도해봐야 할까?";
                _fuseboxYesButton.onClick.RemoveAllListeners();
                _fuseboxNoButton.onClick.RemoveAllListeners();
                _fuseboxYesButton.onClick.AddListener(() => TryFixFusebox(floor));
                _fuseboxNoButton.onClick.AddListener(CloseFuseboxPanel);
            }
        }
        else
        {
            Debug.LogError("Cannot find item to fix the fusebox! floor : " + floor);
        }
    }


    public void CloseFuseboxPanel()
    {
        _fuseBoxPanelRoot.SetActive(false);
    }

    /*
     * 필요한 아이템을 가지고 있을 때 확정적으로 고치는 메서드
     */
    private void FixFusebox(int floor)
    {
        print(floor);
        CloseFuseboxPanel();
        
        // 필요 아이템 소실
        Player.Instance.Inventory.DropItem(_itemsNeedToFixFusebox[floor]);
        
        // 고치기
        SucceedFixingFusebox(floor);
    }

    /*
     * 필요한 아이템이 없어 포만도와 랜덤 아이템을 소모해 시도하는 메서드
     */
    private void TryFixFusebox(int floor)
    {
        CloseFuseboxPanel();
        // 배고픔 5~ 14 소모
        int cost = Random.Range(5, 14);
        Player.Instance.ModifySatiety(-cost);

        // 랜덤 재료 아이템 (없다면 무기 아이템) 소실

        bool success = Random.Range(0, 100) < GameConstantsSO.Instance.FuseboxFixChance;
        if(success) SucceedFixingFusebox(floor);
        
        // 플레이어 대사
        Player.Instance.SetDialogue(success ? "해냈어! 나의 노력이 결실을 맺었어." : "시간만 날렸군!");
    }

    private void SucceedFixingFusebox(int floor)
    {
        _areFuseboxesWorking[floor] = true;
        TurnLightsOn(floor);
        _roomsDictionary[new RoomPosition(floor,RoomDirection.CENTER)].SetFusebox(true);
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