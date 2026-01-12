using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CourseProgressor))]
public class NPCBrain : MonoBehaviour
{
    [Header("AI設定")]
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float thinkInterval = 0.5f;
    [Header("活性化設定")]
    [SerializeField] private float forceChangeInterval = 10.0f;
    private float lastChangeTime; 

    private List<GameObject> randomDeck = new List<GameObject>();
    private const int MAX_RANDOM_SLOTS = 5;
    private GameObject oshoPiecePrefab;
    private CourseProgressor courseProgressor;
    private GameObject currentPieceInstance;
    private PlayerPieceData currentPieceData;
    
    // 優先度リスト
    private readonly List<PieceType> straightBest = new List<PieceType> { PieceType.Hisha, PieceType.Kyosha };
    private readonly List<PieceType> straightBetter = new List<PieceType> { PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho, PieceType.Kakugyo };
    private readonly List<PieceType> curveBest = new List<PieceType> { PieceType.Kakugyo };
    private readonly List<PieceType> curveBetter = new List<PieceType> { PieceType.Osho, PieceType.Kinsho, PieceType.Ginsho, PieceType.Keima };
    private readonly List<PieceType> scurveBest = new List<PieceType> { PieceType.Keima, PieceType.Kakugyo };
    private readonly List<PieceType> scurveBetter = new List<PieceType> { PieceType.Osho, PieceType.Hisha, PieceType.Kinsho, PieceType.Ginsho };

    void Awake() { courseProgressor = GetComponent<CourseProgressor>(); }
    void Start() { InitializeDeck(); lastChangeTime = Time.time; StartCoroutine(StartRoutine()); }

    private void InitializeDeck()
    {
        if (NPCManager.Instance == null) return;
        oshoPiecePrefab = NPCManager.Instance.GetOshoPiece();
        while (randomDeck.Count < MAX_RANDOM_SLOTS)
        {
            GameObject newPiece = NPCManager.Instance.GetRandomPiece();
            if (newPiece != null) randomDeck.Add(newPiece); else break;
        }
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        if(randomDeck.Count > 0) UseRandomPieceAtIndex(0);
        StartCoroutine(ThinkRoutine());
    }

    private IEnumerator ThinkRoutine()
    {
        while (true)
        {
            try
            {
                if (courseProgressor != null && currentPieceData != null)
                {
                    // 10秒強制変更ロジック
                    if (Time.time - lastChangeTime >= forceChangeInterval)
                    {
                        ForceRandomChange();
                    }
                    else
                    {
                        // 地形に応じた変更
                        TerrainType currentTerrain = courseProgressor.GetCurrentTerrainType();
                        DecideBestPiece(currentTerrain, currentPieceData.pieceType);
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning($"NPC思考エラー: {e.Message}"); }
            yield return new WaitForSeconds(thinkInterval);
        }
    }

    private void ForceRandomChange()
    {
        if (randomDeck.Count > 0)
        {
            int randomIndex = Random.Range(0, randomDeck.Count);
            UseRandomPieceAtIndex(randomIndex);
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
                bestList = straightBest; betterList = straightBetter; break;
            case TerrainType.BigCurve:
            case TerrainType.Normal:
                bestList = curveBest; betterList = curveBetter; break;
            case TerrainType.SCurve:
            case TerrainType.Obstacle:
            case TerrainType.Narrow:
                bestList = scurveBest; betterList = scurveBetter; break;
        }

        if (bestList == null) return;

        if (!bestList.Contains(currentKoma))
        {
            if (TrySwitchPiece(bestList)) return; 
            if (!betterList.Contains(currentKoma)) TrySwitchPiece(betterList);
        }
    }

    private bool TrySwitchPiece(List<PieceType> priorityList)
    {
        if (NPCManager.Instance == null) return false;
        foreach (PieceType desiredType in priorityList)
        {
            if (currentPieceData != null && currentPieceData.pieceType == desiredType) return true;
            if (desiredType == PieceType.Osho && oshoPiecePrefab != null)
            {
                var oshoData = oshoPiecePrefab.GetComponent<PlayerPieceData>();
                if (oshoData != null && oshoData.pieceType == PieceType.Osho) { UseOshoPiece(); return true; }
            }
            for (int i = 0; i < randomDeck.Count; i++)
            {
                if (randomDeck[i] == null) continue; 
                PlayerPieceData data = randomDeck[i].GetComponent<PlayerPieceData>();
                if (data != null && data.pieceType == desiredType) { UseRandomPieceAtIndex(i); return true; }
            }
        }
        return false;
    }

    private void UseRandomPieceAtIndex(int index)
    {
        if (index < 0 || index >= randomDeck.Count) return;
        SpawnPiece(randomDeck[index]);
        randomDeck.RemoveAt(index);
        GameObject newPiece = NPCManager.Instance.GetRandomPiece();
        if (newPiece != null) randomDeck.Insert(index, newPiece);
    }

    private void UseOshoPiece() { if (oshoPiecePrefab != null) SpawnPiece(oshoPiecePrefab); }

    private void SpawnPiece(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null) return;
        lastChangeTime = Time.time; // タイマーリセット

        Vector3 pPos = Vector3.zero; Quaternion pRot = Quaternion.identity; bool found = false;
        foreach (Transform c in transform) {
            if (c.gameObject.activeSelf && c.GetComponent<PlayerPieceData>()) { pPos = c.localPosition; pRot = c.localRotation; found = true; break; }
        }
        foreach (Transform c in transform) if (c.GetComponent<PlayerPieceData>()) Destroy(c.gameObject);

        currentPieceInstance = Instantiate(prefabToSpawn, transform);
        if (found) { currentPieceInstance.transform.localPosition = pPos; currentPieceInstance.transform.localRotation = pRot; }
        else { currentPieceInstance.transform.localPosition = Vector3.zero; currentPieceInstance.transform.localRotation = Quaternion.identity; }

        currentPieceData = currentPieceInstance.GetComponent<PlayerPieceData>();
        if (courseProgressor != null && currentPieceData != null) courseProgressor.RefreshPieceData(currentPieceData);
    }
    
    public void LoseCurrentPiece()
    {
        if (randomDeck.Count > 0) UseRandomPieceAtIndex(Random.Range(0, randomDeck.Count));
    }
}