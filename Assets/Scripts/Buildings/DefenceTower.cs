using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : MonoBehaviour
{
    [Header("Tower Settings")]
    [Header("Tower Settings")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 10;


    [Header("Runtime")]
    [SerializeField] private Unit currentTarget;

    private readonly List<Unit> unitsInRange = new List<Unit>();

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
        if (unit == null)
            return;

        if (unit.IsDead)
            return;

        TeamMember otherTeam = unit.GetComponent<TeamMember>();
        if (otherTeam == null)
            return;

        if (!IsEnemy(otherTeam))
            return;

        if (!unitsInRange.Contains(unit))
        {
            unitsInRange.Add(unit);
            Debug.Log($"{name} detected enemy in range: {unit.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Unit unit = other.GetComponentInParent<Unit>();
        if (unit == null)
            return;

        if (unitsInRange.Remove(unit))
        {
            Debug.Log($"{name} enemy left range: {unit.name}");
        }

        if (currentTarget == unit)
        {
            currentTarget = null;
        }
    }

    private void Update()
    {
        CleanupInvalidTargets();
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

        if (currentTarget == null)
            return;

        if (!unitsInRange.Contains(currentTarget))
        {
            currentTarget = null;
        }
    }

    private void AcquireTarget()
    {
        if (currentTarget != null)
            return;

        if (unitsInRange.Count == 0)
            return;

        Unit closest = null;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < unitsInRange.Count; i++)
        {
            Unit unit = unitsInRange[i];
            if (unit == null || unit.IsDead)
                continue;

            float sqrDist = (unit.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = unit;
            }
        }

        currentTarget = closest;

        if (currentTarget != null)
        {
            Debug.Log($"{name} targeting {currentTarget.name}");
        }
    }

    private void HandleAttack()
    {
        if (currentTarget == null)
            return;

        if (currentTarget.IsDead)
        {
            currentTarget = null;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackCooldown)
            return;

        attackTimer = 0f;

        currentTarget.TakeDamage(attackDamage, null);
        Debug.Log($"{name} hit {currentTarget.name} for {attackDamage} damage.");
    }


}
