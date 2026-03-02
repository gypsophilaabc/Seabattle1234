using System.Collections.Generic;
using UnityEngine;
using BattleSystem;

namespace BattleSystem
{
    /// <summary>
    /// 高级武器结算器 - 实现火炮、鱼雷、炸弹、侦察机的具体结算逻辑
    /// 继承 MonoBehaviour 以便在 Unity Inspector 中拖拽
    /// </summary>
    public class AdvancedResolver : MonoBehaviour, IAttackResolver
    {
        [Header("Resolver 配置")]
        [SerializeField] private AttackResolveContext context;
        [SerializeField] private int boardRows = 16;
        [SerializeField] private int boardCols = 20;

        // 如果没有配置context，使用这些默认值
        private int TorpedoLength => context != null ? context.TorpedoLength : 5;
        private int BombSize => context != null ? context.BombSize : 2;
        private int ScoutSize => context != null ? context.ScoutSize : 2;
        private int GunSize => context != null ? context.GunSize : 1;

        public ResolveResult Resolve(BoardCell[,] truth, PlayerViewModel view, AttackAction action)
        {
            var result = new ResolveResult();

            // 按顺序结算：火炮 → 鱼雷 → 炸弹 → 侦察机
            if (action.Guns != null)
                ResolveGuns(truth, view, action.Guns, result);

            if (action.Torpedoes != null)
                ResolveTorpedoes(truth, view, action.Torpedoes, result);

            if (action.Bombs != null)
                ResolveBombs(truth, view, action.Bombs, result);

            if (action.Scouts != null)
                ResolveScouts(truth, view, action.Scouts, result);

            // 检查是否有船只被击沉
            CheckSunkShips(truth, result);

            return result;
        }

        #region 武器结算实现

        private void ResolveGuns(BoardCell[,] truth, PlayerViewModel view, List<GunTarget> guns, ResolveResult result)
        {
            foreach (var gun in guns)
            {
                var pos = gun.pos;
                if (!IsInside(pos)) continue;

                // 写入intel：GunShot
                SetIntel(view, pos, IntelType.GunShot, true);
                result.ShotPositions.Add(pos);

                // 更新wasShot状态
                truth[pos.x, pos.y].WasShot = true;

                // 如果命中
                if (truth[pos.x, pos.y].ShipId != -1 && !truth[pos.x, pos.y].IsDamaged)
                {
                    SetIntel(view, pos, IntelType.GunHit, true);
                    result.HitPositions.Add(pos);
                    result.TotalHits++;

                    truth[pos.x, pos.y].IsDamaged = true;
                }
            }
        }

        private void ResolveTorpedoes(BoardCell[,] truth, PlayerViewModel view, List<Torpedo> torpedoes, ResolveResult result)
        {
            foreach (var torp in torpedoes)
            {
                var path = GetTorpedoPath(torp);
                bool hasHit = false;

                // 先写入所有路径格的TorpLine
                foreach (var pos in path)
                {
                    SetIntel(view, pos, IntelType.TorpLine, true);
                    result.ShotPositions.Add(pos);
                    truth[pos.x, pos.y].WasShot = true;
                }

                // 然后结算命中（鱼雷只命中第一个有效船格）
                foreach (var pos in path)
                {
                    if (!hasHit && truth[pos.x, pos.y].ShipId != -1 && !truth[pos.x, pos.y].IsDamaged)
                    {
                        SetIntel(view, pos, IntelType.TorpHitLine, true);
                        result.HitPositions.Add(pos);
                        result.TotalHits++;

                        truth[pos.x, pos.y].IsDamaged = true;
                        hasHit = true;
                    }
                }
            }
        }

        private void ResolveBombs(BoardCell[,] truth, PlayerViewModel view, List<BombTarget> bombs, ResolveResult result)
        {
            foreach (var bomb in bombs)
            {
                // 遍历2x2区域
                for (int dr = 0; dr < BombSize; dr++)
                {
                    for (int dc = 0; dc < BombSize; dc++)
                    {
                        var pos = new Vector2Int(bomb.topLeft.x + dr, bomb.topLeft.y + dc);
                        if (!IsInside(pos)) continue;

                        SetIntel(view, pos, IntelType.BombArea, true);
                        result.ShotPositions.Add(pos);
                        truth[pos.x, pos.y].WasShot = true;

                        if (truth[pos.x, pos.y].ShipId != -1 && !truth[pos.x, pos.y].IsDamaged)
                        {
                            SetIntel(view, pos, IntelType.BombHit, true);
                            result.HitPositions.Add(pos);
                            result.TotalHits++;

                            truth[pos.x, pos.y].IsDamaged = true;
                        }
                    }
                }
            }
        }

        private void ResolveScouts(BoardCell[,] truth, PlayerViewModel view, List<ScoutTarget> scouts, ResolveResult result)
        {
            foreach (var scout in scouts)
            {
                for (int dr = 0; dr < ScoutSize; dr++)
                {
                    for (int dc = 0; dc < ScoutSize; dc++)
                    {
                        var pos = new Vector2Int(scout.topLeft.x + dr, scout.topLeft.y + dc);
                        if (!IsInside(pos)) continue;

                        SetIntel(view, pos, IntelType.Scout, true);
                        result.ShotPositions.Add(pos);

                        // 侦察机不造成伤害，但可以记录情报
                        if (truth[pos.x, pos.y].ShipId != -1)
                        {
                            result.LogMessages.Add($"侦察机在 ({pos.x},{pos.y}) 发现船只");
                        }
                    }
                }
            }
        }

        #endregion

        #region 辅助方法

        private void SetIntel(PlayerViewModel view, Vector2Int pos, IntelType type, bool value)
        {
            if (view.IntelData == null)
                view.IntelData = new Dictionary<IntelType, bool[,]>();

            if (!view.IntelData.ContainsKey(type))
                view.IntelData[type] = new bool[boardRows, boardCols];

            view.IntelData[type][pos.x, pos.y] = value;
        }

        private List<Vector2Int> GetTorpedoPath(Torpedo torp)
        {
            var path = new List<Vector2Int>();
            var pos = torp.start;

            for (int i = 0; i < TorpedoLength; i++)
            {
                if (!IsInside(pos)) break;

                path.Add(pos);

                switch (torp.dir)
                {
                    case Direction.Up: pos.x--; break;
                    case Direction.Down: pos.x++; break;
                    case Direction.Left: pos.y--; break;
                    case Direction.Right: pos.y++; break;
                }
            }

            return path;
        }

        private void CheckSunkShips(BoardCell[,] truth, ResolveResult result)
        {
            Dictionary<int, bool> shipSunkStatus = new Dictionary<int, bool>();

            for (int r = 0; r < boardRows; r++)
            {
                for (int c = 0; c < boardCols; c++)
                {
                    var cell = truth[r, c];
                    if (cell.ShipId != -1)
                    {
                        if (!shipSunkStatus.ContainsKey(cell.ShipId))
                        {
                            shipSunkStatus[cell.ShipId] = true;
                        }

                        if (!cell.IsDamaged)
                        {
                            shipSunkStatus[cell.ShipId] = false;
                        }
                    }
                }
            }

            foreach (var kv in shipSunkStatus)
            {
                if (kv.Value)
                {
                    result.SunkShipIds.Add(kv.Key);
                    result.LogMessages.Add($"船只 {kv.Key} 被击沉");
                }
            }
        }

        private bool IsInside(Vector2Int p)
        {
            return p.x >= 0 && p.x < boardRows && p.y >= 0 && p.y < boardCols;
        }

        #endregion
    }
}