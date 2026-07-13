using System.Collections;
using UnityEngine;

public class FallingFloor : MonoBehaviour, IStateSaveable
{
    [SerializeField] private float delay;

    private void OnTriggerEnter(Collider other)
    {
        if (EngineConf.Layer.IsInMask(other.gameObject.layer, EngineConf.Layer.FallingMask))
            StartCoroutine(DelayCoroutine(transform.parent.gameObject));
    }

    private IEnumerator DelayCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.intact = transform.parent.gameObject.activeSelf;

        return state;
    }


    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        transform.parent.gameObject.SetActive(_state.intact);
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public bool intact;
    }
}
