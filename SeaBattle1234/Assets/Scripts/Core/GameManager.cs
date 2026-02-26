using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BoardModel[] boards = new BoardModel[2];
    public PlayerViewModel[] views = new PlayerViewModel[2];

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

        boards[0] = new BoardModel();
        boards[1] = new BoardModel();

        views[0] = new PlayerViewModel();
        views[1] = new PlayerViewModel();
    }
}