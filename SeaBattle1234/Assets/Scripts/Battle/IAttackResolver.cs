public interface IAttackResolver
{
    // act = 本次行动（武器+anchor+可能的dir）
    void Resolve(BoardModel enemyBoard, PlayerViewModel attackerView, TurnAction act);
}

// TODO(dyh): 以后可以写 AdvancedResolver : IAttackResolver
// - 完全复刻你们C++ demo顺序（炮->鱼雷->炸弹）
// - 鱼雷阻挡/穿透更复杂规则
// - 炸弹/侦察其它机制