
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/GameConstants")]
public class GameConstantsSO : ScriptableObject
{
    /*
     * MonoBehaviour 대신 ScriptableObject를 이용해 씬간 종속성을 없앴다.
     */
    private static GameConstantsSO _instance;
    public static GameConstantsSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConstantsSO>("Data/GameConstantsSO_Instance");
                if(_instance == null)
                    Debug.LogError("Failed to load GameConstantsSO!");
            }

            return _instance;
        }
    }

    public int MaxFloor;
    public float RoomHeight;
}