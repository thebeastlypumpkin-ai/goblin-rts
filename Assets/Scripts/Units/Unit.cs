using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;
using GoblinRTS.Economy;
using System.Collections;

public class Unit : MonoBehaviour
{
    public static List<Unit> ActiveUnits = new List<Unit>();

    [Header("Health")]
    [SerializeField] private float maxHealth = 200f;
    private float currentHealth;

    [Header("Supply")]
    [SerializeField] private int supplyCost = 1;
    private bool supplyReserved = false;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float buildingAttackRangeBonus = 1.5f;
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float combatAnchorLeashRange = 3f;
    [SerializeField] private float blockedCombatWaitDistance = 4f;
    [Header("Data")]
    [SerializeField] private UnitDefinition unitDefinition;
    [Header("Status Runtime")]
    [SerializeField] private StatusEffectType activeStatusEffect = StatusEffectType.None;
    [SerializeField] private float activeStatusTimer = 0f;
    [SerializeField] private float activeStatusStrength = 0f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthNormalized => (maxHealth <= 0f) ? 0f : (currentHealth / maxHealth);
    public int SupplyCost => supplyCost;
    public UnitDefinition UnitDefinition => unitDefinition;
    public Unit CurrentTarget => currentTarget;
    public Building TargetBuilding => targetBuilding;
    public UnitTag PrimaryTag => (unitDefinition != null) ? unitDefinition.primaryTag : UnitTag.None;
    public bool UsesAoEAttack => unitDefinition != null && unitDefinition.usesAoEAttack;
    public float AoERadius => unitDefinition != null ? unitDefinition.aoeRadius : 0f;
    public float AoEDamageMultiplier => unitDefinition != null ? unitDefinition.aoeDamageMultiplier : 1f;

    public bool IsInAttackRange
    {
        get
        {
            if (currentTarget == null) return false;

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            return distance <= attackRange;
        }
    }

    // UI / systems can subscribe to these
    public event Action<float> OnHealthChanged; // sends HealthNormalized (0..1)
    public event Action<Unit> OnDied;
    public event Action<Unit> OnAttackStarted;
    public event Action<Unit, Unit, float> OnAttackLanded;
    public event Action<Unit, float, Unit> OnDamageTaken;
    public event Action<Unit, Unit> OnUnitKilled;
    public event Action<Unit, StatusEffectType, float, float> OnStatusApplied;

    public bool IsSelected { get; private set; }
    public bool IsDead => isDead;

    private Renderer unitRenderer;
    private Color originalColor;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private Unit currentTarget;
    private Building targetBuilding;
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
    private Vector3 patrolPointA;
    private Vector3 patrolPointB;
    private bool patrolToB = true;
    private bool isPatrolling = false;
    private Vector3 combatAnchorPosition;
    private bool hasCombatAnchor = false;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 2f, 0f);

    private GameObject healthBarInstance;
    private Transform healthBarTransform;
    private CommandType currentCommand = CommandType.None;

    public enum CommandType
    {
        None,
        Move,
        Attack,
        Stop,
        Hold,
        Patrol
    }

    private void OnEnable()
    {
        if (GameTickSystem.Instance != null)
            GameTickSystem.Instance.OnTick += HandleTick;
    }

    private void OnDisable()
    {
        if (GameTickSystem.Instance != null)
            GameTickSystem.Instance.OnTick -= HandleTick;
    }

    private void HandleTick()
    {
        if (Time.time >= nextScanTime && Time.time >= ignoreAggroUntilTime)
        {
            nextScanTime = Time.time + scanInterval;

            if (currentTarget == null && targetBuilding == null)
            {
                Unit foundUnit = FindClosestEnemyInRange();

                if (foundUnit != null)
                {
                    currentTarget = foundUnit;
                }
                else
                {
                    Building foundBuilding = FindClosestEnemyBuildingInRange();
                    if (foundBuilding != null)
                        targetBuilding = foundBuilding;
                }
            }
        }

        if (currentTarget == null && targetBuilding == null)
            return;

        if (currentTarget != null && currentTarget.IsDead)
        {
            hasManualAttackOrder = false;
            ClearTarget();
            return;
        }

        if (targetBuilding != null && targetBuilding.IsDestroyed)
        {
            hasManualAttackOrder = false;
            ClearTarget();
            return;
        }

        if (teamMember != null)
        {
            if (currentTarget != null)
            {
                TeamMember otherTeam = currentTarget.GetComponent<TeamMember>();
                if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
                {
                    hasManualAttackOrder = false;
                    ClearTarget();
                    return;
                }
            }

            if (targetBuilding != null)
            {
                TeamMember otherTeam = targetBuilding.GetComponent<TeamMember>();
                if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
                {
                    hasManualAttackOrder = false;
                    ClearTarget();
                    return;
                }
            }
        }

        float leash = hasManualAttackOrder ? manualAttackLeashRange : detectionRange;

        Vector3 activeTargetPosition = currentTarget != null
    ? currentTarget.transform.position
    : GetBuildingAttackPoint(targetBuilding);

        float sqrDist = (activeTargetPosition - transform.position).sqrMagnitude;
        if (sqrDist > leash * leash)
        {
            hasManualAttackOrder = false;
            ClearTarget();
            return;
        }

        float sqrDistToTarget = (activeTargetPosition - transform.position).sqrMagnitude;
        float effectiveAttackRange = targetBuilding != null
    ? attackRange + buildingAttackRangeBonus
    : attackRange;

        float attackRangeSqr = effectiveAttackRange * effectiveAttackRange;

        if (sqrDistToTarget > attackRangeSqr)
        {
            if (hasCombatAnchor && !hasManualAttackOrder)
            {
                float anchorSqr = (combatAnchorPosition - transform.position).sqrMagnitude;
                float leashSqr = combatAnchorLeashRange * combatAnchorLeashRange;

                if (anchorSqr > leashSqr)
                {
                    if (agent != null)
                    {
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                    }

                    return;
                }
            }

            float blockedWaitDistanceSqr = blockedCombatWaitDistance * blockedCombatWaitDistance;

            if (currentTarget != null && IsFriendlyUnitBlockingTarget(currentTarget) && sqrDistToTarget <= blockedWaitDistanceSqr)
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }

                return;
            }

            if (agent != null)
                agent.stoppingDistance = 0.1f;

            MoveTo(activeTargetPosition);
            return;
        }

        if (agent != null)
            agent.isStopped = true;

        float attackCooldown = 1f / attacksPerSecond;

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (currentTarget != null)
            {
                float finalDamage = attackDamage * GetDamageMultiplierAgainst(currentTarget);

                currentTarget.TakeDamage(finalDamage, this);

                OnAttackLanded?.Invoke(this, currentTarget, finalDamage);

                ApplyAoEDamage(currentTarget, finalDamage);
            }

            else if (targetBuilding != null)
            {
                if (targetBuilding.IsDestroyed)
                {
                    hasManualAttackOrder = false;
                    ClearTarget();
                    return;
                }

                targetBuilding.TakeDamage(attackDamage);
            }
        }
    }

    public void CommandPatrol(Vector3 pointA, Vector3 pointB)
    {
        currentCommand = CommandType.Patrol;

        patrolPointA = pointA;
        patrolPointB = pointB;
        patrolToB = true;
        isPatrolling = true;

        currentTarget = null;
        hasMoveOrder = false;
        hasManualAttackOrder = false;

        if (agent != null)
        {
            agent.SetDestination(patrolPointB);
        }
    }

    private void OnDestroy()
    {
        ActiveUnits.Remove(this);

        if (supplyReserved && SupplyManager.Instance != null)
        {
            SupplyManager.Instance.ReleaseSupply(supplyCost);
            supplyReserved = false;
        }

        if (healthBarInstance != null)
            Destroy(healthBarInstance);
    }

    void Awake()
    {

        currentHealth = maxHealth;
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;
        agent = GetComponent<NavMeshAgent>();
        teamMember = GetComponent<TeamMember>();
        ActiveUnits.Add(this);
        OnHealthChanged?.Invoke(HealthNormalized);

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBarTransform = healthBarInstance.transform;

            HealthBarUI ui = healthBarInstance.GetComponent<HealthBarUI>();
            if (ui != null)
                ui.Bind(this);
            else
                Debug.LogWarning("HealthBar prefab missing HealthBarUI component.");
        }
    }

    private IEnumerator Start()
    {
        yield return null; // wait one frame so Fortress sets supply first

        if (SupplyManager.Instance != null)
        {
            bool reserved = SupplyManager.Instance.TryConsumeSupply(supplyCost);

            if (!reserved)
            {
                Debug.LogWarning($"[Supply] Not enough supply for unit {name}. Needed {supplyCost}, but only {SupplyManager.Instance.FreeSupply} free.");
                Destroy(gameObject);
                yield break;
            }

            supplyReserved = true;
        }
    }

    public void ApplyStatusEffect(StatusEffectType newEffect, float duration, float strength)
    {
        if (newEffect == StatusEffectType.None)
            return;

        // Rule 1: different status types replace the old one
        if (activeStatusEffect != newEffect)
        {
            activeStatusEffect = newEffect;
            activeStatusTimer = duration;
            activeStatusStrength = strength;
            OnStatusApplied?.Invoke(this, activeStatusEffect, activeStatusTimer, activeStatusStrength);
            return;
        }

        // Rule 2: same status type refreshes duration to the higher value
        activeStatusTimer = Mathf.Max(activeStatusTimer, duration);

        // Rule 3: same status type keeps the strongest version
        activeStatusStrength = Mathf.Max(activeStatusStrength, strength);

        OnStatusApplied?.Invoke(this, activeStatusEffect, activeStatusTimer, activeStatusStrength);
    }

    private void ApplyAoEDamage(Unit primaryTarget, float primaryDamage)
    {
        if (!UsesAoEAttack)
            return;

        if (AoERadius <= 0f)
            return;

        float splashDamage = primaryDamage * AoEDamageMultiplier;

        Collider[] hits = Physics.OverlapSphere(primaryTarget.transform.position, AoERadius);

        for (int i = 0; i < hits.Length; i++)
        {
            Unit nearbyUnit = hits[i].GetComponentInParent<Unit>();

            if (nearbyUnit == null) continue;
            if (nearbyUnit == this) continue;
            if (nearbyUnit == primaryTarget) continue;
            if (nearbyUnit.IsDead) continue;

            TeamMember myTeam = teamMember;
            TeamMember otherTeam = nearbyUnit.teamMember;

            if (myTeam != null && otherTeam != null && !myTeam.IsEnemy(otherTeam))
                continue;

            nearbyUnit.TakeDamage(splashDamage, this);
        }
    }

    private Unit FindClosestEnemyInRadius(float radius)
    {
        Unit closest = null;
        float closestSqrDist = float.MaxValue;
        float radiusSqr = radius * radius;

        foreach (Unit u in ActiveUnits)
        {
            if (u == null || u == this) continue;
            if (u.IsDead) continue;

            if (teamMember != null)
            {
                TeamMember otherTeam = u.teamMember;
                if (otherTeam != null && !teamMember.IsEnemy(otherTeam))
                    continue;
            }

            float sqrDist = (u.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > radiusSqr) continue;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = u;
            }
        }

        return closest;
    }

    private bool IsFriendlyUnitBlockingTarget(Unit target)
    {
        if (target == null || teamMember == null)
            return false;

        Vector3 toTarget = target.transform.position - transform.position;
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= attackRange)
            return false;

        Vector3 directionToTarget = toTarget.normalized;

        foreach (Unit u in ActiveUnits)
        {
            if (u == null || u == this || u == target) continue;
            if (u.IsDead) continue;

            TeamMember otherTeam = u.teamMember;
            if (otherTeam == null) continue;
            if (teamMember.IsEnemy(otherTeam)) continue;

            Vector3 toFriendly = u.transform.position - transform.position;
            float friendlyDistance = toFriendly.magnitude;

            if (friendlyDistance >= distanceToTarget)
                continue;

            Vector3 friendlyDirection = toFriendly.normalized;
            float alignment = Vector3.Dot(directionToTarget, friendlyDirection);

            if (alignment < 0.8f)
                continue;

            float lateralOffset = Vector3.Cross(directionToTarget, toFriendly).magnitude;

            if (lateralOffset <= 1.0f)
                return true;
        }

        return false;
    }

    void Update()
    {
        if (isDead) return;

        if (activeStatusEffect != StatusEffectType.None)
        {
            activeStatusTimer -= Time.deltaTime;

            if (activeStatusTimer <= 0f)
            {
                activeStatusEffect = StatusEffectType.None;
                activeStatusTimer = 0f;
                activeStatusStrength = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.X) && IsSelected)
        {
            CommandStop();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9) && IsSelected)
        {
            CommandHold();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) && IsSelected)
        {
            Vector3 startPoint = transform.position;
            Vector3 endPoint = transform.position + transform.forward * 8f;
            CommandPatrol(startPoint, endPoint);
        }

        if (hasMoveOrder)
        {
            float sqr = (transform.position - moveOrderDestination).sqrMagnitude;
            if (sqr <= moveArrivalThreshold * moveArrivalThreshold)
            {
                hasMoveOrder = false;
                ignoreAggroUntilTime = 0f; // aggro allowed again after arriving
            }
        }

        if (isPatrolling && agent != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                patrolToB = !patrolToB;
                agent.SetDestination(patrolToB ? patrolPointB : patrolPointA);
            }
        }

    }

    public void CommandHold()
    {
        currentCommand = CommandType.Hold;

        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        currentTarget = null;
        hasMoveOrder = false;
        hasManualAttackOrder = false;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (IsSelected)
        {
            unitRenderer.material.color = Color.green;
        }
        else
        {
            TeamColorApplier colorApplier = GetComponent<TeamColorApplier>();

            if (colorApplier != null)
            {
                colorApplier.ApplyColor();
            }
            else
            {
                unitRenderer.material.color = originalColor;
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false; // ensure movement resumes
            agent.SetDestination(destination);
        }
    }

    public void CommandStop()
    {
        currentCommand = CommandType.Stop;

        // stop movement
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // clear combat
        currentTarget = null;
        hasManualAttackOrder = false;
        hasCombatAnchor = false;
        hasMoveOrder = false;
    }

    public void CommandMoveTo(Vector3 destination)
    {

        Builder builder = GetComponent<Builder>();
        if (builder != null && builder.IsBuilding)
        {
            builder.CancelBuild();
        }

        currentCommand = CommandType.Move;

        hasManualAttackOrder = false;
        hasCombatAnchor = false;

        hasMoveOrder = true;
        moveOrderDestination = destination;

        ignoreAggroUntilTime = float.PositiveInfinity; // ignore aggro until we arrive
        ClearTarget();

        if (agent != null)
        {
            agent.stoppingDistance = 0.1f;
        }

        MoveTo(destination);
    }

    public bool CommandAttack(Unit target)
    {
        currentCommand = CommandType.Attack;

        if (target == null) return false;

        // Manual attack interrupts move order
        hasMoveOrder = false;

        ignoreAggroUntilTime = 0f;
        hasManualAttackOrder = true;

        combatAnchorPosition = transform.position;
        hasCombatAnchor = true;

        bool accepted = SetTarget(target);

        if (accepted)
        {
            OnAttackStarted?.Invoke(target);
        }

        if (!accepted)
        {
            TeamMember myTeam = teamMember;
            TeamMember tTeam = target.teamMember;
            Debug.LogWarning($"{name} refused target {target.name}. MyTeam={(myTeam ? myTeam.Team.ToString() : "NONE")} TargetTeam={(tTeam ? tTeam.Team.ToString() : "NONE")}");
        }

        return accepted;
    }

    public bool CommandAttackBuilding(Building target)
    {
        currentCommand = CommandType.Attack;

        if (target == null) return false;

        hasMoveOrder = false;

        ignoreAggroUntilTime = 0f;
        hasManualAttackOrder = true;

        combatAnchorPosition = transform.position;
        hasCombatAnchor = true;

        bool accepted = SetBuildingTarget(target);

        return accepted;
    }

    public bool SetTarget(Unit target)
    {
        if (isDead) return false;
        if (target == null) return false;

        if (teamMember != null)
        {
            TeamMember otherTeam = target.teamMember;

            // If either is missing a TeamMember, we will NOT allow targeting (forces consistent setup)
            if (otherTeam == null)
                return false;

            if (!teamMember.IsEnemy(otherTeam))
                return false;
        }

        currentTarget = target;
        return true;
    }

    public bool SetBuildingTarget(Building target)
    {
        if (isDead) return false;
        if (target == null) return false;

        TeamMember myTeam = teamMember;
        TeamMember targetTeam = target.GetComponent<TeamMember>();

        if (myTeam != null)
        {
            if (targetTeam == null)
                return false;

            if (!myTeam.IsEnemy(targetTeam))
                return false;
        }

        currentTarget = null;
        targetBuilding = target;
        return true;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        targetBuilding = null;
        hasCombatAnchor = false;
    }

    public void TakeDamage(float amount, Unit attacker)
    {
        if (currentHealth <= 0f)
            return;

        // Friendly-fire prevention (damage-layer failsafe)
        if (attacker != null && attacker != this)
        {
            TeamMember attackerTeam = attacker.teamMember;
            TeamMember myTeam = teamMember;

            if (attackerTeam != null && myTeam != null && !myTeam.IsEnemy(attackerTeam))
                return;
        }

        currentHealth -= amount;
        OnDamageTaken?.Invoke(this, amount, attacker);
        OnHealthChanged?.Invoke(HealthNormalized);

        Debug.Log(name + " took " + amount + " damage. Current HP: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private Vector3 GetBuildingAttackPoint(Building building)
    {
        if (building == null)
            return transform.position;

        Collider[] colliders = building.GetComponentsInChildren<Collider>();

        if (colliders == null || colliders.Length == 0)
            return building.transform.position;

        Vector3 closestPoint = building.transform.position;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null || !col.enabled)
                continue;

            Vector3 point = col.ClosestPoint(transform.position);
            float sqrDist = (point - transform.position).sqrMagnitude;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestPoint = point;
            }
        }

        return closestPoint;
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
                TeamMember otherTeam = u.teamMember;
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

    private Building FindClosestEnemyBuildingInRange()
    {
        Building closest = null;
        float closestSqrDist = float.MaxValue;
        float rangeSqr = detectionRange * detectionRange;

        Building[] allBuildings = FindObjectsOfType<Building>();

        foreach (Building b in allBuildings)
        {
            if (b == null) continue;
            if (b.IsDestroyed) continue;

            TeamMember otherTeam = b.GetComponent<TeamMember>();

            if (teamMember != null)
            {
                if (otherTeam == null) continue;
                if (!teamMember.IsEnemy(otherTeam)) continue;
            }

            Vector3 targetPoint = GetBuildingAttackPoint(b);
            float sqrDist = (targetPoint - transform.position).sqrMagnitude;

            if (sqrDist > rangeSqr) continue;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = b;
            }
        }

        return closest;
    }

    private void Die()
    {
        if (isDead) return;

        if (currentTarget != null)
        {
            currentTarget.OnUnitKilled?.Invoke(currentTarget, this);
        }
        
        isDead = true;
        OnDied?.Invoke(this);

        hasManualAttackOrder = false;
        currentTarget = null;
        targetBuilding = null;

        ActiveUnits.Remove(this);

        Debug.Log(name + " has died.");

        // Disable movement
        if (agent != null)
            agent.isStopped = true;

        // Change color to indicate death
        unitRenderer.material.color = Color.red;

        // Disable further interaction
        enabled = false;

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject);
    }

    public float GetDamageMultiplierAgainst(Unit target)
    {
        if (target == null)
            return 1f;

        if (unitDefinition == null)
            return 1f;

        return unitDefinition.GetDamageMultiplier(target.PrimaryTag);
    }
}