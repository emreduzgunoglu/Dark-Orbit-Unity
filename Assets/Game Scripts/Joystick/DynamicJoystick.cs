using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DynamicJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Parts")]
    public Image background;   // JoystickBG (Image)
    public Image handle;       // JoystickHandle (Image)

    [Header("Settings")]
    public float handleLimit = 0.6f;  // 0..1
    public bool IsActive { get; private set; } = false;

    private Canvas canvas;
    private RectTransform bgRect;
    private RectTransform handleRect;
    private Vector2 input = Vector2.zero;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        bgRect = background.rectTransform;
        handleRect = handle.rectTransform;

        // başta gizli
        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);
        IsActive = false;
        input = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // joystick'i görünür yap
        background.gameObject.SetActive(true);
        handle.gameObject.SetActive(true);
        IsActive = true;
        input = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;

        // dokunduğun yeri canvas içinde bul
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // joystick arka planını oraya taşı
        bgRect.anchoredPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsActive) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bgRect,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        float radius = bgRect.sizeDelta.x * 0.5f;
        Vector2 dir = localPoint / radius;

        dir = Vector2.ClampMagnitude(dir, 1f);
        input = dir;

        handleRect.anchoredPosition = dir * radius * handleLimit;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsActive = false;
        input = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;
        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);
    }

    public Vector2 GetInput()
    {
        return input;
    }
}
