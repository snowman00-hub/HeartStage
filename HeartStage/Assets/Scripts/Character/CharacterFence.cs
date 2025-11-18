using TMPro;
using UnityEngine;
public class CharacterFence : MonoBehaviour, IDamageable
{
    public static CharacterFence Instance;

    public TextMeshProUGUI currentHPText;

    private int maxHp = 0;
    private int hp = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        int prevMaxHp = maxHp;
        int prevHp = hp;

        // 캐릭터 HP 총합 구하기
        int totalHp = 0;
        var characters = GameObject.FindGameObjectsWithTag(Tag.Tower);

        foreach (var character in characters)
        {
            var characterAttack = character.GetComponent<CharacterAttack>();
            var baseData = DataTableManager.CharacterTable.Get(characterAttack.id);

            int finalHp = Mathf.FloorToInt(
                StatCalc.GetFinalStat(character, StatType.MaxHp, baseData.char_hp)
            );

            totalHp += finalHp;
        }

        maxHp = totalHp;

        //  HP 증가량만큼 현재 체력 증가시키기
        int delta = maxHp - prevMaxHp;

        hp = prevHp + delta;

        if (hp > maxHp)
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