using UnityEngine;
using GoblinRTS.Economy;

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

    [Header("Fortress Runtime")]
    [SerializeField] private bool isFortress;
    [SerializeField] private int fortressBaselineIncomePerTick;
    [Header("Tiering")]
    [SerializeField] private int currentTier = 1;

    private float fortressIncomeTimer;

    private float lastDamageTime = -999f;
    private float spawnTime;

    public BuildingDefinition Definition => definition;
    public int TeamId => teamId;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public bool IsFortress => isFortress;
    public int CurrentTier => currentTier;
    public int FortressBaselineIncomePerTick => fortressBaselineIncomePerTick;

    public bool SupportsTierUpgrades => definition != null && definition.supportsTierUpgrades;
    public bool IsAtMaxTier => definition != null && currentTier >= definition.maxTier;

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

        isFortress = def.isFortress;
        fortressBaselineIncomePerTick = def.fortressBaselineIncomePerTick;

        currentTier = (definition != null) ? definition.startingTier : 1;

        lastDamageTime = Time.time;
        spawnTime = Time.time;

        fortressIncomeTimer = 0f;

        if (isFortress)
        {
            UpdateFortressSupplyCap();
        }

        if (definition != null && definition.buildingName == "Research Building")
        {
            Debug.Log("Research Building active: ready to unlock upgrades.");
        }
    }

    public void UpgradeBuilding()
    {
        if (!SupportsTierUpgrades)
        {
            Debug.Log($"{definition.buildingName} does not support tier upgrades.");
            return;
        }

        if (IsAtMaxTier)
        {
            Debug.Log($"{definition.buildingName} already at max tier.");
            return;
        }

        currentTier++;

        Debug.Log($"{definition.buildingName} upgraded to Tier {currentTier}");

        if (isFortress)
        {
            UpdateFortressSupplyCap();
        }
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

        if (isFortress && SupplyManager.Instance != null)
        {
            SupplyManager.Instance.SetMaxSupply(0);
        }

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
        HandleFortressIncome();

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

    private void HandleFortressIncome()
    {
        if (!isFortress) return;
        if (fortressBaselineIncomePerTick <= 0) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.InGame) return;
        if (IncomeTicker.Instance == null) return;

        fortressIncomeTimer += Time.deltaTime;

        float tickInterval = IncomeTicker.Instance.TickIntervalSeconds;
        if (tickInterval <= 0f) return;

        if (fortressIncomeTimer >= tickInterval)
        {
            fortressIncomeTimer = 0f;
            GameManager.Instance.Essence.Add(fortressBaselineIncomePerTick);
        }
    }

    private void UpdateFortressSupplyCap()
    {
        if (!isFortress)
        {
            Debug.Log("[Supply] UpdateFortressSupplyCap aborted: not a fortress.");
            return;
        }

        if (SupplyManager.Instance == null)
        {
            Debug.LogWarning("[Supply] UpdateFortressSupplyCap aborted: SupplyManager.Instance is NULL.");
            return;
        }

        int supplyCap = GetSupplyCapForTier(currentTier);
        SupplyManager.Instance.SetMaxSupply(supplyCap);

        Debug.Log($"[Supply] Fortress tier {currentTier} set max supply to {supplyCap}.");
    }

    private int GetSupplyCapForTier(int tier)
    {
        switch (tier)
        {
            case 1:
                return 60;
            case 2:
                return 120;
            case 3:
                return 180;
            default:
                return 60;
        }
    }
}