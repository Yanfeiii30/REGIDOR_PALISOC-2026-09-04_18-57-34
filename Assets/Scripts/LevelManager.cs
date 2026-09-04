using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level 1")]
    public int totalObjectives = 2;
    public TextMeshProUGUI progressText;

    [Header("Lose Window")]
    public GameObject losePanel;
    public TextMeshProUGUI loseReasonText;

    [Header("Stats Window")]
    public GameObject statsPanel;
    public TextMeshProUGUI statsText;
    public PlayerMovement playerMovement;
    public LevelTimer levelTimer;

    public int CorrectObjectives { get; private set; }
    public int Mistakes { get; private set; }
    public bool IsLevelEnded { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }

        UpdateProgressText();
    }

    public void RegisterCorrect()
    {
        if (IsLevelEnded) return;

        CorrectObjectives++;
        UpdateProgressText();

        if (CorrectObjectives >= totalObjectives)
        {
            CompleteLevel();
        }
    }

    public void RegisterMistake()
    {
        if (IsLevelEnded) return;

        Mistakes++;
        UpdateProgressText();
    }

    void CompleteLevel()
    {
        if (IsLevelEnded) return;

        IsLevelEnded = true;

        int ammoLeft = playerMovement != null ? playerMovement.CurrentAmmo : 0;
        int secondsLeft = levelTimer != null
            ? Mathf.CeilToInt(levelTimer.TimeRemaining)
            : 0;

        int score = Mathf.Max(
            0,
            CorrectObjectives * 100 +
            ammoLeft * 20 +
            secondsLeft * 5 -
            Mistakes * 25
        );

        if (statsText != null)
        {
            statsText.text =
                "Correct: " + CorrectObjectives +
                "\nMistakes: " + Mistakes +
                "\nAmmo Left: " + ammoLeft +
                "\nTime Left: " + FormatTime(secondsLeft) +
                "\nScore: " + score;
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void FailLevel(string reason)
    {
        if (IsLevelEnded) return;

        IsLevelEnded = true;

        if (loseReasonText != null)
        {
            loseReasonText.text = reason;
        }

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    void UpdateProgressText()
    {
        if (progressText == null) return;

        progressText.text =
            "Correct: " + CorrectObjectives + "/" + totalObjectives +
            "\nMistakes: " + Mistakes;
    }
}