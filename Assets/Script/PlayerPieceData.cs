// PlayerPieceData.cs
using UnityEngine;
using System.Collections.Generic;

public enum TerrainType { Normal, Straight, BigCurve, SCurve, SlopeUp, SlopeDown, Narrow, Obstacle }
public enum PieceType { Hisha, Kyosha, Kakugyo, Kinsho, Ginsho, Keima, Fuhyo, Osho }

[System.Serializable]
public class TerrainModifier
{
    [Tooltip("効果を発動させたい地形タイプ")]
    public TerrainType terrainType;
    
    [Tooltip("この地形で、基本速度が何倍になるか (例: 1.5 = 1.5倍速)")]
    public float speedMultiplier = 1.0f;
}

public class PlayerPieceData : MonoBehaviour
{
    [Header("駒の基本設定")]
    public PieceType pieceType = PieceType.Fuhyo;
    [Tooltip("全体の基準速度に対して何倍の速さか（例: 1.0=標準, 2.0=2倍速）")]
    public float speedRatio = 1.0f;

    [Header("成り設定（基本）")]
    [Tooltip("リストに設定がない地形で、成っている時のデフォルト倍率")]
    public float promotedSpeedMultiplier = 1.2f;

    [Header("成り設定（見た目）")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material promotedMaterial;
    [SerializeField] private Renderer targetRenderer; 

    [Header("UI表示用")]
    public Sprite pieceIcon;

    // --- ▼ 修正：リストを2つに分けました ▼ ---
    [Header("【通常時】地形ごとの速度補正")]
    public List<TerrainModifier> terrainModifiers;

    [Header("【成り時】地形ごとの速度補正")]
    public List<TerrainModifier> promotedTerrainModifiers;
    // --- ▲ ここまで ▲ ---

    // 高速検索用の辞書も2つ用意
    private Dictionary<TerrainType, float> modifierMap;
    private Dictionary<TerrainType, float> promotedModifierMap;

    private bool isPromoted = false;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        Demote(); 

        // 通常時のマップ作成
        modifierMap = new Dictionary<TerrainType, float>();
        if (terrainModifiers != null)
        {
            foreach (var modifier in terrainModifiers)
            {
                if (!modifierMap.ContainsKey(modifier.terrainType))
                    modifierMap.Add(modifier.terrainType, modifier.speedMultiplier);
            }
        }

        // 成り時のマップ作成
        promotedModifierMap = new Dictionary<TerrainType, float>();
        if (promotedTerrainModifiers != null)
        {
            foreach (var modifier in promotedTerrainModifiers)
            {
                if (!promotedModifierMap.ContainsKey(modifier.terrainType))
                    promotedModifierMap.Add(modifier.terrainType, modifier.speedMultiplier);
            }
        }
    }

    public void Promote()
    {
        isPromoted = true;
        if (targetRenderer != null && promotedMaterial != null)
            targetRenderer.material = promotedMaterial;
    }

    public void Demote()
    {
        isPromoted = false;
        if (targetRenderer != null && normalMaterial != null)
            targetRenderer.material = normalMaterial;
    }

    /// <summary>
    /// 現在の地形と状態に応じた速度を計算して返す
    /// </summary>
    public float GetCurrentSpeed(TerrainType currentTerrain)
    {
        // 1. まず基本速度を計算
        float globalBase = 10f;
        if (RaceGameManager.Instance != null) globalBase = RaceGameManager.Instance.globalBaseSpeed;
        float baseSpeed = globalBase * speedRatio;

        // 2. 成っている場合
        if (isPromoted)
        {
            // 「成り専用リスト」に設定があればそれを使う
            if (promotedModifierMap.TryGetValue(currentTerrain, out float multiplier))
            {
                return baseSpeed * multiplier;
            }
            // なければ「成りデフォルト倍率」を使う
            else
            {
                return baseSpeed * promotedSpeedMultiplier;
            }
        }
        // 3. 通常の場合
        else
        {
            // 「通常リスト」に設定があればそれを使う
            if (modifierMap.TryGetValue(currentTerrain, out float multiplier))
            {
                return baseSpeed * multiplier;
            }
            // なければ基本速度 (1.0倍)
            else
            {
                return baseSpeed;
            }
        }
    }
}