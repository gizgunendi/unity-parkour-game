using UnityEngine;

public class CylinderHazard : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        RobotMover mover = collision.transform.GetComponentInParent<RobotMover>();
        if (mover != null)
        {
            mover.KillRobot("GAME OVER! You hit a cylinder obstacle! Press R to restart.");
        }
    }
}