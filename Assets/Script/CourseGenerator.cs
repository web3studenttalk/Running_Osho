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

    [Header("プレイヤーオブジェクト（シーン上の実体を指定）")]
    // 以前の followerPrefab から変更しました
    [Tooltip("HierarchyにあるPlayerRigをここにドラッグ＆ドロップしてください")]
    [SerializeField] private CourseProgressor scenePlayerRig; 

    private Transform lastEndPoint;
    
    void Start()
    {
        GenerateCourse();
    }

    private void GenerateCourse()
    {
        // 古いコース部品を削除
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        if (startPiecePrefab == null || endPiecePrefab == null || middlePiecePrefabs == null || middlePiecePrefabs.Count == 0)
        {
            Debug.LogError("コース部品のプレハブがインスペクターで設定されていません。");
            return;
        }
        
        List<Transform> fullPath = new List<Transform>();

        // 1. スタート部品を配置
        GameObject startPiece = Instantiate(startPiecePrefab, transform.position, transform.rotation, this.transform);
        AddPathPointsFromPiece(startPiece.GetComponent<TrackPiece>(), fullPath, true); 
        lastEndPoint = startPiece.GetComponent<TrackPiece>().endPoint;

        GameObject lastUsedPrefab = null;

        // 2. 中間部品を配置
        for (int i = 0; i < middlePiecesCount; i++)
        {
            List<WeightedTrackPiece> availablePieces = middlePiecePrefabs
                .Where(p => p.piecePrefab != lastUsedPrefab)
                .Where(p => p.weight > 0)
                .ToList();

            if (availablePieces.Count == 0)
            {
                availablePieces = middlePiecePrefabs.Where(p => p.weight > 0).ToList();
                if (availablePieces.Count == 0) availablePieces = middlePiecePrefabs; 
            }
            
            GameObject nextPrefab = GetRandomWeightedPiece(availablePieces);
            
            if (nextPrefab != null)
            {
                PlacePiece(nextPrefab, fullPath); 
                lastUsedPrefab = nextPrefab;
            }
            else
            {
                break;
            }
        }

        // 3. ゴール部品を配置
        PlacePiece(endPiecePrefab, fullPath);

        // デバッグ用のパス可視化
        for (int i = 0; i < fullPath.Count - 1; i++)
        {
            Debug.DrawLine(fullPath[i].position, fullPath[i + 1].position, Color.red, 20f);
        }

        // --- ▼ 修正箇所：シーンにいるプレイヤーにコース情報を渡す ▼ ---
        if (scenePlayerRig != null)
        {
            // 1. プレイヤーをスタート地点に移動させる
            Vector3 startPos = fullPath[0].position;
            Quaternion startRot = fullPath[0].rotation;
            
            // Rigidbodyを使っているので、MovePosition/Rotationで移動させるか、
            // 一時的にtransformを直接セットする（初期化なので直接セットでOK）
            scenePlayerRig.transform.position = startPos;
            scenePlayerRig.transform.rotation = startRot;
            
            // 2. コース情報（パス）を渡す
            scenePlayerRig.SetCourse(fullPath);
        }
        else
        {
            Debug.LogWarning("CourseGenerator: Scene Player Rig が設定されていません！プレイヤーは動きません。");
        }
        // --- ▲ 修正ここまで ▲ ---
    }
    
    private void PlacePiece(GameObject piecePrefab, List<Transform> pathList) 
    {
        GameObject newPiece = Instantiate(piecePrefab, this.transform);
        TrackPiece trackPiece = newPiece.GetComponent<TrackPiece>();
        
        if (trackPiece == null || trackPiece.startPoint == null || trackPiece.endPoint == null)
        {
            Destroy(newPiece);
            return;
        }

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
        if (piece.waypoints != null && piece.waypoints.Count > 0) pathList.AddRange(piece.waypoints);
        pathList.Add(piece.endPoint);
    }
    
    private GameObject GetRandomWeightedPiece(List<WeightedTrackPiece> pieces)
    {
        var availableWeightedPieces = pieces.Where(p => p.weight > 0).ToList();
        if (availableWeightedPieces.Count == 0) return pieces.Count > 0 ? pieces.First().piecePrefab : null;
        
        int totalWeight = 0;
        foreach (var piece in availableWeightedPieces) totalWeight += piece.weight;
        
        int randomPoint = Random.Range(0, totalWeight);
        foreach (var piece in availableWeightedPieces)
        {
            if (randomPoint < piece.weight) return piece.piecePrefab;
            else randomPoint -= piece.weight;
        }
        return availableWeightedPieces.First().piecePrefab;
    }
}