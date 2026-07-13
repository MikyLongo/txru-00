/*
 *  Script used to define FOV of entity in edit mode!
 */

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(FOVHandler),typeof(MeshFilter))]
public class FOVViewer : MonoBehaviour
{
    [SerializeField] private bool createMesh = false;
    [SerializeField] private bool showMesh = true;
    [SerializeField] private FOVHandler FOV;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Mesh lastMesh = null;

    public void Draw()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (showMesh)
        {

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = lastMesh;
            }
        }
        else
        {
            if (meshFilter.sharedMesh != null)
            {
                lastMesh = meshFilter.sharedMesh;
                meshFilter.sharedMesh = null;
            }
        }

        if (createMesh)
        {
            if (FOV == null)
                FOV = GetComponent<FOVHandler>();

            FOV.ApplyMesh();
            showMesh = true;
            createMesh = false;
        }
    }
}
