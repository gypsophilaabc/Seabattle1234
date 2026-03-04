using UnityEngine;

public static class BattleResolver
{
    private const int TORP_LEN = 5;

    public static bool Resolve(BoardModel enemyBoard, PlayerViewModel attackerView,PendingDamage pending, TurnAction act)
    {
        switch (act.weapon)
        {
            case WeaponType.Gun:
                return ResolveGun(enemyBoard, attackerView, pending,act.anchor);

            case WeaponType.Bomb:
                return ResolveBomb2x2(enemyBoard, attackerView,pending, act.anchor);

            case WeaponType.Scout:
                ResolveScout2x2(enemyBoard, attackerView, act.anchor);
                return true;

            case WeaponType.Torpedo:
                if (!act.hasDir) return false;
                return ResolveTorpedo(enemyBoard, attackerView, pending,act.anchor, act.dir);

            default:
                return false;
        }
    }

    private static bool ResolveGun(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int rc)
    {
        view.AddFlag(rc.x, rc.y, CellIntelFlags.GunShot);

        bool ok = board.TryShootPending(pending, rc.x, rc.y, out bool isHit, out _);
        if (ok && isHit) view.AddFlag(rc.x, rc.y, CellIntelFlags.GunHit);
        return ok;
    }

    // 2x2：anchor 当作左上角
    private static bool ResolveBomb2x2(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int tl)
    {
        bool any = false;
        for (int dr = 0; dr < 2; dr++)
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr, c = tl.y + dc;
                if (!board.Inside(r, c)) continue;

                view.AddFlag(r, c, CellIntelFlags.BombArea);

                bool ok = board.TryShootPending(pending, r, c, out bool isHit, out _);
                if (ok) any = true;
                if (ok && isHit) view.AddFlag(r, c, CellIntelFlags.BombHit);
            }
        return any;
    }

    // 侦察：只加 Scout，不算射击
    private static void ResolveScout2x2(BoardModel board, PlayerViewModel view, Vector2Int tl)
    {
        for (int dr = 0; dr < 2; dr++)
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr, c = tl.y + dc;
                if (!board.Inside(r, c)) continue;
                view.AddFlag(r, c, CellIntelFlags.Scout);
            }
    }

    // ✅ 鱼雷：起点+方向，长度 1×5
    // 规则（MVP版，合理且接近你们 demo）：
    // 1) 先把整条路径标成 TorpLine（扫过）
    // 2) 依次检查路径：
    //    - 如果遇到“已经受损 isDamaged 的格子”，鱼雷被挡住，停止
    //    - 如果遇到“未受损且有船”的格子：对该格造成伤害（TryShoot），标 TorpHitLine，然后停止
    private static bool ResolveTorpedo(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int start, Dir4 dir)
    {
        // 先标整条线（扫过）
        var path = GetLine(start, dir, TORP_LEN);

        foreach (var p in path)
        {
            if (!board.Inside(p.x, p.y)) continue;
            view.AddFlag(p.x, p.y, CellIntelFlags.TorpLine);
            pending.Record(p.x, p.y, isHit: false); // 先统一当作 miss 记录 shot（防重复）
            //board.MarkShot(p.x, p.y); // 记录被“扫过/攻击过” //会被 TryShootPending 记录，所以这里不标了；并且该函数会立刻落盘，无法达到缓存效果
        }

        // 再决定命中（严格按顺序）
        foreach (var p in path)
        {
            if (!board.Inside(p.x, p.y)) break;

            var cell = board.truth[p.x, p.y];

            if (cell.isDamaged) // 被已受损格子挡住
                return true;

            if (cell.hasShip && !cell.isDamaged)
            {
                // 对这一格造成伤害（TryShoot 会写 isDamaged）（旧）// 注意：如果 TryShootPending 返回 false（比如重复攻击），则不标 TorpHitLine，因为没有实际造成伤害
                //bool ok = board.TryShootPending(pending,p.x, p.y, out bool isHit, out _);
                pending.SetHit(p.x, p.y); // 直接把这一格标成 hit（前提是之前确实记录过 shot），不管 TryShootPending 成败；因为无论如何这条线都被扫过了，且该格确实有船（不标 TorpHitLine 反而奇怪）
                 
                // if (ok && isHit)
                view.AddFlag(p.x, p.y, CellIntelFlags.TorpHitLine);
                return true;
            }
        }

        return true;
    }

    private static Vector2Int[] GetLine(Vector2Int start, Dir4 dir, int len)
    {
        var arr = new Vector2Int[len];
        int dr = 0, dc = 0;
        switch (dir)
        {
            case Dir4.Up: dr = -1; dc = 0; break;
            case Dir4.Down: dr = 1; dc = 0; break;
            case Dir4.Left: dr = 0; dc = -1; break;
            case Dir4.Right: dr = 0; dc = 1; break;
        }

        for (int i = 0; i < len; i++)
            arr[i] = new Vector2Int(start.x + dr * i, start.y + dc * i);

        return arr;
    }
}