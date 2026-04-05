using UnityEngine;
using UnityEngine.UI;

public class GridOverlayView : MonoBehaviour
{
    [SerializeField] private Image image;

    void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite, float alpha = 1f)
    {
        if (image == null) return;

        image.sprite = sprite;
        image.color = new Color(1f, 1f, 1f, alpha);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
    }

    public void SetAlpha(float alpha)
    {
        if (image == null) return;
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    public RectTransform RectTf => transform as RectTransform;
}