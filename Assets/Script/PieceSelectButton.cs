// PieceSelectButton.cs
using UnityEngine;
using UnityEngine.UI; // Imageを扱うために必要

public class PieceSelectButton : MonoBehaviour
{
    [Header("UI参照")]
    [Tooltip("アイコンを表示するImageコンポーネント")]
    [SerializeField] private Image iconImage; 

    private GameObject myPiecePrefab; 
    private PieceDeckManager manager; 

    /// <summary>
    /// Managerから呼び出され、このボタンの画像を設定する
    /// </summary>
    public void Setup(GameObject piecePrefab, PieceDeckManager deckManager)
    {
        myPiecePrefab = piecePrefab;
        manager = deckManager;

        // 駒からPlayerPieceDataを取得して、画像を設定する
        PlayerPieceData data = piecePrefab.GetComponent<PlayerPieceData>();
        
        if (data != null && data.pieceIcon != null)
        {
            // 駒に設定されているアイコン画像をボタンに反映
            iconImage.sprite = data.pieceIcon;
            
            // 画像が透けていたり見えなくなっている場合のために有効化
            iconImage.enabled = true;
        }
        else
        {
            // 画像が設定されていない場合のエラー回避（白紙にするなど）
            Debug.LogWarning($"プレハブ {piecePrefab.name} にアイコン画像が設定されていません");
        }
    }

    // クリック時の処理（変更なし）
    public void OnClickButton()
    {
        if (manager != null && myPiecePrefab != null)
        {
            manager.SpawnPiece(myPiecePrefab);
        }
    }
}