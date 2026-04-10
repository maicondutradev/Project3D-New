using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public float attack1Duration = 1f;
    public float attack2Duration = 1.5f;
    public float attack2Delay = 0.3f;

    public float aoeAttackDuration = 2f;
    public float aoeAttackDelay = 0.5f;
    public float aoePrefabLifetime = 5f;

    public Transform cameraTransform;
    public InputActionReference attackAction;
    public InputActionReference attack2Action;
    public InputActionReference attack3Action;

    public GameObject attack2Prefab;
    public Transform attack2SpawnPoint;

    public GameObject aoeIndicatorPrefab;
    public GameObject aoeAttackPrefab;
    public LayerMask groundLayer;
    public float maxAoeDistance = 20f;

    private Animator animator;
    public bool IsAttacking { get; private set; }
    public bool IsAimingAoE { get; private set; }

    private GameObject currentAoeIndicator;
    private Vector3 savedAoEPosition;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (attack3Action != null && attack3Action.action != null && attack3Action.action.WasPressedThisFrame() && !IsAttacking)
        {
            ToggleAoeAiming();
        }

        if (IsAimingAoE)
        {
            UpdateAoeIndicatorPosition();

            if (attackAction != null && attackAction.action != null && attackAction.action.WasPressedThisFrame())
            {
                ConfirmAoEAttack();
            }
        }
        else if (!IsAttacking)
        {
            HandleStandardAttacks();
        }
    }

    private void HandleStandardAttacks()
    {
        if (attackAction != null && attackAction.action != null && attackAction.action.WasPressedThisFrame())
        {
            AlignPlayerWithCamera();
            StartCoroutine(AttackRoutine("Attack", false, attack1Duration));
        }
        else if (attack2Action != null && attack2Action.action != null && attack2Action.action.WasPressedThisFrame())
        {
            AlignPlayerWithCamera();
            StartCoroutine(AttackRoutine("Attack2", true, attack2Duration));
        }
    }

    private void ToggleAoeAiming()
    {
        IsAimingAoE = !IsAimingAoE;

        if (IsAimingAoE)
        {
            if (currentAoeIndicator == null && aoeIndicatorPrefab != null)
            {
                currentAoeIndicator = Instantiate(aoeIndicatorPrefab);
            }
            else if (currentAoeIndicator != null)
            {
                currentAoeIndicator.SetActive(true);
            }
        }
        else
        {
            if (currentAoeIndicator != null)
            {
                currentAoeIndicator.SetActive(false);
            }
        }
    }

    private void UpdateAoeIndicatorPosition()
    {
        if (currentAoeIndicator == null || cameraTransform == null) return;

        Ray cameraRay = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxAoeDistance, groundLayer))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxAoeDistance);
        }

        Vector3 raycastStartPosition = new Vector3(targetPoint.x, transform.position.y + 20f, targetPoint.z);

        if (Physics.Raycast(raycastStartPosition, Vector3.down, out RaycastHit groundHit, 40f, groundLayer))
        {
            currentAoeIndicator.transform.position = groundHit.point;
            currentAoeIndicator.transform.up = groundHit.normal;
            savedAoEPosition = groundHit.point;
        }
    }

    private void ConfirmAoEAttack()
    {
        IsAimingAoE = false;
        if (currentAoeIndicator != null)
        {
            currentAoeIndicator.SetActive(false);
        }

        AlignPlayerWithCamera();
        StartCoroutine(AoEAttackRoutine());
    }

    private void AlignPlayerWithCamera()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(cameraForward.normalized);
        }
    }

    private IEnumerator AttackRoutine(string triggerName, bool spawnEffect, float currentAttackDuration)
    {
        IsAttacking = true;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger(triggerName);
        }

        if (spawnEffect && attack2Prefab != null && attack2SpawnPoint != null)
        {
            yield return new WaitForSeconds(attack2Delay);
            GameObject attack2Instance = Instantiate(attack2Prefab, attack2SpawnPoint.position, attack2SpawnPoint.rotation);
            Destroy(attack2Instance, attack2Duration);

            yield return new WaitForSeconds(currentAttackDuration - attack2Delay);
        }
        else
        {
            yield return new WaitForSeconds(currentAttackDuration);
        }

        IsAttacking = false;
    }

    private IEnumerator AoEAttackRoutine()
    {
        IsAttacking = true;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Attack3");
        }

        yield return new WaitForSeconds(aoeAttackDelay);

        if (aoeAttackPrefab != null)
        {
            GameObject aoeEffect = Instantiate(aoeAttackPrefab, savedAoEPosition, Quaternion.identity);
            Destroy(aoeEffect, aoePrefabLifetime);
        }

        yield return new WaitForSeconds(aoeAttackDuration - aoeAttackDelay);

        IsAttacking = false;
    }
}