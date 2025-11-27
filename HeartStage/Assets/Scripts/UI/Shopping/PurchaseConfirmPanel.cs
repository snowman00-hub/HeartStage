using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseConfirmPanel : MonoBehaviour
{
    public static PurchaseConfirmPanel Instance;

    [SerializeField] private GameObject wholePanel;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI currentAmountText;
    [SerializeField] private Button purchaseButton;

    [SerializeField] private TextMeshProUGUI impossiblePurchaseText;

    private int tableID = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        currentSlot = null;
    }

    private void Start()
    {
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
    }

    public void Open(int shopTableID)
    {
        tableID = shopTableID;
        var shopTableData = DataTableManager.ShopTable.Get(shopTableID);
        confirmText.text = $"{shopTableData.Shop_item_name}\n을 구매하시겠습니까?";
        descText.text = shopTableData.Shop_info;

        // Shop_item_type1 이 표시되게 일단
        if (SaveLoadManager.Data.itemList.ContainsKey(shopTableData.Shop_item_type1))
        {
            currentAmountText.text = $"현재 보유량: {SaveLoadManager.Data.itemList[shopTableData.Shop_item_type1]}";
        }
        else
        {
            currentAmountText.text = "현재 보유량: 0";
        }

        wholePanel.gameObject.SetActive(true);
    }

    private ShopItemSlot currentSlot;
    public void Open(int shopTableID, ShopItemSlot slot)
    {
        tableID = shopTableID;
        currentSlot = slot;
        var shopTableData = DataTableManager.ShopTable.Get(shopTableID);
        confirmText.text = $"{shopTableData.Shop_item_name}\n을 구매하시겠습니까?";
        descText.text = shopTableData.Shop_info;

        // Shop_item_type1 이 표시되게 일단
        if (SaveLoadManager.Data.itemList.ContainsKey(shopTableData.Shop_item_type1))
        {
            currentAmountText.text = $"현재 보유량: {SaveLoadManager.Data.itemList[shopTableData.Shop_item_type1]}";
        }
        else
        {
            currentAmountText.text = "현재 보유량: 0";
        }

        wholePanel.gameObject.SetActive(true);
    }

    private void OnPurchaseButtonClicked()
    {
        if (tableID < 101001) // 기능 구입은 나중에 구현
        {
            ShowImpossiblePurchaseAsync().Forget();
            return;
        }

        var shopTableData = DataTableManager.ShopTable.Get(tableID);
        if (shopTableData.Shop_currency < 10) // 현금 구매는 나중에 구현하기
        {
            ShowImpossiblePurchaseAsync().Forget();
            return;
        }

        // 라이트 스틱, 하트 스틱으로 아이템 구매
        var purchaseItemList = shopTableData.GetValidItems();
        int currencyId = shopTableData.Shop_currency;   // LightStick 또는 HeartStick
        int price = shopTableData.Shop_price;

        // 1) 구매 가능 여부 검사
        if (!ItemInvenHelper.TryConsumeItem(currencyId, price))
        {
            ShowImpossiblePurchaseAsync().Forget(); // 구매 실패
            return;
        }

        // 2) 구매 성공 → 아이템 지급
        foreach (var item in purchaseItemList)
        {
            ItemInvenHelper.AddItem(item.id, item.amount);
        }

        // 3) 끝
        wholePanel.gameObject.SetActive(false);
        if(currentSlot != null)
        {
            currentSlot.MarkAsPurchased();
        }
    }

    private async UniTaskVoid ShowImpossiblePurchaseAsync()
    {
        var obj = impossiblePurchaseText.gameObject;
        var rt = impossiblePurchaseText.rectTransform;

        if (obj.activeSelf)
            return;

        obj.SetActive(true);

        float duration = 0.6f;
        float peakScale = 1.2f;

        // 1 -> 1.2
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float p = t / duration;
            float scale = Mathf.Lerp(1f, peakScale, p);
            rt.localScale = Vector3.one * scale;
            await UniTask.Yield();
        }

        // 1.2 -> 1
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float p = t / duration;
            float scale = Mathf.Lerp(peakScale, 1f, p);
            rt.localScale = Vector3.one * scale;
            await UniTask.Yield();
        }

        obj.SetActive(false);
        rt.localScale = Vector3.one; // 원상복구
    }

    public void Close()
    {
        wholePanel.SetActive(false);    
    }
}