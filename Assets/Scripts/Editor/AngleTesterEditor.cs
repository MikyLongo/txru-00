using UnityEditor;

namespace MyEditor
{

    [CustomEditor(typeof(AngleTester))]
    public class AngleTesterEditor : Editor
    {
        public override void OnInspectorGUI() //Get executed when there is change in the inspector!
        {
            base.OnInspectorGUI();

            AngleTester tester = (AngleTester)target;
            tester.Calculate();
        }
    }

}
