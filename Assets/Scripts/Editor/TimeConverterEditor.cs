/*
 * A script that allow us to use the TimeConvert tool in the inspector/editor mode!
 * With this configuring the records of every level will be easy!
 * It's function is get from the TimeConverter script!
 * In the "Tools" folder there is an existing prefab to use this tools!
 */
using UnityEditor;

namespace MyEditor
{

    [CustomEditor(typeof(TimeConverter))]
    public class TimeConverterEditor : Editor
    {
        public override void OnInspectorGUI() //Get executed when there is change in the inspector!
        {
            base.OnInspectorGUI();

            TimeConverter converter = (TimeConverter)target;
            converter.Convert();
        }
    }

}
