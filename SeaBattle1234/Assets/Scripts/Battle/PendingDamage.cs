using System;


public class PendingDamage
{
    // 和 BoardModel 保持一致的棋盘尺寸
    private bool[,] pendingShot = new bool[BoardModel.H, BoardModel.W];
    private bool[,] pendingHit = new bool[BoardModel.H, BoardModel.W];

    /// <summary>
    /// 本回合是否对该格开过火（用于防止同回合重复计数）
    /// </summary>
    public bool WasShot(int r, int c) => pendingShot[r, c];

    /// <summary>
    /// 本回合该格是否被命中（只有第一次有效射击时才会记录）
    /// </summary>
    public bool WasHit(int r, int c) => pendingHit[r, c];

    /// <summary>
    /// 清空本回合缓存（回合结束 Commit 后调用）
    /// </summary>
    public void Clear()
    {
        Array.Clear(pendingShot, 0, pendingShot.Length);
        Array.Clear(pendingHit, 0, pendingHit.Length);
    }

    /// <summary>
    /// 记录本回合对某格的一次射击结果。
    /// 若本回合同一格已射击过，则返回 false（重复射击无效）。
    /// </summary>
    public bool Record(int r, int c, bool isHit)
    {
        if (pendingShot[r, c])
            return false; // 本回合重复打同一格：无效

        pendingShot[r, c] = true;
        if (isHit) 
            pendingHit[r, c] = true;

        return true;
    }
    public void SetHit(int r, int c)
    {
        // 只有本回合确实记录过 shot，hit 才有意义
        if (pendingShot[r, c])
            pendingHit[r, c] = true;
    }
}
