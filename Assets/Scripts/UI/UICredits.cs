/*
 * Script that manages the Credits screen.
 * The screen is implemented using a ScrollView controlled by the UICustomScrollView script.
 * The ScrollView starts scrolling after a specified delay and continues for a defined duration, 
 * displaying all the credits vertically (like a classic credits screen).
 */

using UnityEngine;

public class UICredits : MonoBehaviour, IPageUI
{
    [SerializeField] private UICustomScrollView scrollView;
    [SerializeField] private float time = 5f;
    [SerializeField] private float startingDelay = 1f;

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;
        scrollView.ScrollYTimed(time, startingDelay);
    }

    //Implementation of the IPageUI interface
    public GameObject GetFirstSelected()
    {
        return null;
    }

    public void OnPausePage()
    {
        UIManager.Instance.ShowCreditsUI(false);
    }
}
