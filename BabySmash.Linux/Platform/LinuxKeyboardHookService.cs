using System;
using BabySmash.Core.Interfaces;

namespace BabySmash.Linux.Platform;

public class LinuxKeyboardHookService : IKeyboardHookService
{
    public event EventHandler<KeyboardHookEventArgs> KeyPressed;
    
    public bool IsActive { get; private set; }

    public bool Start()
    {
        // TODO: Implement X11/Wayland keyboard hooks
        IsActive = true;
        return true;
    }

    public void Stop()
    {
        IsActive = false;
    }
    
    // For now, we'll use Avalonia's normal keyboard events
    public void SimulateKeyPress(char character)
    {
        KeyPressed?.Invoke(this, new KeyboardHookEventArgs 
        { 
            Character = character,
            VirtualKeyCode = character
        });
    }
}
