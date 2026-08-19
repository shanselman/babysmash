using System;
using BabySmash.Linux.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class MacOsKeyboardHookService : IKeyboardHookService
{
    public event EventHandler<KeyboardHookEventArgs>? KeyPressed
    {
        add { }
        remove { }
    }

    public bool IsActive { get; private set; }

    public bool Start()
    {
        IsActive = true;
        return true;
    }

    public void DisableSystemScreenshotShortcut()
    {
    }

    public void RestoreSystemScreenshotShortcut()
    {
    }

    public void Stop()
    {
        IsActive = false;
    }
}
