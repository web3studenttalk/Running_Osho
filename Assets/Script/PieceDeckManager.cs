// PieceDeckManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PieceDeckManager : MonoBehaviour
{
    [Header("プレイヤーの土台")]
    [SerializeField] private GameObject playerRig;

    [Header("駒のプレハブ設定")]
    [Tooltip("抽選される駒のプレハブ全種類")]
    [SerializeField] private List<GameObject> piecePool;

    [Header("UI設定")]
    [SerializeField] private List<PieceSelectButton> deckButtons;

    // 現在表示されているコマのインスタンスを保存しておく変数
    private GameObject currentPieceInstance;

    void Start()
    {
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        if (piecePool == null || piecePool.Count == 0) return;

        for (int i = 0; i < deckButtons.Count; i++)
        {
            GameObject selectedPrefab = piecePool[Random.Range(0, piecePool.Count)];
            deckButtons[i].Setup(selectedPrefab, this);

            if (i == 0) SpawnPiece(selectedPrefab);
        }
    }

    // --- ▼ 修正箇所：古いコマを確実に消すロジック ▼ ---
    public void SpawnPiece(GameObject prefabToSpawn)
    {
        if (playerRig == null || prefabToSpawn == null) return;

        // 1. 以前に生成したコマが残っていれば、即座に無効化して削除予約する
        if (currentPieceInstance != null)
        {
            currentPieceInstance.SetActive(false); // これで瞬時に見えなくなります
            Destroy(currentPieceInstance);         // その後、メモリから削除
        }

        // 念のため、PlayerRigの下にある他の不要な子要素も掃除する（初回起動時などのゴミ掃除）
        // ただし、今作ったばかりの currentPieceInstance は消さないように注意
        int childCount = playerRig.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = playerRig.transform.GetChild(i).gameObject;
            // 以前の変な残骸があれば消す（念入りな掃除）
            if (child != currentPieceInstance && child.activeSelf) 
            {
                child.SetActive(false);
                Destroy(child);
            }
        }

        // 2. 新しいコマを生成
        currentPieceInstance = Instantiate(prefabToSpawn, playerRig.transform);
        currentPieceInstance.transform.localPosition = Vector3.zero;
        currentPieceInstance.transform.localRotation = Quaternion.identity;

        // 3. 新しいコマからデータを取得
        PlayerPieceData newData = currentPieceInstance.GetComponent<PlayerPieceData>();

        // 4. 土台に「この新しいデータを使って！」と直接渡す
        CourseProgressor progressor = playerRig.GetComponent<CourseProgressor>();
        if (progressor != null)
        {
            progressor.RefreshPieceData(newData);
        }
    }
    // --- ▲ 修正ここまで ▲ ---
}