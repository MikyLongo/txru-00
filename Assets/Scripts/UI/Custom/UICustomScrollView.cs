/*
 * Script that manages and adds custom behavior to ScrollView.
 * Can be used with a ScrollView (ScrollRect) or a DropDown Menu.
 * The main functionality of the script is to provide a way to scroll the bar when UI navigation 
 * does not allow it (e.g., moving between UI elements using the keyboard or gamepad instead of a mouse).
 * 
 * Functionality:
 * - Manages scrolling on the Y-axis:
 *   This script may not work with all ScrollViews, as it is highly dependent on the configuration of the 
 *   content and its UI elements (pivot, anchors, stretch mode, etc.).
 *   However, it works properly in its current use case within the project (tested with content using a 
 *   stretch top configuration).
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UICustomScrollView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int numSteps = 2; //The scrolling process can be divided into steps
    private Coroutine smoothCoroutine = null;
    private Coroutine timedYCoroutine = null;
    
    public int NumSteps { get { return numSteps; } set { numSteps = value; } }

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void OnDisable()
    {
        DisposeSmoothCoroutine();
        DisposeTimedYCoroutine();
    }

    //Custom ScrollView
    public void ScrollY(float valueY, float minOffset, float maxOffset)
    {
        ScrollY(valueY, minOffset, maxOffset, numSteps);
    }

    public void ScrollY(float valueY, float minOffset, float maxOffset, int numSteps)
    {
        if (content == null || numSteps < 2) //Cannot work
        {
            Debug.LogWarning(
                $"UICustomScrollView: Scroll cannot function.\ncontent {(content == null ? "null" : "not null")} | numSteps: {numSteps}"
            );
            return; //Does nothing!
        }

        float maxValue = content.sizeDelta.y - maxOffset;
        float stepSize = (maxValue - minOffset) / (numSteps);
        float delta = Mathf.Abs(valueY) - minOffset; //valueY may be negative; use abs to ensure positive value

        float normalizedValue = delta / stepSize;

        int stepIndex = (int)Math.Floor(normalizedValue);
        float scaledStepSize = 1.0f / numSteps; //Scales stepSize to the range [0, 1]

        float outValue = stepIndex * scaledStepSize;
        
        scrollRect.verticalNormalizedPosition = 1 - outValue; //Gets the complement (Up=1 to Down=0)
    }

    //Scrolls by centering the element within the ScrollView viewport with a smooth scrolling effect
    public void ScrollYCentered(float valueY, float elementHeight)
    {
        float windowHeight = GetComponent<RectTransform>().rect.height;
        float scrollHeight = content.sizeDelta.y;

        float y = Mathf.Abs(valueY);
        float maxY = scrollHeight - elementHeight;

        float diff = windowHeight - elementHeight; 

        float scrollDest = 0;

        //If the position is within a distance from 0 that is less than the free space in the viewport
        if (y < diff / 2) 
        {
            scrollDest = 1f;
        }
        else if(maxY - y < diff/2)//If the position is within a distance from maxY that is less than
        {                         //the free space in the viewport
            scrollDest = 0f;
        }
        else
        {
            //Center
            float elemCenter = y + elementHeight / 2;
            float outValue = (elemCenter - windowHeight / 2) / (scrollHeight - windowHeight);
            scrollDest = 1f - outValue; //Gets the complement (Up=1 to Down=0)
        }

        DisposeSmoothCoroutine();
        smoothCoroutine = StartCoroutine(SmoothCoroutine(
            0.1f, 
            scrollRect.verticalNormalizedPosition, 
            scrollDest
        ));
    }

    //Scrolls from top to bottom using a timer
    public void ScrollYTimed(float time, float startingDelay)
    {
        DisposeTimedYCoroutine();
        timedYCoroutine = StartCoroutine(TimedYCoroutine(time, startingDelay));
    }

    //Custom DropDown
    public void DDScrollY(int index, int numOptions)
    {
        float stepSize = 1f / (numOptions-1);
        scrollRect.verticalNormalizedPosition = 1 - index * stepSize;
    }

    //Coroutines
    private IEnumerator SmoothCoroutine(float time, float start, float end)
    {
        float t = 0;
        float scale = 1f / time;

        while (t <= time)
        {
            yield return 0;
            t += Time.unscaledDeltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, end, t * scale);
        }
        smoothCoroutine = null;
    }

    private void DisposeSmoothCoroutine()
    {
        if(smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }
    }

    private IEnumerator TimedYCoroutine(float time, float startingDelay)
    {
        float t = 0f;
        float scale = 1f / time;

        scrollRect.verticalNormalizedPosition = 1f; //Top

        while(t < startingDelay)
        {
            yield return 0;
            t += Time.unscaledDeltaTime;
        }

        t = 0f;

        //Scrolls from Top to Bottom
        while(t < time)
        {
            yield return 0;
            t += Time.unscaledDeltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(0, 1, 1 - t * scale);
        }

        timedYCoroutine = null;
        scrollRect.verticalNormalizedPosition = 0f; //Bottom
    }

    private void DisposeTimedYCoroutine()
    {
        if(timedYCoroutine != null)
        {
            StopCoroutine(timedYCoroutine);
            timedYCoroutine = null;
        }
    }
}
