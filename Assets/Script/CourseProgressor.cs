// CourseProgressor.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトをコースの経路上で自動的に前進させるスクリプト。
/// プレイヤーオブジェクトの親にアタッチされることを想定。
/// </summary>
public class CourseProgressor : MonoBehaviour
{
    [Tooltip("前進する速度")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("コースの次の目標地点へ向くときの滑らかさ")]
    [SerializeField] private float rotationSmoothness = 5.0f;

    private List<Transform> courseEndPoints;
    private int currentPointIndex = 0;
    private Transform currentTargetPoint;

    public void SetCourse(List<Transform> endPoints)
    {
        courseEndPoints = endPoints;
        if (courseEndPoints != null && courseEndPoints.Count > 0)
        {
            currentTargetPoint = courseEndPoints[0];
        }
        else
        {
            Debug.LogError("CourseProgressor: コースのEndPointが設定されていません。");
            enabled = false;
        }
    }

    void Update()
    {
        if (currentTargetPoint == null) return;

        // 1. 目標地点の方向へ滑らかに旋回する
        Vector3 direction = currentTargetPoint.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
        }

        // 2. 自身の前方にまっすぐ進む
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // 3. 目標地点に十分に近づいたら、次の目標へ更新
        if (Vector3.Distance(transform.position, currentTargetPoint.position) < 1.0f) // 到達判定距離を少し広げる
        {
            currentPointIndex++;
            if (currentPointIndex < courseEndPoints.Count)
            {
                currentTargetPoint = courseEndPoints[currentPointIndex];
            }
            else
            {
                Debug.Log("CourseProgressor: コースの終点に到達しました。");
                enabled = false;
            }
        }
    }
}