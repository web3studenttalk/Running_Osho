// ItemPickup.cs
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private ItemManager itemManager;

    void Start()
    {
        // シーン内のGameManagerを探して、ItemManagerを取得しておく
        itemManager = FindFirstObjectByType<ItemManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤー（のリグ、またはその子供）がぶつかったら
        // タグ判定などが面倒なので、親にCourseProgressorがあるかで判定します
        if (other.GetComponentInParent<CourseProgressor>() != null)
        {
            if (itemManager != null)
            {
                // アイテムゲット処理を呼ぶ
                itemManager.GetRandomItem();
                
                // 自分自身（アイテム箱）を消す
                Destroy(gameObject);
            }
        }
    }
}