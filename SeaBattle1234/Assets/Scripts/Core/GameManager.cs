using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BoardModel[] boards = new BoardModel[2];
    public PlayerViewModel[] views = new PlayerViewModel[2];

    public PendingDamage[] pending = new PendingDamage[2];  // 每个玩家一个 PendingDamage 组件，用于记录本回合的射击结果
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
    }
}