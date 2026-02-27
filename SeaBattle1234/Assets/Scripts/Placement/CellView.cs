using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int coord;

    private Image img;
    private Action<Vector2Int> onClick;
    private Action<Vector2Int> onHoverEnter;
    private Action<Vector2Int> onHoverExit;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    // ✅ 兼容：点击 + 悬停 enter/exit
    public void Init(
        Vector2Int c,
        Action<Vector2Int> onClick,
        Action<Vector2Int> onHoverEnter = null,
        Action<Vector2Int> onHoverExit = null)
    {
        coord = c;

        this.onClick = onClick;
        this.onHoverEnter = onHoverEnter;
        this.onHoverExit = onHoverExit;

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => this.onClick?.Invoke(coord));
        }
    }

    public void ApplyRenderState(RenderState s)
    {
        switch (s)
        {
            case RenderState.Sea: img.color = new Color(0.2f, 0.5f, 0.9f, 1f); break;
            case RenderState.GunMiss: img.color = new Color(0.6f, 0.8f, 1f, 1f); break;
            case RenderState.GunHit: img.color = new Color(1f, 0.4f, 0.2f, 1f); break;
            case RenderState.TorpLine: img.color = new Color(0.6f, 0.6f, 0.6f, 1f); break;
            case RenderState.TorpHitLine: img.color = new Color(0.7f, 0.9f, 1f, 1f); break;
            case RenderState.BombArea: img.color = new Color(0.2f, 0.9f, 0.3f, 1f); break;
            case RenderState.BombHit: img.color = new Color(0.9f, 0.9f, 0.2f, 1f); break;
            case RenderState.ScoutShip: img.color = Color.black; break;
            case RenderState.ScoutEmpty: img.color = Color.white; break;
        }
    }

    // ✅ 预览覆盖（我们用半透明更像“悬停”）
    public void SetPreview(Color color)
    {
        img.color = color;
    }

    public void ClearPreview()
    {
        // 不在这里恢复颜色：外面会 Refresh() 再 ApplyRenderState
    }

    // ===== Hover events =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke(coord);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke(coord);
    }
}