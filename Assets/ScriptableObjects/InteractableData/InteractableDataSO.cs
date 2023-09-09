using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/InteractableData")]
public class InteractableDataSO : ScriptableObject
{
    public Sprite Sprite;

    /*
     * 상호작용 후의(루팅한) 스프라이트.
     * 없다면 Sprite에서 변경하지 않음.
     */
    public Sprite SpriteAfterInteraction;

    /*
     * 만약 false라면, 게임 내에서 클릭해도 아무 일도 일어나지 않음 (Sprite로써의 역할만 함)
     */
    public bool IsInteractable;
    
    /*
     * 상호작용 시 획득할 수 있는 아이템의 풀.
     * 만약 IsInteractable && PossibleItems.Count == 0 일 시에는 모든 종류의 아이템 획득 가능.
     */
    public List<ItemDataSO> PossibleItems;
}
