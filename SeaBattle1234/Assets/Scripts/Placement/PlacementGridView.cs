using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameManager;
using UnityEngine.UI;
using TMPro;

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

    //wyx 2026.3.1 增加临时棋盘变量，之后所有GameManager.Instance.boards[0]的地方均换为placementBoard，玩家
    public int playerId = 0;          // 0/1，在 Inspector 里改，明确在给哪个玩家摆放棋盘
    private BoardModel placementBoard;  //临时棋盘变量
    

    private Dictionary<int, int> remainingCounts = new Dictionary<int, int>();
    private int selectedTypeId = -1;
    private List<Button> shipButtons = new List<Button>();

    [Header("Placement UI")]
    public TMP_Text playerText;
    public TMP_Text currentShipText;
    public TMP_Text hintText;
    public TMP_Text remainingShipsText;

    [Header("Ship Select UI")]
    public Transform shipListContainer;
    public Button shipButtonPrefab;

    void Start()
    {
        var gm = GameManager.Instance;
        playerId = (gm.phase == GamePhase.PlacementP0) ? 0 : 1;

        placementBoard = new BoardModel();
        BuildGrid();
        BuildPlacementQueue();
        Refresh();
        PrintCurrentShipHint();

        Debug.Log($"[Placement] phase={gm.phase} playerId={playerId}");
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
        bool valid = CanPlaceRect(placementBoard, topR, topC, h, w);

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
        remainingCounts.Clear();

        foreach (var n in ShipCatalog.Fleet)
        {
            remainingCounts[n.typeId] = n.count;

            for (int i = 0; i < n.count; i++)
                queue.Add(n.typeId);
        }

        queueIndex = 0;

        selectedTypeId = -1;
        foreach (var n in ShipCatalog.Fleet)
        {
            if (remainingCounts[n.typeId] > 0)
            {
                selectedTypeId = n.typeId;
                break;
            }
        }
        Debug.Log($"[BuildPlacementQueue] remainingCounts.Count = {remainingCounts.Count}");
        foreach (var kv in remainingCounts)
        {
            Debug.Log($"[BuildPlacementQueue] typeId={kv.Key}, count={kv.Value}");
        }
        Debug.Log($"[BuildPlacementQueue] selectedTypeId = {selectedTypeId}");
        RefreshPlacementUI();
        RebuildShipListUI();
    }//remaining counts记录每种船剩几只，默认选中第一种还有余量的船

    int CurrentTypeId => selectedTypeId;

    void PrintCurrentShipHint()
    {
        int tid = CurrentTypeId;
        if (tid == -1)
        {
            Debug.Log("✅ 摆放完成！");
            RefreshPlacementUI();
            return;
        }

        var t = ShipCatalog.Types[tid];
        int hh = rotated ? t.w : t.h;
        int ww = rotated ? t.h : t.w;

        Debug.Log($"当前要放置：{t.name}  尺寸={hh}x{ww}  (按 R 旋转)");
        RefreshPlacementUI();
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
        if (!inputEnabled) return;

        int tid = CurrentTypeId;
        if (tid == -1)
        {
            Debug.Log("已经全部摆完了。");
            return;
        }

        var t = ShipCatalog.Types[tid];
        int h = rotated ? t.w : t.h;
        int w = rotated ? t.h : t.w;

        bool ok = TryPlaceRectShip(placementBoard, tid, rc.x, rc.y, h, w);
        if (!ok) return;

        remainingCounts[tid]--;
        Debug.Log($"放置后：typeId={tid}, remain={remainingCounts[tid]}");

        queueIndex++;
        AutoSelectNextAvailableShip();

        Debug.Log($"当前选中船 typeId={CurrentTypeId}");

        Refresh();
        PrintCurrentShipHint();

        if (IsPlacementComplete())
        {
            Debug.Log("✅ 当前玩家所有船只已摆放完成，可以按 Enter 进入下一阶段。");
        }

        Debug.Log($"临时船数={placementBoard.ships.Count}");
        RebuildShipListUI();
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
        var board = placementBoard;

        if (board.truth[r, c].hasShip)
        {
            views[r, c].ApplyRenderState(RenderState.ScoutShip);
        }
        else
        {
            views[r, c].ApplyRenderState(RenderState.Sea);
        }
    }

    // ====== For Flow Controller ======
    private bool inputEnabled = true;

    public void BindToPlayer(int pid)
    {
        playerId = pid;

        // 切玩家时重置临时棋盘与队列（最稳妥：每次进入场景都重新Start也行）
        placementBoard = new BoardModel();
        

        BuildPlacementQueue();
        Refresh();
        PrintCurrentShipHint();

        Debug.Log($"[PlacementGridView] BindToPlayer({pid})");
        RefreshPlacementUI();
        RebuildShipListUI();
    }

    public void DisablePlacementInput() => inputEnabled = false;
    public void EnablePlacementInput() => inputEnabled = true;

    // 清屏：只清“显示”，不动数据（最简单就是强制Refresh一次）
    public void ClearVisual()
    {
        // 先把预览清掉（如果有）
        lastPreviewCells.Clear();
        hoverCell = null;

        Refresh();
        Debug.Log("[PlacementGridView] ClearVisual()");
    }

    public BoardModel GetPlacementBoard()
    {
        return placementBoard;
    }
    public void SelectShipType(int typeId)
    {
        if (!remainingCounts.ContainsKey(typeId)) return;
        if (remainingCounts[typeId] <= 0) return;

        selectedTypeId = typeId;
        PrintCurrentShipHint();
        RebuildShipListUI();
    }
    void AutoSelectNextAvailableShip()
    {
        if (selectedTypeId != -1 &&
            remainingCounts.ContainsKey(selectedTypeId) &&
            remainingCounts[selectedTypeId] > 0)
            return;

        selectedTypeId = -1;
        foreach (var n in ShipCatalog.Fleet)
        {
            if (remainingCounts.ContainsKey(n.typeId) && remainingCounts[n.typeId] > 0)
            {
                selectedTypeId = n.typeId;
                break;
            }
        }
    }
    public bool IsPlacementComplete()
    {
        Debug.Log($"[IsPlacementComplete] remainingCounts.Count = {remainingCounts.Count}");

        foreach (var kv in remainingCounts)
        {
            Debug.Log($"[IsPlacementComplete] typeId={kv.Key}, remain={kv.Value}");
            if (kv.Value > 0) return false;
        }

        return true;
    }
    void RefreshPlacementUI()
    {
        if (playerText != null)
            playerText.text = $"Player: P{playerId}";

        if (CurrentTypeId == -1)
        {
            if (currentShipText != null)
                currentShipText.text = "Current Ship: All placed";
        }
        else
        {
            var t = ShipCatalog.Types[CurrentTypeId];
            int hh = rotated ? t.w : t.h;
            int ww = rotated ? t.h : t.w;

            if (currentShipText != null)
                currentShipText.text = $"Current Ship: {t.name} ({hh}x{ww})";
        }

        if (hintText != null)
            hintText.text = "Controls: Click to place, R to rotate, Enter to confirm";

        if (remainingShipsText != null)
        {
            remainingShipsText.text = "Select a ship from the list below";
        }
    }
    void RebuildShipListUI()
    {
        Debug.Log($"[RebuildShipListUI] container = {shipListContainer.name}");
        if (shipListContainer == null || shipButtonPrefab == null) return;

        // 只删除之前动态生成的按钮，不删别的子物体
        foreach (Button btn in shipButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        shipButtons.Clear();

        foreach (var n in ShipCatalog.Fleet)
        {
            int tid = n.typeId;
            int remain = remainingCounts.ContainsKey(tid) ? remainingCounts[tid] : 0;

            Button btn = Instantiate(shipButtonPrefab, shipListContainer);
            shipButtons.Add(btn);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                string shipName = ShipCatalog.Types[tid].name;
                label.text = $"{shipName} x{remain}";
            }

            btn.interactable = remain > 0;

            int capturedTid = tid;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectShipType(capturedTid));

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (tid == selectedTypeId)
                    img.color = new Color(0.85f, 0.9f, 0.45f, 1f);
                else if (remain <= 0)
                    img.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                else
                    img.color = new Color(1f, 1f, 1f, 0.9f);
            }
        }
    }
}