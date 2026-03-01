using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Unit : MonoBehaviour
 
{
    public static List<Unit> ActiveUnits = new List<Unit>();
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float detectionRange = 8f;

    public bool IsSelected { get; private set; }

    public bool IsDead => isDead;

    private Renderer unitRenderer;
    private Color originalColor;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private Unit currentTarget;
    private bool isDead;
    private TeamMember teamMember;
    private float nextScanTime;
    [SerializeField] private float scanInterval = 0.25f;
    private float ignoreAggroUntilTime;
    [SerializeField] private float manualAttackLeashRange = 25f;
    private bool hasManualAttackOrder;
    private bool hasMoveOrder;
    private Vector3 moveOrderDestination;
    [SerializeField] private float moveArrivalThreshold = 0.5f;

    private void OnDestroy()
    {
        ActiveUnits.Remove(this);
    }

    void Awake()
    {
        currentHealth = maxHealth;
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;
        agent = GetComponent<NavMeshAgent>();
        teamMember = GetComponent<TeamMember>();
        ActiveUnits.Add(this);
    }

    void Update()
    {
        if (isDead) return;

        if (hasMoveOrder)
        {
            float sqr = (transform.position - moveOrderDestination).sqrMagnitude;
            if (sqr <= moveArrivalThreshold * moveArrivalThreshold)
            {
                hasMoveOrder = false;
                ignoreAggroUntilTime = 0f; // aggro allowed again after arriving
            }
        }

        // Periodic enemy scan
        if (Time.time >= nextScanTime && Time.time >= ignoreAggroUntilTime)
        {
            nextScanTime = Time.time + scanInterval;

            if (currentTarget == null)
            {
                Unit found = FindClosestEnemyInRange();
                if (found != null)
                    currentTarget = found;
            }
        }

        if (currentTarget == null)
            return;

        if (currentTarget.IsDead)
        {
            hasManualAttackOrder = false;
            ClearTarget();
            return;
        }

        // Safety: if target is not an enemy, drop it
        if (teamMember != null)
        {
            TeamMember otherTeam = currentTarget.GetComponent<TeamMember>();
            if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
            {
                hasManualAttackOrder = false;
                ClearTarget();
                return;
            }
        }

        // If target wandered too far away, drop it (manual orders get a bigger leash)
        float leash = hasManualAttackOrder ? manualAttackLeashRange : detectionRange;

        float sqrDist = (currentTarget.transform.position - transform.position).sqrMagnitude;
        if (sqrDist > leash * leash)
        {
            hasManualAttackOrder = false;
            ClearTarget();
            return;
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
            agent.isStopped = false; // ensure movement resumes
            agent.SetDestination(destination);
        }
    }

    public void CommandMoveTo(Vector3 destination)
    {
        hasManualAttackOrder = false;

        hasMoveOrder = true;
        moveOrderDestination = destination;

        ignoreAggroUntilTime = float.PositiveInfinity; // ignore aggro until we arrive
        ClearTarget();

        MoveTo(destination);
    }

    public void CommandAttack(Unit target)
    {
        hasMoveOrder = false;

        ignoreAggroUntilTime = 0f;
        hasManualAttackOrder = true;

        SetTarget(target);
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
    private Unit FindClosestEnemyInRange()
    {
        Unit closest = null;
        float closestSqrDist = float.MaxValue;
        float rangeSqr = detectionRange * detectionRange;

        foreach (Unit u in ActiveUnits)
        {
            if (u == null || u == this) continue;
            if (u.IsDead) continue;

            // Team filtering (only enemies)
            if (teamMember != null)
            {
                TeamMember otherTeam = u.GetComponent<TeamMember>();
                if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
                    continue;
            }

            float sqrDist = (u.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > rangeSqr) continue;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = u;
            }
        }

        return closest;
    }
    private void Die()
    {
        if (isDead) return;   // Prevent double execution
        isDead = true;

        hasManualAttackOrder = false;
        currentTarget = null;

        ActiveUnits.Remove(this);

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