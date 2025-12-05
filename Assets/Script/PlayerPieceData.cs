// PlayerPieceData.cs
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
    public float baseSpeed = 5.0f;

    [Header("UI表示用")]
    public Sprite pieceIcon;

    [Header("成り設定")]
    [Tooltip("通常時のマテリアル")]
    [SerializeField] private Material normalMaterial;
    [Tooltip("成った時のマテリアル（UV設定済み）")]
    [SerializeField] private Material promotedMaterial;
    
    // --- ▼ 修正：手動でも設定できるようにし、型を Renderer に変更 ▼ ---
    [Tooltip("色を変えたいオブジェクト（higeなど）をここにドラッグ＆ドロップしてください")]
    [SerializeField] private Renderer targetRenderer; 
    // --- ▲ ここまで ▲ ---

    [Header("地形ごとの速度補正リスト")]
    public List<TerrainModifier> terrainModifiers;
    private Dictionary<TerrainType, float> modifierMap;

    void Awake()
    {
        // もしインスペクターで設定されていなければ、自動で探す
        if (targetRenderer == null)
        {
            // MeshRendererだけでなく、SkinnedMeshRendererも探せるように変更
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        // 初期化
        Demote();

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

    public void Promote()
    {
        if (targetRenderer != null && promotedMaterial != null)
        {
            targetRenderer.material = promotedMaterial;
        }
    }

    public void Demote()
    {
        if (targetRenderer != null && normalMaterial != null)
        {
            targetRenderer.material = normalMaterial;
        }
    }

    public float GetCurrentSpeed(TerrainType currentTerrain)
    {
        if (modifierMap.TryGetValue(currentTerrain, out float multiplier))
            return baseSpeed * multiplier;
        else
            return baseSpeed;
    }
}