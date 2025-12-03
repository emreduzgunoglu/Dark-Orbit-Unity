using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Default Settings")]
    [Tooltip("İstersen varsayılan tek bir prefab için önceden havuz oluşturabilirsin.")]
    public GameObject prefab;      // default prefab (opsiyonel)
    public int initialSize = 10;   // default prefab için başlangıç sayısı

    // Her prefab için ayrı kuyruk
    private readonly Dictionary<GameObject, Queue<GameObject>> prefabToPool
        = new Dictionary<GameObject, Queue<GameObject>>();

    // Her instance'ın hangi kuyruğa ait olduğunu hatırlamak için
    private readonly Dictionary<GameObject, Queue<GameObject>> instanceToPool
        = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        // Eski sistem: tek bir prefab için prewarm
        if (prefab != null && initialSize > 0)
        {
            var q = new Queue<GameObject>();
            prefabToPool[prefab] = q;

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                q.Enqueue(obj);
                instanceToPool[obj] = q;
            }
        }
    }

    /// <summary>
    /// Eski API – default prefab üzerinden kullanılır.
    /// </summary>
    public GameObject GetFromPool()
    {
        return GetFromPool(prefab);
    }

    /// <summary>
    /// Yeni API – verilen prefab için ayrı bir havuz kullanır.
    /// LevelData’dan gelen herhangi bir prefab için çağrılabilir.
    /// </summary>
    public GameObject GetFromPool(GameObject prefabOverride)
    {
        if (prefabOverride == null)
        {
            Debug.LogWarning($"{name} ObjectPool: GetFromPool(null) çağrıldı.");
            return null;
        }

        if (!prefabToPool.TryGetValue(prefabOverride, out Queue<GameObject> queue))
        {
            // Bu prefab için daha önce hiç havuz yoksa yeni oluştur
            queue = new Queue<GameObject>();
            prefabToPool[prefabOverride] = queue;
        }

        GameObject obj;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            obj = Instantiate(prefabOverride, transform);
        }

        obj.SetActive(true);
        instanceToPool[obj] = queue;
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);

        if (instanceToPool.TryGetValue(obj, out Queue<GameObject> queue))
        {
            queue.Enqueue(obj);
        }
        else
        {
            // Bu objeyi biz üretmemişsek, kaba fallback:
            // default prefab havuzuna at ya da yok et.
            if (prefab != null && prefabToPool.TryGetValue(prefab, out Queue<GameObject> defaultQueue))
            {
                defaultQueue.Enqueue(obj);
                instanceToPool[obj] = defaultQueue;
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
