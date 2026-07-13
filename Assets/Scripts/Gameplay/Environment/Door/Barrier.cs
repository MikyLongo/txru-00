/*
 * Defines a barrier (an interactable door) that fades out when opened.
 */
using System.Collections;
using UnityEngine;

public class Barrier : MonoBehaviour, IDoorInteractable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float time = 1f;
    private float defaultAlpha = 1f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultAlpha = meshRenderer.material.color.a;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Close() //Show barrier
    {
        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(BarrierOn());
        }
    }

    public void Open() //Remove barrier
    {
        if(gameObject.activeSelf)
        {
            StartCoroutine(BarrierOff());
        }
    }  

    private IEnumerator BarrierOn()
    {
        float t = 0f;
        Color color = meshRenderer.material.color;

        while (t < time)
        {
            color.a = defaultAlpha * (t / time);
            meshRenderer.material.color = color;
            yield return 0;
            t += Time.deltaTime;
        }

        color.a = defaultAlpha;
        meshRenderer.material.color = color;
    }

    private IEnumerator BarrierOff()
    {
        float t = 0f;
        Color color = meshRenderer.material.color;

        while(t < time)
        {
            color.a = defaultAlpha * (1 - t/time);
            meshRenderer.material.color = color;
            yield return 0;
            t += Time.deltaTime;
        }

        gameObject.SetActive(false);
    }
}
