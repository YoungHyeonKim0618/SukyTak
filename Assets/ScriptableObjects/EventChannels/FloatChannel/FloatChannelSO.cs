
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/FloatChannel")]
public class FloatChannelSO : ScriptableObject
{
    public UnityAction<float> OnEventRaised;

    public void OnRaise(float value)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(value);
        }
    }
}