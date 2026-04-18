using System.Collections.Generic;
using UnityEngine;

public class MatchResultManager : MonoBehaviour
{
    public static MatchResultManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f;

    private float nextCheckTime;
    private bool matchEnded = false;
    private bool localPlayerDefeated = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (matchEnded)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.CurrentState != GameState.InGame)
            return;

        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;
        EvaluateMatchState();
    }

    private void EvaluateMatchState()
    {
        HashSet<int> aliveTeams = new HashSet<int>();

        // Check units
        foreach (Unit u in Unit.ActiveUnits)
        {
            if (u == null || u.IsDead)
                continue;

            TeamMember tm = u.GetComponent<TeamMember>();
            if (tm != null && tm.Team != Team.Neutral)
            {
                aliveTeams.Add((int)tm.Team);
            }
        }

        // Check buildings
        Building[] allBuildings = FindObjectsOfType<Building>();

        foreach (Building b in allBuildings)
        {
            if (b == null || b.IsDestroyed)
                continue;

            TeamMember tm = b.GetComponent<TeamMember>();
            if (tm != null && tm.Team != Team.Neutral)
            {
                aliveTeams.Add((int)tm.Team);
            }
        }

        string aliveTeamList = "";

        foreach (int team in aliveTeams)
        {
            aliveTeamList += team + " ";
        }

        CheckLocalPlayerDefeat(aliveTeams);
        CheckForMatchEnd(aliveTeams);
    }

    private void CheckForMatchEnd(HashSet<int> aliveTeams)
    {
        if (aliveTeams.Count > 1)
            return;

        matchEnded = true;

        if (aliveTeams.Count == 1)
        {
            int winningTeam = -1;

            foreach (int team in aliveTeams)
            {
                winningTeam = team;
            }

            Debug.Log($"Victory! Team {winningTeam} is the last team remaining.");
        }
        else
        {
            Debug.Log("Match Ended. No teams remain alive.");
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("[Match] Match ended → game continues running (no pause).");
        }
    }

    private void CheckLocalPlayerDefeat(HashSet<int> aliveTeams)
    {
        if (localPlayerDefeated)
            return;

        if (SpectatorManager.Instance == null)
            return;

        int localTeam = SpectatorManager.Instance.LocalTeamId;

        if (!aliveTeams.Contains(localTeam))
        {
            localPlayerDefeated = true;

            Debug.Log($"[Spectator] Local player team {localTeam} has been defeated.");

            if (SpectatorManager.Instance != null)
            {
                SpectatorManager.Instance.SetSpectator(true);
                Debug.Log("[Spectator] Spectator mode ENABLED.");
            }
        }
    }
}