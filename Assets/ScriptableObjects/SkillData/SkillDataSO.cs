
using UnityEngine;


public abstract class SkillDataSO : ScriptableObject
{
    public string ID;
    public string Name;
    public Sprite Sprite;
    
    [TextArea(3,10)]
    public string Description;
}