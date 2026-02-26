using UnityEngine;

public class BattleTestDriver : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // 玩家0打玩家1：打一炮在 (0,0)
            TurnPlan p0 = new TurnPlan();
            p0.guns.Add(new Vector2Int(0, 0));

            // 玩家1打玩家0：打一炮在 (5,5)
            TurnPlan p1 = new TurnPlan();
            p1.guns.Add(new Vector2Int(5, 5));

            BattleResolver.ResolveTurn(
                GameManager.Instance.boards[0],
                GameManager.Instance.boards[1],
                p0, p1);

            Debug.Log("按键T：已触发一次回合结算");
        }
    }
}