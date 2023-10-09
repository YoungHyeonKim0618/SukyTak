
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    
    
    // ------------------------------------------------------------------------
    // 활성화 / 비활성화 
    // ------------------------------------------------------------------------
    
    // 활성화되어있는지 여부 변수. 
    private bool _activated;
    public bool Activated => _activated;
    
    // 클릭 가능할 때 활성화되는 눈 모양 이미지. MyDataSo.IsInteractable == true 일 때에만 null이 아님
    private Image _activatedImage;

    public void Activate()
    {
        if (MyDataSo.IsInteractable && !_rooted)
        {
            _activated = true;
            _activatedImage.gameObject.SetActive(true);
        }
    }

    public void Deactivate()
    {
        if (MyDataSo.IsInteractable)
        {
            _activated = false;
            _activatedImage.gameObject.SetActive(false);
        }
    }

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
            
            if(dataSO.IsInteractable)
            {
                // 상호작용 가능하다면 눈 이미지 생성
                var go = new GameObject();
                Image image = go.AddComponent<Image>();
                image.transform.SetParent(transform);
                image.transform.localPosition = Vector3.zero;
                image.raycastTarget = false;
                image.sprite = GameConstantsSO.Instance.InteractaleActivatedSprite;
                image.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);

                _activatedImage = image;
            }
        }
    }

    public void AddRootable(ItemDataSO itemData)
    {
        _rootables.Add(itemData);
    }

    public void Interact()
    {
        if(_rootables.Count == 0)
            Player.Instance.SetDialogue("비었어...");
        else if (!_rooted)
        {
            StartCoroutine(Root());
        }
        
        // 만약 있다면, 루팅 후 스프라이트로 변경.
        if (MyDataSo.SpriteAfterInteraction != null)
            _image.sprite = MyDataSo.SpriteAfterInteraction;
        
        _activatedImage.gameObject.SetActive(false);
            
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