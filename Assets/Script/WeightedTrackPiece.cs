// WeightedTrackPiece.cs
using UnityEngine;

[System.Serializable]
public class WeightedTrackPiece
{
    [Tooltip("コース部品のプレハブ")]
    public GameObject piecePrefab;

    [Tooltip("この部品の生成されやすさ。0にすると生成されません。")]
    [Range(0, 100)] // ← 0から始まるように修正
    public int weight = 10;
}