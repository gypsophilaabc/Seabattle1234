using System.Collections.Generic;

public class TurnPlan
{
    public readonly List<TurnAction> gun = new List<TurnAction>();
    public readonly List<TurnAction> torp = new List<TurnAction>();
    public readonly List<TurnAction> bomb = new List<TurnAction>();
    public readonly List<TurnAction> scout = new List<TurnAction>();

    private readonly List<TurnAction> history = new List<TurnAction>();
    public int Count => history.Count;

    public void Clear()
    {
        gun.Clear(); torp.Clear(); bomb.Clear(); scout.Clear();
        history.Clear();
    }

    public void Push(TurnAction a)
    {
        history.Add(a);
        GetList(a.weapon).Add(a);
    }

    public bool TryPop(out TurnAction a)
    {
        int n = history.Count;
        if (n == 0) { a = default; return false; }

        a = history[n - 1];
        history.RemoveAt(n - 1);

        var list = GetList(a.weapon);
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (SameAction(list[i], a))
            {
                list.RemoveAt(i);
                break;
            }
        }
        return true;
    }

    public IEnumerable<TurnAction> GunSeq() => gun;
    public IEnumerable<TurnAction> TorpSeq() => torp;
    public IEnumerable<TurnAction> BombSeq() => bomb;
    public IEnumerable<TurnAction> ScoutSeq() => scout;

    private List<TurnAction> GetList(WeaponType w)
    {
        switch (w)
        {
            case WeaponType.Gun: return gun;
            case WeaponType.Torpedo: return torp;
            case WeaponType.Bomb: return bomb;
            case WeaponType.Scout: return scout;
            default: return gun;
        }
    }

    private bool SameAction(TurnAction x, TurnAction y)
    {
        if (x.weapon != y.weapon) return false;
        if (x.anchor != y.anchor) return false;
        if (x.weapon == WeaponType.Torpedo)
            return x.hasDir == y.hasDir && x.dir == y.dir;
        return true;
    }
}