using UnityEngine;

public class SaveDataCustomizer : MonoBehaviour
{
    [SerializeField] private int numSlot = 11;
    [SerializeField] private bool getCurrentMemory = false;
    [SerializeField] private bool saveData = false;
    [SerializeField] private GameData.GameMemory memory;


    private void OnValidate()
    {
        if(getCurrentMemory)
        {
            getCurrentMemory = false;
            memory = GameManager.Instance.Memory;
            return;
        }

        if(saveData)
        {
            saveData = false;
            GameSaver.SaveGame(memory, numSlot);
            return;
        }
    }
}
