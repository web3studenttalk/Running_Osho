// NPCBrain.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CourseProgressor))]
public class NPCBrain : MonoBehaviour
{
    [Header("駒の候補リスト")]
    [Tooltip("NPCが変身する可能性のある駒（飛車、歩兵など）")]
    [SerializeField] private List<GameObject> piecePool;

    private CourseProgressor courseProgressor;
    private GameObject currentPieceInstance;

    void Awake()
    {
        courseProgressor = GetComponent<CourseProgressor>();
    }

    void Start()
    {
        // ゲーム開始時に、ランダムな駒に変身する
        ChangePieceRandomly();
    }

    /// <summary>
    /// プールの中からランダムに駒を選んで装備する
    /// </summary>
    public void ChangePieceRandomly()
    {
        if (piecePool == null || piecePool.Count == 0) return;

        // 1. ランダムに選ぶ
        GameObject selectedPrefab = piecePool[Random.Range(0, piecePool.Count)];
        
        // 2. 装備する
        SpawnPiece(selectedPrefab);
    }

    // 駒を生成して装備する（PieceDeckManagerとほぼ同じ処理）
    private void SpawnPiece(GameObject prefabToSpawn)
    {
        // 古い駒を削除
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // 新しい駒を生成
        currentPieceInstance = Instantiate(prefabToSpawn, transform);
        currentPieceInstance.transform.localPosition = Vector3.zero;
        currentPieceInstance.transform.localRotation = Quaternion.identity;

        // データ更新
        PlayerPieceData newData = currentPieceInstance.GetComponent<PlayerPieceData>();
        if (courseProgressor != null && newData != null)
        {
            courseProgressor.RefreshPieceData(newData);
        }
    }
}