using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode
{
    Classic,    // 3 lives, miss 3 fruits = game over
    Arcade,     // 60 second timer, no lives
    Zen         // 90 seconds, no bombs, relaxed
}

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Blade blade;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Image fadeImage;

    [Header("UI - Gameplay")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text comboText;
    [SerializeField] private GameObject gameplayUI;

    [Header("UI - Game Over")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalHighScoreText;
    [SerializeField] private Text newHighScoreText;

    [Header("UI - Main Menu")]
    [SerializeField] private GameObject mainMenuUI;

    [Header("Game Settings")]
    [SerializeField] private GameMode gameMode = GameMode.Classic;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float arcadeTime = 60f;
    [SerializeField] private float zenTime = 90f;

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeWindow = 0.5f;
    [SerializeField] private int comboMultiplierThreshold = 3;

    // Properties
    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;
    public int highScore { get; private set; } = 0;
    public GameMode currentGameMode => gameMode;
    public bool isPlaying { get; private set; } = false;

    // Combo tracking
    private int currentCombo = 0;
    private float lastSliceTime = 0f;
    private Coroutine comboResetCoroutine;

    // Timer
    private float gameTimer = 0f;
    private bool timerActive = false;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("highscore", 0);
        ShowMainMenu();
    }

    private void Update()
    {
        // Update timer for Arcade/Zen modes
        if (timerActive && isPlaying)
        {
            gameTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (gameTimer <= 0)
            {
                gameTimer = 0;
                GameOver();
            }
        }

        // Check for combo timeout
        if (currentCombo > 0 && Time.time - lastSliceTime > comboTimeWindow)
        {
            ResetCombo();
        }
    }

    #region Menu & Game State

    public void ShowMainMenu()
    {
        isPlaying = false;
        Time.timeScale = 1f;

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        blade.enabled = false;
        spawner.enabled = false;

        ClearScene();
    }

    public void StartGame(GameMode mode)
    {
        gameMode = mode;
        NewGame();
    }

    public void StartClassicMode() => StartGame(GameMode.Classic);
    public void StartArcadeMode() => StartGame(GameMode.Arcade);
    public void StartZenMode() => StartGame(GameMode.Zen);

    private void NewGame()
    {
        isPlaying = true;
        Time.timeScale = 1f;

        // Hide/show UI
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        ClearScene();

        // Reset game state
        score = 0;
        currentCombo = 0;
        lives = maxLives;

        // Setup timer based on game mode
        switch (gameMode)
        {
            case GameMode.Classic:
                timerActive = false;
                break;
            case GameMode.Arcade:
                gameTimer = arcadeTime;
                timerActive = true;
                break;
            case GameMode.Zen:
                gameTimer = zenTime;
                timerActive = true;
                // Disable bombs in Zen mode
                spawner.bombChance = 0f;
                break;
        }

        // Update spawner for game mode
        if (gameMode != GameMode.Zen)
        {
            spawner.bombChance = 0.05f;
        }

        // Enable gameplay
        blade.enabled = true;
        spawner.enabled = true;

        // Update UI
        UpdateScoreUI();
        UpdateLivesUI();
        UpdateTimerUI();
        UpdateHighScoreUI();
        HideComboText();

        // Fade in
        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
        }
    }

    public void GameOver()
    {
        isPlaying = false;
        blade.enabled = false;
        spawner.enabled = false;

        // Check for new high score
        bool newHighScore = false;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highscore", highScore);
            PlayerPrefs.Save();
            newHighScore = true;
        }

        StartCoroutine(GameOverSequence(newHighScore));
    }

    private IEnumerator GameOverSequence(bool newHighScore)
    {
        float elapsed = 0f;
        float duration = 0.5f;

        // Fade to dark
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            if (fadeImage != null)
            {
                fadeImage.color = Color.Lerp(Color.clear, new Color(0, 0, 0, 0.8f), t);
            }
            Time.timeScale = 1f - (t * 0.5f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 0.5f;

        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = "Score: " + score;
            if (finalHighScoreText != null) finalHighScoreText.text = "Best: " + highScore;
            if (newHighScoreText != null) newHighScoreText.SetActive(newHighScore);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (fadeImage != null) fadeImage.color = Color.clear;
        NewGame();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (fadeImage != null) fadeImage.color = Color.clear;
        ShowMainMenu();
    }

    #endregion

    #region Scoring & Combo

    public void IncreaseScore(int points)
    {
        // Apply combo multiplier
        int comboBonus = 0;
        currentCombo++;
        lastSliceTime = Time.time;

        if (currentCombo >= comboMultiplierThreshold)
        {
            comboBonus = currentCombo - 1; // Bonus points for combo
            ShowComboText(currentCombo);
        }

        int totalPoints = points + comboBonus;
        score += totalPoints;

        UpdateScoreUI();

        // Check high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highscore", highScore);
            UpdateHighScoreUI();
        }
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        HideComboText();
    }

    public void OnFruitMissed()
    {
        if (gameMode != GameMode.Classic) return;
        if (!isPlaying) return;

        lives--;
        UpdateLivesUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    // Called when blade stops slicing
    public void OnSliceEnd()
    {
        // Combo continues if within time window
        // Will be reset by Update() if time expires
    }

    #endregion

    #region UI Updates

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + highScore;
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            if (gameMode == GameMode.Classic)
            {
                livesText.gameObject.SetActive(true);
                string livesDisplay = "";
                for (int i = 0; i < lives; i++)
                {
                    livesDisplay += "X ";
                }
                livesText.text = livesDisplay.Trim();
            }
            else
            {
                livesText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (timerActive)
            {
                timerText.gameObject.SetActive(true);
                int seconds = Mathf.CeilToInt(gameTimer);
                timerText.text = seconds.ToString();
            }
            else
            {
                timerText.gameObject.SetActive(false);
            }
        }
    }

    private void ShowComboText(int combo)
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = combo + "x COMBO!";

            // Scale animation effect
            comboText.transform.localScale = Vector3.one * 1.5f;
            StartCoroutine(AnimateComboText());
        }
    }

    private IEnumerator AnimateComboText()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if (comboText != null)
            {
                comboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void HideComboText()
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Helpers

    private void ClearScene()
    {
        Fruit[] fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        foreach (Fruit fruit in fruits)
        {
            Destroy(fruit.gameObject);
        }

        Bomb[] bombs = FindObjectsByType<Bomb>(FindObjectsSortMode.None);
        foreach (Bomb bomb in bombs)
        {
            Destroy(bomb.gameObject);
        }
    }

    // Legacy method for bomb explosion (still works in Arcade mode)
    public void Explode()
    {
        if (gameMode == GameMode.Arcade)
        {
            // In Arcade mode, bomb just removes 10 points
            score = Mathf.Max(0, score - 10);
            UpdateScoreUI();
            ResetCombo();
        }
        else
        {
            // In Classic mode, bomb ends the game
            GameOver();
        }
    }

    #endregion
}
