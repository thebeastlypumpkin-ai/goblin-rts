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

    public void Init(BuildingDefinition def, int team)
    {
        definition = def;
        teamId = team;

        buildProgress = 0f;
        isComplete = false;

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
        isComplete = true;

        if (definition.completedBuildingPrefab == null)
        {
            Debug.LogError($"{name} cannot complete because completedBuildingPrefab is missing on BuildingDefinition.");
            return;
        }

        GameObject completedBuilding = Instantiate(
            definition.completedBuildingPrefab,
            transform.position,
            transform.rotation
        );

        string buildingLabel = !string.IsNullOrWhiteSpace(definition.buildingName)
            ? definition.buildingName
            : definition.name;

        completedBuilding.name = $"{buildingLabel}_T{teamId}";

        Debug.Log($"{name} construction complete. Spawned {completedBuilding.name}");

        Destroy(gameObject);
    }
}