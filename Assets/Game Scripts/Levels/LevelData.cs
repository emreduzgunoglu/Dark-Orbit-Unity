using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";

    [Header("Background")]
    public Sprite backgroundSprite;

    [Tooltip("Temel level süresi (saniye)")]
    public float levelDuration = 30f;

    [Header("Playable Area")]
    [Tooltip("Player'ın hareket edebileceği ve meteorların spawn olacağı min X")]
    public float minX = -4f;
    public float maxX = 4f;
    public float minY = -3f;
    public float maxY = 3f;

    [Header("Gravity (Y'den hesaplanacak)")]
    [Tooltip("Player tam ortadaysa uygulanacak yer çekimi")]
    public float baseGravity = 0.5f;

    [Tooltip("En alta inince baseGravity + gravityRange olacak, en üste çıkınca baseGravity - gravityRange olacak")]
    public float gravityRange = 0.2f;

    [Header("Player Movement Requirements")]
    [Tooltip("Bu levelde rahat oynamak için player'ın dikey hızı / itişi")]
    public float requiredVerticalSpeed = 3f;

    [Tooltip("Bu levelde rahat oynamak için player'ın yatay hızı")]
    public float requiredHorizontalSpeed = 5f;

    [Header("Meteor Spawning")]
    public float spawnInterval = 1.2f;

    [Tooltip("Orta seviyede meteorların gideceği hız")]
    public float baseMeteorSpeed = 8f;

    [Tooltip("Player aşağı indikçe meteor hızına eklenecek çarpan miktarı. 0.2 küçük, 2 agresif değişim demek.")]
    public float meteorSpeedFactor = 0.5f;

    public GameObject[] meteorPrefabs;

    [Header("Visuals")]
    public Color backgroundColor = Color.black;
}
