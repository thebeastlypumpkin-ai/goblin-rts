using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
 
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attacksPerSecond = 1f;

    public bool IsSelected { get; private set; }

    public bool IsDead => isDead;

    private Renderer unitRenderer;
    private Color originalColor;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private Unit currentTarget;
    private bool isDead;
    private TeamMember teamMember;

    void Awake()
    {
        currentHealth = maxHealth;
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;
        agent = GetComponent<NavMeshAgent>();
        teamMember = GetComponent<TeamMember>();
    }

    void Update()
    {
        if (isDead) return;

        if (currentTarget == null)
            return;

        // If target died, clear it
        if (currentTarget.IsDead)
        {
            ClearTarget();
            return;
        }

        // Safety: if target is not an enemy, drop it
        if (teamMember != null)
        {
            TeamMember otherTeam = currentTarget.GetComponent<TeamMember>();
            if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
            {
                ClearTarget();
                return;
            }
        }

        // Check distance
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distance > attackRange)
        {
            // Chase into range
            MoveTo(currentTarget.transform.position);
            return;
        }

        // In range: stop moving and attack on cooldown
        if (agent != null)
            agent.isStopped = true;

        float attackCooldown = 1f / attacksPerSecond;

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            currentTarget.TakeDamage(attackDamage, this);
        }
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
            agent.isStopped = false; // ✅ ensure movement resumes
            agent.SetDestination(destination);
        }
    }
    public void SetTarget(Unit target)
    {
        if (isDead) return;
        if (target == null) return;

        if (teamMember != null)
        {
            TeamMember otherTeam = target.GetComponent<TeamMember>();
            if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
                return;
        }

        currentTarget = target;
    }
    public void ClearTarget()
    {
        currentTarget = null;
    }

    public void TakeDamage(float amount, Unit attacker)
    {
        if (currentHealth <= 0f)
            return;

        // Friendly-fire prevention (damage-layer failsafe)
        if (attacker != null && attacker != this)
        {
            TeamMember attackerTeam = attacker.GetComponent<TeamMember>();
            TeamMember myTeam = GetComponent<TeamMember>();

            if (attackerTeam != null && myTeam != null && !myTeam.IsEnemy(attackerTeam))
                return;
        }

        currentHealth -= amount;

        Debug.Log(name + " took " + amount + " damage. Current HP: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    private void Die()
    {
        if (isDead) return;   // Prevent double execution
        isDead = true;

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