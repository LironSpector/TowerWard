using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class SpecialAbilitiesManager : MonoBehaviour
{
    public static SpecialAbilitiesManager Instance;

    [Header("Single-Player Abilities (already implemented)")]
    public Button doubleMoneyButton;
    public float doubleMoneyDuration = 15f;
    public float doubleMoneyCooldown = 5f;
    private bool isDoubleMoneyOnCooldown = false;

    public Button towerBuffButton;
    public float towerBuffDuration = 10f;
    public float towerBuffCooldown = 5f;
    private bool isTowerBuffOnCooldown = false;

    [Header("Multiplayer-Only Abilities")]
    public Button fastBalloonsButton;
    public float fastBalloonsDuration = 10f;
    public float fastBalloonsCooldown = 5f;  // 3 minutes
    private bool isFastBalloonsOnCooldown = false;

    public Button balloonPriceDiscountButton;
    public float balloonPriceDiscountDuration = 5f;
    public float balloonPriceDiscountCooldown = 5f;
    private bool isBalloonPriceDiscountOnCooldown = false;

    public Button noMoneyOpponentButton;
    public float noMoneyOpponentDuration = 10f;
    public float noMoneyOpponentCooldown = 5f;
    private bool isNoMoneyOpponentOnCooldown = false;

    public Button cloudScreenButton;
    public float cloudScreenDuration = 10f;
    public float cloudScreenCooldown = 5f;
    private bool isCloudScreenOnCooldown = false;
    public GameObject cloudPanel;

    // For local usage
    private Color disabledButtonColor = new Color(0.627f, 0.565f, 0.501f, 1f); // #A09080


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure there's only one instance
        }
    }

    void Start()
    {
        // Single player abilities
        doubleMoneyButton.onClick.AddListener(OnDoubleMoneyClicked);
        towerBuffButton.onClick.AddListener(OnTowerBuffClicked);

        // Multiplayer abilities
        fastBalloonsButton.onClick.AddListener(OnFastBalloonsClicked);
        balloonPriceDiscountButton.onClick.AddListener(OnBalloonPriceDiscountClicked);
        noMoneyOpponentButton.onClick.AddListener(OnNoMoneyOpponentClicked);
        cloudScreenButton.onClick.AddListener(OnCloudScreenClicked);

        SetupButtonColorBlock(doubleMoneyButton);
        SetupButtonColorBlock(towerBuffButton);
        SetupButtonColorBlock(fastBalloonsButton);
        SetupButtonColorBlock(balloonPriceDiscountButton);
        SetupButtonColorBlock(noMoneyOpponentButton);
        SetupButtonColorBlock(cloudScreenButton);

        // If single-player => hide these 4 multiplayer buttons
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SinglePlayer)
        {
            fastBalloonsButton.gameObject.SetActive(false);
            balloonPriceDiscountButton.gameObject.SetActive(false);
            noMoneyOpponentButton.gameObject.SetActive(false);
            cloudScreenButton.gameObject.SetActive(false);
        }
        else
        {
            // If multiplayer => show them
            fastBalloonsButton.gameObject.SetActive(true);
            balloonPriceDiscountButton.gameObject.SetActive(true);
            noMoneyOpponentButton.gameObject.SetActive(true);
            cloudScreenButton.gameObject.SetActive(true);
        }
    }

    private void SetupButtonColorBlock(Button btn)
    {
        var colors = btn.colors;
        colors.disabledColor = disabledButtonColor;
        btn.colors = colors;
    }

    // -------------------------------------------------------
    // SINGLE-PLAYER ABILITIES
    // -------------------------------------------------------
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


    // -------------------------------------------------------
    // MULTIPLAYER ABILITIES
    // -------------------------------------------------------

    private void OnFastBalloonsClicked()
    {
        if (isFastBalloonsOnCooldown) return;

        Debug.Log("FastBalloons Activated (Local)!");
        isFastBalloonsOnCooldown = true;
        fastBalloonsButton.interactable = false;

        var msg = new
        {
            Type = "UseMultiplayerAbility",
            Data = new
            {
                AbilityName = "FastBalloons"
            }
        };
        string json = JsonConvert.SerializeObject(msg);
        NetworkManager.Instance.SendMessageWithLengthPrefix(json);
    }

    private void OnBalloonPriceDiscountClicked()
    {
        if (isBalloonPriceDiscountOnCooldown) return;

        Debug.Log("BalloonPriceDiscount Activated (Local)!");
        isBalloonPriceDiscountOnCooldown = true;
        balloonPriceDiscountButton.interactable = false;

        StartCoroutine(BalloonPriceDiscountRoutine());
    }

    private IEnumerator BalloonPriceDiscountRoutine()
    {
        // e.g. define a variable => GameManager.Instance.balloonPriceFactor = 0.5f
        // or in your "BalloonButton" or "BalloonSendingPanel" => interpret cost = baseCost * factor
        BalloonSendingPanel balloonSendingPanel = FindObjectOfType<BalloonSendingPanel>();
        if (balloonSendingPanel != null)
        {
            foreach (var balloonButton in balloonSendingPanel.balloonButtons)
            {
                balloonButton.tempDiscountFactor = 0.5f; // or however you do it
                balloonButton.Refresh();
            }
        }

        yield return new WaitForSeconds(balloonPriceDiscountDuration);

        // revert
        if (balloonSendingPanel != null)
        {
            foreach (var balloonButton in balloonSendingPanel.balloonButtons)
            {
                balloonButton.tempDiscountFactor = 1f;
                balloonButton.Refresh();
            }
        }

        // cooldown
        yield return new WaitForSeconds(balloonPriceDiscountCooldown);

        isBalloonPriceDiscountOnCooldown = false;
        balloonPriceDiscountButton.interactable = true;
    }


    private void OnNoMoneyOpponentClicked()
    {
        if (isNoMoneyOpponentOnCooldown) return;

        Debug.Log("NoMoneyForOpponent Activated (Local)!");
        isNoMoneyOpponentOnCooldown = true;
        noMoneyOpponentButton.interactable = false;

        // Send message to server => it will forward to the opponent
        var msg = new
        {
            Type = "UseMultiplayerAbility",
            Data = new
            {
                AbilityName = "NoMoneyForOpponent"
            }
        };
        string json = JsonConvert.SerializeObject(msg);
        NetworkManager.Instance.SendMessageWithLengthPrefix(json);

        // We do NOT do anything locally (since the effect is for the OPPONENT).
        StartCoroutine(NoMoneyOpponentRoutine());
    }

    private IEnumerator NoMoneyOpponentRoutine()
    {
        // We have no local effect. The Opponent sets moneyMultiplier=0 => or a "disallowMoney" flag.

        yield return new WaitForSeconds(noMoneyOpponentDuration);
        // The effect on the opponent will end on their side too. We'll show the user a cooldown only.

        yield return new WaitForSeconds(noMoneyOpponentCooldown);
        isNoMoneyOpponentOnCooldown = false;
        noMoneyOpponentButton.interactable = true;
    }


    private void OnCloudScreenClicked()
    {
        if (isCloudScreenOnCooldown) return;

        Debug.Log("CloudScreen Activated (Local)!");
        isCloudScreenOnCooldown = true;
        cloudScreenButton.interactable = false;

        // Send message => server => opponent => opponent shows the clouds
        var msg = new
        {
            Type = "UseMultiplayerAbility",
            Data = new
            {
                AbilityName = "CloudScreen"
            }
        };
        string json = JsonConvert.SerializeObject(msg);
        NetworkManager.Instance.SendMessageWithLengthPrefix(json);

        StartCoroutine(CloudScreenRoutine());
    }

    private IEnumerator CloudScreenRoutine()
    {
        // No local effect => effect is purely on the opponent’s screen

        yield return new WaitForSeconds(cloudScreenDuration);
        // done

        yield return new WaitForSeconds(cloudScreenCooldown);
        isCloudScreenOnCooldown = false;
        cloudScreenButton.interactable = true;
    }


    public void ResetAllCooldowns()
    {
        // If you track coroutines, you'd stop them or set them to zero
        // For simplicity, set the bools false + re-enable buttons:

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

}
