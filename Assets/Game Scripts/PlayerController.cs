using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Speed Settings (upgrade'lenebilir)")]
    public float moveSpeed = 5f;       // yatay
    public float verticalSpeed = 3f;   // dikey

    [Header("Joystick Settings")]
    public DynamicJoystick dynamicJoystick;
    public float joystickDeadZone = 0.1f;

    [Header("Shooting Settings")]
    public ObjectPool bulletPool;
    public ObjectPool explosionPool;
    public Transform firePoint;
    public int maxAmmo = 20;
    private int currentAmmo;
    public TMP_Text ammoText;

    [Header("Boundary Warning")]
    public float warningDistance = 1.5f; // minY/maxY'ye yaklaşınca uyarı mesafesi

    // level'dan gelen sınırlar
    private float minX, maxX, minY, maxY;

    // level referansını saklayalım ki her frame hesap yapabilelim
    private LevelData currentLevel;

    // 🔹 boundary UI cache – gereksiz UI update'lerini azaltır
    private float lastBoundaryIntensity = -1f;
    private string lastBoundaryMessage = "";

    void Start()
    {
        RefreshStatsFromShipManager();
        UpdateAmmoUI();
    }

    public void RefreshStatsFromShipManager()
    {
        ApplyShipStats();
        UpdateAmmoUI();
    }

    // === YENİ: ShipManager → hız + mermi kapasitesi ===
    private void ApplyShipStats()
    {
        if (ShipManager.Instance == null || ShipManager.Instance.currentShipDefinition == null)
        {
            // ShipManager yoksa, inspector'daki default değerlerle devam et
            currentAmmo = maxAmmo;
            return;
        }

        var sm = ShipManager.Instance;

        // Sağ-sol manevra → moveSpeed
        moveSpeed = sm.GetHorizontalManeuver();

        // Yukarı-aşağı manevra → verticalSpeed
        verticalSpeed = sm.GetVerticalManeuver();

        // Mermi kapasitesi
        maxAmmo = sm.GetMaxAmmo();

        // Başlangıç mermi sayısı
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (currentLevel == null)
        {
            // level daha yüklenmemişse sadece joystick hareketini engelle
            return;
        }

        Vector3 pos = transform.position;

        // önce Y'den t hesapla
        float t = Mathf.InverseLerp(currentLevel.minY, currentLevel.maxY, pos.y);
        t = Mathf.Clamp01(t);

        // GRAVITY hesapla (tek çizgi)
        float centered = (t - 0.5f) * 2f; // -1..+1
        float currentGravity = currentLevel.baseGravity - centered * currentLevel.gravityRange;
        if (currentGravity < 0f) currentGravity = 0f;

        // yer çekimini önce uygula
        pos.y -= currentGravity * Time.deltaTime;

        // joystick hareketi
        if (dynamicJoystick != null && dynamicJoystick.IsActive)
        {
            Vector2 input = dynamicJoystick.GetInput();
            if (input.magnitude < joystickDeadZone)
                input = Vector2.zero;

            pos.x += input.x * moveSpeed * Time.deltaTime;
            pos.y += input.y * verticalSpeed * Time.deltaTime;
        }

        // sınırla (level alanı)
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;

        // === minY / maxY sınırına değdi mi? ===
        if (GameManager.Instance != null)
        {
            // küçük bir tolerans
            float epsilon = 0.001f;
            bool hitMin = pos.y <= minY + epsilon;
            bool hitMax = pos.y >= maxY - epsilon;

            if (hitMin || hitMax)
            {
                // Direkt game over
                GameManager.Instance.ForceGameOver();
                return;
            }

            // === sınır uyarı şiddeti hesapla ===
            float distToMin = Mathf.Abs(pos.y - minY);
            float distToMax = Mathf.Abs(maxY - pos.y);
            float edgeDistance = Mathf.Min(distToMin, distToMax); // en yakın kenar

            float intensity = 0f;
            if (edgeDistance < warningDistance && warningDistance > 0f)
            {
                // kenara yaklaştıkça 1'e yaklaşan bir değer
                intensity = 1f - (edgeDistance / warningDistance);
            }

            // Yön mesajı
            string msg = "";

            if (distToMin < distToMax)
            {
                // Aşağı sınır daha yakın → oyuncu fazla alçaldı
                msg = "PULL UP!";
            }
            else
            {
                // Yukarı sınır daha yakın → oyuncu fazla yükseldi
                msg = "TOO HIGH!"; // veya "PULL DOWN!"
            }

            if (intensity <= 0f)
                msg = ""; // Uzaksa mesaj yok

            // 🔹 Sadece DEĞİŞTİĞİNDE GameManager'a gönder
            if (Mathf.Abs(intensity - lastBoundaryIntensity) > 0.01f)
            {
                GameManager.Instance.SetBoundaryWarning(intensity);
                lastBoundaryIntensity = intensity;
            }

            if (msg != lastBoundaryMessage)
            {
                GameManager.Instance.SetBoundaryDirection(msg);
                lastBoundaryMessage = msg;
            }
        }

        // METEOR hızını da aynı t'den türet ve global yolla
        float meteorMul = 1f + (1f - t) * currentLevel.meteorSpeedFactor;
        float finalMeteorSpeed = currentLevel.baseMeteorSpeed * meteorMul;
        Meteor.SetGlobalSpeed(finalMeteorSpeed);

        // LEVEL SÜRE HIZI (0.5x - 1.5x arası)
        float maxFactor = 1.5f; // player en alttayken
        float minFactor = 0.5f; // player en üstteyken

        // t: 0 = en alt, 0.5 = orta, 1 = en üst
        float timeScale = Mathf.Lerp(maxFactor, minFactor, t); // t=0 ->1.5, t=0.5->1.0, t=1->0.5

        // 1.0, 1.1, 1.2 gibi basamaklı olsun
        timeScale = Mathf.Round(timeScale * 10f) / 10f;

        // GameManager'a gönder
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevelTimeScale(timeScale);
        }
    }

    // LevelManager burayı çağıracak
    public void ApplyLevelData(LevelData data)
    {
        currentLevel = data;

        // sınırları direkt al
        minX = data.minX;
        maxX = data.maxX;
        minY = data.minY;
        maxY = data.maxY;

        // boundary cache reset
        lastBoundaryIntensity = -1f;
        lastBoundaryMessage = "";

        // player'ın hızı level'in istediğinden düşükse istersen yükselt
        // (ShipManager'dan gelen değeri düşürmüyoruz, sadece minimumu garanti ediyoruz)
        moveSpeed = Mathf.Max(moveSpeed, data.requiredHorizontalSpeed);
        verticalSpeed = Mathf.Max(verticalSpeed, data.requiredVerticalSpeed);
    }

    public void Fire()
    {
        if (currentAmmo <= 0) return;
        if (bulletPool == null) return;

        GameObject bullet = bulletPool.GetFromPool();

        Vector3 spawnPos = transform.position;
        if (firePoint != null)
            spawnPos = firePoint.position;

        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.identity;

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.SetPool(bulletPool);
            b.explosionPool = explosionPool;
            b.ResetStartPos();
        }

        currentAmmo--;
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = currentAmmo.ToString();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        float z = transform.position.z;

        Vector3 p1 = new Vector3(minX, minY, z);
        Vector3 p2 = new Vector3(minX, maxY, z);
        Vector3 p3 = new Vector3(maxX, maxY, z);
        Vector3 p4 = new Vector3(maxX, minY, z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}
