using UnityEngine;

public class AssassinNPC : NPCBase
{
    private StateMachine stateMachine;

    public ThreateningState ThreateningState { get; private set; }
    public AssassinStayingState StayingState { get; private set; }
    public AssassinApproachingState ApproachingState { get; private set; }
    public AssassinRushingState RushingState { get; private set; }
    public AssassinNavLinkingState NavLinkingState { get; private set; }
    public AssassinInteractedState InteractedState { get; private set; }

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

    [Header("Off-mesh Link (NavLink) Settings")]
    [Tooltip("Ensure the trigger name in the Animator matches this string")]
    [SerializeField] private string navLinkAnimTrigger = "Jumping";

    public int NavLinkAnimHash { get; private set; }

    [Header("Visual Feedback")]
    [SerializeField] private GameObject mark;

    #region Animation hash cache

    public static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    public static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    public static readonly int IsIdleingHash = Animator.StringToHash("IsIdleing");
    public static readonly int ThreateningTriggerHash = Animator.StringToHash("Threatening");

    #endregion Animation hash cache

    protected override void Awake()
    {
        base.Awake();
        this.npcType = NPCType.Assassin;
        NavLinkAnimHash = Animator.StringToHash(navLinkAnimTrigger);

        // Initialize the state machine and the various state classes
        stateMachine = new StateMachine();
        ThreateningState = new ThreateningState(this);
        StayingState = new AssassinStayingState(this, this.stayDuration);
        ApproachingState = new AssassinApproachingState(this);
        RushingState = new AssassinRushingState(this);
        NavLinkingState = new AssassinNavLinkingState(this);
        InteractedState = new AssassinInteractedState(this);
    }

    protected override void OnSetupBehavior()
    {
        SetNavigationMode(useAgent: true);
        LookAtTargetNPC();

        if (agent != null) agent.autoTraverseOffMeshLink = false;

        ChangeToState(ThreateningState, AssassinState.Threatening);
    }

    private void Update()
    {
        stateMachine.Update();

        if (stateMachine.CurrentState == ApproachingState || stateMachine.CurrentState == RushingState)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh && agent.isOnOffMeshLink)
            {
                ChangeToState(NavLinkingState, AssassinState.NavLinking);
            }
        }
    }

    /// <summary>
    /// Wrapper to change state and update debug enum field
    /// </summary>
    public void ChangeToState(IState newState, AssassinState enumState)
    {
        stateMachine.ChangeState(newState);
        currentState = enumState;
    }

    #region Helpers exposed for states

    public void TargetDistanceCheckAndTransition()
    {
        if (target == null) return;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= startRunningDistance)
            ChangeToState(RushingState, AssassinState.Rushing);
        else
            ChangeToState(ApproachingState, AssassinState.Approaching);
    }

    public void ResetMovementAnimationFlags()
    {
        if (anim == null) return;
        anim.SetBool(IsWalkingHash, false);
        anim.SetBool(IsRunningHash, false);
        anim.SetBool(IsIdleingHash, false);
    }

    public void SetAgentVelocity(float speed, bool isStopped)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = speed;
            agent.isStopped = isStopped;
        }
    }

    public void SetDestinationToTarget()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    public void SyncMovementAnimation()
    {
        if (anim == null || agent == null || !agent.enabled) return;

        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0, agent.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (currentSpeed > 0.15f)
        {
            bool shouldRun = (stateMachine.CurrentState == RushingState) && currentSpeed > (walkSpeed * 1.1f);
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

    private void LookAtTargetNPC()
    {
        if (target != null)
        {
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }
    }

    public void StartMovingAfterThreaten()
    {
        if (stateMachine.CurrentState == ThreateningState)
        {
            ChangeToState(StayingState, AssassinState.Staying);
        }
    }

    #endregion Helpers exposed for states

    public override void OnInteracted()
    {
        base.OnInteracted();
        ChangeToState(InteractedState, AssassinState.Interacted);
    }
}