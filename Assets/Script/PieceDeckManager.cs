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

    void Start() { InitializeDeck(); }

    private void InitializeDeck() {
        if (piecePool != null && piecePool.Count > 0) {
            for (int i = 0; i < deckButtons.Count; i++) {
                if (deckButtons[i] == null) continue;
                GameObject sel = piecePool[Random.Range(0, piecePool.Count)];
                deckButtons[i].Setup(sel, this);
                if (i == 0) SpawnPiece(sel);
            }
        }
        if (oshoButton != null && oshoPrefab != null) oshoButton.Setup(oshoPrefab, this);
    }

    public void AddCapturedPiece(GameObject capturedPrefab) {
        if (capturedPrefab != null) piecePool.Add(capturedPrefab);
    }

    public void LoseCurrentPiece() {
        if (deckButtons.Count <= 1) return; 

        int lastIdx = deckButtons.Count - 1;
        GameObject btnObj = deckButtons[lastIdx].gameObject;
        deckButtons.RemoveAt(lastIdx);
        Destroy(btnObj); // スロットを物理的に削除

        if (piecePool.Count > 0) SpawnPiece(piecePool[Random.Range(0, piecePool.Count)]);
    }

    public void OnPieceButtonPressed(PieceSelectButton button, GameObject prefab) {
        SpawnPiece(prefab);
        if (button != oshoButton && piecePool.Count > 0) {
            button.Setup(piecePool[Random.Range(0, piecePool.Count)], this);
        }
    }

    public void SpawnPiece(GameObject prefab) {
        if (playerRig == null || prefab == null) return;
        Vector3 pos = Vector3.zero; Quaternion rot = Quaternion.identity; bool found = false;
        foreach (Transform child in playerRig.transform) { if (child.gameObject.activeSelf) { pos = child.localPosition; rot = child.localRotation; found = true; break; } }
        foreach (Transform child in playerRig.transform) { Destroy(child.gameObject); }
        GameObject newP = Instantiate(prefab, playerRig.transform);
        newP.transform.localPosition = found ? pos : Vector3.zero;
        newP.transform.localRotation = found ? rot : Quaternion.identity;
        var progressor = playerRig.GetComponent<CourseProgressor>();
        var data = newP.GetComponent<PlayerPieceData>();
        if (progressor != null && data != null) progressor.RefreshPieceData(data);
    }
}