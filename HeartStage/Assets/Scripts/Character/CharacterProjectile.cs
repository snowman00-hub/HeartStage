using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public enum PenetrationType
{
    NonPenetrate,  // 비관통
    Penetrate      // 관통
}

public class CharacterProjectile : MonoBehaviour
{
    private string id;
    private string hitEffectId;
    private int damage;
    private float moveSpeed;
    private Vector3 dir;
    private PenetrationType penetrationType = PenetrationType.NonPenetrate;

    private bool isReleased = false; // 중복 Release 방지용
    private CancellationTokenSource cts;

    private void OnEnable()
    {
        isReleased = false;

        cts = new CancellationTokenSource();

        AutoReleaseAfterDelay(10f, cts.Token).Forget();
    }

    private void OnDisable()
    {
        isReleased = true;
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetMissile(string id,string hitEffectId, Vector3 startPos,  Vector3 dir, float speed, int dmg, PenetrationType penetration = PenetrationType.NonPenetrate)
    {
        this.id = id;
        this.hitEffectId = hitEffectId;
        transform.position = startPos;
        this.dir = dir;
        moveSpeed = speed;
        damage = dmg;
        penetrationType = penetration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            var monsterBehavior = collision.GetComponent<MonsterBehavior>();
            monsterBehavior.OnDamage(damage);

            if(penetrationType == PenetrationType.NonPenetrate)
                ReleaseToPool();

            if(hitEffectId != string.Empty)
                HitEffectAsync().Forget();
        }
    }

    private async UniTask HitEffectAsync()
    {
        var hitGo = PoolManager.Instance.Get(hitEffectId);
        hitGo.transform.position = transform.position;

        var particle = hitGo.GetComponent<ParticleSystem>();
        particle.Play();

        await UniTask.WaitUntil(() => particle == null || particle.IsAlive() == false);

        if (hitGo != null)
        {
            PoolManager.Instance.Release(hitEffectId, hitGo);
        }
    }

    private async UniTaskVoid AutoReleaseAfterDelay(float delay, CancellationToken token)
    {
        try
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: token);
            ReleaseToPool();
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void ReleaseToPool()
    {
        if (isReleased)
            return;

        isReleased = true;
        PoolManager.Instance.Release(id, gameObject);
    }
}
