// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    [Header("走行設定")]
    [SerializeField] private float rotationSmoothness = 10.0f;
    [Tooltip("速度が変化する時の滑らかさ")]
    [SerializeField] private float speedChangeSmoothness = 5.0f;

    private List<Transform> pathPoints;
    private float currentPathProgress = 0.0f;
    private Rigidbody rb;
    
    private PlayerPieceData pieceData;
    private TrackPiece lastCheckedPiece = null;
    
    private float currentMoveSpeed = 0f; 
    private float targetMoveSpeed = 0f;  

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        
        // 初期化時は何もしない（PieceDeckManagerからの登録を待つ）
    }

    // --- ▼ 修正箇所：速度を即座に適用する ▼ ---
    public void RefreshPieceData(PlayerPieceData newPieceData)
    {
        pieceData = newPieceData;

        if (pieceData == null)
        {
            Debug.LogError("新しい駒のデータがnullです！");
            enabled = false;
        }
        else
        {
            // 地形情報をリセット
            lastCheckedPiece = null;
            
            // 【重要】切り替え直後は、現在の速度も目標速度も、新しい駒の基本速度に強制一致させる
            // これにより「速度0」の状態や「前の駒の速度」を引きずらず、即座に走り出す
            currentMoveSpeed = pieceData.baseSpeed;
            targetMoveSpeed = pieceData.baseSpeed;
            
            enabled = true;
        }
    }
    // --- ▲ 修正ここまで ▲ ---

    public void SetCourse(List<Transform> fullPath)
    {
        pathPoints = fullPath;
        if (pathPoints == null || pathPoints.Count < 2)
        {
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (pathPoints == null || pieceData == null) return;

        // 1. 現在のTrackPieceを取得して、目標速度を更新
        int p1_index = Mathf.FloorToInt(currentPathProgress);
        if (p1_index >= pathPoints.Count) return;

        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();

        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            lastCheckedPiece = currentPiece;
        }

        // 2. 速度の更新
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        
        // 3. 移動処理
        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        
        // --- ▼ 修正箇所：スタート時の強制発進ロジック ▼ ---
        float actualSpeed = rb.linearVelocity.magnitude;
        float desiredSpeed = desiredVelocity.magnitude;
        
        // 基本は「動きたいのに動けない」ならブロックとみなす
        bool isBlocked = (desiredSpeed > 1.0f && actualSpeed < desiredSpeed * 0.5f);

        // 【重要】ただし、スタート直後（進捗が1.0未満）は絶対にブロック判定しない
        // これにより、停止状態から確実に動き出せるようにする
        if (currentPathProgress < 1.0f)
        {
            isBlocked = false;
        }
        // --- ▲ 修正ここまで ▲ ---

        if (!isBlocked)
        {
            int p2_index = p1_index + 1;
            if (p2_index >= pathPoints.Count)
            {
                enabled = false;
                return;
            }
            Vector3 p1 = pathPoints[p1_index].position;
            Vector3 p2 = pathPoints[p2_index].position;
            float segmentLength = Vector3.Distance(p1, p2);
            
            // currentMoveSpeedが0だと進まないので、最低値を保証するガードを入れても良いが
            // RefreshPieceDataでの初期化で対応済み
            float progressIncrement = (segmentLength > 0.001f) ? (currentMoveSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
            currentPathProgress += progressIncrement;
        }
        
        // Y軸の速度もそのまま適用（坂道対応）
        rb.linearVelocity = desiredVelocity; 

        if (!isBlocked)
        {
            Vector3 lookDirection = GetPointOnSpline(currentPathProgress + 0.1f) - rb.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime));
            }
        }
    }

    private Vector3 GetPointOnSpline(float progress)
    {
        int p0_index = Mathf.FloorToInt(progress) - 1;
        int p1_index = p0_index + 1;
        int p2_index = p0_index + 2;
        int p3_index = p0_index + 3;
        float t = progress - Mathf.FloorToInt(progress);
        Vector3 p0 = pathPoints[Mathf.Clamp(p0_index, 0, pathPoints.Count - 1)].position;
        Vector3 p1 = pathPoints[Mathf.Clamp(p1_index, 0, pathPoints.Count - 1)].position;
        Vector3 p2 = pathPoints[Mathf.Clamp(p2_index, 0, pathPoints.Count - 1)].position;
        Vector3 p3 = pathPoints[Mathf.Clamp(p3_index, 0, pathPoints.Count - 1)].position;
        Vector3 position = 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
        return position;
    }
}