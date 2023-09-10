
using System;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

/*
 * 클릭해서 상호작용할 수 있는 오브젝트.
 * 여기선 사실상 클릭해서 아이템을 획득할 수 있는 것들을 지칭한다.
 */
public class Interactable : MonoBehaviour
{
    public InteractableDataSO myDataSo;

    private Image _image;

    public void InitInteractable(InteractableDataSO dataSO)
    {
        if(dataSO != null)
        {
            myDataSo = dataSO;
            
            _image = gameObject.AddComponent<Image>();
            _image.sprite = myDataSo.Sprite;
            
            // 상호작용 가능하다면 버튼을 실제 생성한다.
            if (dataSO.IsInteractable)
            {
                Button button = gameObject.AddComponent<Button>();
                
                // 버튼의 OnClick에 Interact를 리스너로 추가함.
                button.onClick.AddListener(Interact);
            }
            else _image.raycastTarget = false;
            

            // 자신의 크기를 DataSo의 Sprite Texture 크기에 따라 맞춤.
            float ppu = myDataSo.Sprite.pixelsPerUnit;
            Vector2 buttonSize = new Vector2(myDataSo.Sprite.texture.width, myDataSo.Sprite.texture.height) / ppu;
            GetComponent<RectTransform>().sizeDelta = buttonSize;
            
        }
    }

    public void Interact()
    {
        //TODO : 정해진 확률에 따라 DataSo의 PossibleItems 중 하나를 획득, 다시 Interact 못하게 비활성화.
        
        // 만약 있다면, 루팅 후 스프라이트로 변경.
        if (myDataSo.SpriteAfterInteraction != null)
            _image.sprite = myDataSo.SpriteAfterInteraction;
    }
}