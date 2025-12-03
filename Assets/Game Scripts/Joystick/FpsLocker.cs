using UnityEngine;

public class FpsLocker : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0;     // vsync kapalı
        Application.targetFrameRate = 120;  // 120 fps iste
    }
}
