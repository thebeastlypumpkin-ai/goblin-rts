using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private TowerProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Runtime")]
    [SerializeField] private Unit currentUnitTarget;
    [SerializeField] private Building currentBuildingTarget;

    private readonly List<Unit> unitsInRange = new List<Unit>();
    private readonly List<Building> buildingsInRange = new List<Building>();

    private TeamMember teamMember;
    private SphereCollider rangeTrigger;
    private float attackTimer = 0f;

    private void Awake()
    {
        teamMember = GetComponent<TeamMember>();
        rangeTrigger = GetComponent<SphereCollider>();

        if (teamMember == null)
        {
            Debug.LogError($"DefenseTower on {name} is missing TeamMember.");
        }

        if (rangeTrigger == null)
        {
            Debug.LogError($"DefenseTower on {name} requires a SphereCollider.");
            return;
        }

        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = attackRange;
    }

    private bool IsEnemy(TeamMember other)
    {
        if (teamMember == null || other == null)
            return false;

        return other.Team != teamMember.Team;
    }

    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponentInParent<Unit>();
        if (unit != null && !unit.IsDead)
        {
            TeamMember otherTeam = unit.GetComponent<TeamMember>();
            if (otherTeam != null && IsEnemy(otherTeam))
            {
                if (!unitsInRange.Contains(unit))
                {
                    unitsInRange.Add(unit);
                    Debug.Log($"{name} detected enemy unit in range: {unit.name}");
                }
            }
        }

        Building building = other.GetComponentInParent<Building>();
        if (building != null && !building.IsDestroyed)
        {
            TeamMember otherTeam = building.GetComponent<TeamMember>();
            if (otherTeam != null && IsEnemy(otherTeam))
            {
                if (!buildingsInRange.Contains(building))
                {
                    buildingsInRange.Add(building);
                    Debug.Log($"{name} detected enemy building in range: {building.name}");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Unit unit = other.GetComponentInParent<Unit>();
        if (unit != null)
        {
            unitsInRange.Remove(unit);

            if (currentUnitTarget == unit)
            {
                currentUnitTarget = null;
            }
        }

        Building building = other.GetComponentInParent<Building>();
        if (building != null)
        {
            buildingsInRange.Remove(building);

            if (currentBuildingTarget == building)
            {
                currentBuildingTarget = null;
            }
        }
    }

    private void Update()
    {
        AcquireTarget();
        HandleAttack();
    }

    private void CleanupInvalidTargets()
    {
        for (int i = unitsInRange.Count - 1; i >= 0; i--)
        {
            if (unitsInRange[i] == null || unitsInRange[i].IsDead)
            {
                unitsInRange.RemoveAt(i);
            }
        }

        for (int i = buildingsInRange.Count - 1; i >= 0; i--)
        {
            if (buildingsInRange[i] == null || buildingsInRange[i].IsDestroyed)
            {
                buildingsInRange.RemoveAt(i);
            }
        }

        if (currentUnitTarget != null && !unitsInRange.Contains(currentUnitTarget))
        {
            currentUnitTarget = null;
        }

        if (currentBuildingTarget != null && !buildingsInRange.Contains(currentBuildingTarget))
        {
            currentBuildingTarget = null;
        }
    }

    private void AcquireTarget()
    {
        currentUnitTarget = null;
        currentBuildingTarget = null;

        float closestSqrDist = float.MaxValue;

        Unit closestUnit = FindClosestEnemyUnitInRange();
        if (closestUnit != null)
        {
            float unitSqrDist = (closestUnit.transform.position - transform.position).sqrMagnitude;
            if (unitSqrDist < closestSqrDist)
            {
                closestSqrDist = unitSqrDist;
                currentUnitTarget = closestUnit;
                currentBuildingTarget = null;
            }
        }

        Building closestBuilding = FindClosestEnemyBuildingInRange();
        if (closestBuilding != null)
        {
            float buildingSqrDist = (closestBuilding.transform.position - transform.position).sqrMagnitude;
            if (buildingSqrDist < closestSqrDist)
            {
                closestSqrDist = buildingSqrDist;
                currentUnitTarget = null;
                currentBuildingTarget = closestBuilding;
            }
        }
    }

    private Unit FindClosestEnemyUnitInRange()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        Unit closest = null;
        float closestSqrDist = float.MaxValue;
        float rangeSqr = attackRange * attackRange;

        for (int i = 0; i < allUnits.Length; i++)
        {
            Unit unit = allUnits[i];
            if (unit == null || unit.IsDead)
                continue;

            TeamMember otherTeam = unit.GetComponent<TeamMember>();
            if (otherTeam == null || !IsEnemy(otherTeam))
                continue;

            float sqrDist = (unit.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > rangeSqr)
                continue;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = unit;
            }
        }

        return closest;
    }

    private Building FindClosestEnemyBuildingInRange()
    {
        Building[] allBuildings = FindObjectsOfType<Building>();
        Building closest = null;
        float closestSqrDist = float.MaxValue;
        float rangeSqr = attackRange * attackRange;

        for (int i = 0; i < allBuildings.Length; i++)
        {
            Building building = allBuildings[i];
            if (building == null || building.IsDestroyed)
                continue;

            TeamMember otherTeam = building.GetComponent<TeamMember>();
            if (otherTeam == null || !IsEnemy(otherTeam))
                continue;

            float sqrDist = (building.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > rangeSqr)
                continue;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = building;
            }
        }

        return closest;
    }

    private void HandleAttack()
    {
        if (currentUnitTarget == null && currentBuildingTarget == null)
            return;

        if (currentUnitTarget != null && currentUnitTarget.IsDead)
        {
            currentUnitTarget = null;
            return;
        }

        if (currentBuildingTarget != null && currentBuildingTarget.IsDestroyed)
        {
            currentBuildingTarget = null;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackCooldown)
            return;

        attackTimer = 0f;

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} is missing projectilePrefab.");
            return;
        }

        Transform spawnPoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;

        TowerProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        if (currentUnitTarget != null)
        {
            projectile.Init(currentUnitTarget, attackDamage);
            Debug.Log($"{name} fired projectile at unit {currentUnitTarget.name}");
        }
        else if (currentBuildingTarget != null)
        {
            projectile.InitBuilding(currentBuildingTarget, attackDamage);
            Debug.Log($"{name} fired projectile at building {currentBuildingTarget.name}");
        }
    }
}