using UnityEngine;

// 대미지 받았을 때 이벤트
public interface IDamaged
{
    void OnDamaged(int damage, GameObject target, bool isCritical = false);
}