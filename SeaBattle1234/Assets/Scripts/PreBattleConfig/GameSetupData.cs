using System;

[Serializable]
public class GameSetupData
{
    public int boardRows;
    public int boardCols;

    public FleetSetupData player0Fleet = new FleetSetupData();
    public FleetSetupData player1Fleet = new FleetSetupData();

    public bool useDefaultSetup = true;
}