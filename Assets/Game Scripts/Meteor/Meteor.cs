using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 8f;        // Eski alan (şart değil ama bozmayalım)
    public float speedMultiplier = 1f;  // Prefab bazlı hız katsayısı (küçük hızlı / büyük yavaş)

    [Header("Lifetime")]
    public float destroyDistance = 25f;

    private ObjectPool pool;
    public ObjectPool playerHitExplosionPool;
    private Vector3 spawnPosition;

    // PlayerController'dan gelen global hız
    private static float globalSpeed = 8f;

    // Spawner’ın atadığı yön
    private Vector3 moveDirection = Vector3.back;

    // 🔹 Cache
    private Transform tr;
    private float destroyDistanceSqr;

    public static void SetGlobalSpeed(float s) => globalSpeed = s;
    public void SetPool(ObjectPool p) => pool = p;

    void Awake()
    {
        tr = transform;
        destroyDistanceSqr = destroyDistance * destroyDistance;
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        spawnPosition = pos;
    }

    public void SetDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            moveDirection = dir.normalized;
        else
            moveDirection = Vector3.back;
    }

    void Update()
    {
        float finalSpeed = globalSpeed * speedMultiplier;

        // transform yerine cache'lenmiş tr kullan
        tr.Translate(moveDirection * finalSpeed * Time.deltaTime, Space.World);

        // Vector3.Distance yerine sqrMagnitude → daha ucuz
        Vector3 diff = tr.position - spawnPosition;
        if (diff.sqrMagnitude >= destroyDistanceSqr)
        {
            ReturnToPool();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        bool hitPlayer = collision.collider.CompareTag("Player");
        bool hitMeteor = collision.collider.CompareTag("Meteor");

        if (!hitPlayer && !hitMeteor)
            return;

        // Patlama efekti
        if (playerHitExplosionPool != null)
        {
            GameObject exp = playerHitExplosionPool.GetFromPool();
            if (exp != null)
            {
                Vector3 hitPoint = collision.contacts.Length > 0
                    ? collision.contacts[0].point
                    : tr.position;

                exp.transform.position = hitPoint;
                exp.transform.rotation = Quaternion.identity;

                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.25f, 0.25f);

                StartCoroutine(ReturnExplosionAfterDelay(exp, 1f));
            }
        }

        if (hitPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHit();
        }

        // Burada 0.01f delay ile Invoke çağırmak yerine
        // direkt pool'a geri dönmek daha ucuz ve davranış olarak aynı.
        ReturnToPool();
    }

    private System.Collections.IEnumerator ReturnExplosionAfterDelay(GameObject exp, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerHitExplosionPool != null)
            playerHitExplosionPool.ReturnToPool(exp);
        else
            exp.SetActive(false);
    }

    // Eski API – başka yerler kullanıyorsa bozulmasın
    public void SetSpeed(float s)
    {
        moveSpeed = s;
    }

    public void SetDestroyDistance(float d)
    {
        destroyDistance = d;
        destroyDistanceSqr = d * d;
    }

    public void ReturnToPool()
    {
        if (pool != null)
            pool.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }
}
