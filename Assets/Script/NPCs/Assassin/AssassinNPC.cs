using UnityEngine;

public class AssassinNPC : NPCBase
{
    #region StateMachine

    public ThreateningState ThreateningState { get; private set; }
    public AssassinStayingState StayingState { get; private set; }
    public AssassinApproachingState ApproachingState { get; private set; }
    public AssassinRushingState RushingState { get; private set; }
    public AssassinNavLinkingState NavLinkingState { get; private set; }
    public AssassinInteractedState InteractedState { get; private set; }

    [HideInInspector] public IState previousState;
    [HideInInspector] public AssassinState previousEnumState;
    [HideInInspector] public float interactionEndTime;

    #endregion StateMachine

    [Header("Behavior Assets")]
    [SerializeField] private ScriptableBehaviorBase moveBehavior;

    public ScriptableBehaviorBase MoveBehavior => moveBehavior;

    [SerializeField] private ScriptableBehaviorBase idleBehavior;
    public ScriptableBehaviorBase IdleBehavior => idleBehavior;

    [SerializeField] private ScriptableBehaviorBase approachingBehavior;
    public ScriptableBehaviorBase ApproachingBehavior => approachingBehavior;

    [SerializeField] private ScriptableBehaviorBase interactedBehavior;
    public ScriptableBehaviorBase InteractedBehavior => interactedBehavior;

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

    #region Animation hash cache

    public static int IsWalking => AnimationConstants.IsWalking;

    public static int IsRunning => AnimationConstants.IsRunning;
    public static int IsIdleing => AnimationConstants.IsIdleing;
    public static int ThreateningTrigger => AnimationConstants.Threatening;

    #endregion Animation hash cache

    [Space(10)]
    [Header("Debug")]
    [SerializeField] private AssassinState currentState = AssassinState.Threatening;

    protected override void Awake()
    {
        base.Awake();
        this.npcType = NPCType.Assassin;
        NavLinkAnimHash = AnimationConstants.JumpingNavLinkAnim;

        // Initialize the state machine and the various state classes
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
        StateMachine.Update();

        if (StateMachine.CurrentState == ApproachingState || StateMachine.CurrentState == RushingState)
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
        StateMachine.ChangeState(newState);
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
        anim.SetBool(IsWalking, false);
        anim.SetBool(IsRunning, false);
        anim.SetBool(IsIdleing, false);
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
            bool shouldRun = (StateMachine.CurrentState == RushingState) && currentSpeed > (walkSpeed * 1.1f);
            ResetMovementAnimationFlags();

            if (shouldRun)
                anim.SetBool(IsRunning, true);
            else
                anim.SetBool(IsWalking, true);
        }
        else
        {
            ResetMovementAnimationFlags();
            anim.SetBool(IsIdleing, true);
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
        if (StateMachine.CurrentState == ThreateningState)
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