using UnityEngine;

public class CitizenNPC : NPCBase
{
    #region StateMachine

    public StateMachine StateMachine { get; private set; }

    public CitizenWatchingState WatchingState { get; private set; }
    public CitizenWalkingState WalkingState { get; private set; }
    public CitizenStayingState StayingState { get; private set; }

    public CitizenPanicState PanicState { get; private set; }

    public CitizenInteractedState InteractedState
    { get; private set; }

    [HideInInspector] public IState previousState;
    [HideInInspector] public NPCState previousEnumState;
    [HideInInspector] public float interactionEndTime;

    #endregion StateMachine

    public CitizenType CitizenType = CitizenType.Audience;

    [Header("Stay Setting")]
    public float minStayDuration = 2f;

    public float maxStayDuration = 5f;

    [Header("Behavior Assets")]
    [SerializeField] private ScriptableBehaviorBase moveBehavior;

    public ScriptableBehaviorBase MoveBehavior => moveBehavior;

    [SerializeField] private ScriptableBehaviorBase idleBehavior;
    public ScriptableBehaviorBase IdleBehavior => idleBehavior;

    [SerializeField] private ScriptableBehaviorBase panicBehavior;
    public ScriptableBehaviorBase PanicBehavior => panicBehavior;

    [SerializeField] private ScriptableBehaviorBase interactedBehavior;
    public ScriptableBehaviorBase InteractedBehavior => interactedBehavior;

    [HideInInspector] public float nextIdleActionTime;

    [HideInInspector] public ScriptableBehaviorBase activePanicBehavior;

    [Space(10)]
    [Header("Debug")]
    [SerializeField] private NPCState currentState = NPCState.Watching;

    protected override void Awake()
    {
        base.Awake();
        this.npcType = NPCType.Citizen;
        StateMachine = new StateMachine();

        WatchingState = new CitizenWatchingState(this);
        WalkingState = new CitizenWalkingState(this);
        StayingState = new CitizenStayingState(this, 0f);
        InteractedState = new CitizenInteractedState(this);
        PanicState = new CitizenPanicState(this);
    }

    protected override void OnSetupBehavior()
    {
        switch (this.CitizenType)
        {
            case CitizenType.Audience:
                ChangeToState(WatchingState, NPCState.Watching);
                break;

            case CitizenType.Passerby:
                ChangeToState(WalkingState, NPCState.Wandering);
                break;

            case CitizenType.None:
            default:
                break;
        }
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public void ChangeToState(IState newState, NPCState enumState)
    {
        StateMachine.ChangeState(newState);
        currentState = enumState;
    }

    public override void OnInteracted()
    {
        base.OnInteracted();

        if (StateMachine.CurrentState != InteractedState)
        {
            previousState = StateMachine.CurrentState;
            previousEnumState = currentState;
        }

        ChangeToState(InteractedState, NPCState.Interacted);
        Debug.Log($"{this.gameObject.name}:I'm citizen");
    }
}