using UnityEngine;
using System.Collections.Generic;

public enum TerrainType { Normal, Straight, BigCurve, SCurve, SlopeUp, SlopeDown, Narrow, Obstacle }
public enum PieceType { Hisha, Kyosha, Kakugyo, Kinsho, Ginsho, Keima, Fuhyo, Osho }

[System.Serializable]
public class TerrainModifier
{
    public TerrainType terrainType;
    public float speedMultiplier = 1.0f;
}

public class PlayerPieceData : MonoBehaviour
{
    [Header("駒の基本設定")]
    public PieceType pieceType = PieceType.Fuhyo;
    public float speedRatio = 1.0f;

    [Header("システム用")]
    [Tooltip("この駒のプレハブ自身（奪われた時に相手に渡す用）")]
    public GameObject myPrefab; 

    [Header("成り設定（基本）")]
    public float promotedSpeedMultiplier = 1.2f;

    [Header("成り設定（見た目）")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material promotedMaterial;
    [SerializeField] private Renderer targetRenderer; 

    [Header("UI表示用")]
    public Sprite pieceIcon;

    [Header("【通常時】地形ごとの速度補正")]
    public List<TerrainModifier> terrainModifiers;

    [Header("【成り時】地形ごとの速度補正")]
    public List<TerrainModifier> promotedTerrainModifiers;

    private Dictionary<TerrainType, float> modifierMap;
    private Dictionary<TerrainType, float> promotedModifierMap;
    private bool isPromoted = false;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        Demote(); 

        modifierMap = new Dictionary<TerrainType, float>();
        if (terrainModifiers != null)
        {
            foreach (var modifier in terrainModifiers)
                if (!modifierMap.ContainsKey(modifier.terrainType))
                    modifierMap.Add(modifier.terrainType, modifier.speedMultiplier);
        }

        promotedModifierMap = new Dictionary<TerrainType, float>();
        if (promotedTerrainModifiers != null)
        {
            foreach (var modifier in promotedTerrainModifiers)
                if (!promotedModifierMap.ContainsKey(modifier.terrainType))
                    promotedModifierMap.Add(modifier.terrainType, modifier.speedMultiplier);
        }
    }

    public void Promote() { isPromoted = true; if (targetRenderer != null && promotedMaterial != null) targetRenderer.material = promotedMaterial; }
    public void Demote() { isPromoted = false; if (targetRenderer != null && normalMaterial != null) targetRenderer.material = normalMaterial; }

    public float GetCurrentSpeed(TerrainType currentTerrain)
    {
        float globalBase = 10f;
        if (RaceGameManager.Instance != null) globalBase = RaceGameManager.Instance.globalBaseSpeed;
        float baseSpeed = globalBase * speedRatio;

        if (isPromoted)
        {
            if (promotedModifierMap.TryGetValue(currentTerrain, out float multiplier)) return baseSpeed * multiplier;
            return baseSpeed * promotedSpeedMultiplier;
        }
        else
        {
            if (modifierMap.TryGetValue(currentTerrain, out float multiplier)) return baseSpeed * multiplier;
            return baseSpeed;
        }
    }
}