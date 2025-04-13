using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Description:
/// Manages the bank system where players can invest a portion of their currency and receive it back after a fixed duration,
/// earning a profit determined by a multiplier. This script handles the bank UI interactions (such as opening the bank,
/// entering an investment amount, and submitting the investment), checks whether the player can afford the investment,
/// deducts the investment amount, and after a specified duration, returns the invested money with profit.
/// </summary>
public class BankManager : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Button that opens the bank (shows the investment input panel).
    /// </summary>
    public Button bankOpenButton;

    /// <summary>
    /// Input field for the player to enter the investment amount.
    /// </summary>
    public TMP_InputField investmentInput;

    /// <summary>
    /// Button to submit the entered investment amount.
    /// </summary>
    public Button submitInvestmentButton;

    [Header("Bank Settings")]
    /// <summary>
    /// Duration (in seconds) for the investment; after this time the investment matures.
    /// </summary>
    public float investDuration = 120f;  // 2 minutes

    /// <summary>
    /// Multiplier applied to the invested amount upon maturity (e.g., 1.2 for a 20% profit).
    /// </summary>
    public float investReturnMultiplier = 1.2f;

    // Private variables for tracking investment state.
    private bool isInvesting = false;    // Whether an investment is currently active.
    private int investedAmount = 0;      // The amount that was invested.

    /// <summary>
    /// Initialization: sets up event listeners for the bank button and the investment submission button,
    /// and hides the investment input and submit button at the start.
    /// </summary>
    void Start()
    {
        // Hook up event listeners.
        bankOpenButton.onClick.AddListener(OnBankButtonClicked);
        submitInvestmentButton.onClick.AddListener(OnSubmitInvestment);

        // Initially hide the investment input and submit button.
        investmentInput.gameObject.SetActive(false);
        submitInvestmentButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when the bank button is clicked.
    /// If no investment is in progress and the game is not over, toggles the visibility of the investment input and submit button.
    /// </summary>
    private void OnBankButtonClicked()
    {
        if (GameManager.Instance.isGameOver)
            return;

        // If an investment is already active, do nothing (or alternatively, notify the user).
        if (isInvesting)
        {
            Debug.Log("Already have money in the bank. Wait until it returns!");
            return;
        }

        // Toggle the display of the investment input field and submit button.
        bool currentlyActive = investmentInput.gameObject.activeSelf;
        investmentInput.gameObject.SetActive(!currentlyActive);
        submitInvestmentButton.gameObject.SetActive(!currentlyActive);
    }

    /// <summary>
    /// Called when the player submits an investment amount.
    /// Parses the investment input, verifies affordability and validity, deducts the investment amount,
    /// and starts a coroutine to return the investment after a set duration.
    /// </summary>
    private void OnSubmitInvestment()
    {
        if (GameManager.Instance.isGameOver || isInvesting)
            return; // Do nothing if the game is over or an investment is already ongoing.

        // Parse the input from the investment field.
        string inputText = investmentInput.text;
        if (int.TryParse(inputText, out int amount))
        {
            // Check if the player can afford the investment.
            if (!GameManager.Instance.CanAfford(amount))
            {
                Debug.Log("Not enough currency to invest!");
                return;
            }

            // Investment amount must be positive.
            if (amount <= 0)
            {
                Debug.Log("Investment amount must be positive!");
                return;
            }

            // Deduct the investment amount from the player's currency.
            GameManager.Instance.SpendCurrency(amount);
            investedAmount = amount;
            isInvesting = true;

            Debug.Log($"Invested {amount} currency in the bank. It will return in {investDuration} seconds with a multiplier of {investReturnMultiplier}.");

            // Hide the investment input and submit button after starting the investment.
            investmentInput.gameObject.SetActive(false);
            submitInvestmentButton.gameObject.SetActive(false);

            // Start the investment routine coroutine.
            StartCoroutine(InvestmentRoutine());
        }
        else
        {
            Debug.Log("Invalid input. Please enter a valid number.");
        }
    }

    /// <summary>
    /// Coroutine that waits for the specified investment duration, then returns the invested amount multiplied by a profit multiplier.
    /// Once the investment is matured and returned, resets the investment state to allow new investments.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    private IEnumerator InvestmentRoutine()
    {
        // Wait for the investment duration.
        yield return new WaitForSeconds(investDuration);

        // Calculate the final amount to return with profit.
        int finalAmount = Mathf.RoundToInt(investedAmount * investReturnMultiplier);
        GameManager.Instance.AddCurrency(finalAmount);

        Debug.Log($"Investment of {investedAmount} is returned as {finalAmount}.");

        // Reset investment state.
        investedAmount = 0;
        isInvesting = false;
    }
}
