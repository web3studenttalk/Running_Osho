// PieceSelectButton.cs
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectButton : MonoBehaviour
{
    [Header("UI参照")]
    [Tooltip("アイコンを表示するImageコンポーネント（設定必須！）")]
    [SerializeField] private Image iconImage; 

    private GameObject myPiecePrefab; 
    private PieceDeckManager manager; 

    // セットアップ処理
    public void Setup(GameObject piecePrefab, PieceDeckManager deckManager)
    {
        myPiecePrefab = piecePrefab;
        manager = deckManager;

        if (piecePrefab == null)
        {
            Debug.LogError($"ボタン {gameObject.name} に空のプレハブが渡されました！");
            return;
        }

        if (iconImage == null)
        {
            Debug.LogError($"【設定忘れ】ボタン {gameObject.name} の 'Icon Image' が設定されていません！Inspectorを確認してください。");
            return;
        }

        PlayerPieceData data = piecePrefab.GetComponent<PlayerPieceData>();
        if (data != null && data.pieceIcon != null)
        {
            iconImage.sprite = data.pieceIcon;
            iconImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"プレハブ {piecePrefab.name} にアイコン画像がありません。");
            iconImage.enabled = false;
        }
    }

    // ボタンがクリックされた時の処理
    public void OnClickButton()
    {
        if (manager == null)
        {
            Debug.LogError($"エラー：ボタン {gameObject.name} のManagerが設定されていません。");
            return;
        }

        if (myPiecePrefab == null)
        {
            Debug.LogError($"エラー：ボタン {gameObject.name} にプレハブが割り当てられていません。");
            return;
        }

        // ▼▼▼ 変更点：直接SpawnPieceを呼ばず、ボタン消費用のメソッドを呼ぶ ▼▼▼
        manager.OnPieceButtonPressed(this, myPiecePrefab);
    }
}