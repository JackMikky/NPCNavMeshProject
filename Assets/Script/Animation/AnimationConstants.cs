using UnityEngine;

public static class AnimationConstants
{
    #region Animation Parameter Names

    public static string IsWalkingString = "IsWalking";

    public static string IsIdleingString = "IsIdleing";

    public static string WaveString = "Wave";

    public static string ExposedString = "Exposed";

    public static string IsRunningString = "IsRunning";

    public static string ThreateningString = "Threatening";

    public static string JumpingNavLinkAnimString = "Jumping";

    public static string ApplauseString = "Applause";

    public static string IsTalkingString = "IsTalking";

    #endregion Animation Parameter Names

    #region Animation Hashes

    /// <summary>Animator parameter "Walking".For all NPCs.</summary>
    public static readonly int IsWalking = Animator.StringToHash(IsWalkingString);

    /// <summary> Animator parameter "Idleing".For all NPCs.</summary>
    public static readonly int IsIdleing = Animator.StringToHash(IsIdleingString);

    /// <summary>
    /// Animator parameter "Wave".For Citizens.
    /// </summary>
    public static readonly int Wave = Animator.StringToHash(WaveString);

    /// <summary>
    /// Animator parameter "Exposed".For Assassins.
    /// </summary>
    public static readonly int Exposed = Animator.StringToHash(ExposedString);

    /// <summary>
    /// Animator parameter "IsRunning".For all NPCs.
    /// </summary>
    public static readonly int IsRunning = Animator.StringToHash(IsRunningString);

    /// <summary>
    /// Animator parameter "Threatening".For Assassins.
    /// </summary>
    public static readonly int Threatening = Animator.StringToHash(ThreateningString);

    /// <summary>
    /// Animator parameter "Jumping".For all NPCs.
    /// </summary>
    public static readonly int JumpingNavLinkAnim = Animator.StringToHash(JumpingNavLinkAnimString);

    /// <summary>
    /// Animator parameter "Applause".For Citizens.
    /// </summary>
    public static readonly int Applause = Animator.StringToHash(ApplauseString);

    /// <summary>
    /// Animator parameter "IsTalking". For VIPs.
    /// </summary>
    public static readonly int IsTalking = Animator.StringToHash(IsTalkingString);

    #endregion Animation Hashes
}