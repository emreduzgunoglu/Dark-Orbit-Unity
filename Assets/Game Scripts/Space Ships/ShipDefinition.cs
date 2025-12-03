using UnityEngine;

[CreateAssetMenu(fileName = "ShipDefinition", menuName = "Game/Ship Definition")]
public class ShipDefinition : ScriptableObject
{
    [Header("Identity")]
    public string shipId;        // Örn: "star_sparrow_1"
    public string displayName;   // Örn: "Star Sparrow"

    [Header("Prefab")]
    public GameObject shipPrefab; // 3D uzay gemisi prefab

    [Header("Base Stats")]
    [Tooltip("Sağ-sol hareket hızı / manevra kabiliyeti")]
    public float baseHorizontalManeuver = 5f;

    [Tooltip("Yukarı-aşağı hareket hızı / manevra kabiliyeti")]
    public float baseVerticalManeuver = 3f;

    [Tooltip("Başlangıç mermi kapasitesi")]
    public int baseAmmo = 20;

    [Header("Max Upgrade Levels")]
    public int maxHorizontalManeuverLevel = 10;
    public int maxVerticalManeuverLevel = 10;
    public int maxAmmoLevel = 10;

    [Header("Per Level Increments")]
    public float horizontalManeuverPerLevel = 0.5f;
    public float verticalManeuverPerLevel = 0.3f;
    public int ammoPerLevel = 2;
}
