using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance;

    [Header("Ship Definitions")]
    [Tooltip("Oyunda kullanılabilen tüm gemiler (şimdilik 1 tane de olsa ekleyebilirsin).")]
    public ShipDefinition[] availableShips;

    [Tooltip("İlk açılışta kullanılacak default gemi.")]
    public ShipDefinition defaultShip;

    [Header("Runtime State (Read Only)")]
    public ShipDefinition currentShipDefinition; // Inspector'da görmek için public
    public ShipUpgradeState currentUpgradeState; // Inspector'da görmek için public

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeShip();
    }

    private void InitializeShip()
    {
        // Save'den oku (ilkse default gemi ile state oluşturur)
        currentUpgradeState = ShipSaveSystem.Load(defaultShip != null ? defaultShip.shipId : "");

        // ShipDefinition bul
        currentShipDefinition = FindShipDefinitionById(currentUpgradeState.shipId);

        // Eğer save'deki shipId hiçbir define ile eşleşmezse fallback
        if (currentShipDefinition == null)
        {
            currentShipDefinition = defaultShip;
            if (currentShipDefinition != null)
            {
                currentUpgradeState.shipId = currentShipDefinition.shipId;
            }
        }
    }

    private ShipDefinition FindShipDefinitionById(string id)
    {
        if (availableShips == null) return null;

        foreach (var def in availableShips)
        {
            if (def != null && def.shipId == id)
                return def;
        }
        return null;
    }

    #region Stat Hesaplama

    // Sağ-sol manevra (PlayerController'daki moveSpeed'e gidecek)
    public float GetHorizontalManeuver()
    {
        return currentShipDefinition.baseHorizontalManeuver +
               currentUpgradeState.horizontalManeuverLevel * currentShipDefinition.horizontalManeuverPerLevel;
    }

    // Yukarı-aşağı manevra (PlayerController'daki verticalSpeed'e gidecek)
    public float GetVerticalManeuver()
    {
        return currentShipDefinition.baseVerticalManeuver +
               currentUpgradeState.verticalManeuverLevel * currentShipDefinition.verticalManeuverPerLevel;
    }

    // Mermi kapasitesi
    public int GetMaxAmmo()
    {
        return currentShipDefinition.baseAmmo +
               currentUpgradeState.ammoLevel * currentShipDefinition.ammoPerLevel;
    }

    #endregion

    #region Save Yardımcı

    public void SaveCurrentState()
    {
        ShipSaveSystem.Save(currentUpgradeState);
    }

    #endregion
}
