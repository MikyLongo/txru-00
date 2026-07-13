using UnityEngine;

public class DashDevice : Equip
{
    [SerializeField] private float distance = 3f;
    [SerializeField] private float dashTime = 0.5f;

    public override void Use()
    {
        if(transform.root.GetComponent<Player>() is IDashable dashPlayer)
        {
            dashPlayer.Dash(distance, dashTime);
        }
    }
}
