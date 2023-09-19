
using System;

[Serializable]
public class StatusEffect : IItemEffect
{
    public float value;
    
    public void OnObtain()
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop()
    {
        throw new System.NotImplementedException();
    }

    public string GetString()
    {
        throw new System.NotImplementedException();
    }
}