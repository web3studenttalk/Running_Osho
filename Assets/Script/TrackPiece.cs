// TrackPiece.cs
using System.Collections.Generic; // Listを使うために必要
using UnityEngine;

/// <summary>
/// 各コース部品が持つ接続情報。
/// 「入口」「中間地点」「出口」を定義する。
/// </summary>
public class TrackPiece : MonoBehaviour
{
    [Tooltip("この部品の接続の基準となる『入り口』")]
    public Transform startPoint;

    [Tooltip("コースの形状に沿って配置する中間地点のリスト")]
    public List<Transform> waypoints;

    [Tooltip("次の部品が接続される『出口』")]
    public Transform endPoint;
}