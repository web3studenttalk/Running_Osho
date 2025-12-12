// CourseGenerator.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [Header("コース構成部品")]
    [SerializeField] private GameObject startPiecePrefab;
    [SerializeField] private List<WeightedTrackPiece> middlePiecePrefabs;
    [SerializeField] private GameObject endPiecePrefab;

    [Header("コース設定")]
    [SerializeField] private int middlePiecesCount = 20;

    [Header("参加者設定")]
    [Tooltip("シーン上のプレイヤー")]
    [SerializeField] private CourseProgressor playerRig; 
    
    // --- ▼ 追加：NPCのリスト ▼ ---
    [Tooltip("シーン上のNPCたち（4人を登録）")]
    [SerializeField] private List<CourseProgressor> npcRigs; 
    // --- ▲ ここまで ▲ ---

    private Transform lastEndPoint;
    
    void Start()
    {
        GenerateCourse();
    }

    private void GenerateCourse()
    {
        // 既存のコース削除
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        if (startPiecePrefab == null) return;
        
        List<Transform> fullPath = new List<Transform>();

        // 1. スタート
        GameObject startPiece = Instantiate(startPiecePrefab, transform.position, transform.rotation, this.transform);
        AddPathPointsFromPiece(startPiece.GetComponent<TrackPiece>(), fullPath, true); 
        lastEndPoint = startPiece.GetComponent<TrackPiece>().endPoint;

        GameObject lastUsedPrefab = null;

        // 2. 中間
        for (int i = 0; i < middlePiecesCount; i++)
        {
            List<WeightedTrackPiece> availablePieces = middlePiecePrefabs
                .Where(p => p.piecePrefab != lastUsedPrefab && p.weight > 0).ToList();

            if (availablePieces.Count == 0) availablePieces = middlePiecePrefabs;
            
            GameObject nextPrefab = GetRandomWeightedPiece(availablePieces);
            if (nextPrefab != null)
            {
                PlacePiece(nextPrefab, fullPath); 
                lastUsedPrefab = nextPrefab;
            }
        }

        // 3. ゴール
        PlacePiece(endPiecePrefab, fullPath);

        // --- ▼ 修正：全員にコース情報を渡す ▼ ---
        
        // プレイヤーへの適用
        if (playerRig != null)
        {
            SetParticipantCourse(playerRig, fullPath);
        }

        // NPCたちへの適用
        if (npcRigs != null)
        {
            foreach (var npc in npcRigs)
            {
                if (npc != null)
                {
                    SetParticipantCourse(npc, fullPath);
                }
            }
        }
        // --- ▲ ここまで ▲ ---
    }

    // 参加者をスタート地点に移動させ、コース情報を渡す共通処理
    private void SetParticipantCourse(CourseProgressor participant, List<Transform> path)
    {
        // 位置は初期位置（X,Y）を維持しつつ、Z（奥行き）と回転だけスタート地点に合わせるのが理想ですが、
        // 今回は単純にスタート地点へ移動させます。
        // ※後でNPCのスタート位置（横並び）を調整します
        
        // コース情報を渡す
        participant.SetCourse(path);
    }
    
    private void PlacePiece(GameObject piecePrefab, List<Transform> pathList) 
    {
        GameObject newPiece = Instantiate(piecePrefab, this.transform);
        TrackPiece trackPiece = newPiece.GetComponent<TrackPiece>();
        
        if (trackPiece == null) return;

        Transform startPoint = trackPiece.startPoint;
        Quaternion rotationDifference = lastEndPoint.rotation * Quaternion.Inverse(startPoint.rotation);
        newPiece.transform.rotation = rotationDifference;
        Vector3 positionOffset = lastEndPoint.position - startPoint.position;
        newPiece.transform.position += positionOffset;
        
        AddPathPointsFromPiece(trackPiece, pathList, false); 
        lastEndPoint = trackPiece.endPoint;
    }
    
    private void AddPathPointsFromPiece(TrackPiece piece, List<Transform> pathList, bool includeStartPoint)
    {
        if (includeStartPoint) pathList.Add(piece.startPoint);
        if (piece.waypoints != null) pathList.AddRange(piece.waypoints);
        pathList.Add(piece.endPoint);
    }
    
    private GameObject GetRandomWeightedPiece(List<WeightedTrackPiece> pieces)
    {
        int totalWeight = pieces.Sum(p => p.weight);
        int randomPoint = Random.Range(0, totalWeight);
        foreach (var piece in pieces)
        {
            if (randomPoint < piece.weight) return piece.piecePrefab;
            randomPoint -= piece.weight;
        }
        return pieces.FirstOrDefault()?.piecePrefab;
    }
}