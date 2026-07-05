using UnityEngine;

public class CitizenStayingState : BaseTimedState<CitizenNPC>
{
    public CitizenStayingState(CitizenNPC npc, float duration) : base(npc, duration)
    {
    }

    public override void Enter()
    {
        this.duration = Random.Range(npc.MinStayDuration, npc.MaxStayDuration);
        base.Enter();

        npc.SetNavigationMode(useAgent: false);
        if (npc.Obstacle != null) npc.Obstacle.carving = true;

        npc.LookAtTarget();
        npc.SetAnimBool(CitizenNPC.IsIdleingHash, true);
        npc.SetAnimBool(CitizenNPC.IsWalkingHash, false);
    }

    public override void Update()
    {
        base.Update();

        npc.HandleRandomIdles();
    }

    protected override void OnTimeOut()
    {
        npc.MoveToRandomTarget();
    }
}