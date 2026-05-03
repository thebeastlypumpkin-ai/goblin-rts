using UnityEngine;
using UnityEngine.AI;

public class GateController : MonoBehaviour
{
    [Header("Gate State")]
    [SerializeField] private bool isOpen = false;

    [Header("Blocking")]
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private Collider[] blockingColliders;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (navMeshObstacle == null)
            navMeshObstacle = GetComponent<NavMeshObstacle>();

        if (blockingColliders == null || blockingColliders.Length == 0)
            blockingColliders = GetComponentsInChildren<Collider>();

        ApplyGateState();
    }

    public void ToggleGate()
    {
        isOpen = !isOpen;
        ApplyGateState();

        Debug.Log($"{name} gate is now {(isOpen ? "OPEN" : "CLOSED")}");
    }

    private void ApplyGateState()
    {
        if (navMeshObstacle != null)
            navMeshObstacle.enabled = !isOpen;

        if (blockingColliders == null) return;

        for (int i = 0; i < blockingColliders.Length; i++)
        {
            if (blockingColliders[i] != null)
                blockingColliders[i].enabled = !isOpen;
        }
    }
}