public struct CellTruth
{
    public bool hasShip;
    public bool isDamaged;

    // -1 表示没有船；>=0 表示 BoardModel.ships 里的索引
    public int shipId;
}