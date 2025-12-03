using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition; // parent’a göre yerini kaydet
    }

    public void Shake(float duration = 0.2f, float magnitude = 0.2f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // parent’ın etrafında sallıyoruz
            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // bittiğinde tam eski local yerine dön
        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
