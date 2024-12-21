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
    [SerializeField] private float accuracyProgressBonus = 0.2f; // Max bonus when 100% accurate
    [SerializeField] private float minDrawLength = 1f; // Minimum length required for a valid draw
    [SerializeField] private float minDrawTime = 0.5f; // Minimum time required for a valid draw
    
    [Header("Combo System")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI maxComboText;
    [SerializeField] private float comboThresholdAccuracy = 0.7f; // Accuracy needed to maintain combo
    [SerializeField] private int comboScoreMultiplier = 10; // Score multiplier per combo
    [SerializeField] private float comboProgressBonus = 0.01f; // Additional progress per combo

    private int currentCombo = 0;
    private int maxCombo = 0;
    
    
    private float drawStartTime;
    private float totalDrawLength = 0f;
    private List<Vector2> playerPoints = new List<Vector2>();
    private bool isDrawing = false;
    private Vector2 lastPoint;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        SetupLineRendererLayers();
        SetupGuideLineRenderer();
        UpdateComboText();
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
    private void UpdateComboText()
    {
        if (comboText != null)
            comboText.text = $"Combo: {currentCombo}";
        if (maxComboText != null)
            maxComboText.text = $"Max Combo: {maxCombo}";
    }

    private void HandleCombo(float accuracy)
    {
        if (accuracy >= comboThresholdAccuracy)
        {
            currentCombo++;
            maxCombo = Mathf.Max(maxCombo, currentCombo);
        }
        else
        {
            currentCombo = 0;
        }
        UpdateComboText();
    }

    private void StartDrawing()
    {
        SetupLineRendererLayers();  // Add this line
        isDrawing = true;
        playerPoints.Clear();
        playerLineRenderer.positionCount = 0;
        Vector2 mousePos = GetMouseWorldPosition();
        AddPoint(mousePos);
        drawStartTime = Time.time;
        totalDrawLength = 0f;
    }

    private void ContinueDrawing()
    {
        if (!isDrawing) return;

        Vector2 mousePos = GetMouseWorldPosition();
        if (Vector2.Distance(lastPoint, mousePos) >= minDistanceForPoint)
        {
            totalDrawLength += Vector2.Distance(lastPoint, mousePos);
            AddPoint(mousePos);
            CheckAccuracy(mousePos);
        }
    }

    private void EndDrawing()
    {
        if (!isDrawing) return;
        isDrawing = false;

        float drawDuration = Time.time - drawStartTime;

        // Validate the drawing attempt
        if (drawDuration < minDrawTime || totalDrawLength < minDrawLength)
        {
            Debug.Log($"Drawing invalid: Duration={drawDuration:F2}s, Length={totalDrawLength:F2}");
            currentCombo = 0; // Break combo on invalid attempt
            UpdateComboText();
            playerPoints.Clear();
            playerLineRenderer.positionCount = 0;
            return;
        }
        
        // Calculate final accuracy
        float accuracy = CalculateOverallAccuracy();
        
        // Handle combo before updating progress
        HandleCombo(accuracy);
        
        // Update progress with combo bonus
        UpdateProgressBasedOnAccuracy(accuracy);
        
        // Display accuracy
        if (accuracyText != null)
        {
            accuracyText.text = $"Accuracy: {accuracy:P1} | Combo: {currentCombo}";
        }
        
        // Calculate score with combo multiplier
        int scoreToAdd = Mathf.RoundToInt(100 * accuracy * (1 + currentCombo * comboScoreMultiplier));
        GameManager.Instance.AddScore(scoreToAdd);

        // Inform shape manager
        shapeManager.HandleShapeCompletion(accuracy);
        
        // Get next shape after a delay
        StartCoroutine(GetNextShapeAfterDelay());
    }

    private void UpdateProgressBasedOnAccuracy(float accuracy)
    {
        // Base progress bonus from accuracy
        float progressBonus = accuracy * accuracyProgressBonus;
        
        // Additional bonus from combo
        progressBonus += currentCombo * comboProgressBonus;
        
        GameManager.Instance.drawingProgressBar = Mathf.Min(
            GameManager.Instance.drawingProgressBar + progressBonus,
            10f
        );
    }
    private IEnumerator GetNextShapeAfterDelay()
    {
        yield return new WaitForSeconds(0.01f);
        currentShape = shapeManager.GetNextShape();
        SetupGuideLineRenderer();
        playerPoints.Clear();
        playerLineRenderer.positionCount = 0;
        DisplayAccuracyText(1f);
    }
    private IEnumerator DisplayAccuracyText(float duration)
    {
        if (accuracyText != null)
        {
            accuracyText.enabled = true; // Make the text visible
            yield return new WaitForSeconds(duration);
            accuracyText.enabled = false; // Hide the text after the duration
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
    void SetupLineRendererLayers()
    {
        // Method 2: Using sorting order
        guideLineRenderer.sortingOrder = 1;  // Higher number = more in front
        playerLineRenderer.sortingOrder = 2;  // Player line appears in front of guide line

        playerLineRenderer.numCornerVertices = 20;
        playerLineRenderer.numCapVertices = 20;
        
        guideLineRenderer.numCornerVertices = 20;
        guideLineRenderer.numCapVertices = 20;
    }
}
