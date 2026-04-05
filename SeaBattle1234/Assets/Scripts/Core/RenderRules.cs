public static class RenderRules
{
    private static bool Has(CellIntelFlags intel, CellIntelFlags flag)
        => (intel & flag) != 0;

    public static RenderState GetRenderState(in CellTruth truth, CellIntelFlags intel)
    {
        // 1. 火炮最高优先级
        if (Has(intel, CellIntelFlags.GunHit))
            return RenderState.GunHit;

        if (Has(intel, CellIntelFlags.GunShot))
            return RenderState.GunMiss;

        // 2. 侦察决定基础真实状态
        // 2. 侦察直接揭示真实状态
        if (Has(intel, CellIntelFlags.Scout))
        {
            if (truth.hasShip)
                return truth.isDamaged ? RenderState.ScoutDamagedShip : RenderState.ScoutShip;
            else
                return RenderState.ScoutEmpty;
        }

        //// 3. 炸弹先维持旧逻辑（鱼雷不再走这里）
        //if (Has(intel, CellIntelFlags.BombAreaHit))
        //    return RenderState.BombAreaHit;

        //if (Has(intel, CellIntelFlags.BombArea))
        //    return RenderState.BombArea;

        // 4. 默认
        return RenderState.Sea;
    }
}