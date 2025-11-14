using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;
    public static readonly string popupId = "damagePopup";
    public GameObject damagePopupPrefab;
    
    private void Awake()
    {
        Instance = this;
    }

    // 오브젝트 풀 생성
    private void Start()
    {
        PoolManager.Instance.CreatePool(popupId, damagePopupPrefab);
    }

    // 대미지 팝업 생성
    public void ShowDamage(int damage, Vector3 hitPoint, bool isCritical = false)
    {
        var go = PoolManager.Instance.Get(popupId);
        go.transform.SetParent(transform);
        var damagePopup = go.GetComponent<DamagePopup>();
        damagePopup.Setup(damage, hitPoint, isCritical);
    }
}