public class PlayerViewModel
{
    public CellIntelFlags[,] intel = new CellIntelFlags[BoardModel.H, BoardModel.W];

    public void AddFlag(int r, int c, CellIntelFlags flag)
    {
        intel[r, c] |= flag;
    }

    public bool HasFlag(int r, int c, CellIntelFlags flag)
    {
        return (intel[r, c] & flag) != 0;
    }
}