using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Progress Bars")]
    [SerializeField] private float maxProgressBar = 10f;
    public float codingProgressBar;
    public float drawingProgressBar;
    [SerializeField] private float drainRate = 0.1f;
    
    [Header("Game State")]
    public bool isGameOver = false;
    public int score = 0;
    [Header("UI Elements")]
    public Slider codingProgressSlider;
    public Slider drawingProgressSlider;
    
    private void Start()
    {
        codingProgressBar = maxProgressBar;
        drawingProgressBar = maxProgressBar;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // Drain progress bars
        codingProgressBar -= drainRate * Time.deltaTime;
        drawingProgressBar -= drainRate * Time.deltaTime;

        // Update sliders
        codingProgressSlider.value = codingProgressBar / maxProgressBar;
        drawingProgressSlider.value = drawingProgressBar / maxProgressBar;

        // Check game over condition
        if (codingProgressBar <= 0 || drawingProgressBar <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        // Add game over logic here
        Debug.Log("Game Over! Final Score: " + score);
    }

    public void AddScore(int points)
    {
        score += points;
    }
}