
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour,IPointerClickHandler, IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler, IPointerExitHandler
{
    private Item _item;
    public Item Item => _item;
    [SerializeField] private Image _image;
    [SerializeField] 
    private TextMeshProUGUI _stackTmp;

    [SerializeField] private Image _stackTmpBackground;



    public void SetItem(Item item)
    {
        _item = item;
    }

    public void Refresh()
    {
        _image.sprite = _item.Data.Sprite;
        if (_item.Data.Stackable)
        {
            _stackTmp.text = $"{_item.Stack}";
            _stackTmpBackground.gameObject.SetActive(true);
        }
        else
        {
            _stackTmp.text = "";
            _stackTmpBackground.gameObject.SetActive(false);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right && _draggingItemUi == null)
        {
            if (_item != null)
            {
                _item.Use();
            }
            else
            {
                Debug.LogError("Item which you tried to use is null!");
            }
        }
    }

    
    // ------------------------------------------------------------------------
    // 아이템 정보
    // ------------------------------------------------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        Player.Instance.DisplayItem(_item.Data, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Player.Instance.CloseDisplay();
        //TODO : 사용으로 없어졌을 때 새로고침하기
    }
    
    // ------------------------------------------------------------------------
    // 드래그 & 드롭으로 위치 변환
    // ------------------------------------------------------------------------

    private static ItemUI _draggingItemUi;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _draggingItemUi = this;
        //무기일 경우 Weapon UI 업데이트
        Player.Instance.Inventory.SetWeaponUiStandby();
        
        _image.maskable = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mouseWorldPos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 다른 ItemUI와 위치 바꾸기
        Player.Instance.CheckItemUiBelow(this,eventData);
        _draggingItemUi = null;
        _image.maskable = true;
    }
    

}