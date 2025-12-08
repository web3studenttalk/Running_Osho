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

    // セットアップ処理（ここでエラーが起きると、ボタンが動かなくなります）
    public void Setup(GameObject piecePrefab, PieceDeckManager deckManager)
    {
        myPiecePrefab = piecePrefab;
        manager = deckManager;

        // 安全チェック：プレハブが空なら何もしない
        if (piecePrefab == null)
        {
            Debug.LogError($"ボタン {gameObject.name} に空のプレハブが渡されました！");
            return;
        }

        // 安全チェック：Imageが設定されていなければ警告を出して、処理を続ける
        if (iconImage == null)
        {
            Debug.LogError($"【設定忘れ】ボタン {gameObject.name} の 'Icon Image' が設定されていません！Inspectorを確認してください。");
            // 画像設定はスキップするが、Managerの登録は完了させる
            return;
        }

        // 画像の設定
        PlayerPieceData data = piecePrefab.GetComponent<PlayerPieceData>();
        if (data != null && data.pieceIcon != null)
        {
            iconImage.sprite = data.pieceIcon;
            iconImage.enabled = true;
        }
        else
        {
            // 画像がない場合は白くするなどで対応
            Debug.LogWarning($"プレハブ {piecePrefab.name} にアイコン画像がありません。");
            iconImage.enabled = false;
        }
        
        Debug.Log($"ボタン {gameObject.name} のセットアップ完了！割り当て: {piecePrefab.name}");
    }

    public void OnClickButton()
    {
        if (manager == null)
        {
            Debug.LogError($"エラー：ボタン {gameObject.name} のManagerが設定されていません。Setupが失敗している可能性があります。");
            return;
        }

        if (myPiecePrefab == null)
        {
            Debug.LogError($"エラー：ボタン {gameObject.name} にプレハブが割り当てられていません。");
            return;
        }

        Debug.Log("コマ切り替え実行: " + myPiecePrefab.name);
        manager.SpawnPiece(myPiecePrefab);
    }
}