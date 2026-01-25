# Avalonia Port - Quick Start Guide

## For Developers Getting Started with the Avalonia Port

This guide helps developers quickly understand and contribute to the BabySmash Avalonia port.

---

## Prerequisites

### Required
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git
- Code editor (Visual Studio 2022, VS Code, or Rider)

### Platform-Specific
- **Windows**: Visual Studio 2022 with .NET desktop development workload
- **macOS**: Xcode Command Line Tools (`xcode-select --install`)
- **Linux**: `build-essential` package

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/shanselman/babysmash.git
cd babysmash
```

### 2. Build and Run (WPF - Windows Only)

```bash
# Current WPF version (existing)
dotnet run --project BabySmash.csproj
```

### 3. Build and Run (Avalonia - Future Cross-Platform)

```bash
# Avalonia version (after implementation)
dotnet run --project BabySmash.Avalonia/BabySmash.Avalonia.csproj
```

---

## Project Structure

```
BabySmash/
├── BabySmash.csproj           ← Current WPF project
├── BabySmash.Avalonia/        ← Future Avalonia project
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
   dotnet new avalonia.app -n BabySmash.Avalonia -f net10.0
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
        x:Class="BabySmash.Avalonia.MainWindow">
    <Grid>
        <TextBlock Text="Hello Avalonia" />
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

**Windows Implementation** (`BabySmash.Avalonia/Platform/Windows/WindowsTtsService.cs`):
```csharp
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

**macOS Implementation** (`BabySmash.Avalonia/Platform/MacOS/MacOSTtsService.cs`):
```csharp
public class MacOSTtsService : ITtsService
{
    public void Speak(string text)
    {
        // Use AVSpeechSynthesizer or 'say' command
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "say",
                Arguments = $"\"{text}\"",
                CreateNoWindow = true
            }
        };
        process.Start();
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        // macOS 'say' command uses -v flag for voice selection
        return true; // Simplified
    }
}
```

**Linux Implementation** (`BabySmash.Avalonia/Platform/Linux/LinuxTtsService.cs`):
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
                CreateNoWindow = true
            }
        };
        process.Start();
    }

    public bool TrySetLanguage(CultureInfo culture)
    {
        // espeak uses -v flag for language
        return true; // Simplified
    }
}
```

### Dependency Injection Setup

**App.axaml.cs**:
```csharp
public override void OnFrameworkInitializationCompleted()
{
    var services = new ServiceCollection();

    // Register platform-specific services
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        services.AddSingleton<ITtsService, WindowsTtsService>();
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        services.AddSingleton<ITtsService, MacOSTtsService>();
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        services.AddSingleton<ITtsService, LinuxTtsService>();
    }

    // Register core services
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

### Run Avalonia Version (Cross-Platform)
```bash
# Windows
dotnet run --project BabySmash.Avalonia/BabySmash.Avalonia.csproj

# macOS
dotnet run --project BabySmash.Avalonia/BabySmash.Avalonia.csproj

# Linux
dotnet run --project BabySmash.Avalonia/BabySmash.Avalonia.csproj
```

### Build for Release
```bash
# Windows x64
dotnet publish BabySmash.Avalonia/BabySmash.Avalonia.csproj -c Release -r win-x64 --self-contained

# macOS x64 (Intel)
dotnet publish BabySmash.Avalonia/BabySmash.Avalonia.csproj -c Release -r osx-x64 --self-contained

# macOS ARM64 (Apple Silicon)
dotnet publish BabySmash.Avalonia/BabySmash.Avalonia.csproj -c Release -r osx-arm64 --self-contained

# Linux x64
dotnet publish BabySmash.Avalonia/BabySmash.Avalonia.csproj -c Release -r linux-x64 --self-contained
```

---

## Common Tasks

### Adding a New Shape

1. **Create Shape XAML** in `BabySmash.Avalonia/Shapes/`:
   ```xml
   <UserControl xmlns="https://github.com/avaloniaui"
                x:Class="BabySmash.Avalonia.Shapes.CoolDiamond">
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

### Windows
- Use Visual Studio 2022 debugger
- Attach to process if needed
- Check Event Viewer for crashes

### macOS
- Use VS Code with C# extension
- Check Console.app for logs
- Grant Accessibility permissions for keyboard hooks

### Linux
- Use VS Code or Rider
- Run with `AVALONIA_DEBUG=1` for verbose logging
- Check `journalctl` for system logs

---

## Contributing

### Before Submitting a PR

1. **Test on your platform** (Windows, macOS, or Linux)
2. **Run existing tests**: `dotnet test` (when test project exists)
3. **Check code style**: Follow existing patterns
4. **Update documentation** if adding features

### PR Checklist
- [ ] Code compiles on your platform
- [ ] Feature tested manually
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

### Q: Why both WPF and Avalonia?
**A**: WPF stays as the primary Windows version (mature, optimized). Avalonia enables macOS/Linux support without abandoning existing Windows users.

### Q: Can I use Avalonia on Windows?
**A**: Yes! Avalonia runs on Windows too. Both versions will be maintained.

### Q: What about mobile (iOS/Android)?
**A**: Future possibility with Avalonia, but not in initial scope.

### Q: How do I test keyboard hooks on macOS?
**A**: You'll need to grant Accessibility permissions: System Preferences → Security & Privacy → Privacy → Accessibility → Add BabySmash

### Q: Where should I start contributing?
**A**: Start with Phase 1 (extracting Core library) or porting simple XAML shapes.

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**For Questions**: Open an issue on GitHub
