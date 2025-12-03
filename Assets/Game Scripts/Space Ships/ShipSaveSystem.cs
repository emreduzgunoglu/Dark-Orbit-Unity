using UnityEngine;

public static class ShipSaveSystem
{
    private const string KEY = "ShipUpgradeState";

    public static void Save(ShipUpgradeState state)
    {
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static ShipUpgradeState Load(string defaultShipId)
    {
        if (!PlayerPrefs.HasKey(KEY))
        {
            // Oyuncu ilk kez giriyorsa, her şey 0 level olsun
            return new ShipUpgradeState
            {
                shipId = defaultShipId,
                horizontalManeuverLevel = 0,
                verticalManeuverLevel = 0,
                ammoLevel = 0
            };
        }

        string json = PlayerPrefs.GetString(KEY);
        return JsonUtility.FromJson<ShipUpgradeState>(json);
    }

}
