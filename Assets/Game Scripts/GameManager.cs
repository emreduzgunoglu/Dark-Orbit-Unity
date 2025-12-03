using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI - Panels")]
    public GameObject welcomePanel;   // ilk ekran
    public GameObject gamePanel;      // oyun sırasında görünen panel (score, joystick, fire)
    public GameObject pausePanel;     // pause olunca
    public GameObject gameOverPanel;  // oyun bitince

    [Header("UI - Level Passed")]
    public GameObject levelPassedPanel;   // Level Passed Panel
    public TMP_Text levelPassedText;      // "Level 1 Passed." yazacak yer

    [Header("UI - Texts")]
    public TMP_Text scoreText;
    public TMP_Text livesText;

    [Header("UI - Level")]
    public Image levelProgressBar;        // dolan çizgi (opsiyonel)

    private float levelElapsed = 0f;      // kaç saniyelik progress oldu
    private float levelDuration = 0f;     // LevelData.levelDuration
    private float levelTimeScale = 1f;    // Player'dan gelen 0.7 .. 1.3
    private bool levelTimerRunning = false;

    [Header("Gameplay")]
    public int startLives = 3;

    private int currentScore = 0;
    private int currentLives = 0;

    [Header("UI - Boundary Warning")]
    public CanvasGroup boundaryWarningCanvas;
    public TMP_Text boundaryDirectionText;   // Yeni eklenen yön yazısı
    public float warningBlinkSpeed = 4f;

    private float warningIntensity = 0f;
    private string currentBoundaryMessage = "";

    private enum GameState
    {
        Welcome,
        Playing,
        Paused,
        GameOver
    }

    private GameState currentState = GameState.Welcome;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ResetGame();
        ShowWelcome();
    }

    void Update()
    {
        // Sadece oynarken timer ilerlesin
        if (currentState == GameState.Playing)
        {
            if (levelTimerRunning && levelDuration > 0f)
            {
                // timeScale'e göre zamanı ilerlet
                levelElapsed += Time.deltaTime * levelTimeScale;
                float progress = Mathf.Clamp01(levelElapsed / levelDuration);

                if (levelProgressBar != null)
                    levelProgressBar.fillAmount = progress;

                if (progress >= 1f)
                {
                    OnLevelPassed();
                }
            }
        }

        // Her durumda boundary uyarı UI'ını güncelle
        UpdateBoundaryWarningUI();
    }

    // ---------------- STATE UI ----------------

    private void ShowWelcome()
    {
        currentState = GameState.Welcome;
        Time.timeScale = 0f;

        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(true);
    }

    private void ShowGameUI()
    {
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void ShowPause()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false); // oyun UI’ını gizle
    }

    private void ShowGameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // ---------------- BUTTON METHODS ----------------

    public void OnLevelPassed()
    {
        levelTimerRunning = false;
        // Oyunu durdur
        Time.timeScale = 0f;

        // Paneli aç
        if (levelPassedPanel != null)
            levelPassedPanel.SetActive(true);

        // Level ismini yaz
        if (levelPassedText != null && LevelManager.Instance != null)
        {
            int levelNumber = LevelManager.Instance.currentLevelIndex + 1; // 0 tabanlı olduğu için +1
            levelPassedText.text = $"Level {levelNumber} Passed.";
        }
    }

    public void OnNextLevelButton()
    {
        // Paneli kapat
        if (levelPassedPanel != null)
            levelPassedPanel.SetActive(false);

        // Oyunu tekrar başlat
        Time.timeScale = 1f;

        // Sonraki leveli yükle
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void OnPlayButton()
    {
        UIManager.Instance.HideShipPreview();
        // her play’de sıfırdan başla
        ResetGame();

        currentState = GameState.Playing;
        Time.timeScale = 1f;
        ShowGameUI();

        // level sistemi varsa ilk level'i yükle
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(0);
    }

    public void OnPauseButton()
    {
        if (currentState != GameState.Playing) return;
        ShowPause();
    }

    public void OnResumeButton()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
    }

    // sahneyi tamamen sıfırdan yüklemek istersen bunu kullan
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🔹 yeni: main menu'ye dön (sahneyi reload ETMEDEN)
    public void OnBackToMainMenuButton()
    {
        
        // değerleri başa çek
        ResetGame();

        // UI'ı welcome'a çek
        ShowWelcome();

        // level sistemi varsa yine level 0'a dön
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(0);

        UIManager.Instance.ShowShipPreview();

        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
        {
            preview.SetFocus(ShipFocusType.Ammo);
        }
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    // LevelManager yeni level yüklediğinde burayı çağıracak
    public void StartLevelTimer(LevelData level)
    {
        if (level == null) return;

        levelDuration = level.levelDuration;
        levelElapsed = 0f;
        levelTimeScale = 1f;
        levelTimerRunning = true;

        if (levelProgressBar != null)
            levelProgressBar.fillAmount = 0f;
    }

    // PlayerController burayı her frame güncelliyor
    public void SetLevelTimeScale(float factor)
    {
        levelTimeScale = factor;
    }

    // ---------------- GAMEPLAY EVENTS ----------------

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    public void PlayerHit()
    {
        if (currentState == GameState.GameOver) return;

        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            ShowGameOver();
        }
    }

    private void UpdateBoundaryWarningUI()
    {
        if (boundaryWarningCanvas == null)
            return;

        if (currentState != GameState.Playing || warningIntensity <= 0f)
        {
            boundaryWarningCanvas.alpha = 0f;

            if (boundaryDirectionText != null)
                boundaryDirectionText.text = "";

            return;
        }

        float blink = Mathf.Sin(Time.unscaledTime * warningBlinkSpeed) * 0.5f + 0.5f;

        boundaryWarningCanvas.alpha = warningIntensity * blink;

        if (boundaryDirectionText != null)
            boundaryDirectionText.alpha = 1f;
    }

    public void SetBoundaryWarning(float intensity)
    {
        warningIntensity = Mathf.Clamp01(intensity);
    }

    public void ForceGameOver()
    {
        // Sadece oyun oynanıyorken ölmek mümkün olsun
        if (currentState != GameState.Playing)
            return;

        ShowGameOver();
    }

    // ---------------- HELPERS ----------------

    private void ResetGame()
    {
        currentScore = 0;
        currentLives = startLives;
        UpdateScoreUI();
        UpdateLivesUI();

        // level timer reset
        levelElapsed = 0f;
        levelDuration = 0f;
        levelTimeScale = 1f;
        levelTimerRunning = false;

        if (levelProgressBar != null)
            levelProgressBar.fillAmount = 0f;

        warningIntensity = 0f;
        if (boundaryWarningCanvas != null)
            boundaryWarningCanvas.alpha = 0f;
        if (boundaryDirectionText != null)
            boundaryDirectionText.text = "";
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + currentLives;
    }

    public void SetBoundaryDirection(string msg)
    {
        currentBoundaryMessage = msg;

        if (boundaryDirectionText != null)
            boundaryDirectionText.text = msg;
    }

}
