using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadBattle()
    {
        SceneManager.LoadScene("Scene_Battle");
    }
    // 添加这个新方法用于加载战前配置场景
    public void LoadPreBattleConfig()
    {
        SceneManager.LoadScene("Scene_PreBattleConfig");
    }
}