//Script that manages the Loading Screen and provides access to the Progress Bar (see UIProgressBar).
using UnityEngine;

public class UILoadingScreen : MonoBehaviour, IPageUI
{
    [SerializeField] private UIProgressBar progressBar;

    public UIProgressBar GetProgressBar()
    {
        return progressBar;
    }

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        return; //Does nothing
    }

    public GameObject GetFirstSelected()
    {
        return null; //No UI elements are selectable
    }
}
