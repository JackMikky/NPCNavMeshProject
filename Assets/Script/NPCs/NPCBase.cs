using UnityEngine;
using UnityEngine.AI;

public enum NPCType
{
    Assassin,
    Citizen,
    VIP
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
public abstract class NPCBase : MonoBehaviour
{
    public NPCType NPCType = NPCType.Citizen;

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

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        myRenderer = GetComponentInChildren<Renderer>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        Initialize();
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

    protected abstract void OnSetupBehavior();

    public virtual void OnInteracted()
    {
    }
}