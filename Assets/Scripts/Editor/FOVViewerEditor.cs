using UnityEditor;

namespace MyEditor
{
    [CustomEditor(typeof(FOVViewer))]
    public class FOVViewerEditor : Editor
    {
        public override void OnInspectorGUI() //Get executed when there is change in the inspector!
        {
            base.OnInspectorGUI();

            FOVViewer viewer = (FOVViewer)target;
            viewer.Draw();
        }
    }
}
