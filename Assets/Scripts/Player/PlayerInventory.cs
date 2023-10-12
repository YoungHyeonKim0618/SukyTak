
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private PlayerBattleManager _battleManager;
    
    // 모든 아이템 리스트
    private List<Item> _items = new List<Item>();

    /*
     * 아이템 타입별로 따로 저장하는 딕셔너리.
     * Obtain/ Remove시에 같이 저장하고 삭제한다.
     * 여러 타입을 가지는 아이템은 모두 같이 저장/삭제함.
     */
    private Dictionary<ItemType, List<Item>> _itemDictionary = new Dictionary<ItemType, List<Item>>();
    
    // 아이템으로 UI를 찾을 수 있는 딕셔너리
    private Dictionary<Item, ItemUI> _itemUiDictionary = new Dictionary<Item, ItemUI>();

    private void InitItemDictionary()
    {
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            _itemDictionary.Add(type,new List<Item>());
        }
    }
    
    private void Start()
    {
        CloseInventoryUi();
        InitWeapons();
        InitItemDictionary();
        SortItemRecipes();
    }
    
    
    public void ObtainItem(ItemDataSO itemData)
    {
        Item existing = _items.Find(x => x.Data == itemData);
        
        // stackable 아이템은 이미 같은 Item이 있는 지 확인하고 있다면 stack을 쌓음
        if (itemData.Stackable && existing != null)
        {
            existing.Stack++;
            _itemUiDictionary[existing].Refresh();
        }
        else
        {
            // 스크립터블오브젝트로부터 Item 오브젝트 생성
            Item item = new Item(itemData);

            // 리스트에 추가
            _items.Add(item);
            
            // 딕셔너리에도 추가
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                if(itemData.Type.HasFlag(type))
                    _itemDictionary[type].Add(item);
            }
            

            // 새로운 ItemUI를 만들고 알맞은 Grid에 추가
            var newItemUi = Instantiate(p_itemUi,_inventoryRoot);
            // Item을 세팅
            newItemUi.SetItem(item);
            // Sprite와 수량 새로고침
            newItemUi.Refresh();
            _itemUiDictionary.Add(item,newItemUi);
            switch (itemData.Type)
            {
                case ItemType.WEAPON:
                    newItemUi.transform.SetParent(_weaponGrid);
                    break;
                case ItemType.ACCESSORY:
                    newItemUi.transform.SetParent(_accessoryGrid);
                    break;
                case ItemType.MEDICAL:
                case ItemType.FOOD:
                    newItemUi.transform.SetParent(_medicalGrid);
                    break;
                default:
                    newItemUi.transform.SetParent(_materialGrid);
                    break;
            }
        }
    }

    /*
     * 아이템을 인벤토리에서 제거하는 메서드.
     * [무기/장신구] 장착되어있는 아이템이 제거될 때에는 : 
     * - 제거되는 아이템의 ItemData가 장착된 ItemData와 같은지 확인한다.
     * - 제거되는 아이템의 남은 수량을 확인한다.
     * - 0이라면 장착을 해제한다.
     */
    public void DropItem(Item item)
    {
        if (!_items.Contains(item))
        {
            Debug.LogError("Cannot drop item which is not obtained!");
            return;
        }
        
        // stackable 아이템은 stack이 1 초과인지 확인 후 stack만 줄임.
        if (item.Data.Stackable && item.Stack > 1)
        {
            item.Stack--;
            _itemUiDictionary[item].Refresh();
        }
        else
        {
            // List에서 Item 삭제
            _items.Remove(item);
            // Dictionary에서도 삭제
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                if(item.Data.Type.HasFlag(type))
                    _itemDictionary[type].Remove(item);
            }
            
            // ItemUI 삭제
            ItemUI itemUi = _itemUiDictionary[item];
            _itemUiDictionary.Remove(item);
            Destroy(itemUi.gameObject);
        }
        
        //TODO : 장착되어있는 무기나 장신구였다면 해제해줌
        TryUnequipWeapon(item.Data);
    }

    public void DropItem(ItemDataSO itemData)
    {
        foreach (var item in _items)
        {
            if (item.Data == itemData)
            {
                DropItem(item);
                break;
            }
        }
    }
    
    public bool IsExists(ItemDataSO data)
    {
        return _items.Any(x => x.Data == data);
    }

    /*
     * 임의의 아이템을 버리는 메서드. other == true라면 기타 아이템 중 하나, 없다면 무기 아이템 중 하나를 버림.
     */
    public void DropRandomItem(bool other)
    {
        if (_items.Count == 0)
        {
            print("No item to drop!");
            return;
        }
            
            
        if (other)
        {
            if (_itemDictionary[ItemType.OTHER].Count > 0)
            {
                int index = Random.Range(0, _itemDictionary[ItemType.OTHER].Count);
                DropItem(_itemDictionary[ItemType.OTHER][index]);
            }
            else if (_itemDictionary[ItemType.WEAPON].Count > 0)
            {
                int index = Random.Range(0, _itemDictionary[ItemType.WEAPON].Count);
                DropItem(_itemDictionary[ItemType.WEAPON][index]);
            }
            // else?
        }
        else
        {
            int index = Random.Range(0, _items.Count);
            DropItem(_items[index]);
        }
    }

    
    /*
     * ItemDataSO를 data로 하는 아이템의 수량을 반환하는 메서드
     */
    public int GetItemNumber(ItemDataSO data)
    {
        if (_items.Any(x => x.Data == data))
        {
            if (data.Stackable)
            {
                return _items.Find(x => x.Data == data).Stack;
            }
            else
            {
                return _items.FindAll(x => x.Data == data).Count;
            }
        }
        else return 0;
    }

    
    // ------------------------------------------------------------------------
    // 무기
    // ------------------------------------------------------------------------
    [Header("무기")] 
    // 아무 무기도 없을 때의 맨주먹 데이터.
    [SerializeField] private WeaponData s_fistData;
    
    // 장착 중인 무기 아이템이 사라질 때, 장착도 해제되어야 함
    [SerializeField, DisableInInspector] private WeaponData _leftWeapon, _rightWeapon;
    public WeaponData LeftWeapon => _leftWeapon != null ? _leftWeapon : s_fistData;
    public WeaponData RightWeapon => _rightWeapon != null ? _rightWeapon : s_fistData;
    
    private bool _selectingLeftWeapon;

    public WeaponData CurWeaponData => _selectingLeftWeapon ? LeftWeapon : RightWeapon;

    [SerializeField] private Image _leftWeaponButtonImage, _rightWeaponButtonImage;
    [SerializeField] private Color _weaponButtonSelectedColor, _defaultWeaponButtonColor;

    private void InitWeapons()
    {
        SelectWeapon(true);
        RefreshWeaponUi();
    }

    public void SelectWeapon(bool left)
    {
        _selectingLeftWeapon = left;
        _leftWeaponButtonImage.color = left ? _weaponButtonSelectedColor : _defaultWeaponButtonColor;
        _rightWeaponButtonImage.color = !left ? _weaponButtonSelectedColor : _defaultWeaponButtonColor;
    }

    private void EquipWeapon(WeaponData data, bool left)
    {
        if (left) _leftWeapon = data;
        else _rightWeapon = data;
        RefreshWeaponUi();
    }

    private void UnEquipWeapon(WeaponData data)
    {
        if (_leftWeapon == data)
        {
            _leftWeapon = null;
        }
        if (_rightWeapon == data)
        {
            _rightWeapon = null;
        }
        RefreshWeaponUi();
    }

    /*
     * 무기를 인벤토리에서 제거할 때 호출되는 메서드.
     * 무기가 장착되었었고, 더 이상 남지 않았다면 장착 해제한다.
     */
    private void TryUnequipWeapon(ItemDataSO data)
    {
        if (data is WeaponData weaponData)
        {
            if (weaponData == _leftWeapon && GetItemNumber(data) == 0)
            {
                UnEquipWeapon(weaponData);
            }
            else if (weaponData == _rightWeapon && GetItemNumber(data) == 0)
            {
                UnEquipWeapon(weaponData);
            }
        }
        RefreshWeaponUi();
    }
    
    
    

    // ------------------------------------------------------------------------
    // 디스플레이
    // ------------------------------------------------------------------------
    [Header("디스플레이")] [SerializeField] private Image _displayImage;
    [SerializeField]
    private TextMeshProUGUI _nameTmp, _descriptionTmp;

    [SerializeField] private float _displayOffsetY;

    public void DisplayItem(ItemDataSO data, Vector2 pos)
    {
        _nameTmp.text = data.Name;
        _descriptionTmp.text = data.GetString();
        _displayImage.gameObject.SetActive(true);
        _displayImage.transform.position = pos + new Vector2(0, _displayOffsetY);
    }

    public void CloseDisplay()
    {
        _displayImage.gameObject.SetActive(false);
        _displayImage.gameObject.SetActive(false);
    }
    
    // ------------------------------------------------------------------------
    // UI
    // ------------------------------------------------------------------------
    [Header("UI")] [SerializeField] private RectTransform _inventoryRoot;

    // Item 버튼 오브젝틑의 프리팹
    [SerializeField] private ItemUI p_itemUi;
    // ItemUI의 부모 그리드 
    [SerializeField] private Transform _weaponGrid, _accessoryGrid, _medicalGrid, _materialGrid;
    [SerializeField] private Sprite _greenCrossSprite;

    [Space(10)] [SerializeField] private GraphicRaycaster _raycaster;
    [SerializeField] private Canvas _draggingItemUiCanvas;
    public Canvas DraggingItemUiCanvas => _draggingItemUiCanvas;



    public void OpenInventoryUi()
    {
        _inventoryRoot.gameObject.SetActive(true);
        SelectRecipeType((int)ItemType.WEAPON);
        
        // WeaponUI의 부모를 인벤토리 UI로 정하고, 그 중 가장 하위에 둠
        _weaponUiRoot.SetParent(_inventoryRoot);
        _weaponUiRoot.SetSiblingIndex(0);
        
        ForceUpdateGridLayouts();
        
        // 무기 선택 UI 활성화
        _battleManager.DisplayEquippedWeapons();
    }

    public void CloseInventoryUi()
    {
        ResetSelectedRecipe();
        _weaponUiRoot.SetParent(_weaponUiRootOutsideParent);
        _inventoryRoot.gameObject.SetActive(false);
        
        // 적이 없다면 무기 선택 UI 비활성화
        _battleManager.TryHideEquippedWeapons();
    }
    
    /*
     * ItemUI 드래그 & 드롭 시 아래의 ItemUI를 감지하고 위치를 뒤바꾸는 메서드.
     */
    public void CheckItemUiBelow(ItemUI caller, PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(eventData,results);

        foreach (var result in results)
        {
            ItemUI itemUi = result.gameObject.GetComponent<ItemUI>();
            if (itemUi != null && itemUi != caller)
            {
                /*
                 * 이들의 부모 오브젝트가 Grid Layout Group을 가지고 있기 때문에 자식 순서를 바꾸면 위치도 바뀜.
                 */
                int myIndex = itemUi.transform.GetSiblingIndex();
                int otherIndex = caller.transform.GetSiblingIndex();
            
                itemUi.transform.SetSiblingIndex(otherIndex);
                caller.transform.SetSiblingIndex(myIndex);
            }
            // 들고 있는 게 무기이고, 아래에 무기 장착 UI가 있으면 장착함
            else if (caller.Item.Data.Type.HasFlag(ItemType.WEAPON) && result.gameObject.CompareTag("WeaponUI"))
            {
                EquipWeapon(caller.Item.Data as WeaponData, caller.transform.position.x < 0);
            }
        }
        RefreshWeaponUi();
        ForceUpdateGridLayouts();
    }

    private void ForceUpdateGridLayouts()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_weaponGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_accessoryGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_medicalGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_materialGrid.transform);
    }


    // WeaponUIRoot는 인벤토리가 열리지 않았을 때엔 다른 캔버스에 존재하다가,
    // 인벤토리가 열릴 때 인벤토리의 자식이 된다.
    // 이럼으로써 Render 순서와 GraphicRaycast를 올바르게 할 수 있음.
    [SerializeField] private Transform _weaponUiRootOutsideParent;
    [SerializeField] private Transform _weaponUiRoot;
    [SerializeField] private Image _leftWeaponImage, _rightWeaponImage;
    [SerializeField] private TextMeshProUGUI _leftWeaponNameTmp, _rightWeaponNameTmp;
    [SerializeField] private TextMeshProUGUI _leftWeaponDescTmp, _rightWeaponDescTmp;
    
    /*
     * WeaponUI들을 동기화하는 메서드
     */
    private void RefreshWeaponUi()
    {
        _leftWeaponImage.sprite = LeftWeapon.Sprite;
        _rightWeaponImage.sprite = RightWeapon.Sprite;
        
        _leftWeaponNameTmp.text = LeftWeapon.Name;
        _rightWeaponNameTmp.text = RightWeapon.Name;

        _leftWeaponDescTmp.text = LeftWeapon.GetString();
        _rightWeaponDescTmp.text = RightWeapon.GetString();
    }

    /*
     * WeaponData를 가지는 ItemUI를 드래그 중일 때 WeaponUI를 대기 상태로 바꾸는 메서드.
     */
    public void SetWeaponUiStandby()
    {
        _leftWeaponImage.sprite = _greenCrossSprite;
        _rightWeaponImage.sprite = _greenCrossSprite;
        
        _leftWeaponNameTmp.text = "";
        _rightWeaponNameTmp.text = "";

        _leftWeaponDescTmp.text ="";
        _rightWeaponDescTmp.text = "";
    }
    
    // ------------------------------------------------------------------------
    // 조합, 조합법
    // ------------------------------------------------------------------------
    [Header("조합")] [SerializeField] private List<ItemRecipeSO> _weaponRecipes;
    [SerializeField] 
    private List<ItemRecipeSO> _accessoryRecipes, _foodRecipes, _medicalRecipes, _otherRecipes;
    
    //TODO : 일반 버튼이 아닌 관리 가능한 클래스로 변경
    [SerializeField] private List<RecipeButton> _recipeButtons;
    /*
     * ItemRecipe들을 필요한 가구의 레벨 오름차순으로 정렬하는 메서드
     */
    private void SortItemRecipes()
    {
        _weaponRecipes.Sort((a,b) => a.NecessaryFurnitureLevel.CompareTo(b.NecessaryFurnitureLevel));
        _accessoryRecipes.Sort((a, b) => a.NecessaryFurnitureLevel.CompareTo(b.NecessaryFurnitureLevel));
        _foodRecipes.Sort((a, b) => a.NecessaryFurnitureLevel.CompareTo(b.NecessaryFurnitureLevel));
        _medicalRecipes.Sort((a, b) => a.NecessaryFurnitureLevel.CompareTo(b.NecessaryFurnitureLevel));
        _otherRecipes.Sort((a, b) => a.NecessaryFurnitureLevel.CompareTo(b.NecessaryFurnitureLevel));
    }

    /*
     * 조합법 왼쪽의 버튼을 누를 때 호출되는 메서드.
     */
    public void SelectRecipeType(int type)
    {
        ResetSelectedRecipe();
        int recipeNum = 0;
        List<ItemRecipeSO> recipes = null;
        switch ((ItemType)type)
        {
            case ItemType.WEAPON:
                recipeNum = _weaponRecipes.Count;
                recipes = _weaponRecipes;
                break;
            case ItemType.ACCESSORY:
                recipeNum = _accessoryRecipes.Count;
                recipes = _accessoryRecipes;
                break;
            case ItemType.FOOD:
                recipeNum = _foodRecipes.Count;
                recipes = _foodRecipes;
                break;
            case ItemType.MEDICAL:
                recipeNum = _medicalRecipes.Count;
                recipes = _medicalRecipes;
                break;
            case ItemType.OTHER:
                recipeNum = _otherRecipes.Count;
                recipes = _otherRecipes;
                break;
        }

        for (int i = 0; i < _recipeButtons.Count; i++)
        {
            if (i < recipeNum)
            {
                _recipeButtons[i].gameObject.SetActive(true);
                _recipeButtons[i].GetButton.image.sprite = recipes[i].Output.Sprite;
                _recipeButtons[i].GetButton.onClick.RemoveAllListeners();
                _recipeButtons[i].SetItemData(recipes[i]);
                
                //TODO : 가구 레벨이 기준치 미만일 때 잠금 Sprite 설정 && onClick에 추가하지 않음
                
                int index = i;
                _recipeButtons[i].GetButton.onClick.AddListener(() => SelectRecipe(recipes[index]));
                
            }
            else
            {
                _recipeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /*
     * 재료 아이템들을 모두 소지하고 있을 때 조합하는 메서드.
     */
    public void Combine(ItemRecipeSO recipe)
    {
        foreach (var itemData in recipe.Inputs)
        {
            if(IsExists(itemData))
                DropItem(itemData);
            else Debug.LogError($"There is no input item : {itemData.name}");
        }
        ObtainItem(recipe.Output);
        ResetSelectedRecipe();
    }
    
    // ------------------------------------------------------------------------
    // 조합 UI
    // ------------------------------------------------------------------------
    [Header("조합 UI")][SerializeField] private GameObject _recipeGrid;

    // 최대 4개의 재료 Sprite 이미지와, 재료가 없을 때만 활성화되는 백그라운드 붉은 이미지
    [SerializeField] private List<Image> _materialDisplays, _materialDisplayBackgrounds;

    private ItemRecipeSO _selectingRecipe;

    [SerializeField] private Sprite _lockedRecipeSprite;
    
    /*
     * 조합법 버튼을 클릭했을 때 호출되는 메서드.
     */
    private void SelectRecipe(ItemRecipeSO recipe)
    {
        if (recipe.Inputs.Count > 4)
        {
            throw new Exception("Inputs of recipes should not over 4!");
        }

        _selectingRecipe = recipe;
        
        
        Dictionary<ItemDataSO, int> numberDictionary = new Dictionary<ItemDataSO, int>();

        for (int i = 0; i < 4; i++)
        {
            if (i < recipe.Inputs.Count)
            {
                bool newData = numberDictionary.TryAdd(recipe.Inputs[i], 1);
                if (!newData) numberDictionary[recipe.Inputs[i]]++;
                
                _materialDisplays[i].gameObject.SetActive(true);
                _materialDisplays[i].sprite = recipe.Inputs[i].Sprite;
                
                //부족한 아이템은 붉은 배경으로 표시하기
                _materialDisplayBackgrounds[i].gameObject.SetActive(GetItemNumber(recipe.Inputs[i]) < numberDictionary[recipe.Inputs[i]]);
            }
            else
            {
                _materialDisplays[i].gameObject.SetActive(false);
                _materialDisplayBackgrounds[i].gameObject.SetActive(false);
            }
        }
    }
    
    /*
     * 인벤토리 창을 닫을 때 선택된 조합법을 초기화하는 메서드.
     */
    private void ResetSelectedRecipe()
    {
        _selectingRecipe = null;

        for (int i = 0; i < 4; i++)
        {
            _materialDisplays[i].gameObject.SetActive(false);
            _materialDisplayBackgrounds[i].gameObject.SetActive(false);
        }
    }

    /*
     * 현재 _selectingRecipe의 재료가 모두 있는지 여부를 반환하는 메서드.
     */
    private bool IsCombinable()
    {
        // 선택중인 조합법이 없다면 false
        if (_selectingRecipe == null) return false;
        Dictionary<ItemDataSO, int> numberDictionary = new Dictionary<ItemDataSO, int>();
        
        // <ItemDataSO,int> 딕셔너리를 만들어 재료의 종류와 수량을 정리함
        foreach (var data in _selectingRecipe.Inputs)
        {
            bool newData = numberDictionary.TryAdd(data, 1);
            if (!newData) numberDictionary[data]++;
        }
        
        // 딕셔너리를 이용해 비교
        foreach (var pair in numberDictionary)
        {
            if (GetItemNumber(pair.Key) < pair.Value) return false;
        }
        return true;
    }
    
    /*
     * '제작' 버튼을 클릭했을 때 호출되는 메서드
     */
    public void TryCombine()
    {
        if (_selectingRecipe != null && IsCombinable() )
        {
            // 손에서 조합 가능한 조합법이거나 VIP 룸에 있어야 조합 가능
            if(_selectingRecipe.HandCombinable || Player.Instance.CurrentRoom.GetRoomPosition().floor == GameConstantsSO.Instance.MaxFloor-1)
                Combine(_selectingRecipe);
        }
    }

}