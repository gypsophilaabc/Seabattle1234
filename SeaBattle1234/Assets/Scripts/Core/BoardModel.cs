using System.Collections.Generic;

public class BoardModel
{
    public const int H = 16;
    public const int W = 20;

    public CellTruth[,] truth = new CellTruth[H, W];
    public List<ShipInstance> ships = new List<ShipInstance>();

    public BoardModel()
    {
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                truth[r, c].shipId = -1;
            }
        }
    }

    public bool Inside(int r, int c) => r >= 0 && r < H && c >= 0 && c < W;
}