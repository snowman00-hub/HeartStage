using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEditor.Animations;
using UnityEngine;

public enum LobbyActionType
{
    Idle,
    Walk,
    Run,
    AttackPractice,
    Meditation,
    Surprised,
}

[Serializable]
public class LobbyAction
{
    public LobbyActionType actionType;
    public float weight;

    [Min(0f)]
    public float durationMin = 2f;
    [Min(0f)]
    public float durationMax = 4f;

    public float GetRandomDuration()
    {
        return UnityEngine.Random.Range(durationMin, durationMax);
    }
}

public class LobbyCharacterAI : MonoBehaviour
{
    private static readonly int HashIdle = Animator.StringToHash("Idle");
    private static readonly int HashWalk = Animator.StringToHash("Walk");
    private static readonly int HashRun = Animator.StringToHash("Run");
    private static readonly int HashAttackPractice = Animator.StringToHash("AttackPractice");
    private static readonly int HashMeditation = Animator.StringToHash("Meditation");
    private static readonly int HashSurprised = Animator.StringToHash("Surprised");

    public AnimatorController aniController;
    public float walkSpeed = 2f;
    public float runSpeed = 3f;

    public float afterDelayMin = 0.3f;
    public float afterDelayMax = 1.2f;

    [SerializeField] private LobbyAction[] actions;
    private Animator animator;
    private bool isRunning = true;
    private Tweener moveTween;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        animator.runtimeAnimatorController = aniController;
        AiLoopAsync().Forget();
    }

    private void OnDestroy()
    {
        isRunning = false;
        KillMove();
    }

    private async UniTaskVoid AiLoopAsync()
    {
        while (isRunning)
        {
            LobbyAction action = GetWeightedRandomAction();
            float duration = action.GetRandomDuration();

            ExecuteAction(action.actionType, duration);

            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            // 행동 끝, 잠깐 대기
            KillMove();
            animator.SetTrigger(HashIdle);

            float wait = UnityEngine.Random.Range(afterDelayMin, afterDelayMax);
            await UniTask.Delay(TimeSpan.FromSeconds(wait));
        }
    }

    private LobbyAction GetWeightedRandomAction()
    {
        float total = 0f;
        foreach (var a in actions)
        {
            total += a.weight;
        }

        float rand = UnityEngine.Random.Range(0f, total);
        float current = 0f;

        foreach (var a in actions)
        {
            current += a.weight;

            if (rand <= current)
                return a;
        }

        return actions[0];
    }

    private void ExecuteAction(LobbyActionType type, float duration)
    {
        ResetAllTriggers();

        switch (type)
        {
            case LobbyActionType.Idle:
                KillMove();
                animator.SetTrigger(HashIdle);
                break;
            case LobbyActionType.Walk:
                animator.SetTrigger(HashWalk);
                MoveRandomDirection(walkSpeed, duration);
                break;
            case LobbyActionType.Run:
                animator.SetTrigger(HashRun);
                MoveRandomDirection(runSpeed, duration);
                break;
            case LobbyActionType.AttackPractice:
                KillMove();
                animator.SetTrigger(HashAttackPractice);
                break;
            case LobbyActionType.Meditation:
                KillMove();
                animator.SetTrigger(HashMeditation);
                break;
            case LobbyActionType.Surprised:
                KillMove();
                animator.SetTrigger(HashSurprised);
                break;
        }
    }

    private void KillMove()
    {
        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();
    }

    private void MoveRandomDirection(float speed, float duration)
    {
        KillMove();

        Bounds bounds = DragZoomPanManager.Instance.BackgroundBounds;

        Vector2 randomDir2D = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 randomDir3D = new(randomDir2D.x, randomDir2D.y, 0f);
        Vector3 target = transform.position + randomDir3D * speed * duration;

        target.x = Mathf.Clamp(target.x, bounds.min.x, bounds.max.x);
        target.y = Mathf.Clamp(target.y, bounds.min.y, bounds.max.y);

        // Flip
        if (randomDir3D.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(randomDir3D.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        moveTween = transform.DOMove(target, duration).SetEase(Ease.Linear);
    }

    private void ResetAllTriggers()
    {
        animator.ResetTrigger(HashIdle);
        animator.ResetTrigger(HashWalk);
        animator.ResetTrigger(HashRun);
        animator.ResetTrigger(HashAttackPractice);
        animator.ResetTrigger(HashMeditation);
        animator.ResetTrigger(HashSurprised);
    }
}