using UnityEngine;

public class AssassinRushingState : IState
{
    private AssassinNPC npc;

    private float pathUpdateTimer;
    private const float PathUpdateInterval = 0.2f;

    public AssassinRushingState(AssassinNPC npc)
    {
        this.npc = npc;
    }

    public void Dispose()
    {
        // no-op
    }

    public void Enter()
    {
        npc.SetAgentVelocity(npc.runSpeed, isStopped: false);
        npc.ResetMovementAnimationFlags();
        if (npc.Anim != null) npc.Anim.SetBool(AssassinNPC.IsRunningHash, true);

        npc.SetDestinationToTarget();
        pathUpdateTimer = PathUpdateInterval;
    }

    public void Exit()
    {
        // no-op
    }

    public void Update()
    {
        if (npc.Target == null) return;

        float distance = Vector3.Distance(npc.transform.position, npc.Target.position);
        if (distance > npc.startRunningDistance)
        {
            npc.ChangeToState(npc.ApproachingState, AssassinState.Approaching);
            return;
        }

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = PathUpdateInterval;
            npc.SetDestinationToTarget();
        }

        npc.SyncMovementAnimation();
    }
}