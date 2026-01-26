# Avalonia Port - Quick Start Guide

## For Developers Getting Started with the Linux Port

This guide helps developers quickly understand and contribute to the BabySmash Linux port using Avalonia.

---

## Prerequisites

### Required
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git
- Code editor (Visual Studio 2022, VS Code, or Rider)

### Platform-Specific
- **Windows**: Visual Studio 2022 with .NET desktop development workload (for WPF)
- **Linux**: `build-essential` package, X11 or Wayland development libraries

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/shanselman/babysmash.git
cd babysmash
```

### 2. Build and Run (WPF - Windows Only)

```bash
# Current WPF version (existing) - Windows only
dotnet run --project BabySmash.csproj
```

### 3. Build and Run (Avalonia - Linux)

```bash
# Avalonia version (after implementation) - Linux
dotnet run --project BabySmash.Linux/BabySmash.Linux.csproj
```

---

## Project Structure

```
BabySmash/
├── BabySmash.csproj           ← Current WPF project (Windows - no changes)
├── BabySmash.Linux/           ← Future Avalonia project (Linux)
├── BabySmash.Core/            ← Shared business logic
├── AVALONIA_PORT_PLAN.md      ← Comprehensive planning doc
└── docs/
    └── AVALONIA_ARCHITECTURE.md  ← Technical architecture
```

---

## Development Workflow

### Phase 1: Extract Shared Core (First Step)

**Goal**: Create `BabySmash.Core` library with platform-agnostic code

1. Create new class library:
   ```bash
   dotnet new classlib -n BabySmash.Core -f net10.0
   ```

2. Move these files to `BabySmash.Core`:
   - `WordFinder.cs`
   - Shape models
   - Extension methods
   - Localization logic

3. Create platform abstraction interfaces:
   - `ITtsService` (Text-to-Speech)
   - `IAudioService` (Audio playback)
   - `IKeyboardHookService` (Keyboard hooks)
   - `ISettingsService` (Settings storage)

4. Update WPF project to reference Core:
   ```xml
   <ProjectReference Include="..\BabySmash.Core\BabySmash.Core.csproj" />
   ```

### Phase 2: Create Avalonia Project

1. Create Avalonia app:
   ```bash
   dotnet new avalonia.app -n BabySmash.Linux -f net10.0
   ```

2. Add reference to Core:
   ```xml
   <ProjectReference Include="..\BabySmash.Core\BabySmash.Core.csproj" />
   ```

3. Add Avalonia packages:
   ```xml
   <PackageReference Include="Avalonia" Version="11.2.0" />
   <PackageReference Include="Avalonia.Desktop" Version="11.2.0" />
   <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.0" />
   ```

### Phase 3: Port XAML

Convert WPF XAML to Avalonia XAML (mostly copy-paste with minor changes):

**WPF → Avalonia Changes**:
- `.xaml` → `.axaml` (file extension)
- Namespace: `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` → `xmlns="https://github.com/avaloniaui"`
- `AllowsTransparency` → `TransparencyLevelHint`
- `WindowStyle` → `SystemDecorations`

**Example**:

WPF (`MainWindow.xaml`):
```xml
<Window x:Class="BabySmash.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <TextBlock Text="Hello WPF" />
    </Grid>
</Window>
```

Avalonia (`MainWindow.axaml`):
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="BabySmash.Linux.MainWindow">
    <Grid>
        <TextBlock Text="Hello Avalonia on Linux" />
    </Grid>
</Window>
```

---

## Platform-Specific Code

### Implementing Platform Services

Each platform service has an interface in `BabySmash.Core` and platform-specific implementations.

#### Example: Text-to-Speech Service

**Interface** (`BabySmash.Core/Interfaces/ITtsService.cs`):
```csharp
public interface ITtsService
{
    void Speak(string text);
    bool TrySetLanguage(CultureInfo culture);
}
```

**Windows Implementation** (WPF - existing, no changes needed):
```csharp
// In existing WPF project - uses System.Speech
public class WindowsTtsService : ITtsService
{
    private readonly SpeechSynthesizer _synthesizer = new();

    public void Speak(string text)
    {
        _synthesizer.SpeakAsync(text);
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        try
        {
            _synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, culture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**Linux Implementation** (`BabySmash.Linux/Platform/LinuxTtsService.cs`):
```csharp
public class LinuxTtsService : ITtsService
{
    public void Speak(string text)
    {
        // Use espeak or speech-dispatcher
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "espeak",
                Arguments = $"\"{text}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        process.Start();
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        // espeak uses -v flag for language (e.g., espeak -v en "text")
        return true; // Simplified for example
    }
}
```

### Dependency Injection Setup

**App.axaml.cs** (Linux Avalonia project):
```csharp
public override void OnFrameworkInitializationCompleted()
{
    var services = new ServiceCollection();

    // Register Linux-specific services
    services.AddSingleton<ITtsService, LinuxTtsService>();
    services.AddSingleton<IAudioService, LinuxAudioService>();
    services.AddSingleton<IKeyboardHookService, LinuxKeyboardHookService>();
    services.AddSingleton<ISettingsService, JsonSettingsService>();
    services.AddSingleton<IScreenService, AvaloniaScreenService>();

    // Register core services (shared)
    services.AddSingleton<GameController>();
    services.AddSingleton<WordFinder>();

    var provider = services.BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        };
    }

    base.OnFrameworkInitializationCompleted();
}
```

---

## Testing

### Run WPF Version (Windows)
```bash
dotnet run --project BabySmash.csproj
```

### Run Linux Avalonia Version
```bash
# Linux
dotnet run --project BabySmash.Linux/BabySmash.Linux.csproj
```

### Build for Release
```bash
# Linux x64
dotnet publish BabySmash.Linux/BabySmash.Linux.csproj -c Release -r linux-x64 --self-contained

# Linux ARM64 (optional, for Raspberry Pi)
dotnet publish BabySmash.Linux/BabySmash.Linux.csproj -c Release -r linux-arm64 --self-contained
```

---

## Common Tasks

### Adding a New Shape

1. **Create Shape XAML** in `BabySmash.Linux/Shapes/`:
   ```xml
   <UserControl xmlns="https://github.com/avaloniaui"
                x:Class="BabySmash.Linux.Shapes.CoolDiamond">
       <Polygon Points="50,0 100,50 50,100 0,50" Fill="{Binding Fill}" />
   </UserControl>
   ```

2. **Create Code-Behind**:
   ```csharp
   public partial class CoolDiamond : UserControl
   {
       public CoolDiamond()
       {
           InitializeComponent();
       }
   }
   ```

3. **Register in GameController** (`BabySmash.Core`):
   ```csharp
   private string[] AvailableShapes => new[]
   {
       "Circle", "Square", "Star", "Heart", "Diamond" // Add new shape
   };
   ```

### Adding a New Sound

1. Add WAV file to `BabySmash.Core/Resources/Sounds/newsound.wav`
2. Mark as Embedded Resource in `.csproj`:
   ```xml
   <EmbeddedResource Include="Resources\Sounds\newsound.wav" />
   ```
3. Use in code:
   ```csharp
   audioService.PlaySound("newsound.wav");
   ```

### Adding a New Language

1. Create JSON file in `BabySmash.Core/Resources/Strings/{locale}.json`:
   ```json
   {
     "ColorShapeFormat": "{0} {1}",
     "Circle": "Kreis",
     "Red": "Rot"
   }
   ```
2. Localization service will auto-detect and use it

---

## Debugging Tips

### Windows (WPF)
- Use Visual Studio 2022 debugger
- Attach to process if needed
- Check Event Viewer for crashes

### Linux
- Use VS Code or Rider
- Run with `AVALONIA_DEBUG=1` for verbose logging
- Check `journalctl` for system logs
- Test on both X11 and Wayland
- For keyboard hooks, check permissions and user groups

---

## Contributing

### Before Submitting a PR

1. **Test on Linux** (your distribution)
2. **Run existing tests**: `dotnet test` (when test project exists)
3. **Check code style**: Follow existing patterns
4. **Update documentation** if adding features

### PR Checklist
- [ ] Code compiles on Linux
- [ ] Feature tested manually on at least one Linux distro
- [ ] No new warnings or errors
- [ ] Documentation updated (if needed)

---

## Resources

### Avalonia Documentation
- [Getting Started](https://docs.avaloniaui.net/docs/getting-started)
- [WPF to Avalonia Migration](https://docs.avaloniaui.net/guides/migration/wpf)
- [XAML Reference](https://docs.avaloniaui.net/docs/xaml)

### BabySmash Specific
- [Main Planning Document](../AVALONIA_PORT_PLAN.md)
- [Architecture Details](AVALONIA_ARCHITECTURE.md)
- [Original README](../README.md)

### Community
- [Avalonia Discord](https://discord.gg/avalonia)
- [BabySmash Issues](https://github.com/shanselman/babysmash/issues)

---

## FAQ

### Q: Why only Linux and not macOS too?
**A**: The current Windows WPF version is mature and optimized. The focus is specifically on bringing BabySmash to Linux users. macOS could be considered in the future if there's demand.

### Q: Can I use the Avalonia version on Windows?
**A**: The Windows WPF version is the primary and recommended version for Windows. The Avalonia port is specifically for Linux.

### Q: What about Raspberry Pi?
**A**: Linux ARM64 support is planned, which would enable Raspberry Pi use (Pi 4 and newer with 64-bit OS).

### Q: How do I test keyboard hooks on Linux?
**A**: You may need to ensure your user is in the appropriate groups (e.g., `input`) or test with appropriate permissions. The implementation will vary between X11 and Wayland.

### Q: Where should I start contributing?
**A**: Start with Phase 1 (extracting Core library) or porting simple XAML shapes to Avalonia syntax.

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**For Questions**: Open an issue on GitHub
