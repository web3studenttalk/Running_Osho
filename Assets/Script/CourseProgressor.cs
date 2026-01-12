using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    public static List<CourseProgressor> AllProgressors = new List<CourseProgressor>();

    [Header("走行設定")]
    [SerializeField] private float rotationSmoothness = 8.0f;
    [SerializeField] private float speedChangeSmoothness = 5.0f;
    [SerializeField] private float laneChangeSpeed = 4.0f;

    private List<Transform> pathPoints;
    private float currentPathProgress = 0.0f;
    private Rigidbody rb;
    private PlayerPieceData pieceData;
    private TrackPiece lastCheckedPiece = null;
    
    public float currentMoveSpeed { get; private set; } = 0f; 
    private float targetMoveSpeed = 0f;  
    private float itemSpeedMultiplier = 1.0f; 
    private bool hasFinished = false;

    public float targetHorizontalOffset = 0f;
    private float currentHorizontalOffset = 0f;

    public void SetSpeedMultiplier(float m) { itemSpeedMultiplier = m; }
    public void PromotePiece() { if (pieceData) pieceData.Promote(); }
    public void DemotePiece() { if (pieceData) pieceData.Demote(); }

    void Awake() 
    { 
        rb = GetComponent<Rigidbody>(); 
        if(!AllProgressors.Contains(this)) AllProgressors.Add(this); 
    }
    
    void OnDestroy() { AllProgressors.Remove(this); }

    private void OnTriggerEnter(Collider other) 
    {
        if (hasFinished || RaceGameManager.Instance == null || !RaceGameManager.Instance.IsRacing()) return;
        if (other.CompareTag("Racer")) 
        {
            CourseProgressor otherP = other.GetComponentInParent<CourseProgressor>();
            if (otherP && otherP != this && currentMoveSpeed >= otherP.currentMoveSpeed) ExecuteCapture(otherP);
        }
    }

    private void ExecuteCapture(CourseProgressor victim) 
    {
        var vData = victim.GetComponentInChildren<PlayerPieceData>();
        if (!vData) return;
        if (vData.pieceType == PieceType.Osho && victim.GetComponent<PlayerLaneController>()) 
        {
            RaceGameManager.Instance.GameOver(); return;
        }
        var vBrain = victim.GetComponent<NPCBrain>();
        if (vBrain) vBrain.LoseCurrentPiece();
    }

    public void RefreshPieceData(PlayerPieceData d) 
    { 
        pieceData = d; 
        if(d) 
        { 
            lastCheckedPiece = null; 
            targetMoveSpeed = d.GetCurrentSpeed(TerrainType.Normal); 
            currentMoveSpeed = targetMoveSpeed; 
            enabled = true; 
        } 
    }
    
    public void SetCourse(List<Transform> p) { pathPoints = p; }

    void FixedUpdate() 
    {
        if (pathPoints == null || pieceData == null || hasFinished) return;
        if (RaceGameManager.Instance && !RaceGameManager.Instance.IsRacing()) 
        { 
            rb.linearVelocity = Vector3.zero; 
            return; 
        }

        int idx = Mathf.FloorToInt(currentPathProgress);
        if (idx >= pathPoints.Count - 1) { FinishRace(); return; }

        TrackPiece piece = pathPoints[idx].GetComponentInParent<TrackPiece>();
        if (piece && piece != lastCheckedPiece) 
        { 
            targetMoveSpeed = pieceData.GetCurrentSpeed(piece.terrainType); 
            lastCheckedPiece = piece; 
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, targetHorizontalOffset, Time.fixedDeltaTime * laneChangeSpeed);

        // --- Catmull-Rom スプラインによる滑らかな座標取得 ---
        Vector3 basePos = GetPointOnSpline(currentPathProgress);
        Vector3 futurePos = GetPointOnSpline(currentPathProgress + 0.1f);
        
        Vector3 fwd = (futurePos - basePos).normalized;
        Vector3 rgt = Vector3.Cross(Vector3.up, fwd).normalized;
        
        Vector3 targetWorldPos = basePos + (rgt * currentHorizontalOffset);
        rb.linearVelocity = (targetWorldPos - rb.position) / Time.fixedDeltaTime;

        float segLen = Vector3.Distance(pathPoints[idx].position, pathPoints[idx+1].position);
        currentPathProgress += (segLen > 0.001f) ? (currentMoveSpeed * itemSpeedMultiplier * Time.fixedDeltaTime) / segLen : 0f;

        if (fwd != Vector3.zero) 
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(fwd.x, 0, fwd.z));
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSmoothness * Time.fixedDeltaTime));
        }
    }

    private void FinishRace() 
    {
        if (hasFinished) return; 
        hasFinished = true;
        rb.linearVelocity = Vector3.zero; 
        rb.isKinematic = true;
        if (RaceGameManager.Instance) RaceGameManager.Instance.ReportGoal(gameObject.name);
    }

    private Vector3 GetPointOnSpline(float p) 
    {
        int p1_index = Mathf.FloorToInt(p);
        int p0_index = Mathf.Clamp(p1_index - 1, 0, pathPoints.Count - 1);
        int p2_index = Mathf.Clamp(p1_index + 1, 0, pathPoints.Count - 1);
        int p3_index = Mathf.Clamp(p1_index + 2, 0, pathPoints.Count - 1);
        p1_index = Mathf.Clamp(p1_index, 0, pathPoints.Count - 1);

        float t = p - Mathf.FloorToInt(p);

        Vector3 p0 = pathPoints[p0_index].position;
        Vector3 p1 = pathPoints[p1_index].position;
        Vector3 p2 = pathPoints[p2_index].position;
        Vector3 p3 = pathPoints[p3_index].position;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    // ▼ これが欠けていたためエラーになっていました。必ず含めてください ▼
    public TerrainType GetCurrentTerrainType() => (lastCheckedPiece != null) ? lastCheckedPiece.terrainType : TerrainType.Normal;
}