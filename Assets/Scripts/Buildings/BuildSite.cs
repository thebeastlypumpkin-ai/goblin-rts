using UnityEngine;

public class BuildSite : MonoBehaviour
{
    [Header("Definition")]
    public BuildingDefinition definition;
    public int teamId;

    [Header("Construction")]
    [Range(0f, 1f)] public float buildProgress;
    public bool isComplete;

    private Builder activeBuilder;
    private GameObject spawnedBuildingInstance;

    public void Init(BuildingDefinition def, int team)
    {
        definition = def;
        teamId = team;

        buildProgress = 0f;
        isComplete = false;
        activeBuilder = null;
        spawnedBuildingInstance = null;

        string buildingLabel = (def != null && !string.IsNullOrWhiteSpace(def.buildingName))
            ? def.buildingName
            : "Unknown";

        name = $"BuildSite_{buildingLabel}_T{team}";
    }

    public void SetBuilder(Builder builder)
    {
        activeBuilder = builder;
    }

    private void Update()
    {
        // If the spawned building was destroyed, clear the reference and reset this site.
        if (isComplete && spawnedBuildingInstance == null)
        {
            buildProgress = 0f;
            isComplete = false;
            activeBuilder = null;
            return;
        }

        if (isComplete) return;
        if (activeBuilder == null) return;
        if (!activeBuilder.IsBuilding) return;
        if (definition == null) return;

        float buildTime = definition.buildTimeSeconds;
        if (buildTime <= 0f) return;

        buildProgress += Time.deltaTime / buildTime;

        if (buildProgress >= 1f)
        {
            buildProgress = 1f;
            CompleteConstruction();
        }
    }

    private void CompleteConstruction()
    {
        if (isComplete) return;

        if (definition.completedBuildingPrefab == null)
        {
            Debug.LogError($"{name} cannot complete because completedBuildingPrefab is missing on BuildingDefinition.");
            return;
        }

        if (spawnedBuildingInstance != null)
        {
            Debug.LogWarning($"{name} already has a spawned building instance.");
            return;
        }

        isComplete = true;

        GameObject completedBuilding = Instantiate(
            definition.completedBuildingPrefab,
            transform.position,
            transform.rotation
        );

        spawnedBuildingInstance = completedBuilding;

        string buildingLabel = !string.IsNullOrWhiteSpace(definition.buildingName)
            ? definition.buildingName
            : definition.name;

        completedBuilding.name = $"{buildingLabel}_T{teamId}";

        Building building = completedBuilding.GetComponent<Building>();
        if (building == null)
        {
            Debug.LogError($"{completedBuilding.name} is missing a Building component.");
        }
        else
        {
            building.Init(definition, teamId, this);
        }

        Debug.Log($"{name} construction complete. Spawned {completedBuilding.name}");
    }
}