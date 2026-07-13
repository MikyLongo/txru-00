/*
 * Custom IVariable implementation to handle the DateTime type.
 */
using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class DateTimeVariable : IVariable
{
    public DateTime Value { get; set; }
    public object GetSourceValue(ISelectorInfo selector)
    {
        return Value;
    }
}
