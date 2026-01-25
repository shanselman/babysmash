using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BabySmash.Core.Interfaces;
using BabySmash.Core.Services;
using BabySmash.Linux.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace BabySmash.Linux;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

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
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}