using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public bool IsSelected { get; private set; }

    private Renderer unitRenderer;
    private Color originalColor;
    private NavMeshAgent agent;

    void Awake()
    {
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (IsSelected)
            unitRenderer.material.color = Color.green;
        else
            unitRenderer.material.color = originalColor;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
}