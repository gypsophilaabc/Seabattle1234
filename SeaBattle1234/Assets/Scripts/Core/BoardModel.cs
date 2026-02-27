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

    public bool TryShoot(int r, int c, out bool isHit, out int hitShipId)
    {
        isHit = false;
        hitShipId = -1;

        if (!Inside(r, c))
            return false;

        var cell = truth[r, c];

        // 已经攻击过
        if (cell.wasShot)
            return false;

        cell.wasShot = true;

        if (cell.hasShip)
        {
            cell.isDamaged = true;
            isHit = true;
            hitShipId = cell.shipId;
        }

        truth[r, c] = cell;

        // 如果命中，更新沉没状态
        if (isHit && hitShipId >= 0)
        {
            UpdateSunkForShip(hitShipId);
        }

        return true;
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

    public void MarkShot(int r, int c)
    {
        if (!Inside(r, c)) return;
        var cell = truth[r, c];
        cell.wasShot = true;
        truth[r, c] = cell;
    }

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