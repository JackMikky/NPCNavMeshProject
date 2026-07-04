using UnityEngine;

public class VipNPC : NPCBase
{
    [SerializeField] private Animator anim;

    private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
    }

    protected override void OnSetupBehavior()
    {
        SetNavigationMode(useAgent: false);

        if (anim != null)
        {
            anim.SetBool(IsTalkingHash, true);
            anim.SetBool(IsWalkingHash, false);
        }

        Debug.Log($"[{name}] VIP is in position and started speaking...");
    }

    public override void OnInteracted()
    {
        base.OnInteracted();

        Debug.LogWarning("Warning: You attacked or interfered with the VIP! All guards are on high alert!");
    }

    public void TriggerPanic()
    {
        if (anim != null)
        {
            anim.SetBool(IsTalkingHash, false); // stop talking
        }
        Debug.Log("VIP panicked and stopped speaking!");
    }
}