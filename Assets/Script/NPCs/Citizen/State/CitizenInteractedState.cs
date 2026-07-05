public class CitizenInteractedState : IState
{
    private CitizenNPC npc;

    public CitizenInteractedState(CitizenNPC npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        if (npc.Agent != null && npc.Agent.enabled && npc.Agent.isOnNavMesh)
        {
            npc.Agent.isStopped = true;
        }
        if (npc.Obstacle != null) npc.Obstacle.carving = false;

        npc.SetAnimBool(CitizenNPC.IsWalkingHash, false);
        npc.SetAnimBool(CitizenNPC.IsIdleingHash, true);

        if (npc.Mark != null) npc.Mark.SetActive(true);
        npc.TriggerAnim(CitizenNPC.WaveHash);
    }

    public void Update()
    {
    }

    public void Exit()
    { }

    public void Dispose()
    { }
}