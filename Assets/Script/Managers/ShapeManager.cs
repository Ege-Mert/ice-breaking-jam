using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    [Header("Shape Collections")]
    [SerializeField] private List<ShapeData> easyShapes;
    [SerializeField] private List<ShapeData> mediumShapes;
    [SerializeField] private List<ShapeData> hardShapes;

    [Header("Difficulty Settings")]
    [SerializeField] private Vector2 easyToMediumAccuracyRange = new Vector2(0.75f, 0.85f);
    [SerializeField] private Vector2 mediumToHardAccuracyRange = new Vector2(0.85f, 0.95f);
    [SerializeField] private int shapesNeededForProgression = 3;

    private int currentDifficulty = 0;
    private int successfulShapesCompleted = 0;
    private float accuracyThresholdForNextLevel;

    private void Start()
    {
        SetNewAccuracyThreshold();
    }

    private void SetNewAccuracyThreshold()
    {
        if (currentDifficulty == 0)
        {
            accuracyThresholdForNextLevel = Random.Range(easyToMediumAccuracyRange.x, easyToMediumAccuracyRange.y);
        }
        else if (currentDifficulty == 1)
        {
            accuracyThresholdForNextLevel = Random.Range(mediumToHardAccuracyRange.x, mediumToHardAccuracyRange.y);
        }
        Debug.Log($"Need {accuracyThresholdForNextLevel:F2} accuracy to progress to next level");
    }

    public ShapeData GetNextShape()
    {
        List<ShapeData> currentPool = currentDifficulty switch
        {
            0 => easyShapes,
            1 => mediumShapes,
            _ => hardShapes
        };

        if (currentPool.Count == 0) return null;
        return currentPool[Random.Range(0, currentPool.Count)];
    }

    public void HandleShapeCompletion(float accuracy)
    {
       // Debug.Log($"Shape completed with accuracy: {accuracy:F2}");

        if (accuracy >= accuracyThresholdForNextLevel)
        {
            successfulShapesCompleted++;
            
            if (successfulShapesCompleted >= shapesNeededForProgression && currentDifficulty < 2)
            {
                currentDifficulty++;
                successfulShapesCompleted = 0;
                SetNewAccuracyThreshold();
                Debug.Log($"Advanced to difficulty level: {currentDifficulty}");
            }
        }
        else
        {
            successfulShapesCompleted = Mathf.Max(0, successfulShapesCompleted - 1);
        }
    }
}