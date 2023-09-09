
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/RoomPositionChannel")]
public class RoomPositionEventChannelSO : ScriptableObject
{
    public UnityAction<RoomPosition> OnEventRaised;

    public void OnRaise(RoomPosition roomPosition)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(roomPosition);
        }
    }
}