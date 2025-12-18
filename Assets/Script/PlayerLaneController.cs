// PlayerLaneController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動スピード")]
    [SerializeField] private float moveSpeed = 5.0f;

    // 操作する対象（現在のコマ）
    private Transform currentPiece;

    void Update()
    {
        // 1. 操作するコマを見つける
        if (currentPiece == null)
        {
            FindCurrentPiece();
            if (currentPiece == null) return;
        }

        // 2. 入力と移動処理
        // 以前のような「目標地点(currentX)」の計算をやめ、
        // 入力があったらその分だけ直接座標を動かします。
        // これなら壁があっても数値が蓄積せず、すぐに逆方向へ戻れます。
        HandleMovement();
    }

    private void FindCurrentPiece()
    {
        var pieceData = GetComponentInChildren<PlayerPieceData>();
        if (pieceData != null)
        {
            currentPiece = pieceData.transform;
        }
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        float inputDirection = 0f;

        // 左入力
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            inputDirection = -1f;
        }
        // 右入力
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            inputDirection = 1f;
        }

        // 入力がある時だけ動かす
        if (inputDirection != 0f)
        {
            Vector3 localPos = currentPiece.localPosition;
            
            // 現在の位置に対して、スピード分を加算する
            float moveAmount = inputDirection * moveSpeed * Time.deltaTime;
            localPos.x += moveAmount;

            // ※ここに制限(Clamp)がないため、壁があるまで無限に動けます
            currentPiece.localPosition = localPos;
        }
    }
}