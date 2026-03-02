using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 结算结果
    /// </summary>
    public class ResolveResult
    {
        public int TotalHits { get; set; }
        public List<Vector2Int> HitPositions { get; set; } = new();
        public List<Vector2Int> ShotPositions { get; set; } = new();
        public List<int> SunkShipIds { get; set; } = new();
        public List<string> LogMessages { get; set; } = new();
    }

    /// <summary>
    /// 攻击行动
    /// </summary>
    public class AttackAction
    {
        public WeaponType Type { get; set; }
        public List<GunTarget> Guns { get; set; } = new();
        public List<Torpedo> Torpedoes { get; set; } = new();
        public List<BombTarget> Bombs { get; set; } = new();
        public List<ScoutTarget> Scouts { get; set; } = new();
    }

    /// <summary>
    /// 武器类型
    /// </summary>
    public enum WeaponType
    {
        Gun,
        Torpedo,
        Bomb,
        Scout
    }

    /// <summary>
    /// 火炮目标
    /// </summary>
    [System.Serializable]
    public struct GunTarget
    {
        public Vector2Int pos;
    }

    /// <summary>
    /// 炸弹目标
    /// </summary>
    [System.Serializable]
    public struct BombTarget
    {
        public Vector2Int topLeft;
    }

    /// <summary>
    /// 侦察机目标
    /// </summary>
    [System.Serializable]
    public struct ScoutTarget
    {
        public Vector2Int topLeft;
    }

    /// <summary>
    /// 鱼雷
    /// </summary>
    [System.Serializable]
    public struct Torpedo
    {
        public Vector2Int start;
        public Direction dir;
    }

    /// <summary>
    /// 方向
    /// </summary>
    public enum Direction
    {
        Up, Down, Left, Right
    }

    /// <summary>
    /// Intel类型
    /// </summary>
    public enum IntelType
    {
        GunShot,
        GunHit,
        TorpLine,
        TorpHitLine,
        BombArea,
        BombHit,
        Scout
    }

    /// <summary>
    /// 玩家视图数据
    /// </summary>
    public class PlayerViewModel
    {
        public Dictionary<IntelType, bool[,]> IntelData { get; set; } = new();
        public bool[,] Shot { get; set; }
        public bool[,] Hit { get; set; }
    }

    /// <summary>
    /// 棋盘单元格
    /// </summary>
    public class BoardCell
    {
        public int ShipId { get; set; } = -1;
        public bool WasShot { get; set; } = false;
        public bool IsDamaged { get; set; } = false;
    }
}