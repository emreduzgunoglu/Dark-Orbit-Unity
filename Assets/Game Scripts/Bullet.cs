using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float maxTravel = 20f;

    private ObjectPool pool;
    private Vector3 startPos;

    public ObjectPool explosionPool;

    public void SetPool(ObjectPool p)
    {
        pool = p;
    }

    void OnEnable()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // +Z'ye git
        transform.position += Vector3.forward * moveSpeed * Time.deltaTime;

        // 20 birim gittiyse geri dön
        if (Vector3.Distance(startPos, transform.position) >= maxTravel)
        {
            ReturnToPool();
        }
    }

    public void ResetStartPos()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Meteor meteor = other.GetComponent<Meteor>();
        if (meteor != null)
        {
            Debug.Log("mermi meteoru vurdu");
            GameManager.Instance.AddScore(10);

            // küçük patlama
            if (explosionPool != null)
            {
                GameObject exp = explosionPool.GetFromPool();
                exp.transform.position = transform.position;
                exp.transform.rotation = Quaternion.identity;
                StartCoroutine(ReturnExplosionAfterDelay(exp, 1f));
            }

            // 💥 micro camera shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.1f, 0.08f); // küçük süre, küçük şiddet
            }

            meteor.ReturnToPool();
            ReturnToPool();
        }
    }

    private System.Collections.IEnumerator ReturnExplosionAfterDelay(GameObject exp, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (explosionPool != null)
            explosionPool.ReturnToPool(exp);
        else
            exp.SetActive(false);
    }

    private void ReturnToPool()
    {
        if (pool != null)
            pool.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }
}
