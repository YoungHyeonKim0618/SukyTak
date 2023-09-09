
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/RoomChannel")]
public class RoomEventChannelSO : ScriptableObject
{
    public UnityAction<Room> OnEventRaised;

    public void OnRaise(Room room)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(room);
        }
    }
}