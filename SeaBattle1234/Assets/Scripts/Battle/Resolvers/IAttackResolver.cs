using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 武器结算接口 - 统一处理火炮、鱼雷、炸弹的结算逻辑
    /// </summary>
    public interface IAttackResolver
    {
        ResolveResult Resolve(BoardCell[,] truth, PlayerViewModel view, AttackAction action);
    }

    // 注意：这里只保留接口，不包含任何数据结构的定义！
    // 所有数据结构应该在单独的文件中定义
}