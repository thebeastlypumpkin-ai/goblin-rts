using UnityEngine;
using TMPro;

public class ProductionUIManager : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [Header("Mode Roots")]
    [SerializeField] private GameObject buildingModeRoot;
    [SerializeField] private GameObject builderModeRoot;

    [Header("Unit Production")]
    [SerializeField] private ProductionButtonUI unitButton;

    [Header("Builder Production")]
    [SerializeField] private Transform builderButtonContainer;
    [SerializeField] private BuilderBuildButtonUI builderButtonPrefab;

    [Header("Info Text")]
    [SerializeField] private TMP_Text buildingNameText;
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private TMP_Text healthText;

    [SerializeField] private BuildingPlacementSystem placementSystem;

    private BuilderBuildButtonUI[] spawnedBuilderButtons = new BuilderBuildButtonUI[0];

    private Building lastBuilding;
    private Builder lastBuilder;
    private bool showingBuildingUI;
    private bool showingBuilderUI;

    private void Start()
    {
        panel.SetActive(false);

        if (buildingModeRoot != null)
            buildingModeRoot.SetActive(false);

        if (builderModeRoot != null)
            builderModeRoot.SetActive(false);

        if (builderButtonPrefab != null)
            builderButtonPrefab.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (SelectionManager.Instance == null)
            return;

        Building selectedBuilding = SelectionManager.Instance.SelectedBuilding;
        var selectedUnits = SelectionManager.Instance.GetSelectedUnits();

        Builder selectedBuilder = null;
        if (selectedBuilding == null && selectedUnits.Count == 1)
        {
            selectedBuilder = selectedUnits[0].GetComponent<Builder>();
        }

        if (selectedBuilding != null)
        {
            if (selectedBuilding != lastBuilding || !showingBuildingUI)
            {
                HandleBuildingUI(selectedBuilding);
                lastBuilding = selectedBuilding;
                lastBuilder = null;
                showingBuildingUI = true;
                showingBuilderUI = false;
            }

            return;
        }

        if (selectedBuilder != null)
        {
            if (selectedBuilder != lastBuilder || !showingBuilderUI)
            {
                HandleBuilderUI(selectedBuilder);
                lastBuilder = selectedBuilder;
                lastBuilding = null;
                showingBuilderUI = true;
                showingBuildingUI = false;
            }

            return;
        }

        if (showingBuildingUI || showingBuilderUI || panel.activeSelf)
        {
            HideAllUI();
        }

        lastBuilding = null;
        lastBuilder = null;
        showingBuildingUI = false;
        showingBuilderUI = false;
    }

    private void HandleBuildingUI(Building selectedBuilding)
    {
        TeamMember buildingTeam = selectedBuilding.GetComponent<TeamMember>();

        if (buildingTeam == null)
        {
            HideAllUI();
            return;
        }

        int localTeamId = SpectatorManager.Instance != null ? SpectatorManager.Instance.LocalTeamId : 0;

        if ((int)buildingTeam.Team != localTeamId)
        {
            HideAllUI();
            return;
        }

        ProductionQueue queue = selectedBuilding.GetComponent<ProductionQueue>();

        if (queue == null || queue.AvailableUnitsCount() == 0)
        {
            HideAllUI();
            return;
        }

        panel.SetActive(true);

        if (buildingModeRoot != null)
            buildingModeRoot.SetActive(true);

        if (builderModeRoot != null)
            builderModeRoot.SetActive(false);

        if (buildingNameText != null)
            buildingNameText.text = selectedBuilding.name;

        if (teamText != null)
            teamText.text = "Friendly";

        if (healthText != null)
            healthText.text = $"HP: {Mathf.CeilToInt(selectedBuilding.CurrentHealth)} / {Mathf.CeilToInt(selectedBuilding.MaxHealth)}";

        if (unitButton != null)
        {
            unitButton.gameObject.SetActive(true);

            UnitDefinition unit = queue.GetAvailableUnit(0);
            unitButton.Setup(queue, unit);
        }

        HideAllBuilderButtons();
    }

    private void HandleBuilderUI(Builder builder)
    {
        panel.SetActive(true);

        if (buildingModeRoot != null)
            buildingModeRoot.SetActive(false);

        if (builderModeRoot != null)
            builderModeRoot.SetActive(true);

        if (unitButton != null)
            unitButton.gameObject.SetActive(false);

        RebuildBuilderButtons(builder.AvailableBuildings);
    }

    private void RebuildBuilderButtons(BuildingDefinition[] buildings)
    {
        ClearSpawnedBuilderButtons();

        if (buildings == null || buildings.Length == 0)
            return;

        if (builderButtonContainer == null)
        {
            Debug.LogWarning("ProductionUIManager: builderButtonContainer is not assigned.");
            return;
        }

        if (builderButtonPrefab == null)
        {
            Debug.LogWarning("ProductionUIManager: builderButtonPrefab is not assigned.");
            return;
        }

        spawnedBuilderButtons = new BuilderBuildButtonUI[buildings.Length];

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null)
                continue;

            BuilderBuildButtonUI newButton = Instantiate(builderButtonPrefab, builderButtonContainer, false);
            newButton.gameObject.SetActive(true);
            newButton.name = $"BuilderButton_{buildings[i].buildingName}";
            newButton.Setup(buildings[i], placementSystem);

            spawnedBuilderButtons[i] = newButton;
        }
    }

    private void HideAllUI()
    {
        panel.SetActive(false);

        if (buildingModeRoot != null)
            buildingModeRoot.SetActive(false);

        if (builderModeRoot != null)
            builderModeRoot.SetActive(false);

        if (unitButton != null)
            unitButton.gameObject.SetActive(false);

        HideAllBuilderButtons();
    }

    private void HideAllBuilderButtons()
    {
        ClearSpawnedBuilderButtons();

        if (builderButtonPrefab != null)
            builderButtonPrefab.gameObject.SetActive(false);
    }

    private void ClearSpawnedBuilderButtons()
    {
        if (spawnedBuilderButtons == null)
            return;

        for (int i = 0; i < spawnedBuilderButtons.Length; i++)
        {
            if (spawnedBuilderButtons[i] != null)
            {
                Destroy(spawnedBuilderButtons[i].gameObject);
            }
        }

        spawnedBuilderButtons = new BuilderBuildButtonUI[0];
    }
}