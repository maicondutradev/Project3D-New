using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnSystem : MonoBehaviour
{
    public float lockOnRadius = 15f;
    public LayerMask enemyLayer;
    public InputActionReference lockOnAction;
    public ThirdPersonCamera cameraScript;

    public Transform CurrentTarget { get; private set; }

    void Update()
    {
        // The null check for lockOnAction.action is already present here.
        if (lockOnAction != null && lockOnAction.action != null && lockOnAction.action.WasPressedThisFrame())
        {
            if (CurrentTarget == null)
            {
                FindClosestTarget();
            }
            else
            {
                ClearTarget();
            }
        }

        if (CurrentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            if (distance > lockOnRadius || !CurrentTarget.gameObject.activeInHierarchy)
            {
                ClearTarget();
            }
        }
    }

    private void FindClosestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = col.transform;
            }
        }

        if (closestTarget != null)
        {
            CurrentTarget = closestTarget;
            if (cameraScript != null)
            {
                cameraScript.SetLockOnTarget(CurrentTarget);
            }
        }
    }

    private void ClearTarget()
    {
        CurrentTarget = null;
        if (cameraScript != null)
        {
            cameraScript.ClearLockOnTarget();
        }
    }
}