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

    [Header("プレイヤーオブジェクト")]
    [SerializeField] private GameObject followerPrefab;

    private Transform lastEndPoint;
    
    // --- ▼ List<Transform> の生成ロジックを変更 ▼ ---

    void Start()
    {
        GenerateCourse();
    }

    private void GenerateCourse()
    {
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        if (startPiecePrefab == null || endPiecePrefab == null || middlePiecePrefabs == null || middlePiecePrefabs.Count == 0)
        {
            Debug.LogError("コース部品のプレハブがインスペクターで設定されていません。");
            return;
        }
        
        // 最終的にプレイヤーに渡す、全ての通過点を格納するリスト
        List<Transform> fullPath = new List<Transform>();

        // 1. スタート部品を配置し、パス情報を取得
        GameObject startPiece = Instantiate(startPiecePrefab, transform.position, transform.rotation, this.transform);
        AddPathPointsFromPiece(startPiece.GetComponent<TrackPiece>(), fullPath, true); // trueで始点も追加
        lastEndPoint = startPiece.GetComponent<TrackPiece>().endPoint;

        GameObject lastUsedPrefab = null;

        // 2. 中間部品を配置し、パス情報を取得
        for (int i = 0; i < middlePiecesCount; i++)
        {
            List<WeightedTrackPiece> availablePieces = middlePiecePrefabs.Where(p => p.piecePrefab != lastUsedPrefab).ToList();
            if (availablePieces.Count == 0) { availablePieces = middlePiecePrefabs; }

            GameObject nextPrefab = GetRandomWeightedPiece(availablePieces);
            PlacePiece(nextPrefab, fullPath); // fullPathを渡す
            lastUsedPrefab = nextPrefab;
        }

        // 3. ゴール部品を配置し、パス情報を取得
        PlacePiece(endPiecePrefab, fullPath);

        // デバッグ用のパス可視化
        for (int i = 0; i < fullPath.Count - 1; i++)
        {
            Debug.DrawLine(fullPath[i].position, fullPath[i + 1].position, Color.red, 20f);
        }

        // 4. プレイヤーを生成し、完成したパス情報を渡す
        if (followerPrefab != null)
        {
            Vector3 spawnPos = fullPath[0].position;
            Quaternion spawnRot = fullPath[0].rotation;
            GameObject playerRig = Instantiate(followerPrefab, spawnPos, spawnRot, this.transform);
            
            CourseProgressor courseProgressor = playerRig.GetComponent<CourseProgressor>(); 
            if (courseProgressor != null)
            {
                courseProgressor.SetCourse(fullPath);
            }
            else
            {
                Debug.LogError("プレイヤープレハブの親オブジェクトに CourseProgressor スクリプトがアタッチされていません！");
            }
        }
    }
    
    // --- ▲ ここまで変更 ▲ ---

    private void PlacePiece(GameObject piecePrefab, List<Transform> pathList) // pathListを受け取る
    {
        GameObject newPiece = Instantiate(piecePrefab, this.transform);
        TrackPiece trackPiece = newPiece.GetComponent<TrackPiece>();
        
        if (trackPiece == null || trackPiece.startPoint == null || trackPiece.endPoint == null)
        {
            Debug.LogError($"配置しようとしたプレハブ '{piecePrefab.name}' に TrackPieceスクリプト、またはStart/End Pointが設定されていません。", piecePrefab);
            Destroy(newPiece);
            return;
        }

        Transform startPoint = trackPiece.startPoint;

        Quaternion rotationDifference = lastEndPoint.rotation * Quaternion.Inverse(startPoint.rotation);
        newPiece.transform.rotation = rotationDifference;

        Vector3 positionOffset = lastEndPoint.position - startPoint.position;
        newPiece.transform.position += positionOffset;
        
        // 配置した部品からパス情報を取得してリストに追加
        AddPathPointsFromPiece(trackPiece, pathList, false); // falseで始点は追加しない

        lastEndPoint = trackPiece.endPoint;
    }
    
    /// <summary>
    /// 一つのコース部品からパス情報を抽出し、総合パスリストに追加する
    /// </summary>
    private void AddPathPointsFromPiece(TrackPiece piece, List<Transform> pathList, bool includeStartPoint)
    {
        if (includeStartPoint)
        {
            pathList.Add(piece.startPoint);
        }
        if (piece.waypoints != null && piece.waypoints.Count > 0)
        {
            pathList.AddRange(piece.waypoints);
        }
        pathList.Add(piece.endPoint);
    }
    
    // --- GetRandomWeightedPieceメソッドは変更なし ---
    private GameObject GetRandomWeightedPiece(List<WeightedTrackPiece> pieces)
    {
        int totalWeight = 0;
        foreach (var piece in pieces) { totalWeight += piece.weight; }
        int randomPoint = Random.Range(0, totalWeight);
        foreach (var piece in pieces)
        {
            if (randomPoint < piece.weight) { return piece.piecePrefab; }
            else { randomPoint -= piece.weight; }
        }
        return pieces.First().piecePrefab;
    }
}