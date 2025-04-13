using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

/// <summary>
/// Description:
/// Manages the special abilities available to the player, both in single-player and multiplayer modes.
/// This includes handling ability button clicks, applying effects (such as double money or tower buffs),
/// managing cooldowns, and, for multiplayer abilities, sending appropriate messages to the server.
/// </summary>
public class SpecialAbilitiesManager : MonoBehaviour
{
    public static SpecialAbilitiesManager Instance;

    [Header("Single-Player Abilities (already implemented)")]
    /// <summary>
    /// Button for activating the Double Money ability.
    /// </summary>
    public Button doubleMoneyButton;
    /// <summary>
    /// Duration for which the Double Money effect lasts.
    /// </summary>
    public float doubleMoneyDuration = 15f;
    /// <summary>
    /// Cooldown time for the Double Money ability.
    /// </summary>
    public float doubleMoneyCooldown = 5f;
    private bool isDoubleMoneyOnCooldown = false;

    /// <summary>
    /// Button for activating the Tower Buff ability.
    /// </summary>
    public Button towerBuffButton;
    /// <summary>
    /// Duration for which the Tower Buff effect lasts.
    /// </summary>
    public float towerBuffDuration = 10f;
    /// <summary>
    /// Cooldown time for the Tower Buff ability.
    /// </summary>
    public float towerBuffCooldown = 5f;
    private bool isTowerBuffOnCooldown = false;

    [Header("Multiplayer-Only Abilities")]
    /// <summary>
    /// Button for activating the Fast Balloons ability.
    /// </summary>
    public Button fastBalloonsButton;
    /// <summary>
    /// Duration for which the Fast Balloons effect lasts.
    /// </summary>
    public float fastBalloonsDuration = 10f;
    /// <summary>
    /// Cooldown time for the Fast Balloons ability.
    /// </summary>
    public float fastBalloonsCooldown = 5f;
    private bool isFastBalloonsOnCooldown = false;

    /// <summary>
    /// Button for activating the Balloon Price Discount ability.
    /// </summary>
    public Button balloonPriceDiscountButton;
    /// <summary>
    /// Duration for which the Balloon Price Discount effect lasts.
    /// </summary>
    public float balloonPriceDiscountDuration = 5f;
    /// <summary>
    /// Cooldown time for the Balloon Price Discount ability.
    /// </summary>
    public float balloonPriceDiscountCooldown = 5f;
    private bool isBalloonPriceDiscountOnCooldown = false;

    /// <summary>
    /// Button for activating the No Money for Opponent ability.
    /// </summary>
    public Button noMoneyOpponentButton;
    /// <summary>
    /// Duration for which the No Money for Opponent effect lasts.
    /// </summary>
    public float noMoneyOpponentDuration = 10f;
    /// <summary>
    /// Cooldown time for the No Money for Opponent ability.
    /// </summary>
    public float noMoneyOpponentCooldown = 5f;
    private bool isNoMoneyOpponentOnCooldown = false;

    /// <summary>
    /// Button for activating the Cloud Screen ability.
    /// </summary>
    public Button cloudScreenButton;
    /// <summary>
    /// Duration for which the Cloud Screen effect lasts.
    /// </summary>
    public float cloudScreenDuration = 10f;
    /// <summary>
    /// Cooldown time for the Cloud Screen ability.
    /// </summary>
    public float cloudScreenCooldown = 5f;
    private bool isCloudScreenOnCooldown = false;
    /// <summary>
    /// Panel displayed as part of the Cloud Screen effect.
    /// </summary>
    public GameObject cloudPanel;

    // For local UI: disabled button color.
    private Color disabledButtonColor = new Color(0.627f, 0.565f, 0.501f, 1f); // #A09080

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Implements the singleton pattern.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure there is only one instance.
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Sets up button listeners, configures their disabled color, and shows/hides multiplayer abilities based on the current game mode.
    /// </summary>
    void Start()
    {
        // Setup listeners for single-player abilities.
        doubleMoneyButton.onClick.AddListener(OnDoubleMoneyClicked);
        towerBuffButton.onClick.AddListener(OnTowerBuffClicked);

        // Setup listeners for multiplayer abilities.
        fastBalloonsButton.onClick.AddListener(OnFastBalloonsClicked);
        balloonPriceDiscountButton.onClick.AddListener(OnBalloonPriceDiscountClicked);
        noMoneyOpponentButton.onClick.AddListener(OnNoMoneyOpponentClicked);
        cloudScreenButton.onClick.AddListener(OnCloudScreenClicked);

        // Setup disabled color blocks for all ability buttons.
        SetupButtonColorBlock(doubleMoneyButton);
        SetupButtonColorBlock(towerBuffButton);
        SetupButtonColorBlock(fastBalloonsButton);
        SetupButtonColorBlock(balloonPriceDiscountButton);
        SetupButtonColorBlock(noMoneyOpponentButton);
        SetupButtonColorBlock(cloudScreenButton);

        // For single-player mode, hide multiplayer ability buttons.
        bool isMultiplayer = (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer);
        fastBalloonsButton.gameObject.SetActive(isMultiplayer);
        balloonPriceDiscountButton.gameObject.SetActive(isMultiplayer);
        noMoneyOpponentButton.gameObject.SetActive(isMultiplayer);
        cloudScreenButton.gameObject.SetActive(isMultiplayer);
    }

    /// <summary>
    /// Configures the disabled color for a given button.
    /// </summary>
    /// <param name="btn">The button to setup.</param>
    private void SetupButtonColorBlock(Button btn)
    {
        var colors = btn.colors;
        colors.disabledColor = disabledButtonColor;
        btn.colors = colors;
    }

    #region Single-Player Abilities

    /// <summary>
    /// Called when the Double Money button is clicked.
    /// Activates the double money effect by doubling the player's money multiplier and starting its cooldown.
    /// </summary>
    private void OnDoubleMoneyClicked()
    {
        if (isDoubleMoneyOnCooldown) return;

        Debug.Log("Double Money Activated!");
        GameManager.Instance.moneyMultiplier = 2f;
        StartCoroutine(DoubleMoneyRoutine());
    }

    /// <summary>
    /// Coroutine managing the Double Money ability.
    /// The effect lasts for a set duration, then the multiplier reverts, followed by a cooldown period.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator DoubleMoneyRoutine()
    {
        doubleMoneyButton.interactable = false;
        isDoubleMoneyOnCooldown = true;

        yield return new WaitForSeconds(doubleMoneyDuration);

        // Revert effect.
        GameManager.Instance.moneyMultiplier = 1f;

        // Cooldown period.
        yield return new WaitForSeconds(doubleMoneyCooldown);

        isDoubleMoneyOnCooldown = false;
        doubleMoneyButton.interactable = true;
    }

    /// <summary>
    /// Called when the Tower Buff button is clicked.
    /// Activates a buff that increases towers' range and fire rate, then refreshes all tower stats.
    /// </summary>
    private void OnTowerBuffClicked()
    {
        if (isTowerBuffOnCooldown) return;

        Debug.Log("Tower Buff Activated!");
        isTowerBuffOnCooldown = true;
        towerBuffButton.interactable = false;

        // Set global buff factors.
        GameManager.Instance.rangeBuffFactor = 1.5f;
        GameManager.Instance.fireRateBuffFactor = 1.5f;

        // Refresh all towers to apply new stats.
        GameManager.Instance.cellManager.RefreshAllTowersStats();

        StartCoroutine(TowerBuffRoutine());
    }

    /// <summary>
    /// Coroutine managing the Tower Buff ability.
    /// The buff is active for a specific duration, after which it reverts and enters a cooldown period.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator TowerBuffRoutine()
    {
        yield return new WaitForSeconds(towerBuffDuration);

        // Revert buff factors.
        GameManager.Instance.rangeBuffFactor = 1f;
        GameManager.Instance.fireRateBuffFactor = 1f;
        GameManager.Instance.cellManager.RefreshAllTowersStats();

        yield return new WaitForSeconds(towerBuffCooldown);

        isTowerBuffOnCooldown = false;
        towerBuffButton.interactable = true;
    }

    #endregion

    #region Multiplayer Abilities

    /// <summary>
    /// Called when the Fast Balloons button is clicked.
    /// Sends a request to the server to activate the Fast Balloons ability.
    /// </summary>
    private void OnFastBalloonsClicked()
    {
        if (isFastBalloonsOnCooldown) return;

        Debug.Log("FastBalloons Activated (Local)!");
        isFastBalloonsOnCooldown = true;
        fastBalloonsButton.interactable = false;

        JObject dataObj = new JObject
        {
            ["AbilityName"] = "FastBalloons",
        };
        NetworkManager.Instance.messageSender.SendAuthenticatedMessage("UseMultiplayerAbility", dataObj);
    }

    /// <summary>
    /// Called when the Balloon Price Discount button is clicked.
    /// Activates a temporary discount on balloon prices locally by adjusting the discount factor.
    /// </summary>
    private void OnBalloonPriceDiscountClicked()
    {
        if (isBalloonPriceDiscountOnCooldown) return;

        Debug.Log("BalloonPriceDiscount Activated (Local)!");
        isBalloonPriceDiscountOnCooldown = true;
        balloonPriceDiscountButton.interactable = false;

        StartCoroutine(BalloonPriceDiscountRoutine());
    }

    /// <summary>
    /// Coroutine managing the Balloon Price Discount ability.
    /// Applies a discount factor to balloon prices for a fixed duration, then reverts and starts a cooldown.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator BalloonPriceDiscountRoutine()
    {
        BalloonSendingPanel balloonSendingPanel = FindObjectOfType<BalloonSendingPanel>();
        if (balloonSendingPanel != null)
        {
            foreach (var balloonButton in balloonSendingPanel.balloonButtons)
            {
                balloonButton.tempDiscountFactor = 0.5f;
                balloonButton.Refresh();
            }
        }

        yield return new WaitForSeconds(balloonPriceDiscountDuration);

        // Revert discount.
        if (balloonSendingPanel != null)
        {
            foreach (var balloonButton in balloonSendingPanel.balloonButtons)
            {
                balloonButton.tempDiscountFactor = 1f;
                balloonButton.Refresh();
            }
        }

        yield return new WaitForSeconds(balloonPriceDiscountCooldown);

        isBalloonPriceDiscountOnCooldown = false;
        balloonPriceDiscountButton.interactable = true;
    }

    /// <summary>
    /// Called when the No Money for Opponent button is clicked.
    /// Sends a request to the server to apply the no money effect on the opponent.
    /// </summary>
    private void OnNoMoneyOpponentClicked()
    {
        if (isNoMoneyOpponentOnCooldown) return;

        Debug.Log("NoMoneyForOpponent Activated (Local)!");
        isNoMoneyOpponentOnCooldown = true;
        noMoneyOpponentButton.interactable = false;

        JObject dataObj = new JObject
        {
            ["AbilityName"] = "NoMoneyForOpponent",
        };
        NetworkManager.Instance.messageSender.SendAuthenticatedMessage("UseMultiplayerAbility", dataObj);

        // We do NOT do anything locally (since the effect is for the OPPONENT).

        StartCoroutine(NoMoneyOpponentRoutine());
    }

    /// <summary>
    /// Coroutine managing the No Money for Opponent ability.
    /// Waits for the effect duration and cooldown without applying any local effect.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator NoMoneyOpponentRoutine()
    {
        // We have no local effect. The Opponent sets moneyMultiplier=0 => or a "disallowMoney" flag.

        yield return new WaitForSeconds(noMoneyOpponentDuration);
        // The effect on the opponent will end on their side too. We'll show the user a cooldown only.

        yield return new WaitForSeconds(noMoneyOpponentCooldown);

        isNoMoneyOpponentOnCooldown = false;
        noMoneyOpponentButton.interactable = true;
    }

    /// <summary>
    /// Called when the Cloud Screen button is clicked.
    /// Sends a request to the server to activate the Cloud Screen effect.
    /// </summary>
    private void OnCloudScreenClicked()
    {
        if (isCloudScreenOnCooldown) return;

        Debug.Log("CloudScreen Activated (Local)!");
        isCloudScreenOnCooldown = true;
        cloudScreenButton.interactable = false;

        JObject dataObj = new JObject
        {
            ["AbilityName"] = "CloudScreen",
        };
        NetworkManager.Instance.messageSender.SendAuthenticatedMessage("UseMultiplayerAbility", dataObj);

        StartCoroutine(CloudScreenRoutine());
    }

    /// <summary>
    /// Coroutine managing the Cloud Screen ability.
    /// Waits for the duration of the effect and the cooldown period before re-enabling the Cloud Screen button.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator CloudScreenRoutine()
    {
        // No local effect => effect is purely on the opponent’s screen
        yield return new WaitForSeconds(cloudScreenDuration);
        // done

        yield return new WaitForSeconds(cloudScreenCooldown);

        isCloudScreenOnCooldown = false;
        cloudScreenButton.interactable = true;
    }

    #endregion

    #region Reset Methods

    /// <summary>
    /// Resets all special ability cooldowns and re-enables all associated buttons.
    /// Typically called by a SurpriseBox or similar game mechanic.
    /// </summary>
    public void ResetAllCooldowns()
    {
        isDoubleMoneyOnCooldown = false;
        doubleMoneyButton.interactable = true;

        isTowerBuffOnCooldown = false;
        towerBuffButton.interactable = true;

        isFastBalloonsOnCooldown = false;
        fastBalloonsButton.interactable = true;

        isBalloonPriceDiscountOnCooldown = false;
        balloonPriceDiscountButton.interactable = true;

        isNoMoneyOpponentOnCooldown = false;
        noMoneyOpponentButton.interactable = true;

        isCloudScreenOnCooldown = false;
        cloudScreenButton.interactable = true;

        Debug.Log("All special ability cooldowns reset by SurpriseBox!");
    }

    #endregion
}

