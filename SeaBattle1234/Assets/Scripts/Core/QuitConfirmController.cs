using UnityEngine;

public class QuitConfirmController : MonoBehaviour
{
    public GameObject exitConfirmPanel;
    public float confirmDuration = 2f;

    private bool waitingSecondEsc = false;
    private float timer = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!waitingSecondEsc)
            {
                waitingSecondEsc = true;
                timer = confirmDuration;

                if (exitConfirmPanel != null)
                    exitConfirmPanel.SetActive(true);
            }
            else
            {
                QuitGame();
            }
        }

        if (waitingSecondEsc)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                waitingSecondEsc = false;

                if (exitConfirmPanel != null)
                    exitConfirmPanel.SetActive(false);
            }
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}