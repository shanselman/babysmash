# BabySmash!

A game for babies who like to bang on the keyboard.

As babies or children smash on the keyboard, colored shapes, letters and numbers appear on the screen and are spoken aloud to help with letter and number recognition.

**Download the latest release:** [GitHub Releases](https://github.com/shanselman/babysmash/releases)

## Features

- 🎨 Colorful shapes with happy faces (Circle, Heart, Hexagon, Star, Triangle, and more)
- 🔤 Letters and numbers with text-to-speech
- 🔊 Fun sounds and giggles
- 🖥️ Multi-monitor support
- 🔒 Locks out system keys to prevent accidental exits
- 🔄 **Auto-updates** via GitHub Releases (Windows)
- 🐧 **Linux support** via Avalonia

## Downloads

| Platform | Download | Notes |
|----------|----------|-------|
| **Windows** | [BabySmash-Setup.exe](https://github.com/shanselman/babysmash/releases/latest/download/BabySmash-Setup.exe) | Installer with auto-updates |
| **Windows** | [BabySmash-win-x64.zip](https://github.com/shanselman/babysmash/releases/latest/download/BabySmash-win-x64.zip) | Portable version |
| **Linux** | [BabySmash-linux-x64.tar.gz](https://github.com/shanselman/babysmash/releases/latest/download/BabySmash-linux-x64.tar.gz) | Self-contained executable |

---

## Windows

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Any key | Display shapes/letters! |
| `Ctrl+Shift+Alt+O` | Options dialog |
| `Alt+F4` | Exit |

### Requirements

- Windows 10 or later (64-bit)
- No .NET installation required (self-contained)

### Touchpad Gestures

BabySmash blocks most keyboard shortcuts, but **Windows touchpad gestures** (like three-finger swipe for Task View) are handled at the OS level and cannot be blocked by applications.

**To prevent accidental exits via touchpad:**

1. Open **Windows Settings** → **Bluetooth & devices** → **Touchpad**
2. Under **Three-finger gestures**, set "Swipes" to **Nothing**
3. Optionally disable four-finger gestures too

---

## Linux

### Installation

1. Download and extract:
   ```bash
   tar -xzf BabySmash-linux-x64.tar.gz
   ```

2. Install dependencies:
   ```bash
   # For text-to-speech
   sudo apt install espeak
   
   # For audio (one of these)
   sudo apt install pulseaudio-utils  # for paplay
   # or
   sudo apt install alsa-utils        # for aplay
   ```

3. Run:
   ```bash
   ./babysmash
   ```

### Add to Application Menu (Optional)

To make BabySmash appear in your desktop's app launcher:

```bash
# Copy executable to a permanent location
sudo cp babysmash /usr/local/bin/

# Install the icon
sudo cp babysmash.png /usr/share/icons/hicolor/256x256/apps/

# Install desktop entry
cp babysmash.desktop ~/.local/share/applications/

# Update icon cache
gtk-update-icon-cache /usr/share/icons/hicolor/ 2>/dev/null || true
```

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Any key | Display shapes/letters! |
| `Alt+O` | Options dialog |
| `Escape` | Exit |

### Requirements

- 64-bit Linux (x64)
- `espeak` for text-to-speech
- `paplay` or `aplay` for audio
- No .NET installation required (self-contained)

---

## Localization / Language Support

BabySmash automatically uses your **keyboard language** for:

1. **Text-to-speech voice** - Shapes and colors are spoken in your language
2. **Shape/color names** - Translated to your locale (if available)
3. **Word order** - "Red Circle" (English) vs "Círculo Vermelho" (Portuguese)

**Supported locales:** English (en), German (de), Spanish (es), French (fr), Greek (el), Latvian (lv), Portuguese (pt-BR, pt-PT), Russian (ru)

### Adding a New Language

Create a JSON file in `Shared/Resources/Strings/` named `{locale}.json` (e.g., `ja-JP.json` for Japanese):

```json
{
  "ColorShapeFormat": "{0} {1}",
  "Circle": "丸",
  "Red": "赤",
  ...
}
```

- Use `"{0} {1}"` for color-first languages (English: "Red Circle")
- Use `"{1} {0}"` for shape-first languages (Portuguese: "Círculo Vermelho")

---

## Building from Source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/shanselman/babysmash.git
cd babysmash

# Windows
dotnet run

# Linux
dotnet run --project BabySmash.Linux
```

### Publish Executables

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish BabySmash.Linux -c Release -r linux-x64 --self-contained
```

---

## History

Originally developed by [Scott Hanselman](https://www.hanselman.com), based on AlphaBaby.

- **v1-v2**: Original .NET Framework 3.5 version
- **v3.0**: Migrated to .NET 10, single-file deployment
- **v4.0**: Linux support via Avalonia, shared resources, auto-updates

> **Looking for the original code?** The legacy .NET Framework 3.5 version is preserved in the [legacy-dotnet35](https://github.com/shanselman/babysmash/tree/legacy-dotnet35) branch.

## License

See [LICENSE](LICENSE)
