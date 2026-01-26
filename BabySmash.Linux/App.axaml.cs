using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BabySmash.Linux.Core.Interfaces;
using BabySmash.Linux.Core.Services;
using BabySmash.Linux.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace BabySmash.Linux;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static List<MainWindow> Windows { get; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var services = new ServiceCollection();

        // Register Linux-specific platform services
        services.AddSingleton<ITtsService, LinuxTtsService>();
        services.AddSingleton<IAudioService, LinuxAudioService>();
        services.AddSingleton<IKeyboardHookService, LinuxKeyboardHookService>();
        services.AddSingleton<ISettingsService, LinuxSettingsService>();

        // Register core services
        services.AddSingleton<WordFinder>();

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create first window to get screen info
            var firstWindow = new MainWindow();
            desktop.MainWindow = firstWindow;
            
            // Need to show first window briefly to access Screens
            firstWindow.Show();
            
            // Create a window for each screen (multi-monitor support)
            var screens = firstWindow.Screens;
            bool isFirst = true;
            
            foreach (var screen in screens.All)
            {
                MainWindow window;
                if (isFirst)
                {
                    window = firstWindow;
                    isFirst = false;
                }
                else
                {
                    window = new MainWindow();
                    window.Show();
                }
                
                window.Position = new PixelPoint(screen.Bounds.X, screen.Bounds.Y);
                window.Width = screen.Bounds.Width;
                window.Height = screen.Bounds.Height;
                
                Windows.Add(window);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}