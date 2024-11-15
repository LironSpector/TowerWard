using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int lives = 20;
    public int currency = 100;

    // Store occupied cell positions and associated towers
    public Dictionary<Vector2, Tower> occupiedCells = new Dictionary<Vector2, Tower>();

    // UI Elements
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI currencyText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    public bool isGameOver = false; // Track if the game is over

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    public void LoseLife()
    {
        if (isGameOver)
            return;

        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        UpdateUI();
    }

    public bool CanAfford(int amount)
    {
        return currency >= amount;
    }

    public void SpendCurrency(int amount)
    {
        currency -= amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        livesText.text = "Lives: " + lives;
        currencyText.text = "Currency: " + currency;
    }

    public void WinGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        winPanel.SetActive(true);
        Time.timeScale = 0; // Pause the game (the tower's shooting, the projectiles and more)
    }

    void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0; // Pause the game (the tower's shooting, the projectiles and more)
    }


    // Method to check if a position is occupied, with float tolerance
    public bool IsCellOccupied(Vector2 position)
    {
        return occupiedCells.ContainsKey(position);
    }

    // Method to mark a cell as occupied by a tower
    public void OccupyCell(Vector2 position, Tower tower)
    {
        occupiedCells[position] = tower;
    }

    // Optionally, method to free a cell when a tower is removed (if necessary)
    public void FreeCell(Vector2 position)
    {
        occupiedCells.Remove(position);
    }

    // Method to retrieve the tower at a specific position
    public Tower GetTowerAtPosition(Vector2 position)
    {
        if (occupiedCells.ContainsKey(position))
        {
            return occupiedCells[position];
        }
        return null;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1; // Ensure the game is not paused
        SceneManager.LoadScene("MainMenu");
    }
}
