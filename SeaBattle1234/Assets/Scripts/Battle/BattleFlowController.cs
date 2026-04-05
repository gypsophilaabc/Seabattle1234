using UnityEngine;
using static GameManager;

public class BattleFlowController : MonoBehaviour
{
    [Header("References")]
    public BattleController battle;
    public BattleEnemyGridView enemyGrid0; // 给 P0 用（攻击 P1）
    public BattleEnemyGridView enemyGrid1; // 给 P1 用（攻击 P0）
    private bool[] underfillConfirmed = new bool[2];
    private bool[] nextRoundReady = new bool[2];

    public int p0LossThisRound = 0;
    public int p1LossThisRound = 0;

    public GameOverController gameOverUI;


    void Start()
    {
        var gm = GameManager.Instance;

        // 防御：如果有人直接从Battle场景进来
        if (gm.phase != GamePhase.BattlePlanningP0 && gm.phase != GamePhase.BattlePlanningP1)
        {
            gm.phase = GamePhase.BattlePlanningP0;
            gm.activePlayerId = 0;
            gm.ready[0] = gm.ready[1] = false;
        }

        Debug.Log($"[BattleFlow] Start phase={gm.phase}.");
        ApplyPhase();
    }

    //void Update()
    //{
    //    var gm = GameManager.Instance;

    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        int pid = gm.activePlayerId;

    //        // 先做计划合法性检查
    //        bool ok = battle.ValidatePlanCounts(pid, out string msg, out bool hasOverflow, out bool hasUnderflow);

    //        // 1) 溢出：直接拦住，不能 ready
    //        if (hasOverflow)
    //        {
    //            Debug.LogWarning($"[BattleFlow] P{pid} 计划非法，无法进入结算：\n{msg}");
    //            return;
    //        }

    //        // 2) 未用满：第一次按R只提示，第二次再放行
    //        if (hasUnderflow && !IsUnderfillConfirmed(pid))
    //        {
    //            ConfirmUnderfill(pid);
    //            Debug.LogWarning($"[BattleFlow] P{pid} 还有武器未使用，确认后再次按 R 继续：\n{msg}");
    //            return;
    //        }

    //        // 真正 ready
    //        gm.ready[pid] = true;
    //        ResetUnderfillConfirm(pid);

    //        Debug.Log($"[BattleFlow] P{pid} READY.");

    //        if (gm.phase == GamePhase.BattlePlanningP0)
    //        {
    //            gm.phase = GamePhase.BattlePlanningP1;
    //            gm.activePlayerId = 1;
    //            ApplyPhase();
    //        }
    //        else if (gm.phase == GamePhase.BattlePlanningP1)
    //        {
    //            if (gm.ready[0] && gm.ready[1])
    //            {
    //                Debug.Log("[BattleFlow] Both READY -> ResolveTurn");
    //                gm.phase = GamePhase.BattleResolving;

    //                battle.ResolveTurnPublic();

    //                bool p0Lose = gm.boards[0].AllShipsSunk();
    //                bool p1Lose = gm.boards[1].AllShipsSunk();

    //                if (p0Lose || p1Lose)
    //                {
    //                    gm.phase = GamePhase.GameOver;

    //                    if (p0Lose && p1Lose)
    //                        Debug.Log("[Battle] Game Over: Draw");
    //                    else if (p0Lose)
    //                        Debug.Log("[Battle] Game Over: Player 1 Wins");
    //                    else
    //                        Debug.Log("[Battle] Game Over: Player 0 Wins");

    //                    return;
    //                }

    //                gm.ready[0] = gm.ready[1] = false;
    //                gm.phase = GamePhase.BattlePlanningP0;
    //                gm.activePlayerId = 0;
    //                ResetUnderfillConfirm(0);
    //                ResetUnderfillConfirm(1);
    //                ApplyPhase();
    //            }
    //        }
    //    }
    //}

    void ApplyPhase()
    {
        var gm = GameManager.Instance;

        enemyGrid0.gameObject.SetActive(true);
        enemyGrid1.gameObject.SetActive(true);

        if (gm.phase == GamePhase.BattlePlanningP0 || gm.phase == GamePhase.BattlePlanningP1)
        {
            int pid = gm.activePlayerId;
            int enemy = 1 - pid;

            var grid = (pid == 0) ? enemyGrid0 : enemyGrid1;

            battle.SetContext(
                pid,
                gm.boards[enemy],
                gm.views[pid],
                grid
            );

            SetGridInteractable(enemyGrid0, pid == 0);
            SetGridInteractable(enemyGrid1, pid == 1);

            Debug.Log($"[BattleFlow] ApplyPhase -> planningPid={pid}, enemy={enemy}");

            Debug.Log(
                $"[BattleFlow] P{pid} quota: " +
                $"Gun={battle.GetWeaponQuotaPublic(pid, WeaponType.Gun)}, " +
                $"Torp={battle.GetWeaponQuotaPublic(pid, WeaponType.Torpedo)}, " +
                $"Bomb={battle.GetWeaponQuotaPublic(pid, WeaponType.Bomb)}, " +
                $"Scout={battle.GetWeaponQuotaPublic(pid, WeaponType.Scout)}"
            );
        }
        else if (gm.phase == GamePhase.BattleResolving)
        {
            SetGridInteractable(enemyGrid0, false);
            SetGridInteractable(enemyGrid1, false);

            var cg0 = enemyGrid0.GetComponent<CanvasGroup>();
            var cg1 = enemyGrid1.GetComponent<CanvasGroup>();

            if (cg0 != null) cg0.alpha = 1f;
            if (cg1 != null) cg1.alpha = 1f;

            Debug.Log("[BattleFlow] ApplyPhase -> resolving");
        }
        battle.ShowResolvedBoards(enemyGrid0, enemyGrid1);
    }

    void SetGridInteractable(BattleEnemyGridView grid, bool interactable)
    {
        var cg = grid.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = grid.gameObject.AddComponent<CanvasGroup>();

        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
        cg.alpha = interactable ? 1f : 0.6f; // 非当前玩家的棋盘稍微变暗
    }

    public void ResetUnderfillConfirm(int pid)
    {
        underfillConfirmed[pid] = false;
    }

    public bool IsUnderfillConfirmed(int pid)
    {
        return underfillConfirmed[pid];
    }

    public void ConfirmUnderfill(int pid)
    {
        underfillConfirmed[pid] = true;
    }
    public void UI_ConfirmCurrentPlayer()
    {
        var gm = GameManager.Instance;
        int pid = gm.activePlayerId;

        bool ok = battle.ValidatePlanCounts(pid, out string msg, out bool hasOverflow, out bool hasUnderflow);
        Debug.Log($"[ConfirmCheck] pid={pid}, hasOverflow={hasOverflow}, hasUnderflow={hasUnderflow}, msg={msg}");

        if (hasOverflow)
        {
            Debug.LogWarning($"[BattleFlow] P{pid} plan invalid:\n{msg}");
            return;
        }
        Debug.Log($"[ConfirmCheck] underfillConfirmed[{pid}] = {IsUnderfillConfirmed(pid)}");
        if (hasUnderflow && !IsUnderfillConfirmed(pid))
        {
            ConfirmUnderfill(pid);

            var hud = FindObjectOfType<BattleHUDController>();
            if (hud != null)
                hud.ShowWarning(BuildUnderuseMessage(pid));

            return;
        }

        gm.ready[pid] = true;
        ResetUnderfillConfirm(pid);

        Debug.Log($"[BattleFlow] P{pid} confirmed.");

        if (gm.phase == GamePhase.BattlePlanningP0)
        {
            gm.phase = GamePhase.BattlePlanningP1;
            gm.activePlayerId = 1;
            ApplyPhase();
        }
        else if (gm.phase == GamePhase.BattlePlanningP1)
        {
            if (gm.ready[0] && gm.ready[1])
            {
                // 先记录结算前沉船数
                int p0SunkBefore = CountSunkShips(gm.boards[0]);
                int p1SunkBefore = CountSunkShips(gm.boards[1]);

                gm.phase = GamePhase.BattleResolving;

                // 先执行结算
                battle.ResolveTurnPublic();

                // 再记录结算后沉船数
                int p0SunkAfter = CountSunkShips(gm.boards[0]);
                int p1SunkAfter = CountSunkShips(gm.boards[1]);

                // 本回合新增战损
                p0LossThisRound = p0SunkAfter - p0SunkBefore;
                p1LossThisRound = p1SunkAfter - p1SunkBefore;

                // 这时候再应用 resolving 布局，展示结果
                ApplyPhase();
                battle.ShowResolvedBoards(enemyGrid0, enemyGrid1);

                bool p0Lose = gm.boards[0].AllShipsSunk();
                bool p1Lose = gm.boards[1].AllShipsSunk();

                if (p0Lose || p1Lose)
                {
                    gm.phase = GamePhase.GameOver;

                    int unlockedId = CodexUnlockSystem.Instance.UnlockRandomCodex();

                    Debug.Log($"Unlocked codex card {unlockedId}");

                    int winner = -1;

                    if (p0Lose && p1Lose)
                        winner = -1;
                    else if (p0Lose)
                        winner = 1;
                    else
                        winner = 0;

                    if (gameOverUI != null)
                        gameOverUI.ShowWinner(winner);

                    return;
                }

                gm.ready[0] = gm.ready[1] = false;
                nextRoundReady[0] = false;
                nextRoundReady[1] = false;

                Debug.Log("[BattleFlow] Turn resolved. Waiting for both players to start next round.");
            }
        }
    }
    public void UI_ReadyNextRound(int pid)
    {
        var gm = GameManager.Instance;

        if (gm.phase != GamePhase.BattleResolving)
            return;

        nextRoundReady[pid] = true;
        Debug.Log($"[BattleFlow] P{pid} is ready for next round.");

        if (nextRoundReady[0] && nextRoundReady[1])
        {
            gm.phase = GamePhase.BattlePlanningP0;
            gm.activePlayerId = 0;
            gm.roundNumber += 1;

            nextRoundReady[0] = false;
            nextRoundReady[1] = false;
            ResetUnderfillConfirm(0);
            ResetUnderfillConfirm(1);

            ApplyPhase();
        }
    }
    public bool IsPlanningPhase()
    {
        var gm = GameManager.Instance;
        return gm.phase == GamePhase.BattlePlanningP0 || gm.phase == GamePhase.BattlePlanningP1;
    }

    public bool IsResolvingPhase()
    {
        return GameManager.Instance.phase == GamePhase.BattleResolving;
    }

    public int GetActivePlanningPlayer()
    {
        return GameManager.Instance.activePlayerId;
    }

    int CountSunkShips(BoardModel board)
    {
        int count = 0;
        foreach(var ship in board.ships)
            {
            if (ship.sunk)
                count++;
        }
        return count;
    }
    string BuildUnderuseMessage(int pid)
    {
        if (battle == null) return "Unused weapons remain.";

        int gunUsed = battle.GetPlannedCountPublic(pid, WeaponType.Gun);
        int gunMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Gun);

        int torpUsed = battle.GetPlannedCountPublic(pid, WeaponType.Torpedo);
        int torpMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Torpedo);

        int bombUsed = battle.GetPlannedCountPublic(pid, WeaponType.Bomb);
        int bombMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Bomb);

        int scoutUsed = battle.GetPlannedCountPublic(pid, WeaponType.Scout);
        int scoutMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Scout);

        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();

        if (gunUsed < gunMax) parts.Add($"Gun {gunUsed}/{gunMax}");
        if (torpUsed < torpMax) parts.Add($"Torpedo {torpUsed}/{torpMax}");
        if (bombUsed < bombMax) parts.Add($"Bomb {bombUsed}/{bombMax}");
        if (scoutUsed < scoutMax) parts.Add($"Scout {scoutUsed}/{scoutMax}");

        if (parts.Count == 0)
            return "";

        return "Unused weapons remain: " + string.Join(", ", parts) + ".\nPress Confirm again to continue.";
    }

}