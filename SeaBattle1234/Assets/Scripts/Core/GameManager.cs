using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BoardModel[] boards = new BoardModel[2];
    public PlayerViewModel[] views = new PlayerViewModel[2];

    public PendingDamage[] pending = new PendingDamage[2];  // 每个玩家一个 PendingDamage 组件，用于记录本回合的射击结果

    public bool DebugMode = true;
    public bool DebugFixedSetup = true;

    public int currentPlacementPlayer = 0;

    public enum GamePhase
    {
        PlacementP0,
        PlacementP1,
        BattlePlanningP0,
        BattlePlanningP1,
        BattleResolving,
        GameOver
    }

    public GamePhase phase = GamePhase.PlacementP0;

    // 当前正在摆船/正在规划的玩家
    public int activePlayerId = 0;

    // 双方是否已经在本回合“按下确认”（规划结束）
    public bool[] ready = new bool[2];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 确保数组本身被创建
        boards = new BoardModel[2];
        views = new PlayerViewModel[2];
        pending = new PendingDamage[2];

        boards[0] = new BoardModel();  //0为我方
        boards[1] = new BoardModel();   //1为敌方

        views[0] = new PlayerViewModel();
        views[1] = new PlayerViewModel();

        pending[0] = new PendingDamage();
        pending[1] = new PendingDamage();

        // ===== DEBUG: 固定棋盘（黑盒验收用）=====
        if (DebugMode && DebugFixedSetup)
        {
            boards[0].Debug_ClearAll();
            boards[1].Debug_ClearAll();

            // ===== Enemy (b1) =====
            boards[1].Debug_PlaceShipLine(0, new Vector2Int(0, 0), new Vector2Int(0, 1));   // len2
            boards[1].Debug_PlaceShipLine(1, new Vector2Int(5, 5), new Vector2Int(7, 5));   // len3
            boards[1].Debug_PlaceShipLine(2, new Vector2Int(0, 6), new Vector2Int(0, 9));   // len4
            boards[1].Debug_PlaceShipLine(3, new Vector2Int(15, 17), new Vector2Int(15, 19));  // len3 (edge)
            boards[1].Debug_PlaceShipLine(4, new Vector2Int(10, 0), new Vector2Int(11, 0));   // len2 (edge)

            // ===== Player (b0) =====
            boards[0].Debug_PlaceShipLine(0, new Vector2Int(2, 2), new Vector2Int(2, 3));   // len2
            boards[0].Debug_PlaceShipLine(1, new Vector2Int(9, 10), new Vector2Int(11, 10));  // len3
            boards[0].Debug_PlaceShipLine(2, new Vector2Int(14, 5), new Vector2Int(14, 8));   // len4

            Debug.Log("[DBG] New fixed setup applied.");
        }
        //初始化
        phase = GamePhase.PlacementP0;
        activePlayerId = 0;
        ready[0] = ready[1] = false;
    }

    void Start()
    {
        var line = AttackMath.GetLine(new Vector2Int(5, 5), Dir4.Right, 5);

        Debug.Log("AttackMath line test:");

        foreach (var p in line)
            Debug.Log(p);

        var rect = AttackMath.GetRect(new Vector2Int(3, 3), 2, 2);

        Debug.Log("Rect test:");
        foreach (var p in rect)
            Debug.Log(p);

    }
}