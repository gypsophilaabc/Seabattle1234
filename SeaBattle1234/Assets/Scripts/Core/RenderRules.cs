public static class RenderRules
{
    private static bool Has(CellIntelFlags intel, CellIntelFlags flag)
        => (intel & flag) != 0;

    // ÓÅÏÈ¼¶£ºBombHit > GunHit > TorpHitLine > BombArea > TorpLine > GunShot > Scout > Sea
    public static RenderState GetRenderState(in CellTruth truth, CellIntelFlags intel)
    {
        if (Has(intel, CellIntelFlags.BombHit)) return RenderState.BombHit;
        if (Has(intel, CellIntelFlags.GunHit)) return RenderState.GunHit;
        if (Has(intel, CellIntelFlags.TorpHitLine)) return RenderState.TorpHitLine;

        if (Has(intel, CellIntelFlags.BombArea)) return RenderState.BombArea;
        if (Has(intel, CellIntelFlags.TorpLine)) return RenderState.TorpLine;
        if (Has(intel, CellIntelFlags.GunShot)) return RenderState.GunMiss;

        if (Has(intel, CellIntelFlags.Scout))
            return truth.hasShip ? RenderState.ScoutShip : RenderState.ScoutEmpty;

        return RenderState.Sea;
    }
}