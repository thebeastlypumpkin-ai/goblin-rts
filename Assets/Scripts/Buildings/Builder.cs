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

    /// <summary>
    /// Begin channeling construction at a target build site.
    /// </summary>
    public void BeginBuild(BuildSite site)
    {
        if (site == null)
        {
            Debug.LogWarning("Builder.BeginBuild called with null BuildSite.");
            return;
        }

        currentBuildSite = site;
        currentBuildSite.SetBuilder(this);
        isBuilding = true;
    }

    /// <summary>
    /// Stop channeling construction.
    /// </summary>
    public void CancelBuild()
    {
        isBuilding = false;
        currentBuildSite = null;
    }

    private void Update()
    {
        if (!isBuilding) return;

        if (currentBuildSite == null)
        {
            CancelBuild();
            return;
        }

        if (currentBuildSite.isComplete)
        {
            CancelBuild();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentBuildSite.transform.position);
        if (dist > buildRange)
        {
            CancelBuild();
            Debug.Log("Builder cancelled building");
            return;
        }

        // Keep the site linked continuously while building.
        currentBuildSite.SetBuilder(this);
    }
}