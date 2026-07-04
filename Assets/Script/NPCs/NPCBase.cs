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

    protected Transform target;
    protected NavMeshAgent agent;
    protected NavMeshObstacle obstacle;
    protected Renderer myRenderer;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        myRenderer = GetComponentInChildren<Renderer>();
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

    protected void SetNavigationMode(bool useAgent)
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