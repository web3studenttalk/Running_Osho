using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RaceGameManager : MonoBehaviour
{
    public static RaceGameManager Instance;

    [Header("ゲーム全体の設定")]
    public float globalBaseSpeed = 10.0f;

    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject startButtonObj;
    [SerializeField] private GameObject goalTextObj;
    [SerializeField] private TextMeshProUGUI resultText; // 順位表示用

    private bool isRacing = false;
    private float currentTime = 0f;
    private List<string> rankingList = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (goalTextObj != null) goalTextObj.SetActive(false);
        if (resultText != null) resultText.text = "";
        UpdateTimerUI();
    }

    void Update()
    {
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void OnStartButton()
    {
        isRacing = true;
        currentTime = 0f;
        rankingList.Clear();
        if (resultText != null) resultText.text = "";
        if (startButtonObj != null) startButtonObj.SetActive(false);
        if (goalTextObj != null) goalTextObj.SetActive(false);
    }

    // 各レーサーがゴールした時に呼び出す
    public void ReportGoal(string racerName)
    {
        if (rankingList.Contains(racerName)) return;

        rankingList.Add(racerName);
        int rank = rankingList.Count;

        string timeStr = GetFormattedTime(currentTime);

        // UIに順位を追記
        if (resultText != null)
        {
            resultText.text += $"{rank}: {racerName} ({timeStr})\n";
        }

        // プレイヤー(Racerタグ)がゴールしたらタイマーを止める
        if (racerName.Contains("Player") || racerName.Contains("PlayerRig"))
        {
            isRacing = false;
            if (goalTextObj != null) goalTextObj.SetActive(true);
        }
    }

    public bool IsRacing() => isRacing;

    private void UpdateTimerUI()
    {
        if (timerText != null) timerText.text = GetFormattedTime(currentTime);
    }

    private string GetFormattedTime(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60F);
        float seconds = Mathf.FloorToInt(time % 60F);
        float milliseconds = Mathf.FloorToInt((time * 100F) % 100F);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}