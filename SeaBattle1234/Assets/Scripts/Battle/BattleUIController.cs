using UnityEngine;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    [Header("References")]
    public BattleFlowController flow;

    public GameObject confirmButton;
    public GameObject p0ReadyButton;
    public GameObject p1ReadyButton;

    [Header("Optional")]
    public TMP_Text phaseText;

    void Update()
    {
        if (flow == null) return;

        bool planning = flow.IsPlanningPhase();
        bool resolving = flow.IsResolvingPhase();

        if (confirmButton != null)
            confirmButton.SetActive(planning);

        if (p0ReadyButton != null)
            p0ReadyButton.SetActive(resolving);

        if (p1ReadyButton != null)
            p1ReadyButton.SetActive(resolving);

        if (phaseText != null)
        {
            if (planning)
            {
                int pid = flow.GetActivePlanningPlayer();
                phaseText.text = $"Planning: Player {pid}";
            }
            else if (resolving)
            {
                phaseText.text = "Resolving / Next Round Ready";
            }
            else
            {
                phaseText.text = "Battle";
            }
        }
    }
    public TMP_Text gunText;
    public TMP_Text torpText;
    public TMP_Text bombText;
    public TMP_Text scoutText;

    public TMP_Text warningText;

    
}