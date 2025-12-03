using UnityEngine;

public enum ShipFocusType
{
    Default,
    Horizontal, // motorlar / arka taraf
    Vertical,   // yan / üst görünüm
    Ammo        // silah / ön taraf
}

public class ShipPreviewController : MonoBehaviour
{
    [Header("Ship Parent")]
    [Tooltip("Gemi instance'ının yerleştirileceği boş transform")]
    public Transform shipParent;

    [Header("Camera")]
    [Tooltip("ShipCamera'nın Transform'u")]
    public Transform cameraTransform;

    [Header("Camera Focus Points")]
    public Transform defaultFocus;     // Genel görünüm
    public Transform horizontalFocus;  // Horizontal upgrade → motorlar / arka
    public Transform verticalFocus;    // Vertical upgrade → yan/üst
    public Transform ammoFocus;        // Ammo upgrade → silahlar/ön

    [Header("Ship Rotation Points")]
    [Tooltip("Gemi rotasyonlarını tanımlamak için referans boş objeler")]
    public Transform defaultShipRot;     // Default odakta geminin bakacağı yön
    public Transform horizontalShipRot;  // Horizontal odakta geminin bakacağı yön
    public Transform verticalShipRot;    // Vertical odakta geminin bakacağı yön
    public Transform ammoShipRot;        // Ammo odakta geminin bakacağı yön

    [Header("Move & Rotate Settings")]
    public float focusMoveSpeed = 5f;     // Kamera pozisyonu için
    public float focusRotateSpeed = 5f;   // Kamera rotasyonu için
    public float shipRotateSpeed = 5f;    // Gemi rotasyonu için

    [Header("Preview Scale")]
    public float previewScale = 3f;

    [Header("Intro Animation")]
    public float introDuration = 1f;  // yoktan gelme + ammo'ya dönme süresi

    private bool isIntroPlaying = false;

    private GameObject previewShipInstance;
    private Transform currentFocus;
    private Quaternion targetShipRotation;
    private Transform shipTransform;

    void Start()
    {
        SpawnPreviewShip();
        SetInitialFocus();
    }

    private void SpawnPreviewShip()
    {
        if (ShipManager.Instance == null ||
            ShipManager.Instance.currentShipDefinition == null ||
            ShipManager.Instance.currentShipDefinition.shipPrefab == null)
        {
            Debug.LogWarning("ShipPreviewController: ShipDefinition veya prefab eksik.");
            return;
        }

        if (previewShipInstance != null)
        {
            Destroy(previewShipInstance);
        }

        var prefab = ShipManager.Instance.currentShipDefinition.shipPrefab;
        previewShipInstance = Instantiate(prefab, shipParent);

        shipTransform = previewShipInstance.transform;

        shipTransform.localPosition = Vector3.zero;
        shipTransform.localScale = Vector3.zero; // <-- burası önemli

        // default rot
        if (defaultShipRot != null)
            shipTransform.localRotation = defaultShipRot.localRotation;
        else
            shipTransform.localRotation = Quaternion.identity;

        targetShipRotation = shipTransform.localRotation;
    }

    private void SetInitialFocus()
    {
        if (cameraTransform != null && defaultFocus != null)
        {
            // Kamera başlangıçta DEFAULT pozda
            cameraTransform.position = defaultFocus.position;
            cameraTransform.rotation = defaultFocus.rotation;
        }

        if (defaultShipRot != null)
            shipTransform.localRotation = defaultShipRot.localRotation;

        // Intro başlasın
        if (shipTransform != null && cameraTransform != null)
            StartCoroutine(IntroSequence());
    }

    private System.Collections.IEnumerator IntroSequence()
    {
        isIntroPlaying = true;

        float t = 0f;

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * previewScale;

        Vector3 camStartPos = defaultFocus.position;
        Quaternion camStartRot = defaultFocus.rotation;

        Vector3 camEndPos = ammoFocus.position;
        Quaternion camEndRot = ammoFocus.rotation;

        Quaternion shipStartRot = defaultShipRot != null
            ? defaultShipRot.localRotation
            : shipTransform.localRotation;

        Quaternion shipEndRot = ammoShipRot != null
            ? ammoShipRot.localRotation
            : shipStartRot;

        cameraTransform.position = camStartPos;
        cameraTransform.rotation = camStartRot;
        shipTransform.localScale = startScale;
        shipTransform.localRotation = shipStartRot;

        while (t < introDuration)
        {
            t += Time.unscaledDeltaTime;

            // 0–1 arası ham zaman
            float raw = Mathf.Clamp01(t / introDuration);

            // ✅ Ease-out: başta hızlı, sona doğru yavaşlar
            // lerp = 1 - (1 - x)^2  (quadratic ease-out)
            float lerp = 1f - (1f - raw) * (1f - raw);

            // 1) Gemi yoktan var oluyor
            shipTransform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            // 2) Kamera default → ammo
            cameraTransform.position = Vector3.Lerp(camStartPos, camEndPos, lerp);
            cameraTransform.rotation = Quaternion.Slerp(camStartRot, camEndRot, lerp);

            // 3) Gemi rotasyonu default → ammoShipRot
            shipTransform.localRotation = Quaternion.Slerp(shipStartRot, shipEndRot, lerp);

            yield return null;
        }

        shipTransform.localScale = endScale;
        cameraTransform.position = camEndPos;
        cameraTransform.rotation = camEndRot;
        shipTransform.localRotation = shipEndRot;

        SetFocus(ShipFocusType.Ammo);
        isIntroPlaying = false;
    }

    void Update()
    {
        if (!isIntroPlaying)
        {
            UpdateCameraFocus();
            UpdateShipRotation();
        }
    }

    private void UpdateCameraFocus()
    {
        if (cameraTransform == null || currentFocus == null)
            return;

        float dt = Time.unscaledDeltaTime;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            currentFocus.position,
            dt * focusMoveSpeed
        );

        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation,
            currentFocus.rotation,
            dt * focusRotateSpeed
        );
    }

    private void UpdateShipRotation()
    {
        if (shipTransform == null)
            return;

        float dt = Time.unscaledDeltaTime;

        shipTransform.localRotation = Quaternion.Slerp(
            shipTransform.localRotation,
            targetShipRotation,
            dt * shipRotateSpeed
        );
    }

    public void SetFocus(ShipFocusType focusType)
    {
        switch (focusType)
        {
            case ShipFocusType.Horizontal:
                if (horizontalFocus != null) currentFocus = horizontalFocus;
                if (horizontalShipRot != null) targetShipRotation = horizontalShipRot.localRotation;
                break;

            case ShipFocusType.Vertical:
                if (verticalFocus != null) currentFocus = verticalFocus;
                if (verticalShipRot != null) targetShipRotation = verticalShipRot.localRotation;
                break;

            case ShipFocusType.Ammo:
                if (ammoFocus != null) currentFocus = ammoFocus;
                if (ammoShipRot != null) targetShipRotation = ammoShipRot.localRotation;
                break;

            default:
                if (defaultFocus != null) currentFocus = defaultFocus;
                if (defaultShipRot != null) targetShipRotation = defaultShipRot.localRotation;
                break;
        }
    }
}
