using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipType
{
    public string name;
    public int h;
    public int w;

    public ShipType(string name, int h, int w)
    {
        this.name = name;
        this.h = h;
        this.w = w;
    }
}

public static class ShipCatalog
{
    // typeId 对应下标
    public static readonly List<ShipType> Types = new List<ShipType>
    {
        new ShipType("护卫舰", 1, 2),      // 0
        new ShipType("鱼雷艇", 1, 2),      // 1
        new ShipType("驱逐舰", 1, 3),      // 2
        new ShipType("巡洋舰", 1, 4),      // 3
        new ShipType("重巡洋舰", 1, 5),    // 4
        new ShipType("轻巡洋舰", 1, 4),    // 5
        new ShipType("战列巡洋舰", 2, 4),  // 6
        new ShipType("战列舰", 2, 5),      // 7
        new ShipType("航空战列舰1", 2, 4), // 8
        new ShipType("航空战列舰2", 2, 4), // 9
        new ShipType("装甲战列舰", 2, 4),  // 10
        new ShipType("护航航母", 2, 3),    // 11
        new ShipType("航空母舰", 2, 5),    // 12
    };

    public struct Need { public int typeId; public int count; public Need(int t, int c) { typeId = t; count = c; } }

    // 你 demo 的舰队需求（顺序按你 main 里写的）
    public static readonly List<Need> Fleet = new List<Need>
    {
        new Need(12, 1), // CV
        new Need(11, 1), // Escort CV
        new Need(7,  1), // BB
        new Need(3,  1), // CRUI
        new Need(5,  1), // LCRU
        new Need(2,  2), // DEST
        new Need(0,  2), // FRIG
    };
}