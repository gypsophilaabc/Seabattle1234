using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class PlacementFlowController : MonoBehaviour
{
    [Header("References (drag in Inspector)")]
    public PlacementGridView grid;   // 拖你场景里挂了 PlacementGridView 的对象

    [Header("Scene Names")]
    public string placementSceneName = "Scene_Placement";
    public string battleSceneName = "Scene_Battle";

    void Start()
    {
        var gm = GameManager.Instance;

        // 确保 phase 合理；如果有人直接从Placement进场景也能跑
        if (gm.phase != GamePhase.PlacementP0 && gm.phase != GamePhase.PlacementP1)
        {
            gm.phase = GamePhase.PlacementP0;
            gm.activePlayerId = 0;
        }

        // 根据 phase 决定当前摆船玩家
        gm.activePlayerId = (gm.phase == GamePhase.PlacementP0) ? 0 : 1;

        Debug.Log($"[PlacementFlow] Start phase={gm.phase} activePlayer={gm.activePlayerId}");

        // 绑定给 grid（你需要在 PlacementGridView 里实现/保留这个方法；下面我也给你）
        if (grid != null)
        {
            grid.BindToPlayer(gm.activePlayerId);
            grid.EnablePlacementInput();
            grid.ClearVisual(); // 清屏（只清显示，不改数据）
        }
        else
        {
            Debug.LogWarning("[PlacementFlow] grid reference is null. Please drag PlacementGridView in Inspector.");
        }
    }

    void Update()
    {
        // 先用 Enter 当“完成摆船”
        if (Input.GetKeyDown(KeyCode.Return))
        {
            FinishPlacement();
        }
    }

    public void FinishPlacement()
    {
        var gm = GameManager.Instance;
        int pid = gm.activePlayerId;

        Debug.Log("[FinishPlacement] Enter pressed");

        if (grid == null)
        {
            Debug.LogError("PlacementFlowController: grid 未绑定");
            return;
        }

        Debug.Log($"[FinishPlacement] grid object = {grid.gameObject.name}");

        bool completed = grid.IsPlacementComplete();
        Debug.Log($"[FinishPlacement] completed = {completed}");

        if (!completed)
        {
            Debug.Log("还有船未摆放，不能进入下一阶段。");
            return;
        }

        gm.boards[pid].CopyFrom(grid.GetPlacementBoard());

        Debug.Log($"[Placement] Player{pid} finished placement. ships={gm.boards[pid].ships.Count}");

        if (gm.phase == GamePhase.PlacementP0)
        {
            gm.phase = GamePhase.PlacementP1;
            gm.activePlayerId = 1;

            Debug.Log($"[PlacementFlow] Loading placement scene = {placementSceneName}");
            SceneManager.LoadScene(placementSceneName);
        }
        else
        {
            gm.phase = GamePhase.BattlePlanningP0;
            gm.activePlayerId = 0;
            gm.ready[0] = gm.ready[1] = false;

            Debug.Log($"[PlacementFlow] Loading battle scene = {battleSceneName}");
            SceneManager.LoadScene(battleSceneName);
        }
    }
}