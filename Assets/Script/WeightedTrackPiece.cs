// WeightedTrackPiece.cs
using UnityEngine;

/// <summary>
/// コース部品のプレハブとその生成比率（ウエイト）をセットで管理するためのクラス。
/// [System.Serializable]を付けることで、インスペクター上に表示できるようになる。
/// </summary>
[System.Serializable]
public class WeightedTrackPiece
{
    [Tooltip("コース部品のプレハブ")]
    public GameObject piecePrefab;

    [Tooltip("この部品の生成されやすさ。数値が大きいほど選ばれやすい。")]
    [Range(1, 100)] // インスペクターでスライダーとして表示
    public int weight = 10;
}