using UnityEngine;

public class Screenshot : MonoBehaviour
{
    [SerializeField] private bool screen = false;
    [SerializeField] private string path;
    [SerializeField] private string screenshotName;
    [SerializeField] private int resMultiplier = 1;
    [SerializeField] private int startingNum = 0;


    private void Update()
    {
        if(screen)
        {
            screen = false;
            ScreenCapture.CaptureScreenshot($"{path}/{screenshotName}-{startingNum}.png",resMultiplier);
            startingNum++;
        }
    }
}
