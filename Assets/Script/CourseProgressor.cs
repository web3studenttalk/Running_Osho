// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトをコースの経路上で自動的に前進させるスクリプト。（改良版）
/// 2点間のセグメント（線路）に沿って移動することで、コースから逸脱しないようにする。
/// </summary>
public class CourseProgressor : MonoBehaviour
{
    [Tooltip("前進する速度")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("コースの次の目標地点へ向くときの滑らかさ")]
    [SerializeField] private float rotationSmoothness = 10.0f;

    private List<Transform> pathPoints;
    private int currentPointIndex = 0;
    
    // 現在走行している線路（セグメント）の始点と終点
    private Transform previousPoint;
    private Transform nextPoint;

    /// <summary>
    /// CourseGeneratorからコース全体のパス情報を受け取る
    /// </summary>
    public void SetCourse(List<Transform> fullPath)
    {
        pathPoints = fullPath;
        if (pathPoints != null && pathPoints.Count >= 2)
        {
            // 最初の線路（セグメント）を設定
            currentPointIndex = 0;
            previousPoint = pathPoints[0];
            nextPoint = pathPoints[1];
        }
        else
        {
            Debug.LogError("CourseProgressor: パス情報が不十分です。");
            enabled = false;
        }
    }

    void Update()
    {
        if (previousPoint == null || nextPoint == null) return;

        // --- ▼ 移動ロジックを全面的に刷新 ▼ ---

        // 1. 現在走るべき線路の方向を計算
        Vector3 segmentDirection = (nextPoint.position - previousPoint.position).normalized;

        // 2. その線路の方向にまっすぐ進む
        transform.position += segmentDirection * moveSpeed * Time.deltaTime;

        // 3. 常に線路の方向を向くように滑らかに回転する
        if (segmentDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(segmentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
        }

        // 4. 次の目標点を通り過ぎたかどうかを判定
        Vector3 toNextPoint = nextPoint.position - transform.position;
        // segmentDirectionと逆方向を向いていたら、通り過ぎたと判断
        if (Vector3.Dot(toNextPoint, segmentDirection) < 0)
        {
            // 次の線路（セグメント）へ移行する
            currentPointIndex++;
            if (currentPointIndex < pathPoints.Count - 1)
            {
                previousPoint = pathPoints[currentPointIndex];
                nextPoint = pathPoints[currentPointIndex + 1];
            }
            else
            {
                // 全てのコースを走り終えた
                Debug.Log("CourseProgressor: コースの終点に到達しました。");
                enabled = false;
            }
        }
    }
}