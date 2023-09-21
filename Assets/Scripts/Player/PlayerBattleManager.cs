
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBattleManager : MonoBehaviour
{

    /*
     * 플레이어나 몬스터의 공격 애니메이션 중 다른 행동을 하지 못하게 하는 상위 레이어의 투명한 이미지.
     */
    [SerializeField] private Image _raycastBlocker;

    private void Start()
    {
        UnblockRaycast();
        CloseHitSelectionPanel();
    }

    public void BlockRaycast()
    {
        _raycastBlocker.gameObject.SetActive(true);
    }
    public void UnblockRaycast()
    {
        _raycastBlocker.gameObject.SetActive(false);
    }
    
    // ------------------------------------------------------------------------
    // 전투 상태
    // ------------------------------------------------------------------------
    
    [Header("전투 상태")] [SerializeField]
    // 가방을 여는 버튼
    private GameObject _inventoryButton;
    
    public void BeginBattle()
    {
        _inventoryButton.SetActive(false);
    }

    public void EndBattle()
    {
        _inventoryButton.SetActive(true);
    }

    // ------------------------------------------------------------------------
    // 전투
    // ------------------------------------------------------------------------

    [Header("전투")] [SerializeField]private HitSelectionPanel _hitSelectionPanel;

    public void OpenHitSelectionPanel(Monster monster)
    {
        _hitSelectionPanel.gameObject.SetActive(true);
        _hitSelectionPanel.SetMonster(monster);
    }

    public void CloseHitSelectionPanel()
    {
        _hitSelectionPanel.gameObject.SetActive(false);
    }
}