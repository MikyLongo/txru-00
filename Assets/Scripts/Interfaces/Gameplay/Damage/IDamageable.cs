// Interface defining a damageable entity and specifying how to handle damage interactions.
public interface IDamageable 
{
    public void TakeDamage(int damage);
    public void Kill();
}
