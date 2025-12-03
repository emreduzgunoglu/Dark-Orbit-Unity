using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Levels")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;

    [Header("Scene Refs")]
    public MeteorSpawner meteorSpawner;
    public PlayerController playerController;
    public Camera mainCam;

    private LevelData currentLevel;
    public LevelBackgroundController levelBackground;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (levels != null && levels.Length > 0)
        {
            LoadLevel(currentLevelIndex);
        }
    }

    public LevelData GetCurrentLevel()
    {
        return currentLevel;
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Length == 0) return;
        if (index < 0 || index >= levels.Length) index = 0;

        currentLevelIndex = index;
        currentLevel = levels[index];

        // 1) Meteor ayarları
        if (meteorSpawner != null)
        {
            // spawn süresi senin eskisiyle aynı
            meteorSpawner.spawnInterval = currentLevel.spawnInterval;

            // spawn alanını level alanına göre ayarla
            meteorSpawner.ApplyLevelArea(currentLevel);

            // prefab override
            if (currentLevel.meteorPrefabs != null && currentLevel.meteorPrefabs.Length > 0)
                meteorSpawner.SetCustomPrefabs(currentLevel.meteorPrefabs);
            else
                meteorSpawner.SetCustomPrefabs(null);
        }

        // 2) Player'a sınırları ve gereksinimleri aktar
        if (playerController != null)
        {
            playerController.ApplyLevelData(currentLevel);
        }

        // 3) Kamera rengi
        if (mainCam != null)
        {
            mainCam.backgroundColor = currentLevel.backgroundColor;
        }

        // 4) LEVEL BACKGROUND SPRITE
        if (levelBackground != null)
        {
            levelBackground.ApplyBackground(currentLevel);
        }

         // 5) Level süresini GameManager'a bildir
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevelTimer(currentLevel);
        }
    }

    public void LoadCurrentLevelAgain()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        int next = currentLevelIndex + 1;
        if (next >= levels.Length) next = 0;
        LoadLevel(next);
    }
}
