# BabySmash! Avalonia Architecture

## Overview

This document details the technical architecture for the Avalonia port, focusing on the dual-codebase approach with maximum code sharing through the `BabySmash.Core` library.

---

## Solution Structure

```
BabySmash/
├── BabySmash.sln
├── BabySmash.WPF/                    (Existing Windows-only WPF app)
│   ├── BabySmash.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / MainWindow.xaml.cs
│   ├── Options.xaml / Options.xaml.cs
│   ├── UpdateDialog.xaml / UpdateDialog.xaml.cs
│   ├── DownloadProgressDialog.xaml / DownloadProgressDialog.xaml.cs
│   ├── Shapes/                       (14 WPF UserControls)
│   │   ├── CoolCircle.xaml
│   │   ├── CoolSquare.xaml
│   │   └── ...
│   └── Platform/                     (Windows-specific implementations)
│       ├── WindowsTtsService.cs
│       ├── WindowsAudioService.cs
│       ├── WindowsKeyboardHookService.cs
│       └── WindowsSettingsService.cs
│
├── BabySmash.Avalonia/               (New cross-platform Avalonia app)
│   ├── BabySmash.Avalonia.csproj
│   ├── App.axaml / App.axaml.cs
│   ├── MainWindow.axaml / MainWindow.axaml.cs
│   ├── Options.axaml / Options.axaml.cs
│   ├── UpdateDialog.axaml / UpdateDialog.axaml.cs
│   ├── DownloadProgressDialog.axaml / DownloadProgressDialog.axaml.cs
│   ├── Shapes/                       (14 Avalonia UserControls)
│   │   ├── CoolCircle.axaml
│   │   ├── CoolSquare.axaml
│   │   └── ...
│   └── Platform/                     (Platform-specific implementations)
│       ├── Windows/
│       │   ├── WindowsTtsService.cs
│       │   ├── WindowsAudioService.cs
│       │   └── WindowsKeyboardHookService.cs
│       ├── MacOS/
│       │   ├── MacOSTtsService.cs
│       │   ├── MacOSAudioService.cs
│       │   └── MacOSKeyboardHookService.cs
│       └── Linux/
│           ├── LinuxTtsService.cs
│           ├── LinuxAudioService.cs
│           └── LinuxKeyboardHookService.cs
│
└── BabySmash.Core/                   (Shared .NET Standard 2.0 or .NET 6+ library)
    ├── BabySmash.Core.csproj
    ├── Interfaces/                   (Platform abstraction interfaces)
    │   ├── ITtsService.cs
    │   ├── IAudioService.cs
    │   ├── IKeyboardHookService.cs
    │   ├── ISettingsService.cs
    │   └── IScreenService.cs
    ├── Models/
    │   ├── Shape.cs
    │   ├── AppSettings.cs
    │   └── WordMatch.cs
    ├── Services/
    │   ├── GameController.cs         (Extracted from Controller.cs)
    │   ├── WordFinder.cs
    │   ├── LocalizationService.cs
    │   └── ColorManager.cs
    ├── Extensions/
    │   ├── ColorExtensions.cs
    │   └── StringExtensions.cs
    ├── ViewModels/                   (MVVM support for both platforms)
    │   ├── MainWindowViewModel.cs
    │   └── OptionsViewModel.cs
    └── Resources/
        ├── Sounds/                   (Embedded WAV files)
        ├── Strings/                  (Localization JSON)
        └── Words.txt
```

---

## Core Abstractions

### 1. Service Interfaces

All platform-specific functionality is accessed through interfaces defined in `BabySmash.Core`.

#### ITtsService (Text-to-Speech)

```csharp
namespace BabySmash.Core.Interfaces
{
    public interface ITtsService
    {
        /// <summary>
        /// Speaks the specified text using the platform's TTS engine.
        /// </summary>
        void Speak(string text);

        /// <summary>
        /// Sets the language/culture for TTS. May fail if voice not installed.
        /// </summary>
        /// <returns>True if language was set successfully, false otherwise.</returns>
        bool TrySetLanguage(CultureInfo culture);

        /// <summary>
        /// Gets the current TTS language/culture.
        /// </summary>
        CultureInfo CurrentLanguage { get; }

        /// <summary>
        /// Gets the available TTS voices/languages on this platform.
        /// </summary>
        IEnumerable<CultureInfo> AvailableLanguages { get; }

        /// <summary>
        /// Cancels any currently speaking text.
        /// </summary>
        void CancelSpeech();
    }
}
```

**Platform Implementations**:

- **Windows** (`WindowsTtsService`): Uses `System.Speech.Synthesis.SpeechSynthesizer`
- **macOS** (`MacOSTtsService`): Uses `AVFoundation.AVSpeechSynthesizer` via P/Invoke or Xamarin bindings
- **Linux** (`LinuxTtsService`): Executes `espeak` or `speech-dispatcher` CLI

#### IAudioService (Sound Playback)

```csharp
namespace BabySmash.Core.Interfaces
{
    public interface IAudioService
    {
        /// <summary>
        /// Plays an embedded sound resource by name (e.g., "giggle.wav").
        /// </summary>
        void PlaySound(string resourceName);

        /// <summary>
        /// Plays an embedded sound asynchronously.
        /// </summary>
        Task PlaySoundAsync(string resourceName);

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        void StopAll();
    }
}
```

**Implementation Options**:
- **NAudio** (Windows, Linux with ALSA/PulseAudio)
- **System.Media.SoundPlayer** (.NET, limited but cross-platform)
- **SkiaSharp.HarfBuzz** (via Avalonia, has audio capabilities)
- **Platform-specific**: AVFoundation (macOS), ALSA (Linux)

#### IKeyboardHookService (Low-Level Keyboard Interception)

```csharp
namespace BabySmash.Core.Interfaces
{
    public interface IKeyboardHookService
    {
        /// <summary>
        /// Event raised when a key is pressed (before OS processing).
        /// </summary>
        event EventHandler<KeyboardHookEventArgs> KeyPressed;

        /// <summary>
        /// Starts the low-level keyboard hook.
        /// </summary>
        /// <returns>True if hook started successfully, false otherwise.</returns>
        bool Start();

        /// <summary>
        /// Stops the keyboard hook.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets whether the hook is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Blocks specific key combinations (e.g., Alt+Tab, Windows key).
        /// </summary>
        void BlockKeys(params KeyCombination[] keys);
    }

    public class KeyboardHookEventArgs : EventArgs
    {
        public int VirtualKeyCode { get; set; }
        public char Character { get; set; }
        public bool Handled { get; set; } // Set to true to suppress the key
    }
}
```

**Platform Implementations**:
- **Windows**: Win32 `SetWindowsHookEx` with `WH_KEYBOARD_LL`
- **macOS**: `CGEventTap` (requires Accessibility permissions, code signing, and notarization)
- **Linux**: X11 `XGrabKeyboard` or Wayland input capture

**Permission Requirements**:
- macOS: Must request Accessibility permissions, show instructions to user
- Linux: May require running as root or specific user groups (input, uinput)

#### ISettingsService (Cross-Platform Settings Storage)

```csharp
namespace BabySmash.Core.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value by key, or returns default if not found.
        /// </summary>
        T Get<T>(string key, T defaultValue = default);

        /// <summary>
        /// Sets a setting value by key.
        /// </summary>
        void Set<T>(string key, T value);

        /// <summary>
        /// Saves all settings to disk.
        /// </summary>
        void Save();

        /// <summary>
        /// Loads settings from disk.
        /// </summary>
        void Load();

        /// <summary>
        /// Resets all settings to defaults.
        /// </summary>
        void Reset();
    }
}
```

**Implementation**:
Use `System.Text.Json` to serialize settings to JSON file in platform-specific directory:

- Windows: `%APPDATA%\BabySmash\settings.json`
- macOS: `~/Library/Application Support/BabySmash/settings.json`
- Linux: `~/.config/babysmash/settings.json`

#### IScreenService (Multi-Monitor Support)

```csharp
namespace BabySmash.Core.Interfaces
{
    public interface IScreenService
    {
        /// <summary>
        /// Gets all screens/monitors currently connected.
        /// </summary>
        IEnumerable<ScreenInfo> GetAllScreens();

        /// <summary>
        /// Gets the primary screen.
        /// </summary>
        ScreenInfo GetPrimaryScreen();
    }

    public class ScreenInfo
    {
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
```

**Platform Implementations**:
- **WPF**: `System.Windows.Forms.Screen.AllScreens`
- **Avalonia**: `Avalonia.Platform.Screens.All`

---

## XAML Differences: WPF vs Avalonia

### Window Definition

**WPF (MainWindow.xaml)**:
```xml
<Window x:Class="BabySmash.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="BabySmash" Width="800" Height="600"
    WindowStyle="None" AllowsTransparency="True">
    <!-- Content -->
</Window>
```

**Avalonia (MainWindow.axaml)**:
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="BabySmash.Avalonia.MainWindow"
        Title="BabySmash" Width="800" Height="600"
        SystemDecorations="None" TransparencyLevelHint="Transparent">
    <!-- Content -->
</Window>
```

**Key Differences**:
- Namespace: `http://schemas.microsoft.com/winfx/2006/xaml/presentation` → `https://github.com/avaloniaui`
- `AllowsTransparency="True"` → `TransparencyLevelHint="Transparent"`
- `WindowStyle="None"` → `SystemDecorations="None"`

### Animations

**WPF & Avalonia**: Mostly compatible syntax

```xml
<Storyboard x:Key="FadeIn">
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                     From="0" To="1" Duration="0:0:0.5"/>
</Storyboard>
```

**Note**: Avalonia also supports CSS-like animations for more modern approach.

---

## Performance Considerations

### Memory Usage
- **Target**: < 200MB (similar to WPF version)
- **Strategy**: Reuse shape instances from object pool, dispose old shapes

### CPU Usage
- **Target**: < 5% idle, < 20% during rapid keypress
- **Strategy**: Throttle shape creation, use hardware acceleration

### Startup Time
- **Target**: < 2 seconds
- **Strategy**: Lazy-load resources, defer update check

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Status**: DRAFT
