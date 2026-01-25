# BabySmash! Avalonia Architecture

## Overview

This document details the technical architecture for the Avalonia port to **Linux**, focusing on a dual-codebase approach with maximum code sharing through the `BabySmash.Core` library.

**Target**: Linux support via Avalonia while maintaining Windows WPF version

---

## Solution Structure

```
BabySmash/
├── BabySmash.sln
├── BabySmash.csproj                  (Existing Windows WPF app - no changes)
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / MainWindow.xaml.cs
│   ├── Options.xaml / Options.xaml.cs
│   ├── Shapes/                       (14 WPF UserControls)
│   │   ├── CoolCircle.xaml
│   │   ├── CoolSquare.xaml
│   │   └── ...
│   └── Windows-specific code (System.Speech, keyboard hooks, etc.)
│
├── BabySmash.Linux/                  (New Avalonia for Linux)
│   ├── BabySmash.Linux.csproj
│   ├── App.axaml / App.axaml.cs
│   ├── MainWindow.axaml / MainWindow.axaml.cs
│   ├── Options.axaml / Options.axaml.cs
│   ├── Shapes/                       (14 Avalonia UserControls ported from WPF)
│   │   ├── CoolCircle.axaml
│   │   ├── CoolSquare.axaml
│   │   └── ...
│   └── Platform/                     (Linux-specific implementations)
│       ├── LinuxTtsService.cs
│       ├── LinuxAudioService.cs
│       └── LinuxKeyboardHookService.cs
│
└── BabySmash.Core/                   (Shared .NET library)
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

- **Windows** (`WindowsTtsService`): Uses `System.Speech.Synthesis.SpeechSynthesizer` (existing WPF)
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
- **NAudio** (cross-platform with ALSA/PulseAudio backend for Linux)
- **ALSA** (Linux direct audio)
- **PulseAudio** (most common on modern Linux)

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
- **Windows**: Win32 `SetWindowsHookEx` with `WH_KEYBOARD_LL` (existing WPF)
- **Linux**: X11 `XGrabKeyboard` or Wayland input capture (new)

**Permission Requirements**:
- Linux: May require specific user groups (input, uinput) or running with appropriate permissions

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
Use `System.Text.Json` to serialize settings to JSON file:

- Windows: `%APPDATA%\BabySmash\settings.json` (WPF continues using existing approach)
- Linux: `~/.config/babysmash/settings.json` (new Avalonia)

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
- **WPF**: `System.Windows.Forms.Screen.AllScreens` (existing, no changes)
- **Avalonia**: `Avalonia.Platform.Screens.All` (Linux)

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
