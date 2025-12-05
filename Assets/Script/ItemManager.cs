// ItemManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ItemType
{
    None, Totsugeki, Nari, Kakoi, Kokaku
}

public class ItemManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private CourseProgressor playerProgressor;
    [SerializeField] private Button itemButton;
    [SerializeField] private Image itemIconImage;

    [Header("アイテム画像素材")]
    [SerializeField] private Sprite iconTotsugeki;
    [SerializeField] private Sprite iconNari;
    [SerializeField] private Sprite iconKakoi;
    [SerializeField] private Sprite iconKokaku;
    [SerializeField] private Sprite iconNone;

    private ItemType currentItem = ItemType.None;
    private Coroutine currentEffectCoroutine = null; // 現在実行中の効果コルーチン

    void Start()
    {
        UpdateUI();
        itemButton.onClick.AddListener(UseItem);
    }

    public void GetRandomItem()
    {
        int rand = Random.Range(1, 5);
        currentItem = (ItemType)rand;
        UpdateUI();
    }

    public void UseItem()
    {
        if (currentItem == ItemType.None) return;

        // 前の効果がまだ続いていたらキャンセルする（重複防止）
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
            ResetAllEffects(); // 全ての効果をリセット
        }

        // 新しい効果を開始
        currentEffectCoroutine = StartCoroutine(ApplyItemEffect(currentItem));

        currentItem = ItemType.None;
        UpdateUI();
    }

    // 効果終了時などに呼ばれ、全てのステータスを元に戻す
    private void ResetAllEffects()
    {
        playerProgressor.SetSpeedMultiplier(1.0f);
        playerProgressor.DemotePiece(); // 見た目を元に戻す
        // 他の効果（無敵など）があればここでも解除する
    }

    private IEnumerator ApplyItemEffect(ItemType type)
    {
        switch (type)
        {
            case ItemType.Totsugeki: // 突撃：3秒間、速度2倍
                playerProgressor.SetSpeedMultiplier(2.0f);
                yield return new WaitForSeconds(3.0f);
                break;

            // --- ▼ 修正：「成り」の効果実装 ▼ ---
            case ItemType.Nari: // 成り：5秒間、速度1.5倍 ＋ 見た目変更
                playerProgressor.SetSpeedMultiplier(1.5f);
                playerProgressor.PromotePiece(); // 見た目を「成り」にする
                yield return new WaitForSeconds(5.0f);
                break;
            // --- ▲ ここまで ▲ ---

            case ItemType.Kakoi: // 囲い（仮）：5秒間ちょっと加速
                playerProgressor.SetSpeedMultiplier(1.2f);
                yield return new WaitForSeconds(5.0f);
                break;

            case ItemType.Kokaku: // 降格：3秒間速度半分
                playerProgressor.SetSpeedMultiplier(0.5f);
                yield return new WaitForSeconds(3.0f);
                break;
        }

        // 効果時間が終了したらリセット処理を呼ぶ
        ResetAllEffects();
        currentEffectCoroutine = null;
    }

    private void UpdateUI()
    {
        if (currentItem == ItemType.None)
        {
            itemIconImage.sprite = iconNone;
            itemButton.interactable = false;
        }
        else
        {
            itemButton.interactable = true;
            switch (currentItem)
            {
                case ItemType.Totsugeki: itemIconImage.sprite = iconTotsugeki; break;
                case ItemType.Nari:      itemIconImage.sprite = iconNari; break;
                case ItemType.Kakoi:     itemIconImage.sprite = iconKakoi; break;
                case ItemType.Kokaku:    itemIconImage.sprite = iconKokaku; break;
            }
        }
    }
}