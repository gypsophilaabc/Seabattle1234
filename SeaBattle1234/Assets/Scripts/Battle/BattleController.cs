using UnityEngine;

public class BattleController : MonoBehaviour
{
    public BattleEnemyGridView enemyGridView;
    [Header("Config (拖 GameConfig_Default 进来)")]
    public GameConfig config;

    public System.Action OnPlanChanged; // lq ; UI 面板监听（如果回合内攻击计划变化，就刷新列表/数量）

    private BoardModel enemyBoard;
    private PlayerViewModel playerView;
    private Dir4 currentTorpDir = Dir4.Right;

    private TurnPlan plan = new TurnPlan();
    private WeaponType currentWeapon = WeaponType.Gun;

    private bool hasHover;
    private Vector2Int hoverRC;

    public IAttackResolver resolver;
    // TODO(yjl): 当前仍可直接用 BattleResolver.Resolve
    // TODO(dyh): unity里面不能直接拖interface，所以这里只留一个注释。你需要在现在代码里面new一个来实现。未来在 Awake/Start 里 resolver = new AdvancedResolver();

    void Start()
    {
        enemyBoard = GameManager.Instance.boards[1];
        playerView = GameManager.Instance.views[0];

        enemyGridView.Bind(OnClickEnemyCell);
        enemyGridView.BindHover(OnHoverEnter, OnHoverExit);
        RedrawAll(); 

        Debug.Log("BattleController ready. 1=Gun 2=Torpedo 3=Bomb 4=Scout, Space=Resolve, Backspace=Undo, C=ClearPlan");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentWeapon = WeaponType.Gun; Debug.Log("Weapon=Gun"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentWeapon = WeaponType.Torpedo; Debug.Log("Weapon=Torpedo"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentWeapon = WeaponType.Bomb; Debug.Log("Weapon=Bomb(2x2)"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { currentWeapon = WeaponType.Scout; Debug.Log("Weapon=Scout(2x2)"); }
        if (Input.GetKeyDown(KeyCode.W)) { currentTorpDir = Dir4.Up; Debug.Log("TorpDir=Up"); }
        if (Input.GetKeyDown(KeyCode.S)) { currentTorpDir = Dir4.Down; Debug.Log("TorpDir=Down"); }
        if (Input.GetKeyDown(KeyCode.A)) { currentTorpDir = Dir4.Left; Debug.Log("TorpDir=Left"); }
        if (Input.GetKeyDown(KeyCode.D)) { currentTorpDir = Dir4.Right; Debug.Log("TorpDir=Right"); }

        if (Input.GetKeyDown(KeyCode.Space))
            ResolvePlan();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (plan.TryPop(out var a))
            {
                Debug.Log($"Undo plan: {a.weapon} at {a.anchor} (planCount={plan.Count})");
                OnPlanChanged?.Invoke();
                RedrawAll();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            plan.Clear();
            Debug.Log("Plan cleared.");
            RedrawAll();
        }
    }

    private void OnClickEnemyCell(Vector2Int rc)
    {
        if (currentWeapon == WeaponType.Torpedo)
            plan.Push(TurnAction.Torpedo(rc, currentTorpDir));
        else
            plan.Push(new TurnAction(currentWeapon, rc));
        Debug.Log($"Plan +1: {currentWeapon} at {rc} (planCount={plan.Count})");
        
        OnPlanChanged?.Invoke();

        RedrawAll();
    }

    private void ResolvePlan()
    {
        Debug.Log($"Resolve plan: total={plan.Count}  gun={plan.gun.Count} torp={plan.torp.Count} bomb={plan.bomb.Count} scout={plan.scout.Count}");

        // 你们的 demo 结算顺序：Gun -> Torpedo -> Bomb
        foreach (var a in plan.GunSeq()) BattleResolver.Resolve(enemyBoard, playerView, a);
        foreach (var a in plan.TorpSeq()) BattleResolver.Resolve(enemyBoard, playerView, a);
        foreach (var a in plan.BombSeq())
        {
            var area = GetArea2x2(a.anchor);
            foreach (var p in area)
                enemyGridView.PreviewCell(p.x, p.y, new Color(0.3f, 1f, 0.3f, 1f)); // 绿
        }

        foreach (var a in plan.ScoutSeq())
        {
            var area = GetArea2x2(a.anchor);
            foreach (var p in area)
                enemyGridView.PreviewCell(p.x, p.y, new Color(1f, 1f, 0.4f, 1f)); // 黄
        }

        plan.Clear();

        OnPlanChanged?.Invoke();

        RedrawAll(); // 这会刷新到正式状态且没有预览
        Debug.Log("Resolve done. Plan cleared.");
        
    }

    private void RedrawAll()
    {
        // 1) 正式渲染
        enemyGridView.Refresh(enemyBoard, playerView);

        // 2) 画“已计划”的预览（点击入栈）
        DrawPlanPreviews();

        // 3) 画“悬停跟随”的预览（优先级最高）
        if (hasHover)
            DrawHoverPreview();
    }

    private void DrawPlanPreviews()
    {
        // 计划预览：用半透明颜色（更像“预览层”）
        var gunCol = new Color(0.4f, 0.7f, 1f, 0.55f);
        var torpCol = new Color(0.7f, 0.7f, 0.7f, 0.55f);
        var bombCol = new Color(0.3f, 1f, 0.3f, 0.55f);
        var scoutCol = new Color(1f, 1f, 0.4f, 0.55f);

        foreach (var a in plan.GunSeq())
            enemyGridView.PreviewCell(a.anchor.x, a.anchor.y, gunCol);

        foreach (var a in plan.TorpSeq())
            foreach (var p in GetTorpPath(a.anchor, a.dir))
                enemyGridView.PreviewCell(p.x, p.y, torpCol);

        foreach (var a in plan.BombSeq())
            foreach (var p in GetArea2x2(a.anchor))
                enemyGridView.PreviewCell(p.x, p.y, bombCol);

        foreach (var a in plan.ScoutSeq())
            foreach (var p in GetArea2x2(a.anchor))
                enemyGridView.PreviewCell(p.x, p.y, scoutCol);
    }

    private void DrawHoverPreview()
    {
        // 悬停预览：更明显一点（更不透明）
        var gunCol = new Color(0.4f, 0.7f, 1f, 0.85f);
        var torpCol = new Color(0.7f, 0.7f, 0.7f, 0.85f);
        var bombCol = new Color(0.3f, 1f, 0.3f, 0.85f);
        var scoutCol = new Color(1f, 1f, 0.4f, 0.85f);

        if (currentWeapon == WeaponType.Gun)
        {
            enemyGridView.PreviewCell(hoverRC.x, hoverRC.y, gunCol);
        }
        else if (currentWeapon == WeaponType.Torpedo)
        {
            foreach (var p in GetTorpPath(hoverRC, currentTorpDir))
                enemyGridView.PreviewCell(p.x, p.y, torpCol);
        }
        else if (currentWeapon == WeaponType.Bomb)
        {
            foreach (var p in GetArea2x2(hoverRC))
                enemyGridView.PreviewCell(p.x, p.y, bombCol);
        }
        else if (currentWeapon == WeaponType.Scout)
        {
            foreach (var p in GetArea2x2(hoverRC))
                enemyGridView.PreviewCell(p.x, p.y, scoutCol);
        }
    }

    private Vector2Int[] GetTorpPath(Vector2Int start, Dir4 dir)
    {
        const int LEN = 5;
        int dr = 0, dc = 0;
        switch (dir)
        {
            case Dir4.Up: dr = -1; break;
            case Dir4.Down: dr = 1; break;
            case Dir4.Left: dc = -1; break;
            case Dir4.Right: dc = 1; break;
        }

        var arr = new Vector2Int[LEN];
        for (int i = 0; i < LEN; i++)
            arr[i] = new Vector2Int(start.x + dr * i, start.y + dc * i);
        return arr;
    }

    private Vector2Int[] GetArea2x2(Vector2Int tl)
    {
        return new Vector2Int[]
        {
        new Vector2Int(tl.x,     tl.y),
        new Vector2Int(tl.x,     tl.y + 1),
        new Vector2Int(tl.x + 1, tl.y),
        new Vector2Int(tl.x + 1, tl.y + 1),
        };
    }

    private void OnHoverEnter(Vector2Int rc)
    {
        hasHover = true;
        hoverRC = rc;
        RedrawAll();
    }

    private void OnHoverExit(Vector2Int rc)
    {
        hasHover = false;
        RedrawAll();
    }

    // =======================
    // UI 接口（lq同学只需要调用这些，不需要直接改 plan / resolver）
    // =======================

    public void UI_SetWeapon(WeaponType w)
    {
        currentWeapon = w;
        if (config != null && config.enableDebugLogs) Debug.Log($"[UI] Weapon={w}");
        RedrawAll();
    }

    public void UI_SetTorpDir(Dir4 dir)
    {
        currentTorpDir = dir;
        if (config != null && config.enableDebugLogs) Debug.Log($"[UI] TorpDir={dir}");
        RedrawAll();
    }

    public void UI_Undo()
    {
        if (plan.TryPop(out var a))
        {
            if (config != null && config.enableDebugLogs) Debug.Log($"[UI] Undo {a.weapon} {a.anchor}");
            OnPlanChanged?.Invoke();
            RedrawAll();
        }
    }

    public void UI_ClearPlan()
    {
        plan.Clear();
        if (config != null && config.enableDebugLogs) Debug.Log("[UI] ClearPlan");
        OnPlanChanged?.Invoke();
        RedrawAll();
    }

    public void UI_Resolve()
    {
        ResolvePlan(); 
        OnPlanChanged?.Invoke();
    }
}