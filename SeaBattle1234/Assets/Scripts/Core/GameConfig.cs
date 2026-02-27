using UnityEngine;

[CreateAssetMenu(menuName = "SeaBattle/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board Size (当前工程仍用 BoardModel.H/W，但未来改成读这里)")]
    public int boardH = 16;
    public int boardW = 20;

    [Header("Weapon Sizes")]
    public int torpedoLen = 5;   // ✅ 你刚确认鱼雷范围 1*5
    public int bombSize = 2;     // 2x2
    public int scoutSize = 2;    // 2x2

    [Header("MVP Debug")]
    public bool enableDebugLogs = true;

    // TODO(队友-规则): 以后把更多规则参数搬进来，例如：
    // - 每回合各舰种提供的炮/雷/炸弹次数
    // - 鱼雷阻挡规则/是否穿透
    // - 是否允许重复攻击同一格
}