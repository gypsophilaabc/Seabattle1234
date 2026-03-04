public static class RenderRules
{
    private static bool Has(CellIntelFlags intel, CellIntelFlags flag)
        => (intel & flag) != 0;

    public static RenderState GetRenderState(in CellTruth truth, CellIntelFlags intel)
    {
        // 1) 火炮信息最强：命中/未命中永远覆盖其他染色
        if (Has(intel, CellIntelFlags.GunHit)) return RenderState.GunHit;
        if (Has(intel, CellIntelFlags.GunShot)) return RenderState.GunMiss;

        // 2) 侦察永远显示（但仍要能显示受损船）
        if (Has(intel, CellIntelFlags.Scout))
        {
            if (truth.hasShip)
                return truth.isDamaged ? RenderState.ScoutDamagedShip : RenderState.ScoutShip;
            return RenderState.ScoutEmpty;
        }

        // 3) 组强提示（只有造成新伤害时才会被 resolver 加上）
        if (Has(intel, CellIntelFlags.TorpHitLine)) return RenderState.TorpHitLine;
        if (Has(intel, CellIntelFlags.BombAreaHit)) return RenderState.BombAreaHit;

        // 4) 组弱提示
        if (Has(intel, CellIntelFlags.BombArea)) return RenderState.BombArea;
        if (Has(intel, CellIntelFlags.TorpLine)) return RenderState.TorpLine;

        return RenderState.Sea;
    }
}