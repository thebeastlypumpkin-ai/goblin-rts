using System.Collections.Generic;
using UnityEngine;
using GoblinRTS.Economy;

public class ProductionQueue : MonoBehaviour
{
    private Queue<UnitDefinition> unitQueue = new Queue<UnitDefinition>();

    private UnitDefinition currentUnit;

    private float productionTimer = 0f;

    private Building building;

    private int rallySpawnIndex = 0;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int teamId = 0;
    [SerializeField] private List<UnitDefinition> availableUnits = new List<UnitDefinition>();

    void Awake()
    {
        building = GetComponent<Building>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (availableUnits.Count > 0)
            {
                EnqueueUnit(availableUnits[0]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            CancelCurrentProduction();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            CancelLastQueuedUnit();
        }

        ProcessQueue();
    }

    public int AvailableUnitsCount()
    {
        return availableUnits.Count;
    }

    public UnitDefinition GetAvailableUnit(int index)
    {
        if (index < 0 || index >= availableUnits.Count)
            return null;

        return availableUnits[index];
    }

    public void EnqueueUnit(UnitDefinition unit)
    {
        if (building == null)
        {
            Debug.LogWarning("ProductionQueue missing Building reference.");
            return;
        }

        if (unit == null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Cannot queue unit: GameManager.Instance is null.");
            return;
        }

        if (SupplyManager.Instance == null)
        {
            Debug.LogWarning("Cannot queue unit: SupplyManager.Instance is null.");
            return;
        }

        if (!GameManager.Instance.Essence.TrySpend(unit.essenceCost))
        {
            Debug.Log($"Not enough Essence to queue: {unit.unitName}");
            return;
        }

        if (!SupplyManager.Instance.TryConsumeSupply(unit.supplyCost))
        {
            Debug.Log($"Not enough Supply to queue: {unit.unitName}");
            GameManager.Instance.Essence.Add(unit.essenceCost);
            return;
        }

        unitQueue.Enqueue(unit);

        Debug.Log($"Unit added to queue: {unit.unitName}");
    }

    private void ProcessQueue()
    {
        if (currentUnit == null)
        {
            if (unitQueue.Count == 0)
                return;

            currentUnit = unitQueue.Dequeue();
            productionTimer = currentUnit.buildTime;

            Debug.Log($"Started training: {currentUnit.unitName}");
        }

        productionTimer -= Time.deltaTime;

        if (productionTimer <= 0f)
        {
            Debug.Log($"Finished training: {currentUnit.unitName}");

            SpawnUnit(currentUnit);

            currentUnit = null;
            productionTimer = 0f;
        }
    }

    private void RefundUnit(UnitDefinition unit)
    {
        if (unit == null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Essence.Add(unit.essenceCost);
        }

        if (SupplyManager.Instance != null)
        {
            SupplyManager.Instance.ReleaseSupply(unit.supplyCost);
        }

        Debug.Log($"Refunded unit: {unit.unitName} | Essence +{unit.essenceCost} | Supply -{unit.supplyCost}");
    }

    private void ClearCurrentProduction()
    {
        currentUnit = null;
        productionTimer = 0f;
    }

    public void CancelCurrentProduction()
    {
        if (currentUnit == null)
        {
            Debug.Log("No active production to cancel.");
            return;
        }

        Debug.Log($"Cancelling current production: {currentUnit.unitName}");

        RefundUnit(currentUnit);

        ClearCurrentProduction();
    }

    public void CancelLastQueuedUnit()
    {
        if (unitQueue.Count == 0)
        {
            Debug.Log("No queued units to cancel.");
            return;
        }

        UnitDefinition[] queuedUnits = unitQueue.ToArray();
        UnitDefinition cancelledUnit = queuedUnits[queuedUnits.Length - 1];

        Queue<UnitDefinition> rebuiltQueue = new Queue<UnitDefinition>();

        for (int i = 0; i < queuedUnits.Length - 1; i++)
        {
            rebuiltQueue.Enqueue(queuedUnits[i]);
        }

        unitQueue = rebuiltQueue;

        Debug.Log($"Cancelling queued unit: {cancelledUnit.unitName}");

        RefundUnit(cancelledUnit);
    }

    private void SpawnUnit(UnitDefinition unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Spawn failed: UnitDefinition is null.");
            return;
        }

        if (unit.unitPrefab == null)
        {
            Debug.LogWarning($"Spawn failed: {unit.unitName} has no unitPrefab assigned.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn failed: No spawnPoint assigned on {gameObject.name}.");
            return;
        }

        GameObject spawnedObject = Instantiate(unit.unitPrefab, spawnPoint.position, spawnPoint.rotation);

        TeamMember tm = spawnedObject.GetComponent<TeamMember>();
        if (tm != null)
        {
            // team assignment will be handled by prefab default for now
        }

        Building building = GetComponent<Building>();
        Unit spawnedUnit = spawnedObject.GetComponent<Unit>();

        if (building != null && spawnedUnit != null && building.HasRallyPoint)
        {
            float baseAngleOffset = (Mathf.Abs(gameObject.GetInstanceID()) % 8) * 22.5f;
            float angle = baseAngleOffset + (rallySpawnIndex * 45f);
            float radius = 2.5f + (rallySpawnIndex / 8) * 2f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            Vector3 rallyTarget = building.RallyPoint + offset;
            spawnedUnit.CommandMoveTo(rallyTarget);

            rallySpawnIndex = (rallySpawnIndex + 1) % 8;

        }

        Debug.Log($"Spawned unit: {unit.unitName} for team {teamId}");
    }
}