using UnityEngine;

[CreateAssetMenu(fileName = "NewShape", menuName = "Game/Shape Data")]
public class ShapeData : ScriptableObject
{
    public Vector2[] pathPoints;
    public float toleranceThreshold = 0.1f;
    public int difficulty; // 0: Easy, 1: Medium, 2: Hard
    public float completionThreshold = 0.8f;
    
    [Header("Visual Settings")]
    public Color shapeColor = Color.blue;
    public float lineWidth = 0.1f;
    public bool useGradient = false;
    public Gradient lineGradient;

    public void LoadIntoLineRenderer(LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = pathPoints.Length;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(pathPoints[i].x, pathPoints[i].y, 0));
        }

        lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
        
        if (useGradient)
            lineRenderer.colorGradient = lineGradient;
        else
            lineRenderer.startColor = lineRenderer.endColor = shapeColor;
    }
}