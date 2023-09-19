
using System;
using System.Collections.Generic;
using UnityEngine;

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
                    newItemUi.transform.SetParent(medicalGrid);
                    break;
                default:
                    newItemUi.transform.SetParent(materialGrid);
                    break;
            }
        }
        print($"Current Items : {_items.Count}");
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
    // UI
    // ------------------------------------------------------------------------
    [Header("UI")] [SerializeField] private RectTransform _inventoryRoot;
    // Item 버튼 오브젝틑의 프리팹
    [SerializeField] private ItemUI p_itemUi;
    // ItemUI의 부모 그리드 
    [SerializeField] private Transform _weaponGrid, _accessoryGrid, medicalGrid, materialGrid;


    private void Start()
    {
        CloseInventoryUi();
    }

    public void OpenInventoryUi()
    {
        _inventoryRoot.gameObject.SetActive(true);
    }

    public void CloseInventoryUi()
    {
        _inventoryRoot.gameObject.SetActive(false);
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