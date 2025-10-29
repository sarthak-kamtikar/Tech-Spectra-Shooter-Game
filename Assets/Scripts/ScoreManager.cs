using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;   // singleton pattern
    public int score = 0;
    public UnityEvent<int> onScoreChanged;

    void Awake()                                          // i write awake unity calls it and im updating that awake with mine
                                           //You write Awake(), Unity calls it, and whenever you update the code inside it, Unity will call your new version automatically
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        if (onScoreChanged == null) onScoreChanged = new UnityEvent<int>();
    }

    public void AddScore(int amount)
    {
        score += amount;
        onScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        onScoreChanged?.Invoke(score);
    }
}
