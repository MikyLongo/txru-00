/*
 * Identifies GameObjects/Scripts that have a state that can be saved or loaded.
 * The generic state is defined by the IState interface.
 */

public interface IStateSaveable 
{
    IState SaveState();
    void LoadState(IState state);
}