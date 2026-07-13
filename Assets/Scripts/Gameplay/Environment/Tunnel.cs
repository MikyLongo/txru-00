/*
 * Script that defines a Tunnel and its behavior with the player when they are a box (PlayerBox).
 * A tunnel is a small passage designed to be accessible only by small entities like a box. For this reason,
 * the tunnel will appear as a hole-like structure covered on the lateral sides and the top side.
 * 
 * For gameplay purposes, the tunnel is always visible on the minimap as a semi-transparent area, allowing
 * the player to see their position inside the tunnel. Additionally, we want the player to understand their
 * position within the tunnel not only through the minimap but also through the screen/main camera view.
 * To achieve this, we enable and disable the mesh renderer of a building block as needed, making the player
 * visible through the screen/main camera.
 * 
 * The organization of the GameObject for the Tunnel passage follows this structure:
 * - A trigger collider to define the hole that represents the tunnel, controlled by this script.
 * - Building blocks with mesh renderers on the sides covering the tunnel, following these rules:
 *   - The top side of the tunnel must have two mesh renderers: one with the layer "Building" and one with the 
 *     layer "BuildingNoMinimap." The first will be rendered on the minimap and must be semi-transparent, 
 *     allowing the player to see their position and recognize the presence of a tunnel. 
 *     The second will not be rendered on the minimap but will be rendered on the screen/main camera, 
 *     requiring a solid material to show a proper wall/building covering the semi-transparent wall.
 * - The sides of the tunnel that block the view from the screen/main camera (preventing visibility of the 
 *   player inside the tunnel) must have the MeshRenderer referenced in this script. These sides should 
 *   have the layer "BuildingNoMinimap" to ensure enabling/disabling their visibility does not affect the 
 *   minimap.
 */
using UnityEngine;

public class Tunnel : MonoBehaviour
{
    [SerializeReference] MeshRenderer meshRenNoMinimap = null;

    private void OnTriggerEnter(Collider other)
    {
        if (meshRenNoMinimap == null)
            return;

        if(other.gameObject.layer == EngineConf.Layer.PLAYER && other.CompareTag(EngineConf.Tag.PLAYER_BODY_PART))
        {
            if(PlayerManager.Instance.Player is PlayerBox)
            {
                meshRenNoMinimap.enabled = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (meshRenNoMinimap == null)
            return;

        if (other.gameObject.layer == EngineConf.Layer.PLAYER && other.CompareTag(EngineConf.Tag.PLAYER_BODY_PART))
        {
            if (PlayerManager.Instance.Player is PlayerBox)
            {
                meshRenNoMinimap.enabled = true;
            }
        }
    }
}
