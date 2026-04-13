using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Unit target;
    [SerializeField] private int damage;
    [SerializeField] private float moveSpeed = 12f;

    public void Init(Unit newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    private void Update()
    {
        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float sqrDist = (targetPosition - transform.position).sqrMagnitude;
        if (sqrDist <= 0.1f * 0.1f)
        {
            target.TakeDamage(damage, null);
            Destroy(gameObject);
        }
    }
}