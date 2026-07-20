using UnityEngine;
using UnityEngine.AI;

public enum NPCType
{
    Assassin,
    Citizen,
    Police,
    VIP
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCBase : MonoBehaviour
{
    protected NPCType npcType = NPCType.Citizen;
    [HideInInspector] public NPCType NpcType => npcType;

    protected StateMachine StateMachine { get; private set; }

    [Header("NavMesh Settings")]
    protected Transform target { get; private set; }

    public Transform Target => target;

    protected NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    protected NavMeshObstacle obstacle;
    public NavMeshObstacle Obstacle => obstacle;

    [Space(10)]
    protected Renderer myRenderer;

    protected Animator anim;
    public Animator Anim => anim;

    [Space(10)]
    [Header("Visual Feedback")]
    [SerializeField] private GameObject mark;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        myRenderer = GetComponentInChildren<Renderer>();
        anim = GetComponentInChildren<Animator>();
        StateMachine = new StateMachine();
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        StateMachine.Update();

        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }

    public void LookAtPlayer(Transform player)
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public virtual void Initialize()
    {
        OnSetupBehavior();
    }

    public void SetNavMeshTarget(Transform target)
    {
        this.target = target;
    }

    public void SetNavigationMode(bool useAgent)
    {
        if (useAgent)
        {
            if (obstacle != null) obstacle.enabled = false;
            if (agent != null) agent.enabled = true;
        }
        else
        {
            if (agent != null) agent.enabled = false;
            if (obstacle != null) obstacle.enabled = true;
        }
    }

    /// <summary>
    /// Safely reset all base locomotion animation state flags.
    /// Prevent blending errors where walking or idle animations play while running
    /// </summary>
    public void ResetMovementAnimationFlags()
    {
        if (anim == null) return;

        anim.SetBool(AnimationConstants.IsWalking, false);
        anim.SetBool(AnimationConstants.IsRunning, false);
        anim.SetBool(AnimationConstants.IsIdling, false);
    }

    /// <summary>
    /// Safely modify the movement speed and halt state of pathfinding entities
    /// Includes built-in safety checks for component activation status and mesh baking bounds
    /// </summary>
    public void SetAgentVelocity(float speed, bool isStopped)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = speed;
            agent.isStopped = isStopped;
        }
    }

    // ==========================================

    protected abstract void OnSetupBehavior();

    public virtual void OnInteracted()
    {
    }

    public void ShowMark(bool show)
    {
        if (mark != null) mark.SetActive(show);
    }
}