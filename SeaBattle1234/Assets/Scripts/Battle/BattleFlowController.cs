using UnityEngine;
using static GameManager;

public class BattleFlowController : MonoBehaviour
{
    [Header("References")]
    public BattleController battle;
    public BattleEnemyGridView enemyGrid0; // 给 P0 用（攻击 P1）
    public BattleEnemyGridView enemyGrid1; // 给 P1 用（攻击 P0）

    

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

        Debug.Log($"[BattleFlow] Start phase={gm.phase}. Press R to Ready.");
        ApplyPhase();
    }

    void Update()
    {
        var gm = GameManager.Instance;

        if (Input.GetKeyDown(KeyCode.R))
        {
            int pid = gm.activePlayerId;
            gm.ready[pid] = true;

            Debug.Log($"[BattleFlow] P{pid} READY.");

            // 切到另一位玩家规划
            if (gm.phase == GamePhase.BattlePlanningP0)
            {
                gm.phase = GamePhase.BattlePlanningP1;
                gm.activePlayerId = 1;
                ApplyPhase();
            }
            else if (gm.phase == GamePhase.BattlePlanningP1)
            {
                // 两边都 ready 了才结算
                if (gm.ready[0] && gm.ready[1])
                {
                    Debug.Log("[BattleFlow] Both READY -> ResolveTurn");
                    gm.phase = GamePhase.BattleResolving;

                    battle.ResolveTurnPublic(); // 下一步我们会加这个 public 包装

                    gm.ready[0] = gm.ready[1] = false;
                    gm.phase = GamePhase.BattlePlanningP0;
                    gm.activePlayerId = 0;
                    ApplyPhase();
                }
                else
                {
                    // 理论上走不到（因为 P0 ready 后一定切到 P1）
                    gm.phase = GamePhase.BattlePlanningP0;
                    gm.activePlayerId = 0;
                    ApplyPhase();
                }
            }
        }
    }

    void ApplyPhase()
    {
        var gm = GameManager.Instance;
        int pid = gm.activePlayerId;
        int enemy = 1 - pid;

        // 显示：只开当前玩家对应的网格
        enemyGrid0.gameObject.SetActive(pid == 0);
        enemyGrid1.gameObject.SetActive(pid == 1);

        // P0 用 grid0；P1 用 grid1
        var grid = (pid == 0) ? enemyGrid0 : enemyGrid1;

        // 关键：把 BattleController 的上下文切到“当前规划者”
        battle.SetContext(
            pid,
            gm.boards[enemy],   // 当前玩家要打的敌方棋盘
            gm.views[pid],      // 当前玩家自己的情报板
            grid
        );

        Debug.Log($"[BattleFlow] ApplyPhase -> planningPid={pid}, enemy={enemy}");
    }
}