// NPCBrain.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CourseProgressor))]
public class NPCBrain : MonoBehaviour
{
    [Header("AI設定")]
    [Tooltip("スタート時の待機時間")]
    [SerializeField] private float startDelay = 1.0f;
    [Tooltip("思考間隔（秒）")]
    [SerializeField] private float thinkInterval = 1.0f;

    // --- デッキ管理 ---
    // ランダム枠（5つ）
    private List<GameObject> randomDeck = new List<GameObject>();
    private const int MAX_RANDOM_SLOTS = 5;

    // 王将枠（固定・消費しない）
    private GameObject oshoPiecePrefab;

    private CourseProgressor courseProgressor;
    private GameObject currentPieceInstance;
    private PlayerPieceData currentPieceData;
    private TerrainType lastCheckedTerrain = TerrainType.Normal;

    // ========================================================================
    //  ★優先度リスト（パラメータ表完全準拠版）★
    // ========================================================================

    // ■ 直進・上り坂・下り坂 (Straight, Slope)
    // 最強(2.0): 飛車, 香車
    private readonly List<PieceType> straight1 = new List<PieceType> 
    { 
        PieceType.Hisha, PieceType.Kyosha 
    };
    // 妥協(1.5): 王将
    // ※表によると金・銀・角(1.0)は遅いので、ここには入れません。
    private readonly List<PieceType> straight2 = new List<PieceType> 
    { 
        PieceType.Hisha, PieceType.Kyosha, 
        PieceType.Osho 
    };
    
    // ■ カーブ（大カーブ） (Curve, BigCurve)
    // 最強(2.0): 角行
    private readonly List<PieceType> curve1 = new List<PieceType> 
    { 
        PieceType.Kakugyo 
    };
    // 妥協(1.5): 王将, 金将, 銀将
    // ※表によると飛車(1.0)は曲がれないので除外。香車(0.5)も除外。
    private readonly List<PieceType> curve2 = new List<PieceType> 
    { 
        PieceType.Kakugyo, 
        PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho 
    };

    // ■ S字・障害物・狭窄 (S-Curve, Obstacle, Narrow)
    // 最強(2.0): 桂馬, 角行
    private readonly List<PieceType> scurve1 = new List<PieceType> 
    { 
        PieceType.Keima, PieceType.Kakugyo 
    };
    // 妥協(1.5): 王将, 金将, 飛車
    // ※表によると飛車はS字/障害物で1.5あるので採用。銀将(1.0)は除外。
    private readonly List<PieceType> scurve2 = new List<PieceType> 
    { 
        PieceType.Keima, PieceType.Kakugyo, 
        PieceType.Osho, PieceType.Kinsho, PieceType.Hisha 
    };

    // ========================================================================


    void Awake()
    {
        courseProgressor = GetComponent<CourseProgressor>();
    }

    void Start()
    {
        InitializeDeck();
        StartCoroutine(StartRoutine());
    }

    // デッキの初期化（ランダム5つ + 王将）
    private void InitializeDeck()
    {
        if (NPCManager.Instance == null) return;

        // 1. 王将を受け取る
        oshoPiecePrefab = NPCManager.Instance.GetOshoPiece();

        // 2. ランダム枠を5つ埋める
        while (randomDeck.Count < MAX_RANDOM_SLOTS)
        {
            GameObject newPiece = NPCManager.Instance.GetRandomPiece();
            if (newPiece != null) randomDeck.Add(newPiece);
            else break;
        }
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        
        // 最初はランダム枠の0番目を使ってスタート
        if(randomDeck.Count > 0)
        {
            UseRandomPieceAtIndex(0);
        }

        StartCoroutine(ThinkRoutine());
    }

    private IEnumerator ThinkRoutine()
    {
        while (true)
        {
            if (courseProgressor != null && currentPieceData != null)
            {
                TerrainType currentTerrain = courseProgressor.GetCurrentTerrainType();
                CheckSituationAndSwitch(currentTerrain, currentPieceData.pieceType);
                lastCheckedTerrain = currentTerrain;
            }
            yield return new WaitForSeconds(thinkInterval);
        }
    }

    // 地形と今のコマを見て、変えるべきか判断する
    private void CheckSituationAndSwitch(TerrainType terrain, PieceType currentKoma)
    {
        switch (terrain)
        {
            // --- 直進エリア ---
            case TerrainType.Straight:
            case TerrainType.SlopeUp:
            case TerrainType.SlopeDown:
                // 現在がスコア1.0以下のコマ（金,銀,角,桂,歩）なら、1.5以上のコマに変える
                if (IsOneOf(currentKoma, PieceType.Kinsho, PieceType.Ginsho, PieceType.Kakugyo, PieceType.Keima, PieceType.Fuhyo))
                {
                    // まず最強(2.0)を探し、なければ妥協(1.5)を探す
                    if (!TrySwitchPiece(straight1)) TrySwitchPiece(straight2);
                }
                // もし今が王将(1.5)なら、最強(2.0)があれば変えたい
                else if (currentKoma == PieceType.Osho)
                {
                    TrySwitchPiece(straight1);
                }
                break;

            // --- カーブエリア ---
            case TerrainType.BigCurve:
            case TerrainType.Normal:
                // 現在がスコア1.0以下のコマ（飛,桂,香,歩）なら、1.5以上のコマに変える
                if (IsOneOf(currentKoma, PieceType.Hisha, PieceType.Keima, PieceType.Kyosha, PieceType.Fuhyo))
                {
                    if (!TrySwitchPiece(curve1)) TrySwitchPiece(curve2);
                }
                break;

            // --- S字・障害物エリア ---
            case TerrainType.SCurve:
            case TerrainType.Obstacle:
            case TerrainType.Narrow:
                // 現在がスコア1.0以下のコマ（銀,香,歩）なら、1.5以上のコマに変える
                if (IsOneOf(currentKoma, PieceType.Ginsho, PieceType.Kyosha, PieceType.Fuhyo))
                {
                    if (!TrySwitchPiece(scurve1)) TrySwitchPiece(scurve2);
                }
                // 今が1.5グループ（王,金,飛）なら、最強(2.0)があれば変えたい
                else if (IsOneOf(currentKoma, PieceType.Osho, PieceType.Kinsho, PieceType.Hisha))
                {
                    TrySwitchPiece(scurve1);
                }
                break;
        }
    }

    private bool IsOneOf(PieceType target, params PieceType[] types)
    {
        foreach (var t in types) if (target == t) return true;
        return false;
    }

    // 優先度リストに従って、手持ち（王将 or ランダム枠）から探して交代する
    private bool TrySwitchPiece(List<PieceType> priorityList)
    {
        if (NPCManager.Instance == null) return false;

        foreach (PieceType desiredType in priorityList)
        {
            // すでにその駒なら何もしない
            if (currentPieceData != null && currentPieceData.pieceType == desiredType) return true;

            // --- A. 王将チェック (固定枠) ---
            if (desiredType == PieceType.Osho && oshoPiecePrefab != null)
            {
                var oshoData = oshoPiecePrefab.GetComponent<PlayerPieceData>();
                if (oshoData != null && oshoData.pieceType == PieceType.Osho)
                {
                    UseOshoPiece();
                    return true;
                }
            }

            // --- B. ランダム枠チェック (5枠) ---
            for (int i = 0; i < randomDeck.Count; i++)
            {
                PlayerPieceData data = randomDeck[i].GetComponent<PlayerPieceData>();
                if (data != null && data.pieceType == desiredType)
                {
                    UseRandomPieceAtIndex(i);
                    return true;
                }
            }
        }
        return false;
    }

    // ランダム枠の駒を使う（消費して補充）
    private void UseRandomPieceAtIndex(int index)
    {
        if (index < 0 || index >= randomDeck.Count) return;

        GameObject prefabToSpawn = randomDeck[index];
        SpawnPiece(prefabToSpawn);

        randomDeck.RemoveAt(index);
        GameObject newPiece = NPCManager.Instance.GetRandomPiece();
        if (newPiece != null) randomDeck.Insert(index, newPiece);
    }

    // 王将を使う（消費しない）
    private void UseOshoPiece()
    {
        if (oshoPiecePrefab != null)
        {
            SpawnPiece(oshoPiecePrefab);
        }
    }

    // 生成処理（位置引継ぎ）
    private void SpawnPiece(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null) return;

        Vector3 previousLocalPos = Vector3.zero;
        Quaternion previousLocalRot = Quaternion.identity;
        bool foundActive = false;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                previousLocalPos = child.localPosition;
                previousLocalRot = child.localRotation;
                foundActive = true;
                break;
            }
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        currentPieceInstance = Instantiate(prefabToSpawn, transform);

        if (foundActive)
        {
            currentPieceInstance.transform.localPosition = previousLocalPos;
            currentPieceInstance.transform.localRotation = previousLocalRot;
        }
        else
        {
            currentPieceInstance.transform.localPosition = Vector3.zero;
            currentPieceInstance.transform.localRotation = Quaternion.identity;
        }

        currentPieceData = currentPieceInstance.GetComponent<PlayerPieceData>();
        if (courseProgressor != null && currentPieceData != null)
        {
            courseProgressor.RefreshPieceData(currentPieceData);
        }
    }
}