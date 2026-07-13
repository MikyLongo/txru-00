//Interface that defines a patroller entity.
public interface IPatrol
{
    public void ChangePatrol(PatrolHandler handler, PatrolHandler.PatrolState state);
}