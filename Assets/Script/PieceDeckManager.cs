// PieceDeckManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PieceDeckManager : MonoBehaviour
{
    [Header("プレイヤーの土台")]
    [SerializeField] private GameObject playerRig;

    [Header("駒のプレハブ設定")]
    [Tooltip("ランダム抽選される駒のリスト（飛車、角行、金、銀、桂、香、歩など。※王将は入れない）")]
    [SerializeField] private List<GameObject> piecePool;

    [Tooltip("王将のプレハブ（固定用）")]
    [SerializeField] private GameObject oshoPrefab; // ← 追加

    [Header("UI設定")]
    [Tooltip("画面中央下の5つのボタン（ランダム枠）")]
    [SerializeField] private List<PieceSelectButton> deckButtons;

    [Tooltip("画面右の王将専用ボタン")]
    [SerializeField] private PieceSelectButton oshoButton; // ← 追加

    // 現在表示されているコマのインスタンス
    private GameObject currentPieceInstance;

    void Start()
    {
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        // 1. 中央の5つのボタンをランダムに設定
        if (piecePool != null && piecePool.Count > 0)
        {
            for (int i = 0; i < deckButtons.Count; i++)
            {
                GameObject selectedPrefab = piecePool[Random.Range(0, piecePool.Count)];
                deckButtons[i].Setup(selectedPrefab, this);

                // ゲーム開始時は、一番左の駒でスタート
                if (i == 0) SpawnPiece(selectedPrefab);
            }
        }

        // 2. 右の王将ボタンを設定（固定）
        if (oshoButton != null && oshoPrefab != null)
        {
            oshoButton.Setup(oshoPrefab, this);
        }
    }

    // コマを生成して切り替える（変更なし）
    public void SpawnPiece(GameObject prefabToSpawn)
    {
        if (playerRig == null || prefabToSpawn == null) return;

        // 古いコマを消す（即時非表示＋削除）
        if (currentPieceInstance != null)
        {
            currentPieceInstance.SetActive(false);
            Destroy(currentPieceInstance);
        }

        // 念のための掃除
        int childCount = playerRig.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = playerRig.transform.GetChild(i).gameObject;
            if (child != currentPieceInstance && child.activeSelf) 
            {
                child.SetActive(false);
                Destroy(child);
            }
        }

        // 新しいコマを生成
        currentPieceInstance = Instantiate(prefabToSpawn, playerRig.transform);
        currentPieceInstance.transform.localPosition = Vector3.zero;
        currentPieceInstance.transform.localRotation = Quaternion.identity;

        // データ更新
        PlayerPieceData newData = currentPieceInstance.GetComponent<PlayerPieceData>();
        CourseProgressor progressor = playerRig.GetComponent<CourseProgressor>();
        if (progressor != null)
        {
            progressor.RefreshPieceData(newData);
        }
    }
}