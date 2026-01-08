// NPCManager.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PieceEntry
{
    public string name;
    public GameObject prefab;
    [Tooltip("確率の重み（大きいほど出やすい）")]
    public int weight = 10;
}

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("【固定枠】")]
    [Tooltip("王将のプレハブ（必ず保持するコマ）")]
    public GameObject oshoPrefab;

    [Header("【ランダム枠】")]
    [Tooltip("出現させたいコマと確率（王将以外を登録）")]
    public List<PieceEntry> pieceTable;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject GetRandomPiece()
    {
        if (pieceTable == null || pieceTable.Count == 0) return null;

        int totalWeight = 0;
        foreach (var entry in pieceTable) totalWeight += entry.weight;

        int randomValue = Random.Range(0, totalWeight);

        foreach (var entry in pieceTable)
        {
            if (randomValue < entry.weight) return entry.prefab;
            randomValue -= entry.weight;
        }

        return pieceTable[0].prefab;
    }
    
    public GameObject GetOshoPiece()
    {
        return oshoPrefab;
    }
}