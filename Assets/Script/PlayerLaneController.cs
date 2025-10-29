// PlayerLaneController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneController : MonoBehaviour
{
    [Tooltip("左右に移動する速さ")]
    [SerializeField] private float laneChangeSpeed = 5.0f;

    // このスクリプト内での移動範囲制限は使わないので、
    // laneWidth変数は削除しても、残しておいても影響ありません。
    // 分かりやすさのためにコメントアウトまたは削除します。
    // [SerializeField] private float laneWidth = 2.0f;

    private float currentHorizontalPosition = 0f;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool isLeftPressed = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool isRightPressed = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        
        float input = 0f;
        if (isRightPressed) input = 1f;
        else if (isLeftPressed) input = -1f;
        
        // 入力に基づいて水平位置を更新し続ける（制限なし）
        currentHorizontalPosition += input * laneChangeSpeed * Time.deltaTime;

        // YとZの位置は元のままで、X座標だけを更新
        transform.localPosition = new Vector3(
            currentHorizontalPosition,
            transform.localPosition.y,
            transform.localPosition.z
        );
    }
}