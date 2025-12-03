using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ShipUpgradeUI : MonoBehaviour
{
    [Header("Horizontal Maneuver UI")]
    public TMP_Text horizontalNameText;
    public TMP_Text horizontalLevelText;   // örn: "2/10"
    public TMP_Text horizontalValueText;   // örn: "Speed: 6.0"
    public Image horizontalProgressBar;    // fillAmount

    [Header("Vertical Maneuver UI")]
    public TMP_Text verticalNameText;
    public TMP_Text verticalLevelText;
    public TMP_Text verticalValueText;
    public Image verticalProgressBar;

    [Header("Ammo UI")]
    public TMP_Text ammoNameText;
    public TMP_Text ammoLevelText;
    public TMP_Text ammoValueText;
    public Image ammoProgressBar;

    [Header("Panels (CanvasGroup)")]
    [Tooltip("Horizontal panelinin üstünde CanvasGroup olmalı")]
    public CanvasGroup horizontalPanel;
    [Tooltip("Vertical panelinin üstünde CanvasGroup olmalı")]
    public CanvasGroup verticalPanel;
    [Tooltip("Ammo panelinin üstünde CanvasGroup olmalı")]
    public CanvasGroup ammoPanel;
    [Tooltip("Upgrade panelinin üstünde CanvasGroup olmalı")]
    public CanvasGroup upgradePanel;

    [Header("Animation Settings")]
    public float fadeDuration = 0.25f;
    public float moveDuration = 0.25f;
    public float verticalMoveOffsetY = 200f;
    public float ammoMoveOffsetY = 400f;

    [Header("Preview Controller")]
    public ShipPreviewController shipPreviewController;

    [Header("Reset Popup")]
    public CanvasGroup resetPopup;         // popup paneli (CanvasGroup)
    public TMP_Text resetPopupMessage;     // "Are you sure..." yazısı
    public float resetPopupFadeDuration = 0.2f;

    // --- Dahili durum ---
    private ShipFocusType currentSelectedFocus = ShipFocusType.Default;
    private bool isTransitioning = false;

    private RectTransform horizontalRT;
    private RectTransform verticalRT;
    private RectTransform ammoRT;

    private Vector2 horizontalOriginalPos;
    private Vector2 verticalOriginalPos;
    private Vector2 ammoOriginalPos;
    private bool originalPositionsCached = false;

    private void Awake()
    {
        CacheOriginalPositions();
    }

    private void OnEnable()
    {
        CacheOriginalPositions();
        SetupInitialLayout();
        RefreshUI();
    }

    private void CacheOriginalPositions()
    {
        if (originalPositionsCached)
            return;

        if (horizontalPanel != null)
        {
            horizontalRT = horizontalPanel.GetComponent<RectTransform>();
            if (horizontalRT != null) horizontalOriginalPos = horizontalRT.anchoredPosition;
        }

        if (verticalPanel != null)
        {
            verticalRT = verticalPanel.GetComponent<RectTransform>();
            if (verticalRT != null) verticalOriginalPos = verticalRT.anchoredPosition;
        }

        if (ammoPanel != null)
        {
            ammoRT = ammoPanel.GetComponent<RectTransform>();
            if (ammoRT != null) ammoOriginalPos = ammoRT.anchoredPosition;
        }

        originalPositionsCached = true;
    }

    private void SetupInitialLayout()
    {
        // Başlangıçta: 3 panel açık, Upgrade kapalı, pozisyonlar original
        SetCanvasGroupInstant(horizontalPanel, true);
        SetCanvasGroupInstant(verticalPanel, true);
        SetCanvasGroupInstant(ammoPanel, true);
        SetCanvasGroupInstant(upgradePanel, false);

        if (horizontalRT != null) horizontalRT.anchoredPosition = horizontalOriginalPos;
        if (verticalRT != null) verticalRT.anchoredPosition = verticalOriginalPos;
        if (ammoRT != null) ammoRT.anchoredPosition = ammoOriginalPos;

        currentSelectedFocus = ShipFocusType.Default;

        if (shipPreviewController != null)
        {
            shipPreviewController.SetFocus(ShipFocusType.Default);
        }
    }

    private void SetCanvasGroupInstant(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;

        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    // =====================  UI REFRESH  ======================

    public void RefreshUI()
    {
        if (ShipManager.Instance == null ||
            ShipManager.Instance.currentShipDefinition == null ||
            ShipManager.Instance.currentUpgradeState == null)
        {
            return;
        }

        var sm = ShipManager.Instance;
        var def = sm.currentShipDefinition;
        var st = sm.currentUpgradeState;

        // Horizontal
        if (horizontalNameText != null)
            horizontalNameText.text = "Horizontal Maneuver";

        if (horizontalLevelText != null)
            horizontalLevelText.text = $"{st.horizontalManeuverLevel}/{def.maxHorizontalManeuverLevel}";

        if (horizontalValueText != null)
        {
            float value = sm.GetHorizontalManeuver();
            horizontalValueText.text = $"Speed: {value:0.0}";
        }

        if (horizontalProgressBar != null)
        {
            float fill = (def.maxHorizontalManeuverLevel > 0)
                ? (float)st.horizontalManeuverLevel / def.maxHorizontalManeuverLevel
                : 0f;
            horizontalProgressBar.fillAmount = Mathf.Clamp01(fill);
        }

        // Vertical
        if (verticalNameText != null)
            verticalNameText.text = "Vertical Maneuver";

        if (verticalLevelText != null)
            verticalLevelText.text = $"{st.verticalManeuverLevel}/{def.maxVerticalManeuverLevel}";

        if (verticalValueText != null)
        {
            float value = sm.GetVerticalManeuver();
            verticalValueText.text = $"Speed: {value:0.0}";
        }

        if (verticalProgressBar != null)
        {
            float fill = (def.maxVerticalManeuverLevel > 0)
                ? (float)st.verticalManeuverLevel / def.maxVerticalManeuverLevel
                : 0f;
            verticalProgressBar.fillAmount = Mathf.Clamp01(fill);
        }

        // Ammo
        if (ammoNameText != null)
            ammoNameText.text = "Max Ammo";

        if (ammoLevelText != null)
            ammoLevelText.text = $"{st.ammoLevel}/{def.maxAmmoLevel}";

        if (ammoValueText != null)
        {
            int value = sm.GetMaxAmmo();
            ammoValueText.text = $"Ammo: {value}";
        }

        if (ammoProgressBar != null)
        {
            float fill = (def.maxAmmoLevel > 0)
                ? (float)st.ammoLevel / def.maxAmmoLevel
                : 0f;
            ammoProgressBar.fillAmount = Mathf.Clamp01(fill);
        }
    }

    // =====================  SELECT BUTTONLARI  ======================

    // Horizontal paneldeki ana buton
    public void OnSelectHorizontalUI()
    {
        if (isTransitioning) return;
        StartCoroutine(SwitchToFocusCoroutine(ShipFocusType.Horizontal));
    }

    // Vertical paneldeki ana buton
    public void OnSelectVerticalUI()
    {
        if (isTransitioning) return;
        StartCoroutine(SwitchToFocusCoroutine(ShipFocusType.Vertical));
    }

    // Ammo paneldeki ana buton
    public void OnSelectAmmoUI()
    {
        if (isTransitioning) return;
        StartCoroutine(SwitchToFocusCoroutine(ShipFocusType.Ammo));
    }

    private IEnumerator SwitchToFocusCoroutine(ShipFocusType focus)
    {
        isTransitioning = true;

        // Kamera / gemi odak değişimi
        if (shipPreviewController != null)
        {
            shipPreviewController.SetFocus(focus);
        }

        // Metric panellerden hangisi "aktif" kalacak?
        CanvasGroup keepPanel = null;

        switch (focus)
        {
            case ShipFocusType.Horizontal:
                keepPanel = horizontalPanel;
                break;
            case ShipFocusType.Vertical:
                keepPanel = verticalPanel;
                break;
            case ShipFocusType.Ammo:
                keepPanel = ammoPanel;
                break;
            default:
                keepPanel = null;
                break;
        }

        CanvasGroup[] metricPanels = new CanvasGroup[] { horizontalPanel, verticalPanel, ammoPanel };

        // Fade için başlangıç alfaları
        float[] startAlpha = new float[metricPanels.Length];
        for (int i = 0; i < metricPanels.Length; i++)
        {
            if (metricPanels[i] != null)
                startAlpha[i] = metricPanels[i].alpha;
            else
                startAlpha[i] = 0f;
        }

        float upgradeStartAlpha = upgradePanel != null ? upgradePanel.alpha : 0f;

        // Pozisyonlar (Vertical / Ammo için)
        Vector2 verticalStartPos = verticalRT != null ? verticalRT.anchoredPosition : Vector2.zero;
        Vector2 verticalEndPos = verticalStartPos;
        if (verticalRT != null)
        {
            verticalEndPos = (focus == ShipFocusType.Vertical)
                ? verticalOriginalPos + new Vector2(0f, verticalMoveOffsetY)
                : verticalOriginalPos;
        }

        Vector2 ammoStartPos = ammoRT != null ? ammoRT.anchoredPosition : Vector2.zero;
        Vector2 ammoEndPos = ammoStartPos;
        if (ammoRT != null)
        {
            ammoEndPos = (focus == ShipFocusType.Ammo)
                ? ammoOriginalPos + new Vector2(0f, ammoMoveOffsetY)
                : ammoOriginalPos;
        }

        // Upgrade panelini aktif hale getirelim ki fade-in sırasında tıklanabilir olsun
        if (upgradePanel != null)
        {
            upgradePanel.gameObject.SetActive(true);
            upgradePanel.interactable = false;   // animasyon bitince true
            upgradePanel.blocksRaycasts = false;
        }

        float t = 0f;
        float fadeDur = Mathf.Max(0.01f, fadeDuration);
        float moveDur = Mathf.Max(0.01f, moveDuration);

        while (t < Mathf.Max(fadeDur, moveDur))
        {
            t += Time.unscaledDeltaTime;
            float fadeLerp = Mathf.Clamp01(t / fadeDur);
            float moveLerp = Mathf.Clamp01(t / moveDur);

            // Metric panellerin alfaları
            for (int i = 0; i < metricPanels.Length; i++)
            {
                var cg = metricPanels[i];
                if (cg == null) continue;

                bool isKeep = (cg == keepPanel);
                float targetAlpha = isKeep ? 1f : 0f;
                float a = Mathf.Lerp(startAlpha[i], targetAlpha, fadeLerp);
                cg.alpha = a;
                cg.interactable = isKeep;          // sadece seçili açık kalacak
                cg.blocksRaycasts = isKeep;
            }

            // Upgrade panel alpha (0 → 1)
            if (upgradePanel != null)
            {
                float a = Mathf.Lerp(upgradeStartAlpha, 1f, fadeLerp);
                upgradePanel.alpha = a;
            }

            // Vertical / Ammo hareketi
            if (verticalRT != null)
            {
                verticalRT.anchoredPosition = Vector2.Lerp(verticalStartPos, verticalEndPos, moveLerp);
            }
            if (ammoRT != null)
            {
                ammoRT.anchoredPosition = Vector2.Lerp(ammoStartPos, ammoEndPos, moveLerp);
            }

            yield return null;
        }

        // Nihai durum
        // Metric paneller
        foreach (var cg in metricPanels)
        {
            if (cg == null) continue;

            bool isKeep = (cg == keepPanel);
            cg.alpha = isKeep ? 1f : 0f;
            cg.interactable = isKeep;
            cg.blocksRaycasts = isKeep;
            cg.gameObject.SetActive(isKeep); // görünmeyenleri komple kapat
        }

        // Upgrade panel
        if (upgradePanel != null)
        {
            upgradePanel.alpha = 1f;
            upgradePanel.interactable = true;
            upgradePanel.blocksRaycasts = true;
            upgradePanel.gameObject.SetActive(true);
        }

        // Pozisyonları tam oturt
        if (verticalRT != null) verticalRT.anchoredPosition = verticalEndPos;
        if (ammoRT != null) ammoRT.anchoredPosition = ammoEndPos;

        currentSelectedFocus = focus;

        isTransitioning = false;
    }

    // =====================  BACK BUTONLARI  ======================

    // Horizontal / Vertical / Ammo inside "Back" hepsi buna bağlanabilir
    public void OnBackToDefaultPanels()
    {
        if (isTransitioning) return;
        StartCoroutine(BackToDefaultCoroutine());
    }

    private IEnumerator BackToDefaultCoroutine()
    {
        isTransitioning = true;

        if (shipPreviewController != null)
        {
            shipPreviewController.SetFocus(ShipFocusType.Default);
        }

        CanvasGroup[] metricPanels = new CanvasGroup[] { horizontalPanel, verticalPanel, ammoPanel };

        // Metric panelleri aktif edelim ki fade-in sırasında görünsünler
        foreach (var cg in metricPanels)
        {
            if (cg == null) continue;
            cg.gameObject.SetActive(true);
        }

        float[] startAlpha = new float[metricPanels.Length];
        for (int i = 0; i < metricPanels.Length; i++)
        {
            if (metricPanels[i] != null)
                startAlpha[i] = metricPanels[i].alpha;
            else
                startAlpha[i] = 0f;
        }

        float upgradeStartAlpha = upgradePanel != null ? upgradePanel.alpha : 0f;

        Vector2 verticalStartPos = verticalRT != null ? verticalRT.anchoredPosition : Vector2.zero;
        Vector2 ammoStartPos = ammoRT != null ? ammoRT.anchoredPosition : Vector2.zero;

        float t = 0f;
        float fadeDur = Mathf.Max(0.01f, fadeDuration);
        float moveDur = Mathf.Max(0.01f, moveDuration);

        while (t < Mathf.Max(fadeDur, moveDur))
        {
            t += Time.unscaledDeltaTime;
            float fadeLerp = Mathf.Clamp01(t / fadeDur);
            float moveLerp = Mathf.Clamp01(t / moveDur);

            // Metric paneller 0 → 1
            for (int i = 0; i < metricPanels.Length; i++)
            {
                var cg = metricPanels[i];
                if (cg == null) continue;
                float a = Mathf.Lerp(startAlpha[i], 1f, fadeLerp);
                cg.alpha = a;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            // Upgrade panel 1 → 0
            if (upgradePanel != null)
            {
                float a = Mathf.Lerp(upgradeStartAlpha, 0f, fadeLerp);
                upgradePanel.alpha = a;
            }

            // Vertical / Ammo pozisyonları original'e dönsün
            if (verticalRT != null)
            {
                verticalRT.anchoredPosition = Vector2.Lerp(verticalStartPos, verticalOriginalPos, moveLerp);
            }
            if (ammoRT != null)
            {
                ammoRT.anchoredPosition = Vector2.Lerp(ammoStartPos, ammoOriginalPos, moveLerp);
            }

            yield return null;
        }

        // Nihai durum
        foreach (var cg in metricPanels)
        {
            if (cg == null) continue;
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.gameObject.SetActive(true);
        }

        if (upgradePanel != null)
        {
            upgradePanel.alpha = 0f;
            upgradePanel.interactable = false;
            upgradePanel.blocksRaycasts = false;
            upgradePanel.gameObject.SetActive(false);
        }

        if (verticalRT != null) verticalRT.anchoredPosition = verticalOriginalPos;
        if (ammoRT != null) ammoRT.anchoredPosition = ammoOriginalPos;

        currentSelectedFocus = ShipFocusType.Default;

        isTransitioning = false;
    }

    // =====================  UPGRADE BUTONU  ======================

    // Upgrade panelindeki TEK Upgrade butonu buna bağlanacak
    public void OnUpgradeButton()
    {
        switch (currentSelectedFocus)
        {
            case ShipFocusType.Horizontal:
                OnUpgradeHorizontal();
                break;
            case ShipFocusType.Vertical:
                OnUpgradeVertical();
                break;
            case ShipFocusType.Ammo:
                OnUpgradeAmmo();
                break;
            default:
                // Default'tayken basılırsa hiçbir şey yapma
                return;
        }
    }

    public void OnUpgradeHorizontal()
    {
        var sm = ShipManager.Instance;
        if (sm == null || sm.currentShipDefinition == null || sm.currentUpgradeState == null)
            return;

        var def = sm.currentShipDefinition;
        var st = sm.currentUpgradeState;

        if (st.horizontalManeuverLevel >= def.maxHorizontalManeuverLevel)
            return; // max seviyeye ulaştı

        st.horizontalManeuverLevel++;

        sm.SaveCurrentState();
        RefreshUI();
        RefreshPlayerStatsInScene();
    }

    public void OnUpgradeVertical()
    {
        var sm = ShipManager.Instance;
        if (sm == null || sm.currentShipDefinition == null || sm.currentUpgradeState == null)
            return;

        var def = sm.currentShipDefinition;
        var st = sm.currentUpgradeState;

        if (st.verticalManeuverLevel >= def.maxVerticalManeuverLevel)
            return;

        st.verticalManeuverLevel++;

        sm.SaveCurrentState();
        RefreshUI();
        RefreshPlayerStatsInScene();
    }

    public void OnUpgradeAmmo()
    {
        var sm = ShipManager.Instance;
        if (sm == null || sm.currentShipDefinition == null || sm.currentUpgradeState == null)
            return;

        var def = sm.currentShipDefinition;
        var st = sm.currentUpgradeState;

        if (st.ammoLevel >= def.maxAmmoLevel)
            return;

        st.ammoLevel++;

        sm.SaveCurrentState();
        RefreshUI();
        RefreshPlayerStatsInScene();
    }

    // === Sahnedeki PlayerController'a haber ver ===
    private void RefreshPlayerStatsInScene()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.RefreshStatsFromShipManager();
        }
    }

    // =====================  RESET POPUP LOGIC  ======================

    // Settings panelindeki "Reset Ship Stats" butonu bunu çağıracak
    public void OnClickOpenResetPopup()
    {
        if (resetPopup == null)
            return;

        // Mesajı ayarla (isterseni TR yap)
        if (resetPopupMessage != null)
            resetPopupMessage.text = "Reset ship stats to default?";

        // popup'ı göster
        gameObject.SetActive(true); // ShipUpgradeUI zaten aktifse sorun yok
        resetPopup.gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(resetPopup, 0f, 1f, resetPopupFadeDuration, true));
    }

    // Popup içindeki "Go Back" / "Cancel" butonu bunu çağıracak
    public void OnClickCancelReset()
    {
        if (resetPopup == null)
            return;

        StartCoroutine(FadeCanvasGroup(resetPopup, resetPopup.alpha, 0f, resetPopupFadeDuration, false));
    }

    // Popup içindeki "Yes" butonu bunu çağıracak
    public void OnClickConfirmReset()
    {
        // Önce gerçekten resetle
        ResetShipStats();

        // Sonra popup'ı kapat
        if (resetPopup != null)
            StartCoroutine(FadeCanvasGroup(resetPopup, resetPopup.alpha, 0f, resetPopupFadeDuration, false));
    }

    // Asıl reset işlemi (daha önce yazdığımız)
    public void ResetShipStats()
    {
        var sm = ShipManager.Instance;
        if (sm == null || sm.currentUpgradeState == null)
            return;

        var st = sm.currentUpgradeState;

        // Tüm statları sıfırla
        st.horizontalManeuverLevel = 0;
        st.verticalManeuverLevel = 0;
        st.ammoLevel = 0;

        // Kaydet
        sm.SaveCurrentState();

        // UI yenile
        RefreshUI();

        // Player Controller sahnedeyse statları güncelle
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.RefreshStatsFromShipManager();
        }

        Debug.Log("Ship stats have been reset to default.");
    }

    // Genel fade helper
    private System.Collections.IEnumerator FadeCanvasGroup(
        CanvasGroup cg,
        float from,
        float to,
        float duration,
        bool setActiveAfter)
    {
        if (cg == null) yield break;

        if (setActiveAfter)
        {
            cg.gameObject.SetActive(true);
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, duration);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / dur);
            float a = Mathf.Lerp(from, to, lerp);
            cg.alpha = a;
            yield return null;
        }

        cg.alpha = to;

        if (to <= 0.01f)
        {
            cg.gameObject.SetActive(false);
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        else
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

}
