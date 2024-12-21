using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShapeCreator : MonoBehaviour
{
    [Header("Shape Settings")]
    public Color shapeColor = Color.blue;
    public float lineWidth = 0.1f;
    
    [Header("Preview")]
    public LineRenderer previewLine;
    
    private List<Vector2> points = new List<Vector2>();
    private bool isCreating = false;

    private void OnDrawGizmos()
    {
        // Draw points
        Gizmos.color = shapeColor;
        foreach (Vector2 point in points)
        {
            Gizmos.DrawSphere(point, lineWidth/2);
        }

        // Draw lines between points
        if (points.Count > 1)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }
    }

    public void AddPoint(Vector2 point)
    {
        points.Add(point);
        UpdatePreviewLine();
    }

    public void ClearPoints()
    {
        points.Clear();
        UpdatePreviewLine();
    }

    private void UpdatePreviewLine()
    {
        if (previewLine != null)
        {
            previewLine.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                previewLine.SetPosition(i, new Vector3(points[i].x, points[i].y, 0));
            }
        }
    }

    public Vector2[] GetPoints()
    {
        return points.ToArray();
    }

    public void LoadPoints(Vector2[] loadedPoints)
    {
        points = new List<Vector2>(loadedPoints);
        UpdatePreviewLine();
    }
}