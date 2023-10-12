
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/ItemDataChannel")]
public class ItemDataChannelSO : ScriptableObject
{
    public UnityAction<ItemDataSO> OnEventRaised;

    public void OnRaise(ItemDataSO data)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(data);
        }
    }
}