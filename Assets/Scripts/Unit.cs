using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool IsSelected { get; private set; }

    private Renderer unitRenderer;
    private Color originalColor;

    void Awake()
    {
        unitRenderer = GetComponent<Renderer>();
        originalColor = unitRenderer.material.color;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (IsSelected)
        {
            unitRenderer.material.color = Color.green;
        }
        else
        {
            unitRenderer.material.color = originalColor;
        }
    }
}