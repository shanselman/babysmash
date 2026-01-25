namespace BabySmash.Core.Models;

/// <summary>
/// Settings keys and defaults for BabySmash
/// </summary>
public static class SettingsKeys
{
    public const string Sounds = "Sounds";
    public const string FadeAway = "FadeAway";
    public const string FadeAfter = "FadeAfter";
    public const string ClearAfter = "ClearAfter";
    public const string MouseDraw = "MouseDraw";
    public const string FacesOnShapes = "FacesOnShapes";
    public const string BitmapEffects = "BitmapEffects";
    public const string TransparentBackground = "TransparentBackground";
    public const string CursorType = "CursorType";
    public const string ForceUppercase = "ForceUppercase";
}

/// <summary>
/// Default setting values
/// </summary>
public static class SettingsDefaults
{
    public const string Sounds = "Speech";          // "Speech", "Laughter", or "None"
    public const bool FadeAway = true;
    public const double FadeAfter = 4.0;            // Seconds
    public const int ClearAfter = 30;               // Number of shapes before clearing old ones
    public const bool MouseDraw = false;
    public const bool FacesOnShapes = true;
    public const bool BitmapEffects = true;
    public const bool TransparentBackground = false;
    public const string CursorType = "Arrow";       // "Arrow" or "Hand"
    public const bool ForceUppercase = true;
}
