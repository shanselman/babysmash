# AGENTS.md

Quick context for coding agents working on BabySmash.

## What this repo is
- BabySmash is a keyboard-smash game for babies/kids.
- Two frontends:
  - Windows WPF app: `BabySmash.csproj` (net10.0-windows)
  - Linux Avalonia app: `BabySmash.Linux/BabySmash.Linux.csproj` (net10.0)
- Shared assets live in `Shared/Resources/` and are embedded into both apps.

## Repo structure
- Windows WPF entry points: `App.xaml.cs`, `MainWindow.xaml.cs`, `Controller.cs`
- Linux Avalonia entry points: `BabySmash.Linux/Program.cs`, `BabySmash.Linux/App.axaml.cs`, `BabySmash.Linux/MainWindow.axaml.cs`
- Shapes:
  - Windows: `Shapes/`
  - Linux: `BabySmash.Linux/Shapes/`
- Animation helpers:
  - Windows: `Tweening/`
  - Linux: `BabySmash.Linux/Core/Animation/`

## How to run/build
- Windows:
  - `dotnet run`
  - `dotnet publish -c Release -r win-x64 --self-contained`
- Linux:
  - `dotnet run --project BabySmash.Linux`
  - `dotnet publish BabySmash.Linux -c Release -r linux-x64 --self-contained`

## Update flow
- Windows auto-update is handled by Updatum in `App.xaml.cs`.
- Updates are pulled from GitHub Releases for `shanselman/babysmash`.
- The update is checked before launch so a parent can handle it before the game starts.

## Important behavior
- Windows uses a low-level keyboard hook to block system keys in `App.xaml.cs`.
- Linux currently relies on Avalonia key events (no global hook).
- Auto-update (Windows) uses Updatum in `App.xaml.cs`.

## Shared resources
- Sounds: `Shared/Resources/Sounds/*.wav`
- Localization: `Shared/Resources/Strings/*.json`
- Word list: `Shared/Resources/Words.txt`

## Parity notes
- Try to keep Windows and Linux behavior aligned where feasible:
  - shape generation, sounds, word detection, fade/clear, cursor behavior
- Localization on Windows uses JSON resources; Linux TTS uses language mapping.

## Settings
- Windows: `Properties/Settings.settings` and `Properties/Settings.Designer.cs`
- Linux: JSON settings via `BabySmash.Linux/Platform/LinuxSettingsService.cs` (~/.config/babysmash/settings.json)

## Where to look first
- Windows gameplay: `Controller.cs`, `MainWindow.xaml.cs`
- Linux gameplay: `BabySmash.Linux/MainWindow.axaml.cs`

## Testing
- No automated test suite currently.
- Basic manual smoke checks:
  - Windows: shapes render, sounds/tts fire, options dialog opens, update check does not block launch, keyboard hook blocks system keys.
  - Linux: shapes render, sounds/tts fire (espeak/paplay installed), options dialog opens, multi-monitor windows appear.

## CI and versioning
- CI is defined in `.github/workflows/build.yml` (Build BabySmash).
- Runs on all branches and PRs; release jobs run only on tags `v*`.
- Versioning uses GitVersion in the Windows build:
  - SemVer from GitVersion is injected into `Version`, `AssemblyVersion`, and `FileVersion`.
  - GitVersion config (`GitVersion.yml`):
    - `mode: ContinuousDelivery`
    - `tag-prefix: v`
    - `assembly-versioning-scheme: MajorMinorPatch`
    - `main` branch increments `Patch` with no label
- Windows artifacts are code-signed via Azure Trusted Signing (exe and installer).
- Linux build produces a tarball with `babysmash`, desktop entry, icon, and README.
- Release is created via `softprops/action-gh-release` with the three artifacts.

## Release packaging
- Windows artifacts are built as self-contained single-file (`BabySmash.csproj`).
- Linux artifacts are built as self-contained single-file (`BabySmash.Linux/BabySmash.Linux.csproj`).
- Installer is configured in `installer.iss`.

## Contribution guardrails
- Avoid breaking kiosk-style behavior on Windows (keyboard hook).
- Keep Windows and Linux in functional parity where reasonable.
- Preserve embedded resources and their names; they are used at runtime by name.
- Avoid large dependency additions without a clear cross-platform plan.

## Constraints
- Keep changes minimal and test on both platforms when possible.
