// CourseGenerator.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// スタート/ゴール/ウエイトを指定し、同じ部品が連続しないようにコースを自動生成する。
/// 生成後、CourseProgressorにコース情報（EndPointリスト）を渡す。
/// </summary>
public class CourseGenerator : MonoBehaviour
{
    [Header("コース構成部品")]
    [Tooltip("コースの開始地点に必ず配置される部品")]
    [SerializeField] private GameObject startPiecePrefab;
    
    [Tooltip("コースの中間部分でランダムに使用される部品リスト（ウエイト付き）")]
    [SerializeField] private List<WeightedTrackPiece> middlePiecePrefabs;
    
    [Tooltip("コースの終着点に必ず配置される部品")]
    [SerializeField] private GameObject endPiecePrefab;

    [Header("コース設定")]
    [Tooltip("中間に生成するコース部品の数")]
    [SerializeField] private int middlePiecesCount = 20;

    [Header("プレイヤーオブジェクト")]
    [Tooltip("コース上を自動で進むプレイヤーのプレハブ（親オブジェクト）")]
    [SerializeField] private GameObject followerPrefab; // この名前はplayerPrefabなどに変更してもOK

    private Transform lastEndPoint;
    // 生成された全てのEndPointを格納するリスト
    private List<Transform> allEndPoints = new List<Transform>(); 

    void Start()
    {
        GenerateCourse();
    }

    private void GenerateCourse()
    {
        // 既存のコースとプレイヤーを削除（複数回生成した場合の対処）
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        allEndPoints.Clear(); // リストもクリア

        if (startPiecePrefab == null || endPiecePrefab == null || middlePiecePrefabs == null || middlePiecePrefabs.Count == 0)
        {
            Debug.LogError("コース部品のプレハブがインスペクターで設定されていません。");
            return;
        }

        // 1. スタート部品を配置
        GameObject startPiece = Instantiate(startPiecePrefab, transform.position, transform.rotation, this.transform);
        lastEndPoint = startPiece.GetComponent<TrackPiece>().endPoint;
        allEndPoints.Add(lastEndPoint); // EndPointをリストに追加

        GameObject lastUsedPrefab = null;

        // 2. 中間部品を指定した数だけランダムに連結
        for (int i = 0; i < middlePiecesCount; i++)
        {
            List<WeightedTrackPiece> availablePieces = middlePiecePrefabs.Where(p => p.piecePrefab != lastUsedPrefab).ToList();
            if (availablePieces.Count == 0)
            {
                availablePieces = middlePiecePrefabs;
            }

            GameObject nextPrefab = GetRandomWeightedPiece(availablePieces);
            PlacePiece(nextPrefab);
            lastUsedPrefab = nextPrefab;
        }

        // 3. ゴール部品を最後に配置
        PlacePiece(endPiecePrefab);

        // 4. プレイヤーオブジェクトの生成と設定
        if (followerPrefab != null)
        {
            // プレイヤーをスタート部品のスタート地点に生成
            Vector3 spawnPos = startPiece.GetComponent<TrackPiece>().startPoint.position;
            Quaternion spawnRot = startPiece.GetComponent<TrackPiece>().startPoint.rotation;

            GameObject playerRig = Instantiate(followerPrefab, spawnPos, spawnRot, this.transform);
            
            // CourseProgressorスクリプトを取得するように変更
            CourseProgressor courseProgressor = playerRig.GetComponent<CourseProgressor>(); 
            if (courseProgressor != null)
            {
                // 生成された全てのEndPointのリストをプレイヤーの親オブジェクトに渡す
                courseProgressor.SetCourse(allEndPoints);
            }
            else
            {
                // エラーメッセージも更新
                Debug.LogError("プレイヤープレハブの親オブジェクトに CourseProgressor スクリプトがアタッチされていません！");
            }
        }
        else
        {
            Debug.LogWarning("プレイヤープレハブが設定されていません。");
        }
    }

    private GameObject GetRandomWeightedPiece(List<WeightedTrackPiece> pieces)
    {
        int totalWeight = 0;
        foreach (var piece in pieces)
        {
            totalWeight += piece.weight;
        }

        int randomPoint = Random.Range(0, totalWeight);

        foreach (var piece in pieces)
        {
            if (randomPoint < piece.weight)
            {
                return piece.piecePrefab;
            }
            else
            {
                randomPoint -= piece.weight;
            }
        }
        return pieces.First().piecePrefab;
    }

    private void PlacePiece(GameObject piecePrefab)
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
        
        lastEndPoint = trackPiece.endPoint;
        allEndPoints.Add(lastEndPoint); // ここでもEndPointをリストに追加
    }
}