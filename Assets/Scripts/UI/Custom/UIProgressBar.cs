//Script that handles a Progress Bar
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text tProgress;
    private void Start()
    {
        progressBar.fillAmount = 0;
        tProgress.text = "0%";
    }

    public void UpdateFill(float amount) //Amount within the range [0, 1]
    {
        progressBar.fillAmount = amount;
        tProgress.text = $"{amount*100}%";
    }
}
