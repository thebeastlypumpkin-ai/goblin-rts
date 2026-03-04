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

    // These will be used in later steps/phases
    private Vector3 _buildSitePosition;

    /// <summary>
    /// Begin channeling construction at a target site. (No actual construction logic yet.)
    /// </summary>
    public void BeginBuild(Vector3 sitePosition)
    {
        _buildSitePosition = sitePosition;
        isBuilding = true;
    }

    /// <summary>
    /// Stop channeling construction.
    /// </summary>
    public void CancelBuild()
    {
        isBuilding = false;
    }

    private void Update()
    {
        if (!isBuilding) return;

        // If builder wanders too far, cancel channeling (Option A rule).
        float dist = Vector3.Distance(transform.position, _buildSitePosition);
        if (dist > buildRange)
        {
            CancelBuild();
        }
    }
}