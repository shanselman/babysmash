namespace BabySmash.Linux.Core.Animation;

/// <summary>
/// Defines the set of easing equation types.
/// Based on Robert Penner's easing equations.
/// </summary>
public enum TransitionType
{
    EaseNone,
    EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseOutInQuad,
    EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseOutInCubic,
    EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseOutInQuart,
    EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseOutInQuint,
    EaseInSine, EaseOutSine, EaseInOutSine, EaseOutInSine,
    EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseOutInCirc,
    EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseOutInExpo,
    EaseInElastic, EaseOutElastic, EaseInOutElastic, EaseOutInElastic,
    EaseInBack, EaseOutBack, EaseInOutBack, EaseOutInBack,
    EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseOutInBounce
}
