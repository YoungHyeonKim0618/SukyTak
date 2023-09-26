
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    
    
    // 모든 아이템 리스트
    private List<Item> _items = new List<Item>();
    // 아이템으로 UI를 찾을 수 있는 딕셔너리
    private Dictionary<Item, ItemUI> _itemUiDictionary = new Dictionary<Item, ItemUI>();
    
    
    
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
            
            // ItemUI 삭제
            ItemUI itemUi = _itemUiDictionary[item];
            _itemUiDictionary.Remove(item);
            Destroy(itemUi.gameObject);
        }
        
    }
    
    
    // ------------------------------------------------------------------------
    // 무기
    // ------------------------------------------------------------------------
    [Header("무기")] 
    // 아무 무기도 없을 때의 맨주먹 데이터.
    [SerializeField] private WeaponData s_fistData;
    
    // 장착 중인 무기 아이템이 사라질 때, 장착도 해제되어야 함
    [SerializeField, DisableInInspector] private WeaponData _leftWeapon, _rightWeapon;

    private bool _selectingLeftWeapon;

    public WeaponData CurWeaponData => _selectingLeftWeapon ? _leftWeapon : _rightWeapon;

    private void InitWeapons()
    {
        _leftWeapon = s_fistData;
        _rightWeapon = s_fistData;
        RefreshWeaponUi();
    }

    private void EquipWeapon(WeaponData data, bool left)
    {
        if (left) _leftWeapon = data;
        else _rightWeapon = data;
    }

    private void UnEquipWeapon(WeaponData data)
    {
        if (_leftWeapon == data || _rightWeapon == data)
        {
            
        }
    }

    // ------------------------------------------------------------------------
    // 디스플레이
    // ------------------------------------------------------------------------
    [Header("디스플레이")] [SerializeField] private Image _displayImage;
    [SerializeField]
    private TextMeshProUGUI _nameTmp, _descriptionTmp;

    [SerializeField] private float _displayOffsetY;

    public void DisplayItem(Item item, Vector2 pos)
    {
        ItemDataSO data = item.Data;
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

    [Space(10)] [SerializeField] private GraphicRaycaster _raycaster;


    private void Start()
    {
        CloseInventoryUi();
        InitWeapons();
    }

    public void OpenInventoryUi()
    {
        _inventoryRoot.gameObject.SetActive(true);
        ForceUpdateGridLayouts();
    }

    public void CloseInventoryUi()
    {
        _inventoryRoot.gameObject.SetActive(false);
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
                return;
            }
        }
        ForceUpdateGridLayouts();
    }

    private void ForceUpdateGridLayouts()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_weaponGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_accessoryGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_medicalGrid.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_materialGrid.transform);
    }


    [SerializeField] private Image _leftWeaponImage, _rightWeaponImage;
    [SerializeField] private TextMeshProUGUI _leftWeaponNameTmp, _rightWeaponNameTmp;
    [SerializeField] private TextMeshProUGUI _leftWeaponDescTmp, _rightWeaponDescTmp;
    private void RefreshWeaponUi()
    {
        _leftWeaponImage.sprite = _leftWeapon.Sprite;
        _rightWeaponImage.sprite = _rightWeapon.Sprite;
        
        _leftWeaponNameTmp.text = _leftWeapon.Name;
        _rightWeaponNameTmp.text = _rightWeapon.Name;

        _leftWeaponDescTmp.text = _leftWeapon.GetString();
        _rightWeaponDescTmp.text = _rightWeapon.GetString();
    }
    
    // ------------------------------------------------------------------------
    // 조합, 조합법
    // ------------------------------------------------------------------------
    private void FindRecipes()
    {
        
    }
    private void Craft()
    {
        
    }
}