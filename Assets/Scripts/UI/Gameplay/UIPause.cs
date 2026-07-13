//Script that manages the Pause menu
using UnityEngine;
using UnityEngine.UI;

public class UIPause : MonoBehaviour, IPageUI
{
    [SerializeField] private Button bSettings;
    [SerializeField] private Button bHelp;
    [SerializeField] private Button bResume;
    [SerializeField] private Button bRestart;
    [SerializeField] private Button bExit;

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;
        bSettings.onClick.AddListener(OnSettingsClick);
        bHelp.onClick.AddListener(OnHelpClick);
        bResume.onClick.AddListener(OnResumeClick);
        bRestart.onClick.AddListener(OnRestartClick);
        bExit.onClick.AddListener(OnExitClick);
    }

    private void OnDisable()
    {
        bSettings.onClick.RemoveAllListeners();
        bHelp.onClick.RemoveAllListeners();
        bResume.onClick.RemoveAllListeners();
        bRestart.onClick.RemoveAllListeners();
        bExit.onClick.RemoveAllListeners();
    }

    public void OnSettingsClick()
    {
        UIManager.Instance.ShowSettingsUI(true, false);
    }

    public void OnHelpClick()
    {
        UIManager.Instance.ShowGameHelpUI(true);
    }

    public void OnResumeClick()
    {
        GameManager.Instance.SetPause(false);
    }

    public void OnRestartClick()
    {
        GameManager.Instance.RestartLevel();
    }

    public void OnExitClick()
    {
        GameManager.Instance.ReturnToHub();
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        OnResumeClick();
    }

    public GameObject GetFirstSelected()
    {
        return bResume.gameObject;
    }
}
