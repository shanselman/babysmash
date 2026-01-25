# Avalonia Port Implementation Checklist

This checklist tracks the implementation of the Avalonia port based on the [AVALONIA_PORT_PLAN.md](../AVALONIA_PORT_PLAN.md).

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

**Goal**: Create basic Avalonia project structure

### Project Creation
- [ ] Create `BabySmash.Avalonia` project using Avalonia template
- [ ] Configure project to target `net10.0`
- [ ] Set RuntimeIdentifiers: `win-x64;osx-x64;osx-arm64;linux-x64`
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
- [ ] Setup dependency injection in `App.axaml.cs`
- [ ] Configure resource embedding

### Platform Service Stubs
- [ ] Create `Platform/Windows/` directory
- [ ] Create `Platform/MacOS/` directory
- [ ] Create `Platform/Linux/` directory
- [ ] Create stub implementations for all services (return NotImplementedException)

### Build Configuration
- [ ] Configure single-file publishing for all platforms
- [ ] Test build on Windows: `dotnet build`
- [ ] Test publish for Windows: `dotnet publish -r win-x64`
- [ ] (If on Mac) Test publish for macOS: `dotnet publish -r osx-x64`
- [ ] (If on Linux) Test publish for Linux: `dotnet publish -r linux-x64`

### Verification
- [ ] App launches and shows empty window
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

### Windows Platform Services
- [ ] Implement `WindowsTtsService` using `System.Speech`
- [ ] Implement `WindowsAudioService` using NAudio or existing P/Invoke
- [ ] Implement `WindowsKeyboardHookService` using existing Win32 hooks
- [ ] Test all services on Windows

### macOS Platform Services
- [ ] Research macOS TTS options (AVSpeechSynthesizer or 'say' command)
- [ ] Implement `MacOSTtsService`
- [ ] Research macOS audio options (AVFoundation or NAudio)
- [ ] Implement `MacOSAudioService`
- [ ] Research macOS keyboard hooks (CGEventTap + Accessibility permissions)
- [ ] Implement `MacOSKeyboardHookService`
- [ ] Create permission request dialog for Accessibility
- [ ] Test all services on macOS

### Linux Platform Services
- [ ] Research Linux TTS options (espeak, speech-dispatcher)
- [ ] Implement `LinuxTtsService`
- [ ] Research Linux audio options (ALSA, PulseAudio, NAudio)
- [ ] Implement `LinuxAudioService`
- [ ] Research Linux keyboard hooks (X11 XGrabKeyboard, Wayland)
- [ ] Implement `LinuxKeyboardHookService`
- [ ] Handle permission requirements gracefully
- [ ] Test all services on Linux (Ubuntu, Fedora, Arch)

### Cross-Platform Audio
- [ ] Evaluate NAudio for cross-platform use
- [ ] Test WAV playback on all platforms
- [ ] Ensure embedded resources load correctly

### Localization
- [ ] Test TTS with multiple languages on each platform
- [ ] Verify en-EN, ru-RU, pt-BR work on Windows
- [ ] Verify localization on macOS
- [ ] Verify localization on Linux

### Settings Storage
- [ ] Implement `JsonSettingsService` with platform-specific directories
- [ ] Test settings persist across app restarts on all platforms
- [ ] Test migration from WPF settings (Windows only)

### Integration Testing
- [ ] Test full workflow on Windows: keypress → shape → sound → speech
- [ ] Test full workflow on macOS
- [ ] Test full workflow on Linux
- [ ] Test keyboard blocking (Alt+Tab, Win key, etc.) on each platform

---

## Phase 5: Auto-Update & Signing (2 weeks)

**Goal**: Implement auto-update and code signing

### Updatum Integration
- [ ] Configure Updatum for multi-platform asset patterns
- [ ] Update `App.axaml.cs` with platform-specific update logic
- [ ] Test update check on all platforms
- [ ] Test download and install on Windows
- [ ] Test download and install on macOS
- [ ] Test download and install on Linux

### Code Signing - Windows
- [ ] Verify Azure Trusted Signing works with Avalonia build
- [ ] Test signed executable launches without SmartScreen warning

### Code Signing - macOS
- [ ] Enroll in Apple Developer Program ($99/year)
- [ ] Create Developer ID Application certificate
- [ ] Sign .app bundle with codesign
- [ ] Notarize with Apple notarization service
- [ ] Staple notarization ticket
- [ ] Test signed app launches without warning
- [ ] Test on macOS with Gatekeeper enabled

### Code Signing - Linux
- [ ] Determine if signing needed (AppImage, Flatpak)
- [ ] Implement if beneficial

### Testing
- [ ] Test auto-update downloads new version
- [ ] Test auto-update installs correctly
- [ ] Verify signed apps launch without warnings

---

## Phase 6: GitHub Actions Multi-Platform Build (1 week)

**Goal**: Automate builds for all platforms

### Workflow Setup
- [ ] Create `.github/workflows/build-avalonia.yml`
- [ ] Configure build matrix for platforms
- [ ] Setup Windows build job (win-x64)
- [ ] Setup macOS build job (osx-x64, osx-arm64)
- [ ] Setup Linux build job (linux-x64)

### Windows Build
- [ ] Publish single-file executable
- [ ] Sign with Azure Trusted Signing
- [ ] Create ZIP with exe + README
- [ ] Upload artifact

### macOS Build
- [ ] Publish for osx-x64 and osx-arm64
- [ ] Create .app bundle
- [ ] Sign with Apple Developer ID
- [ ] Notarize with Apple
- [ ] Create DMG installer
- [ ] Upload artifacts

### Linux Build
- [ ] Publish for linux-x64
- [ ] Create AppImage (optional)
- [ ] Create tarball
- [ ] Upload artifact

### Release Creation
- [ ] Configure release creation on tag push
- [ ] Auto-generate release notes
- [ ] Upload all platform artifacts
- [ ] Test release workflow end-to-end

### Testing
- [ ] Push test tag, verify builds trigger
- [ ] Download artifacts, verify they work
- [ ] Test auto-update finds new release

---

## Phase 7: Testing & Polish (2 weeks)

**Goal**: Comprehensive testing and UX improvements

### Functionality Testing - Windows
- [ ] All shapes display correctly
- [ ] Animations run smoothly (60 FPS)
- [ ] Audio plays on keypress
- [ ] Text-to-speech works
- [ ] Multi-monitor support
- [ ] Options dialog saves settings
- [ ] Auto-update works
- [ ] Keyboard shortcuts work (Ctrl+Alt+Shift+O, Alt+F4)

### Functionality Testing - macOS
- [ ] All shapes display correctly
- [ ] Animations run smoothly
- [ ] Audio plays on keypress
- [ ] Text-to-speech works (multiple languages)
- [ ] Multi-monitor support
- [ ] Options dialog saves settings
- [ ] Auto-update works
- [ ] Keyboard shortcuts work
- [ ] Accessibility permissions requested correctly

### Functionality Testing - Linux
- [ ] All shapes display correctly
- [ ] Animations run smoothly
- [ ] Audio plays on keypress
- [ ] Text-to-speech works (espeak)
- [ ] Multi-monitor support (X11 and Wayland)
- [ ] Options dialog saves settings
- [ ] Auto-update works
- [ ] Keyboard shortcuts work

### Performance Testing
- [ ] CPU usage < 5% when idle (all platforms)
- [ ] CPU usage < 20% during rapid keypress (all platforms)
- [ ] Memory usage < 200MB (all platforms)
- [ ] Startup time < 2 seconds (all platforms)
- [ ] No lag during 10-minute keyboard mashing session

### Platform-Specific Testing
- [ ] Test on Windows 10, Windows 11
- [ ] Test on macOS 12+ (Intel and Apple Silicon)
- [ ] Test on Ubuntu 22.04+, Fedora, Arch Linux
- [ ] Test with different window managers (GNOME, KDE, etc.)

### UI Polish
- [ ] Consistent look across platforms
- [ ] Native window decorations where appropriate
- [ ] Platform-appropriate icons
- [ ] High DPI support on all platforms

### Accessibility (Bonus)
- [ ] Keyboard navigation works
- [ ] Screen reader compatibility (test with NVDA on Windows)

### Bug Fixes
- [ ] Fix all critical bugs found during testing
- [ ] Address performance issues
- [ ] Polish rough edges

---

## Documentation & Release

### Documentation
- [x] Planning document (AVALONIA_PORT_PLAN.md) - COMPLETED
- [x] Architecture document (docs/AVALONIA_ARCHITECTURE.md) - COMPLETED
- [x] Quick start guide (docs/AVALONIA_QUICKSTART.md) - COMPLETED
- [ ] Update main README with Avalonia instructions
- [ ] Add platform-specific installation guides
- [ ] Create BUILDING_AVALONIA.md for contributors
- [ ] Document known issues and workarounds

### Release Preparation
- [ ] Decide on version number (v5.0.0 for first Avalonia release?)
- [ ] Write release notes
- [ ] Create changelog
- [ ] Update all documentation links

### Community
- [ ] Announce Avalonia port on blog/social media
- [ ] Recruit platform testers (macOS, Linux users)
- [ ] Open GitHub Discussions for feedback
- [ ] Consider creating a Discord or Slack channel

---

## Post-Release

### Monitoring
- [ ] Monitor GitHub issues for platform-specific bugs
- [ ] Track download metrics per platform
- [ ] Gather user feedback

### Iteration
- [ ] Address high-priority bugs
- [ ] Add requested features
- [ ] Optimize performance based on feedback

### Community Growth
- [ ] Celebrate first macOS/Linux contributors
- [ ] Highlight success stories
- [ ] Consider additional platforms (mobile?)

---

## Success Metrics

- [ ] Avalonia app builds on all platforms without errors
- [ ] All core features work on Windows, macOS, Linux
- [ ] At least 10 downloads of non-Windows versions in first month
- [ ] At least 1 community contribution (PR or detailed bug report)
- [ ] Positive feedback from macOS/Linux users
- [ ] No major regressions in WPF version

---

**Implementation Timeline**: 3-4 months  
**Estimated Effort**: ~200-300 developer hours  
**Cost**: ~$120/year (Apple Developer Program)

**Status**: Planning Complete - Ready for Phase 1

---

## Quick Links

- [Full Planning Document](../AVALONIA_PORT_PLAN.md)
- [Architecture Details](AVALONIA_ARCHITECTURE.md)
- [Quick Start Guide](AVALONIA_QUICKSTART.md)
- [Main README](../README.md)
