using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    private Vector3 direction;
    public float speed;
    public int damage;  

    public void Init(Vector3 direction, float bulletSpeed, int damage)
    {
        speed = bulletSpeed;
        this.direction = direction.normalized;
        this.damage = damage;   
    }

    private void Update()
    {
        transform.position +=  direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            var testObj = other.GetComponent<IDamageable>();
            if (testObj != null)
            {
                testObj.OnDamage(damage);
            }

            PlayHitEffect(transform.position);

            PoolManager.Instance.Release(MonsterSpawner.GetMonsterProjectilePoolId(), gameObject);
            gameObject.SetActive(false);            
        }
    }

    private void PlayHitEffect(Vector3 hitPosition)
    {
        GameObject hitEffectPrefab = ResourceManager.Instance.Get<GameObject>("monsterHitEffect");
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);

            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
                Destroy(effect, particles.main.duration + particles.main.startLifetime.constantMax);
            }
        }
    }
}
