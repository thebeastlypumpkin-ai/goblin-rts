using System.Collections.Generic;
using UnityEngine;

public class FissureNode : MonoBehaviour
{
    private List<Unit> unitsInside = new List<Unit>();
    private float incomeTimer = 0f;

    [Header("Capture Settings")]
    public float captureDuration = 10f;

    [Header("Income")]
    public int essencePerTick = 5;

    [Header("Runtime")]
    public int ownerTeam = -1; // -1 = neutral
    public float captureProgress = 0f;
    public bool contested = false;

    public bool IsNeutral()
    {
        return ownerTeam < 0;
    }

    public void SetOwner(int team)
    {
        ownerTeam = team;
        captureProgress = 0f;
    }

    public void ResetCapture()
    {
        captureProgress = 0f;
        contested = false;
    }

    private void Update()
    {
        if (ownerTeam >= 0)
        {
            incomeTimer += Time.deltaTime;

            float tickInterval = 1f;

            if (GoblinRTS.Economy.IncomeTicker.Instance != null)
            {
                tickInterval = GoblinRTS.Economy.IncomeTicker.Instance.TickIntervalSeconds;
            }

            if (incomeTimer >= tickInterval)
            {
                incomeTimer = 0f;

                if (GameManager.Instance != null && GameManager.Instance.Essence != null)
                {
                    GameManager.Instance.Essence.Add(essencePerTick);
                    Debug.Log($"{gameObject.name} generated {essencePerTick} Essence for Team {ownerTeam}");
                }
            }
        }

        int capturingTeam = GetSingleTeamInZone();

        if (capturingTeam == -2)
        {
            contested = true;
            captureProgress = 0f;
            return;
        }

        contested = false;

        if (capturingTeam == -1)
        {
            captureProgress = 0f;
            return;
        }

        if (ownerTeam == capturingTeam)
        {
            captureProgress = 0f;
            return;
        }

        captureProgress += Time.deltaTime;

        if (captureProgress >= captureDuration)
        {
            SetOwner(capturingTeam);
            contested = false;
            Debug.Log($"{gameObject.name} captured by Team {capturingTeam}");
        }
    }

    private int GetSingleTeamInZone()
    {
        int foundTeam = -1;

        for (int i = unitsInside.Count - 1; i >= 0; i--)
        {
            Unit unit = unitsInside[i];

            if (unit == null)
            {
                unitsInside.RemoveAt(i);
                continue;
            }

            TeamMember teamMember = unit.GetComponent<TeamMember>();

            if (teamMember == null)
            {
                continue;
            }

            int unitTeam = (int)teamMember.Team;

            if (foundTeam == -1)
            {
                foundTeam = unitTeam;
            }
            else if (foundTeam != unitTeam)
            {
                return -2; // multiple teams present
            }
        }

        return foundTeam; // -1 if empty, otherwise team id
    }

    private void OnTriggerEnter(Collider other)
    {

        Unit unit = other.GetComponent<Unit>();

        if (unit == null)
        {
            return;
        }

        if (!unitsInside.Contains(unit))
        {
            unitsInside.Add(unit);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        Unit unit = other.GetComponent<Unit>();

        if (unit == null)
        {
            return;
        }

        if (unitsInside.Contains(unit))
        {
            unitsInside.Remove(unit);
        }
    }
}