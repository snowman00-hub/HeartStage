using UnityEngine;

// 대미지 팝업 뜨는 이벤트
public class DamagePopupEvent : MonoBehaviour, IDamaged
{
    public void OnDamaged(int damage, GameObject target, bool isCritical = false)
    {
        DamagePopupManager.Instance.ShowDamage(damage, target.transform.position, isCritical);
    }
}
