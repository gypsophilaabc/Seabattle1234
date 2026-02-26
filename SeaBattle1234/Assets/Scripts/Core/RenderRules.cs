public static class RenderRules
{
    // ÓÅÏÈ¼¶£ºBombHit > GunHit > TorpHitLine > BombArea > TorpLine > GunShot > Scout > Sea
    public static RenderState GetRenderState(in CellTruth truth, CellIntelFlags intel)
    {
        if (intel.HasFlag(CellIntelFlags.BombHit)) return RenderState.BombHit;
        if (intel.HasFlag(CellIntelFlags.GunHit)) return RenderState.GunHit;
        if (intel.HasFlag(CellIntelFlags.TorpHitLine)) return RenderState.TorpHitLine;

        if (intel.HasFlag(CellIntelFlags.BombArea)) return RenderState.BombArea;
        if (intel.HasFlag(CellIntelFlags.TorpLine)) return RenderState.TorpLine;
        if (intel.HasFlag(CellIntelFlags.GunShot)) return RenderState.GunMiss;

        if (intel.HasFlag(CellIntelFlags.Scout))
            return truth.hasShip ? RenderState.ScoutShip : RenderState.ScoutEmpty;

        return RenderState.Sea;
    }
}