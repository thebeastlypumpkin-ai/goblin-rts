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

    public void BeginBuild(BuildSite site)
    {
        if (site == null)
        {
            Debug.LogWarning("Builder.BeginBuild called with null BuildSite.");
            return;
        }

        Debug.Log($"Builder.BeginBuild called on site: {site.name}");

        currentBuildSite = site;
        currentBuildSite.SetBuilder(this);
        isBuilding = true;

        Debug.Log($"Builder isBuilding set to TRUE for site: {currentBuildSite.name}");
    }

    public void CancelBuild()
    {
        Debug.Log("Builder.CancelBuild called.");

        isBuilding = false;
        currentBuildSite = null;
    }

    private void Update()
    {
        if (!isBuilding) return;

        if (currentBuildSite == null)
        {
            Debug.Log("Builder currentBuildSite was null, canceling build.");
            CancelBuild();
            return;
        }

        if (currentBuildSite.isComplete)
        {
            Debug.Log("Builder saw currentBuildSite.isComplete == true, canceling build.");
            CancelBuild();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentBuildSite.transform.position);
        if (dist > buildRange)
        {
            Debug.Log($"Builder out of range. Distance={dist}, Range={buildRange}");
            CancelBuild();
            return;
        }

        currentBuildSite.SetBuilder(this);
    }
}