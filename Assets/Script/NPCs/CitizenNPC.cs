using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public class CitizenNPC : NPCBase
{
    public enum NPCState
    {
        Watching,    // Standing and watching the speech (non-wandering mode)
        Wandering,   // Wandering / moving
        Staying,     // Paused while wandering to rest
        Interacted   // Interacted / caught by player
    }

    [Header("State Machine Debug")]
    [SerializeField] private NPCState currentState = NPCState.Watching;

    [Header("Mode Settings")]
    [Tooltip("Enable wandering mode")]
    [SerializeField] private bool isWandering = true;

    [SerializeField] private Transform lookTarget;

    [Header("Wandering Parameters")]
    [SerializeField] private float wanderRadius = 8f;

    [SerializeField] private float minStayDuration = 3f;
    [SerializeField] private float maxStayDuration = 7f;
    [SerializeField] private float walkSpeed = 1.0f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject mark;

    [SerializeField] private Animator anim;

    // Animator parameter hashes
    private static readonly int WaveHash = Animator.StringToHash("Wave");

    private static readonly int ApplauseHash = Animator.StringToHash("Applause");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsIdleingHash = Animator.StringToHash("IsIdleing");

    private float nextActionTime;
    private float stayTimer;
    private Vector3 spawnPosition;

    [SerializeField] private float minActionInterval = 5f;
    [SerializeField] private float maxActionInterval = 15f;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        spawnPosition = transform.position;
    }

    protected override void OnSetupBehavior()
    {
        SetNextActionTime();
        if (isWandering)
        {
            // Wandering mode: pick a target and switch to moving state
            MoveToRandomTarget();
        }
        else
        {
            // Traditional mode: stay and watch the speech
            TransitionToState(NPCState.Watching);
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case NPCState.Watching:
                HandleRandomIdles();
                break;

            case NPCState.Wandering:
                HandleWanderingState();
                break;

            case NPCState.Staying:
                HandleStayingState();
                break;

            case NPCState.Interacted:
                // In Interacted state: remain idle, no pathfinding or random actions
                break;
        }
    }

    /// <summary>
    /// Core state transition center: isolate all component toggles and animation switches here.
    /// </summary>
    private void TransitionToState(NPCState newState)
    {
        // Cleanup when leaving current state (if needed)
        // ...

        // Assign new state
        currentState = newState;

        // Initialize new state
        switch (currentState)
        {
            case NPCState.Watching:
                SetNavigationMode(useAgent: false);
                if (obstacle != null) obstacle.carving = true;

                LookAtTarget();
                SetAnimBool(IsIdleingHash, true);
                SetAnimBool(IsWalkingHash, false);
                break;

            case NPCState.Wandering:
                if (obstacle != null) obstacle.carving = false;
                SetNavigationMode(useAgent: true);

                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.speed = walkSpeed;
                }
                SetAnimBool(IsIdleingHash, false);
                SetAnimBool(IsWalkingHash, true);
                break;

            case NPCState.Staying:
                SetNavigationMode(useAgent: false);
                if (obstacle != null) obstacle.carving = true;

                stayTimer = Random.Range(minStayDuration, maxStayDuration);
                LookAtTarget();
                SetAnimBool(IsIdleingHash, true);
                SetAnimBool(IsWalkingHash, false);
                break;

            case NPCState.Interacted:
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                if (obstacle != null) obstacle.carving = false;

                SetAnimBool(IsWalkingHash, false);
                SetAnimBool(IsIdleingHash, true);
                if (mark != null) mark.SetActive(true);
                TriggerAnim(WaveHash);
                break;
        }
    }

    #region Per-state per-frame logic

    private void HandleWanderingState()
    {
        if (agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            TransitionToState(NPCState.Staying);
        }
    }

    private void HandleStayingState()
    {
        stayTimer -= Time.deltaTime;
        HandleRandomIdles();

        if (stayTimer <= 0f)
        {
            // Finished resting: pick next point and go back to moving
            MoveToRandomTarget();
        }
    }

    #endregion Per-state per-frame logic

    #region Helper utilities

    private void MoveToRandomTarget()
    {
        if (agent == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomTargetPos = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (NavMesh.SamplePosition(randomTargetPos, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            // Switch navigation state first
            TransitionToState(NPCState.Wandering);

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void LookAtTarget()
    {
        if (lookTarget != null)
        {
            transform.LookAt(new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z));
        }
    }

    private void HandleRandomIdles()
    {
        if (anim == null) return;

        if (Time.time >= nextActionTime)
        {
            TriggerAnim(Random.value > 0.5f ? WaveHash : ApplauseHash);
            SetNextActionTime();
        }
    }

    private void SetNextActionTime()
    {
        nextActionTime = Time.time + Random.Range(minActionInterval, maxActionInterval);
    }

    private void SetAnimBool(int id, bool value)
    {
        if (anim != null) anim.SetBool(id, value);
    }

    private void TriggerAnim(int id)
    {
        if (anim != null) anim.SetTrigger(id);
        anim.SetBool(IsIdleingHash, false);
    }

    // Single entry point for player interaction
    public override void OnInteracted()
    {
        base.OnInteracted();
        Debug.Log("Citizen: Why are you grabbing me?!");

        // Instantly switch to the interacted state; stop all running coroutines/timers
        TransitionToState(NPCState.Interacted);
    }

    #endregion Helper utilities
}