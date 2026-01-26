using System;
namespace BabySmash.Linux.Core.Interfaces;

public class KeyboardHookEventArgs : EventArgs
{
    public int VirtualKeyCode { get; set; }
    public char Character { get; set; }
    public bool Handled { get; set; }
}

public interface IKeyboardHookService
{
    event EventHandler<KeyboardHookEventArgs>? KeyPressed;
    bool Start();
    void Stop();
    bool IsActive { get; }
}
