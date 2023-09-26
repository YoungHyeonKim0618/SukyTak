
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

/*
 * 클릭해서 상호작용할 수 있는 오브젝트.
 * 여기선 사실상 클릭해서 아이템을 획득할 수 있는 것들을 지칭한다.
 */
public class Interactable : MonoBehaviour
{
    public InteractableDataSO MyDataSo;

    private Image _image;

    // Interactable이 아이템을 실제 가지고 있는지 여부는 Building 초기화 과정에서 정해진다.
    private List<ItemDataSO> _rootables = new List<ItemDataSO>();
    [SerializeField,DisableInInspector]
    private bool _rooted;

    public void InitInteractable(InteractableDataSO dataSO)
    {
        if(dataSO != null)
        {
            MyDataSo = dataSO;
            
            _image = gameObject.AddComponent<Image>();
            _image.sprite = MyDataSo.Sprite;
            
            // 상호작용 가능하다면 버튼을 실제 생성한다.
            if (dataSO.IsInteractable)
            {
                Button button = gameObject.AddComponent<Button>();
                
                // 버튼의 OnClick에 Interact를 리스너로 추가함.
                button.onClick.AddListener(Interact);
            }
            else _image.raycastTarget = false;
            

            // 자신의 크기를 DataSo의 Sprite Texture 크기에 따라 맞춤.
            float ppu = MyDataSo.Sprite.pixelsPerUnit;
            Vector2 buttonSize = new Vector2(MyDataSo.Sprite.texture.width, MyDataSo.Sprite.texture.height) / ppu;
            GetComponent<RectTransform>().sizeDelta = buttonSize;
            
        }
    }

    public void AddRootable(ItemDataSO itemData)
    {
        _rootables.Add(itemData);
    }

    public void Interact()
    {
        if (_rootables.Count > 0 && !_rooted)
        {
            StartCoroutine(Root());
        }
        
        // 만약 있다면, 루팅 후 스프라이트로 변경.
        if (MyDataSo.SpriteAfterInteraction != null)
            _image.sprite = MyDataSo.SpriteAfterInteraction;
        
        _rooted = true;
    }

    private IEnumerator Root()
    {
        foreach (var itemData in _rootables)
        {
            Player.Instance.ObtainItem(itemData,transform.position);
            yield return new WaitForSeconds(0.3f);
        }
    }
}