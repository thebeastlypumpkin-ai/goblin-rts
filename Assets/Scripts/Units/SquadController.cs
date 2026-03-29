using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SquadController : MonoBehaviour
{
    [Header("Squad Core")]
    [SerializeField] private bool useSquadSystem = true;
    [SerializeField] private int visualMemberCount = 5;
    private Unit unit;
    [Header("Formation Settings")]
    [SerializeField] private float spacingX = 0.9f;
    [SerializeField] private float spacingZ = 0.9f;
    [Header("Health Sync")]
    [SerializeField] private bool useSharedHealthPool = true;

    private readonly List<Transform> squadVisuals = new List<Transform>();
    private readonly List<GameObject> visualPool = new List<GameObject>();
    private readonly List<GameObject> activeVisuals = new List<GameObject>();

    public bool UseSquadSystem => useSquadSystem;
    public int VisualMemberCount => visualMemberCount;
    public IReadOnlyList<Transform> SquadVisuals => squadVisuals;
    public IReadOnlyList<GameObject> VisualPool => visualPool;
    public IReadOnlyList<GameObject> ActiveVisuals => activeVisuals;
    public bool UseSharedHealthPool => useSharedHealthPool;
    public Unit RootUnit => unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();

        if (visualMemberCount < 1)
            visualMemberCount = 1;

        if (unit != null && unit.UnitDefinition != null)
        {
            visualMemberCount = Mathf.Max(1, unit.UnitDefinition.visualSquadSize);
        }
    }

    private void Start()
    {
        if (!useSquadSystem) return;

        CreateVisualPool();
        ActivateVisualMembers();
    }

    private void OnEnable()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        if (unit != null)
        {
            unit.OnHealthChanged += HandleRootHealthChanged;
            unit.OnDied += HandleRootDied;
        }
    }

    private void OnDisable()
    {
        if (unit != null)
        {
            unit.OnHealthChanged -= HandleRootHealthChanged;
            unit.OnDied -= HandleRootDied;
        }
    }

    private void CreateVisualPool()
    {
        visualPool.Clear();
        activeVisuals.Clear();
        squadVisuals.Clear();

        for (int i = 0; i < visualMemberCount; i++)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            Collider col = visual.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
            }

            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null && rend != null)
            {
                rend.material.color = rootRenderer.material.color;
            }

            visual.name = "SquadVisual_" + i;

            // Parent to unit
            visual.transform.SetParent(transform);

            Vector3 formationOffset = GetFormationOffset(i);
            visual.transform.localPosition = formationOffset;

            visual.SetActive(false);
            visualPool.Add(visual);
        }
    }

    private void ActivateVisualMembers()
    {
        activeVisuals.Clear();
        squadVisuals.Clear();

        int count = Mathf.Min(visualMemberCount, visualPool.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject visual = visualPool[i];
            if (visual == null) continue;

            visual.SetActive(true);
            activeVisuals.Add(visual);
            RegisterVisual(visual.transform);
        }
    }

    private void HandleRootHealthChanged(float normalized)
    {
        if (!useSharedHealthPool)
            return;

        int targetVisibleCount = Mathf.CeilToInt(normalized * visualMemberCount);
        targetVisibleCount = Mathf.Clamp(targetVisibleCount, 1, visualMemberCount);

        UpdateActiveVisualCount(targetVisibleCount);
    }

    private void HandleRootDied(Unit deadUnit)
    {
        foreach (GameObject visual in activeVisuals)
        {
            if (visual != null)
                visual.SetActive(false);
        }

        activeVisuals.Clear();
        squadVisuals.Clear();
    }

    private void UpdateActiveVisualCount(int targetCount)
    {
        activeVisuals.Clear();
        squadVisuals.Clear();

        for (int i = 0; i < visualPool.Count; i++)
        {
            GameObject visual = visualPool[i];
            if (visual == null) continue;

            bool shouldBeActive = i < targetCount;
            visual.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                activeVisuals.Add(visual);
                RegisterVisual(visual.transform);
            }
        }
    }

    private Vector3 GetFormationOffset(int index)
    {
        if (visualMemberCount <= 1)
            return new Vector3(0f, 1f, 0f);

        float angleStep = 360f / visualMemberCount;
        float angle = angleStep * index;

        float ringRadius = 1.5f;

        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * ringRadius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * ringRadius;

        // small goblin-style messiness
        float jitterX = Random.Range(-0.35f, 0.35f);
        float jitterZ = Random.Range(-0.35f, 0.35f);

        return new Vector3(x + jitterX, 1f, z + jitterZ);
    }

    public void RegisterVisual(Transform visual)
    {
        if (visual == null) return;
        if (!squadVisuals.Contains(visual))
        {
            squadVisuals.Add(visual);
        }
    }

    public void UnregisterVisual(Transform visual)
    {
        if (visual == null) return;
        squadVisuals.Remove(visual);
    }

    public void ClearVisuals()
    {
        squadVisuals.Clear();
    }
}