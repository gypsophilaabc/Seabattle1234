using UnityEngine;

public static class BattleResolver
{
    private const int TORP_LEN = 5;

    public static bool Resolve(BoardModel enemyBoard, PlayerViewModel attackerView, PendingDamage pending, TurnAction act)
    {
        switch (act.weapon)
        {
            case WeaponType.Gun:
                return ResolveGun(enemyBoard, attackerView, pending, act.anchor);

            case WeaponType.Bomb:
                return ResolveBomb2x2(enemyBoard, attackerView, pending, act.anchor);

            case WeaponType.Scout:
                ResolveScout2x2(enemyBoard, attackerView, act.anchor);
                return true;

            case WeaponType.Torpedo:
                if (!act.hasDir) return false;
                return ResolveTorpedo(enemyBoard, attackerView, pending, act.anchor, act.dir);

            default:
                return false;
        }
    }

    private static bool ResolveGun(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int rc)
    {
        if (!board.Inside(rc.x, rc.y))
            return false;

        var cell = board.truth[rc.x, rc.y];

        // ===== 视觉逻辑：只看底层 truth，不看这次是否新增伤害 =====
        if (cell.hasShip)
            view.AddFlag(rc.x, rc.y, CellIntelFlags.GunHit);
        else
            view.AddFlag(rc.x, rc.y, CellIntelFlags.GunShot);

        // ===== 伤害逻辑：仍然走 pending 去重 / 拦截 =====
        bool ok = board.TryShootPending(pending, rc.x, rc.y, out bool isHit, out int sid);

        Debug.Log($"[DBG] Gun ({rc.x},{rc.y}) visualHit={cell.hasShip} ok={ok} hit={isHit} sid={sid}");
        return ok;
    }

    // 2x2：anchor 当作左上角
    private static bool ResolveBomb2x2(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int tl)
    {
        bool any = false;
        bool visualHit = false;
        bool causedNewDamage = false;

        // 先扫描：分离“视觉命中”和“是否新增伤害”
        for (int dr = 0; dr < 2; dr++)
        {
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr;
                int c = tl.y + dc;
                if (!board.Inside(r, c)) continue;

                var cell = board.truth[r, c];

                // 视觉上只要格里有船，就算命中区域
                if (cell.hasShip)
                    visualHit = true;

                bool wasUndamagedShip = cell.hasShip && !cell.isDamaged;

                bool ok = board.TryShootPending(pending, r, c, out bool isHit, out _);
                if (ok) any = true;

                if (ok && isHit && wasUndamagedShip)
                    causedNewDamage = true;
            }
        }

        // 再渲染：永远显示炸弹区域
        for (int dr = 0; dr < 2; dr++)
        {
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr;
                int c = tl.y + dc;
                if (!board.Inside(r, c)) continue;

                view.AddFlag(r, c, CellIntelFlags.BombArea);

                // 视觉上命中船，就整块给强提示
                if (visualHit)
                    view.AddFlag(r, c, CellIntelFlags.BombAreaHit);
            }
        }

        Debug.Log($"[DBG] Bomb tl={tl} visualHit={visualHit} causedNewDamage={causedNewDamage} any={any}");
        return any;
    }

    // 侦察：只加 Scout，不算射击
    private static void ResolveScout2x2(BoardModel board, PlayerViewModel view, Vector2Int tl)
    {
        for (int dr = 0; dr < 2; dr++)
        {
            for (int dc = 0; dc < 2; dc++)
            {
                int r = tl.x + dr;
                int c = tl.y + dc;
                if (!board.Inside(r, c)) continue;
                view.AddFlag(r, c, CellIntelFlags.Scout);
            }
        }
    }

    private static bool ResolveTorpedo(BoardModel board, PlayerViewModel view, PendingDamage pending, Vector2Int start, Dir4 dir)
    {
        var path = AttackMath.GetLine(start, dir, TORP_LEN);

        bool causedNewDamage = false;

        // 1. 先整条标记“扫过”
        for (int i = 0; i < path.Count; i++)
        {
            var p = path[i];
            if (!board.Inside(p.x, p.y)) continue;

            view.AddFlag(p.x, p.y, CellIntelFlags.TorpLine);

            // 扫过算 shot；如果该格本回合已被别的武器占用，Record 失败也没关系
            pending.Record(p.x, p.y, isHit: false);
        }

        // 2. 找第一个“未受损船格”作为真正爆炸点
        for (int i = 0; i < path.Count; i++)
        {
            var p = path[i];
            if (!board.Inside(p.x, p.y)) break;

            var cell = board.truth[p.x, p.y];

            bool damagedNow = cell.isDamaged || pending.WasHit(p.x, p.y);

            if (cell.hasShip && !damagedNow)
            {
                pending.SetHit(p.x, p.y);
                causedNewDamage = true;

                Debug.Log($"[DBG] Torp sethit at ({p.x},{p.y}) WasShot={pending.WasShot(p.x, p.y)} WasHit={pending.WasHit(p.x, p.y)} damagedBefore={cell.isDamaged}");
                break;
            }
        }

        // 3. 只有真正造成新命中，才整条显示 hit
        if (causedNewDamage)
        {
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i];
                if (!board.Inside(p.x, p.y)) continue;
                view.AddFlag(p.x, p.y, CellIntelFlags.TorpHitLine);
            }
        }

        // 4. 右向分片渲染：hit / miss 也跟 causedNewDamage 走
        for (int i = 0; i < path.Count; i++)
        {
            var p = path[i];
            if (!board.Inside(p.x, p.y)) continue;

            view.SetTorpedoVisual(
                p.x,
                p.y,
                dir,
                i,
                causedNewDamage
            );
        }

        Debug.Log($"[DBG] Torp start={start} dir={dir} causedNewDamage={causedNewDamage}");
        return true;
    }
}