// PlayerPieceData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地形のタイプ（更新版）
/// </summary>
public enum TerrainType
{
    Normal,      // 設定なし（デフォルト）
    Straight,    // 直線
    BigCurve,    // 大カーブ
    SCurve,      // S字カーブ
    SlopeUp,     // 上り坂
    SlopeDown,   // 下り坂
    Narrow,      // 狭窄（道幅が狭い）
    Obstacle     // 障害物
}

/// <summary>
/// 駒の種類
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

[System.Serializable]
public class TerrainModifier
{
    [Tooltip("効果を発動させたい地形タイプ")]
    public TerrainType terrainType;
    
    [Tooltip("この地形で、基本速度が何倍になるか (例: 1.5 = 1.5倍速, 0.5 = 半分の速度)")]
    public float speedMultiplier = 1.0f;
}

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
    
    private Dictionary<TerrainType, float> modifierMap;

    void Awake()
    {
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