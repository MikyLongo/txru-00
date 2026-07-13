/*
 * This script manages the view of the minimap.
 * The minimap is an orthographic camera placed at the top of the scene, showing the scene from above.
 * The area covered by the minimap is a rectangle defined by the top, left, right, and bottom borders (corners).
 * The script follows the player's position, ensuring that the camera does not trespass the borders.
 * Note: The camera will render its content to a texture, which will be used by a canvas to display the minimap.
 */
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private float leftBorder;
    [SerializeField] private float rightBorder;
    [SerializeField] private float bottomBorder;
    [SerializeField] private float topBorder;

    private void Start()
    {
        Vector2 topLeft = LevelManager.Instance.TopLeft;
        Vector2 bottomRight = LevelManager.Instance.BottomRight;
        leftBorder = topLeft.x;
        rightBorder = bottomRight.x;
        topBorder = topLeft.y;
        bottomBorder = bottomRight.y;
    }

    private void LateUpdate()
    {
        Transform player = PlayerManager.Instance.Player.transform;
        Vector3 position = new Vector3(
            Mathf.Clamp(player.position.x,leftBorder,rightBorder),
            transform.position.y,
            Mathf.Clamp(player.position.z, bottomBorder, topBorder)
        );
        transform.position = position;
    }

    public float LeftBorder { get { return leftBorder; } set {  leftBorder = value; } }
    public float RightBorder { get { return rightBorder; } set { rightBorder = value; } }
    public float BottomBorder { get { return bottomBorder; } set { bottomBorder = value; } }
    public float TopBorder { get { return topBorder; } set {  topBorder = value; } }
}
