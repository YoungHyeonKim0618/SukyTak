using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/RoomData")]
public class RoomDataSO : ScriptableObject
{
    public string ID;
    public Sprite Background;
    
    /*
     * 방에 있는 InteractableData와 그 위치/크기/회전 정보들.
     */
    public List<VisualObject> VisualObjects;
}
[Serializable]
public struct VisualObject
{
    public InteractableDataSO interactableSo;
    public Vector2 position;
    public Vector2 scale;
}