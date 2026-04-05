using UnityEngine;
using UnityEngine.UI;
using static BattleController;

public class BattleEnemyGridView : MonoBehaviour
{
    [Header("Grid")]
    public GameObject cellPrefab;

    [SerializeField] private RectTransform cellsRoot;

    private CellView[,] views = new CellView[BoardModel.H, BoardModel.W];
    private System.Action<Vector2Int> onCellClick;
    private System.Action<Vector2Int> onHoverEnter;
    private System.Action<Vector2Int> onHoverExit;

    private bool gridBuilt = false;

    public void BindHover(System.Action<Vector2Int> enter, System.Action<Vector2Int> exit)
    {
        onHoverEnter = enter;
        onHoverExit = exit;
    }

    public void Bind(System.Action<Vector2Int> onClick)
    {
        onCellClick = onClick;
    }

    void Awake()
    {
        EnsureRoots();
        BuildGrid();
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();

        if (cellsRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(cellsRoot);
    }

    private void EnsureRoots()
    {
        if (cellsRoot == null)
        {
            Transform t = transform.Find("CellsRoot");
            if (t != null) cellsRoot = t as RectTransform;
        }

        if (cellsRoot == null)
            Debug.LogWarning($"[EnemyGrid] {gameObject.name} 没找到 CellsRoot，请在 Inspector 里手动挂。");
    }

    void BuildGrid()
    {
        if (gridBuilt) return;

        Debug.Log($"[EnemyGrid] BuildGrid on {gameObject.name}, prefab={(cellPrefab ? cellPrefab.name : "NULL")}");

        if (cellPrefab == null)
        {
            Debug.LogError("[EnemyGrid] cellPrefab is NULL!");
            return;
        }

        if (cellsRoot == null)
        {
            Debug.LogError("[EnemyGrid] cellsRoot is NULL!");
            return;
        }

        for (int r = 0; r < BoardModel.H; r++)
        {
            for (int c = 0; c < BoardModel.W; c++)
            {
                GameObject cell = Instantiate(cellPrefab, cellsRoot);
                cell.name = $"Cell_{r}_{c}";

                var cv = cell.GetComponent<CellView>();
                if (cv == null)
                {
                    Debug.LogError($"[EnemyGrid] cellPrefab {cellPrefab.name} 上没有 CellView 组件！");
                    return;
                }

                views[r, c] = cv;

                cv.Init(
                    new Vector2Int(r, c),
                    rc => { onCellClick?.Invoke(rc); },
                    rc => { onHoverEnter?.Invoke(rc); },
                    rc => { onHoverExit?.Invoke(rc); }
                );
            }
        }

        gridBuilt = true;
        Debug.Log($"[EnemyGrid] BuildGrid done. cellsRoot.childCount={cellsRoot.childCount}");
    }

    public void Refresh(BoardModel enemyBoard, PlayerViewModel playerView)
    {
        if (!gridBuilt || views[0, 0] == null)
            BuildGrid();

        if (!gridBuilt) return;

        for (int r = 0; r < BoardModel.H; r++)
        {
            for (int c = 0; c < BoardModel.W; c++)
            {
                if (views[r, c] == null) continue;

                RenderState rs = RenderRules.GetRenderState(enemyBoard.truth[r, c], playerView.intel[r, c]);
                views[r, c].ApplyRenderState(rs);

                // 每次先清旧 overlay
                views[r, c].ClearBombOverlay();

                if (playerView.TryGetTorpedoVisual(r, c, out var torp) && torp.active)
                    views[r, c].SetTorpedoOverlay(torp.dir, torp.index, torp.isHitLine);
                else
                    views[r, c].ClearTorpedoOverlay();
            }
        }

        RebuildBombOverlays(playerView);
    }

    public void ClearAllPreviews()
    {
        for (int r = 0; r < BoardModel.H; r++)
        {
            for (int c = 0; c < BoardModel.W; c++)
            {
                if (views[r, c] != null)
                    views[r, c].ClearPreview();
            }
        }
    }

    public void PreviewCellSprite(int r, int c, Sprite sprite, float alpha)
    {
        if (r < 0 || r >= BoardModel.H || c < 0 || c >= BoardModel.W) return;
        if (views[r, c] == null) return;

        views[r, c].SetPreviewSprite(sprite, alpha);
    }

    private bool HasFlag(CellIntelFlags intel, CellIntelFlags flag)
    {
        return (intel & flag) != 0;
    }

    private void RebuildBombOverlays(PlayerViewModel playerView)
    {
        for (int r = 0; r < BoardModel.H - 1; r++)
        {
            for (int c = 0; c < BoardModel.W - 1; c++)
            {
                bool a = HasFlag(playerView.intel[r, c], CellIntelFlags.BombArea);
                bool b = HasFlag(playerView.intel[r, c + 1], CellIntelFlags.BombArea);
                bool c0 = HasFlag(playerView.intel[r + 1, c], CellIntelFlags.BombArea);
                bool d = HasFlag(playerView.intel[r + 1, c + 1], CellIntelFlags.BombArea);

                if (!(a && b && c0 && d))
                    continue;

                bool hitA = HasFlag(playerView.intel[r, c], CellIntelFlags.BombAreaHit);
                bool hitB = HasFlag(playerView.intel[r, c + 1], CellIntelFlags.BombAreaHit);
                bool hitC = HasFlag(playerView.intel[r + 1, c], CellIntelFlags.BombAreaHit);
                bool hitD = HasFlag(playerView.intel[r + 1, c + 1], CellIntelFlags.BombAreaHit);

                bool isHit = hitA && hitB && hitC && hitD;

                float alpha = isHit ? 0.85f : 0.6f;

                views[r, c].SetBombOverlay(QuadPart.TL, isHit, alpha);
                views[r, c + 1].SetBombOverlay(QuadPart.TR, isHit, alpha);
                views[r + 1, c].SetBombOverlay(QuadPart.BL, isHit, alpha);
                views[r + 1, c + 1].SetBombOverlay(QuadPart.BR, isHit, alpha);
            }
        }
    }
}