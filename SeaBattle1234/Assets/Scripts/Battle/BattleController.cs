using TMPro;
using UnityEngine;
using static GameManager;

public class BattleController : MonoBehaviour
{
    public BattleEnemyGridView enemyGridView;
    [Header("Config (拖 GameConfig_Default 进来)")]
    public GameConfig config;

    public System.Action OnPlanChanged; // lq ; UI 面板监听（如果回合内攻击计划变化，就刷新列表/数量）

    private BoardModel enemyBoard;
    private PlayerViewModel playerView;
    private Dir4 currentTorpDir = Dir4.Right;

    private WeaponType currentWeapon = WeaponType.Gun;

    private bool hasHover;
    private Vector2Int hoverRC;

    public bool DebugMode = true;
    public bool DebugFixedSetup = true;

    public IAttackResolver resolver;
    // TODO(yjl): 当前仍可直接用 BattleResolver.Resolve
    // TODO(dyh): unity里面不能直接拖interface，所以这里只留一个注释。你需要在现在代码里面new一个来实现。未来在 Awake/Start 里 resolver = new AdvancedResolver();

    // wyx: 预留接口，由于目前是还未做回合切换暂时没用到。可以让玩家切换不同的 resolver
    private TurnPlan[] plans = new TurnPlan[2] { new TurnPlan(), new TurnPlan() };
    private int planningPlayerId = 0; // 当前正在规划的玩家：0 或 1
    private TurnPlan plan => plans[planningPlayerId];

    
    private int EnemyOf(int pid) => 1 - pid;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentWeapon = WeaponType.Gun; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentWeapon = WeaponType.Torpedo; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentWeapon = WeaponType.Bomb; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { currentWeapon = WeaponType.Scout; }
        if (Input.GetKeyDown(KeyCode.W)) { currentTorpDir = Dir4.Up; }
        if (Input.GetKeyDown(KeyCode.S)) { currentTorpDir = Dir4.Down; }
        if (Input.GetKeyDown(KeyCode.A)) { currentTorpDir = Dir4.Left; }
        if (Input.GetKeyDown(KeyCode.D)) { currentTorpDir = Dir4.Right; }

        //if (Input.GetKeyDown(KeyCode.Space))
        //    ResolveTurn();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (plan.TryPop(out var a))
            {
                OnPlanChanged?.Invoke();
                RedrawAll();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            plan.Clear();
            RedrawAll();
        }
    }

    private void OnClickEnemyCell(Vector2Int rc)
    {
        bool ok = false;

        if (currentWeapon == WeaponType.Torpedo)
        {
            ok = AddPlannedTorpedo(planningPlayerId, rc, currentTorpDir);
        }
        else
        {
            ok = AddPlannedAction(planningPlayerId, currentWeapon, rc);
        }

        if (!ok)
        {
            Debug.LogWarning($"[Battle] P{planningPlayerId} cannot add more {currentWeapon}. Quota reached.");
        }
    }

    public void UI_EndPlanningForCurrentPlayer()  // 切换规划玩家（攻防切换时调用），也可以在 UI 里做个按钮来调用它
    {
        planningPlayerId = 1 - planningPlayerId;
        OnPlanChanged?.Invoke();
        RedrawAll();
    }

    public void ResolveTurn() // 结算当前回合双方的计划（攻防切换时要结算双方计划），并进入下一回合（清空计划，重置状态，先手权切换等）。你们的 demo 结算顺序：Gun -> Torpedo -> Bomb -> Scout
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

        // Gun：P0打P1、P1打P0
        foreach (var a in p0.GunSeq())
        {
            BattleResolver.Resolve(b1, v0, pd1, a);
        }

        foreach (var a in p1.GunSeq())
        {
            BattleResolver.Resolve(b0, v1, pd0, a);
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

        // 清空双方计划，进入下一回合
        p0.Clear();
        p1.Clear();

        // 下一回合从 P0 开始规划（你们也可以交替先手）
        planningPlayerId = 0;

        OnPlanChanged?.Invoke();
        RedrawAll();
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

    public bool AddPlannedAction(int attackerId, WeaponType wpn, Vector2Int anchor)
    {
        if (!CanPlanWeapon(attackerId, wpn))
        {
            Debug.LogWarning($"[Battle] P{attackerId} cannot plan more {wpn}. quota={GetWeaponQuota(attackerId, wpn)} used={GetPlannedCountPublic(attackerId, wpn)}");
            return false;
        }

        plans[attackerId].Push(new TurnAction(wpn, anchor));

        var flow = FindObjectOfType<BattleFlowController>();
        if (flow != null)
            flow.ResetUnderfillConfirm(attackerId);

        OnPlanChanged?.Invoke();
        RedrawAll();

        Debug.Log($"[Battle] P{attackerId} planned {wpn} at {anchor}. used={GetPlannedCountPublic(attackerId, wpn)}/{GetWeaponQuota(attackerId, wpn)}");
        return true;
    }

    public bool AddPlannedTorpedo(int attackerId, Vector2Int start, Dir4 dir)
    {
        if (!CanPlanWeapon(attackerId, WeaponType.Torpedo))
        {
            Debug.LogWarning($"[Battle] P{attackerId} cannot plan more Torpedo. quota={GetWeaponQuota(attackerId, WeaponType.Torpedo)} used={GetPlannedCountPublic(attackerId, WeaponType.Torpedo)}");
            return false;
        }

        plans[attackerId].Push(TurnAction.Torpedo(start, dir));

        var flow = FindObjectOfType<BattleFlowController>();
        if (flow != null)
            flow.ResetUnderfillConfirm(attackerId);

        OnPlanChanged?.Invoke();
        RedrawAll();

        Debug.Log($"[Battle] P{attackerId} planned Torpedo at {start} dir={dir}. used={GetPlannedCountPublic(attackerId, WeaponType.Torpedo)}/{GetWeaponQuota(attackerId, WeaponType.Torpedo)}");
        return true;
    }

    public void SetContext(int planningPid, BoardModel enemyBoard, PlayerViewModel playerView, BattleEnemyGridView grid)
    {
        this.planningPlayerId = planningPid;
        this.enemyBoard = enemyBoard;
        this.playerView = playerView;
        this.enemyGridView = grid;

        // 重新绑定点击/hover
        enemyGridView.Bind(OnClickEnemyCell);
        enemyGridView.BindHover(OnHoverEnter, OnHoverExit);

        RedrawAll();
    }

    public void ResolveTurnPublic()
    {
        ResolveTurn();
    }

    int GetPlannedCount(int pid, WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Gun: return plans[pid].gun.Count;
            case WeaponType.Torpedo: return plans[pid].torp.Count;
            case WeaponType.Bomb: return plans[pid].bomb.Count;
            case WeaponType.Scout: return plans[pid].scout.Count;
            default: return 0;
        }
    }

    bool CanPlanWeapon(int pid, WeaponType weapon)
    {
        int quota = GetWeaponQuota(pid, weapon);
        int used = GetPlannedCount(pid, weapon);
        return used < quota;
    }

    public int GetPlannedCountPublic(int pid, WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Gun: return plans[pid].gun.Count;
            case WeaponType.Torpedo: return plans[pid].torp.Count;
            case WeaponType.Bomb: return plans[pid].bomb.Count;
            case WeaponType.Scout: return plans[pid].scout.Count;
            default: return 0;
        }
    }

    int GetWeaponQuota(int pid, WeaponType weapon)
    {
        var board = GameManager.Instance.boards[pid];
        int total = 0;

        foreach (var ship in board.ships)
        {
            if (ship.sunk) continue;

            string name = ShipCatalog.Types[ship.typeId].name;

            switch (name)
            {
                case "Frigate":
                    if (weapon == WeaponType.Gun) total += 1;
                    break;

                case "Torpedo Boat":
                    if (weapon == WeaponType.Torpedo) total += 1;
                    break;

                case "Destroyer":
                    if (weapon == WeaponType.Gun) total += 1;
                    if (weapon == WeaponType.Torpedo) total += 1;
                    break;

                case "Cruiser":
                    if (weapon == WeaponType.Gun) total += 3;
                    break;

                case "Heavy Cruiser":
                    if (weapon == WeaponType.Gun) total += 4;
                    break;

                case "Light Cruiser":
                    if (weapon == WeaponType.Gun) total += 2;
                    if (weapon == WeaponType.Torpedo) total += 1;
                    break;

                case "Battlecruiser":
                    if (weapon == WeaponType.Gun) total += 4;
                    if (weapon == WeaponType.Torpedo) total += 1;
                    break;

                case "Battleship":
                    if (weapon == WeaponType.Gun) total += 6;
                    break;

                case "Aviation Battleship I":
                    if (weapon == WeaponType.Gun) total += 3;
                    if (weapon == WeaponType.Scout) total += 1;
                    break;

                case "Aviation Battleship II":
                    if (weapon == WeaponType.Gun) total += 2;
                    if (weapon == WeaponType.Bomb) total += 1;
                    break;

                case "Armored Battleship":
                    if (weapon == WeaponType.Gun) total += 5;
                    break;

                case "Escort Carrier":
                    if (weapon == WeaponType.Bomb) total += 1;
                    if (weapon == WeaponType.Scout) total += 1;
                    break;

                case "Carrier":
                    if (weapon == WeaponType.Bomb) total += 2;
                    if (weapon == WeaponType.Torpedo) total += 1;
                    if (weapon == WeaponType.Scout) total += 1;
                    break;

                default:
                    break;
            }
        }

        return total;
    }

    public int GetWeaponQuotaPublic(int pid, WeaponType weapon)
    {
        return GetWeaponQuota(pid, weapon);
    }

    public bool ValidatePlanCounts(int pid, out string msg, out bool hasOverflow, out bool hasUnderflow)
    {
        hasOverflow = false;
        hasUnderflow = false;
        msg = "";

        int quota, used;

        // Gun
        quota = GetWeaponQuotaPublic(pid, WeaponType.Gun);
        used = GetPlannedCountPublic(pid, WeaponType.Gun);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[火炮] 超出配额：已计划 {used} / 允许 {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[火炮] 尚未用满：已计划 {used} / 允许 {quota}\n";
        }

        // Torpedo
        quota = GetWeaponQuotaPublic(pid, WeaponType.Torpedo);
        used = GetPlannedCountPublic(pid, WeaponType.Torpedo);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[鱼雷] 超出配额：已计划 {used} / 允许 {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[鱼雷] 尚未用满：已计划 {used} / 允许 {quota}\n";
        }

        // Bomb
        quota = GetWeaponQuotaPublic(pid, WeaponType.Bomb);
        used = GetPlannedCountPublic(pid, WeaponType.Bomb);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[炸弹] 超出配额：已计划 {used} / 允许 {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[炸弹] 尚未用满：已计划 {used} / 允许 {quota}\n";
        }

        // Scout
        quota = GetWeaponQuotaPublic(pid, WeaponType.Scout);
        used = GetPlannedCountPublic(pid, WeaponType.Scout);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[侦察] 超出配额：已计划 {used} / 允许 {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[侦察] 尚未用满：已计划 {used} / 允许 {quota}\n";
        }

        return !hasOverflow;
    }

    public TMP_Text gunText;
    public TMP_Text torpText;
    public TMP_Text bombText;
    public TMP_Text scoutText;

    public TMP_Text warningText;

    public void RefreshAttackList(int pid)
    {
        int gunUsed = GetPlannedCountPublic(pid, WeaponType.Gun);
        int gunMax = GetWeaponQuotaPublic(pid, WeaponType.Gun);

        int torpUsed = GetPlannedCountPublic(pid, WeaponType.Torpedo);
        int torpMax = GetWeaponQuotaPublic(pid, WeaponType.Torpedo);

        int bombUsed = GetPlannedCountPublic(pid, WeaponType.Bomb);
        int bombMax = GetWeaponQuotaPublic(pid, WeaponType.Bomb);

        int scoutUsed = GetPlannedCountPublic(pid, WeaponType.Scout);
        int scoutMax = GetWeaponQuotaPublic(pid, WeaponType.Scout);

        gunText.text = $"Gun: {gunUsed} / {gunMax}";
        torpText.text = $"Torpedo: {torpUsed} / {torpMax}";
        bombText.text = $"Bomb: {bombUsed} / {bombMax}";
        scoutText.text = $"Scout: {scoutUsed} / {scoutMax}";

        bool hasUnused =
            gunUsed < gunMax ||
            torpUsed < torpMax ||
            bombUsed < bombMax ||
            scoutUsed < scoutMax;

        warningText.gameObject.SetActive(hasUnused);
    }
    
}