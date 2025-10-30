// PlayerPieceData.cs
using UnityEngine;
using System.Collections.Generic; // ListとDictionaryを使うために必要

// --- 他スクリプトからも参照されるenum定義 ---
// （TrackPiece.csにも定義がある場合、重複しないよう管理が必要ですが、
//   PlayerPieceData.csで一括管理するのが簡単です）

/// <summary>
/// 地形のタイプ（TrackPiece.csと共通で使用）
/// </summary>
public enum TerrainType
{
    Normal,     // 通常（デフォルト）
    Straight,   // 直線
    Curve,      // カーブ
    SlopeUp,    // 上り坂
    SlopeDown   // 下り坂
}

/// <summary>
/// 駒の種類（識別用）
/// </summary>
public enum PieceType
{
    Osho,     // 王将
    Kinsho,   // 金将
    Hisha,    // 飛車
    Fuhyo     // 歩兵（デフォルト）
}

// --- ここからが新しい定義です ---

/// <summary>
/// 「地形」と「速度倍率」をペアにするためのデータクラス。
/// [System.Serializable]を付けることで、インスペクターに表示されます。
/// </summary>
[System.Serializable]
public class TerrainModifier
{
    [Tooltip("効果を発動させたい地形タイプ")]
    public TerrainType terrainType;
    
    [Tooltip("この地形で、基本速度が何倍になるか (例: 1.5 = 1.5倍速, 0.5 = 半分の速度)")]
    public float speedMultiplier = 1.0f;
}

// --- PlayerPieceData本体の改造 ---

/// <summary>
/// プレイヤーの駒（モデル）にアタッチし、
/// 駒の固有の性能と、地形ごとの「速度補正リスト」を定義する。
/// </summary>
public class PlayerPieceData : MonoBehaviour
{
    [Header("駒の基本設定")]
    [Tooltip("この駒の種類（主に識別のために使用）")]
    public PieceType pieceType = PieceType.Fuhyo;

    [Tooltip("この駒の基本となる移動速度")]
    public float baseSpeed = 5.0f;

    [Header("地形ごとの速度補正リスト")]
    [Tooltip("この駒が特定の地形に入った時の速度倍率を自由に設定します。")]
    public List<TerrainModifier> terrainModifiers;
    
    // 検索を高速化するための内部辞書
    private Dictionary<TerrainType, float> modifierMap;

    void Awake()
    {
        // ゲーム開始時に、インスペクターで設定したリストを高速な辞書（マップ）に変換する
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
    /// CourseProgressor（土台）から呼び出される関数。
    /// 現在の地形を受け取り、この駒が出すべき速度を計算して返す。
    /// </summary>
    public float GetCurrentSpeed(TerrainType currentTerrain)
    {
        float multiplier;

        // マップ（辞書）に現在の地形用の設定があるか検索
        if (modifierMap.TryGetValue(currentTerrain, out multiplier))
        {
            // 設定が見つかった場合
            return baseSpeed * multiplier;
        }
        else
        {
            // 設定が見つからない場合（Normal地形や未設定の地形）
            return baseSpeed; // 基本速度をそのまま返す
        }
    }
}