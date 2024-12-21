using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI currentLineText;
    [SerializeField] private TextMeshProUGUI nextLineText;
    [SerializeField] private TextMeshProUGUI typedText;
    [SerializeField] private CodeLineCollection codeLines;

    [Header("Difficulty Settings")]
    [SerializeField] private Vector2 easyToMediumComboRange = new Vector2(15, 25); // Random range for progression
    [SerializeField] private Vector2 mediumToHardComboRange = new Vector2(25, 35);
    [SerializeField] private int linesToPickFrom = 3; // How many lines to randomly choose from

    [Header("Combo System")]
    [SerializeField] private int currentCombo = 0;
    [SerializeField] private int maxCombo = 0;
    [SerializeField] private float progressBarBonus = 0.1f;
    private int comboRequiredForNextLevel;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mistypeSound;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("Score")]
    [SerializeField] private int scorePerCorrectChar = 10;
    [SerializeField] private int penaltyPerMistype = 50;
    private bool isFlashing = false;
    
    [Header("ScoreUI")]
    [SerializeField] private TextMeshProUGUI CurrentComboText;
    [SerializeField] private TextMeshProUGUI MaxComboText;
    

    private string currentLine = "";
    private string nextLine = "";
    private int currentIndex = 0;
    private int currentDifficulty = 0; // 0: Easy, 1: Medium, 2: Hard

    private void Start()
    {
        SetNewComboThreshold();
        GetNewLine();
    }

    private void Update()
    {
        CurrentComboText.text = currentCombo.ToString();
        MaxComboText.text = maxCombo.ToString();
        
        if (GameManager.Instance.isGameOver) return;

        foreach (char c in Input.inputString)
        {
            if (currentIndex < currentLine.Length)
            {
                if (c == currentLine[currentIndex])
                {
                    // Correct character
                    currentIndex++;
                    currentCombo++;
                    maxCombo = Mathf.Max(maxCombo, currentCombo);
                    UpdateTypedText();
                    
                    // Add score based on combo
                    int scoreToAdd = scorePerCorrectChar * (1 + currentCombo / 10);
                    GameManager.Instance.AddScore(scoreToAdd);
                
                    // Progress bar bonus
                    float currentProgress = GameManager.Instance.codingProgressBar;
                    currentProgress += progressBarBonus * (1 + currentCombo * 0.1f);
                    GameManager.Instance.codingProgressBar = Mathf.Min(currentProgress, 10f);
                }
                else
                {
                    // Wrong character
                    currentCombo = 0;
                    GameManager.Instance.codingProgressBar -= 0.05f; // Penalty

                    // Play sound
                    if (audioSource != null && mistypeSound != null)
                    {
                        audioSource.PlayOneShot(mistypeSound);
                    }
                    float currentProgress = GameManager.Instance.codingProgressBar;
                    currentProgress -= penaltyPerMistype;
                    GameManager.Instance.codingProgressBar = Mathf.Min(currentProgress, 10f);

                    // Visual feedback
                    StartCoroutine(FlashBackground());

                    // Score penalty
                    GameManager.Instance.AddScore(-penaltyPerMistype);
                }
            }

            // Check if line is completed
            if (currentIndex >= currentLine.Length)
            {
                CompleteCurrentLine();
            }
        }
    }
        private void SetNewComboThreshold()
    {
        if (currentDifficulty == 0)
        {
            comboRequiredForNextLevel = Random.Range((int)easyToMediumComboRange.x, (int)easyToMediumComboRange.y + 1);
        }
        else if (currentDifficulty == 1)
        {
            comboRequiredForNextLevel = Random.Range((int)mediumToHardComboRange.x, (int)mediumToHardComboRange.y + 1);
        }
        Debug.Log($"Need {comboRequiredForNextLevel} combo to progress to next level");
    }

    private void UpdateTypedText()
    {
        string typed = "<color=green>" + currentLine.Substring(0, currentIndex) + "</color>";
        string remaining = "<color=white>" + currentLine.Substring(currentIndex) + "</color>";
        typedText.text = typed + remaining;
    }
    private void GetNewLine()
    {
        List<CodeLine> currentPool = currentDifficulty switch
        {
            0 => codeLines.easyLines,
            1 => codeLines.mediumLines,
            _ => codeLines.hardLines
        };

        if (currentPool.Count == 0) return;

        // Get subset of lines to choose from
        int poolSize = Mathf.Min(linesToPickFrom, currentPool.Count);
        List<int> availableIndices = new List<int>();
        
        for(int i = 0; i < currentPool.Count; i++)
        {
            availableIndices.Add(i);
        }

        // Get current line
        int randomIndex = Random.Range(0, availableIndices.Count);
        int selectedIndex = availableIndices[randomIndex];
        currentLine = currentPool[selectedIndex].text;
        availableIndices.RemoveAt(randomIndex);

        // Get next line (from remaining indices)
        if(availableIndices.Count > 0)
        {
            randomIndex = Random.Range(0, availableIndices.Count);
            selectedIndex = availableIndices[randomIndex];
            nextLine = currentPool[selectedIndex].text;
        }
        else
        {
            nextLine = currentPool[Random.Range(0, currentPool.Count)].text;
        }

        currentIndex = 0;
        UpdateDisplayedText();
    }

    private void UpdateDisplayedText()
    {
        currentLineText.text = currentLine;
        nextLineText.text = nextLine;
        typedText.text = currentLine;
        nextLineText.color = new Color(nextLineText.color.r, nextLineText.color.g, nextLineText.color.b, 0.5f);
    }

    private void CompleteCurrentLine()
    {
        // Add score based on combo
        int scoreToAdd = 100 * (1 + currentCombo / 10);
        GameManager.Instance.AddScore(scoreToAdd);

        // Progress difficulty if needed
        if (currentCombo >= comboRequiredForNextLevel && currentDifficulty < 2)
        {
            currentDifficulty++;
            SetNewComboThreshold();
            Debug.Log($"Advanced to difficulty level: {currentDifficulty}");
        }

        GetNewLine();
    }
    private IEnumerator FlashBackground()
    {
        if (isFlashing) yield break;
        
        isFlashing = true;
        backgroundPanel.color = errorColor;
        yield return new WaitForSeconds(flashDuration);
        backgroundPanel.color = normalColor;
        isFlashing = false;
    }
}