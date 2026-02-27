using UnityEngine;

public class BattleEnemyGridView : MonoBehaviour
{
    public GameObject cellPrefab;

    private CellView[,] views = new CellView[BoardModel.H, BoardModel.W];
    private System.Action<Vector2Int> onCellClick;

    private System.Action<Vector2Int> onHoverEnter;
    private System.Action<Vector2Int> onHoverExit;

    public void BindHover(System.Action<Vector2Int> enter, System.Action<Vector2Int> exit)
    {
        onHoverEnter = enter;
        onHoverExit = exit;
    }

    public void Bind(System.Action<Vector2Int> onClick)
    {
        onCellClick = onClick;
    }

    void Start()
    {
        BuildGrid();
    }

    void BuildGrid()
    {
        for (int r = 0; r < BoardModel.H; r++)
            for (int c = 0; c < BoardModel.W; c++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                var cv = cell.GetComponent<CellView>();
                views[r, c] = cv;

                cv.Init(
                    new Vector2Int(r, c),
                    rc => { onCellClick?.Invoke(rc); },
                    rc => { onHoverEnter?.Invoke(rc); },
                    rc => { onHoverExit?.Invoke(rc); }
                    );
            }
    }

    public void Refresh(BoardModel enemyBoard, PlayerViewModel playerView)
    {
        for (int r = 0; r < BoardModel.H; r++)
            for (int c = 0; c < BoardModel.W; c++)
            {
                RenderState rs = RenderRules.GetRenderState(enemyBoard.truth[r, c], playerView.intel[r, c]);
                views[r, c].ApplyRenderState(rs);
            }
    }
    public void ClearAllPreviews()
    {
        for (int r = 0; r < BoardModel.H; r++)
            for (int c = 0; c < BoardModel.W; c++)
                views[r, c].ClearPreview();
    }

    public void PreviewCell(int r, int c, Color color)
    {
        if (r < 0 || r >= BoardModel.H || c < 0 || c >= BoardModel.W) return;
        views[r, c].SetPreview(color);
    }
}