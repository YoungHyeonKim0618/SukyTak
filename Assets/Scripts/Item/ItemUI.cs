
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private Item _item;
    [SerializeField] private Image _image;
    [SerializeField] 
    private TextMeshProUGUI _stackTmp;

    [SerializeField] private Image _stackTmpBackground;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

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

    public void OnClick()
    {
        if(_draggingItemUi != this)
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
    
    //TODO : Item의 GetString()을 호출해 설명을 Display한다.
    
    // ------------------------------------------------------------------------
    // 드래그 & 드롭으로 위치 변환
    // ------------------------------------------------------------------------

    private static ItemUI _draggingItemUi;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _draggingItemUi = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mouseWorldPos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _draggingItemUi = null;
    }
    
    //TODO : 다른 ItemUI와 위치 변환
}