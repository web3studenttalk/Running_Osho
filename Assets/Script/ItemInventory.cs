// ItemInventory.cs (UI・使用機能付き)
using UnityEngine;
using UnityEngine.UI; // UIを使うために必要

public class ItemInventory : MonoBehaviour
{
    [Header("現在の状態")]
    public string currentItemName = "なし";
    public bool hasItem = false;

    [Header("プレイヤー用UI設定 (Playerのみ)")]
    [SerializeField] private Text itemText; // 画面のどこかにアイテム名を表示する場合

    private bool isPlayer;

    void Awake()
    {
        isPlayer = CompareTag("Player");
    }

    void Update()
    {
        // プレイヤーかつアイテムを持っているなら、Spaceキーで使用
        if (isPlayer && hasItem && Input.GetKeyDown(KeyCode.Space))
        {
            UseItem();
        }
    }

    public void AddItem(string itemName)
    {
        hasItem = true;
        currentItemName = itemName;
        
        Debug.Log($"{gameObject.name} が {itemName} を取得！");

        if (isPlayer && itemText != null)
        {
            itemText.text = itemName; // UIのテキストを書き換える
        }
    }

    public void UseItem()
    {
        Debug.Log(gameObject.name + " がアイテムを使用しました！: " + currentItemName);
        
        // ここに加速などの具体的な効果を入れる（今はログのみ）

        // アイテムをリセット（これで次が取れるようになる）
        hasItem = false;
        currentItemName = "なし";

        if (isPlayer && itemText != null)
        {
            itemText.text = "なし";
        }
    }
}