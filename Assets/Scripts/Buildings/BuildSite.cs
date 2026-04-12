using UnityEngine;
using UnityEngine.AI;

public class BuildSite : MonoBehaviour
{
    [Header("Definition")]
    public BuildingDefinition definition;
    public int teamId;

    [Header("Construction")]
    [Range(0f, 1f)] public float buildProgress;
    public bool isComplete;

    [Header("Debug / Testing")]
    [SerializeField] private bool completeOnStartForTesting = false;

    [Header("Starting Unit Spawn")]
    [SerializeField] private bool spawnUnitOnComplete = false;
    [SerializeField] private GameObject unitToSpawnOnComplete;
    [SerializeField] private Vector3 unitSpawnOffset = new Vector3(2f, 0f, 2f);

    private Builder activeBuilder;
    private GameObject spawnedBuildingInstance;

    public void Init(BuildingDefinition def, int team)
    {
        definition = def;
        teamId = team;

        ResetSiteState();

        string buildingLabel = (def != null && !string.IsNullOrWhiteSpace(def.buildingName))
            ? def.buildingName
            : "Unknown";

        name = $"BuildSite_{buildingLabel}_T{team}";
    }

    public void SetBuilder(Builder builder)
    {
        activeBuilder = builder;
    }

    public void ResetSiteState()
    {
        buildProgress = 0f;
        isComplete = false;
        activeBuilder = null;
        spawnedBuildingInstance = null;
    }

    private void Start()
    {
        if (completeOnStartForTesting && definition != null && !isComplete)
        {
            buildProgress = 1f;
            CompleteConstruction();
        }
    }

    private void Update()
    {
        // If the spawned building was destroyed, fully reset this site.
        if (isComplete && spawnedBuildingInstance == null)
        {
            ResetSiteState();
            Debug.Log($"{name} reset after spawned building was destroyed.");
            return;
        }

        if (isComplete) return;
        if (activeBuilder == null) return;
        if (!activeBuilder.IsBuilding) return;
        if (activeBuilder.CurrentBuildSite != this) return;
        if (definition == null) return;

        float builderDistance = Vector3.Distance(activeBuilder.transform.position, transform.position);
        if (builderDistance > activeBuilder.buildRange)
        {
            return;
        }

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

        if (activeBuilder != null)
        {
            Vector3 moveAwayDirection = (activeBuilder.transform.position - transform.position).normalized;

            if (moveAwayDirection == Vector3.zero)
            {
                moveAwayDirection = transform.forward;
            }

            Vector3 desiredPosition = transform.position + (moveAwayDirection * 4f);

            activeBuilder.CancelBuild();

            NavMeshAgent agent = activeBuilder.GetComponent<NavMeshAgent>();
            Unit builderUnit = activeBuilder.GetComponent<Unit>();

            if (agent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(desiredPosition, out hit, 3f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);

                    if (builderUnit != null)
                    {
                        builderUnit.CommandMoveTo(hit.position + (moveAwayDirection * 1.5f));
                    }
                }
            }
        }

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

        if (spawnUnitOnComplete && unitToSpawnOnComplete != null)
        {
            Vector3 spawnPosition = transform.position + unitSpawnOffset;

            GameObject spawnedUnit = Instantiate(
                unitToSpawnOnComplete,
                spawnPosition,
                Quaternion.identity
            );

            spawnedUnit.name = $"{unitToSpawnOnComplete.name}_T{teamId}";

            TeamMember teamMember = spawnedUnit.GetComponent<TeamMember>();
            if (teamMember != null)
            {
                teamMember.SetTeam(teamId);
            }

            VisionEmitter visionEmitter = spawnedUnit.GetComponent<VisionEmitter>();
            if (visionEmitter != null)
            {
                visionEmitter.SetTeam(teamId);
            }

            Debug.Log($"{name} spawned starting unit {spawnedUnit.name}");
        }

        Debug.Log($"{name} construction complete. Spawned {completedBuilding.name}");
    }
}