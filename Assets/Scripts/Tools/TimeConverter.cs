/*
 * This script is executed only in the editor/inspector mode!
 * This script provide a tool to convert a time defined by integer in a float in the inspector.
 * With this configuring the records of every level will be easy!
 * The script TimeConverterEditor is the one that actually use this script and allow us to operate in EditMode.
 * In the "Tools" folder there is an existing prefab to use this tools!
 */
using UnityEngine;

namespace MyEditor
{
    [ExecuteInEditMode]
    public class TimeConverter : MonoBehaviour
    {
        //Inputs
        public int minutes = 0;
        public int seconds = 0;
        public int milliseconds = 0;
        //Output
        public float time;

        public void Convert() 
        {
            time = Utilities.FromIntTimeToFloat(minutes, seconds, milliseconds);
        }
    }
}
