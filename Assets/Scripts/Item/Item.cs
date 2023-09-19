
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Item
{
    // 스크립터블 오브젝트 데이터
    private ItemDataSO _data;
    public ItemDataSO Data => _data;
    // 보유 개수
    private int _stack;
    public int Stack
    {
        get { return _stack; }
        set { _stack = value; }
    }
    
    
    // 장비 아이템의 보유 효과
    private List<IItemEffect> _itemEffects;

    public Item(ItemDataSO data)
    {
        _data = data;

        switch (data.Type)
        {
            case ItemType.WEAPON:
            case ItemType.ACCESSORY:
            case ItemType.MATERIAL:
                break;
            case ItemType.FOOD:
                break;
            case ItemType.MEDICAL:
                break;
        }
        _stack = 1;
    }
    public void Use()
    {
        bool used = false;
        if (_data.Type == ItemType.FOOD && _data is FoodData foodData)
        {
            // 배고픔 회복
            Player.Instance.ModifySatiety(foodData.Satiety);
            
            // 상한 음식은 50% 확률로 피해 입음
            if(foodData.IsRotten && Random.Range(0,100) < 50)
                Player.Instance.ModifyHp(GameConstantsSO.Instance.RottenFoodDmg);
            used = true;
        }
        if (_data.Type == ItemType.MEDICAL && _data is MedicalData medicalData)
        {
            // 체력 회복
            Player.Instance.ModifyHp(medicalData.Recovery);
            used = true;
        }
        
        // 사용했다면 제거
        if(used) Player.Instance.DropItem(this);
    }

}

[Flags]
public enum ItemType
{
    WEAPON = 1 << 0,
    ACCESSORY = 1 << 1,
    FOOD = 1 << 2,
    MEDICAL = 1 << 3,
    MATERIAL = 1 << 4
}