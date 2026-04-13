using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Unit target;
    [SerializeField] private int damage;
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    private float lifeTimer = 0f;

    public void Init(Unit newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    private void Update()
    {

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + targetOffset;
        float step = moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        float sqrDist = (targetPosition - transform.position).sqrMagnitude;
        if (sqrDist <= 0.05f * 0.05f)
        {
            target.TakeDamage(damage, null);
            Destroy(gameObject);
        }
    }
}