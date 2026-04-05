using System.Collections.Generic;

public static class CodexDatabase
{
    public static int TotalCodexCount = 50;

    private static bool[] unlocked = new bool[TotalCodexCount];

    public static bool IsUnlocked(int id)
    {
        if (id < 0 || id >= TotalCodexCount)
            return false;

        return unlocked[id];
    }

    public static void Unlock(int id)
    {
        if (id < 0 || id >= TotalCodexCount)
            return;

        unlocked[id] = true;
    }

    public static bool[] GetAll()
    {
        return unlocked;
    }
}