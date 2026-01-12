using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RaceGameManager : MonoBehaviour
{
    public static RaceGameManager Instance;
    public float globalBaseSpeed = 10.0f;

    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject startButtonObj;
    [SerializeField] private GameObject goalTextObj;
    [SerializeField] private GameObject gameOverTextObj; 
    [SerializeField] private TextMeshProUGUI resultText;

    private bool isRacing = false;
    private float currentTime = 0f;
    private List<string> rankingList = new List<string>();

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
    void Start() { if (goalTextObj) goalTextObj.SetActive(false); if (gameOverTextObj) gameOverTextObj.SetActive(false); }
    void Update() { if (isRacing) { currentTime += Time.deltaTime; if(timerText) timerText.text = FormatTime(currentTime); } }

    public void OnStartButton() { isRacing = true; currentTime = 0f; rankingList.Clear(); if(startButtonObj) startButtonObj.SetActive(false); }
    
    // ゴール時の処理（名前を受け取る）
    public void ReportGoal(string name) {
        if (rankingList.Contains(name)) return;
        rankingList.Add(name);
        if (resultText) resultText.text += $"{rankingList.Count}位: {name} ({FormatTime(currentTime)})\n";
        if (name.Contains("Player") || name.Contains("PlayerRig")) { isRacing = false; if(goalTextObj) goalTextObj.SetActive(true); }
    }

    public void GameOver() {
        isRacing = false;
        if (gameOverTextObj) gameOverTextObj.SetActive(true);
        Debug.Log("王将が取られました！ゲームオーバー");
    }

    public bool IsRacing() => isRacing;
    private string FormatTime(float t) => string.Format("{0:00}:{1:00}.{2:00}", Mathf.FloorToInt(t/60), Mathf.FloorToInt(t%60), Mathf.FloorToInt((t*100)%100));
}