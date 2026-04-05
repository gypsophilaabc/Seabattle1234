using UnityEngine;

public class BattleController : MonoBehaviour
{
    public BattleEnemyGridView enemyGridView;

    [Header("Config (拖 GameConfig_Default 进来)")]
    public GameConfig config;

    public System.Action OnPlanChanged;

    private BoardModel enemyBoard;
    private PlayerViewModel playerView;
    private Dir4 currentTorpDir = Dir4.Right;
    private WeaponType currentWeapon = WeaponType.Gun;

    private bool hasHover;
    private Vector2Int hoverRC;

    public bool DebugMode = true;
    public bool DebugFixedSetup = true;

    public IAttackResolver resolver;

    private TurnPlan[] plans = new TurnPlan[2] { new TurnPlan(), new TurnPlan() };
    private int planningPlayerId = 0;
    private TurnPlan plan => plans[planningPlayerId];

    [Header("Preview Sprites - Green")]
    public Sprite gunPreviewSprite;
    

    public Sprite[] torpedoRightPreviewSprites = new Sprite[5];
    public Sprite[] torpedoLeftPreviewSprites = new Sprite[5];
    public Sprite[] torpedoUpPreviewSprites = new Sprite[5];
    public Sprite[] torpedoDownPreviewSprites = new Sprite[5];

    private const float HoverPreviewAlpha = 0.5f;
    private const float PlannedPreviewAlpha = 0.75f;

    [Header("Bomb Preview Sprites")]
    [SerializeField] private Sprite bombPreviewTL;
    [SerializeField] private Sprite bombPreviewTR;
    [SerializeField] private Sprite bombPreviewBL;
    [SerializeField] private Sprite bombPreviewBR;

    [Header("Scout Preview Sprites")]
    [SerializeField] private Sprite scoutPreviewTL;
    [SerializeField] private Sprite scoutPreviewTR;
    [SerializeField] private Sprite scoutPreviewBL;
    [SerializeField] private Sprite scoutPreviewBR;




    public enum QuadPart
    {
        TL,
        TR,
        BL,
        BR
    }
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeapon = WeaponType.Gun;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentWeapon = WeaponType.Torpedo;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentWeapon = WeaponType.Bomb;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentWeapon = WeaponType.Scout;

        if (Input.GetKeyDown(KeyCode.W)) currentTorpDir = Dir4.Up;
        if (Input.GetKeyDown(KeyCode.S)) currentTorpDir = Dir4.Down;
        if (Input.GetKeyDown(KeyCode.A)) currentTorpDir = Dir4.Left;
        if (Input.GetKeyDown(KeyCode.D)) currentTorpDir = Dir4.Right;

        if (Input.GetKeyDown(KeyCode.Backspace))
            UI_Undo();

        if (Input.GetKeyDown(KeyCode.C))
            UI_ClearPlan();
    }

    private void OnClickEnemyCell(Vector2Int rc)
    {
        bool ok;

        if (currentWeapon == WeaponType.Torpedo)
            ok = AddPlannedTorpedo(planningPlayerId, rc, currentTorpDir);
        else
            ok = AddPlannedAction(planningPlayerId, currentWeapon, rc);

        // 这里不再统一弹“at limit”提示
        // 具体错误（重复 / 超额）由 AddPlannedAction / AddPlannedTorpedo 自己提示
        if (!ok)
            return;
    }

    public void UI_EndPlanningForCurrentPlayer()
    {
        hasHover = false;
        planningPlayerId = 1 - planningPlayerId;

        OnPlanChanged?.Invoke();
        RedrawAll();
    }

    public void ResolveTurn()
    {
        hasHover = false;

        var gm = GameManager.Instance;

        var b0 = gm.boards[0];
        var b1 = gm.boards[1];
        var v0 = gm.views[0];
        var v1 = gm.views[1];
        var pd0 = gm.pending[0];
        var pd1 = gm.pending[1];

        var p0 = plans[0];
        var p1 = plans[1];

        foreach (var a in p0.GunSeq())
            BattleResolver.Resolve(b1, v0, pd1, a);

        foreach (var a in p1.GunSeq())
            BattleResolver.Resolve(b0, v1, pd0, a);

        foreach (var a in p0.TorpSeq())
            BattleResolver.Resolve(b1, v0, pd1, a);

        foreach (var a in p1.TorpSeq())
            BattleResolver.Resolve(b0, v1, pd0, a);

        foreach (var a in p0.BombSeq())
            BattleResolver.Resolve(b1, v0, pd1, a);

        foreach (var a in p1.BombSeq())
            BattleResolver.Resolve(b0, v1, pd0, a);

        foreach (var a in p0.ScoutSeq())
            BattleResolver.Resolve(b1, v0, pd1, a);

        foreach (var a in p1.ScoutSeq())
            BattleResolver.Resolve(b0, v1, pd0, a);

        b0.CommitPending(pd0);
        b1.CommitPending(pd1);

        p0.Clear();
        p1.Clear();

        planningPlayerId = 0;

        OnPlanChanged?.Invoke();
        RedrawAll();
    }

    private void RedrawAll()
    {
        if (enemyGridView == null || enemyBoard == null || playerView == null) return;

        enemyGridView.Refresh(enemyBoard, playerView);
        enemyGridView.ClearAllPreviews();

        DrawPlanPreviews();

        if (hasHover)
            DrawHoverPreview();
    }

    private void DrawPlanPreviews()
    {
        var curPlan = plans[planningPlayerId];

        foreach (var a in curPlan.GunSeq())
            enemyGridView.PreviewCellSprite(a.anchor.x, a.anchor.y, gunPreviewSprite, PlannedPreviewAlpha);

        foreach (var a in curPlan.TorpSeq())
        {
            var path = GetTorpPath(a.anchor, a.dir);
            for (int i = 0; i < path.Length; i++)
            {
                var p = path[i];
                Sprite s = GetTorpedoPreviewSprite(a.dir, i);
                enemyGridView.PreviewCellSprite(p.x, p.y, s, PlannedPreviewAlpha);
            }
        }

        foreach (var a in curPlan.BombSeq())
        {
            int r = a.anchor.x;
            int c = a.anchor.y;

            enemyGridView.PreviewCellSprite(r, c, GetBombPreviewSprite(QuadPart.TL), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r, c + 1, GetBombPreviewSprite(QuadPart.TR), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r + 1, c, GetBombPreviewSprite(QuadPart.BL), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r + 1, c + 1, GetBombPreviewSprite(QuadPart.BR), PlannedPreviewAlpha);
        }

        foreach (var a in curPlan.ScoutSeq())
        {
            int r = a.anchor.x;
            int c = a.anchor.y;

            enemyGridView.PreviewCellSprite(r, c, GetScoutPreviewSprite(QuadPart.TL), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r, c + 1, GetScoutPreviewSprite(QuadPart.TR), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r + 1, c, GetScoutPreviewSprite(QuadPart.BL), PlannedPreviewAlpha);
            enemyGridView.PreviewCellSprite(r + 1, c + 1, GetScoutPreviewSprite(QuadPart.BR), PlannedPreviewAlpha);
        }
    }

    private void DrawHoverPreview()
    {
        if (currentWeapon == WeaponType.Gun)
        {
            enemyGridView.PreviewCellSprite(hoverRC.x, hoverRC.y, gunPreviewSprite, HoverPreviewAlpha);
        }
        else if (currentWeapon == WeaponType.Torpedo)
        {
            var path = GetTorpPath(hoverRC, currentTorpDir);
            for (int i = 0; i < path.Length; i++)
            {
                var p = path[i];
                Sprite s = GetTorpedoPreviewSprite(currentTorpDir, i);
                enemyGridView.PreviewCellSprite(p.x, p.y, s, HoverPreviewAlpha);
            }
        }
        else if (currentWeapon == WeaponType.Bomb)
        {
            enemyGridView.PreviewCellSprite(hoverRC.x, hoverRC.y, GetBombPreviewSprite(QuadPart.TL), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x, hoverRC.y + 1, GetBombPreviewSprite(QuadPart.TR), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x + 1, hoverRC.y, GetBombPreviewSprite(QuadPart.BL), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x + 1, hoverRC.y + 1, GetBombPreviewSprite(QuadPart.BR), HoverPreviewAlpha);
        }
        else if (currentWeapon == WeaponType.Scout)
        {
            enemyGridView.PreviewCellSprite(hoverRC.x, hoverRC.y, GetScoutPreviewSprite(QuadPart.TL), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x, hoverRC.y + 1, GetScoutPreviewSprite(QuadPart.TR), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x + 1, hoverRC.y, GetScoutPreviewSprite(QuadPart.BL), HoverPreviewAlpha);
            enemyGridView.PreviewCellSprite(hoverRC.x + 1, hoverRC.y + 1, GetScoutPreviewSprite(QuadPart.BR), HoverPreviewAlpha);
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
            new Vector2Int(tl.x, tl.y),
            new Vector2Int(tl.x, tl.y + 1),
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

    public void UI_SetWeapon(WeaponType w)
    {
        currentWeapon = w;

        if (config != null && config.enableDebugLogs)
            Debug.Log($"[UI] Weapon={w}");

        RedrawAll();
    }

    public void UI_SetTorpDir(Dir4 dir)
    {
        currentTorpDir = dir;

        if (config != null && config.enableDebugLogs)
            Debug.Log($"[UI] TorpDir={dir}");

        RedrawAll();
    }

    public void UI_Resolve()
    {
        ResolveTurn();
        OnPlanChanged?.Invoke();
    }

    private bool HasPlannedSameCell(int pid, WeaponType weapon, Vector2Int anchor)
    {
        switch (weapon)
        {
            case WeaponType.Gun:
                foreach (var a in plans[pid].gun)
                    if (a.anchor == anchor) return true;
                return false;

            case WeaponType.Bomb:
                foreach (var a in plans[pid].bomb)
                    if (a.anchor == anchor) return true;
                return false;

            case WeaponType.Scout:
                foreach (var a in plans[pid].scout)
                    if (a.anchor == anchor) return true;
                return false;

            default:
                return false;
        }
    }

    private bool HasPlannedSameTorpedo(int pid, Vector2Int start, Dir4 dir)
    {
        foreach (var a in plans[pid].torp)
        {
            if (a.anchor == start && a.dir == dir)
                return true;
        }
        return false;
    }

    public bool AddPlannedAction(int attackerId, WeaponType wpn, Vector2Int anchor)
    {
        if (HasPlannedSameCell(attackerId, wpn, anchor))
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning($"{WeaponDisplayName(wpn)} already planned at ({anchor.x},{anchor.y}).");

            Debug.LogWarning($"[Battle] Duplicate planned {wpn} at {anchor} by P{attackerId}");
            return false;
        }

        if (!CanPlanWeapon(attackerId, wpn))
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning($"{WeaponDisplayName(wpn)} is already at limit: {GetPlannedCountPublic(attackerId, wpn)}/{GetWeaponQuota(attackerId, wpn)}");

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
        if (HasPlannedSameTorpedo(attackerId, start, dir))
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning($"Torpedo already planned at ({start.x},{start.y}) dir={dir}.");

            Debug.LogWarning($"[Battle] Duplicate planned Torpedo at {start} dir={dir} by P{attackerId}");
            return false;
        }

        if (!CanPlanWeapon(attackerId, WeaponType.Torpedo))
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning($"Torpedo is already at limit: {GetPlannedCountPublic(attackerId, WeaponType.Torpedo)}/{GetWeaponQuota(attackerId, WeaponType.Torpedo)}");

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

            ShipWeaponLoadout loadout = ShipWeaponCatalog.GetLoadout(ship.typeId);

            switch (weapon)
            {
                case WeaponType.Gun:
                    total += loadout.gun;
                    break;
                case WeaponType.Torpedo:
                    total += loadout.torpedo;
                    break;
                case WeaponType.Bomb:
                    total += loadout.bomb;
                    break;
                case WeaponType.Scout:
                    total += loadout.scout;
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

        quota = GetWeaponQuotaPublic(pid, WeaponType.Gun);
        used = GetPlannedCountPublic(pid, WeaponType.Gun);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[Gun] Overflow: planned {used} / allowed {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[Gun] Underused: planned {used} / allowed {quota}\n";
        }

        quota = GetWeaponQuotaPublic(pid, WeaponType.Torpedo);
        used = GetPlannedCountPublic(pid, WeaponType.Torpedo);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[Torpedo] Overflow: planned {used} / allowed {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[Torpedo] Underused: planned {used} / allowed {quota}\n";
        }

        quota = GetWeaponQuotaPublic(pid, WeaponType.Bomb);
        used = GetPlannedCountPublic(pid, WeaponType.Bomb);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[Bomb] Overflow: planned {used} / allowed {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[Bomb] Underused: planned {used} / allowed {quota}\n";
        }

        quota = GetWeaponQuotaPublic(pid, WeaponType.Scout);
        used = GetPlannedCountPublic(pid, WeaponType.Scout);
        if (used > quota)
        {
            hasOverflow = true;
            msg += $"[Scout] Overflow: planned {used} / allowed {quota}\n";
        }
        else if (quota > 0 && used < quota)
        {
            hasUnderflow = true;
            msg += $"[Scout] Underused: planned {used} / allowed {quota}\n";
        }

        return !hasOverflow;
    }

    public void ShowResolvedBoards(BattleEnemyGridView grid0, BattleEnemyGridView grid1)
    {
        var gm = GameManager.Instance;

        if (grid0 != null)
        {
            grid0.ClearAllPreviews();
            grid0.Refresh(gm.boards[1], gm.views[0]);
        }

        if (grid1 != null)
        {
            grid1.ClearAllPreviews();
            grid1.Refresh(gm.boards[0], gm.views[1]);
        }
    }

    string WeaponDisplayName(WeaponType w)
    {
        switch (w)
        {
            case WeaponType.Gun: return "Gun";
            case WeaponType.Torpedo: return "Torpedo";
            case WeaponType.Bomb: return "Bomb";
            case WeaponType.Scout: return "Scout";
            default: return w.ToString();
        }
    }

    string FormatTurnAction(TurnAction a)
    {
        if (a.weapon == WeaponType.Torpedo)
            return $"{WeaponDisplayName(a.weapon)} at ({a.anchor.x},{a.anchor.y}), dir={a.dir}";
        else
            return $"{WeaponDisplayName(a.weapon)} at ({a.anchor.x},{a.anchor.y})";
    }

    public void UI_Undo()
    {
        if (plan.TryPop(out var a))
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
            {
                string msg = $"Undo: {FormatTurnAction(a)}";
                Debug.Log("[UndoWarning] " + msg);
                hud.ShowWarning(msg);
            }

            if (config != null && config.enableDebugLogs)
                Debug.Log($"[UI] Undo {a.weapon} {a.anchor}");

            OnPlanChanged?.Invoke();
            RedrawAll();
        }
        else
        {
            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning("Nothing to undo.");
        }
    }

    public void UI_ClearPlan()
    {
        bool hadAnyPlan =
            plans[planningPlayerId].gun.Count > 0 ||
            plans[planningPlayerId].torp.Count > 0 ||
            plans[planningPlayerId].bomb.Count > 0 ||
            plans[planningPlayerId].scout.Count > 0;

        plan.Clear();

        var hud = FindObjectOfType<BattleHUDController>();
        if (hud != null)
        {
            string msg = hadAnyPlan ? "Current attack plan cleared." : "No planned attack to clear.";
            Debug.Log("[ClearWarning] " + msg);
            hud.ShowWarning(msg);
        }

        if (config != null && config.enableDebugLogs)
            Debug.Log("[UI] ClearPlan");

        OnPlanChanged?.Invoke();
        RedrawAll();
    }

    private Sprite GetTorpedoPreviewSprite(Dir4 dir, int index)
    {
        if (index < 0 || index >= 5) return null;

        switch (dir)
        {
            case Dir4.Right:
                return torpedoRightPreviewSprites[index];

            case Dir4.Left:
                return torpedoLeftPreviewSprites[index];

            case Dir4.Up:
                return torpedoDownPreviewSprites[index];   // 先故意交换

            case Dir4.Down:
                return torpedoUpPreviewSprites[index];     // 先故意交换

            default:
                return null;
        }
    }
    private Sprite GetBombPreviewSprite(QuadPart part)
    {
        switch (part)
        {
            case QuadPart.TL: return bombPreviewTL;
            case QuadPart.TR: return bombPreviewTR;
            case QuadPart.BL: return bombPreviewBL;
            case QuadPart.BR: return bombPreviewBR;
            default: return null;
        }
    }

    private Sprite GetScoutPreviewSprite(QuadPart part)
    {
        switch (part)
        {
            case QuadPart.TL: return scoutPreviewTL;
            case QuadPart.TR: return scoutPreviewTR;
            case QuadPart.BL: return scoutPreviewBL;
            case QuadPart.BR: return scoutPreviewBR;
            default: return null;
        }
    }
}
