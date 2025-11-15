using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    // CSV 필드 이름 그대로
    public int item_id;
    public string item_name;
    public string item_desc;
    public int item_type;
    public int item_use;
    public int item_drop;
    public string item_dropinfo;
    public int item_inv;
    public int item_dup;
    public bool item_canbuy;
    public int item_shop;
    public int price_type;
    public float item_price;
    public string info;
    public string prefab;

    // CSV 데이터를 ScriptableObject로 업데이트
    public void UpdateData(ItemCSVData data)
    {
        item_id = data.item_id;
        item_name = data.item_name;
        item_desc = data.item_desc;
        item_type = data.item_type;
        item_use = data.item_use;
        item_drop = data.item_drop;
        item_dropinfo = data.item_dropinfo;
        item_inv = data.item_inv;
        item_dup = data.item_dup;
        item_canbuy = data.item_canbuy;
        item_shop = data.item_shop;
        price_type = data.price_type;
        item_price = data.item_price;
        info = data.info;
        prefab = data.prefab;
    }

    // ScriptableObject 데이터를 CSV 데이터로 변환
    public ItemCSVData ToCSVData()
    {
        ItemCSVData csvData = new ItemCSVData();
        csvData.item_id = item_id;
        csvData.item_name = item_name;
        csvData.item_desc = item_desc;
        csvData.item_type = item_type;
        csvData.item_use = item_use;
        csvData.item_drop = item_drop;
        csvData.item_dropinfo = item_dropinfo;
        csvData.item_inv = item_inv;
        csvData.item_dup = item_dup;
        csvData.item_canbuy = item_canbuy;
        csvData.item_shop = item_shop;
        csvData.price_type = price_type;
        csvData.item_price = item_price;
        csvData.info = info;
        csvData.prefab = prefab;

        return csvData;
    }
}