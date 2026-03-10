using UnityEngine;

public class BattleLayoutController : MonoBehaviour
{
    [Header("References")]
    public BattleFlowController flow;

    public RectTransform grid0Rect;
    public RectTransform grid1Rect;

    public CanvasGroup grid0Group;
    public CanvasGroup grid1Group;

    [Header("Corner Position (小棋盘位置)")]
    public Vector2 cornerOffset = new Vector2(240f, 100f);

    [Header("Focus Position (主棋盘位置)")]
    public Vector2 focusOffset = new Vector2(40f, 20f);

    [Header("Resolving Position")]
    public Vector2 resolveOffset = new Vector2(320f, 0f);

    [Header("Scale")]
    public float bigScale = 1f;
    public float smallScale = 0.6f;
    public float resolveScale = 0.8f;

    [Header("Visual")]
    public float activeAlpha = 1f;
    public float inactiveAlpha = 0.22f;

    [Header("Animation Speed")]
    public float animSpeed = 8f;

    void Update()
    {
        if (flow == null) return;

        if (flow.IsPlanningPhase())
        {
            int pid = flow.GetActivePlanningPlayer();

            if (pid == 0)
                LayoutPlanningP0();
            else
                LayoutPlanningP1();
        }
        else if (flow.IsResolvingPhase())
        {
            LayoutResolving();
        }
    }

    void LayoutPlanningP0()
    {
        // 当前使用中的棋盘放到最上层
        if (grid0Rect != null) grid0Rect.SetAsLastSibling();

        // P0 主棋盘
        SetGrid(
            grid0Rect,
            new Vector2(-focusOffset.x, focusOffset.y),
            bigScale,
            activeAlpha
        );

        // P1 弃用棋盘
        SetGrid(
            grid1Rect,
            new Vector2(cornerOffset.x, -cornerOffset.y),
            smallScale,
            inactiveAlpha
        );
    }

    void LayoutPlanningP1()
    {
        // 当前使用中的棋盘放到最上层
        if (grid1Rect != null) grid1Rect.SetAsLastSibling();

        // P0 弃用棋盘
        SetGrid(
            grid0Rect,
            new Vector2(-cornerOffset.x, cornerOffset.y),
            smallScale,
            inactiveAlpha
        );

        // P1 主棋盘
        SetGrid(
            grid1Rect,
            new Vector2(focusOffset.x, focusOffset.y),
            bigScale,
            activeAlpha
        );
    }

    void LayoutResolving()
    {
        // resolving 阶段不用强调层级，保持当前顺序即可
        SetGrid(
            grid0Rect,
            new Vector2(-resolveOffset.x, resolveOffset.y),
            resolveScale,
            1f
        );

        SetGrid(
            grid1Rect,
            new Vector2(resolveOffset.x, resolveOffset.y),
            resolveScale,
            1f
        );
    }

    void SetGrid(RectTransform rect, Vector2 targetPos, float targetScale, float alpha)
    {
        if (rect == null) return;

        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            targetPos,
            Time.deltaTime * animSpeed
        );

        rect.localScale = Vector3.Lerp(
            rect.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * animSpeed
        );

        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = Mathf.Lerp(
                cg.alpha,
                alpha,
                Time.deltaTime * animSpeed
            );
        }
    }
}