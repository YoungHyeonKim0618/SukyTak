
/*
 * 어떤 층에 처음 내려왔을 때에 등록하는 데이터.
 */

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SkillData/Register/LowestFloor")]
public class LowestFloorRegisterDataSO : SkillRegisterDataSO
{
    [Tooltip("특정 층에서 발동을 원할 때")]
    public int DestinationFloor;
    [Tooltip("매 n층 내려갈 때마다 발동을 원할 때")]
    public int Interval;
    
    // 어떤 층에 처음 내려왔을 때 Invoke되는 이벤트 채널.
    public IntChannelSO LowestFloorChannel;
    
    public override void Register(SkillEffectDataSO effect)
    {
        LowestFloorChannel.OnEventRaised += (x) => Arrive(x,effect);
    }

    private void Arrive(int floor, SkillEffectDataSO effect)
    {
        if (DestinationFloor != 0)
        {
            if(floor == DestinationFloor) 
                effect.OnUse();
        }
        else
        {
            if((floor + 1)% Interval == 0)
                effect.OnUse();
        }
    }
}