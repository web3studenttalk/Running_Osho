// NPCBrain.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CourseProgressor))]
public class NPCBrain : MonoBehaviour
{
    [Header("AI設定")]
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private float thinkInterval = 1.0f;

    // --- デッキ管理 ---
    // ランダム枠（5つ）に変更
    private List<GameObject> randomDeck = new List<GameObject>();
    private const int MAX_RANDOM_SLOTS = 5; // ← ここを4から5に変更しました

    // 王将枠（固定・消費しない）
    private GameObject oshoPiecePrefab;

    private CourseProgressor courseProgressor;
    private GameObject currentPieceInstance;
    private PlayerPieceData currentPieceData;
    private TerrainType lastCheckedTerrain = TerrainType.Normal;

    // --- 優先度リスト ---
    private readonly List<PieceType> straight1 = new List<PieceType> { PieceType.Hisha, PieceType.Kyosha };
    // 直線妥協リストに王将を含める
    private readonly List<PieceType> straight2 = new List<PieceType> { PieceType.Hisha, PieceType.Kyosha, PieceType.Kinsho, PieceType.Ginsho, PieceType.Kakugyo, PieceType.Osho };
    
    private readonly List<PieceType> curve1 = new List<PieceType> { PieceType.Kakugyo };
    // カーブ妥協リストに王将を含める
    private readonly List<PieceType> curve2 = new List<PieceType> { PieceType.Kakugyo, PieceType.Kinsho, PieceType.Ginsho, PieceType.Hisha, PieceType.Osho };

    private readonly List<PieceType> scurve1 = new List<PieceType> { PieceType.Keima, PieceType.Kakugyo };
    // S字妥協リストに王将を含める
    private readonly List<PieceType> scurve2 = new List<PieceType> { PieceType.Keima, PieceType.Kakugyo, PieceType.Kinsho, PieceType.Ginsho, PieceType.Hisha, PieceType.Osho };


    void Awake()
    {
        courseProgressor = GetComponent<CourseProgressor>();
    }

    void Start()
    {
        // デッキ初期化
        InitializeDeck();
        
        StartCoroutine(StartRoutine());
    }

    /// <summary>
    /// デッキの初期化（ランダム5つ + 王将）
    /// </summary>
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

    private void CheckSituationAndSwitch(TerrainType terrain, PieceType currentKoma)
    {
        switch (terrain)
        {
            case TerrainType.Straight:
            case TerrainType.SlopeUp:
            case TerrainType.SlopeDown:
                if (IsOneOf(currentKoma, PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho, PieceType.Kakugyo, PieceType.Fuhyo, PieceType.Keima))
                {
                    if (!TrySwitchPiece(straight1)) TrySwitchPiece(straight2);
                }
                break;

            case TerrainType.BigCurve:
            case TerrainType.Normal:
                if (IsOneOf(currentKoma, PieceType.Kyosha, PieceType.Hisha, PieceType.Fuhyo, PieceType.Osho))
                {
                    if (!TrySwitchPiece(curve1)) TrySwitchPiece(curve2);
                }
                break;

            case TerrainType.SCurve:
            case TerrainType.Obstacle:
            case TerrainType.Narrow:
                if (IsOneOf(currentKoma, PieceType.Hisha, PieceType.Kyosha, PieceType.Osho, PieceType.Fuhyo))
                {
                    if (!TrySwitchPiece(scurve1)) TrySwitchPiece(scurve2);
                }
                break;
        }
    }

    private bool IsOneOf(PieceType target, params PieceType[] types)
    {
        foreach (var t in types) if (target == t) return true;
        return false;
    }

    /// <summary>
    /// 優先度リストに従って、手持ち（ランダム枠 or 王将）から探して交代する
    /// </summary>
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

            // --- B. ランダム枠チェック (5枠から検索) ---
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

    /// <summary>
    /// ランダム枠の駒を使う（消費して補充する）
    /// </summary>
    private void UseRandomPieceAtIndex(int index)
    {
        if (index < 0 || index >= randomDeck.Count) return;

        GameObject prefabToSpawn = randomDeck[index];
        SpawnPiece(prefabToSpawn);

        // 消費＆補充
        randomDeck.RemoveAt(index);
        GameObject newPiece = NPCManager.Instance.GetRandomPiece();
        if (newPiece != null) randomDeck.Insert(index, newPiece);
    }

    /// <summary>
    /// 王将を使う（消費しない）
    /// </summary>
    private void UseOshoPiece()
    {
        if (oshoPiecePrefab != null)
        {
            SpawnPiece(oshoPiecePrefab);
            // 補充処理はしない（何度でも使える）
        }
    }

    // 実際の生成処理
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