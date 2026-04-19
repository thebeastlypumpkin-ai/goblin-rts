using TMPro;
using UnityEngine;

public class SelectedUnitPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text squadSizeText;
    [SerializeField] private TMP_Text teamText;

    public void ShowUnit(Unit unit)
    {
        if (unit == null)
        {
            Hide();
            return;
        }

        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (unitNameText != null)
            unitNameText.text = unit.name;

        if (teamText != null)
        {
            TeamMember tm = unit.GetComponent<TeamMember>();

            if (tm != null)
            {
                int localTeamId = 0;

                if (SpectatorManager.Instance != null)
                    localTeamId = SpectatorManager.Instance.LocalTeamId;

                int unitTeamId = (int)tm.Team;

                if (unitTeamId == localTeamId)
                    teamText.text = "Friendly";
                else
                    teamText.text = "Enemy";
            }
            else
            {
                teamText.text = "Unknown";
            }
        }

        if (healthText != null)
            healthText.text = $"HP: {Mathf.CeilToInt(unit.CurrentHealth)} / {Mathf.CeilToInt(unit.MaxHealth)}";

        SquadController squad = unit.GetComponent<SquadController>();
        if (squadSizeText != null)
        {
            if (squad != null)
                squadSizeText.text = $"Squad: {squad.VisualMemberCount}";
            else
                squadSizeText.text = "Squad: 1";
        }
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }
}