using UnityEngine;

[DisallowMultipleComponent]
public class Building : MonoBehaviour
{
    [Header("Definition")]
    [SerializeField] private BuildingDefinition definition;
    [SerializeField] private int teamId;

    private BuildSite parentSite;

    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    [Header("Passive Repair")]
    [SerializeField] private bool enablePassiveRepair;
    [SerializeField] private float passiveRepairDelay;
    [SerializeField] private float passiveRepairPerSecond;

    private float lastDamageTime = -999f;
    private float spawnTime;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public void Init(BuildingDefinition def, int team, BuildSite site)
    {
        definition = def;
        teamId = team;
        parentSite = site;

        maxHealth = def.maxHealth;
        currentHealth = maxHealth;

        enablePassiveRepair = def.enablePassiveRepair;
        passiveRepairDelay = def.passiveRepairDelay;
        passiveRepairPerSecond = def.passiveRepairPerSecond;

        lastDamageTime = Time.time;
        spawnTime = Time.time;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        lastDamageTime = Time.time;

        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} destroyed!");

        if (parentSite != null)
        {
            parentSite.buildProgress = 0f;
            parentSite.isComplete = false;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        HandlePassiveRepair();

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(100f);
        }
    }

    private void HandlePassiveRepair()
    {
        if (!enablePassiveRepair) return;
        if (currentHealth <= 0f) return;
        if (currentHealth >= maxHealth) return;

        float timeSinceSpawn = Time.time - spawnTime;
        if (timeSinceSpawn < passiveRepairDelay) return;

        float timeSinceLastDamage = Time.time - lastDamageTime;
        if (timeSinceLastDamage < passiveRepairDelay) return;

        currentHealth += passiveRepairPerSecond * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}