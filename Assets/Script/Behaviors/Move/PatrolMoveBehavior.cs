using UnityEngine;

[CreateAssetMenu(fileName = "NewPatrolMove", menuName = BehaviorConstants.MoveBehaviorMenuName + "PatrolMove")]
public class PatrolMoveBehavior : ScriptableMoveBehavior
{
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.2f;

    public override void Enter(NPCBase npc)
    {
        if (npc.Agent == null) return;

        npc.SetNavigationMode(true);
        npc.Agent.speed = patrolSpeed;
        npc.Agent.stoppingDistance = stoppingDistance;

        if (npc.Anim != null)
        {
            npc.Anim.SetBool("IsWalking", true);
            npc.Anim.SetBool("IsIdleing", false);
        }
    }

    public override void UpdateBehavior(NPCBase npc)
    {
        if (npc.Agent == null || npc.Target == null) return;

        if (npc.Agent.enabled && npc.Agent.isOnNavMesh)
        {
            npc.Agent.SetDestination(npc.Target.position);
        }
    }

    public override void Exit(NPCBase npc)
    {
        if (npc.Agent == null) return;

        if (npc.Agent.enabled && npc.Agent.isOnNavMesh)
        {
            npc.Agent.ResetPath();
        }
    }
}