
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public int mapSeed;
    [DisableInInspector]
    public GameDifficulty difficulty;


    [DisableInInspector]
    public PassiveSkillDataSO passiveSkill;
    [DisableInInspector]
    public ActiveSkillDataSO activeSkill;

}