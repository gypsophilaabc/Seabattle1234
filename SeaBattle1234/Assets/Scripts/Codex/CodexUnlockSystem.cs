using UnityEngine;

public class CodexUnlockSystem : MonoBehaviour
{
    public static CodexUnlockSystem Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int UnlockRandomCodex()
    {
        int count = CodexDatabase.TotalCodexCount;

        int id = Random.Range(0, count);

        CodexDatabase.Unlock(id);

        Debug.Log($"[Codex] Unlock card id={id}");

        return id;
    }
}