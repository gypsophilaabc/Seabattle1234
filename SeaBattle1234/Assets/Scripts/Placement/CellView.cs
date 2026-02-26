using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
    public Vector2Int coord;
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void Init(Vector2Int c, System.Action<Vector2Int> onClick)
    {
        coord = c;
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick(coord));
    }

    public void ApplyRenderState(RenderState s)
    {
        // 先用颜色占位，后面换Sprite
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

    public void SetPreview(Color color)
    {
        // 用更高优先级的颜色覆盖当前显示
        var img = GetComponent<UnityEngine.UI.Image>();
        img.color = color;
    }

    public void ClearPreview()
    {
        // 清预览后，外部会调用 ApplyRenderState 刷回正常颜色
    }


}