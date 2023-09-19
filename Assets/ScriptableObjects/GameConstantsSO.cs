
using System.Collections.Generic;
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
    public float RoomWidth;
    public float RoomHeight;
    public float RottenFoodDmg;

    public List<float> RootChancePerDifficulty;
    public float GetRootChanceFromDifficulty(GameDifficulty difficulty)
    {
        float ret = 0;
        switch (difficulty)
        {
            case GameDifficulty.EASY:
                ret = RootChancePerDifficulty[0];
                break;
            case GameDifficulty.NORMAL:
                ret = RootChancePerDifficulty[1];
                break;
            case GameDifficulty.HARD:
                ret = RootChancePerDifficulty[2];
                break;
            case GameDifficulty.HARDCORE:
                ret = RootChancePerDifficulty[3];
                break;
        }

        return ret;
    }
    
    
    [SerializeField] private List<ItemDataSO> _rootableItems;
    public ItemDataSO GetRandomRootableItem()
    {
        int index = Random.Range(0, _rootableItems.Count);
        return _rootableItems[index];
    }
}