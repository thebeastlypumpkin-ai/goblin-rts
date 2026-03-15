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
        }
    }
}