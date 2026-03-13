public static class GameSetupRuntime
{
    public static GameSetupData CurrentSetup;

    public static void Clear()
    {
        CurrentSetup = null;
    }

    public static void UseDefault()
    {
        CurrentSetup = new GameSetupData();
        CurrentSetup.boardRows = 16;
        CurrentSetup.boardCols = 20;
        CurrentSetup.useDefaultSetup = true;

        CurrentSetup.player0Fleet = CreateDefaultFleet();
        CurrentSetup.player1Fleet = CreateDefaultFleet();
    }

    private static FleetSetupData CreateDefaultFleet()
    {
        FleetSetupData fleet = new FleetSetupData();

        foreach (var need in ShipCatalog.Fleet)
        {
            fleet.selectedShips.Add(new ShipPickData
            {
                typeId = need.typeId,
                count = need.count
            });
        }

        return fleet;
    }
}