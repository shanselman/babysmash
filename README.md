# BabySmash!

A game for babies who like to bang on the keyboard.

As babies or children smash on the keyboard, colored shapes, letters and numbers appear on the screen and are spoken aloud to help with letter and number recognition.

**Download the latest release:** [GitHub Releases](https://github.com/shanselman/babysmash/releases)

## Features

- 🎨 Colorful shapes with happy faces (Circle, Heart, Hexagon, Star, Triangle, and more)
- 🔤 Letters and numbers with text-to-speech
- 🔊 Fun sounds and giggles
- 🖥️ Multi-monitor support with Per-Monitor DPI awareness
- 🔒 Locks out Windows key, Ctrl+Esc, Alt+Tab to prevent accidental exits
- 🔄 **Auto-updates** via GitHub Releases

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Any key | Display shapes/letters! |
| `Ctrl+Shift+Alt+O` | Options dialog |
| `Alt+F4` | Exit |

## Requirements

- Windows 10 or later (64-bit)
- No .NET installation required (self-contained)

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build & Run

```bash
git clone https://github.com/shanselman/babysmash.git
cd babysmash
dotnet run
```

### Publish Single-File Executable

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Output: `bin/Release/net10.0-windows/win-x64/publish/BabySmash.exe`

## History

Originally developed by [Scott Hanselman](https://www.hanselman.com), based on AlphaBaby.

- **v1-v2**: Original .NET Framework 3.5 version ([legacy-dotnet35 branch](https://github.com/shanselman/babysmash/tree/legacy-dotnet35))
- **v3.0**: Migrated to .NET 10, single-file deployment
- **v4.0**: Added Updatum auto-updates, Azure code signing, new icon

## License

See [LICENSE](LICENSE)
