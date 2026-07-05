public class CitizenWatchingState : IState
{
    private CitizenNPC npc;

    public CitizenWatchingState(CitizenNPC npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        npc.SetNavigationMode(useAgent: false);
        if (npc.Obstacle != null) npc.Obstacle.carving = true;

        npc.LookAtTarget();
        npc.SetAnimBool(CitizenNPC.IsIdleingHash, true);
        npc.SetAnimBool(CitizenNPC.IsWalkingHash, false);
    }

    public void Update()
    {
        // 观看演讲时，偶尔鼓掌或挥手
        npc.HandleRandomIdles();
    }

    public void Exit()
    { }

    public void Dispose()
    { }
}