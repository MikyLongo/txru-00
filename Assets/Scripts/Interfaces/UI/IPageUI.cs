//Interface to define a UI container.
using UnityEngine;

public interface IPageUI
{
    //Method to call when the "Pause/Menu" button is pressed.
    public void OnPausePage();

    // Method to call to determine which UI element should have focus when the container opens or regains focus.
    public GameObject GetFirstSelected(); 
}
