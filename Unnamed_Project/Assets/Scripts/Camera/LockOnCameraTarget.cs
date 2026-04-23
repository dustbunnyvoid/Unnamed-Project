using UnityEngine;

public class LockOnCameraTarget : MonoBehaviour
{
    public Transform player;
    public float enemyWeight = 0.5f;

    public float smoothSpeed = 5f; // higher = faster, lower = slower

    LockOnSystem lockOn;

    void Start()
    {
        lockOn = player.GetComponent<LockOnSystem>();
    }

    void LateUpdate()
    {
        Vector3 targetPosition;

        if (lockOn == null || lockOn.TargetEnemy == null)
        {
            targetPosition = player.position;
        }
        else
        {
            targetPosition = Vector3.Lerp(
                player.position,
                lockOn.TargetEnemy.transform.position,
                enemyWeight
            );
        }

        // Smoothly move toward target position over time
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}