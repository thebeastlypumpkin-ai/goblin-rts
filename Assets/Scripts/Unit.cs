using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
 
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public bool IsSelected { get; private set; }

    private Renderer unitRenderer;
    private Color originalColor;
    private NavMeshAgent agent;

    void Awake()
    {
        currentHealth = maxHealth;
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
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= amount;

        Debug.Log(name + " took " + amount + " damage. Current HP: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    private void Die()
    {
        Debug.Log(name + " has died.");

        // Disable movement
        if (agent != null)
            agent.isStopped = true;

        // Change color to indicate death
        unitRenderer.material.color = Color.red;

        // Disable further interaction
        enabled = false;
    }
}