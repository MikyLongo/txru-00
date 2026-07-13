/*
 * Custom formatter for smart strings handling the DateTime type in the Localization System.
 * Note: Ensure it is added to the localization settings for proper functionality.
 */
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

public class DateTimeFormatter : FormatterBase
{
    public override string[] DefaultNames => new string[] { "dateformatter", "" };

    /*
     * Note: The "" formatter is required for the DateTimeFormatter to function correctly.
     * Without it, the formatter will not work, and this detail is not mentioned in the documentation.
     * Ensure to add this formatter in the project settings:
     * Localization > String Database > Smart Format > Formatters, and place it before the Default Formatter.
     */

    public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is DateTime dateTime)
        {
            string formattedDate = dateTime.ToString(
                //Retrieve the CultureInfo of the currently selected language.
                LocalizationSettings.SelectedLocale.Identifier.CultureInfo.DateTimeFormat
                );
            formattingInfo.Write(formattedDate);
            return true;
        }
        return false;
    }
}

