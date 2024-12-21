using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ShapeTracing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer guideLineRenderer;  // For the shape to trace
    [SerializeField] private LineRenderer playerLineRenderer; // For the player's drawing
    [SerializeField] private ShapeData currentShape;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private TextMeshProUGUI accuracyText; // For displaying accuracy

    [Header("Drawing Settings")]
    [SerializeField] private float minDistanceForPoint = 0.1f;
    [SerializeField] private float accuracyThreshold = 0.1f;
    
    private List<Vector2> playerPoints = new List<Vector2>();
    private bool isDrawing = false;
    private Vector2 lastPoint;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        SetupGuideLineRenderer();
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrawing();
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        playerPoints.Clear();
        playerLineRenderer.positionCount = 0;
        Vector2 mousePos = GetMouseWorldPosition();
        AddPoint(mousePos);
    }

    private void ContinueDrawing()
    {
        if (!isDrawing) return;

        Vector2 mousePos = GetMouseWorldPosition();
        if (Vector2.Distance(lastPoint, mousePos) >= minDistanceForPoint)
        {
            AddPoint(mousePos);
            CheckAccuracy(mousePos);
        }
    }

    private void EndDrawing()
    {
        if (!isDrawing) return;
        isDrawing = false;
        
        float accuracy = CalculateOverallAccuracy();
        UpdateProgressBasedOnAccuracy(accuracy);
        
        // Display accuracy
        if (accuracyText != null)
        {
            accuracyText.text = $"Accuracy: {accuracy:P1}";
        }
        
        // Inform shape manager
        shapeManager.HandleShapeCompletion(accuracy);
        
        // Get next shape after a short delay
        StartCoroutine(GetNextShapeAfterDelay());
    } 
    private IEnumerator GetNextShapeAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        currentShape = shapeManager.GetNextShape();
        SetupGuideLineRenderer();
        playerPoints.Clear();
        playerLineRenderer.positionCount = 0;
        if (accuracyText != null)
        {
            accuracyText.text = "";
        }
    }

    private void AddPoint(Vector2 point)
    {
        playerPoints.Add(point);
        lastPoint = point;
        
        // Update line renderer
        playerLineRenderer.positionCount = playerPoints.Count;
        playerLineRenderer.SetPosition(playerPoints.Count - 1, new Vector3(point.x, point.y, 0));
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void CheckAccuracy(Vector2 currentPoint)
    {
        // Find closest point on guide line
        float minDistance = float.MaxValue;
        for (int i = 0; i < currentShape.pathPoints.Length - 1; i++)
        {
            float distance = HandleUtility.DistancePointLine(
                currentPoint,
                currentShape.pathPoints[i],
                currentShape.pathPoints[i + 1]
            );
            minDistance = Mathf.Min(minDistance, distance);
        }

        // Update progress bar based on accuracy
        if (minDistance > accuracyThreshold)
        {
            GameManager.Instance.drawingProgressBar -= 0.05f;
        }
    }

    private float CalculateOverallAccuracy()
    {
        float totalDistance = 0;
        int pointCount = 0;

        foreach (Vector2 point in playerPoints)
        {
            float minDistance = float.MaxValue;
            for (int i = 0; i < currentShape.pathPoints.Length - 1; i++)
            {
                float distance = HandleUtility.DistancePointLine(
                    point,
                    currentShape.pathPoints[i],
                    currentShape.pathPoints[i + 1]
                );
                minDistance = Mathf.Min(minDistance, distance);
            }
            totalDistance += minDistance;
            pointCount++;
        }

        return 1f - Mathf.Clamp01(totalDistance / (pointCount * accuracyThreshold));
    }

    private void UpdateProgressBasedOnAccuracy(float accuracy)
    {
        float progressBonus = accuracy * 0.2f; // 20% max bonus
        GameManager.Instance.drawingProgressBar = Mathf.Min(
            GameManager.Instance.drawingProgressBar + progressBonus,
            10f
        );
    }

    private void SetupGuideLineRenderer()
    {
        if (currentShape == null || currentShape.pathPoints == null) return;

        guideLineRenderer.positionCount = currentShape.pathPoints.Length;
        for (int i = 0; i < currentShape.pathPoints.Length; i++)
        {
            guideLineRenderer.SetPosition(i, new Vector3(
                currentShape.pathPoints[i].x,
                currentShape.pathPoints[i].y,
                0
            ));
        }
    }
}
