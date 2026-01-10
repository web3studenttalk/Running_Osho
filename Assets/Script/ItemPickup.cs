using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    [Header("再出現の設定")]
    [SerializeField] private float respawnTime = 5.0f; //

    private Collider itemCollider;
    private Renderer[] allRenderers; // 全ての見た目コンポーネントを保持

    void Awake()
    {
        // 自身のコライダーを取得
        itemCollider = GetComponent<Collider>();
        
        // 自分自身および子オブジェクトに含まれる全ての Renderer を取得
        allRenderers = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 誰に当たったか判定（本人または親が Racer か確認）
        bool isRacer = other.CompareTag("Racer") || (other.transform.parent != null && other.transform.parent.CompareTag("Racer"));
        
        if (!isRacer) return;

        // 2. アイテムをランダムに決定
        ItemType randomItem = (ItemType)Random.Range(1, 5); //

        // 3. プレイヤーかNPCかで処理を分ける
        if (other.GetComponentInParent<PlayerLaneController>() != null)
        {
            ItemManager.Instance.GiveItemToPlayer(randomItem); //
        }
        else
        {
            CourseProgressor npc = other.GetComponentInParent<CourseProgressor>();
            if (npc != null)
            {
                ItemManager.Instance.StartCoroutine(ItemManager.Instance.ApplyEffect(npc, randomItem)); //
            }
        }

        // 4. 削除せずに、非表示にして復活タイマーを開始
        StopAllCoroutines(); // 念のため重複動作を防止
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // --- 非表示・当たり判定オフ ---
        if (itemCollider != null) itemCollider.enabled = false;
        
        // 全てのレンダラーをオフにする
        foreach (var rend in allRenderers)
        {
            if (rend != null) rend.enabled = false;
        }

        // 指定時間待機
        yield return new WaitForSeconds(respawnTime);

        // --- 表示・当たり判定オン ---
        if (itemCollider != null) itemCollider.enabled = true;
        
        // 全てのレンダラーをオンに戻す
        foreach (var rend in allRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
    }
}