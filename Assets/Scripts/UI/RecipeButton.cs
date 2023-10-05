
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

/*
 * 레시피를 선택하는 버튼의 클래스.
 * 마우스를 올렸을 때 ItemUI와 유사하게 설명 창이 떠야 한다.
 */
public class RecipeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemRecipeSO _data;
    
    [SerializeField] private Button _button;
    public Button GetButton => _button;

    [SerializeField] private Image _handUncombinableImage;
    

    public void SetItemData(ItemRecipeSO data)
    {
        _data = data;
        _handUncombinableImage.gameObject.SetActive(!data.HandCombinable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Player.Instance.DisplayItem(_data.Output, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Player.Instance.CloseDisplay();
    }
}