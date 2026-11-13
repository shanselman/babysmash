using System.Runtime.InteropServices;

namespace BabySmash.Linux.Platform;

public static class PlatformInfo
{
    public static bool IsMacOs => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static string DisplayName => IsMacOs ? "macOS" : IsLinux ? "Linux" : "Desktop";
}
