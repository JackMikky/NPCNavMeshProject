using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AssassinNPC : NPCBase
{
    public enum AssassinState
    {
        Threatening,
        Staying,
        Approaching,
        Rushing,
        NavLinking,
        Interacted
    }

    [Header("State Machine Debug")]
    [SerializeField] private AssassinState currentState = AssassinState.Threatening;

    [Header("Speed and Distance Settings")]
    public float walkSpeed = 1.2f;

    public float runSpeed = 2.8f;

    [Tooltip("Start running when distance to VIP is less than this")]
    public float startRunningDistance = 4.0f;

    [Header("Stay Time Settings")]
    [Tooltip("How long to idle after the threatening animation finishes")]
    [SerializeField] private float stayDuration = 2.5f;

    private float stayTimer;

    [Header("Off-mesh Link (NavLink) Settings")]
    [Tooltip("Ensure the trigger name in the Animator matches this string")]
    [SerializeField] private string navLinkAnimTrigger = "Jumping";

    private int navLinkAnimHash;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject mark;

    [SerializeField] private Animator anim;

    // Animation hash cache
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsIdleingHash = Animator.StringToHash("IsIdleing");
    private static readonly int ThreateningHash = Animator.StringToHash("Threatening");

    protected override void Awake()
    {
        base.Awake();
        navLinkAnimHash = Animator.StringToHash(navLinkAnimTrigger);
    }

    protected override void OnSetupBehavior()
    {
        SetNavigationMode(useAgent: true);
        LookAtTargetNPC();

        if (agent != null) agent.autoTraverseOffMeshLink = false;

        TransitionToState(AssassinState.Threatening);
    }

    private void Update()
    {
        switch (currentState)
        {
            case AssassinState.Threatening:
                break;

            case AssassinState.Staying:
                HandleStayingState();
                break;

            case AssassinState.Approaching:
                if (CheckAndTransitionToNavLink()) return;

                HandleDistanceCheck();
                SyncMovementAnimation();
                break;

            case AssassinState.Rushing:
                if (CheckAndTransitionToNavLink()) return;

                HandleDistanceCheck();
                SyncMovementAnimation();
                break;

            case AssassinState.NavLinking:
                break;

            case AssassinState.Interacted:
                break;
        }
    }

    private void TransitionToState(AssassinState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case AssassinState.Threatening:
                SetAgentVelocity(0f, isStopped: true);
                if (anim != null)
                {
                    anim.SetTrigger(ThreateningHash);
                    StartCoroutine(WaitForAnimationToFinish());
                }
                else
                {
                    TransitionToState(AssassinState.Staying);
                }
                break;

            case AssassinState.Staying:
                SetAgentVelocity(0f, isStopped: true);
                stayTimer = stayDuration;
                ResetMovementAnimationFlags();
                if (anim != null) anim.SetBool(IsIdleingHash, true);
                break;

            case AssassinState.Approaching:
                SetAgentVelocity(walkSpeed, isStopped: false);
                ResetMovementAnimationFlags();
                if (anim != null) anim.SetBool(IsWalkingHash, true);
                SetDestinationToTarget();
                break;

            case AssassinState.Rushing:
                SetAgentVelocity(runSpeed, isStopped: false);
                ResetMovementAnimationFlags();
                if (anim != null) anim.SetBool(IsRunningHash, true);
                SetDestinationToTarget();
                break;

            case AssassinState.NavLinking:
                // Detach regular movement animations and trigger the jump/climb animation
                ResetMovementAnimationFlags();
                if (anim != null) anim.SetTrigger(navLinkAnimHash);

                // Start coroutine to perform a smooth off-mesh link traversal
                StartCoroutine(TraverseNavLinkCoroutine());
                break;

            case AssassinState.Interacted:
                StopAllCoroutines();
                SetAgentVelocity(0f, isStopped: true);
                if (mark != null) mark.SetActive(true);
                ResetMovementAnimationFlags();
                if (anim != null) anim.SetBool(IsIdleingHash, true);
                break;
        }
    }

    #region Per-frame state core logic

    private void HandleStayingState()
    {
        stayTimer -= Time.deltaTime;
        LookAtTargetNPC();
        if (stayTimer <= 0f)
        {
            TransitionToState(AssassinState.Approaching);
        }
    }

    private bool CheckAndTransitionToNavLink()
    {
        // Use isOnOffMeshLink to detect contact with an off-mesh link
        if (agent != null && agent.enabled && agent.isOnNavMesh && agent.isOnOffMeshLink)
        {
            TransitionToState(AssassinState.NavLinking);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Custom off-mesh link traversal coroutine that performs a smooth arc/position interpolation
    /// </summary>
    private IEnumerator TraverseNavLinkCoroutine()
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos;

        // Face the landing point
        Vector3 lookTarget = new Vector3(endPos.x, transform.position.y, endPos.z);
        transform.LookAt(lookTarget);

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            // Base horizontal interpolation between start and end
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);

            float jumpHeight = 1.2f; // Peak jump height
            currentPos.y += Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;

            transform.position = currentPos;
            yield return null;
        }

        // Ensure final position exactly matches the NavMesh end point
        transform.position = endPos;

        agent.CompleteOffMeshLink();

        Debug.Log("【StateMachine】Assassin landed from off-mesh link and resumed pursuit.");

        // After landing, re-evaluate distance and switch back to walking or running
        float distanceToVip = Vector3.Distance(transform.position, target.position);
        if (distanceToVip <= startRunningDistance)
        {
            TransitionToState(AssassinState.Rushing);
        }
        else
        {
            TransitionToState(AssassinState.Approaching);
        }
    }

    private void HandleDistanceCheck()
    {
        if (target == null) return;

        float distanceToVip = Vector3.Distance(transform.position, target.position);

        if (currentState == AssassinState.Approaching && distanceToVip <= startRunningDistance)
        {
            TransitionToState(AssassinState.Rushing);
        }
        else if (currentState == AssassinState.Rushing && distanceToVip > startRunningDistance)
        {
            TransitionToState(AssassinState.Approaching);
        }

        SetDestinationToTarget();
    }

    private void SyncMovementAnimation()
    {
        if (anim == null || agent == null || !agent.enabled) return;

        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0, agent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (currentSpeed > 0.15f)
        {
            bool shouldRun = currentState == AssassinState.Rushing && currentSpeed > (walkSpeed * 1.1f);
            ResetMovementAnimationFlags();

            if (shouldRun)
                anim.SetBool(IsRunningHash, true);
            else
                anim.SetBool(IsWalkingHash, true);
        }
        else
        {
            ResetMovementAnimationFlags();
            anim.SetBool(IsIdleingHash, true);
        }
    }

    private void ResetMovementAnimationFlags()
    {
        if (anim == null) return;
        anim.SetBool(IsWalkingHash, false);
        anim.SetBool(IsRunningHash, false);
        anim.SetBool(IsIdleingHash, false);
    }

    #endregion Per-frame state core logic

    #region Helper utilities

    private IEnumerator WaitForAnimationToFinish()
    {
        yield return new WaitForSeconds(0.1f);

        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("Threatening") || stateInfo.shortNameHash == ThreateningHash)
            {
                float remainingTime = stateInfo.length * (1f - Mathf.Clamp01(stateInfo.normalizedTime));
                yield return new WaitForSeconds(remainingTime + 0.2f);

                if (currentState == AssassinState.Threatening)
                {
                    TransitionToState(AssassinState.Staying);
                }
            }
            else
            {
                yield return new WaitForSeconds(2.5f);
                if (currentState == AssassinState.Threatening)
                {
                    TransitionToState(AssassinState.Staying);
                }
            }
        }
    }

    public void StartMovingAfterThreaten()
    {
        if (currentState != AssassinState.Threatening) return;
        TransitionToState(AssassinState.Staying);
    }

    private void SetAgentVelocity(float speed, bool isStopped)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = speed;
            agent.isStopped = isStopped;
        }
    }

    private void SetDestinationToTarget()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void LookAtTargetNPC()
    {
        if (target != null)
        {
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }
    }

    public override void OnInteracted()
    {
        base.OnInteracted();
        TransitionToState(AssassinState.Interacted);
    }

    #endregion Helper utilities
}