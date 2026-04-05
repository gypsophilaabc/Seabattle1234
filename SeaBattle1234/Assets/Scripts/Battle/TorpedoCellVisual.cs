using UnityEngine;

[System.Serializable]
public struct TorpedoCellVisual
{
    public bool active;      // 这格是否有鱼雷正式overlay
    public Dir4 dir;         // 方向
    public int index;        // 在1x5里的第几个，范围 0~4
    public bool isHitLine;   // 这整条是否为命中版
}