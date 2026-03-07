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

    void Update()
    {
        if (battle == null || flow == null) return;

        RefreshTopTexts();
        RefreshAttackList();
        RefreshButtons();
    }

    void RefreshTopTexts()
    {
        int pid = flow.GetActivePlanningPlayer();

        if (currentPlayerText != null)
            currentPlayerText.text = $"Current Attacker: P{pid}";

        if (roundText != null)
            roundText.text = $"Round: 1";

        if (phaseText != null)
        {
            if (flow.IsPlanningPhase())
                phaseText.text = $"Planning: Player {pid}";
            else if (flow.IsResolvingPhase())
                phaseText.text = "Resolving / Next Round Ready";
            else
                phaseText.text = "";
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

        bool hasUnused =
            (gunUsed < gunMax) ||
            (torpUsed < torpMax) ||
            (bombUsed < bombMax) ||
            (scoutUsed < scoutMax);

        if (warningText != null)
        {
            warningText.gameObject.SetActive(hasUnused);

            if (hasUnused)
                warningText.text = "Unused weapons remain";
        }
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
}