using TMPro;
using UnityEngine;
public class CharacterFence : MonoBehaviour, IDamageable
{
    public TextMeshProUGUI currentHPText;

    private int maxHp = 1000;
    private int hp;

    public void Init(int maxHp)
    {
        this.maxHp = maxHp;
        hp = maxHp;
        SetHpText();
    }

    public void SetHpText()
    {
        currentHPText.text = $"HP: {hp} / {maxHp}";
    }

    public void OnDamage(int damage, bool isCritical = false)
    {
        hp -= damage;
        SetHpText();
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

    }
}