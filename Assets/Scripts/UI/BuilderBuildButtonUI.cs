using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuilderBuildButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text labelText;

    private BuildingDefinition buildingDefinition;
    private BuildingPlacementSystem placementSystem;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(BuildingDefinition definition, BuildingPlacementSystem system)
    {
        buildingDefinition = definition;
        placementSystem = system;

        if (labelText != null)
        {
            labelText.text = definition != null ? definition.buildingName : "Missing";
        }
    }

    private void OnClick()
    {
        if (buildingDefinition == null)
        {
            Debug.LogWarning("BuilderBuildButtonUI: buildingDefinition is null.");
            return;
        }

        if (placementSystem == null)
        {
            Debug.LogWarning("BuilderBuildButtonUI: placementSystem is null.");
            return;
        }

        placementSystem.StartPlacing(buildingDefinition);
    }
}