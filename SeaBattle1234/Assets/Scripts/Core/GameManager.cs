using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BoardModel[] boards = new BoardModel[2];
    public PlayerViewModel[] views = new PlayerViewModel[2];

    public PendingDamage[] pending = new PendingDamage[2];  // 每个玩家一个 PendingDamage 组件，用于记录本回合的射击结果

    public bool DebugMode = true;
    public bool DebugFixedSetup = true;

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
    }
}