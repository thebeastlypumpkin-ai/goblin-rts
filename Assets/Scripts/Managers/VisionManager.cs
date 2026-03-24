using System.Collections.Generic;
using UnityEngine;

public class VisionManager : MonoBehaviour
{
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
        if (activeEmitters.Contains(emitter)) return;

        activeEmitters.Add(emitter);
    }

    public void UnregisterEmitter(VisionEmitter emitter)
    {
        if (emitter == null) return;

        activeEmitters.Remove(emitter);
    }
}