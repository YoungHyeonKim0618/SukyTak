
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/VoidChannel")]
public class VoidChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;
    
    public void OnRaise()
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke();
        }
    }
}