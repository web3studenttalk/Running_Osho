// TrackPiece.cs
using System.Collections.Generic;
using UnityEngine;

// enum TerrainType の定義を削除！ (PlayerPieceData.csに移動したため)

public class TrackPiece : MonoBehaviour
{
    public Transform startPoint;
    public List<Transform> waypoints;
    public Transform endPoint;

    [Header("この区間の地形設定")]
    [Tooltip("このコース部品の地形タイプ")]
    public TerrainType terrainType = TerrainType.Normal; 
}