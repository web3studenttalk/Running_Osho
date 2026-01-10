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
    private bool hasFinished = false;

    public void SetSpeedMultiplier(float multiplier) { itemSpeedMultiplier = multiplier; }
    public void PromotePiece() { if (pieceData != null) { pieceData.Promote(); RecalculateSpeed(); } }
    public void DemotePiece() { if (pieceData != null) { pieceData.Demote(); RecalculateSpeed(); } }

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
            float startSpeed = pieceData.GetCurrentSpeed(TerrainType.Normal);
            currentMoveSpeed = startSpeed;
            targetMoveSpeed = startSpeed;
            enabled = true;
        }
        else enabled = false;
    }

    public void SetCourse(List<Transform> fullPath)
    {
        pathPoints = fullPath;
        if (pathPoints == null || pathPoints.Count < 2) enabled = false;
    }

    void FixedUpdate()
    {
        if (pathPoints == null || pieceData == null || hasFinished) return;

        if (RaceGameManager.Instance != null && !RaceGameManager.Instance.IsRacing())
        {
            rb.linearVelocity = Vector3.zero;
            return; 
        }

        int p1_index = Mathf.FloorToInt(currentPathProgress);
        if (p1_index >= pathPoints.Count - 1)
        {
            FinishRace(); // ここでゴール処理
            return;
        }

        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();
        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            lastCheckedPiece = currentPiece;
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        float finalSpeed = currentMoveSpeed * itemSpeedMultiplier;

        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        
        rb.linearVelocity = desiredVelocity; 

        // 進捗の更新
        int p2_index = p1_index + 1;
        float segmentLength = Vector3.Distance(pathPoints[p1_index].position, pathPoints[p2_index].position);
        float progressIncrement = (segmentLength > 0.001f) ? (finalSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
        currentPathProgress += progressIncrement;

        // 回転の更新
        Vector3 lookDirection = GetPointOnSpline(currentPathProgress + 0.1f) - rb.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime));
        }
    }

    private void RecalculateSpeed()
    {
        if (pathPoints == null || pieceData == null) return;
        int index = Mathf.FloorToInt(currentPathProgress);
        if (index < pathPoints.Count)
        {
            TrackPiece piece = pathPoints[index].GetComponentInParent<TrackPiece>();
            if (piece != null) targetMoveSpeed = pieceData.GetCurrentSpeed(piece.terrainType);
        }
    }

    // 重複エラー(CS0111)を避けるため、一つにまとめます
    private void FinishRace()
    {
        if (hasFinished) return;
        hasFinished = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (RaceGameManager.Instance != null)
        {
            RaceGameManager.Instance.ReportGoal(gameObject.name);
        }
    }

    private Vector3 GetPointOnSpline(float progress)
    {
        int p0_index = Mathf.Clamp(Mathf.FloorToInt(progress) - 1, 0, pathPoints.Count - 1);
        int p1_index = Mathf.Clamp(p0_index + 1, 0, pathPoints.Count - 1);
        int p2_index = Mathf.Clamp(p0_index + 2, 0, pathPoints.Count - 1);
        int p3_index = Mathf.Clamp(p0_index + 3, 0, pathPoints.Count - 1);
        float t = progress - Mathf.FloorToInt(progress);
        Vector3 p0 = pathPoints[p0_index].position;
        Vector3 p1 = pathPoints[p1_index].position;
        Vector3 p2 = pathPoints[p2_index].position;
        Vector3 p3 = pathPoints[p3_index].position;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
    }

    public TerrainType GetCurrentTerrainType() => (lastCheckedPiece != null) ? lastCheckedPiece.terrainType : TerrainType.Normal;
}