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
    [Header("Attack Sync")]
    [SerializeField] private bool useAttackSync = true;
    [SerializeField] private float attackPushDistance = 0.35f;

    private readonly List<Transform> squadVisuals = new List<Transform>();
    private readonly List<GameObject> visualPool = new List<GameObject>();
    private readonly List<GameObject> activeVisuals = new List<GameObject>();
    private readonly List<GameObject> aliveVisuals = new List<GameObject>();
    private readonly List<Vector3> cachedFormationOffsets = new List<Vector3>();
    private Vector3 currentAttackLocalDirection = Vector3.zero;

    public bool UseSquadSystem => useSquadSystem;
    public int VisualMemberCount => visualMemberCount;
    public IReadOnlyList<Transform> SquadVisuals => squadVisuals;
    public IReadOnlyList<GameObject> VisualPool => visualPool;
    public IReadOnlyList<GameObject> ActiveVisuals => activeVisuals;
    public int ActiveVisualCount => aliveVisuals.Count;
    public bool UseSharedHealthPool => useSharedHealthPool;
    public Unit RootUnit => unit;

    public bool IsCombatEngaged
    {
        get
        {
            if (unit == null) return false;
            if (unit.CurrentTarget == null) return false;
            return !unit.CurrentTarget.IsDead;
        }
    }

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

        if (unit == null)
            unit = GetComponent<Unit>();

        if (unit != null)
        {
            unit.OnHealthChanged += HandleRootHealthChanged;
            unit.OnDied += HandleRootDied;

            HandleRootHealthChanged(unit.HealthNormalized);
        }
    }

    private void Update()
    {
        if (!useSquadSystem) return;
        if (!useAttackSync) return;
        if (unit == null) return;

        if (unit.CurrentTarget == null)
        {
            currentAttackLocalDirection = Vector3.zero;
            ResetVisualPositions();
            return;
        }

        SyncAttackFacing();

        if (unit.IsInAttackRange)
        {
            Vector3 localTargetPosition = transform.InverseTransformPoint(unit.CurrentTarget.transform.position);
            currentAttackLocalDirection = new Vector3(localTargetPosition.x, 0f, localTargetPosition.z).normalized;

            ApplyAttackStance();
        }
        else
        {
            currentAttackLocalDirection = Vector3.zero;
            ResetVisualPositions();
        }
    }

    private void OnEnable()
    {
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
        aliveVisuals.Clear();
        cachedFormationOffsets.Clear();

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
            cachedFormationOffsets.Add(formationOffset);
            visual.transform.localPosition = formationOffset;

            visual.SetActive(false);
            visualPool.Add(visual);
        }
    }

    private void ActivateVisualMembers()
    {
        activeVisuals.Clear();
        squadVisuals.Clear();
        aliveVisuals.Clear();

        int count = Mathf.Min(visualMemberCount, visualPool.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject visual = visualPool[i];
            if (visual == null) continue;

            visual.SetActive(true);
            activeVisuals.Add(visual);
            aliveVisuals.Add(visual);
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
        aliveVisuals.Clear();
        squadVisuals.Clear();
    }

    private void UpdateActiveVisualCount(int targetCount)
    {
        targetCount = Mathf.Clamp(targetCount, 1, visualMemberCount);

        while (aliveVisuals.Count > targetCount)
        {
            RemoveOneVisualMember();
        }

        while (aliveVisuals.Count < targetCount)
        {
            RestoreOneVisualMember();
        }

        RefreshActiveVisualLists();
    }

    private void RemoveOneVisualMember()
    {
        if (aliveVisuals.Count <= 1)
            return;

        int randomIndex = Random.Range(0, aliveVisuals.Count);
        GameObject visualToRemove = aliveVisuals[randomIndex];
        if (visualToRemove == null)
            return;

        visualToRemove.SetActive(false);
        aliveVisuals.RemoveAt(randomIndex);
    }

    private void RestoreOneVisualMember()
    {
        for (int i = 0; i < visualPool.Count; i++)
        {
            GameObject visual = visualPool[i];
            if (visual == null) continue;
            if (aliveVisuals.Contains(visual)) continue;

            visual.SetActive(true);
            aliveVisuals.Add(visual);
            return;
        }
    }

    private void RefreshActiveVisualLists()
    {
        activeVisuals.Clear();
        squadVisuals.Clear();

        for (int i = 0; i < aliveVisuals.Count; i++)
        {
            GameObject visual = aliveVisuals[i];
            if (visual == null) continue;

            activeVisuals.Add(visual);
            RegisterVisual(visual.transform);
        }
    }

    private int GetVisualPoolIndex(GameObject visual)
    {
        if (visual == null)
            return -1;

        return visualPool.IndexOf(visual);
    }

    private void SyncAttackFacing()
    {
        if (unit.CurrentTarget == null)
            return;

        Vector3 targetPosition = unit.CurrentTarget.transform.position;

        for (int i = 0; i < activeVisuals.Count; i++)
        {
            GameObject visual = activeVisuals[i];
            if (visual == null) continue;

            Vector3 direction = targetPosition - visual.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                visual.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void ApplyAttackStance()
    {
        for (int i = 0; i < activeVisuals.Count; i++)
        {
            GameObject visual = activeVisuals[i];
            if (visual == null) continue;

            int poolIndex = GetVisualPoolIndex(visual);
            if (poolIndex < 0 || poolIndex >= cachedFormationOffsets.Count) continue;

            Vector3 baseOffset = cachedFormationOffsets[poolIndex];
            Vector3 targetOffset = baseOffset + (currentAttackLocalDirection * 0.75f);

            visual.transform.localPosition = Vector3.Lerp(
                visual.transform.localPosition,
                targetOffset,
                Time.deltaTime * 10f
            );
        }
    }

    private void ResetVisualPositions()
    {
        for (int i = 0; i < activeVisuals.Count; i++)
        {
            GameObject visual = activeVisuals[i];
            if (visual == null) continue;

            int poolIndex = GetVisualPoolIndex(visual);
            if (poolIndex < 0 || poolIndex >= cachedFormationOffsets.Count) continue;

            Vector3 baseOffset = cachedFormationOffsets[poolIndex];

            visual.transform.localPosition = Vector3.Lerp(
                visual.transform.localPosition,
                baseOffset,
                Time.deltaTime * 10f
            );
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

    private void OnDrawGizmosSelected()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        if (unit == null) return;

        // Stress-state color
        if (ActiveVisualCount <= 1)
            Gizmos.color = Color.magenta;
        else if (IsCombatEngaged)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.25f, 1.2f);

        // Draw line to target
        if (unit.CurrentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, unit.CurrentTarget.transform.position);
        }

        // Draw visual members
        Gizmos.color = Color.cyan;

        for (int i = 0; i < cachedFormationOffsets.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(cachedFormationOffsets[i]);
            Gizmos.DrawWireSphere(worldPos, 0.15f);
        }
    }
}