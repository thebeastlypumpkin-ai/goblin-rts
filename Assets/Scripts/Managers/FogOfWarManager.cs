using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("Fog Settings")]
    [SerializeField] private int textureWidth = 128;
    [SerializeField] private int textureHeight = 128;
    [SerializeField] private float worldWidth = 200f;
    [SerializeField] private float worldHeight = 200f;

    [Header("References")]
    [SerializeField] private Renderer fogRenderer;

    public int TextureWidth => textureWidth;
    public int TextureHeight => textureHeight;
    public float WorldWidth => worldWidth;
    public float WorldHeight => worldHeight;

    private Texture2D fogTexture;
    public Texture2D FogTexture => fogTexture;

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

        Color[] pixels = new Color[textureWidth * textureHeight];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0f, 0f, 0f, 0.6f);
        }

        fogTexture.SetPixels(pixels);
        fogTexture.Apply();
    }

    private void ApplyFogTextureToRenderer()
    {
        if (fogRenderer == null)
            return;

        fogRenderer.material.mainTexture = fogTexture;
    }
}