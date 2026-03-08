using UnityEngine;

[DisallowMultipleComponent]
public class Building : MonoBehaviour
{
    [Header("Definition")]
    [SerializeField] private BuildingDefinition definition;
    [SerializeField] private int teamId;

    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public void Init(BuildingDefinition def, int team)
    {
        definition = def;
        teamId = team;

        maxHealth = def.maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;

        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} destroyed!");

        // Find the nearest BuildSite and reset it
        BuildSite site = FindFirstObjectByType<BuildSite>();
        if (site != null)
        {
            site.buildProgress = 0f;
            site.isComplete = false;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(100f);
        }
    }
}