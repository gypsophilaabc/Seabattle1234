using BattleSystem;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BattleController : MonoBehaviour
{
    public BattleEnemyGridView enemyGridView;
    [Header("Config (拖 GameConfig_Default 进来)")]
    public GameConfig config;

    [Header("Resolver (拖 AdvancedResolver 组件进来)")]
    public MonoBehaviour resolverBehaviour;
    private IAttackResolver resolver;

    public System.Action OnPlanChanged;

    private BoardModel enemyBoard;
    private PlayerViewModel originalPlayerView;        // 原来的类型（用于UI）- 使用CellIntelFlags
    private BattleSystem.PlayerViewModel battlePlayerView;  // BattleSystem类型（用于Resolver）
    private Dir4 currentTorpDir = Dir4.Right;

    private TurnPlan plan = new TurnPlan();
    private WeaponType currentWeapon = WeaponType.Gun;

    private bool hasHover;
    private Vector2Int hoverRC;

    void Start()
    {
        enemyBoard = GameManager.Instance.boards[1];
        originalPlayerView = GameManager.Instance.views[0];  // 保存原来的

        // 创建 BattleSystem.PlayerViewModel 
        battlePlayerView = new BattleSystem.PlayerViewModel();
        // 初始化IntelData字典
        battlePlayerView.IntelData = new Dictionary<IntelType, bool[,]>();

        // 为每种IntelType初始化数组
        foreach (IntelType type in System.Enum.GetValues(typeof(IntelType)))
        {
            battlePlayerView.IntelData[type] = new bool[16, 20];
        }

        if (resolverBehaviour != null)
        {
            resolver = resolverBehaviour as IAttackResolver;
            if (resolver == null)
            {
                Debug.LogError("resolverBehaviour 没有实现 IAttackResolver 接口！");
            }
        }
        else
        {
            resolverBehaviour = GetComponent<MonoBehaviour>() as MonoBehaviour;
            if (resolverBehaviour != null)
            {
                resolver = resolverBehaviour as IAttackResolver;
            }

            if (resolver == null)
            {
                Debug.LogWarning("未找到 resolver，创建默认 AdvancedResolver");
                var advancedResolver = gameObject.AddComponent<AdvancedResolver>();
                resolver = advancedResolver;
                resolverBehaviour = advancedResolver;
            }
        }

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

        if (resolver == null)
        {
            Debug.LogError("Resolver is null! 无法结算");
            return;
        }

        var attackAction = new AttackAction();

        foreach (var a in plan.GunSeq())
        {
            attackAction.Guns.Add(new GunTarget { pos = a.anchor });
        }

        foreach (var a in plan.TorpSeq())
        {
            attackAction.Torpedoes.Add(new Torpedo
            {
                start = a.anchor,
                dir = ConvertDir(a.dir)
            });
        }

        foreach (var a in plan.BombSeq())
        {
            attackAction.Bombs.Add(new BombTarget { topLeft = a.anchor });
        }

        foreach (var a in plan.ScoutSeq())
        {
            attackAction.Scouts.Add(new ScoutTarget { topLeft = a.anchor });
        }

        // 转换 CellTruth[,] 到 BattleSystem.BoardCell[,]
        var boardCells = ConvertToBoardCell(enemyBoard.truth);

        // 使用 battlePlayerView 结算
        var result = resolver.Resolve(boardCells, battlePlayerView, attackAction);

        Debug.Log($"结算完成: 总命中={result.TotalHits}, 击沉={result.SunkShipIds.Count}艘");
        foreach (var log in result.LogMessages)
        {
            Debug.Log(log);
        }

        // 将结算结果同步回 enemyBoard.truth
        SyncBackToTruth(boardCells, enemyBoard.truth);

        // 将 battlePlayerView 的数据同步回 originalPlayerView (使用CellIntelFlags)
        SyncBackToOriginalView();

        plan.Clear();
        OnPlanChanged?.Invoke();
        RedrawAll();

        Debug.Log("Resolve done. Plan cleared.");
    }

    private Direction ConvertDir(Dir4 dir)
    {
        return dir switch
        {
            Dir4.Up => Direction.Up,
            Dir4.Down => Direction.Down,
            Dir4.Left => Direction.Left,
            Dir4.Right => Direction.Right,
            _ => Direction.Right
        };
    }

    private BattleSystem.BoardCell[,] ConvertToBoardCell(CellTruth[,] truth)
    {
        var result = new BattleSystem.BoardCell[16, 20];
        for (int r = 0; r < 16; r++)
        {
            for (int c = 0; c < 20; c++)
            {
                result[r, c] = new BattleSystem.BoardCell
                {
                    ShipId = truth[r, c].shipId,
                    WasShot = truth[r, c].wasShot,
                    IsDamaged = truth[r, c].isDamaged
                };
            }
        }
        return result;
    }

    private void SyncBackToTruth(BattleSystem.BoardCell[,] boardCells, CellTruth[,] truth)
    {
        for (int r = 0; r < 16; r++)
        {
            for (int c = 0; c < 20; c++)
            {
                truth[r, c].wasShot = boardCells[r, c].WasShot;
                truth[r, c].isDamaged = boardCells[r, c].IsDamaged;
            }
        }
    }

    private void SyncBackToOriginalView()
    {
        // 将 battlePlayerView 的Intel数据同步回 originalPlayerView 的intel数组
        for (int r = 0; r < 16; r++)
        {
            for (int c = 0; c < 20; c++)
            {
                CellIntelFlags flags = 0;

                // 根据battlePlayerView的IntelData设置对应的flags
                if (battlePlayerView.IntelData != null)
                {
                    if (battlePlayerView.IntelData.ContainsKey(IntelType.GunShot) &&
                        battlePlayerView.IntelData[IntelType.GunShot][r, c])
                        flags |= CellIntelFlags.GunShot;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.GunHit) &&
                        battlePlayerView.IntelData[IntelType.GunHit][r, c])
                        flags |= CellIntelFlags.GunHit;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.TorpLine) &&
                        battlePlayerView.IntelData[IntelType.TorpLine][r, c])
                        flags |= CellIntelFlags.TorpLine;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.TorpHitLine) &&
                        battlePlayerView.IntelData[IntelType.TorpHitLine][r, c])
                        flags |= CellIntelFlags.TorpHitLine;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.BombArea) &&
                        battlePlayerView.IntelData[IntelType.BombArea][r, c])
                        flags |= CellIntelFlags.BombArea;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.BombHit) &&
                        battlePlayerView.IntelData[IntelType.BombHit][r, c])
                        flags |= CellIntelFlags.BombHit;

                    if (battlePlayerView.IntelData.ContainsKey(IntelType.Scout) &&
                        battlePlayerView.IntelData[IntelType.Scout][r, c])
                        flags |= CellIntelFlags.Scout;
                }

                originalPlayerView.intel[r, c] = flags;
            }
        }
    }

    private void RedrawAll()
    {
        // 使用 originalPlayerView 来刷新UI
        enemyGridView.Refresh(enemyBoard, originalPlayerView);
        DrawPlanPreviews();
        if (hasHover)
            DrawHoverPreview();
    }

    private void DrawPlanPreviews()
    {
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