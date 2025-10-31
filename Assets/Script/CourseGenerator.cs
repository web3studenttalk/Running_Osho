// CourseGenerator.cs
using System.Collections.Generic;
using System.Linq; // Whereメソッドなどを使うために必要
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
        
        List<Transform> fullPath = new List<Transform>();

        // 1. スタート部品を配置し、パス情報を取得
        GameObject startPiece = Instantiate(startPiecePrefab, transform.position, transform.rotation, this.transform);
        AddPathPointsFromPiece(startPiece.GetComponent<TrackPiece>(), fullPath, true); 
        lastEndPoint = startPiece.GetComponent<TrackPiece>().endPoint;

        GameObject lastUsedPrefab = null;

        // 2. 中間部品を配置し、パス情報を取得
        for (int i = 0; i < middlePiecesCount; i++)
        {
            // --- ▼ 連続しないロジック（ウエイトが0の部品も考慮）▼ ---
            List<WeightedTrackPiece> availablePieces = middlePiecePrefabs
                .Where(p => p.piecePrefab != lastUsedPrefab) // 前回使用したものを除外
                .Where(p => p.weight > 0)                   // ウエイトが0のものも除外
                .ToList();

            // もし候補がなくなったら（例：ウエイト0以外の部品が1種類しかない）
            if (availablePieces.Count == 0)
            {
                // フィルターを緩めて、ウエイトが0より大きいものから再抽選
                availablePieces = middlePiecePrefabs.Where(p => p.weight > 0).ToList();
                
                // それでも候補がなければ（＝全てウエイト0）、警告を出して元のリストを使う
                if (availablePieces.Count == 0)
                {
                    Debug.LogWarning("抽選可能な（ウエイトが0より大きい）部品がありません。");
                    availablePieces = middlePiecePrefabs; 
                }
            }
            
            // --- ▲ ロジックここまで ▲ ---

            GameObject nextPrefab = GetRandomWeightedPiece(availablePieces); // 修正済みの抽選メソッドを呼ぶ
            
            if (nextPrefab != null)
            {
                PlacePiece(nextPrefab, fullPath); 
                lastUsedPrefab = nextPrefab;
            }
            else
            {
                Debug.LogError("次のプレハブの抽選に失敗しました。インスペクターの設定を確認してください。");
                break; // ループを中断
            }
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
    
    private void PlacePiece(GameObject piecePrefab, List<Transform> pathList) 
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
        
        AddPathPointsFromPiece(trackPiece, pathList, false); 

        lastEndPoint = trackPiece.endPoint;
    }
    
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
    
    /// <summary>
    /// ウエイト付きリストから、重みを考慮してランダムにプレハブを1つ選んで返す
    /// （ウエイトが0の部品は抽選から除外するよう修正）
    /// </summary>
    private GameObject GetRandomWeightedPiece(List<WeightedTrackPiece> pieces)
    {
        // 1. ウエイトが0より大きい、抽選対象となる部品だけのリストを新しく作成
        var availableWeightedPieces = pieces.Where(p => p.weight > 0).ToList();

        // 2. もし抽選可能な部品が一つもなかったら
        if (availableWeightedPieces.Count == 0)
        {
            // 渡された元のリストが空でなければ、警告を出して「ウエイト0」の部品から先頭のものを返す
            if (pieces.Count > 0)
            {
                Debug.LogWarning("GetRandomWeightedPiece: 抽選可能な（ウエイトが0より大きい）部品がありません。ウエイト0の部品を返します。");
                return pieces.First().piecePrefab;
            }
            
            // 渡されたリスト自体が空なら、致命的なエラー
            Debug.LogError("GetRandomWeightedPiece: 抽選リストが空です！ CourseGeneratorのインスペクターを確認してください。");
            return null;
        }
        
        // 3. 抽選可能な部品だけで合計ウエイトを計算
        int totalWeight = 0;
        foreach (var piece in availableWeightedPieces)
        {
            totalWeight += piece.weight;
        }
        
        // 4. 0から合計ウエイトまでの範囲でランダムな数値を決める
        int randomPoint = Random.Range(0, totalWeight);

        // 5. 抽選可能なリストから、当選する部品を選ぶ
        foreach (var piece in availableWeightedPieces)
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
        
        // 6. 万が一のフォールバック
        return availableWeightedPieces.First().piecePrefab;
    }
}