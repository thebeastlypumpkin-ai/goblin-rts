using UnityEngine;

public class SquadAoEHandler : MonoBehaviour
{
    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void ApplyAoEDamage(float damage, Unit attacker)
    {
        if (unit == null) return;

        unit.TakeDamage(damage, attacker);
    }

    public static void TryApplyAoE(GameObject target, float damage, Unit attacker)
    {
        if (target == null) return;

        SquadAoEHandler squadAoE = target.GetComponentInParent<SquadAoEHandler>();
        if (squadAoE != null)
        {
            squadAoE.ApplyAoEDamage(damage, attacker);
            return;
        }

        Unit unit = target.GetComponentInParent<Unit>();
        if (unit != null)
        {
            unit.TakeDamage(damage, attacker);
        }
    }
}