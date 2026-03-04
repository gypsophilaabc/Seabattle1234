public enum RenderState
{
    Sea,            // 默认蓝海

    // Scout reveal (黑/白由 truth.hasShip 决定)
    ScoutShip,      // 侦察黑：有船
    ScoutEmpty,     // 侦察白：无船
    ScoutDamagedShip,

    // Gun
    GunMiss,        // 波纹
    GunHit,         // 火炮命中

    // Torpedo
    TorpLine,       // 灰带
    TorpHitLine,    // 水花带（整条）

    // Bomb
    BombArea,       // 绿色覆盖
    BombHit,         // 蘑菇云
    BombAreaHit,
    

}