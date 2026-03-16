using System.Collections.Generic;
using UnityEngine;
using GoblinRTS.Economy;

public class ProductionQueue : MonoBehaviour
{
    private Queue<UnitDefinition> unitQueue = new Queue<UnitDefinition>();

    private UnitDefinition currentUnit;

    private float productionTimer = 0f;

    [SerializeField] private UnitDefinition debugUnit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (debugUnit != null)
            {
                EnqueueUnit(debugUnit);
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

    public void EnqueueUnit(UnitDefinition unit)
    {
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
}