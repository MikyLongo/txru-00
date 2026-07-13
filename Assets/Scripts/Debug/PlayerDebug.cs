using UnityEngine;

[ExecuteInEditMode]
public class PlayerDebug : MonoBehaviour
{
    [SerializeField] private Player player = null;
    [SerializeField] private bool showBoundPoints = true;
    [SerializeField] private Color boundPointColor = Color.magenta;
    [SerializeField] private float boundPointRadius = 0.01f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnDrawGizmos()
    {
        if (player != null && player.gameObject.activeSelf && showBoundPoints)
        {
            Gizmos.color = boundPointColor;

            foreach (Transform point in player.BoundPoints)
            {
                Gizmos.DrawSphere(point.position, boundPointRadius);
            }
        }
    }
}
