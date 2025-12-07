// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    [Header("走行設定")]
    [SerializeField] private float rotationSmoothness = 10.0f;
    [SerializeField] private float speedChangeSmoothness = 5.0f;

    private List<Transform> pathPoints;
    private float currentPathProgress = 0.0f;
    private Rigidbody rb;
    
    private PlayerPieceData pieceData;
    private TrackPiece lastCheckedPiece = null;
    
    private float currentMoveSpeed = 0f; 
    private float targetMoveSpeed = 0f;  
    private float itemSpeedMultiplier = 1.0f; 

    // --- ゴールしたかどうかのフラグ ---
    private bool hasFinished = false;

    public void SetSpeedMultiplier(float multiplier)
    {
        itemSpeedMultiplier = multiplier;
    }

    public void PromotePiece() { if (pieceData != null) pieceData.Promote(); }
    public void DemotePiece() { if (pieceData != null) pieceData.Demote(); }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    public void RefreshPieceData(PlayerPieceData newPieceData)
    {
        pieceData = newPieceData;
        if (pieceData != null)
        {
            lastCheckedPiece = null;
            currentMoveSpeed = pieceData.baseSpeed;
            targetMoveSpeed = pieceData.baseSpeed;
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }

    public void SetCourse(List<Transform> fullPath)
    {
        pathPoints = fullPath;
        if (pathPoints == null || pathPoints.Count < 2) enabled = false;
    }

    void FixedUpdate()
    {
        // 準備ができていない、または既にゴール済みなら動かない
        if (pathPoints == null || pieceData == null || hasFinished) return;

        // --- ▼ 修正：レース中でなければ動かない（スタート待ち） ▼ ---
        if (RaceGameManager.Instance != null && !RaceGameManager.Instance.IsRacing())
        {
            // まだレースが始まっていないなら、速度を0にして待機
            rb.linearVelocity = Vector3.zero;
            return; 
        }
        // --- ▲ ここまで ▲ ---

        // 1. 目標速度の更新
        int p1_index = Mathf.FloorToInt(currentPathProgress);
        
        // --- ▼ 修正：ゴール判定 ▼ ---
        // パスの最後の点に到達したらゴール
        if (p1_index >= pathPoints.Count - 1)
        {
            FinishRace();
            return;
        }
        // --- ▲ ここまで ▲ ---

        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();
        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            lastCheckedPiece = currentPiece;
        }

        // 2. 移動処理
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        float finalSpeed = currentMoveSpeed * itemSpeedMultiplier;

        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        
        float actualSpeed = rb.linearVelocity.magnitude;
        float desiredSpeed = desiredVelocity.magnitude;
        bool isBlocked = (desiredSpeed > 1.0f && actualSpeed < desiredSpeed * 0.5f);
        if (currentPathProgress < 1.0f) isBlocked = false;

        if (!isBlocked)
        {
            int p2_index = p1_index + 1;
            Vector3 p1 = pathPoints[p1_index].position;
            Vector3 p2 = pathPoints[p2_index].position;
            float segmentLength = Vector3.Distance(p1, p2);
            float progressIncrement = (segmentLength > 0.001f) ? (finalSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
            currentPathProgress += progressIncrement;
        }
        
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

    /// <summary>
    /// ゴール時の処理
    /// </summary>
    private void FinishRace()
    {
        hasFinished = true;
        rb.linearVelocity = Vector3.zero; // 完全停止
        rb.isKinematic = true;      // 物理演算も停止

        // マネージャーにゴールを報告
        if (RaceGameManager.Instance != null)
        {
            RaceGameManager.Instance.OnGoal();
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