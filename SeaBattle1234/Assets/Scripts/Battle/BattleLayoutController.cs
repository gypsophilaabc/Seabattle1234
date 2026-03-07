using UnityEngine;

public class BattleLayoutController : MonoBehaviour
{
    [Header("References")]
    public BattleFlowController flow;

    public RectTransform grid0Rect;
    public RectTransform grid1Rect;

    public CanvasGroup grid0Group;
    public CanvasGroup grid1Group;

    [Header("Corner Position (–°∆Â≈ÃŒª÷√)")]
    public Vector2 cornerOffset = new Vector2(240f, 100f);

    [Header("Focus Position (÷˜∆Â≈ÃŒª÷√)")]
    public Vector2 focusOffset = new Vector2(40f, 20f);

    [Header("Resolving Position")]
    public Vector2 resolveOffset = new Vector2(180f, 0f);

    [Header("Scale")]
    public float bigScale = 1f;
    public float smallScale = 0.6f;
    public float resolveScale = 0.8f;

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
        // P0 ÷˜∆Â≈Ã
        SetGrid(
            grid0Rect,
            new Vector2(-focusOffset.x, focusOffset.y),
            bigScale,
            1f
        );

        // P1 –°∆Â≈Ã
        SetGrid(
            grid1Rect,
            new Vector2(cornerOffset.x, -cornerOffset.y),
            smallScale,
            0.4f
        );
    }

    void LayoutPlanningP1()
    {
        // P0 –°∆Â≈Ã
        SetGrid(
            grid0Rect,
            new Vector2(-cornerOffset.x, cornerOffset.y),
            smallScale,
            0.4f
        );

        // P1 ÷˜∆Â≈Ã
        SetGrid(
            grid1Rect,
            new Vector2(focusOffset.x, focusOffset.y),
            bigScale,
            1f
        );
    }

    void LayoutResolving()
    {
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