using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text winnerText;

    public void ShowWinner(int winner)
    {
        panel.SetActive(true);

        if (winner == -1)
            winnerText.text = "Draw!";
        else
            winnerText.text = $"Player {winner} Wins!";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Scene_Placement");
    }

    public void BackToSetup()
    {
        SceneManager.LoadScene("Scene_PreBattleConfig");
    }
}