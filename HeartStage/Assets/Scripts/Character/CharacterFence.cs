using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class CharacterFence : MonoBehaviour, IDamageable
{
    public static CharacterFence Instance;

    public TextMeshProUGUI currentHPText;
    public Transform imageGo;

    private int maxHp = 0;
    private int hp = 0;

    // 흔들기 효과
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 10f;
    private CancellationTokenSource shakeCts;
    private Vector3 originalPos;
    private bool isShaking = false;

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
        StartShake();

        if (hp <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        StageManager.Instance.Defeat();
    }

    private void StartShake()
    {
        shakeCts?.Cancel();
        shakeCts = new CancellationTokenSource();
        ShakeAsync(shakeCts.Token).Forget();
    }

    // 흔들기
    private async UniTask ShakeAsync(CancellationToken token)
    {
        if (!isShaking)
        {
            // 최초 흔들기 시작할 때만 원래 위치 저장
            originalPos = imageGo.localPosition;
            isShaking = true;
        }

        float timer = 0f;

        try
        {
            while (timer < shakeDuration)
            {
                if (token.IsCancellationRequested)
                    return;

                float y = Mathf.Sin(timer * 50f) * shakeMagnitude;
                imageGo.localPosition = new Vector3(originalPos.x, originalPos.y + y, originalPos.z);

                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                imageGo.localPosition = originalPos;
                isShaking = false;
            }
        }
    }

}