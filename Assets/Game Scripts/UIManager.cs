using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Screens")]
    public CanvasGroup welcomeScreen;   // Home
    public CanvasGroup upgradeScreen;
    public CanvasGroup settingsScreen;  // varsa ayarlar ekranı

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f; // 0.5 sn

    [Header("Black Background")]
    public GameObject blackBackgroundPanel;

    [Header("Ship Preview Global")]
    public GameObject shipPreviewCanvas; // Canvas_ShipPreview
    public GameObject shipPreviewRoot;   // ShipPreviewRoot (kamera + ship controller)

    // ===== ÜST BUTTONLAR =====
    [Header("Top Buttons")]
    public RectTransform homeButton;
    public RectTransform upgradeButton;
    public RectTransform settingsButton;
    public float buttonAnimDuration = 0.2f;

    private Vector2 homeOriginalSize;
    private Vector2 upgradeOriginalSize;
    private Vector2 settingsOriginalSize;

    private Coroutine currentTransition;
    private Coroutine buttonAnimCoroutine;

    // Hangi ekran aktif?
    private enum ScreenType { Home, Upgrade, Settings }
    private ScreenType currentScreen = ScreenType.Home;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // istersen kaldırabilirsin

        // Buttonların orijinal boyutlarını kaydet
        if (homeButton != null) homeOriginalSize = homeButton.sizeDelta;
        if (upgradeButton != null) upgradeOriginalSize = upgradeButton.sizeDelta;
        if (settingsButton != null) settingsOriginalSize = settingsButton.sizeDelta;
    }

    private void Start()
    {
        ShowShipPreview(); // oyun açılır açılmaz preview görünsün (welcome ekranında)

        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
            preview.SetFocus(ShipFocusType.Ammo);

        // Başlangıç ekranı Home varsayıyoruz
        currentScreen = ScreenType.Home;
        AnimateButtonsForScreen(ScreenType.Home);
    }

    // =================== EKRAN GEÇİŞLERİ GENEL ===================

    private void SwitchScreen(ScreenType newScreen)
    {
        // Aynı ekrana yeniden tıklandıysa hiçbir şey yapma
        if (newScreen == currentScreen)
            return;

        CanvasGroup from = GetCanvasGroupForScreen(currentScreen);
        CanvasGroup to = GetCanvasGroupForScreen(newScreen);

        // Fade
        FadeScreens(from, to);

        // Ship preview kamera odağı
        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
        {
            switch (newScreen)
            {
                case ScreenType.Home:
                    preview.SetFocus(ShipFocusType.Ammo);
                    break;
                case ScreenType.Upgrade:
                    preview.SetFocus(ShipFocusType.Default);
                    break;
                case ScreenType.Settings:
                    // İstersen farklı bir odağa da çekebilirsin
                    preview.SetFocus(ShipFocusType.Default);
                    break;
            }
        }

        // Button animasyonları
        AnimateButtonsForScreen(newScreen);

        currentScreen = newScreen;
    }

    private CanvasGroup GetCanvasGroupForScreen(ScreenType screen)
    {
        switch (screen)
        {
            case ScreenType.Home:     return welcomeScreen;
            case ScreenType.Upgrade:  return upgradeScreen;
            case ScreenType.Settings: return settingsScreen;
        }
        return null;
    }

    /// <summary>
    /// Genel geçiş fonksiyonu.
    /// fromScreen yavaşça kaybolurken toScreen aynı anda görünür.
    /// </summary>
    public void FadeScreens(CanvasGroup fromScreen, CanvasGroup toScreen)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(FadeRoutine(fromScreen, toScreen));
    }

    private IEnumerator FadeRoutine(CanvasGroup from, CanvasGroup to)
    {
        // === ANİMASYON BAŞLAMADAN ÖNCE: Siyah paneli aç ===
        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(true);

        // Başlangıç ayarları
        if (to != null)
        {
            to.gameObject.SetActive(true);
            to.alpha = 0f;
            to.interactable = false;
            to.blocksRaycasts = false;
        }

        if (from != null)
        {
            from.alpha = 1f;
            from.interactable = false;      // animasyon sırasında tıklanmasın
            from.blocksRaycasts = false;
        }

        float t = 0f;

        // --- 1. FAZ: SADECE FROM FADE OUT ---
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            if (from != null)
                from.alpha = Mathf.Lerp(1f, 0f, lerp);

            yield return null;
        }

        // from tamamen kapandı
        if (from != null)
        {
            from.alpha = 0f;
            from.gameObject.SetActive(false);
        }

        // --- 2. FAZ: SADECE TO FADE IN ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            if (to != null)
                to.alpha = Mathf.Lerp(0f, 1f, lerp);

            yield return null;
        }

        // to tamamen açıldı
        if (to != null)
        {
            to.alpha = 1f;
            to.interactable = true;
            to.blocksRaycasts = true;
        }

        // === ANİMASYON BİTTİKTEN SONRA: Siyah paneli kapat ===
        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(false);

        currentTransition = null;
    }

    // =================== BUTTON ANİMASYONLARI ===================

    private void AnimateButtonsForScreen(ScreenType activeScreen)
    {
        if (buttonAnimCoroutine != null)
            StopCoroutine(buttonAnimCoroutine);

        buttonAnimCoroutine = StartCoroutine(ButtonSizeAnimCoroutine(activeScreen));
    }

    private IEnumerator ButtonSizeAnimCoroutine(ScreenType activeScreen)
    {
        // Başlangıç boyutlarını al
        Vector2 homeStart = homeButton != null ? homeButton.sizeDelta : Vector2.zero;
        Vector2 upgradeStart = upgradeButton != null ? upgradeButton.sizeDelta : Vector2.zero;
        Vector2 settingsStart = settingsButton != null ? settingsButton.sizeDelta : Vector2.zero;

        // Hedef boyutlar (aktif +30, diğerleri -15)
        Vector2 grow = new Vector2(30f, 30f);
        Vector2 shrink = new Vector2(15f, 15f);

        Vector2 homeTarget = homeOriginalSize;
        Vector2 upgradeTarget = upgradeOriginalSize;
        Vector2 settingsTarget = settingsOriginalSize;

        switch (activeScreen)
        {
            case ScreenType.Home:
                homeTarget    = homeOriginalSize    + grow;
                upgradeTarget = upgradeOriginalSize - shrink;
                settingsTarget= settingsOriginalSize- shrink;
                break;
            case ScreenType.Upgrade:
                homeTarget    = homeOriginalSize    - shrink;
                upgradeTarget = upgradeOriginalSize + grow;
                settingsTarget= settingsOriginalSize- shrink;
                break;
            case ScreenType.Settings:
                homeTarget    = homeOriginalSize    - shrink;
                upgradeTarget = upgradeOriginalSize - shrink;
                settingsTarget= settingsOriginalSize+ grow;
                break;
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, buttonAnimDuration);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / dur);

            if (homeButton != null)
                homeButton.sizeDelta = Vector2.Lerp(homeStart, homeTarget, lerp);
            if (upgradeButton != null)
                upgradeButton.sizeDelta = Vector2.Lerp(upgradeStart, upgradeTarget, lerp);
            if (settingsButton != null)
                settingsButton.sizeDelta = Vector2.Lerp(settingsStart, settingsTarget, lerp);

            yield return null;
        }

        // son değerler
        if (homeButton != null)    homeButton.sizeDelta    = homeTarget;
        if (upgradeButton != null) upgradeButton.sizeDelta = upgradeTarget;
        if (settingsButton != null)settingsButton.sizeDelta= settingsTarget;

        buttonAnimCoroutine = null;
    }

    // =================== BUTONLARIN ÇAĞIRACAĞI FONKSİYONLAR ===================

    // Üst bardaki HOME butonu
    public void OnHomeButtonClicked()
    {
        SwitchScreen(ScreenType.Home);
    }

    // Üst bardaki UPGRADE butonu
    public void OnUpgradeButtonClicked()
    {
        SwitchScreen(ScreenType.Upgrade);
    }

    // Üst bardaki SETTINGS butonu
    public void OnSettingsButtonClicked()
    {
        SwitchScreen(ScreenType.Settings);
    }

    /// <summary>
    /// Eski kullanımlar için: Welcome'dan Upgrade'e geç.
    /// (İstersen proje genelinde bunu kullanmaya da devam edebilirsin.)
    /// </summary>
    public void GoToUpgradeScreen()
    {
        SwitchScreen(ScreenType.Upgrade);
    }

    /// <summary>
    /// Eski kullanımlar için: Upgrade'den Welcome'a dön.
    /// </summary>
    public void BackToMainMenu()
    {
        SwitchScreen(ScreenType.Home);
    }

    public void ShowShipPreview()
    {
        if (shipPreviewCanvas != null)
            shipPreviewCanvas.SetActive(true);

        if (shipPreviewRoot != null)
            shipPreviewRoot.SetActive(true);
    }

    public void HideShipPreview()
    {
        if (shipPreviewCanvas != null)
            shipPreviewCanvas.SetActive(false);

        if (shipPreviewRoot != null)
            shipPreviewRoot.SetActive(false);
    }
}
