// RaceGameManager.cs
using UnityEngine;
using TMPro; // TextMeshProを使うため

public class RaceGameManager : MonoBehaviour
{
    // どこからでもアクセスできるようにする（シングルトン）
    public static RaceGameManager Instance;

    [Header("UI参照")]
    [Tooltip("タイムを表示するテキスト")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Tooltip("スタート時に押すボタン（ゲーム開始後に消すため）")]
    [SerializeField] private GameObject startButtonObj;

    [Tooltip("ゴール時に表示するテキスト（任意）")]
    [SerializeField] private GameObject goalTextObj;

    // ゲームの状態
    private bool isRacing = false;
    private float currentTime = 0f;

    void Awake()
    {
        // 自分自身を登録（これで他のスクリプトから RaceGameManager.Instance で呼べるようになります）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初期化
        if (goalTextObj != null) goalTextObj.SetActive(false);
        if (startButtonObj != null) startButtonObj.SetActive(true);
        UpdateTimerUI();
    }

    void Update()
    {
        // レース中のみタイマーを進める
        if (isRacing)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // --- 外部から呼ぶ機能 ---

    /// <summary>
    /// スタートボタンが押されたら呼ばれる
    /// </summary>
    public void OnStartButton()
    {
        isRacing = true;
        currentTime = 0f;

        // スタートボタンを隠す
        if (startButtonObj != null) startButtonObj.SetActive(false);
        
        Debug.Log("レーススタート！");
    }

    /// <summary>
    /// プレイヤーがゴールしたら呼ばれる
    /// </summary>
    public void OnGoal()
    {
        if (!isRacing) return; // 既にゴールしてたら何もしない

        isRacing = false; // タイマー停止
        
        // ゴール表示
        if (goalTextObj != null) goalTextObj.SetActive(true);

        Debug.Log("ゴール！ タイム: " + currentTime);
    }

    /// <summary>
    /// プレイヤーが「今走っていいか」を確認するための関数
    /// </summary>
    public bool IsRacing()
    {
        return isRacing;
    }

    // --- 内部処理 ---

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 分:秒.ミリ秒 の形式で表示
            float minutes = Mathf.FloorToInt(currentTime / 60F);
            float seconds = Mathf.FloorToInt(currentTime % 60F);
            float milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);
            timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }
}