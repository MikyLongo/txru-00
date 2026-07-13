/*
 * Defines a message displayed during the looting process of an Item.
 * The message appears in the world scene, starting from the Item's position, rising upward, 
 * and disappearing after a short duration.
 * In addition to displaying a text message, it provides sound (SFX) feedback.
 */
using System.Collections;
using TMPro;
using UnityEngine;

public class ItemMessage : MonoBehaviour
{
    /*
     * The mesh of the lootable item is rendered by a child GameObject of the GameObject executing this script.
     * Upon looting, the message is displayed, and the object is made to disappear.
     */
    [SerializeField] private GameObject meshRoot;

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text textItemName;
    [SerializeField] private TMP_Text textStatus;
    [SerializeField] private float yOffset = 2f;
    
    [SerializeField] private ItemLootingAudioMapper lootingAudioMapper;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Color lootedColor = Color.green;
    [SerializeField] private Color notLootedColor = new Color(0.67f, 0.33f, 0f); //Orange

    //Localization Key
    private static readonly string LK_INV_FULL = "ILootErrorInvFull";
    private static readonly string LK_OBTAINED = "IObtained";

    private Coroutine textMessageCoroutine = null;
    private float time = 2f;

    public enum ItemMessageType
    {
        LOOTED,
        NOTLOOTED
    }

    private void Start()
    {
        canvas.worldCamera = LevelManager.Instance.MainCamera;
        textItemName.color = lootedColor;
    }

    public void Dispose()
    {
        meshRoot.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowMessage(ItemMessageType msgType)
    {
        TextMessageDispose();

        if (msgType == ItemMessageType.LOOTED)
        {
            meshRoot.SetActive(false);
            audioSource.clip = lootingAudioMapper.looted;
            audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
            audioSource.Play();
            textMessageCoroutine = StartCoroutine(TextMessageCoroutine(true));
        }
        else
        {
            audioSource.clip = lootingAudioMapper.notLooted;
            audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
            audioSource.Play();
            textMessageCoroutine = StartCoroutine(TextMessageCoroutine(false));
        }
    }

    private IEnumerator TextMessageCoroutine(bool looted)
    {
        Vector3 start = transform.position + Vector3.up * yOffset;
        Vector3 dest = start + Vector3.up * 5f;

        float t = 0f;

        if (looted)
        {
            textItemName.gameObject.SetActive(true);
            textStatus.color = lootedColor;
            textStatus.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.ITEM_TABLE, LK_OBTAINED);
        }
        else
        {
            textItemName.gameObject.SetActive(false);
            textStatus.color = notLootedColor;
            textStatus.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.ITEM_TABLE, LK_INV_FULL);
        }
        Transform cam = LevelManager.Instance.MainCamera.transform;
        //Adjust the canvas to face the camera!
        canvas.transform.LookAt(canvas.transform.position + cam.rotation * Vector3.forward, 
                                cam.rotation * Vector3.up);

        canvas.gameObject.SetActive(true);

        while (t<time)
        {
            yield return 0;
            t += Time.deltaTime; 
            canvas.transform.position = Vector3.Lerp(start, dest, t / time);
        }

        textMessageCoroutine = null;
        canvas.gameObject.SetActive(false);

        if (looted) //If Looted is true, disable the root GameObject
            gameObject.SetActive(false);
    }

    private void TextMessageDispose()
    {
        if (textMessageCoroutine != null)
        {
            StopCoroutine(textMessageCoroutine);

            canvas.gameObject.SetActive(false);
            canvas.transform.position = transform.position + Vector3.up * yOffset;

            textMessageCoroutine = null;
        }
    }
}
