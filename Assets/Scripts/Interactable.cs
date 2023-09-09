
using System;
using UnityEngine;
using Button = UnityEngine.UI.Button;

/*
 * 클릭해서 상호작용할 수 있는 오브젝트.
 * 여기선 사실상 클릭해서 아이템을 획득할 수 있는 것들을 지칭한다.
 */
public class Interactable : MonoBehaviour
{
    public InteractableDataSO myDataSo;
    
    /*
     * 상호작용하기 위해 플레이어가 클릭할 수 있는 버튼.
     * 버튼의 크기는 초기화 시 myDataSo의 Sprite 크기에 맞춰진다.
     */
    [SerializeField] private Button _button;


    private void Start()
    {
        InitInteractable();
    }

    public void InitInteractable()
    {
        if(myDataSo != null)
        {
            _button.image.sprite = myDataSo.Sprite;

            // 버튼의 크기를 DataSo의 Sprite Texture 크기에 따라 맞춤.
            float ppu = myDataSo.Sprite.pixelsPerUnit;
            Vector2 buttonSize = new Vector2(myDataSo.Sprite.texture.width, myDataSo.Sprite.texture.height) / ppu;
            _button.GetComponent<RectTransform>().sizeDelta = buttonSize;

            // 버튼이 상호작용 가능한지 여부
            _button.interactable = myDataSo.IsInteractable;
            
            // 버튼의 OnClick에 Interact를 리스너로 추가함.
            _button.onClick.AddListener(Interact);
        }
    }

    public void Interact()
    {
        //TODO : 정해진 확률에 따라 DataSo의 PossibleItems 중 하나를 획득.
        
        // 만약 있다면, 루팅 후 스프라이트로 변경.
        if (myDataSo.SpriteAfterInteraction != null)
            _button.image.sprite = myDataSo.SpriteAfterInteraction;
    }
}