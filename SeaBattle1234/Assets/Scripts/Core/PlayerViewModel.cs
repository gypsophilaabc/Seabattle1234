public class PlayerViewModel
{
    public CellIntelFlags[,] intel = new CellIntelFlags[BoardModel.H, BoardModel.W];
    public TorpedoCellVisual[,] torpedoVisual = new TorpedoCellVisual[BoardModel.H, BoardModel.W];

    public void AddFlag(int r, int c, CellIntelFlags flag)
    {
        intel[r, c] |= flag;
    }

    public bool HasFlag(int r, int c, CellIntelFlags flag)
    {
        return (intel[r, c] & flag) != 0;
    }
    public void ClearTorpedoVisual(int r, int c)
    {
        torpedoVisual[r, c] = default;
    }

    public void SetTorpedoVisual(int r, int c, Dir4 dir, int index, bool isHitLine)
    {
        torpedoVisual[r, c] = new TorpedoCellVisual
        {
            active = true,
            dir = dir,
            index = index,
            isHitLine = isHitLine
        };
    }

    public bool TryGetTorpedoVisual(int r, int c, out TorpedoCellVisual v)
    {
        v = torpedoVisual[r, c];
        return v.active;
    }
}