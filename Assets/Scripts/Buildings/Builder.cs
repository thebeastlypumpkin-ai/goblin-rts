using UnityEngine;

[DisallowMultipleComponent]
public class Builder : MonoBehaviour
{
    [Header("Build Settings")]
    [Tooltip("How close the builder must be to the build site to start/continue channeling.")]
    [Min(0.1f)] public float buildRange = 2.0f;

    [Tooltip("If true, the builder is currently channeling construction.")]
    public bool IsBuilding => isBuilding;

    [SerializeField] private bool isBuilding;

    private BuildSite currentBuildSite;
    private Unit unit;
    private bool isMovingToBuildSite;

    private void Awake()
    {
        unit = GetComponent<Unit>();

        if (unit == null)
        {
            Debug.LogError($"{name} Builder requires a Unit component on the same GameObject.");
        }
    }

    public void BeginBuild(BuildSite site)
    {
        Debug.Log($"[Builder] BeginBuild called on {name} for site {(site != null ? site.name : "NULL")}");

        if (site == null)
        {
            Debug.LogWarning("Builder.BeginBuild called with null BuildSite.");
            return;
        }

        currentBuildSite = site;
        isBuilding = false;
        isMovingToBuildSite = true;

        if (unit != null)
        {
            unit.CommandMoveTo(site.transform.position);
        }

        Debug.Log($"Builder moving to build site: {site.name}");
    }

    public void CancelBuild()
    {
        Debug.Log("Builder.CancelBuild called.");

        isBuilding = false;
        isMovingToBuildSite = false;
        currentBuildSite = null;
    }

    private void Update()
    {
        if (currentBuildSite == null)
            return;

        if (currentBuildSite.isComplete)
        {
            Debug.Log("Builder saw currentBuildSite.isComplete == true, canceling build.");
            CancelBuild();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentBuildSite.transform.position);

        // Step 1: move until in range
        if (isMovingToBuildSite)
        {
            if (dist <= buildRange)
            {
                isMovingToBuildSite = false;
                isBuilding = true;
                currentBuildSite.SetBuilder(this);

                Debug.Log($"Builder reached site and started construction: {currentBuildSite.name}");
            }

            return;
        }

        // Step 2: actively build while in range
        if (!isBuilding)
            return;

        if (dist > buildRange)
        {
            Debug.Log($"Builder moved out of range. Distance={dist}, Range={buildRange}");
            CancelBuild();
            return;
        }

        currentBuildSite.SetBuilder(this);
    }
}