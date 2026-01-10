using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ItemType { None, Totsugeki, Nari, Kakoi, Kokaku }

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [Header("プレイヤー参照")]
    [SerializeField] private CourseProgressor playerProgressor;
    
    [Header("UI参照")]
    [SerializeField] private Button itemButton;
    [SerializeField] private Image itemIconImage;

    [Header("アイテム画像素材")]
    public Sprite iconTotsugeki;
    public Sprite iconNari;
    public Sprite iconKakoi;
    public Sprite iconKokaku;
    public Sprite iconNone;

    private ItemType currentItem = ItemType.None;
    private Coroutine currentEffectCoroutine = null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        if (itemButton != null) itemButton.onClick.AddListener(UseItem);
    }

    // プレイヤーがアイテムを取得した時に呼ばれる
    public void GiveItemToPlayer(ItemType type)
    {
        if (currentItem != ItemType.None) return; // 既に持っていたら取れない
        
        currentItem = type;
        UpdateUI();
        Debug.Log($"プレイヤーがアイテム【{type}】を取得！");
    }

    // UIボタンを押した時に実行
    public void UseItem()
    {
        if (currentItem == ItemType.None) return;

        if (currentEffectCoroutine != null) StopCoroutine(currentEffectCoroutine);
        currentEffectCoroutine = StartCoroutine(ApplyEffect(playerProgressor, currentItem));

        currentItem = ItemType.None;
        UpdateUI();
    }

    // アイテムの効果内容
    public IEnumerator ApplyEffect(CourseProgressor target, ItemType type)
    {
        if (target == null) yield break;

        switch (type)
        {
            case ItemType.Totsugeki:
                target.SetSpeedMultiplier(2.0f);
                yield return new WaitForSeconds(3.0f);
                break;
            case ItemType.Nari:
                target.PromotePiece();
                yield return new WaitForSeconds(5.0f);
                break;
            case ItemType.Kakoi:
                target.SetSpeedMultiplier(1.2f);
                yield return new WaitForSeconds(5.0f);
                break;
            case ItemType.Kokaku:
                target.SetSpeedMultiplier(0.5f);
                yield return new WaitForSeconds(3.0f);
                break;
        }

        // 効果終了
        target.SetSpeedMultiplier(1.0f);
        target.DemotePiece();
        currentEffectCoroutine = null;
    }

    private void UpdateUI()
    {
        if (itemIconImage == null || itemButton == null) return;

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