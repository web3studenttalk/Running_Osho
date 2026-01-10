using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    public static List<CourseProgressor> AllProgressors = new List<CourseProgressor>();

    [Header("走行設定")]
    [SerializeField] private float rotationSmoothness = 10.0f;
    [SerializeField] private float speedChangeSmoothness = 5.0f;
    [SerializeField] private float laneChangeSpeed = 5.0f;

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

    public void SetSpeedMultiplier(float multiplier) { itemSpeedMultiplier = multiplier; }
    public void PromotePiece() { if (pieceData != null) { pieceData.Promote(); RecalculateSpeed(); } }
    public void DemotePiece() { if (pieceData != null) { pieceData.Demote(); RecalculateSpeed(); } }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        if (!AllProgressors.Contains(this)) AllProgressors.Add(this);
    }

    private void OnDestroy() { AllProgressors.Remove(this); }

    // すり抜け判定（Is TriggerがONの場合）
    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished || RaceGameManager.Instance == null || !RaceGameManager.Instance.IsRacing()) return;

        // タグ「Racer」がエディタに登録されている必要があります
        if (other.CompareTag("Racer"))
        {
            CourseProgressor otherProgressor = other.GetComponentInParent<CourseProgressor>();
            if (otherProgressor == null || otherProgressor == this) return;
            
            // 自分の方が速ければ相手のコマを奪う
            if (this.currentMoveSpeed >= otherProgressor.currentMoveSpeed) ExecuteCapture(otherProgressor);
        }
    }

    private void ExecuteCapture(CourseProgressor victim)
    {
        PlayerPieceData victimPiece = victim.GetComponentInChildren<PlayerPieceData>();
        if (victimPiece == null) return;

        if (victimPiece.pieceType == PieceType.Osho)
        {
            if (victim.GetComponent<PlayerLaneController>() != null) RaceGameManager.Instance.GameOver(); 
            return;
        }

        var myDeck = GetComponent<PieceDeckManager>();
        if (myDeck != null) myDeck.AddCapturedPiece(victimPiece.myPrefab);

        var victimDeck = victim.GetComponent<PieceDeckManager>();
        if (victimDeck != null) victimDeck.LoseCurrentPiece();

        var victimBrain = victim.GetComponent<NPCBrain>();
        if (victimBrain != null) victimBrain.LoseCurrentPiece();

        Debug.Log($"{gameObject.name} が {victim.gameObject.name} の駒を奪った！");
    }

    public void RefreshPieceData(PlayerPieceData newPieceData)
    {
        pieceData = newPieceData;
        if (pieceData != null) {
            lastCheckedPiece = null;
            targetMoveSpeed = pieceData.GetCurrentSpeed(TerrainType.Normal);
            currentMoveSpeed = targetMoveSpeed;
            enabled = true;
        }
    }

    public void SetCourse(List<Transform> fullPath) { pathPoints = fullPath; }

    void FixedUpdate()
    {
        if (pathPoints == null || pieceData == null || hasFinished) return;
        if (RaceGameManager.Instance != null && !RaceGameManager.Instance.IsRacing()) { rb.linearVelocity = Vector3.zero; return; }

        int p1_index = Mathf.FloorToInt(currentPathProgress);
        if (p1_index >= pathPoints.Count - 1) { FinishRace(); return; }

        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();
        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            lastCheckedPiece = currentPiece;
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        float finalSpeed = currentMoveSpeed * itemSpeedMultiplier;

        currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, targetHorizontalOffset, Time.fixedDeltaTime * laneChangeSpeed);
        
        Vector3 basePos = GetPointOnSpline(currentPathProgress);
        Vector3 nextPos = GetPointOnSpline(currentPathProgress + 0.01f);
        Vector3 forward = (nextPos - basePos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 targetWorldPos = basePos + (right * currentHorizontalOffset);

        rb.linearVelocity = (targetWorldPos - rb.position) / Time.fixedDeltaTime;

        float segmentLength = Vector3.Distance(pathPoints[p1_index].position, pathPoints[p1_index+1].position);
        currentPathProgress += (segmentLength > 0.001f) ? (finalSpeed * Time.fixedDeltaTime) / segmentLength : 0f;

        if (forward != Vector3.zero) 
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(forward), rotationSmoothness * Time.fixedDeltaTime));
    }

    // エラー CS0103 を解消：RecalculateSpeedメソッド
    private void RecalculateSpeed()
    {
        if (pathPoints == null || pieceData == null) return;
        int i = Mathf.FloorToInt(currentPathProgress);
        if (i < pathPoints.Count) {
            var p = pathPoints[i].GetComponentInParent<TrackPiece>();
            if (p != null) targetMoveSpeed = pieceData.GetCurrentSpeed(p.terrainType);
        }
    }

    // エラー CS0111 を解消：FinishRaceメソッドを統合
    private void FinishRace()
    {
        if (hasFinished) return;
        hasFinished = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (RaceGameManager.Instance != null) RaceGameManager.Instance.ReportGoal(gameObject.name);
    }

    private Vector3 GetPointOnSpline(float p)
    {
        int i = Mathf.Clamp(Mathf.FloorToInt(p), 0, pathPoints.Count-1);
        int j = Mathf.Clamp(i+1, 0, pathPoints.Count-1);
        return Vector3.Lerp(pathPoints[i].position, pathPoints[j].position, p - Mathf.FloorToInt(p));
    }

    public TerrainType GetCurrentTerrainType() => (lastCheckedPiece != null) ? lastCheckedPiece.terrainType : TerrainType.Normal;
}