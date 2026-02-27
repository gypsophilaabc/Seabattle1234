public struct CellTruth
{
    // 真实信息（是否有船）
    public bool hasShip;

    // 攻击记录
    public bool wasShot;    // 这个格子是否被攻击过
    public bool isDamaged;  // 如果有船且被击中，则为 true

    // 船索引（ships 列表中的下标）
    public int shipId;      // -1 表示无船
}