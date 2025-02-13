using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class BankManager : MonoBehaviour
{
    [Header("UI References")]
    public Button bankOpenButton;       // The button with a bank icon
    public TMP_InputField investmentInput; // The text input for entering the invest amount
    public Button submitInvestmentButton;  // Button to submit the investment

    [Header("Bank Settings")]
    public float investDuration = 120f;  // 2 minutes
    public float investReturnMultiplier = 1.2f; // 20% profit

    private bool isInvesting = false;    // track if we already have an ongoing investment
    private int investedAmount = 0;      // how much was invested

    void Start()
    {
        // Hook up event listeners
        bankOpenButton.onClick.AddListener(OnBankButtonClicked);
        submitInvestmentButton.onClick.AddListener(OnSubmitInvestment);

        // At start, hide or disable the input / submit if you want
        investmentInput.gameObject.SetActive(false);
        submitInvestmentButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when the "Bank" button is clicked.
    /// Toggles showing the input field if not investing, else does nothing or show a message.
    /// </summary>
    private void OnBankButtonClicked()
    {
        if (GameManager.Instance.isGameOver) return;

        // If we're currently investing, do nothing (or show a message that it's locked).
        if (isInvesting)
        {
            Debug.Log("Already have money in the bank. Wait until it returns!");
            return;
        }

        // Otherwise, toggle the input + submit
        bool currentlyActive = investmentInput.gameObject.activeSelf;
        // If currently hidden => show, else hide
        investmentInput.gameObject.SetActive(!currentlyActive);
        submitInvestmentButton.gameObject.SetActive(!currentlyActive);
    }

    /// <summary>
    /// Called when the user presses "Submit" for the investment input.
    /// We parse the input, check if the user can afford it, then start the investment if valid.
    /// </summary>
    private void OnSubmitInvestment()
    {
        if (GameManager.Instance.isGameOver) return;
        if (isInvesting) return; // already investing => ignore

        // Parse the input
        string inputText = investmentInput.text;
        if (int.TryParse(inputText, out int amount))
        {
            // Check if user can afford
            if (!GameManager.Instance.CanAfford(amount))
            {
                Debug.Log("Not enough currency to invest!");
                return;
            }

            if (amount <= 0)
            {
                Debug.Log("Investment amount must be positive!");
                return;
            }

            // Deduct from currency
            GameManager.Instance.SpendCurrency(amount);
            investedAmount = amount;
            isInvesting = true;

            Debug.Log($"Invested {amount} currency in the bank. It will return in {investDuration} seconds with a multiplier of {investReturnMultiplier}.");

            // Optionally hide input & submit again
            investmentInput.gameObject.SetActive(false);
            submitInvestmentButton.gameObject.SetActive(false);

            // Start the coroutine to wait 2 minutes
            StartCoroutine(InvestmentRoutine());
        }
        else
        {
            Debug.Log("Invalid input. Please enter a valid number.");
        }
    }

    /// <summary>
    /// Waits for the investDuration, then returns the money with the multiplier.
    /// After that, user can invest again.
    /// </summary>
    private IEnumerator InvestmentRoutine()
    {
        // Wait the 2 minutes
        yield return new WaitForSeconds(investDuration);

        // Return money * 1.2
        int finalAmount = Mathf.RoundToInt(investedAmount * investReturnMultiplier);
        GameManager.Instance.AddCurrency(finalAmount);

        Debug.Log($"Investment of {investedAmount} is returned as {finalAmount}.");

        // reset
        investedAmount = 0;
        isInvesting = false;
    }
}
