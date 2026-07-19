using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "NewWanderingMove", menuName = BehaviorConstants.MoveBehaviorMenuName + "WanderingMove")]
public class WanderingMoveBehavior : ScriptableBehaviorBase
{
    [SerializeField] private float wanderRadius = 5f;

    public override void Enter(NPCBase npc)
    {
        if (npc.Agent == null) return;
        if (npc.Anim == null) return;
        npc.SetNavigationMode(true);

        npc.Anim.SetBool(this.defaultAnimationTrigger, true);

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += npc.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);
        npc.Agent.SetDestination(hit.position);
    }

    public override void Exit(NPCBase npc)
    {
    }

    public override void UpdateBehavior(NPCBase npc)
    {
    }
}