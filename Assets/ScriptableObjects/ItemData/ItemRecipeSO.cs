
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Recipe")]
public class ItemRecipeSO : ScriptableObject
{
    public ItemDataSO Output;

    public bool HandCombinable;
    
    // 조합하기 위해 필요한 가구 레벨.
    // 가구의 종류는 PlayerInventory에서 결정함.
    public int NecessaryFurnitureLevel;
    
    public List<ItemDataSO> Inputs;
}