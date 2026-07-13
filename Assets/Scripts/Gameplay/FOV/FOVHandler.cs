/*
 * Class used to define and manage the Field of View (FOV) of an entity.
 * In addition to defining the FOV, it creates a mesh representing the FOV for use in the game and 
 * provides methods to determine whether a point is inside the FOV, whether the player or a guardable entity 
 * is within the FOV, and whether it is visible (not obstructed by other entities, such as buildings). 
 * See the methods IsInsideFOV, CheckFOV, and CheckGuardableFOV.
 * Additional methods are provided to "interact" with the FOV.
 *
 * There are two types of FOV:
 * 
 * 1) Pyramid
 * The FOV is structured as a pyramid with a circular base and a specified height.
 * The tip of the pyramid represents the origin point of the FOV, while the width is determined 
 * by the horizontal angle (hAngle), defining the breadth of the FOV.
 * The height is used to calculate the vertical angle (vAngle) for the IsInsideFOV check.
 * The pyramid also extends for a specific length (distance).
 *
 * Since the FOV is represented by a Mesh, which is a geometric structure composed of triangles, 
 * and the base of the FOV is circular, the Mesh must simulate this shape. 
 * To accomplish this, a specific number of segments are used to create additional triangles in the Mesh, 
 * effectively simulating the circular form. The greater the number of segments, the smoother the visual effect.
 *
 * 2) Sphere
 * The FOV is represented as a sphere with a radius defined by its distance.
 */

using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FOVHandler : MonoBehaviour
{
    //FOV settings
    [SerializeField] private bool isSpherical = false; //false = pyramid
    [SerializeField] private float distance = 3f;
    [SerializeField] private float height = 1f;
    [SerializeField, Range(1, 360)] private float hAngle = 30f; //[1,360]
    [SerializeField] private float vAngle = 18; //[0,180[
    [SerializeField] private int segments = 10;//Higher number of segments => Better resolution of the mesh
    [SerializeField] private Mesh mesh = null; //Current mesh rendered
    [SerializeField] private Mesh sphericalMesh = null; //Reference to Unity's built-in mesh
    [SerializeField] private MeshFilter meshFilter = null;
    [SerializeField] private MeshRenderer meshRenderer = null;
    [SerializeField] private Color clearColor = Color.blue;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color alertColor = Color.red;

    //FOV check
    private Collider[] colliders = new Collider[1];
    private RaycastHit[] raycastHits = new RaycastHit[20];

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = clearColor;
        ApplyMesh();
    }

    public float Distance { get { return distance; } }
    public float Height { get { return height; } }
    public float HAngle { get { return hAngle; } }
    public float VAngle { get { return vAngle; } }

    public void ApplyMesh()
    {
        ApplyMesh(distance, hAngle, height, segments);
    }

    public void ApplyMesh(float distance, float hAngle, float height, int segments)
    {
        if (meshFilter == null) //Error
            return;

        if (mesh != null)
        {
            mesh.Clear();
        }
        else
            mesh = new Mesh();

        mesh.name = $"{transform.root.name}-FOV";

        if (isSpherical) //Sphere Mesh
        {
            mesh.vertices = sphericalMesh.vertices;
            mesh.triangles = sphericalMesh.triangles;
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            //Adjust the scale of the GameObject to achieve a spherical mesh with the correct radius
            transform.localScale = new Vector3(distance * 2, distance * 2, distance * 2);
            return;
        }

        //Pyramid Mesh
        transform.localScale = new Vector3(1, 1, 1);

        if (height > 0f) //Utilize the height to determine vAngle
        {
            /*
             * The vertical angle is calculated as twice the angle between the central lateral line 
             * (of the pyramid, at the height of the origin) and the upper lateral line.
             */
            vAngle =
                Vector3.Angle(
                        Quaternion.Euler(0, hAngle / 2, 0) * Vector3.forward * distance + Vector3.up * height / 2,
                        Quaternion.Euler(0, hAngle / 2, 0) * Vector3.forward * distance
                ) * 2;
        }
        else
            return; //Error

        //The order of vertices in a triangle determines the direction of the normal.
        //In Unity, the winding order (i.e., the sequence in which the triangle's vertices are listed)
        //is CLOCKWISE to produce outward-facing normals.

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
        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;

        float currentAngle = -hAngle / 2;
        float deltaAngle = hAngle / segments;

        for (int i = 0; i < segments; i++)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance - Vector3.up * height / 2;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance - Vector3.up * height / 2;
            topLeft = bottomLeft + Vector3.up * height;
            topRight = bottomRight + Vector3.up * height;

            //Far side (2 triangles):
            //Left triangle
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;

            //Right triangle
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            //Top (1 triangle):
            vertices[vert++] = origin;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            //Bottom (1 triangle):
            vertices[vert++] = origin;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    public void EnableMeshRenderer(bool enable)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = enable;
    }

    public void SetClear()
    {
        meshRenderer.material.color = clearColor;
    }

    public void SetWarning()
    {
        meshRenderer.material.color = warningColor;
    }

    public void SetAlert()
    {
        meshRenderer.material.color = alertColor;
    }

    public struct FOVCheckState
    {
        public bool gameOver;
        public bool boxSpotted;

        public FOVCheckState(bool gameOver, bool boxSpotted)
        {
            this.gameOver = gameOver; //true: Player spotted — Game Over | false: Player not visible
            this.boxSpotted = boxSpotted; //true: Box spotted 
        }
    }

    public struct GuardCheckState
    {
        public bool visible; //true: Guardable entity visible | false: Guardable entity not visible
        public bool boxSpotted; //true: Box spotted (covers the guardable)

        public GuardCheckState(bool visible, bool boxSpotted)
        {
            this.visible = visible;
            this.boxSpotted = boxSpotted;
        }
    }

    public bool IsInsideFOV(Vector3 point)
    {
        if(Vector3.Distance(transform.position, point) <= distance)
        {
            if(isSpherical)
                return true;

            //Pyramid check
            Vector3 pointDir = (point - transform.position).normalized;
            float pointHAngle = Vector3.Angle(pointDir, transform.forward); //Result: [0, 180]
            float pointVAngle = Vector3.Angle(pointDir, transform.up);      //Result: [0, 180]

            //Used for the validation check on vAngle:
            float minAngle = 90 - (vAngle / 2);
            float maxAngle = 180 - minAngle;

            /* 
             * UP                       UP                  UP
             * ^                        ^                   ^ 0° (relative to UP)
             * |  / FOV upper limit line|                   |  / FOV upper limit line
             * | /.                     |-.                 |-/
             * |/  \ vAngle/2           |  \ pointVAngle    |/ \ pointVAngle
             * 0------> FWD             0------> FWD        0---------> FWD
             * |\                       |\ /                |\ /
             * | \                      | ' Point           | \ Point
             * |  \                     |                   |  \ FOV lower limit line
             * |                        |                   | 180° (relative to UP)
             * 
             * vAngle/2 represents the angle between the FOV forward direction and the direction of the 
             * FOV upper limit line.
             * pointVAngle indicates the angle between the FOV upward direction and the point direction.
             * minAngle and maxAngle define the range of angles relative to the UP direction, internal 
             * to the FOV, and include both the upper and lower limit lines.
             */

            //The checks are performed on the absolute value of the angle (the sign is not needed)
            if (
                //The hAngle is divided by 2 along the forward direction, resembling: \|/ or _|_ [ | = fwd ]
                pointHAngle <= hAngle / 2 &&
                (pointVAngle >= minAngle && pointVAngle <= maxAngle)
            )
                return true;
        }

        return false;
    }

    //Default behavior for checking the presence of the player within the FOV
    public FOVCheckState CheckFOV(LayerMask searchMask, LayerMask obstructionMask)
    {
        Player player = null;

        int count = Physics.OverlapBoxNonAlloc(
            transform.position,
            new Vector3(distance, height/2, distance),
            colliders,
            transform.rotation,
            searchMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            if (colliders[i].CompareTag(EngineConf.Tag.PLAYER))
            {
                player = colliders[i].transform.root.GetComponent<Player>();
                if (!player.IsVisible)
                    return new FOVCheckState(false, false);

                break;
            }
        }

        if (player != null)
        {
            bool targetVisible;

            /*
             * If we only check the transform position of the player, we may find that the player is not visible 
             * due to an obstacle, even if only part of the body (the part containing the pivot/position) is 
             * covered.
             * To address this issue, the player has a list of BoundPoints defined for visibility checks. 
             * These points correspond to different zones of the body. The higher the number of BoundPoints, 
             * the greater the accuracy in checking each part of the body to determine if any part is visible 
             * or not.
             */

            foreach (Transform point in player.BoundPoints)
            {
                if (!IsInsideFOV(point.position))
                {
                    continue;
                }

                targetVisible = true;
                Vector3 dir = (point.position - transform.position).normalized;
                int nhit = Physics.RaycastNonAlloc(
                    transform.position, dir, raycastHits, 
                    Mathf.Min(Vector3.Distance(transform.position, point.position), distance)
                    //The raycast's maximum distance should equal the distance between the FOV origin and the
                    //point, but it must not exceed the FOV's view distance.
                );

                for (int i = 0; i < nhit; i++)
                {
                    //Debug.DrawRay(transform.position, dir * raycastHits[i].distance, Color.yellow);
                    if (EngineConf.Layer.IsInMask(raycastHits[i].transform.gameObject.layer, obstructionMask))
                    {
                        if (!EngineConf.Tag.IsInvisibleBuilding(raycastHits[i].transform.tag))
                        {
                            targetVisible = false;
                            break;
                        }
                    }
                }

                if (targetVisible)
                {
                    //Debug.DrawRay(transform.position, dir * Vector3.Distance(point.position, transform.position), Color.white, 100f);
                    
                    if (player is not PlayerBox)
                    {
                        //Debug.Log($"I can see enemy: {point.name}|{point.position}");
                        return new FOVCheckState(true,false);
                    }
                    else
                    {
                        //Debug.Log($"I can see a box: {point.name}|{point.position}");
                        if (player.IsMoving)
                        {   
                            //The player is disguised as a box but is moving, causing the enemy to recognize it as the
                            //player.

                            //Debug.Log($"The box is moving: {point.name}|{point.position}");
                            return new FOVCheckState(true, true);
                        }
                        else
                        {   //Spotted a box but cannot determine if it's a GameOver.
                            //The caller needs to extend the behavior.

                            return new FOVCheckState(false, true);
                        }
                    }
                }
            }
        }

        return new FOVCheckState(false, false);
    }

    //Default behavior for checking the presence of the Guardable within the FOV
    public GuardCheckState CheckGuardableState(IGuardable guardable, LayerMask obstructionMask)
    {
        if(IsInsideFOV(guardable.Position))
        {
            RaycastHit info;
            Vector3 dir = (guardable.Position - transform.position).normalized;
            float dist= Vector3.Distance(guardable.Position,transform.position);

            if (Physics.Raycast(transform.position, dir, out info, dist, obstructionMask))//Vision is obstructed
            {
                if (info.transform.gameObject.layer == EngineConf.Layer.PLAYER) //By player
                {
                    Player player = info.transform.root.GetComponent<Player>();

                    if (player.IsVisible)
                    {
                        if (player is PlayerBox) //Not visible due to the presence of a box (the player)
                            return new GuardCheckState(false, true);
                    }//else => player invisible = guardable visibile
                }
                else
                    return new GuardCheckState(false, false); //By buildings

            }

            return new GuardCheckState(true, false); //Visible
        }
        else
            return new GuardCheckState(false, false); //Not visible
    }
}