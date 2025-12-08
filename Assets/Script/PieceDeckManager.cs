// PieceDeckManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PieceDeckManager : MonoBehaviour
{
    [Header("プレイヤーの土台")]
    [SerializeField] private GameObject playerRig;

    [Header("駒のプレハブ設定")]
    [SerializeField] private List<GameObject> piecePool;
    [SerializeField] private GameObject oshoPrefab;

    [Header("UI設定")]
    [SerializeField] private List<PieceSelectButton> deckButtons;
    [SerializeField] private PieceSelectButton oshoButton;

    void Start()
    {
        Debug.Log("PieceDeckManager: 初期化を開始します...");
        InitializeDeck();
        Debug.Log("PieceDeckManager: 初期化が完了しました！");
    }

    private void InitializeDeck()
    {
        // 1. 中央5つのボタン設定
        if (piecePool != null && piecePool.Count > 0)
        {
            for (int i = 0; i < deckButtons.Count; i++)
            {
                if (deckButtons[i] == null)
                {
                    Debug.LogError($"エラー：Deck Buttonsの {i}番目 が空欄です！Inspectorを確認してください。");
                    continue;
                }

                GameObject selectedPrefab = piecePool[Random.Range(0, piecePool.Count)];
                
                // ボタンのセットアップを呼び出す
                deckButtons[i].Setup(selectedPrefab, this);

                // 最初は左端のコマでスタート
                if (i == 0) SpawnPiece(selectedPrefab);
            }
        }
        else
        {
            Debug.LogError("エラー：Piece Pool（駒リスト）が空っぽです！");
        }

        // 2. 王将ボタン設定
        if (oshoButton != null && oshoPrefab != null)
        {
            oshoButton.Setup(oshoPrefab, this);
        }
        else
        {
            if (oshoButton == null) Debug.LogError("エラー：Osho Buttonが設定されていません！");
            if (oshoPrefab == null) Debug.LogError("エラー：Osho Prefabが設定されていません！");
        }
    }

    public void SpawnPiece(GameObject prefabToSpawn)
    {
        if (playerRig == null || prefabToSpawn == null)
        {
            Debug.LogError("SpawnPieceエラー: PlayerRig または Prefab がnullです");
            return;
        }

        // 子要素を全削除
        foreach (Transform child in playerRig.transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // 生成
        GameObject newPiece = Instantiate(prefabToSpawn, playerRig.transform);
        newPiece.transform.localPosition = Vector3.zero;
        newPiece.transform.localRotation = Quaternion.identity;

        // データ渡し
        PlayerPieceData newData = newPiece.GetComponent<PlayerPieceData>();
        CourseProgressor progressor = playerRig.GetComponent<CourseProgressor>();
        
        if (progressor != null && newData != null)
        {
            progressor.RefreshPieceData(newData);
        }
    }
}