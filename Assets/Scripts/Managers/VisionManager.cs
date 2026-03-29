using System.Collections.Generic;
using UnityEngine;

public class VisionManager : MonoBehaviour
{
    private Dictionary<int, List<VisionEmitter>> teamEmitters = new Dictionary<int, List<VisionEmitter>>();

    public static VisionManager Instance { get; private set; }

    private readonly List<VisionEmitter> activeEmitters = new List<VisionEmitter>();

    public IReadOnlyList<VisionEmitter> ActiveEmitters => activeEmitters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterEmitter(VisionEmitter emitter)
    {
        if (emitter == null) return;

        if (!activeEmitters.Contains(emitter))
        {
            activeEmitters.Add(emitter);
        }

        int teamId = emitter.TeamId;

        if (!teamEmitters.ContainsKey(teamId))
        {
            teamEmitters[teamId] = new List<VisionEmitter>();
        }

        if (!teamEmitters[teamId].Contains(emitter))
        {
            teamEmitters[teamId].Add(emitter);
        }
    }

    public void UnregisterEmitter(VisionEmitter emitter)
    {
        if (emitter == null) return;

        activeEmitters.Remove(emitter);

        int teamId = emitter.TeamId;

        if (teamEmitters.ContainsKey(teamId))
        {
            teamEmitters[teamId].Remove(emitter);

            if (teamEmitters[teamId].Count == 0)
            {
                teamEmitters.Remove(teamId);
            }
        }
    }

    public List<VisionEmitter> GetEmittersForTeam(int teamId)
    {
        if (teamEmitters.ContainsKey(teamId))
        {
            return teamEmitters[teamId];
        }

        return new List<VisionEmitter>();
    }

}