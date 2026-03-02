using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 攻击结算上下文配置
    /// 集中管理所有武器结算相关的规则参数
    /// 方便后续修改规则，只需修改这里
    /// </summary>
    [CreateAssetMenu(fileName = "AttackResolveContext", menuName = "Battle/AttackResolveContext")]
    public class AttackResolveContext : ScriptableObject
    {
        [Header("棋盘配置")]
        [SerializeField] private int _boardRows = 16;
        [SerializeField] private int _boardCols = 20;

        [Header("武器参数")]
        [SerializeField] private int _torpedoLength = 5;      // 鱼雷长度
        [SerializeField] private int _bombSize = 2;           // 炸弹范围（2x2）
        [SerializeField] private int _scoutSize = 2;          // 侦察机范围（2x2）
        [SerializeField] private int _gunSize = 1;            // 火炮范围（1x1）

        [Header("结算顺序")]
        [SerializeField]
        private WeaponType[] _resolveOrder = new WeaponType[]
        {
            WeaponType.Gun,      // 火炮优先
            WeaponType.Torpedo,   // 然后是鱼雷
            WeaponType.Bomb,      // 然后是炸弹
            WeaponType.Scout      // 最后是侦察机
        };

        [Header("鱼雷规则")]
        [SerializeField] private bool _torpedoStopAtFirstHit = true;     // 鱼雷是否在第一次命中后停止
        [SerializeField] private bool _torpedoIgnoreDamagedCells = true; // 鱼雷是否忽略已损坏的格子

        [Header("炸弹规则")]
        [SerializeField] private bool _bombRequireAllCellsInside = false; // 炸弹是否需要所有格子都在棋盘内

        [Header("侦察机规则")]
        [SerializeField] private bool _scoutRevealShipType = false;      // 侦察机是否揭示船只类型

        // 公共属性访问
        public int BoardRows => _boardRows;
        public int BoardCols => _boardCols;
        public int TorpedoLength => _torpedoLength;
        public int BombSize => _bombSize;
        public int ScoutSize => _scoutSize;
        public int GunSize => _gunSize;
        public WeaponType[] ResolveOrder => _resolveOrder;
        public bool TorpedoStopAtFirstHit => _torpedoStopAtFirstHit;
        public bool TorpedoIgnoreDamagedCells => _torpedoIgnoreDamagedCells;
        public bool BombRequireAllCellsInside => _bombRequireAllCellsInside;
        public bool ScoutRevealShipType => _scoutRevealShipType;

        /// <summary>
        /// 检查坐标是否在棋盘内
        /// </summary>
        public bool IsInsideBoard(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _boardRows && pos.y >= 0 && pos.y < _boardCols;
        }

        /// <summary>
        /// 检查矩形区域是否完全在棋盘内
        /// </summary>
        public bool IsRectInsideBoard(Vector2Int topLeft, int height, int width)
        {
            return IsInsideBoard(topLeft) &&
                   IsInsideBoard(new Vector2Int(topLeft.x + height - 1, topLeft.y + width - 1));
        }

        /// <summary>
        /// 获取鱼雷路径上的所有格子
        /// </summary>
        public Vector2Int[] GetTorpedoPath(Vector2Int start, Direction dir)
        {
            var path = new Vector2Int[_torpedoLength];
            var current = start;

            for (int i = 0; i < _torpedoLength; i++)
            {
                path[i] = current;

                switch (dir)
                {
                    case Direction.Up: current.x--; break;
                    case Direction.Down: current.x++; break;
                    case Direction.Left: current.y--; break;
                    case Direction.Right: current.y++; break;
                }
            }

            return path;
        }

        /// <summary>
        /// 获取炸弹区域的所有格子
        /// </summary>
        public Vector2Int[] GetBombArea(Vector2Int topLeft)
        {
            var area = new Vector2Int[_bombSize * _bombSize];
            int index = 0;

            for (int dr = 0; dr < _bombSize; dr++)
            {
                for (int dc = 0; dc < _bombSize; dc++)
                {
                    area[index++] = new Vector2Int(topLeft.x + dr, topLeft.y + dc);
                }
            }

            return area;
        }

        /// <summary>
        /// 获取侦察机区域的所有格子
        /// </summary>
        public Vector2Int[] GetScoutArea(Vector2Int topLeft)
        {
            var area = new Vector2Int[_scoutSize * _scoutSize];
            int index = 0;

            for (int dr = 0; dr < _scoutSize; dr++)
            {
                for (int dc = 0; dc < _scoutSize; dc++)
                {
                    area[index++] = new Vector2Int(topLeft.x + dr, topLeft.y + dc);
                }
            }

            return area;
        }

        /// <summary>
        /// 验证鱼雷是否有效（起点在棋盘内）
        /// </summary>
        public bool IsTorpedoValid(Torpedo torpedo)
        {
            return IsInsideBoard(torpedo.start);
        }

        /// <summary>
        /// 验证炸弹是否有效（根据规则可能要求所有格子在棋盘内）
        /// </summary>
        public bool IsBombValid(BombTarget bomb)
        {
            if (_bombRequireAllCellsInside)
            {
                return IsRectInsideBoard(bomb.topLeft, _bombSize, _bombSize);
            }
            return IsInsideBoard(bomb.topLeft);
        }

        /// <summary>
        /// 验证侦察机是否有效
        /// </summary>
        public bool IsScoutValid(ScoutTarget scout)
        {
            return IsInsideBoard(scout.topLeft);
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static AttackResolveContext CreateDefault()
        {
            var context = CreateInstance<AttackResolveContext>();
            context._boardRows = 16;
            context._boardCols = 20;
            context._torpedoLength = 5;
            context._bombSize = 2;
            context._scoutSize = 2;
            context._gunSize = 1;
            context._torpedoStopAtFirstHit = true;
            context._torpedoIgnoreDamagedCells = true;
            context._bombRequireAllCellsInside = false;
            context._scoutRevealShipType = false;
            return context;
        }
    }
}