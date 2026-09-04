using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [Header("Timer")]
    public float startingTime = 60f;
    public TextMeshProUGUI timerText;

    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    void Start()
    {
        TimeRemaining = startingTime;
        IsRunning = true;
        UpdateTimerText();
    }

    void Update()
    {
        if (!IsRunning)
        {
            return;
        }

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            UpdateTimerText();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.FailLevel("TIME'S UP!");
            }

            return;
        }

        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(TimeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"TIME: {minutes:00}:{seconds:00}";
    }
}