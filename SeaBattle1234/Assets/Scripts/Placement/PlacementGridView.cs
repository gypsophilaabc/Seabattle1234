using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementGridView : MonoBehaviour
{
    public GameObject cellPrefab;

    private CellView[,] views = new CellView[BoardModel.H, BoardModel.W];

    private List<int> queue = new List<int>(); // 依次存 typeId
    private int queueIndex = 0;
    private bool rotated = false;

    // 预览
    private Vector2Int? hoverCell = null;
    private List<Vector2Int> lastPreviewCells = new List<Vector2Int>();
    private bool lastPreviewValid = false;

    void Start()
    {
        BuildGrid();
        EnsureScoutRevealForAll();
        BuildPlacementQueue();
        Refresh();
        PrintCurrentShipHint();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotated = !rotated;
            Debug.Log(rotated ? "旋转：开 (h/w交换)" : "旋转：关");
            PrintCurrentShipHint();
            UpdatePreview(); // 旋转后更新预览
        }

        UpdateHoverCellFromUIRaycast();
        UpdatePreview();
    }

    void UpdateHoverCellFromUIRaycast()
    {
        hoverCell = null;

        if (EventSystem.current == null) return;

        var ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        for (int i = 0; i < results.Count; i++)
        {
            var cv = results[i].gameObject.GetComponentInParent<CellView>();
            if (cv != null)
            {
                hoverCell = cv.coord;
                break;
            }
        }
    }

    void UpdatePreview()
    {
        // 清除上一次预览（恢复正常渲染）
        if (lastPreviewCells.Count > 0)
        {
            foreach (var p in lastPreviewCells)
                RefreshCell(p.x, p.y);

            lastPreviewCells.Clear();
        }

        int tid = CurrentTypeId;
        if (tid == -1) return;
        if (!hoverCell.HasValue) return;

        var t = ShipCatalog.Types[tid];
        int h = rotated ? t.w : t.h;
        int w = rotated ? t.h : t.w;

        int topR = hoverCell.Value.x;
        int topC = hoverCell.Value.y;

        // 计算预览覆盖格
        var cells = GetRectCells(topR, topC, h, w);
        bool valid = CanPlaceRect(GameManager.Instance.boards[0], topR, topC, h, w);

        lastPreviewValid = valid;
        lastPreviewCells = cells;

        // 上色（合法绿 / 非法红）
        Color col = valid ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
        foreach (var p in cells)
            views[p.x, p.y].SetPreview(col);
    }

    List<Vector2Int> GetRectCells(int topR, int topC, int h, int w)
    {
        var list = new List<Vector2Int>();

        // 越界时也别崩：只把棋盘内的格子加入 list，方便显示“红色超界边缘”
        for (int dr = 0; dr < h; dr++)
        {
            for (int dc = 0; dc < w; dc++)
            {
                int r = topR + dr;
                int c = topC + dc;
                if (r >= 0 && r < BoardModel.H && c >= 0 && c < BoardModel.W)
                    list.Add(new Vector2Int(r, c));
            }
        }
        return list;
    }

    bool CanPlaceRect(BoardModel board, int topR, int topC, int h, int w)
    {
        // 越界直接非法
        if (topR < 0 || topC < 0 || topR + h > BoardModel.H || topC + w > BoardModel.W)
            return false;

        // 重叠非法
        for (int dr = 0; dr < h; dr++)
            for (int dc = 0; dc < w; dc++)
                if (board.truth[topR + dr, topC + dc].hasShip)
                    return false;

        return true;
    }

    void BuildPlacementQueue()
    {
        queue.Clear();
        foreach (var n in ShipCatalog.Fleet)
            for (int i = 0; i < n.count; i++)
                queue.Add(n.typeId);

        queueIndex = 0;
    }

    int CurrentTypeId => (queueIndex < queue.Count) ? queue[queueIndex] : -1;

    void PrintCurrentShipHint()
    {
        int tid = CurrentTypeId;
        if (tid == -1)
        {
            Debug.Log("✅ 摆放完成！");
            return;
        }

        var t = ShipCatalog.Types[tid];
        int hh = rotated ? t.w : t.h;
        int ww = rotated ? t.h : t.w;

        Debug.Log($"当前要放置：{t.name}  尺寸={hh}x{ww}  (按 R 旋转)");
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

                cv.Init(new Vector2Int(r, c), OnCellClicked);
            }
        }
    }

    void OnCellClicked(Vector2Int rc)
    {
        int tid = CurrentTypeId;
        if (tid == -1)
        {
            Debug.Log("已经全部摆完了。");
            return;
        }

        var t = ShipCatalog.Types[tid];
        int h = rotated ? t.w : t.h;
        int w = rotated ? t.h : t.w;

        bool ok = TryPlaceRectShip(GameManager.Instance.boards[0], tid, rc.x, rc.y, h, w);
        if (!ok) return;

        queueIndex++;
        Refresh();
        PrintCurrentShipHint();
    }

    bool TryPlaceRectShip(BoardModel board, int typeId, int topR, int topC, int h, int w)
    {
        if (!CanPlaceRect(board, topR, topC, h, w))
        {
            Debug.Log("❌ 非法：越界或重叠");
            return false;
        }

        // 创建船实例，shipId = 即将插入的索引
        int shipId = board.ships.Count;
        ShipInstance inst = new ShipInstance();
        inst.typeId = typeId;

        for (int dr = 0; dr < h; dr++)
        {
            for (int dc = 0; dc < w; dc++)
            {
                int r = topR + dr;
                int c = topC + dc;

                var cell = board.truth[r, c];
                cell.hasShip = true;
                cell.shipId = shipId;
                board.truth[r, c] = cell;

                inst.cells.Add(new UnityEngine.Vector2Int(r, c));
            }
        }

        board.ships.Add(inst);

        Debug.Log($"✅ 放置成功：typeId={typeId} {h}x{w} shipId={shipId} @ ({topR},{topC})");
        return true;
    }

    void EnsureScoutRevealForAll()
    {
        var view = GameManager.Instance.views[0];
        for (int r = 0; r < BoardModel.H; r++)
            for (int c = 0; c < BoardModel.W; c++)
                view.intel[r, c] |= CellIntelFlags.Scout;
    }

    public void Refresh()
    {
        for (int r = 0; r < BoardModel.H; r++)
            for (int c = 0; c < BoardModel.W; c++)
                RefreshCell(r, c);
    }

    void RefreshCell(int r, int c)
    {
        var board = GameManager.Instance.boards[0];
        var view = GameManager.Instance.views[0];

        RenderState rs = RenderRules.GetRenderState(board.truth[r, c], view.intel[r, c]);
        views[r, c].ApplyRenderState(rs);
    }
}