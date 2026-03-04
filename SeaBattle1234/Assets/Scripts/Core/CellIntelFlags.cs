using System;

[Flags]
public enum CellIntelFlags
{
    None = 0,

    // Gun
    GunShot = 1 << 0,   // 波纹
    GunHit = 1 << 1,   // 单格命中（红橙+黑框）

    // Torpedo (line style)
    TorpLine = 1 << 2, // 灰带（扫过）
    TorpHitLine = 1 << 3, // 水花带（该条鱼雷结算命中）


    // Bomb (2x2)
    BombArea = 1 << 4,  // 绿色覆盖区（投弹过）
    BombHit = 1 << 5,  // 蘑菇云（该炸弹结算命中）
    BombAreaHit = 1 <<7 ,

    // Scout (2x2)
    Scout = 1 << 6 ,     // 侦察揭示（永久保留）


}