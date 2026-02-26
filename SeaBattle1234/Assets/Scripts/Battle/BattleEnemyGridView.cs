using UnityEngine;

public class BattleEnemyGridView : MonoBehaviour
{
    public GameObject cellPrefab;

    private CellView[,] views = new CellView[BoardModel.H, BoardModel.W];

    void Start()
    {
        BuildGrid();
        Refresh();
    }

    void BuildGrid()
    {
        for (int r = 0; r < BoardModel.H; r++)
        {
            for (int c = 0; c < BoardModel.W; c++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                var cv = cell.GetComponent<CellView>();
                views[r, c] = cv;

                cv.Init(new Vector2Int(r, c), (rc) =>
                {
                    Debug.Log($"EnemyClick r={rc.x}, c={rc.y}");
                });
            }
        }
        Debug.Log($"Built enemy grid: {BoardModel.H}x{BoardModel.W} = {BoardModel.H * BoardModel.W}");
    }

    public void Refresh()
    {
        BoardModel enemyTruth = GameManager.Instance.boards[1];
        PlayerViewModel p0View = GameManager.Instance.views[0];

        for (int r = 0; r < BoardModel.H; r++)
        {
            for (int c = 0; c < BoardModel.W; c++)
            {
                RenderState rs = RenderRules.GetRenderState(enemyTruth.truth[r, c], p0View.intel[r, c]);
                views[r, c].ApplyRenderState(rs);
            }
        }
    }
}