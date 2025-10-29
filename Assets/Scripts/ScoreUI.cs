using UnityEngine;
using TMPro; // Use TextMeshPro namespace

public class ScoreUI : MonoBehaviour
{
    [Tooltip("Assign your TextMeshProUGUI score text here")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onScoreChanged.AddListener(OnScoreChanged);
            OnScoreChanged(ScoreManager.Instance.score);
        }
        else
        {
            UpdateText(0);
        }
    }

    void OnScoreChanged(int newScore)
    {
        UpdateText(newScore);
    }

    void UpdateText(int s)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + s.ToString();
    }
}
    