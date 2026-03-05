using UnityEngine;

public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private BuildSite buildSitePrefab;

    private BuildingDefinition selectedDefinition;
    private GameObject ghost;

    public bool IsPlacing => selectedDefinition != null;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (!IsPlacing) return;

        if (!TryGetMouseGroundPoint(out var point)) return;

        UpdateGhost(point);

        // Left click confirms
        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuildSite(point);
        }

        // Right click cancels
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void StartPlacing(BuildingDefinition def)
    {
        selectedDefinition = def;
        CreateGhost();
    }

    public void CancelPlacement()
    {
        selectedDefinition = null;
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

    private void CreateGhost()
    {
        if (ghost != null) Destroy(ghost);

        ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = "BuildingGhost";
        Destroy(ghost.GetComponent<Collider>());

        // Temporary size (we can tie this to the BuildingDefinition later)
        ghost.transform.localScale = new Vector3(2, 0.5f, 2);
    }

    private void UpdateGhost(Vector3 point)
    {
        if (ghost == null) return;

        ghost.transform.position = new Vector3(point.x, point.y + 0.25f, point.z);
    }

    private void PlaceBuildSite(Vector3 point)
    {
        if (buildSitePrefab == null)
        {
            Debug.LogError("BuildingPlacementSystem: buildSitePrefab not set.");
            return;
        }

        int teamId = 0; // simple for now
        var buildSite = Instantiate(buildSitePrefab, point, Quaternion.identity);
        buildSite.Init(selectedDefinition, teamId);

        Debug.Log($"Placed BuildSite for {selectedDefinition.name} at {point}");
    }
}