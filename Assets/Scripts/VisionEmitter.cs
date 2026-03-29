using UnityEngine;

[DisallowMultipleComponent]
public class VisionEmitter : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float visionRadius = 8f;
    [SerializeField] private int teamId = 0;

    public float VisionRadius => visionRadius;
    public int TeamId => teamId;

    public void SetTeam(int newTeamId)
    {
        teamId = newTeamId;
    }

    private void OnEnable()
    {
        if (VisionManager.Instance != null)
        {
            VisionManager.Instance.RegisterEmitter(this);
        }
    }

    private void Start()
    {
        if (VisionManager.Instance != null)
        {
            VisionManager.Instance.RegisterEmitter(this);
        }
    }

    private void OnDisable()
    {
        if (VisionManager.Instance != null)
        {
            VisionManager.Instance.UnregisterEmitter(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}