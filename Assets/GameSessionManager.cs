using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    private const string UsedNicknamesKey = "UsedNicknamesList";
    private const string ScoreboardKey = "ParkourScoreboardData";

    [Serializable]
    public class ScoreEntry
    {
        public string nickname;
        public int stars;
        public int remainingSeconds;
    }

    [Header("Panel ve UI Elemanlari")]
    public GameObject gamePanel;
    public InputField nicknameInput;
    public Button playButton;
    public Text statusText;
    public Text scoreText;
    public Text timerText;
    public Text scoreboardText;

    [Header("Oyun Ayarlari")]
    public string defaultNickname = "Player";
    public float roundDurationSeconds = 30f;

    public bool IsGameplayActive { get; private set; } = false;
    public string PlayerNickname { get; private set; } = "Player";

    private int totalStars = 10;
    private int collectedStars = 0;
    private float timeRemaining = 30f;
    private bool roundEnded = false;
    private bool scoreSavedForRound = false;
    private HashSet<string> usedNicknames = new HashSet<string>();
    private List<ScoreEntry> scoreboardEntries = new List<ScoreEntry>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        usedNicknames = LoadUsedNicknames();
        scoreboardEntries = LoadScoreboard();
    }

    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGameFromButton);
        }

        PrepareStartScreen();
    }

    void Update()
    {
        if (!IsGameplayActive || roundEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateHudTexts();
            OnTimeExpired();
            return;
        }

        UpdateHudTexts();
    }

    public void PrepareStartScreen()
    {
        IsGameplayActive = false;
        roundEnded = false;
        scoreSavedForRound = false;

        totalStars = GetCourseStarCount();
        collectedStars = 0;
        timeRemaining = roundDurationSeconds > 0f ? roundDurationSeconds : totalStars * 3f;

        SetRobotFrozen(true);

        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "Enter your nickname and press PLAY!";
        }

        RefreshScoreboardUI();
        UpdateHudTexts();
    }

    public void StartGameFromButton()
    {
        string typedName = nicknameInput != null ? nicknameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(typedName))
        {
            ShowPanelWithMessage("Please enter a nickname before pressing PLAY!");
            return;
        }

        string normalizedNickname = typedName.ToLowerInvariant();
        if (usedNicknames.Contains(normalizedNickname))
        {
            ShowPanelWithMessage("Nickname \"" + typedName + "\" has already been used! Choose another one.");
            return;
        }

        usedNicknames.Add(normalizedNickname);
        SaveUsedNicknames(usedNicknames);

        PlayerNickname = typedName;

        roundEnded = false;
        scoreSavedForRound = false;
        IsGameplayActive = false;

        RobotMover mover = FindObjectOfType<RobotMover>();
        if (mover != null)
        {
            mover.RestartGame();
        }
        else
        {
            CourseGenerator generator = FindObjectOfType<CourseGenerator>();
            if (generator != null)
            {
                generator.RebuildCourse();
            }
        }

        totalStars = GetCourseStarCount();
        collectedStars = 0;
        timeRemaining = roundDurationSeconds > 0f ? roundDurationSeconds : totalStars * 3f;
        roundEnded = false;
        scoreSavedForRound = false;
        IsGameplayActive = true;

        if (statusText != null)
        {
            statusText.text = "";
        }

        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }

        SetRobotFrozen(false);
        UpdateHudTexts();

        Debug.Log("Game started! Player: " + PlayerNickname + " | Time: " + timeRemaining + "s");
    }

    public void ResetSessionCounters(bool keepPanelOpen)
    {
        totalStars = GetCourseStarCount();
        collectedStars = 0;
        timeRemaining = roundDurationSeconds > 0f ? roundDurationSeconds : totalStars * 3f;
        roundEnded = false;
        scoreSavedForRound = false;

        if (statusText != null)
        {
            statusText.text = "";
        }

        if (gamePanel != null)
        {
            gamePanel.SetActive(keepPanelOpen);
        }

        IsGameplayActive = !keepPanelOpen;
        SetRobotFrozen(keepPanelOpen);
        UpdateHudTexts();
    }

    private int GetCourseStarCount()
    {
        CourseGenerator generator = FindObjectOfType<CourseGenerator>();
        if (generator != null && generator.starCount > 0)
        {
            return generator.starCount;
        }

        int starCount = FindObjectsOfType<StarCollector>().Length;
        return starCount > 0 ? starCount : 10;
    }

    public void RegisterStarCollected()
    {
        if (!IsGameplayActive || roundEnded)
        {
            return;
        }

        collectedStars = Mathf.Min(collectedStars + 1, totalStars);
        UpdateHudTexts();

        if (collectedStars >= totalStars)
        {
            roundEnded = true;
            IsGameplayActive = false;
            SetRobotFrozen(true);
            RecordCurrentScore();

            string msg = "CONGRATULATIONS, " + PlayerNickname + "! All stars collected (" + collectedStars + "/" + totalStars + ")!";
            Debug.Log(msg + "\n" + GetScoreboardSummary());
            ShowPanelWithMessage(msg);
        }
    }

    public void OnPlayerHitHazard(string reason = "GAME OVER! You hit a cylinder obstacle! Press R to restart.")
    {
        if (roundEnded)
        {
            return;
        }

        roundEnded = true;
        IsGameplayActive = false;
        SetRobotFrozen(true);
        RecordCurrentScore();

        string msg = PlayerNickname + ": " + reason;
        Debug.Log(msg + "\n" + GetScoreboardSummary());
        ShowPanelWithMessage(msg);
    }

    private void OnTimeExpired()
    {
        roundEnded = true;
        IsGameplayActive = false;
        SetRobotFrozen(true);
        RecordCurrentScore();

        string msg = "TIME'S UP, " + PlayerNickname + "! Stars collected: " + collectedStars + "/" + totalStars + ". Press R to restart.";
        Debug.Log(msg + "\n" + GetScoreboardSummary());
        ShowPanelWithMessage(msg);
    }

    private void RecordCurrentScore()
    {
        if (scoreSavedForRound)
        {
            return;
        }

        scoreSavedForRound = true;
        scoreboardEntries.Add(new ScoreEntry
        {
            nickname = string.IsNullOrWhiteSpace(PlayerNickname) ? defaultNickname : PlayerNickname,
            stars = collectedStars,
            remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(timeRemaining))
        });

        scoreboardEntries = scoreboardEntries
            .OrderByDescending(e => e.stars)
            .ThenByDescending(e => e.remainingSeconds)
            .Take(5)
            .ToList();

        SaveScoreboard(scoreboardEntries);
    }

    public string GetScoreboardSummary()
    {
        if (scoreboardEntries == null || scoreboardEntries.Count == 0)
        {
            return "SCOREBOARD: No scores yet.";
        }

        List<string> lines = new List<string> { "SCOREBOARD (TOP 5):" };
        for (int i = 0; i < scoreboardEntries.Count; i++)
        {
            ScoreEntry entry = scoreboardEntries[i];
            lines.Add((i + 1) + ". " + entry.nickname + " - " + entry.stars + "/" + totalStars + " Stars (" + entry.remainingSeconds + "s left)");
        }
        return string.Join("\n", lines);
    }

    private void RefreshScoreboardUI()
    {
        string summary = GetScoreboardSummary();
        if (scoreboardText != null)
        {
            scoreboardText.gameObject.SetActive(true);
            scoreboardText.text = summary;
        }
        else if (statusText != null)
        {
            statusText.text = string.IsNullOrEmpty(statusText.text) ? summary : statusText.text + "\n\n" + summary;
        }
    }

    private void ShowPanelWithMessage(string message)
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = message;
        }

        RefreshScoreboardUI();
    }

    private void UpdateHudTexts()
    {
        if (scoreText != null)
        {
            scoreText.text = PlayerNickname + " | Stars: " + collectedStars + " / " + totalStars;
        }

        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining) + "s";
        }
    }

    private void SetRobotFrozen(bool frozen)
    {
        RobotMover mover = FindObjectOfType<RobotMover>();
        if (mover != null)
        {
            mover.SetMovementLocked(frozen);
        }
    }

    private HashSet<string> LoadUsedNicknames()
    {
        string saved = PlayerPrefs.GetString(UsedNicknamesKey, "");
        if (string.IsNullOrWhiteSpace(saved))
        {
            return new HashSet<string>();
        }

        return saved
            .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim().ToLowerInvariant())
            .Where(name => !string.IsNullOrEmpty(name))
            .ToHashSet();
    }

    private void SaveUsedNicknames(HashSet<string> names)
    {
        PlayerPrefs.SetString(UsedNicknamesKey, string.Join("|", names));
        PlayerPrefs.Save();
    }

    public void ResetUsedNicknames()
    {
        usedNicknames.Clear();
        PlayerPrefs.DeleteKey(UsedNicknamesKey);
        PlayerPrefs.Save();
    }

    private List<ScoreEntry> LoadScoreboard()
    {
        string raw = PlayerPrefs.GetString(ScoreboardKey, "");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<ScoreEntry>();
        }

        List<ScoreEntry> result = new List<ScoreEntry>();
        string[] rows = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] parts = row.Split('|');
            if (parts.Length >= 3 && int.TryParse(parts[1], out int starsVal) && int.TryParse(parts[2], out int secVal))
            {
                result.Add(new ScoreEntry
                {
                    nickname = parts[0],
                    stars = starsVal,
                    remainingSeconds = secVal
                });
            }
        }
        return result;
    }

    private void SaveScoreboard(List<ScoreEntry> entries)
    {
        List<string> rows = new List<string>();
        foreach (ScoreEntry entry in entries)
        {
            rows.Add(entry.nickname.Replace("|", "") + "|" + entry.stars + "|" + entry.remainingSeconds);
        }
        PlayerPrefs.SetString(ScoreboardKey, string.Join("\n", rows));
        PlayerPrefs.Save();
    }
}