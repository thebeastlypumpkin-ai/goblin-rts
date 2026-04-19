using UnityEngine;
using TMPro;

public class ProductionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private ProductionButtonUI button;
    [SerializeField] private TMP_Text buildingNameText;
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (SelectionManager.Instance == null)
            return;

        Building selectedBuilding = SelectionManager.Instance.SelectedBuilding;

        if (selectedBuilding == null)
        {
            panel.SetActive(false);
            return;
        }

        TeamMember buildingTeam = selectedBuilding.GetComponent<TeamMember>();
        if (buildingTeam == null)
        {
            panel.SetActive(false);
            Debug.LogWarning($"Production UI blocked: {selectedBuilding.name} has no TeamMember.");
            return;
        }

        int localTeamId = 0;
        if (SpectatorManager.Instance != null)
        {
            localTeamId = SpectatorManager.Instance.LocalTeamId;
        }

        if ((int)buildingTeam.Team != localTeamId)
        {
            panel.SetActive(false);
            Debug.Log($"Production UI blocked: enemy building {selectedBuilding.name}");
            return;
        }

        ProductionQueue queue = selectedBuilding.GetComponent<ProductionQueue>();

        if (queue == null || queue.AvailableUnitsCount() == 0)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);

        if (buildingNameText != null)
            buildingNameText.text = selectedBuilding.name;

        if (teamText != null)
            teamText.text = "Friendly";

        if (healthText != null)
            healthText.text = $"HP: {Mathf.CeilToInt(selectedBuilding.CurrentHealth)} / {Mathf.CeilToInt(selectedBuilding.MaxHealth)}";

        UnitDefinition unit = queue.GetAvailableUnit(0);
        button.Setup(queue, unit);
    }
}