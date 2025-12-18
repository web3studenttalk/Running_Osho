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

    // 現在のコマを保持する変数
    private GameObject currentPieceInstance;

    void Start()
    {
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        if (piecePool != null && piecePool.Count > 0)
        {
            for (int i = 0; i < deckButtons.Count; i++)
            {
                if (deckButtons[i] == null) continue;
                // ランダムに選んでセット
                GameObject selectedPrefab = piecePool[Random.Range(0, piecePool.Count)];
                deckButtons[i].Setup(selectedPrefab, this);

                // 最初は左端(0番)のコマでスタート
                if (i == 0) SpawnPiece(selectedPrefab);
            }
        }
        
        // 王将ボタンのセットアップ
        if (oshoButton != null && oshoPrefab != null) 
            oshoButton.Setup(oshoPrefab, this);
    }

    // --- ▼ 修正箇所：ボタンを消さずに、中身を入れ替える ▼ ---
    public void OnPieceButtonPressed(PieceSelectButton button, GameObject prefab)
    {
        // 1. コマを生成して変身
        SpawnPiece(prefab);

        // 2. ボタンの処理
        if (button != null)
        {
            // 王将ボタンは「固定」なので何もしない（そのまま）
            if (button == oshoButton)
            {
                return;
            }

            // 通常のデッキボタンなら「補充」を行う
            if (piecePool != null && piecePool.Count > 0)
            {
                // 新しいコマをランダムに抽選
                GameObject nextPrefab = piecePool[Random.Range(0, piecePool.Count)];
                
                // そのボタンに新しいコマをセット（アイコンも自動で変わります）
                button.Setup(nextPrefab, this);
                
                // 念のため表示をオンにする（消えていたら復活させる）
                button.gameObject.SetActive(true);
            }
            else
            {
                // 万が一プールが空なら、ボタンを消すしかない
                button.gameObject.SetActive(false);
            }
        }
    }
    // --- ▲ 修正ここまで ▲ ---

    public void SpawnPiece(GameObject prefabToSpawn)
    {
        if (playerRig == null || prefabToSpawn == null)
        {
            Debug.LogError("SpawnPieceエラー: PlayerRig または Prefab がnullです");
            return;
        }

        Vector3 previousLocalPos = Vector3.zero;
        Quaternion previousLocalRot = Quaternion.identity;
        bool foundActivePiece = false;

        // 1. Activeな（見えている）子供を探して位置を保存
        foreach (Transform child in playerRig.transform)
        {
            if (child.gameObject.activeSelf)
            {
                previousLocalPos = child.localPosition;
                previousLocalRot = child.localRotation;
                foundActivePiece = true;
                break;
            }
        }

        // 2. 子供をすべて削除
        foreach (Transform child in playerRig.transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // 3. 新しいコマを生成
        GameObject newPiece = Instantiate(prefabToSpawn, playerRig.transform);
        currentPieceInstance = newPiece;

        // 4. 位置と回転を復元
        if (foundActivePiece)
        {
            newPiece.transform.localPosition = previousLocalPos;
            newPiece.transform.localRotation = previousLocalRot;
        }
        else
        {
            newPiece.transform.localPosition = Vector3.zero;
            newPiece.transform.localRotation = Quaternion.identity;
        }

        // 5. データ渡し
        PlayerPieceData newData = newPiece.GetComponent<PlayerPieceData>();
        CourseProgressor progressor = playerRig.GetComponent<CourseProgressor>();
        
        if (progressor != null && newData != null)
        {
            progressor.RefreshPieceData(newData);
        }
    }
}