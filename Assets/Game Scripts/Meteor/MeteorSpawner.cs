using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Pools")]
    public ObjectPool meteorPool;              // Çoklu prefab havuzu
    public ObjectPool playerHitExplosionPool;  // Patlama efekti havuzu

    // LevelData.meteorPrefabs buraya set edilecek
    private GameObject[] meteorPrefabs = null;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.2f;
    public float spawnRangeX = 2.5f;
    public float spawnRangeY = 1.5f;
    public float currentMeteorSpeed = 8f;

    [Header("Meteor Lifetime")]
    public float meteorDestroyDistance = 25f;

    [Header("Meteor Direction")]
    [Tooltip("Sağa/sola maksimum sapma açısı (derece). Küçük değer = daha düz meteorlar.")]
    public float maxHorizontalAngle = 8f;

    [Tooltip("Aşağı/yukarı tilt için maksimum açı (derece). Çok küçük tut (ör. 3–5).")]
    public float maxDownAngle = 4f;

    private float timer;
    private float levelMinY, levelMaxY;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMeteor();
            timer = 0f;
        }
    }

    // LevelManager buradan çağırıyor
    public void ApplyLevelArea(LevelData data)
    {
        if (data == null) return;

        // Spawner'ı level alanının ortasına koy
        float centerX = (data.minX + data.maxX) * 0.5f;
        float centerY = (data.minY + data.maxY) * 0.5f;
        transform.position = new Vector3(centerX, centerY, transform.position.z);

        // Spawn alanını level boyutuna göre ayarla
        float extra = 2f;
        spawnRangeX = (data.maxX - data.minX) * 0.5f + extra;
        spawnRangeY = (data.maxY - data.minY) * 0.5f + extra;

        // Level’dan meteor tempo ve temel hız
        spawnInterval = data.spawnInterval;
        currentMeteorSpeed = data.baseMeteorSpeed;

        levelMinY = data.minY;
        levelMaxY = data.maxY;

        // LevelData içindeki meteor prefab’larını kaydet
        meteorPrefabs = data.meteorPrefabs;
    }

    public void SetMeteorSpeed(float speed)
    {
        currentMeteorSpeed = speed;
    }

    /// <summary>
    /// Eski LevelManager kodunda kullanıyorsan dursun.
    /// </summary>
    public void SetCustomPrefabs(GameObject[] prefabs)
    {
        meteorPrefabs = prefabs;
    }

    void SpawnMeteor()
    {
        if (meteorPool == null) return;
        if (meteorPrefabs == null || meteorPrefabs.Length == 0) return;

        // 1) Hangi prefab'ı kullanacağımızı LevelData’dan seç
        GameObject chosenPrefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];
        if (chosenPrefab == null) return;

        // 2) Spawn pozisyonu
        Vector3 pos = transform.position;
        pos.x += Random.Range(-spawnRangeX, spawnRangeX);
        pos.y += Random.Range(-spawnRangeY, spawnRangeY);

        // 3) Pool’dan direkt SEÇİLEN PREFAB’a göre meteor al
        GameObject meteor = meteorPool.GetFromPool(chosenPrefab);
        if (meteor == null) return;

        meteor.transform.position = pos;
        meteor.transform.rotation = Quaternion.identity;

        // 4) Meteor script ayarları
        Meteor m = meteor.GetComponent<Meteor>();
        if (m != null)
        {
            m.SetPool(meteorPool);
            m.SetSpeed(currentMeteorSpeed);         // globalSpeed hesabı PlayerController’dan
            m.playerHitExplosionPool = playerHitExplosionPool;
            m.SetSpawnPosition(pos);
            m.SetDestroyDistance(meteorDestroyDistance);

            // YÖN: hem aşağı hem sağ/sol random
            Vector3 baseDir = Vector3.back;        // oyuncuya doğru

            float yaw   = Random.Range(-maxHorizontalAngle, maxHorizontalAngle); // sağ/sol
            float pitch = Random.Range(-maxDownAngle,       maxDownAngle);       // aşağı/yukarı

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = rot * baseDir;

            // Yukarı gitmesin; z mutlaka negatif olsun
            if (dir.z > -0.1f)
                dir.z = -0.1f;

            dir.Normalize();
            m.SetDirection(dir);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        float z = transform.position.z;
        Vector3 center = transform.position;

        Vector3 p1 = new Vector3(center.x - spawnRangeX, center.y - spawnRangeY, z);
        Vector3 p2 = new Vector3(center.x - spawnRangeX, center.y + spawnRangeY, z);
        Vector3 p3 = new Vector3(center.x + spawnRangeX, center.y + spawnRangeY, z);
        Vector3 p4 = new Vector3(center.x + spawnRangeX, center.y - spawnRangeY, z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}
