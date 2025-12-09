// RaceGameManager.cs
using UnityEngine;
using TMPro;

public class RaceGameManager : MonoBehaviour
{
    public static RaceGameManager Instance;

    [Header("ゲーム全体の設定")]
    [Tooltip("全てのコマの基準となる速度（時速のようなもの）")]
    public float globalBaseSpeed = 10.0f; // ← 新しく追加！

    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject startButtonObj;
    [SerializeField] private GameObject goalTextObj;

    private bool isRacing = false;
    private float currentTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (goalTextObj != null) goalTextObj.SetActive(false);
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
        if (startButtonObj != null) startButtonObj.SetActive(false);
    }

    public void OnGoal()
    {
        if (!isRacing) return;
        isRacing = false;
        if (goalTextObj != null) goalTextObj.SetActive(true);
    }

    public bool IsRacing()
    {
        return isRacing;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float minutes = Mathf.FloorToInt(currentTime / 60F);
            float seconds = Mathf.FloorToInt(currentTime % 60F);
            float milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);
            timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }
}