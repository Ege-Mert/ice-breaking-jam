using UnityEngine;

[CreateAssetMenu(fileName = "ShapeData", menuName = "Game/Shape Data")]
public class ShapeData : ScriptableObject
{
    public Vector2[] pathPoints;
    public float toleranceThreshold = 0.1f;
    public int difficulty; // 0: Easy, 1: Medium, 2: Hard
    public float completionThreshold = 0.8f;
}