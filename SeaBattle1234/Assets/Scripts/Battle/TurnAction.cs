using UnityEngine;

public struct TurnAction
{
    public WeaponType weapon;
    public Vector2Int anchor;

    // ½öÓãÀ×Ê¹ÓÃ
    public bool hasDir;
    public Dir4 dir;

    public TurnAction(WeaponType weapon, Vector2Int anchor)
    {
        this.weapon = weapon;
        this.anchor = anchor;
        this.hasDir = false;
        this.dir = Dir4.Right;
    }

    public static TurnAction Torpedo(Vector2Int start, Dir4 dir)
    {
        var a = new TurnAction(WeaponType.Torpedo, start);
        a.hasDir = true;
        a.dir = dir;
        return a;
    }
}