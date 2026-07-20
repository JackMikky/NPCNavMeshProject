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
    [SerializeField] private float actionChance = 0.35f;

    public override void Enter(NPCBase npc)
    {
        npc.SetNavigationMode(false);
        if (npc.Anim != null)
        {
            npc.Anim.SetBool(AnimationConstants.IsWalking, false);
            npc.Anim.SetBool(AnimationConstants.IsIdleing, true);
        }

        if (npc is CitizenNPC citizen)
        {
            if (availableAnimationTriggers != null && availableAnimationTriggers.Length > 0)
            {
                citizen.cachedIdleAnimationHashes = new int[availableAnimationTriggers.Length];

                for (int i = 0; i < availableAnimationTriggers.Length; i++)
                {
                    citizen.cachedIdleAnimationHashes[i] = Animator.StringToHash(availableAnimationTriggers[i]);
                }
            }

            UpdateNextActionTime(citizen);
        }
    }

    public override void UpdateBehavior(NPCBase npc)
    {
        if (npc.Anim == null) return;

        if (npc is CitizenNPC citizen)
        {
            if (citizen.cachedIdleAnimationHashes == null || citizen.cachedIdleAnimationHashes.Length == 0)
                return;

            if (Time.time >= citizen.nextIdleActionTime)
            {
                if (Random.value <= actionChance)
                {
                    int randomIndex = Random.Range(0, citizen.cachedIdleAnimationHashes.Length);
                    int selectedTriggerHash = citizen.cachedIdleAnimationHashes[randomIndex];

                    npc.Anim.SetTrigger(selectedTriggerHash);
                }

                UpdateNextActionTime(citizen);
            }
        }
    }

    public override void Exit(NPCBase npc)
    {
        if (npc is CitizenNPC citizen)
        {
            citizen.cachedIdleAnimationHashes = null;
        }
    }

    private void UpdateNextActionTime(CitizenNPC citizen)
    {
        citizen.nextIdleActionTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}