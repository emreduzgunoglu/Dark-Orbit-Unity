using UnityEngine;

public class ShipPreviewUIButtons : MonoBehaviour
{
    public void OnViewDefault()
    {
        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
            preview.SetFocus(ShipFocusType.Default);
    }

    public void OnViewHorizontal()
    {
        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
            preview.SetFocus(ShipFocusType.Horizontal);
    }

    public void OnViewVertical()
    {
        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
            preview.SetFocus(ShipFocusType.Vertical);
    }

    public void OnViewAmmo()
    {
        var preview = FindFirstObjectByType<ShipPreviewController>();
        if (preview != null)
            preview.SetFocus(ShipFocusType.Ammo);
    }
}
