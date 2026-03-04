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

    public bool DebugMode = true;
    public bool DebugFixedSetup = true;

    public IAttackResolver resolver;
    // TODO(yjl): 当前仍可直接用 BattleResolver.Resolve
    // TODO(dyh): unity里面不能直接拖interface，所以这里只留一个注释。你需要在现在代码里面new一个来实现。未来在 Awake/Start 里 resolver = new AdvancedResolver();

    //wyx: 预留接口，由于目前是还未做回合切换暂时没用到。可以让玩家切换不同的 resolver
    private TurnPlan[] plans = new TurnPlan[2] { new TurnPlan(), new TurnPlan() };
    private int planningPlayerId = 0; // 当前正在规划的玩家：0 或 1
    private int EnemyOf(int pid) => 1 - pid;
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
            ResolveTurn();

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
    
    private void OnClickEnemyCell(Vector2Int rc) // 点击敌方格子时，根据当前选择的武器类型和（如果是鱼雷）方向，把对应的攻击动作加入计划，适应攻防切换模式
    {
        var curPlan = plans[planningPlayerId];

        if (currentWeapon == WeaponType.Torpedo)
            curPlan.Push(TurnAction.Torpedo(rc, currentTorpDir));
        else
            curPlan.Push(new TurnAction(currentWeapon, rc));

        Debug.Log($"P{planningPlayerId} Plan +1: {currentWeapon} at {rc} (planCount={curPlan.Count})");

        OnPlanChanged?.Invoke();
        RedrawAll();
    }
    //    private void OnClickEnemyCell(Vector2Int rc)
    //{
    //    var curPlan = plans[planningPlayerId];

    //    if (currentWeapon == WeaponType.Torpedo)
    //        curPlan.Push(TurnAction.Torpedo(rc, currentTorpDir));
    //    else
    //        curPlan.Push(new TurnAction(currentWeapon, rc));

    //    Debug.Log($"P{planningPlayerId} Plan +1: {currentWeapon} at {rc} (planCount={curPlan.Count})");

    //    OnPlanChanged?.Invoke();
    //    RedrawAll();
    //}

    public void UI_EndPlanningForCurrentPlayer()  //切换规划玩家（攻防切换时调用），也可以在 UI 里做个按钮来调用它
    {
        planningPlayerId = 1 - planningPlayerId;
        Debug.Log($"Now planning for P{planningPlayerId}");
        OnPlanChanged?.Invoke();
        RedrawAll();
    }
    //private void ResolvePlan()
    //{
    //    Debug.Log($"Resolve plan: total={plan.Count}  gun={plan.gun.Count} torp={plan.torp.Count} bomb={plan.bomb.Count} scout={plan.scout.Count}");

    //    // 你们的 demo 结算顺序：Gun -> Torpedo -> Bomb
    //    var enemyPending = GameManager.Instance.pending[1];

        
    //    foreach (var a in plan.GunSeq()) BattleResolver.Resolve(enemyBoard, playerView, enemyPending, a);
    //    foreach (var a in plan.TorpSeq()) BattleResolver.Resolve(enemyBoard, playerView, enemyPending, a);
    //    // foreach (var a in plan.GunSeq()) BattleResolver.Resolve(enemyBoard, playerView, a);
    //    // foreach (var a in plan.TorpSeq()) BattleResolver.Resolve(enemyBoard, playerView, a);
    //    foreach (var a in plan.BombSeq()) BattleResolver.Resolve(enemyBoard, playerView, enemyPending, a);
        
    //    foreach (var a in plan.ScoutSeq())  // 侦察没有 pending 和 board 变化，但我们也先画预览（等同于“已计划”状态），再正式结算（正式结算会把 intel 加到 playerView 里，UI 会显示）
    //    {
    //        var area = GetArea2x2(a.anchor);
    //        foreach (var p in area)
    //            enemyGridView.PreviewCell(p.x, p.y, new Color(1f, 1f, 0.4f, 1f)); // 黄
    //    }

    //    enemyBoard.CommitPending(enemyPending);
    //    if (enemyBoard.AllShipsSunk())
    //        Debug.Log("Enemy all ships sunk!");
    //    plan.Clear();

    //    OnPlanChanged?.Invoke();

    //    RedrawAll(); // 这会刷新到正式状态且没有预览
    //    Debug.Log("Resolve done. Plan cleared.");
        
    //}
    private void ResolveTurn() // 结算当前回合双方的计划（攻防切换时要结算双方计划），并进入下一回合（清空计划，重置状态，先手权切换等）。你们的 demo 结算顺序：Gun -> Torpedo -> Bomb -> Scout
    {
        var gm = GameManager.Instance;

        var b0 = gm.boards[0];
        var b1 = gm.boards[1];
        var v0 = gm.views[0];
        var v1 = gm.views[1];
        var pd0 = gm.pending[0]; // 记录对 board0 的伤害
        var pd1 = gm.pending[1]; // 记录对 board1 的伤害

        var p0 = plans[0];
        var p1 = plans[1];

        Debug.Log($"ResolveTurn: P0={p0.Count}, P1={p1.Count}");

        // Gun：P0打P1、P1打P0
        // Gun：P0打P1、P1打P0
        foreach (var a in p0.GunSeq())
        {
            bool ok = BattleResolver.Resolve(b1, v0, pd1, a);
            Debug.Log($"[DBG] P0 Gun {a.anchor} ok={ok} pd1WasShot={pd1.WasShot(a.anchor.x, a.anchor.y)} pd1WasHit={pd1.WasHit(a.anchor.x, a.anchor.y)}");
        }

        foreach (var a in p1.GunSeq())
        {
            bool ok = BattleResolver.Resolve(b0, v1, pd0, a);
            Debug.Log($"[DBG] P1 Gun {a.anchor} ok={ok} pd0WasShot={pd0.WasShot(a.anchor.x, a.anchor.y)} pd0WasHit={pd0.WasHit(a.anchor.x, a.anchor.y)}");
        }

        // Torp
        foreach (var a in p0.TorpSeq()) BattleResolver.Resolve(b1, v0, pd1, a);
        foreach (var a in p1.TorpSeq()) BattleResolver.Resolve(b0, v1, pd0, a);

        // Bomb
        foreach (var a in p0.BombSeq()) BattleResolver.Resolve(b1, v0, pd1, a);
        foreach (var a in p1.BombSeq()) BattleResolver.Resolve(b0, v1, pd0, a);

        // Scout（不落盘，只加情报）
        foreach (var a in p0.ScoutSeq()) BattleResolver.Resolve(b1, v0, pd1, a);
        foreach (var a in p1.ScoutSeq()) BattleResolver.Resolve(b0, v1, pd0, a);

        // 回合末落盘（包含 sunk 更新 + pd.Clear）
        b0.CommitPending(pd0);
        b1.CommitPending(pd1);

        Debug.Log($"[DBG] After commit: b1(0,0) hasShip={b1.truth[0, 0].hasShip} shipId={b1.truth[0, 0].shipId} wasShot={b1.truth[0, 0].wasShot} damaged={b1.truth[0, 0].isDamaged}");
        if (b1.ships != null && b1.ships.Count > 0)
            Debug.Log($"[DBG] After commit: b1 ship0 cells={b1.ships[0].cells.Count} sunk={b1.ships[0].sunk}");
        else
            Debug.Log("[DBG] After commit: b1 ships.Count==0");
        Debug.Log($"[DBG] After commit: b1(0,1) wasShot={b1.truth[0, 1].wasShot} damaged={b1.truth[0, 1].isDamaged}");
        Debug.Log($"[DBG] After commit: b1(5,5) wasShot={b1.truth[5, 5].wasShot} damaged={b1.truth[5, 5].isDamaged}");
        Debug.Log($"[DBG] After commit: b1(5,4) wasShot={b1.truth[5, 4].wasShot} damaged={b1.truth[5, 4].isDamaged}"); // 用来验证鱼雷扫过但不命中
        Debug.Log($"[DBG] After commit: b1(6,5) wasShot={b1.truth[6, 5].wasShot} damaged={b1.truth[6, 5].isDamaged}");
        // 清空双方计划，进入下一回合
        p0.Clear();
        p1.Clear();

        // 下一回合从 P0 开始规划（你们也可以交替先手）
        planningPlayerId = 0;

        OnPlanChanged?.Invoke();
        RedrawAll();
        Debug.Log("ResolveTurn done. Plans cleared.");
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

    private void DrawPlanPreviews() // 画当前计划的预览（点击后入栈），不同武器类型不同颜色，不同于悬停预览（更淡一些）。有了攻防切换，需要区分“已计划”预览和“悬停”预览，后者优先级更高。
    {
        var curPlan = plans[planningPlayerId];   // ✅ 关键：用当前正在规划的玩家 plan

        var gunCol = new Color(0.4f, 0.7f, 1f, 0.55f);
        var torpCol = new Color(0.7f, 0.7f, 0.7f, 0.55f);
        var bombCol = new Color(0.3f, 1f, 0.3f, 0.55f);
        var scoutCol = new Color(1f, 1f, 0.4f, 0.55f);

        foreach (var a in curPlan.GunSeq())
            enemyGridView.PreviewCell(a.anchor.x, a.anchor.y, gunCol);

        foreach (var a in curPlan.TorpSeq())
            foreach (var p in GetTorpPath(a.anchor, a.dir))
                enemyGridView.PreviewCell(p.x, p.y, torpCol);

        foreach (var a in curPlan.BombSeq())
            foreach (var p in GetArea2x2(a.anchor))
                enemyGridView.PreviewCell(p.x, p.y, bombCol);

        foreach (var a in curPlan.ScoutSeq())
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
        ResolveTurn();
        OnPlanChanged?.Invoke();
    }

   
}