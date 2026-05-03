using UnityEngine;
using GoblinRTS.Economy;

public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private BuildSite buildSitePrefab;
    [SerializeField] private Builder builder;
    

    private Builder activeBuilder;
    private BuildingDefinition selectedDefinition;
    private GameObject ghost;

    private float currentRotationY = 0f;

    public bool IsPlacing => selectedDefinition != null;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {

        if (!IsPlacing) return;

        HandleRotationInput();

        if (!TryGetMouseGroundPoint(out var rawPoint))
        {
            Debug.LogWarning("No valid ground point found for placement.");
            return;
        }

        Vector3 finalPoint = GetPlacementPoint(rawPoint);

        UpdateGhost(finalPoint);

        // Left click confirms
        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuildSite(finalPoint);
        }

        // Right click cancels
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void StartPlacing(BuildingDefinition def)
    {
        if (def == null)
        {
            Debug.LogError("BuildingPlacementSystem: StartPlacing called with null BuildingDefinition.");
            selectedDefinition = null;
            activeBuilder = null;
            return;
        }

        if (SelectionManager.Instance == null)
        {
            Debug.LogError("BuildingPlacementSystem: SelectionManager instance is missing.");
            selectedDefinition = null;
            activeBuilder = null;
            return;
        }

        var selectedUnits = SelectionManager.Instance.GetSelectedUnits();

        if (selectedUnits == null || selectedUnits.Count == 0)
        {
            Debug.LogWarning("BuildingPlacementSystem: no unit selected when starting placement.");
            selectedDefinition = null;
            activeBuilder = null;
            return;
        }

        Builder foundBuilder = selectedUnits[0].GetComponent<Builder>();

        if (foundBuilder == null)
        {
            Debug.LogWarning("BuildingPlacementSystem: selected unit is not a builder.");
            selectedDefinition = null;
            activeBuilder = null;
            return;
        }

        selectedDefinition = def;
        activeBuilder = foundBuilder;
        currentRotationY = 0f;
        CreateGhost();
    }

    public void CancelPlacement()
    {
        selectedDefinition = null;
        activeBuilder = null;
        currentRotationY = 0f;

        if (ghost != null) Destroy(ghost);
        ghost = null;
    }

    private bool TryGetMouseGroundPoint(out Vector3 point)
    {
        point = default;
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, 1000f, groundMask))
        {
            point = hit.point;
            return true;
        }

        return false;
    }

    private void HandleRotationInput()
    {
        if (selectedDefinition == null) return;
        if (!selectedDefinition.isWallSegment) return;

        float step = Mathf.Max(1f, selectedDefinition.wallRotationStep);

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentRotationY += step;
            if (currentRotationY >= 360f)
                currentRotationY -= 360f;
        }
    }

    private Vector3 GetPlacementPoint(Vector3 rawPoint)
    {
        if (selectedDefinition == null)
            return rawPoint;

        if (!selectedDefinition.isWallSegment)
            return rawPoint;

        float snap = Mathf.Max(0.1f, selectedDefinition.wallSegmentLength * 0.5f);

        float snappedX = Mathf.Round(rawPoint.x / snap) * snap;
        float snappedZ = Mathf.Round(rawPoint.z / snap) * snap;

        return new Vector3(snappedX, rawPoint.y, snappedZ);
    }

    private void CreateGhost()
    {
        if (ghost != null) Destroy(ghost);

        ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = "BuildingGhost";

        Collider ghostCollider = ghost.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            Destroy(ghostCollider);
        }

        ghost.transform.localScale = new Vector3(2, 0.5f, 2);
    }

    private void UpdateGhost(Vector3 point)
    {
        if (ghost == null) return;

        ghost.transform.position = new Vector3(point.x, point.y + 0.25f, point.z);
        ghost.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

        if (selectedDefinition != null && selectedDefinition.isWallSegment)
        {
            float length = Mathf.Max(0.1f, selectedDefinition.wallSegmentLength);
            ghost.transform.localScale = new Vector3(0.75f, 2f, length);
        }
        else
        {
            ghost.transform.localScale = new Vector3(2, 0.5f, 2);
        }
    }

    private void PlaceBuildSite(Vector3 point)
    {
        if (buildSitePrefab == null)
        {
            Debug.LogError("BuildingPlacementSystem: buildSitePrefab not set.");
            return;
        }

        if (selectedDefinition == null)
        {
            Debug.LogError("BuildingPlacementSystem: no selected building definition.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("BuildingPlacementSystem: GameManager missing.");
            return;
        }

        if (activeBuilder == null)
        {
            Debug.LogError("BuildingPlacementSystem: activeBuilder is missing.");
            return;
        }

        if (activeBuilder.IsBuilding)
        {
            Debug.LogWarning("BuildingPlacementSystem: builder is already constructing.");
            return;
        }

        TeamMember builderTeam = activeBuilder.GetComponent<TeamMember>();
        if (builderTeam == null)
        {
            Debug.LogError("BuildingPlacementSystem: selected builder is missing TeamMember.");
            return;
        }

        int teamId = (int)builderTeam.Team;

        if (!GameManager.Instance.TrySpendTeamEssence(teamId, selectedDefinition.cost))
        {
            Debug.Log($"Not enough Essence to place {selectedDefinition.buildingName}. Cost: {selectedDefinition.cost}, Current: {GameManager.Instance.GetTeamEssence(teamId)}, Team: {teamId}");
            return;
        }

        Quaternion spawnRotation = Quaternion.Euler(0f, currentRotationY, 0f);

        var buildSite = Instantiate(buildSitePrefab, point, spawnRotation);
        buildSite.Init(selectedDefinition, teamId);

        activeBuilder.BeginBuild(buildSite);

        Debug.Log($"Placed BuildSite for {selectedDefinition.buildingName} at {point} with rotation {currentRotationY}");

        CancelPlacement();
    }
}