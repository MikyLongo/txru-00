/*
 * An entity may contain body parts that are damageable and others that are not.
 * Additionally, different body parts can utilize distinct damage handlers.
 * This interface defines a damageable body component and specifies how to retrieve the damage handler.
 * For more information, refer to IDamageable.
 */

public interface IDamageableComponent 
{
    public IDamageable GetDamageable(); //Returns the damage handler
}
