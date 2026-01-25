using Avalonia;
using System;
using System.Threading;

namespace BabySmash.Linux;

class Program
{
    private static Mutex? _singleInstanceMutex;
    
    [STAThread]
    public static void Main(string[] args)
    {
        // Ensure single instance
        _singleInstanceMutex = new Mutex(true, "BabySmashLinuxSingleInstance", out bool createdNew);
        
        if (!createdNew)
        {
            // Another instance is already running
            Console.WriteLine("BabySmash is already running.");
            return;
        }
        
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
