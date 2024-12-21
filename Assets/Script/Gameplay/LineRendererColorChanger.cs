using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererColor : MonoBehaviour
{
    // Expose this color in the Inspector
    [SerializeField] private Color lineColor = Color.white;

    private LineRenderer lineRenderer;

    void Awake()
    {
        // Get the LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();

        // Apply the color
        ApplyColor();
    }

    // Apply the color to the LineRenderer
    private void ApplyColor()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }

    // If you want to change the color dynamically, add a setter
    public void SetColor(Color newColor)
    {
        lineColor = newColor;
        ApplyColor();
    }
}
