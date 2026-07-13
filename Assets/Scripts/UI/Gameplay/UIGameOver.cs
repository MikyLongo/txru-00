//Script that manages the UI screen displayed when the player loses a level.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour, IPageUI
{
    [SerializeField] private Button bRestart;
    [SerializeField] private Button bContinue;
    [SerializeField] private Button bExit;
    [SerializeField] private bool hasContinue = false;
    [SerializeField] private Image bg;
    private Coroutine fillBG = null;
    public bool HasContinue { set { hasContinue = value; } }

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;

        if(hasContinue)
        {
            bContinue.onClick.AddListener(OnContinueClick);
        }

        bContinue.gameObject.SetActive(hasContinue);

        bRestart.onClick.AddListener(OnRestartClick);
        bExit.onClick.AddListener(OnExitClick);

        fillBG = StartCoroutine(FillBGCoroutine());
    }

    private void OnDisable()
    {
        bRestart.onClick.RemoveAllListeners();
        bContinue.onClick.RemoveAllListeners();
        bExit.onClick.RemoveAllListeners();

        FillBGDispose();
    }

    private void OnRestartClick()
    {
        GameManager.Instance.RestartLevel();
    }

    private void OnContinueClick()
    {
        LevelManager.Instance.RestartLevelWithContinue();
    }

    private void OnExitClick()
    {
        GameManager.Instance.ReturnToHub();
    }

    //Coroutine
    private IEnumerator FillBGCoroutine()
    {
        float t = 0f;

        while(t<1f)
        {
            bg.fillAmount = t;
            yield return 0;
            t += Time.unscaledDeltaTime;
        }

        bg.fillAmount = 1f;
        fillBG = null;
    }

    private void FillBGDispose()
    {
        if(fillBG != null)
        {
            StopCoroutine(fillBG);
            fillBG = null;
        }
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        //Does Nothing
    }

    public GameObject GetFirstSelected()
    {
        if (hasContinue)
        {
            return bContinue.gameObject;
        }
        else
            return bRestart.gameObject;
    }
}
