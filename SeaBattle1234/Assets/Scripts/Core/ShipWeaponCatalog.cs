using System.Collections.Generic;

public struct ShipWeaponLoadout
{
    public int gun;
    public int torpedo;
    public int bomb;
    public int scout;

    public ShipWeaponLoadout(int gun, int torpedo, int bomb, int scout)
    {
        this.gun = gun;
        this.torpedo = torpedo;
        this.bomb = bomb;
        this.scout = scout;
    }
}

public static class ShipWeaponCatalog
{
    public static ShipWeaponLoadout GetLoadout(int typeId)
    {
        if (typeId < 0 || typeId >= ShipCatalog.Types.Count)
            return new ShipWeaponLoadout(0, 0, 0, 0);

        string name = ShipCatalog.Types[typeId].name;

        switch (name)
        {
            case "Frigate":
                return new ShipWeaponLoadout(1, 0, 0, 0);

            case "Torpedo Boat":
                return new ShipWeaponLoadout(0, 1, 0, 0);

            case "Destroyer":
                return new ShipWeaponLoadout(1, 1, 0, 0);

            case "Cruiser":
                return new ShipWeaponLoadout(3, 0, 0, 0);

            case "Heavy Cruiser":
                return new ShipWeaponLoadout(4, 0, 0, 0);

            case "Light Cruiser":
                return new ShipWeaponLoadout(2, 1, 0, 0);

            case "Battlecruiser":
                return new ShipWeaponLoadout(4, 1, 0, 0);

            case "Battleship":
                return new ShipWeaponLoadout(6, 0, 0, 0);

            case "Aviation Battleship I":
                return new ShipWeaponLoadout(3, 0, 0, 1);

            case "Aviation Battleship II":
                return new ShipWeaponLoadout(2, 0, 1, 0);

            case "Armored Battleship":
                return new ShipWeaponLoadout(5, 0, 0, 0);

            case "Escort Carrier":
                return new ShipWeaponLoadout(0, 0, 1, 1);

            case "Carrier":
                return new ShipWeaponLoadout(0, 1, 2, 1);

            default:
                return new ShipWeaponLoadout(0, 0, 0, 0);
        }
    }

    public static string GetLoadoutText(int typeId)
    {
        ShipWeaponLoadout loadout = GetLoadout(typeId);
        List<string> parts = new List<string>();

        if (loadout.gun > 0) parts.Add($"Gun x{loadout.gun}");
        if (loadout.torpedo > 0) parts.Add($"Torpedo x{loadout.torpedo}");
        if (loadout.bomb > 0) parts.Add($"Bomb x{loadout.bomb}");
        if (loadout.scout > 0) parts.Add($"Scout x{loadout.scout}");

        if (parts.Count == 0)
            return "No weapons";

        return string.Join("   ", parts);
    }
}