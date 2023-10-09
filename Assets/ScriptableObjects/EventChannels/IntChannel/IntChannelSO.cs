
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/IntChannel")]
public class IntChannelSO : ScriptableObject
{
    public UnityAction<int> OnEventRaised;

    public void OnRaise(int value)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(value);
        }
    }
}