using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public class CitizenNPC : NPCBase
{
    public StateMachine StateMachine { get; private set; }

    public CitizenWatchingState WatchingState { get; private set; }
    public CitizenWanderingState WanderingState { get; private set; }
    public CitizenStayingState StayingState { get; private set; }
    public CitizenInteractedState InteractedState { get; private set; }

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

    #region Animation hash cache

    public static readonly int WaveHash = Animator.StringToHash("Wave");
    public static readonly int ApplauseHash = Animator.StringToHash("Applause");
    public static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    public static readonly int IsIdleingHash = Animator.StringToHash("IsIdleing");

    #endregion Animation hash cache

    private float nextActionTime;
    private Vector3 spawnPosition;

    [SerializeField] private float minActionInterval = 5f;
    [SerializeField] private float maxActionInterval = 15f;

    public float WalkSpeed => walkSpeed;

    public float MinStayDuration => minStayDuration;
    public float MaxStayDuration => maxStayDuration;
    public GameObject Mark => mark;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        spawnPosition = transform.position;

        StateMachine = new StateMachine();
        WatchingState = new CitizenWatchingState(this);
        WanderingState = new CitizenWanderingState(this);
        StayingState = new CitizenStayingState(this, 0f);
        InteractedState = new CitizenInteractedState(this);
    }

    protected override void OnSetupBehavior()
    {
        SetNextActionTime();
        if (isWandering)
        {
            MoveToRandomTarget();
        }
        else
        {
            ChangeToState(WatchingState, NPCState.Watching);
        }
    }

    private void Update()
    {
        StateMachine.Update();
    }

    /// <summary>
    /// A wrapper function for state transitions that also updates the Debug enum in the Inspector
    /// </summary>
    public void ChangeToState(IState newState, NPCState enumState)
    {
        StateMachine.ChangeState(newState);
        currentState = enumState;
    }

    #region Helper utilities

    public void MoveToRandomTarget()
    {
        if (agent == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomTargetPos = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (NavMesh.SamplePosition(randomTargetPos, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            ChangeToState(WanderingState, NPCState.Wandering);

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    public void LookAtTarget()
    {
        if (lookTarget != null)
        {
            transform.LookAt(new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z));
        }
    }

    public void HandleRandomIdles()
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

    public void SetAnimBool(int id, bool value)
    {
        if (anim != null) anim.SetBool(id, value);
    }

    public void TriggerAnim(int id)
    {
        if (anim != null) anim.SetTrigger(id);
        anim.SetBool(IsIdleingHash, false);
    }

    public override void OnInteracted()
    {
        base.OnInteracted();
        Debug.Log("Citizen: Why are you grabbing me?!");

        ChangeToState(InteractedState, NPCState.Interacted);
    }

    #endregion Helper utilities
}