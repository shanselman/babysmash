# Testing BabySmash Linux on WSLg

## What is WSLg?

WSLg (Windows Subsystem for Linux GUI) allows you to run Linux GUI applications directly on Windows 11. Perfect for testing the BabySmash Linux port!

## Prerequisites

1. **Windows 11** with WSL2 installed
2. **WSLg** (comes with Windows 11 by default)
3. **Ubuntu** (or any Linux distro)

## Setup

### 1. Enable WSL and install Ubuntu (if not already done)

```powershell
# In PowerShell (Administrator)
wsl --install
```

### 2. Install .NET 10 SDK in WSL

```bash
# In WSL terminal
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc
source ~/.bashrc
```

### 3. Install espeak for Text-to-Speech (optional)

```bash
sudo apt update
sudo apt install espeak
```

## Running BabySmash Linux

### From Source

```bash
# Clone the repository (or navigate to existing clone)
cd /path/to/babysmash

# Build and run
cd BabySmash.Linux
dotnet run
```

### Building Release Version

```bash
cd BabySmash.Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Run the built executable
./bin/Release/net10.0/linux-x64/publish/BabySmash.Linux
```

## Features Currently Working

✅ Displays window on WSLg  
✅ Keyboard input handling  
✅ Creates colored shapes when pressing keys  
✅ Letters and numbers recognition  
✅ Word detection (tries to find words you type)  
✅ Text-to-speech (if espeak installed)  
✅ ESC to exit  

## Known Limitations

⚠️ Audio playback not yet implemented (just console output)  
⚠️ No fancy shape animations yet (coming in Phase 3)  
⚠️ No multi-monitor support yet  
⚠️ No low-level keyboard hooks (uses normal Avalonia events)  

## Troubleshooting

### WSLg window doesn't appear

Make sure you have Windows 11 and WSLg is working:
```bash
# Test WSLg with a simple app
sudo apt install x11-apps
xclock
```

If xclock appears, WSLg is working!

### "espeak: command not found"

Text-to-speech is optional. Install espeak:
```bash
sudo apt install espeak
```

### .NET SDK not found

Make sure .NET is in your PATH:
```bash
dotnet --version
# Should show 10.x.x
```

## Next Steps

The basic Linux port is working! Next phases will add:
- More shapes (circles, stars, hearts, etc.)
- Better animations
- Audio playback
- Multi-monitor support
- Packaging (AppImage, etc.)

## Feedback

This is an early implementation for testing. Please report any issues!
