using System.Collections.Generic;
using UnityEngine;

public class BoardModel
{
    public const int H = 16;
    public const int W = 20;

    public CellTruth[,] truth = new CellTruth[H, W];
    public List<ShipInstance> ships = new List<ShipInstance>();

    public BoardModel()
    {
        // 初始化所有格子
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                truth[r, c].shipId = -1;
            }
        }
    }

    public bool Inside(int r, int c)
    {
        return r >= 0 && r < H && c >= 0 && c < W;
    }

    // ===========================
    // ✅ 攻击逻辑核心
    // ===========================

    //public bool TryShoot(int r, int c, out bool isHit, out int hitShipId)
    //{
    //    isHit = false;
    //    hitShipId = -1;

    //    if (!Inside(r, c))
    //        return false;

    //    var cell = truth[r, c];

        // 已经攻击过
    //    if (cell.wasShot)
    //        return false;

    //    cell.wasShot = true;

    //    if (cell.hasShip)
    //    {
    //        cell.isDamaged = true;
    //        isHit = true;
    //        hitShipId = cell.shipId;
    //    }

    //    truth[r, c] = cell;

        // 如果命中，更新沉没状态
    //    if (isHit && hitShipId >= 0)
    //    {
    //        UpdateSunkForShip(hitShipId);
    //    }

    //    return true;
    //}
    // ===========================
    //  回合内缓存射击（不落盘）
    // ===========================
    public bool TryShootPending(PendingDamage pd, int r, int c, out bool isHit, out int hitShipId)
    {
        isHit = false;
        hitShipId = -1;

        if (!Inside(r, c))
            return false;

        //  已经落盘的攻击（上一回合/更早）不能再打
        if (truth[r, c].wasShot)
            return false;

        //  本回合同一格重复打：无效（防重复伤害）
        if (pd.WasShot(r, c))
            return false;

        var cell = truth[r, c];
        isHit = cell.hasShip;
        hitShipId = isHit ? cell.shipId : -1;

        //  只记录到 pending，不写 truth
        return pd.Record(r, c, isHit);
    }

    // ===========================
    // ✅ 回合末提交（落盘到 truth）
    // ===========================
    public void CommitPending(PendingDamage pd)
    {
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                if (r == 0 && c == 0)
                {
                    var cell00 = truth[0, 0];
                    Debug.Log($"[DBG] Commit (0,0): pdShot={pd.WasShot(0, 0)} pdHit={pd.WasHit(0, 0)} truthWasShot={cell00.wasShot} truthDamaged={cell00.isDamaged} ship={cell00.hasShip} shipId={cell00.shipId}");
                }

                if (!pd.WasShot(r, c))
                    continue;

                var cell = truth[r, c];

                // 防御：如果这格已经落盘 shot 过，就跳过
                if (cell.wasShot)
                {
                    if (r == 0 && c == 0)
                        Debug.Log("[DBG] Commit (0,0) SKIP because truth.wasShot already true");
                    continue;
                }

                cell.wasShot = true;

                if (pd.WasHit(r, c))
                {
                    cell.isDamaged = true;
                    truth[r, c] = cell;

                    // 命中则可能导致沉没
                    int sid = cell.shipId;
                    if (sid >= 0) UpdateSunkForShip(sid);
                }
                else
                {
                    truth[r, c] = cell;
                }
            }
        }
        Debug.Log($"[DBG] After Commit: truth(0,0) wasShot={truth[0, 0].wasShot} damaged={truth[0, 0].isDamaged}");

        // 提交完清空，进入下一回合
        pd.Clear();
    }
    private void UpdateSunkForShip(int shipId)
    {
        if (shipId < 0 || shipId >= ships.Count)
            return;

        var ship = ships[shipId];

        if (ship.sunk)
            return;

        foreach (var pos in ship.cells)
        {
            if (!truth[pos.x, pos.y].isDamaged)
                return;
        }

        ship.sunk = true;
        ships[shipId] = ship;
    }

    public bool AllShipsSunk()
    {
        foreach (var ship in ships)
        {
            if (!ship.sunk)
                return false;
        }

        return true;
    }

    // ===========================
    // ✅ Debug：清空棋盘 & 固定摆船
    // ===========================
    public void Debug_ClearAll()
    {
        // 清 truth
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                var cell = truth[r, c];
                cell.shipId = -1;
                cell.hasShip = false;
                cell.wasShot = false;
                cell.isDamaged = false;
                truth[r, c] = cell;
            }
        }

        // 清 ships
        ships.Clear();
    }

    /// <summary>
    /// 在 a->b（含端点）摆一条直线船（仅支持水平或垂直）
    /// shipId 用 ships 的 index；如果 shipId == ships.Count 会自动 Add 一个新船
    /// </summary>
    public void Debug_PlaceShipLine(int shipId, Vector2Int a, Vector2Int b)
    {
        while (ships.Count <= shipId)
            ships.Add(new ShipInstance());

        var ship = ships[shipId];
        ship.cells.Clear();
        ship.sunk = false;

        int dr = (b.x > a.x) ? 1 : (b.x < a.x ? -1 : 0);
        int dc = (b.y > a.y) ? 1 : (b.y < a.y ? -1 : 0);

        int r = a.x;
        int c = a.y;

        while (true)
        {
            var cell = truth[r, c];

            cell.shipId = shipId;
            cell.hasShip = true;
            cell.wasShot = false;
            cell.isDamaged = false;

            truth[r, c] = cell;

            ship.cells.Add(new Vector2Int(r, c));

            if (r == b.x && c == b.y)
                break;

            r += dr;
            c += dc;
        }

        Debug.Log($"[DBG] Ship {shipId} placed from {a} to {b}");
    }

    //public void MarkShot(int r, int c)  由于 TryShootPending 已经记录了本回合的射击，这个函数就没什么用了；并且它会立刻落盘，无法达到缓存效果
    //{
    //    if (!Inside(r, c)) return;
    //   var cell = truth[r, c];
    //   cell.wasShot = true;
    //  truth[r, c] = cell;
    // }

    public void CopyFrom(BoardModel src)
    {
        // TODO(yjl): 摆放完成后调用，把 placementBoard 复制到 GameManager 的 boards[playerId]
        // 注意：truth 是 struct 数组，逐格复制最安全
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                truth[r, c] = src.truth[r, c];

        // ships 深拷贝（最小可用：先浅拷贝引用也行，但建议复制 cells）
        ships.Clear();
        foreach (var s in src.ships)
        {
            var ns = new ShipInstance();
            ns.typeId = s.typeId;
            ns.sunk = s.sunk;
            foreach (var p in s.cells) ns.cells.Add(p);
            ships.Add(ns);
        }
    }
}