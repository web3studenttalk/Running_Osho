// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    [Header("走行設定")]
    [SerializeField] private float rotationSmoothness = 10.0f;
    [Tooltip("速度が変化する時の滑らかさ（値が小さいほどゆっくり変化）")]
    [SerializeField] private float speedChangeSmoothness = 5.0f;

    private List<Transform> pathPoints;
    private float currentPathProgress = 0.0f;
    private Rigidbody rb;
    
    private PlayerPieceData pieceData;
    private TrackPiece lastCheckedPiece = null;
    
    // --- ▼ 速度管理の変数を変更 ▼ ---
    private float currentMoveSpeed; // 現在の（滑らかに変化中の）速度
    private float targetMoveSpeed;  // 駒が指示する目標速度
    // --- ▲ 変更ここまで ▲ ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        
        pieceData = GetComponentInChildren<PlayerPieceData>();
        
        if (pieceData == null)
        {
            Debug.LogError("PlayerRigの子オブジェクトにPlayerPieceDataが見つかりません！");
            enabled = false;
            return;
        }
        
        // スタート時の速度を駒の基本速度に設定
        currentMoveSpeed = pieceData.baseSpeed;
        targetMoveSpeed = pieceData.baseSpeed;
    }

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
        if (pathPoints == null) return;

        // --- ▼ 速度計算ロジックを変更 ▼ ---

        // 1. 現在のインデックスから、今いるTrackPieceを取得
        int p1_index = Mathf.FloorToInt(currentPathProgress);
        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();

        // 2. 新しい区間に入ったら、駒に「目標速度」を問い合わせる
        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            // 目標速度(targetMoveSpeed)を更新
            targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            lastCheckedPiece = currentPiece;
        }

        // 3. 「現在の速度」を「目標速度」に向かって滑らかに変化させる
        currentMoveSpeed = Mathf.Lerp(
            currentMoveSpeed, 
            targetMoveSpeed, 
            Time.fixedDeltaTime * speedChangeSmoothness
        );
        
        // --- 速度蓄積防止ロジック（変更なし） ---
        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        float actualSpeed = rb.linearVelocity.magnitude;
        float desiredSpeed = desiredVelocity.magnitude;
        bool isBlocked = (desiredSpeed > 1.0f && actualSpeed < desiredSpeed * 0.5f);

        // 4. ブロックされていなければ、滑らかに変化させた「現在の速度」で進捗を進める
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
            // 速度の計算に targetMoveSpeed ではなく currentMoveSpeed を使用
            float progressIncrement = (segmentLength > 0.001f) ? (currentMoveSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
            currentPathProgress += progressIncrement;
        }
        
        // --- 物理・回転ロジック（変更なし） ---
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

    // --- GetPointOnSpline メソッド（変更なし） ---
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