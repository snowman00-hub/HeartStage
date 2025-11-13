using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Slider monsterHealthSlider;
    [SerializeField] private Slider bossHealthSlider;

    private MonsterBehavior monster;
    private int maxHealth;
    private bool isBoss;

    public void Init(MonsterBehavior monster, bool isBossMonster = false)
    {
        this.monster = monster;
        isBoss = isBossMonster;

        if (monster != null && monster.GetMonsterData() != null)
        {
            maxHealth = monster.GetMonsterData().hp;

        }
    }

    public void UpdateHealthBar(int currentHP)
    {
        if (maxHealth <= 0) return;

        // 데미지를 받은 양: (최대HP - 현재HP)
        int damage = maxHealth - currentHP;

        // 데미지 받은 비율 (0에서 1까지): 데미지받은양 / 최대HP
        float damagePercentage = (float)damage / maxHealth;

        Slider targetSlider = isBoss ? bossHealthSlider : monsterHealthSlider;

        if (targetSlider != null)
        {
            targetSlider.value = damagePercentage;
        }
    }

    private void Update()
    {
        if(monster != null)
        {
            UpdateHealthBar(monster.GetCurrentHP());

            if(!monster.gameObject.activeInHierarchy || monster.GetCurrentHP() <= 0)
            {
                HideHealthBar();
            }
        }
    }

    public void ShowHealthBar()
    {
        Slider targetSlider = isBoss ? bossHealthSlider : monsterHealthSlider;
        if (targetSlider != null)
            targetSlider.gameObject.SetActive(true);
    }

    public void HideHealthBar()
    {
        if (monsterHealthSlider != null)
            monsterHealthSlider.gameObject.SetActive(false);
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(false);
    }
}
