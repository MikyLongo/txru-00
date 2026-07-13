using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AngleTester : MonoBehaviour
{
    [SerializeField] private bool calculate = false;
    [SerializeField] private float distance = 3f;
    [SerializeField] private float height = -1f;
    [SerializeField, Range(1, 360)] private float hAngle = 30f;
    [SerializeField] private float vAngle = 18;
    [SerializeField] private int segments = 10;//Higher number of segments => Better resolution & quality of the mesh
    [SerializeField] private Mesh mesh = null;
    [SerializeField] private Mesh sphereMesh = null;
    [SerializeField] private MeshFilter meshFilter = null;
    [SerializeField] private MeshRenderer meshRenderer = null;
    [SerializeField] private Material mater = null;
    [SerializeField] private int num = 0;
    [SerializeField] private bool pyramid = true;
    public void Calculate()
    {
        if(calculate)
        {
            if(!pyramid)
            {
                if (mesh != null)
                {
                    mesh.Clear();
                }

                mesh.name = $"{transform.root.name}-{num++}-FOV";
                mesh.vertices = sphereMesh.vertices;
                mesh.triangles = sphereMesh.triangles;
                mesh.RecalculateNormals();

                if (meshFilter == null)
                    return;
                meshFilter.mesh = mesh;
                meshRenderer.sharedMaterial = mater;
                transform.localScale = new Vector3(distance * 2, distance * 2, distance * 2);
                return;
            }



            if (mesh != null)
            {
                mesh.Clear();
            }
            else
                mesh = new Mesh();

            if (height > 0f) //Use height to calculate vAngle
            {
                /*
                 * The vertical angle is simply the angle between the central lateral line (at the height of the origin)
                 * and the upper lateral line, multiplied by two!
                 */
                vAngle =
                    Vector3.Angle(
                            Quaternion.Euler(0, hAngle / 2, 0) * Vector3.forward * distance + Vector3.up * height / 2,
                            Quaternion.Euler(0, hAngle / 2, 0) * Vector3.forward * distance
                    ) * 2;
            }
            else //Use vAngle to calculate height
            {
                //Third Trigonometric Theorem on the Right-Angled Triangle (multiplied by two)
                height = distance * Mathf.Tan(Mathf.Deg2Rad * vAngle / 2) * 2;
            }

            //int numTriangles = 6; //2 far + 1 left + 1 right + 1 top + 1 bottom
            int numTriangles = (segments * 4) + 2; //(For each segment: 2 far + 1 top + 1 bottom!) + 1 left + 1 right
            int numVertices = numTriangles * 3;

            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            Vector3 origin = Vector3.zero;
            Vector3 bottomLeft = Quaternion.Euler(0, -hAngle / 2, 0) * Vector3.forward * distance - Vector3.up * height / 2;
            Vector3 bottomRight = Quaternion.Euler(0, hAngle / 2, 0) * Vector3.forward * distance - Vector3.up * height / 2;
            Vector3 topLeft = bottomLeft + Vector3.up * height;
            Vector3 topRight = bottomRight + Vector3.up * height;

            int vert = 0;
            //Left side  (1 triangle)
            vertices[vert++] = origin;
            vertices[vert++] = bottomLeft;
            vertices[vert++] = topLeft;

            //Right side (1 triangle)
            vertices[vert++] = origin;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            float currentAngle = -hAngle / 2;
            float deltaAngle = hAngle / segments;

            for (int i = 0; i < segments; i++)
            {
                bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance - Vector3.up * height / 2;
                bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance - Vector3.up * height / 2;
                topLeft = bottomLeft + Vector3.up * height;
                topRight = bottomRight + Vector3.up * height;

                //Far side (2 triangles)
                vertices[vert++] = bottomLeft;
                vertices[vert++] = topLeft;
                vertices[vert++] = topRight;

                vertices[vert++] = topRight;
                vertices[vert++] = bottomRight;
                vertices[vert++] = bottomLeft;

                //Top (1 triangle)
                vertices[vert++] = origin;
                vertices[vert++] = topLeft;
                vertices[vert++] = topRight;

                //Bottom (1 triangle)
                vertices[vert++] = origin;
                vertices[vert++] = bottomLeft;
                vertices[vert++] = bottomRight;

                currentAngle += deltaAngle;
            }

            for (int i = 0; i < numVertices; i++)
            {
                triangles[i] = i;
            }

            mesh.name = $"{transform.root.name}-{num++}-FOV";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            if (meshFilter == null)
                return;
            meshFilter.mesh = mesh;
            meshRenderer.sharedMaterial = mater;

            calculate = false;
        }
    }
}
