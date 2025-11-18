// PlayerPieceData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地形のタイプ
/// </summary>
public enum TerrainType
{
    Normal,     // 通常
    Straight,   // 直線
    Curve,      // カーブ
    SlopeUp,    // 上り坂
    SlopeDown   // 下り坂
}

/// <summary>
/// 駒の種類（指定された8種類）
/// </summary>
public enum PieceType
{
    Hisha,    // 飛車
    Kyosha,   // 香車
    Kakugyo,  // 角行
    Kinsho,   // 金将
    Ginsho,   // 銀将
    Keima,    // 桂馬
    Fuhyo,    // 歩兵
    Osho      // 王将
}

/// <summary>
/// 「地形」と「速度倍率」をペアにするためのデータクラス
/// </summary>
[System.Serializable]
public class TerrainModifier
{
    [Tooltip("効果を発動させたい地形タイプ")]
    public TerrainType terrainType;
    
    [Tooltip("この地形で、基本速度が何倍になるか (例: 1.5 = 1.5倍速, 0.5 = 半分の速度)")]
    public float speedMultiplier = 1.0f;
}

/// <summary>
/// プレイヤーの駒（モデル）にアタッチするデータクラス
/// </summary>
public class PlayerPieceData : MonoBehaviour
{
    [Header("駒の基本設定")]
    [Tooltip("この駒の種類")]
    public PieceType pieceType = PieceType.Fuhyo;

    [Tooltip("この駒の基本となる移動速度")]
    public float baseSpeed = 5.0f;

    [Header("UI表示用")]
    [Tooltip("この駒に対応するボタン画像（アイコン）")]
    public Sprite pieceIcon;

    [Header("地形ごとの速度補正リスト")]
    [Tooltip("この駒が特定の地形に入った時の速度倍率を自由に設定します。")]
    public List<TerrainModifier> terrainModifiers;
    
    // 検索を高速化するための内部辞書
    private Dictionary<TerrainType, float> modifierMap;

    void Awake()
    {
        // リストを辞書に変換して検索を高速化
        modifierMap = new Dictionary<TerrainType, float>();
        if (terrainModifiers != null)
        {
            foreach (var modifier in terrainModifiers)
            {
                if (!modifierMap.ContainsKey(modifier.terrainType))
                {
                    modifierMap.Add(modifier.terrainType, modifier.speedMultiplier);
                }
            }
        }
    }

    /// <summary>
    /// 現在の地形に応じた速度を計算して返す
    /// </summary>
    public float GetCurrentSpeed(TerrainType currentTerrain)
    {
        if (modifierMap.TryGetValue(currentTerrain, out float multiplier))
        {
            return baseSpeed * multiplier;
        }
        else
        {
            return baseSpeed;
        }
    }
}