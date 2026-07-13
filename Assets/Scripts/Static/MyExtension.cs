/*
 * Static class that contains custom extension methods.
 * Note: This does not include all extension methods.
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class MyExtension 
{
    /*
     *  Button
     */

    //Text
    public static TMP_Text GetTMPText(this Button button)
    {
        return button.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public static string GetText(this Button button)
    {
        return button.gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text;
    }

    public static void SetText(this Button button, string text) 
    {
        button.gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = text;
    }

    /*
     *  Float -> Time
     */

    //Convert a float into a TimeSpan.
    //The float must represent a time value expressed in seconds and milliseconds [seconds.milliseconds].
    public static TimeSpan ToTimeSpan(this float value)
    {
        return TimeSpan.FromSeconds(value);
    }

    //Convert a float into a string representing time in the format "mm:ss:ff" (ff = milliseconds in 2 digits).
    public static string ToTimeString(this float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(value);
        int minutes = time.Minutes;
        int seconds = time.Seconds;
        int milliseconds = time.Milliseconds/10; //Convert the value to a two-digit format.

        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D2}";
    }

    /*
     *  Materials
     *  Note: Copy from Unity's Built-in Standard Shader (StandardShaderGUI.cs) with some adjustment.
     */
    public static void SetRenderingMode(this Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1; //Default

                break;
            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                material.EnableKeyword("_ALPHATEST_ON"); 
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest; //Default
                break;
            case BlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; //Default
                break;
            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; //Default
                break;
        }

        /*
         *  Note:
         *  In the inspector, the following renderQueue values can be found:
         *  - Opaque: From Shader & Geometry => 2000
         *  - Cutout: AlphaTest => 2450
         *  - Fade & Transparent: Transparent => 3000
         */
    }

    //Based on the functionality of the SetRenderingMode method.
    public static BlendMode GetRenderingMode(this Material material)
    {
        if(material.GetFloat("_ZWrite") > 0f)
        {
            if (material.IsKeywordEnabled("_ALPHATEST_ON"))
                return BlendMode.Cutout;
            else
                return BlendMode.Opaque;
        }
        else
        {
            if (material.IsKeywordEnabled("_ALPHABLEND_ON"))
                return BlendMode.Fade;
            else
                return BlendMode.Transparent;
        }
    }

    public static float GetMetallicValue(this Material material)
    {
        return material.GetFloat("_Metallic");
    }

    public static void SetMetallicValue(this Material material, float value)
    {
        material.SetFloat("_Metallic", value);
    }

    public static float GetSmoothnessValue(this Material material)
    {
        return material.GetFloat("_Glossiness");
    }

    public static void SetSmoothnessValue(this Material material, float value)
    {
        material.SetFloat("_Glossiness", value);
    }

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,   //Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent //Physically plausible transparency mode, implemented as alpha pre-multiply
    }
}
