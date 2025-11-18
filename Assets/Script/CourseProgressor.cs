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
    
    private float currentMoveSpeed; 
    private float targetMoveSpeed;  

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        
        // 初期化時は子要素から探す（念のため）
        pieceData = GetComponentInChildren<PlayerPieceData>();
        
        if (pieceData != null)
        {
            currentMoveSpeed = pieceData.baseSpeed;
            targetMoveSpeed = pieceData.baseSpeed;
        }
    }

    // --- ▼ 修正箇所：新しい駒を直接受け取り、状態をリセットするメソッド ▼ ---
    public void RefreshPieceData(PlayerPieceData newPieceData)
    {
        // 1. 新しい駒のデータを登録
        pieceData = newPieceData;

        if (pieceData == null)
        {
            Debug.LogError("新しい駒のデータがnullです！");
            enabled = false;
        }
        else
        {
            // 2. 「最後に確認した地形」情報をリセット
            // これにより、FixedUpdateで「地形が変わった！」と判定され、
            // 即座に新しい駒の能力で速度が再計算されます。
            lastCheckedPiece = null;
            
            // 3. スクリプトを有効化して再開
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
        if (pathPoints == null) return;

        // 1. 現在のTrackPieceを取得
        int p1_index = Mathf.FloorToInt(currentPathProgress);
        // エラー回避：インデックスが範囲外なら処理しない
        if (p1_index >= pathPoints.Count) return;

        TrackPiece currentPiece = pathPoints[p1_index].GetComponentInParent<TrackPiece>();

        // 2. 地形チェック（lastCheckedPieceをnullにしたので、切り替え直後は必ず実行される）
        if (currentPiece != null && currentPiece != lastCheckedPiece)
        {
            // 新しい駒に「この地形での速度」を問い合わせて更新
            if (pieceData != null)
            {
                targetMoveSpeed = pieceData.GetCurrentSpeed(currentPiece.terrainType);
            }
            lastCheckedPiece = currentPiece;
        }

        // 3. 速度の更新
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetMoveSpeed, Time.fixedDeltaTime * speedChangeSmoothness);
        
        // 4. 移動処理
        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        
        // ブロック判定
        float actualSpeed = rb.linearVelocity.magnitude;
        float desiredSpeed = desiredVelocity.magnitude;
        bool isBlocked = (desiredSpeed > 1.0f && actualSpeed < desiredSpeed * 0.5f);

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
            
            float progressIncrement = (segmentLength > 0.001f) ? (currentMoveSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
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