
using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    [DisableInInspector]
    public int MapSeed;
    [DisableInInspector]
    public GameDifficulty Difficulty;
}