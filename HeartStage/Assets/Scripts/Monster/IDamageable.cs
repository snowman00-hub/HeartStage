public interface IDamageable 
{
    public void OnDamage(int damage, bool isCritical = false);
    public void Die();
}
