using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SquadController : MonoBehaviour
{
    [Header("Squad Core")]
    [SerializeField] private bool useSquadSystem = true;
    [SerializeField] private int visualMemberCount = 5;

    private readonly List<Transform> squadVisuals = new List<Transform>();
    private readonly List<GameObject> visualPool = new List<GameObject>();
    private readonly List<GameObject> activeVisuals = new List<GameObject>();

    public bool UseSquadSystem => useSquadSystem;
    public int VisualMemberCount => visualMemberCount;
    public IReadOnlyList<Transform> SquadVisuals => squadVisuals;
    public IReadOnlyList<GameObject> VisualPool => visualPool;
    public IReadOnlyList<GameObject> ActiveVisuals => activeVisuals;

    private void Awake()
    {
        if (visualMemberCount < 1)
            visualMemberCount = 1;
    }

    private void Start()
    {
        if (!useSquadSystem) return;

        CreateVisualPool();
        ActivateVisualMembers();
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

            // Random small offset (temporary, will fix later)
            float offsetX = Random.Range(-1f, 1f);
            float offsetZ = Random.Range(-1f, 1f);

            visual.transform.localPosition = new Vector3(offsetX, 0f, offsetZ);

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