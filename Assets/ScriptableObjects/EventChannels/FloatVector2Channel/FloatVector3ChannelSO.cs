
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ScriptableObject/EventChannel/FloatVector2Channel")]

/*
 * 대미지와 위치, 그리고 크리티컬 정보를 받아 화면에 띄우는 용도의 채널.
 * 크리티컬 정보는 Vector3의 z의 값이 양수인지 아닌지로 확인할 수 있다.
 */
public class FloatVector3ChannelSO : ScriptableObject
{
    public UnityAction<float, Vector3> OnEventRaised;

    public void OnRaise(float value, Vector3 posAndCrit)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(value,posAndCrit);
        }
    }
}