using UnityEngine;
using TMPro;

public class BattleHUDController : MonoBehaviour
{
    [Header("References")]
    public BattleController battle;
    public BattleFlowController flow;

    [Header("Top UI")]
    public TMP_Text currentPlayerText;
    public TMP_Text roundText;
    public TMP_Text phaseText;
    public TMP_Text p0LossText;
    public TMP_Text p1LossText;

    [Header("Left Panel")]
    public TMP_Text warningText;
    public TMP_Text gunText;
    public TMP_Text torpedoText;
    public TMP_Text bombText;
    public TMP_Text scoutText;

    [Header("Right Panel")]
    public GameObject confirmButton;
    public GameObject p0ReadyButton;
    public GameObject p1ReadyButton;
    public GameObject undoButton;
    public GameObject clearButton;

    [Header("Transient Warning")]
    public float warningDuration = 2.5f;

    private float warningTimer = 0f;

    public TMP_Text hintText;

    void Update()
    {
        if (battle == null || flow == null) return;

        RefreshTopTexts();
        RefreshAttackList();
        RefreshButtons();
        RefreshHintText();
        UpdateWarningTimer();
    }

    void RefreshTopTexts()
    {
        int pid = flow.GetActivePlanningPlayer();

        if (currentPlayerText != null)
            currentPlayerText.text = $"Current Attacker: P{pid}";

        if (roundText != null)
            roundText.text = $"Round: {GameManager.Instance.roundNumber}";

        if (phaseText != null)
        {
            if (flow.IsPlanningPhase())
                phaseText.text = $"Planning: Player {pid}";
            else if (flow.IsResolvingPhase())
                phaseText.text = "Resolving / Next Round Ready";
            else
                phaseText.text = "";
        }
        if (p0LossText != null)
        {
            p0LossText.gameObject.SetActive(flow.IsResolvingPhase());
            p0LossText.text = $"P0 Loss This Round: {flow.p0LossThisRound}";
        }

        if (p1LossText != null)
        {
            p1LossText.gameObject.SetActive(flow.IsResolvingPhase());
            p1LossText.text = $"P1 Loss This Round: {flow.p1LossThisRound}";
        }
    }

    void RefreshAttackList()
    {
        int pid = flow.GetActivePlanningPlayer();

        int gunUsed = battle.GetPlannedCountPublic(pid, WeaponType.Gun);
        int gunMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Gun);

        int torpUsed = battle.GetPlannedCountPublic(pid, WeaponType.Torpedo);
        int torpMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Torpedo);

        int bombUsed = battle.GetPlannedCountPublic(pid, WeaponType.Bomb);
        int bombMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Bomb);

        int scoutUsed = battle.GetPlannedCountPublic(pid, WeaponType.Scout);
        int scoutMax = battle.GetWeaponQuotaPublic(pid, WeaponType.Scout);

        if (gunText != null) gunText.text = $"Gun: {gunUsed} / {gunMax}";
        if (torpedoText != null) torpedoText.text = $"Torpedo: {torpUsed} / {torpMax}";
        if (bombText != null) bombText.text = $"Bomb: {bombUsed} / {bombMax}";
        if (scoutText != null) scoutText.text = $"Scout: {scoutUsed} / {scoutMax}";
    }

    void RefreshButtons()
    {
        bool planning = flow.IsPlanningPhase();
        bool resolving = flow.IsResolvingPhase();

        if (confirmButton != null) confirmButton.SetActive(planning);
        if (undoButton != null) undoButton.SetActive(planning);
        if (clearButton != null) clearButton.SetActive(planning);

        if (p0ReadyButton != null) p0ReadyButton.SetActive(resolving);
        if (p1ReadyButton != null) p1ReadyButton.SetActive(resolving);
    }

    void RefreshHintText()
    {
        if (hintText == null) return;

        if (flow.IsPlanningPhase())
        {
            hintText.text =
                "1-4: Select weapon\n" +
                "WASD: Torpedo direction\n" +
                "Confirm: End planning\n" +
                "Undo / Clear: Edit plan";
        }
        else if (flow.IsResolvingPhase())
        {
            hintText.text =
                "Both players must press Ready\nto enter the next round.";
        }
        else
        {
            hintText.text = "";
        }
    }
    public void ShowWarning(string message, float duration = -1f)
    {
        if (warningText == null) return;

        Debug.Log("[HUD Warning] " + message);

        warningText.text = message;
        warningText.gameObject.SetActive(true);
        warningTimer = (duration > 0f) ? duration : warningDuration;
    }

    void UpdateWarningTimer()
    {
        if (warningText == null) return;
        if (!warningText.gameObject.activeSelf) return;

        warningTimer -= Time.deltaTime;
        if (warningTimer <= 0f)
        {
            warningText.gameObject.SetActive(false);
        }
    }
}