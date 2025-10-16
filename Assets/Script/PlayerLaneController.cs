// PlayerLaneController.cs
using UnityEngine;

/// <summary>
/// プレイヤーの左右移動を制御するスクリプト。
/// 親オブジェクトに追従しつつ、左右の入力でレーン移動を行う。
/// </summary>
public class PlayerLaneController : MonoBehaviour
{
    [Tooltip("左右に移動する速さ")]
    [SerializeField] private float laneChangeSpeed = 5.0f;

    [Tooltip("中央から左右にどれだけ移動できるか（レーンの幅）")]
    [SerializeField] private float laneWidth = 2.0f;

    private float currentHorizontalPosition = 0f;

    void Update()
    {
        // 1. 左右のキー入力を取得（-1.0f から 1.0f の範囲）
        float input = Input.GetAxis("Horizontal");

        // 2. 入力に基づいて目標の水平位置を計算
        currentHorizontalPosition += input * laneChangeSpeed * Time.deltaTime;

        // 3. 移動範囲を制限する (Clamping)
        currentHorizontalPosition = Mathf.Clamp(currentHorizontalPosition, -laneWidth, laneWidth);

        // 4. 自身のローカル座標（親オブジェクトからの相対位置）に反映
        //    X軸方向のみ変更し、YとZは0のままにする
        transform.localPosition = new Vector3(currentHorizontalPosition, 0, 0);
    }
}