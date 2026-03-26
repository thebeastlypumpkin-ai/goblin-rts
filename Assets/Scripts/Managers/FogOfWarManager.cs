using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("Fog Settings")]
    [SerializeField] private int textureWidth = 128;
    [SerializeField] private int textureHeight = 128;
    [SerializeField] private float worldWidth = 200f;
    [SerializeField] private float worldHeight = 200f;
    [SerializeField] private int localTeamId = 0;

    [Header("References")]
    [SerializeField] private Renderer fogRenderer;

    public int TextureWidth => textureWidth;
    public int TextureHeight => textureHeight;
    public float WorldWidth => worldWidth;
    public float WorldHeight => worldHeight;

    private Texture2D fogTexture;
    public Texture2D FogTexture => fogTexture;
    private Color[] currentPixels;
    private bool[] exploredPixels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateFogTexture();
        ApplyFogTextureToRenderer();
    }

    private void CreateFogTexture()
    {
        fogTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        currentPixels = new Color[textureWidth * textureHeight];
        exploredPixels = new bool[textureWidth * textureHeight];

        for (int i = 0; i < currentPixels.Length; i++)
        {
            currentPixels[i] = new Color(0f, 0f, 0f, 0.6f);
            exploredPixels[i] = false;
        }

        fogTexture.SetPixels(currentPixels);
        fogTexture.Apply();
    }

    private void ApplyFogTextureToRenderer()
    {
        if (fogRenderer == null)
            return;

        fogRenderer.material.SetTexture("_BaseMap", fogTexture);
    }

    private void Update()
    {
        UpdateFog();
    }

    private void UpdateFog()
    {
        if (VisionManager.Instance == null) return;
        if (currentPixels == null || fogTexture == null) return;

        for (int i = 0; i < currentPixels.Length; i++)
        {
            currentPixels[i] = new Color(0f, 0f, 0f, 0.6f);
        }

        var emitters = VisionManager.Instance.GetEmittersForTeam(localTeamId);

        foreach (var emitter in emitters)
        {
            if (emitter == null) continue;

            RevealVisionAtWorldPosition(emitter.transform.position, emitter.VisionRadius);
        }

        fogTexture.SetPixels(currentPixels);
        fogTexture.Apply();
    }

    private bool WorldToFogPixel(Vector3 worldPosition, out int px, out int py)
    {
        px = 0;
        py = 0;

        if (fogRenderer == null)
            return false;

        Bounds bounds = fogRenderer.bounds;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;

        float normalizedX = 1f - Mathf.InverseLerp(minX, maxX, worldPosition.x);
        float normalizedY = 1f - Mathf.InverseLerp(minZ, maxZ, worldPosition.z);

        px = Mathf.RoundToInt(normalizedX * (textureWidth - 1));
        py = Mathf.RoundToInt(normalizedY * (textureHeight - 1));

        return true;
    }

    private void RevealVisionAtWorldPosition(Vector3 worldPosition, float visionRadius)
    {
        if (!WorldToFogPixel(worldPosition, out int centerX, out int centerY))
            return;

        float pixelRadiusX = (visionRadius / worldWidth) * textureWidth;
        float pixelRadiusY = (visionRadius / worldHeight) * textureHeight;

        int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - pixelRadiusX));
        int maxX = Mathf.Min(textureWidth - 1, Mathf.CeilToInt(centerX + pixelRadiusX));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - pixelRadiusY));
        int maxY = Mathf.Min(textureHeight - 1, Mathf.CeilToInt(centerY + pixelRadiusY));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = (x - centerX) / pixelRadiusX;
                float dy = (y - centerY) / pixelRadiusY;

                if ((dx * dx) + (dy * dy) <= 1f)
                {
                    int index = y * textureWidth + x;

                    exploredPixels[index] = true;
                    currentPixels[index] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }
    }
}