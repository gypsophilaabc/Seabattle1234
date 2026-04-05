using UnityEngine;

public class GlobalQuitController : MonoBehaviour
{
    [Header("Quit Settings")]
    [SerializeField] private float doublePressInterval = 2f;

    private float lastQuitKeyTime = -999f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleQuitKey();
        }
    }

    private void HandleQuitKey()
    {
        float now = Time.time;

        if (now - lastQuitKeyTime < doublePressInterval)
        {
            // 第二次按 -> 退出
            QuitGame();
        }
        else
        {
            // 第一次按 -> 提示
            lastQuitKeyTime = now;
            ShowQuitHint();
        }
    }

    private void ShowQuitHint()
    {
        Debug.Log("Press Q again to quit");

        // 如果你有 HUD，可以在这里接入 UI 提示
        var hud = FindObjectOfType<BattleHUDController>();
        if (hud != null)
        {
            hud.ShowWarning("Press Q again to quit");
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}