using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCBrain : MonoBehaviour
{
    [Header("AI索敵設定")]
    [SerializeField] private float detectionRange = 10f;

    private List<GameObject> randomDeck = new List<GameObject>();
    private const int MAX_RANDOM_SLOTS = 5;
    private CourseProgressor courseProgressor;
    private PlayerPieceData currentPieceData;

    private readonly List<PieceType> straightBest = new List<PieceType> { PieceType.Hisha, PieceType.Kyosha };
    private readonly List<PieceType> curveBest = new List<PieceType> { PieceType.Kakugyo };

    void Awake() { courseProgressor = GetComponent<CourseProgressor>(); }
    void Start() { InitializeDeck(); StartCoroutine(ThinkRoutine()); }

    private void InitializeDeck() {
        if (NPCManager.Instance == null) return;
        while (randomDeck.Count < MAX_RANDOM_SLOTS) {
            GameObject p = NPCManager.Instance.GetRandomPiece();
            if (p != null) randomDeck.Add(p);
        }
        if (randomDeck.Count > 0) UseRandomPieceAtIndex(0);
    }

    private IEnumerator ThinkRoutine() {
        while (true) {
            if (courseProgressor != null && currentPieceData != null) {
                DecideBestPiece(courseProgressor.GetCurrentTerrainType(), currentPieceData.pieceType);
                SteerBasedOnOthers();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SteerBasedOnOthers() {
        CourseProgressor targetToAttack = null;
        CourseProgressor threatToAvoid = null;
        float minDistance = float.MaxValue;

        foreach (var other in CourseProgressor.AllProgressors) {
            if (other == courseProgressor) continue;

            float distZ = Vector3.Dot(other.transform.position - transform.position, transform.forward);
            
            // 前方の遅い走者をターゲットに設定
            if (distZ > 0 && distZ < detectionRange && other.currentMoveSpeed < courseProgressor.currentMoveSpeed) {
                if (distZ < minDistance) { minDistance = distZ; targetToAttack = other; }
            }
            // 後方の速い走者を脅威に設定
            else if (distZ < 0 && distZ > -detectionRange && other.currentMoveSpeed > courseProgressor.currentMoveSpeed) {
                threatToAvoid = other;
            }
        }

        if (targetToAttack != null) {
            // 攻撃：相手の横位置に寄せる
            courseProgressor.targetHorizontalOffset = targetToAttack.targetHorizontalOffset;
        }
        else if (threatToAvoid != null) {
            // 回避：相手の横位置から離れる
            float escapeDir = (threatToAvoid.targetHorizontalOffset >= 0) ? -1.5f : 1.5f;
            courseProgressor.targetHorizontalOffset = escapeDir;
        }
        else {
            // 索敵対象がいない場合は中央に戻る
            courseProgressor.targetHorizontalOffset = Mathf.Lerp(courseProgressor.targetHorizontalOffset, 0, 0.05f);
        }
    }

    private void DecideBestPiece(TerrainType t, PieceType curr) {
        List<PieceType> best = (t == TerrainType.Straight) ? straightBest : curveBest;
        if (!best.Contains(curr)) TrySwitchPiece(best);
    }

    private bool TrySwitchPiece(List<PieceType> prio) {
        foreach (PieceType p in prio) {
            for (int i = 0; i < randomDeck.Count; i++) {
                if (randomDeck[i].GetComponent<PlayerPieceData>().pieceType == p) { UseRandomPieceAtIndex(i); return true; }
            }
        }
        return false;
    }

    private void UseRandomPieceAtIndex(int i) {
        if (i < 0 || i >= randomDeck.Count) return;
        SpawnPiece(randomDeck[i]);
        randomDeck[i] = NPCManager.Instance.GetRandomPiece();
    }

    private void SpawnPiece(GameObject prefab) {
        foreach (Transform c in transform) { if (c.GetComponent<PlayerPieceData>()!=null) Destroy(c.gameObject); }
        GameObject n = Instantiate(prefab, transform);
        currentPieceData = n.GetComponent<PlayerPieceData>();
        if (courseProgressor != null) courseProgressor.RefreshPieceData(currentPieceData);
    }

    public void LoseCurrentPiece() {
        if (randomDeck.Count > 1) {
            randomDeck.RemoveAt(randomDeck.Count - 1);
            UseRandomPieceAtIndex(0);
        }
    }
}