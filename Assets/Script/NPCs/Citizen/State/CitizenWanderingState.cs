public class CitizenWanderingState : IState
{
    private CitizenNPC npc;

    public CitizenWanderingState(CitizenNPC npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        if (npc.Obstacle != null) npc.Obstacle.carving = false;
        npc.SetNavigationMode(useAgent: true);

        if (npc.Agent.enabled && npc.Agent.isOnNavMesh)
        {
            npc.Agent.speed = npc.WalkSpeed;
        }

        npc.SetAnimBool(CitizenNPC.IsIdleingHash, false);
        npc.SetAnimBool(CitizenNPC.IsWalkingHash, true);
    }

    public void Update()
    {
        // 灵敏地检测是否到达了随机目的地
        if (npc.Agent.enabled && !npc.Agent.pathPending && npc.Agent.remainingDistance <= npc.Agent.stoppingDistance)
        {
            npc.ChangeToState(npc.StayingState, NPCState.Staying);
        }
    }

    public void Exit()
    { }

    public void Dispose()
    { }
}