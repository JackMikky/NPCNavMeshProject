using UnityEngine;

[CreateAssetMenu(fileName = "NewFlexibleIdle", menuName = BehaviorConstants.IdleBehaviorMenuName + "FlexibleIdle")]
public class FlexibleIdleBehavior : ScriptableBehaviorBase
{
    [Header("Animation Trigger List")]
    [SerializeField] private string[] availableAnimationTriggers;

    [Header("Action Trigger Time Interval")]
    [SerializeField] private float minInterval = 5f;

    [SerializeField] private float maxInterval = 15f;

    [Header("Probability Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float actionChance = 0.3f;

    public override void Enter(NPCBase npc)
    {
        npc.SetNavigationMode(false);
        if (npc.Anim != null)
        {
            npc.Anim.SetBool("IsWalking", false);
            npc.Anim.SetBool("IsIdleing", true);
        }

        if (npc is CitizenNPC citizen)
        {
            UpdateNextActionTime(citizen);
        }
    }

    public override void UpdateBehavior(NPCBase npc)
    {
        if (npc.Anim == null || availableAnimationTriggers == null || availableAnimationTriggers.Length == 0)
            return;

        if (npc is CitizenNPC citizen)
        {
            if (Time.time >= citizen.nextIdleActionTime)
            {
                if (Random.value <= actionChance)
                {
                    int randomIndex = Random.Range(0, availableAnimationTriggers.Length);
                    string selectedTrigger = availableAnimationTriggers[randomIndex];

                    npc.Anim.SetTrigger(selectedTrigger);
                }
                else
                {
                    // Debug.Log($"{citizen.name}:idling");
                }
                UpdateNextActionTime(citizen);
            }
        }
    }

    public override void Exit(NPCBase npc)
    { }

    private void UpdateNextActionTime(CitizenNPC citizen)
    {
        citizen.nextIdleActionTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}