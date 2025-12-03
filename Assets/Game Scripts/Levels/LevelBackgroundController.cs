using UnityEngine;

public class LevelBackgroundController : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;

    // LevelData geldiğinde çağıracağımız fonksiyon
    public void ApplyBackground(LevelData data)
    {
        if (data == null || backgroundRenderer == null) return;

        if (data.backgroundSprite != null)
        {
            backgroundRenderer.sprite = data.backgroundSprite;
        }
    }
}
