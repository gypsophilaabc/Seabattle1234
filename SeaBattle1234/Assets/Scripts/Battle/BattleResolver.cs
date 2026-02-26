using UnityEngine;

public static class BattleResolver
{
    public static void ResolveTurn(BoardModel p0, BoardModel p1,
                                   TurnPlan plan0, TurnPlan plan1)
    {
        Debug.Log("=== 开始回合结算 ===");

        // 1️⃣ 先处理玩家0攻击玩家1
        ApplyAllAttacks(p0, p1, plan0);

        // 2️⃣ 再处理玩家1攻击玩家0
        ApplyAllAttacks(p1, p0, plan1);

        // 3️⃣ 更新沉没状态
        UpdateSunk(p0);
        UpdateSunk(p1);

        Debug.Log("=== 回合结算结束 ===");
    }

    static void ApplyAllAttacks(BoardModel attacker,
                                BoardModel defender,
                                TurnPlan plan)
    {
        // 先火炮
        foreach (var pos in plan.guns)
            ApplyGun(defender, pos);

        // 再鱼雷
        foreach (var torp in plan.torpedoes)
            ApplyTorpedo(defender, torp);

        // 最后炸弹
        foreach (var bomb in plan.bombs)
            ApplyBomb(defender, bomb);
    }

    static void ApplyGun(BoardModel defender, Vector2Int pos)
    {
        if (!defender.Inside(pos.x, pos.y)) return;

        var cell = defender.truth[pos.x, pos.y];
        if (cell.hasShip)
        {
            cell.isDamaged = true;
            defender.truth[pos.x, pos.y] = cell;

            Debug.Log($"火炮命中 ({pos.x},{pos.y})");
        }
    }

    static void ApplyTorpedo(BoardModel defender, TurnPlan.Torpedo torp)
    {
        Vector2Int p = torp.start;

        for (int i = 0; i < 4; i++)
        {
            if (!defender.Inside(p.x, p.y)) break;

            var cell = defender.truth[p.x, p.y];

            if (cell.hasShip)
            {
                cell.isDamaged = true;
                defender.truth[p.x, p.y] = cell;

                Debug.Log($"鱼雷命中 ({p.x},{p.y})");
                break;
            }

            p += torp.dir;
        }
    }

    static void ApplyBomb(BoardModel defender, TurnPlan.Bomb bomb)
    {
        for (int dr = 0; dr < 2; dr++)
        {
            for (int dc = 0; dc < 2; dc++)
            {
                int r = bomb.topLeft.x + dr;
                int c = bomb.topLeft.y + dc;

                if (!defender.Inside(r, c)) continue;

                var cell = defender.truth[r, c];
                if (cell.hasShip)
                {
                    cell.isDamaged = true;
                    defender.truth[r, c] = cell;

                    Debug.Log($"炸弹命中 ({r},{c})");
                }
            }
        }
    }

    static void UpdateSunk(BoardModel board)
    {
        foreach (var ship in board.ships)
        {
            bool allDamaged = true;

            foreach (var pos in ship.cells)
            {
                if (!board.truth[pos.x, pos.y].isDamaged)
                {
                    allDamaged = false;
                    break;
                }
            }

            ship.sunk = allDamaged;
        }
    }
}