// ItemManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ItemType { None, Totsugeki, Nari, Kakoi, Kokaku }

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
    private Coroutine currentEffectCoroutine = null;

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

        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
            ResetAllEffects();
        }

        currentEffectCoroutine = StartCoroutine(ApplyItemEffect(currentItem));

        currentItem = ItemType.None;
        UpdateUI();
    }

    private void ResetAllEffects()
    {
        playerProgressor.SetSpeedMultiplier(1.0f);
        playerProgressor.DemotePiece(); 
    }

    private IEnumerator ApplyItemEffect(ItemType type)
    {
        switch (type)
        {
            case ItemType.Totsugeki: // 突撃：3秒間、速度2倍（アイテム倍率で対応）
                playerProgressor.SetSpeedMultiplier(2.0f);
                yield return new WaitForSeconds(3.0f);
                break;

            case ItemType.Nari: // 成り：5秒間
                // ▼ 修正：速度の強制変更(SetSpeedMultiplier)を削除しました
                // コマ側で設定された PromotedSpeedMultiplier が適用されます
                playerProgressor.PromotePiece(); 
                yield return new WaitForSeconds(5.0f);
                break;

            case ItemType.Kakoi: // 囲い：5秒間
                playerProgressor.SetSpeedMultiplier(1.2f);
                yield return new WaitForSeconds(5.0f);
                break;

            case ItemType.Kokaku: // 降格：3秒間
                playerProgressor.SetSpeedMultiplier(0.5f);
                yield return new WaitForSeconds(3.0f);
                break;
        }

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