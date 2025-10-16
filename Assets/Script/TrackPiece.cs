// TrackPiece.cs
using UnityEngine;

/// <summary>
/// 各コース部品が持つ接続情報。
/// 「入り口」と「出口」の両方を定義する。
/// </summary>
public class TrackPiece : MonoBehaviour
{
    [Tooltip("この部品の接続の基準となる『入り口』")]
    public Transform startPoint;

    [Tooltip("次の部品が接続される『出口』")]
    public Transform endPoint;
}