
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    private void Start()
    {
        InitSkillUis();
        HideSkill();
        
        // 기본적으로는 Idle 스킬들을 선택하고 시작한다.
        SelectSkill(_passiveSkillUis[0].Skill);
        SelectSkill(_activeSkillUis[0].Skill);
        CancelStartGame();
    }

    [SerializeField] private GameObject _panelsRoot;
    [SerializeField] private GameObject _selectDifficultyRoot;
    [SerializeField] private GameObject _selectSkillsRoot;

    private GameDifficulty _selectedDifficulty;
    
    
    // ------------------------------------------------------------------------
    // 스킬 선택
    // ------------------------------------------------------------------------

    [SerializeField] private List<SkillUI> _passiveSkillUis;
    [SerializeField] private List<SkillUI> _activeSkillUis;

    [SerializeField] private GameObject _skillDisplay;
    [SerializeField] private TextMeshProUGUI _skillNameTmp, _skillDescriptionTmp;

    [SerializeField, DisableInInspector] private PassiveSkillDataSO _selectedPassiveSkill;
    [SerializeField] private ActiveSkillDataSO _selectedActiveSkill;

    private void InitSkillUis()
    {
        foreach (var skillUi in _passiveSkillUis)
        {
            skillUi.Init();
        }
        foreach (var skillUi in _activeSkillUis)
        {
            skillUi.Init();
        }
    }

    public void SelectSkill(SkillDataSO skillData)
    {
        // 패시브 스킬 선택 시
        if (skillData is PassiveSkillDataSO passive)
        {
            foreach (var skillUi in _passiveSkillUis)
            {
                skillUi.SetState(skillUi.Skill == skillData);
            }

            _selectedPassiveSkill = passive;
        }
        // 액티브 스킬 선택 시
        else
        {
            foreach (var skillUi in _activeSkillUis)
            {
                skillUi.SetState(skillUi.Skill == skillData);
            }
            _selectedActiveSkill = skillData as ActiveSkillDataSO;
        }
    }
    
    public void DisplaySkill(SkillDataSO _skillData, Vector2 pos)
    {
        // 아무 효과도 없는 스킬은 표기하지 않음
        if (_skillData.ID == "IDLE") return;
        
        _skillDisplay.SetActive(true);
        _skillDisplay.transform.position = pos + new Vector2(0,2);
        _skillNameTmp.text = _skillData.Name;
        _skillDescriptionTmp.text = _skillData.Description;
    }

    public void HideSkill()
    {
        _skillDisplay.SetActive(false);
    }

    public void OpenSelectDifficultyPanel()
    {
        _panelsRoot.SetActive(true);
        _selectDifficultyRoot.SetActive(true);
        _selectSkillsRoot.SetActive(false);
    }

    public void OpenSelectSkillsPanel()
    {
        _panelsRoot.SetActive(true);
        _selectDifficultyRoot.SetActive(false);
        _selectSkillsRoot.SetActive(true);
    }

    public void CancelStartGame()
    {
        _panelsRoot.SetActive(false);
    }

    public void SelectDifficulty(int difficulty)
    {
        _selectedDifficulty = (GameDifficulty) difficulty;
        OpenSelectSkillsPanel();
    }

    public void StartGame()
    {
        DataManager.Instance.passiveSkill = _selectedPassiveSkill;
        DataManager.Instance.activeSkill = _selectedActiveSkill;
        DataManager.Instance.difficulty = _selectedDifficulty;
        SceneManager.LoadScene("Game");
    }
}