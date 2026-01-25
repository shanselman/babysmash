# Avalonia Port Implementation Checklist - Linux Focus

This checklist tracks the implementation of the Avalonia port for **Linux** based on the [AVALONIA_PORT_PLAN.md](../AVALONIA_PORT_PLAN.md).

**Scope**: Linux support via Avalonia while maintaining Windows WPF version

---

## Phase 1: Project Setup & Shared Core (2-3 weeks)

**Goal**: Extract shared business logic into `BabySmash.Core`

### Core Library Setup
- [ ] Create `BabySmash.Core` project (.NET 10 class library)
- [ ] Configure project file with embedded resources
- [ ] Setup solution structure with proper project references

### Define Platform Abstractions
- [ ] Create `Interfaces/ITtsService.cs` (Text-to-Speech)
- [ ] Create `Interfaces/IAudioService.cs` (Sound playback)
- [ ] Create `Interfaces/IKeyboardHookService.cs` (Keyboard interception)
- [ ] Create `Interfaces/ISettingsService.cs` (Settings storage)
- [ ] Create `Interfaces/IScreenService.cs` (Multi-monitor support)

### Extract Core Logic
- [ ] Move `WordFinder.cs` to Core (already platform-agnostic)
- [ ] Extract `GameController` from `Controller.cs` (refactor to remove WPF dependencies)
- [ ] Move shape models to `Core/Models/`
- [ ] Move localization logic to `Core/Services/LocalizationService.cs`
- [ ] Move color management to `Core/Services/ColorManager.cs`
- [ ] Move extension methods to `Core/Extensions/`

### Move Resources
- [ ] Move `Resources/Sounds/*.wav` to Core as embedded resources
- [ ] Move `Resources/Strings/*.json` to Core as embedded resources
- [ ] Move `Words.txt` to Core as embedded resource
- [ ] Update resource loading to work from Core assembly

### Update WPF Project
- [ ] Rename current project to `BabySmash.WPF` (optional)
- [ ] Add project reference to `BabySmash.Core`
- [ ] Implement `WindowsTtsService` (wraps System.Speech)
- [ ] Implement `WindowsAudioService` (wraps current Audio.cs)
- [ ] Implement `WindowsKeyboardHookService` (wraps KeyboardHook.cs)
- [ ] Implement `WindowsSettingsService` (wraps Properties.Settings)
- [ ] Implement `WindowsScreenService` (wraps System.Windows.Forms.Screen)
- [ ] Update `App.xaml.cs` to use dependency injection
- [ ] Update `Controller.cs` to use injected services from Core

### Testing
- [ ] Build solution successfully
- [ ] Run WPF app - verify all features still work
- [ ] Test shapes display correctly
- [ ] Test audio playback
- [ ] Test text-to-speech
- [ ] Test options dialog
- [ ] Test multi-monitor support
- [ ] Test auto-update functionality

---

## Phase 2: Avalonia Project Bootstrap (1 week)

**Goal**: Create basic Avalonia project structure for Linux

### Project Creation
- [ ] Create `BabySmash.Linux` project using Avalonia template
- [ ] Configure project to target `net10.0`
- [ ] Set RuntimeIdentifiers: `linux-x64` (and optionally `linux-arm64` for Raspberry Pi)
- [ ] Add project reference to `BabySmash.Core`

### NuGet Packages
- [ ] Add `Avalonia` (11.2.0+)
- [ ] Add `Avalonia.Desktop`
- [ ] Add `Avalonia.Themes.Fluent`
- [ ] Add `Updatum` (1.3.4+)
- [ ] Add `Microsoft.Extensions.DependencyInjection`

### Basic App Structure
- [ ] Create `App.axaml` and `App.axaml.cs`
- [ ] Create `MainWindow.axaml` and `MainWindow.axaml.cs`
- [ ] Setup dependency injection in `App.axaml.cs` for Linux services
- [ ] Configure resource embedding

### Linux Service Stubs
- [ ] Create `Platform/` directory
- [ ] Create stub implementations for Linux services (return NotImplementedException)
  - [ ] LinuxTtsService
  - [ ] LinuxAudioService
  - [ ] LinuxKeyboardHookService
  - [ ] LinuxSettingsService
  - [ ] LinuxScreenService

### Build Configuration
- [ ] Configure single-file publishing for Linux
- [ ] Test build on Linux: `dotnet build`
- [ ] Test publish for Linux x64: `dotnet publish -r linux-x64`
- [ ] (Optional) Test publish for Linux ARM64: `dotnet publish -r linux-arm64`

### Verification
- [ ] App launches on Linux and shows empty window
- [ ] No build errors or warnings
- [ ] Dependency injection works (services resolve correctly)

---

## Phase 3: Port Core UI & Shapes (3-4 weeks)

**Goal**: Implement main game UI with shapes and animations

### Main Window
- [ ] Port `MainWindow.xaml` to `MainWindow.axaml`
- [ ] Convert WPF-specific properties to Avalonia equivalents
- [ ] Port `mainGrid` layout with Canvas elements
- [ ] Port `infoLabel` TextBlock
- [ ] Port `UpdateAvailableLabel` TextBlock (remove BitmapEffect)
- [ ] Port window triggers and storyboards
- [ ] Update code-behind to use Avalonia APIs

### Shapes UserControls
- [ ] Create `Shapes/` directory in Avalonia project
- [ ] Port `CoolCircle.xaml` → `CoolCircle.axaml`
- [ ] Port `CoolOval.xaml` → `CoolOval.axaml`
- [ ] Port `CoolSquare.xaml` → `CoolSquare.axaml`
- [ ] Port `CoolStar.xaml` → `CoolStar.axaml`
- [ ] Port `CoolHeart.xaml` → `CoolHeart.axaml`
- [ ] Port `CoolHexagon.xaml` → `CoolHexagon.axaml`
- [ ] Port `CoolRectangle.xaml` → `CoolRectangle.axaml`
- [ ] Port `CoolTrapezoid.xaml` → `CoolTrapezoid.axaml`
- [ ] Port `CoolTriangle.xaml` → `CoolTriangle.axaml`
- [ ] Port all remaining shapes (verify count matches WPF)
- [ ] Convert animations to Avalonia Animation API
- [ ] Test face animations work correctly

### Options Dialog
- [ ] Port `Options.xaml` → `Options.axaml` (64KB XAML)
- [ ] Consider simplifying UI for cross-platform
- [ ] Update code-behind for Avalonia
- [ ] Test all settings save/load correctly

### Update Dialogs
- [ ] Port `UpdateDialog.xaml` → `UpdateDialog.axaml`
- [ ] Port `DownloadProgressDialog.xaml` → `DownloadProgressDialog.axaml`
- [ ] Update code-behind for Avalonia

### Multi-Monitor Support
- [ ] Implement `AvaloniaScreenService` using `Avalonia.Platform.Screens`
- [ ] Update `GameController.Launch()` to create windows per screen
- [ ] Test with multiple monitors

### Testing
- [ ] Launch app, verify window appears
- [ ] Press keys, verify shapes appear (even without sound/speech)
- [ ] Test all 14+ shapes display correctly
- [ ] Test animations run smoothly
- [ ] Test multi-monitor support
- [ ] Open options dialog (Ctrl+Shift+Alt+O)
- [ ] Verify settings save and load

---

## Phase 4: Platform Services Implementation (3-4 weeks)

**Goal**: Implement platform-specific services

## Phase 4: Platform Services Implementation (2-3 weeks)

**Goal**: Implement Linux-specific services

### Linux Platform Services
- [ ] Research Linux TTS options (espeak, speech-dispatcher)
- [ ] Implement `LinuxTtsService`
  - [ ] Test with espeak
  - [ ] Test with speech-dispatcher
  - [ ] Handle missing TTS gracefully
- [ ] Research Linux audio options (ALSA, PulseAudio, NAudio)
- [ ] Implement `LinuxAudioService`
  - [ ] Test with NAudio
  - [ ] Test with direct ALSA/PulseAudio
- [ ] Research Linux keyboard hooks (X11 XGrabKeyboard, Wayland)
- [ ] Implement `LinuxKeyboardHookService`
  - [ ] Implement X11 keyboard hooks
  - [ ] Implement Wayland keyboard hooks (or fallback)
  - [ ] Handle permission requirements gracefully
- [ ] Test all services on major Linux distributions:
  - [ ] Ubuntu 22.04+ (GNOME, X11 and Wayland)
  - [ ] Fedora (GNOME, Wayland)
  - [ ] Arch Linux (KDE, X11)
  - [ ] Linux Mint (Cinnamon)

### Audio Playback
- [ ] Test WAV playback on Linux
- [ ] Ensure embedded resources load correctly
- [ ] Verify audio works with PulseAudio
- [ ] Verify audio works with ALSA

### Localization
- [ ] Test TTS with multiple languages on Linux
- [ ] Verify en-EN, ru-RU, pt-BR work with espeak/speech-dispatcher
- [ ] Handle missing language packs gracefully

### Settings Storage
- [ ] Implement `JsonSettingsService` for Linux
- [ ] Use `~/.config/babysmash/settings.json`
- [ ] Test settings persist across app restarts on Linux
- [ ] Handle missing directories gracefully

### Integration Testing
- [ ] Test full workflow on Linux: keypress → shape → sound → speech
- [ ] Test keyboard blocking (Alt+Tab, Super key, etc.) on X11
- [ ] Test keyboard blocking on Wayland
- [ ] Test multi-monitor support on Linux

---

## Phase 5: Auto-Update & Packaging (1-2 weeks)

**Goal**: Implement auto-update and Linux packaging

### Updatum Integration
- [ ] Configure Updatum for Linux asset patterns
- [ ] Update `App.axaml.cs` with Linux update logic
- [ ] Test update check on Linux
- [ ] Test download and install on Linux

### Linux Packaging
- [ ] Create AppImage build process
- [ ] Test AppImage on multiple distros
- [ ] Create tarball (.tar.gz) distribution
- [ ] (Optional) Create .deb package for Debian/Ubuntu
- [ ] (Optional) Create .rpm package for Fedora/RHEL
- [ ] (Optional) Create Flatpak manifest
- [ ] (Optional) Create Snap package

### Testing
- [ ] Test auto-update downloads new version
- [ ] Test auto-update installs correctly
- [ ] Verify packages work across distributions

---

## Phase 6: GitHub Actions Linux Build (1 week)

**Goal**: Automate Linux builds

### Workflow Setup
- [ ] Create `.github/workflows/build-linux.yml` or extend existing workflow
- [ ] Configure build matrix for Linux platforms
- [ ] Setup Linux x64 build job
- [ ] (Optional) Setup Linux ARM64 build job (for Raspberry Pi)

### Linux Build
- [ ] Publish for linux-x64
- [ ] Create AppImage
- [ ] Create tarball (.tar.gz)
- [ ] (Optional) Create .deb package
- [ ] (Optional) Create .rpm package
- [ ] Upload artifacts

### Release Creation
- [ ] Configure release creation on tag push
- [ ] Auto-generate release notes
- [ ] Upload Linux artifacts alongside Windows WPF artifacts
- [ ] Test release workflow end-to-end

### Testing
- [ ] Push test tag, verify builds trigger
- [ ] Download Linux artifacts, verify they work
- [ ] Test auto-update finds new release from Linux

---

## Phase 7: Testing & Polish (1-2 weeks)

**Goal**: Comprehensive testing on Linux and UX improvements

### Functionality Testing - Windows (WPF - ensure no regressions)
- [ ] WPF version still works (no regressions from Core extraction)
- [ ] All existing features continue to function

### Functionality Testing - Linux (Avalonia - new)
- [ ] All shapes display correctly
- [ ] Animations run smoothly (60 FPS)
- [ ] Audio plays on keypress
- [ ] Text-to-speech works (espeak/speech-dispatcher)
- [ ] Multi-monitor support
- [ ] Options dialog saves settings
- [ ] Auto-update works
- [ ] Keyboard shortcuts work (Ctrl+Alt+Shift+O, Alt+F4)
- [ ] Keyboard blocking works (Alt+Tab, Super key)

### Performance Testing
- [ ] CPU usage < 5% when idle on Linux
- [ ] CPU usage < 20% during rapid keypress on Linux
- [ ] Memory usage < 200MB on Linux
- [ ] Startup time < 2 seconds on Linux
- [ ] No lag during 10-minute keyboard mashing session

### Platform-Specific Testing on Linux
- [ ] Test on Ubuntu 22.04+ (GNOME, X11 and Wayland)
- [ ] Test on Fedora (GNOME, Wayland)
- [ ] Test on Arch Linux (KDE, X11)
- [ ] Test on Linux Mint (Cinnamon)
- [ ] (Optional) Test on Raspberry Pi OS (ARM64)
- [ ] Test with different window managers (GNOME, KDE, XFCE, etc.)

### UI Polish
- [ ] Consistent look with Linux desktop themes
- [ ] Native window decorations
- [ ] Proper icon integration with Linux desktops
- [ ] High DPI support on Linux

### Accessibility (Bonus)
- [ ] Keyboard navigation works
- [ ] Screen reader compatibility (test with Orca on Linux)

### Bug Fixes
- [ ] Fix all critical bugs found during Linux testing
- [ ] Address performance issues on Linux
- [ ] Polish rough edges

---

## Documentation & Release

### Documentation
- [x] Planning document (AVALONIA_PORT_PLAN.md) - UPDATED for Linux focus
- [x] Architecture document (docs/AVALONIA_ARCHITECTURE.md) - UPDATED for Linux focus
- [x] Quick start guide (docs/AVALONIA_QUICKSTART.md) - UPDATED for Linux focus
- [ ] Update main README with Linux instructions
- [ ] Add Linux-specific installation guide
- [ ] Create BUILDING_LINUX.md for contributors
- [ ] Document known issues and workarounds for Linux

### Release Preparation
- [ ] Decide on version number (v5.0.0 for first Linux release?)
- [ ] Write release notes highlighting Linux support
- [ ] Create changelog
- [ ] Update all documentation links

### Community
- [ ] Announce Linux port on blog/social media
- [ ] Recruit Linux testers from community
- [ ] Open GitHub Discussions for Linux user feedback
- [ ] Engage with Linux-focused communities (r/linux, etc.)

---

## Post-Release

### Monitoring
- [ ] Monitor GitHub issues for Linux-specific bugs
- [ ] Track download metrics for Linux packages
- [ ] Gather user feedback

### Iteration
- [ ] Address high-priority Linux bugs
- [ ] Add requested Linux-specific features
- [ ] Optimize performance based on Linux user feedback

### Community Growth
- [ ] Celebrate first Linux contributors
- [ ] Highlight Linux user success stories

---

## Success Metrics

- [ ] Linux version builds without errors
- [ ] All core features work on Linux (Ubuntu, Fedora, Arch)
- [ ] At least 10 downloads of Linux version in first month
- [ ] At least 1 community contribution from Linux users (PR or detailed bug report)
- [ ] Positive feedback from Linux community
- [ ] No regressions in Windows WPF version

---

**Implementation Timeline**: 2-3 months  
**Estimated Effort**: ~150-200 developer hours  
**Cost**: ~$0/year (no code signing needed for Linux)

**Status**: Planning Updated for Linux Focus - Ready for Phase 1

---

## Quick Links

- [Full Planning Document](../AVALONIA_PORT_PLAN.md)
- [Architecture Details](AVALONIA_ARCHITECTURE.md)
- [Quick Start Guide](AVALONIA_QUICKSTART.md)
- [Main README](../README.md)
