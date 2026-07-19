using UnityEngine;

[CreateAssetMenu(fileName = "NewRandomIdle", menuName = BehaviorConstants.IdleBehaviorMenuName + "RandomIdle")]
public class RandomIdleBehavior : ScriptableBehaviorBase
{
    [SerializeField] private float minInterval = 5f;
    [SerializeField] private float maxInterval = 15f;

    private float nextActionTime; // 注意：多台NPC共享SO时此变量会有冲突风险。

    // 最佳实践：如果需要精准计时，时间数据应存在 NPC 本身，SO 只做逻辑判定。

    public override void Enter(NPCBase npc)
    {
        npc.SetNavigationMode(false);
        if (npc.Anim != null) npc.Anim.SetBool("IsIdleing", true);
        UpdateNextActionTime();
    }

    public override void UpdateBehavior(NPCBase npc)
    {
        if (npc.Anim == null) return;

        if (Time.time >= nextActionTime)
        {
            int animHash = Random.value > 0.5f ?
                Animator.StringToHash("Wave") : Animator.StringToHash("Applause");

            npc.Anim.SetTrigger(animHash);
            UpdateNextActionTime();
        }
    }

    public override void Exit(NPCBase npc)
    { }

    private void UpdateNextActionTime()
    {
        nextActionTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}