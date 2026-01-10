// NPCBrain.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CourseProgressor))]
public class NPCBrain : MonoBehaviour
{
    [Header("AI設定")]
    [Tooltip("スタート時の待機時間")]
    [SerializeField] private float startDelay = 0.5f;
    [Tooltip("思考の間隔（秒）：小さいほど頻繁に駒を変えます")]
    [SerializeField] private float thinkInterval = 0.5f;

    // --- デッキ管理 ---
    private List<GameObject> randomDeck = new List<GameObject>();
    private const int MAX_RANDOM_SLOTS = 5;
    private GameObject oshoPiecePrefab; // 王将枠

    private CourseProgressor courseProgressor;
    private GameObject currentPieceInstance;
    private PlayerPieceData currentPieceData;
    private TerrainType lastCheckedTerrain = TerrainType.Normal;

    // --- 優先度リスト ---
    private readonly List<PieceType> straightBest = new List<PieceType> { PieceType.Hisha, PieceType.Kyosha };
    private readonly List<PieceType> straightBetter = new List<PieceType> { PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho, PieceType.Kakugyo };

    private readonly List<PieceType> curveBest = new List<PieceType> { PieceType.Kakugyo };
    private readonly List<PieceType> curveBetter = new List<PieceType> { PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho, PieceType.Keima };

    private readonly List<PieceType> scurveBest = new List<PieceType> { PieceType.Keima, PieceType.Kakugyo };
    private readonly List<PieceType> scurveBetter = new List<PieceType> { PieceType.Osho, PieceType.Hisha, PieceType.Kinsho, PieceType.Ginsho };


    void Awake()
    {
        courseProgressor = GetComponent<CourseProgressor>();
    }

    void Start()
    {
        InitializeDeck();
        StartCoroutine(StartRoutine());
    }

    private void InitializeDeck()
    {
        if (NPCManager.Instance == null) return;
        
        // 王将の取得
        oshoPiecePrefab = NPCManager.Instance.GetOshoPiece();
        
        // ランダム枠の補充
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
        if(randomDeck.Count > 0) UseRandomPieceAtIndex(0);
        
        // 思考ルーチン開始
        StartCoroutine(ThinkRoutine());
    }

    // --- エラー対策版：思考ルーチン ---
    private IEnumerator ThinkRoutine()
    {
        while (true)
        {
            // エラーが起きてもループを止めないように try-catch で囲む
            try
            {
                if (courseProgressor != null && currentPieceData != null)
                {
                    TerrainType currentTerrain = courseProgressor.GetCurrentTerrainType();
                    
                    // 地形と手持ちを見て最適な行動をとる
                    DecideBestPiece(currentTerrain, currentPieceData.pieceType);
                    
                    lastCheckedTerrain = currentTerrain;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"NPC思考中にエラー発生（復帰します）: {e.Message}");
            }

            yield return new WaitForSeconds(thinkInterval);
        }
    }

    private void DecideBestPiece(TerrainType terrain, PieceType currentKoma)
    {
        List<PieceType> bestList = null;
        List<PieceType> betterList = null;

        switch (terrain)
        {
            case TerrainType.Straight:
            case TerrainType.SlopeUp:
            case TerrainType.SlopeDown:
                bestList = straightBest;
                betterList = straightBetter;
                break;

            case TerrainType.BigCurve:
            case TerrainType.Normal:
                bestList = curveBest;
                betterList = curveBetter;
                break;

            case TerrainType.SCurve:
            case TerrainType.Obstacle:
            case TerrainType.Narrow:
                bestList = scurveBest;
                betterList = scurveBetter;
                break;
        }

        if (bestList == null) return;

        // 今の駒が最強リストに入っていないなら交代を検討
        bool isUsingBest = bestList.Contains(currentKoma);

        if (!isUsingBest)
        {
            // 最強の駒を持っていれば即交代
            if (TrySwitchPiece(bestList)) return; 
            
            // 持っていなくて、今の駒が「妥協リスト」にも入っていないなら、妥協リスト内の駒へ交代
            bool isUsingBetter = betterList.Contains(currentKoma);
            if (!isUsingBetter)
            {
                TrySwitchPiece(betterList);
            }
        }
    }

    private bool TrySwitchPiece(List<PieceType> priorityList)
    {
        if (NPCManager.Instance == null) return false;

        foreach (PieceType desiredType in priorityList)
        {
            if (currentPieceData != null && currentPieceData.pieceType == desiredType) return true;

            // 1. 王将チェック
            if (desiredType == PieceType.Osho && oshoPiecePrefab != null)
            {
                // 王将プレハブ自体が消えている可能性ケア
                if (oshoPiecePrefab == null) continue;

                var oshoData = oshoPiecePrefab.GetComponent<PlayerPieceData>();
                if (oshoData != null && oshoData.pieceType == PieceType.Osho)
                {
                    UseOshoPiece();
                    return true;
                }
            }

            // 2. ランダム枠チェック
            for (int i = 0; i < randomDeck.Count; i++)
            {
                if (randomDeck[i] == null) continue; // 空データケア

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

    private void UseRandomPieceAtIndex(int index)
    {
        if (index < 0 || index >= randomDeck.Count) return;

        GameObject prefabToSpawn = randomDeck[index];
        SpawnPiece(prefabToSpawn);

        // 消費と補充
        randomDeck.RemoveAt(index);
        GameObject newPiece = NPCManager.Instance.GetRandomPiece();
        
        // 補充できた場合のみ追加
        if (newPiece != null) 
        {
            randomDeck.Insert(index, newPiece);
        }
        else
        {
            // 万が一補充できなかったら、穴埋めとして既存のprefabを入れるなどの対策も可能だが
            // ここではリストが減ったままにする（エラー回避優先）
        }
    }

    private void UseOshoPiece()
    {
        if (oshoPiecePrefab != null)
        {
            SpawnPiece(oshoPiecePrefab);
        }
    }

    // --- ▼▼▼ 修正箇所：安全な削除と生成 ▼▼▼ ---
    private void SpawnPiece(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null) return;

        Vector3 previousLocalPos = Vector3.zero;
        Quaternion previousLocalRot = Quaternion.identity;
        bool foundActive = false;

        // 1. 古い駒（Activeなもの）を探して位置を記憶
        //    ただし「PlayerPieceData」を持っているものだけを対象にする！
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf && child.GetComponent<PlayerPieceData>() != null)
            {
                previousLocalPos = child.localPosition;
                previousLocalRot = child.localRotation;
                foundActive = true;
                break;
            }
        }

        // 2. 古い駒だけを削除（無差別削除をやめる）
        //    リストに入れてから消す（ループ中の削除エラー防止）
        List<GameObject> objectsDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            // PlayerPieceDataがついているものだけを消す対象にする
            if (child.GetComponent<PlayerPieceData>() != null)
            {
                objectsDestroy.Add(child.gameObject);
            }
        }
        
        // 実際に削除
        foreach (GameObject obj in objectsDestroy)
        {
            obj.SetActive(false);
            Destroy(obj);
        }

        // 3. 新しい駒を生成
        currentPieceInstance = Instantiate(prefabToSpawn, transform);

        // 4. 位置の復元
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

        // 5. データの更新
        currentPieceData = currentPieceInstance.GetComponent<PlayerPieceData>();
        if (courseProgressor != null && currentPieceData != null)
        {
            courseProgressor.RefreshPieceData(currentPieceData);
        }
    }
}