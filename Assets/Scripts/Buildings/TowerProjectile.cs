using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Unit target;
    [SerializeField] private int damage;
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Building buildingTarget;

    private float lifeTimer = 0f;

    public void Init(Unit newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    public void InitBuilding(Building newTarget, int newDamage)
    {
        buildingTarget = newTarget;
        damage = newDamage;
        target = null;
    }

    private void Update()
    {

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if ((target == null || target.IsDead) && (buildingTarget == null || buildingTarget.IsDestroyed))
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition;

        if (target != null)
        {
            targetPosition = target.transform.position + targetOffset;
        }
        else
        {
            targetPosition = buildingTarget.transform.position + targetOffset;
        }
        float step = moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        float sqrDist = (targetPosition - transform.position).sqrMagnitude;
        if (sqrDist <= 0.05f * 0.05f)
        {
            if (target != null)
            {
                target.TakeDamage(damage, null);
            }
            else if (buildingTarget != null)
            {
                buildingTarget.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}