// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CourseProgressor : MonoBehaviour
{
    [Header("走行設定")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSmoothness = 10.0f;

    private List<Transform> pathPoints;
    private float currentPathProgress = 0.0f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
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

        // 1. 本来の目標位置と、そこへ向かうべき速度を計算
        Vector3 targetPosition = GetPointOnSpline(currentPathProgress);
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;

        // 2. 実際の速度（物理演算の結果）を取得
        float actualSpeed = rb.linearVelocity.magnitude;
        float desiredSpeed = desiredVelocity.magnitude;

        // 3. ブロックされているか判定
        bool isBlocked = false;
        if (desiredSpeed > 1.0f && actualSpeed < desiredSpeed * 0.5f)
        {
            isBlocked = true;
        }

        // 4. ブロックされていない時だけ、進捗(currentPathProgress)を進める
        if (!isBlocked)
        {
            int p1_index = Mathf.FloorToInt(currentPathProgress);
            int p2_index = p1_index + 1;
            if (p2_index >= pathPoints.Count)
            {
                enabled = false;
                return;
            }
            Vector3 p1 = pathPoints[p1_index].position;
            Vector3 p2 = pathPoints[p2_index].position;
            float segmentLength = Vector3.Distance(p1, p2);
            float progressIncrement = (segmentLength > 0.001f) ? (moveSpeed * Time.fixedDeltaTime) / segmentLength : 0f;
            
            currentPathProgress += progressIncrement;
        }
        
        // 5. Rigidbodyに速度を適用
        rb.linearVelocity = desiredVelocity; 

        // --- ▼ 回転ロジックを修正 ▼ ---
        
        // 6. ブロックされていない時だけ、進行方向を向くように回転する
        if (!isBlocked)
        {
            Vector3 lookDirection = GetPointOnSpline(currentPathProgress + 0.1f) - rb.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSmoothness * Time.fixedDeltaTime));
            }
        }
        // ブロックされている間は、MoveRotationが呼ばれないため、現在の向きを維持しようとします。
        // --- ▲ 修正ここまで ▲ ---
    }

    // GetPointOnSpline メソッド（変更なし）
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