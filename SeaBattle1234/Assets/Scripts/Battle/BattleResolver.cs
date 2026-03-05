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

        bool ok = board.TryShootPending(pending, rc.x, rc.y, out bool isHit, out int sid);
        Debug.Log($"[DBG] Gun ({rc.x},{rc.y}) ok={ok} hit={isHit} sid={sid}");

        if (ok && isHit) view.AddFlag(rc.x, rc.y, CellIntelFlags.GunHit);
        return ok;
    }

    // 2x2：anchor 当作左上角
    private static bool ResolveBomb2x2(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int tl)
    {
        bool any = false;
        bool causedNewDamage = false;

        // 先结算：判断是否造成“新伤害”
        for (int dr = 0; dr < 2; dr++)
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr, c = tl.y + dc;
                if (!board.Inside(r, c)) continue;

                // 若该格本来就是未受损船格，那么只要本回合把它标成 hit，就算“新伤害”
                var cell = board.truth[r, c];
                bool wasUndamagedShip = cell.hasShip && !cell.isDamaged;

                // 统一走 TryShootPending（去重）
                bool ok = board.TryShootPending(pending, r, c, out bool isHit, out _);
                if (ok) any = true;

                // 只有在 ok && isHit && 该格之前未受损 才算新伤害
                if (ok && isHit && wasUndamagedShip) causedNewDamage = true;
            }

        // 再渲染：永远显示爆炸区
        for (int dr = 0; dr < 2; dr++)
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr, c = tl.y + dc;
                if (!board.Inside(r, c)) continue;

                view.AddFlag(r, c, CellIntelFlags.BombArea);

                // 只有造成新伤害才整块强提示
                if (causedNewDamage)
                    view.AddFlag(r, c, CellIntelFlags.BombAreaHit);
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

    private static bool ResolveTorpedo(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int start, Dir4 dir)
    {
        var path = AttackMath.GetLine(start, dir, TORP_LEN);

        // 弱提示：扫过
        foreach (var p in path)
        {
            if (!board.Inside(p.x, p.y)) continue;
            view.AddFlag(p.x, p.y, CellIntelFlags.TorpLine);
            pending.Record(p.x, p.y, isHit: false); // 扫过算 shot
        }

        bool causedNewDamage = false;

        // 找命中：穿过受损格，不阻挡；命中第一个“未受损船格”
        foreach (var p in path)
        {
            if (!board.Inside(p.x, p.y)) break;

            var cell = board.truth[p.x, p.y];
            bool damagedNow = cell.isDamaged || pending.WasHit(p.x, p.y); // 本回合已命中也算“受损”

            if (cell.hasShip && !damagedNow)
            {
                pending.SetHit(p.x, p.y);
                Debug.Log($"[DBG] Torp sethit at ({p.x},{p.y}) WasShot={pending.WasShot(p.x, p.y)} WasHit={pending.WasHit(p.x, p.y)}  damagedBefore={cell.isDamaged}");
                causedNewDamage = true;
                break;
            }
            // 若是 hasShip && isDamaged：继续往后找（不阻挡）
        }

        // 强提示：只在造成新伤害时整条变色
        if (causedNewDamage)
        {
            foreach (var p in path)
            {
                Debug.Log($"[DBG] Torp start={start} dir={dir} truth00 ship={board.truth[0, 0].hasShip} dmg={board.truth[0, 0].isDamaged} | truth01 ship={board.truth[0, 1].hasShip} dmg={board.truth[0, 1].isDamaged}");
                if (!board.Inside(p.x, p.y)) continue;
                view.AddFlag(p.x, p.y, CellIntelFlags.TorpHitLine);
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