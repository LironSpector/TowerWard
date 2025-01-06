using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages Single-Player special abilities:
/// 1) Double Money
/// 2) Tower Buff (+50% range/fireRate)
/// </summary>
public class SpecialAbilitiesManager : MonoBehaviour
{
    [Header("Ability Buttons")]
    public Button doubleMoneyButton;
    public Button towerBuffButton;

    [Header("Double Money Ability")]
    public float doubleMoneyDuration = 15f;
    public float doubleMoneyCooldown = 120f;
    private bool isDoubleMoneyOnCooldown = false;

    [Header("Tower Buff Ability")]
    public float towerBuffDuration = 10f;
    public float towerBuffCooldown = 90f;
    private bool isTowerBuffOnCooldown = false;

    private Color disabledButtonColor = new Color(0.627f, 0.565f, 0.501f, 1f); // #A09080

    void Start()
    {
        doubleMoneyButton.interactable = true;
        towerBuffButton.interactable = true;

        doubleMoneyButton.onClick.AddListener(OnDoubleMoneyClicked);
        towerBuffButton.onClick.AddListener(OnTowerBuffClicked);

        SetupButtonColorBlock(doubleMoneyButton);
        SetupButtonColorBlock(towerBuffButton);
    }

    private void SetupButtonColorBlock(Button btn)
    {
        var colors = btn.colors;
        colors.disabledColor = disabledButtonColor;
        btn.colors = colors;
    }

    // =========================
    // DOUBLE MONEY
    // =========================
    private void OnDoubleMoneyClicked()
    {
        if (isDoubleMoneyOnCooldown) return;

        Debug.Log("Double Money Activated!");
        GameManager.Instance.moneyMultiplier = 2f;

        StartCoroutine(DoubleMoneyRoutine());
    }

    private IEnumerator DoubleMoneyRoutine()
    {
        doubleMoneyButton.interactable = false;
        isDoubleMoneyOnCooldown = true;

        yield return new WaitForSeconds(doubleMoneyDuration);

        // revert
        GameManager.Instance.moneyMultiplier = 1f;

        // cooldown
        yield return new WaitForSeconds(doubleMoneyCooldown);

        isDoubleMoneyOnCooldown = false;
        doubleMoneyButton.interactable = true;
    }

    // =========================
    // TOWER BUFF
    // =========================
    private void OnTowerBuffClicked()
    {
        if (isTowerBuffOnCooldown) return;

        Debug.Log("Tower Buff Activated!");
        isTowerBuffOnCooldown = true;
        towerBuffButton.interactable = false;

        // 1) Set global buff factors
        GameManager.Instance.rangeBuffFactor = 1.5f;
        GameManager.Instance.fireRateBuffFactor = 1.5f;

        // 2) Refresh stats => each tower re-calculates range/fireRate
        GameManager.Instance.RefreshAllTowersStats();

        StartCoroutine(TowerBuffRoutine());
    }

    private IEnumerator TowerBuffRoutine()
    {
        // Wait for effect
        yield return new WaitForSeconds(towerBuffDuration);

        // revert buff => set factors to 1.0
        GameManager.Instance.rangeBuffFactor = 1f;
        GameManager.Instance.fireRateBuffFactor = 1f;

        // re-apply stats
        GameManager.Instance.RefreshAllTowersStats();

        // wait cooldown
        yield return new WaitForSeconds(towerBuffCooldown);

        isTowerBuffOnCooldown = false;
        towerBuffButton.interactable = true;
    }
}
