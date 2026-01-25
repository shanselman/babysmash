# BabySmash! Avalonia Port - Planning Document

## Executive Summary

This document outlines the strategy for creating, maintaining, and releasing an **Avalonia port** of BabySmash!, enabling the application to run on **Linux** while maintaining the existing WPF version for Windows.

**Current State**: WPF application targeting .NET 10, Windows-only  
**Target State**: WPF for Windows (existing) + Avalonia for Linux (new)  
**Timeline**: 2-3 month phased implementation  
**Maintenance Strategy**: Shared business logic, platform-specific UI

---

## Table of Contents

1. [Why Avalonia?](#why-avalonia)
2. [Architecture Strategy](#architecture-strategy)
3. [Technical Analysis](#technical-analysis)
4. [Implementation Phases](#implementation-phases)
5. [Maintenance Strategy](#maintenance-strategy)
6. [Release Strategy](#release-strategy)
7. [Risk Assessment](#risk-assessment)
8. [Success Criteria](#success-criteria)

---

## Why Avalonia for Linux?

### Benefits
- **Linux Support**: Bring BabySmash to Linux users and education environments
- **Modern .NET**: Built for .NET 6+ with excellent performance
- **XAML Similarity**: Minimal learning curve - reuse WPF XAML with minor changes
- **Active Development**: Strong community and commercial backing
- **Raspberry Pi**: Great for educational use on ARM Linux devices

### Use Cases for Linux BabySmash
- **Linux Desktop**: Parents using Ubuntu, Fedora, or other Linux distributions
- **Education Environments**: Schools and libraries running Linux
- **Raspberry Pi**: Educational computing devices (x64 and ARM64)
- **ChromeOS Linux Mode**: Chromebooks with Linux container support
- **Community Growth**: Expand user base to the Linux community

---

## Architecture Strategy

### Option 1: Dual Codebase (RECOMMENDED)
Maintain separate WPF (Windows) and Avalonia (Linux) projects with **shared business logic**.

```
BabySmash.sln
├── BabySmash.csproj (existing - WPF for Windows)
│   ├── WPF-specific UI (XAML, code-behind)
│   └── Windows-specific features (keyboard hooks, System.Speech)
├── BabySmash.Linux (new - Avalonia for Linux)
│   ├── Avalonia UI (XAML ported from WPF)
│   └── Linux-specific implementations
└── BabySmash.Core (new - shared library)
    ├── Controller.cs (platform-agnostic logic)
    ├── Shapes (core shape definitions)
    ├── WordFinder.cs
    ├── Settings.cs (abstracted storage)
    └── Models & ViewModels
```

**Pros**:
- Clean separation of concerns
- Windows version remains unchanged (WPF)
- Linux gets native support via Avalonia
- Lower risk - only adding Linux support
- Can ship incrementally

**Cons**:
- Two UI codebases to maintain (but shared core logic)
- Need to sync major features between platforms

### Option 2: Full Migration (NOT RECOMMENDED)
Replace WPF entirely with Avalonia.

**Pros**: Single codebase  
**Cons**: Lose Windows-specific features, high risk, all-or-nothing approach

### Decision: **Option 1 - Dual Codebase**

---

## Technical Analysis

### Current WPF Dependencies

| Component | WPF Implementation (Windows) | Avalonia for Linux | Complexity |
|-----------|------------------------------|-------------------|------------|
| **XAML UI** | WPF XAML | Avalonia XAML | ⚠️ Medium (95% compatible) |
| **Shapes** | 14 custom UserControls | Port to Avalonia UserControls | ✅ Low |
| **Animations** | WPF Storyboards | Avalonia Animations | ✅ Low |
| **Text-to-Speech** | System.Speech (Windows) | espeak/speech-dispatcher | ⚠️ Medium |
| **Keyboard Hooks** | Win32 P/Invoke | X11/Wayland capture | 🔴 High |
| **Audio** | P/Invoke winmm.dll | NAudio or ALSA/PulseAudio | ⚠️ Medium |
| **Multi-Monitor** | System.Windows.Forms.Screen | Avalonia.Platform.Screen | ✅ Low |
| **Settings** | .NET Settings (Windows) | JSON in ~/.config | ✅ Low |
| **Auto-Update** | Updatum (GitHub Releases) | Same (Updatum works on Linux) | ✅ Low |

### Platform-Specific Challenges

#### 1. Text-to-Speech
**WPF (Windows)**: `System.Speech.Synthesis.SpeechSynthesizer`

**Avalonia (Linux)**: Use `espeak` or `speech-dispatcher` via CLI or P/Invoke

**Implementation Options**:
- **espeak**: Widely available on Linux, lightweight, supports many languages
- **speech-dispatcher**: More sophisticated, better voice quality
- **Festival**: Alternative TTS engine

**Recommendation**: Create `ITtsService` interface with platform implementations

```csharp
public interface ITtsService
{
    void Speak(string text);
    void SetLanguage(CultureInfo culture);
}

// Implementations:
// - TtsService.Windows.cs (System.Speech) - existing WPF
// - TtsService.Linux.cs (espeak wrapper) - new Avalonia
```

#### 2. Keyboard Hooks
**WPF (Windows)**: Low-level keyboard hook via Win32 API to intercept ALL keys (prevent Alt+Tab, etc.)

**Avalonia (Linux)**: Use `XGrabKeyboard` (X11) or input capture on Wayland

**Implementation Options**:
- **X11**: `XGrabKeyboard` for full keyboard capture
- **Wayland**: May require different approach (compositor-specific)
- **Fallback**: Use standard Avalonia keyboard events (less comprehensive)

**Challenge**: Permission requirements on Linux (may need specific user groups)  
**Recommendation**: Implement `IKeyboardHookService` with Linux-specific implementation

#### 3. Audio Playback
**WPF (Windows)**: Direct P/Invoke to `winmm.dll` for WAV playback

**Avalonia (Linux)**: Use NAudio or native ALSA/PulseAudio

**Implementation Options**:
- **NAudio**: Works on Linux with ALSA/PulseAudio backend
- **ALSA direct**: Lower-level Linux audio
- **PulseAudio**: Higher-level audio server (most common on modern Linux)
- **System.Media.SoundPlayer**: Simple but limited

**Recommendation**: Use `NAudio` with `IAudioService` abstraction

```csharp
public interface IAudioService
{
    void PlaySound(string resourceName);
}
// - AudioService.Windows.cs (winmm.dll) - existing WPF
// - AudioService.Linux.cs (NAudio/ALSA) - new Avalonia
```

#### 4. Settings Storage
**WPF (Windows)**: Uses `Properties.Settings` (Windows Registry or user.config XML)

**Avalonia (Linux)**: Store JSON settings in user directory
- Linux: `~/.config/babysmash/settings.json`

**Implementation**: Use simple `System.Text.Json` serialization

```csharp
public interface ISettingsService
{
    T Get<T>(string key, T defaultValue);
    void Set<T>(string key, T value);
    void Save();
}
```

### XAML Compatibility

Most WPF XAML translates directly to Avalonia with minor changes:

| WPF Feature | Avalonia Equivalent | Notes |
|-------------|---------------------|-------|
| `Window` | `Window` | ✅ Same |
| `Grid`, `StackPanel`, `Canvas` | Same | ✅ Same |
| `TextBlock`, `Button` | Same | ✅ Same |
| `UserControl` | `UserControl` | ✅ Same |
| `Storyboard` | `Storyboard` (in `Animation` namespace) | ⚠️ Slightly different API |
| `BitmapEffect` | `Effect` (use `DropShadowEffect`) | ⚠️ Already migrated in WPF version |
| `DispatcherTimer` | `DispatcherTimer` | ✅ Same |
| `x:Name` | `x:Name` or `Name` | ✅ Same |

**Incompatibilities**:
- No `AllowsTransparency` on Windows - use compositor APIs instead
- Some easing functions have different names
- `BitmapEffect` is already removed in current WPF version (good!)

---

## Implementation Phases

### Phase 1: Project Setup & Shared Core (2-3 weeks)

**Goal**: Extract shared business logic into `BabySmash.Core`

#### Tasks
- [ ] Create `BabySmash.Core` (.NET Standard 2.0 or .NET 6+ class library)
- [ ] Move platform-agnostic code to Core:
  - [ ] `Controller.cs` (refactor to remove WPF dependencies)
  - [ ] `WordFinder.cs`
  - [ ] Shape models/definitions
  - [ ] `Settings.cs` (create abstraction)
  - [ ] Extension methods
- [ ] Define platform abstraction interfaces:
  - [ ] `ITtsService` (text-to-speech)
  - [ ] `IAudioService` (sound playback)
  - [ ] `IKeyboardHookService` (keyboard interception)
  - [ ] `ISettingsService` (cross-platform settings)
  - [ ] `IScreenService` (multi-monitor support)
- [ ] Update WPF project to reference `BabySmash.Core`
- [ ] Verify WPF app still works after refactoring

**Deliverable**: Working WPF app using shared Core library

---

### Phase 2: Avalonia Project Bootstrap (1 week)

**Goal**: Create basic Avalonia project structure for Linux

#### Tasks
- [ ] Create `BabySmash.Linux` Avalonia project
  - Target: `net10.0` with Avalonia 11.x
  - Platform: `linux-x64` (and optionally `linux-arm64` for Raspberry Pi)
- [ ] Add Avalonia NuGet packages:
  ```xml
  <PackageReference Include="Avalonia" Version="11.2.0" />
  <PackageReference Include="Avalonia.Desktop" Version="11.2.0" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.0" />
  ```
- [ ] Reference `BabySmash.Core` project
- [ ] Create basic `App.axaml` and `MainWindow.axaml`
- [ ] Embed resources (sounds, Words.txt, localization JSON)
- [ ] Configure Linux publishing

**Deliverable**: Empty Avalonia app that launches on Linux

---

### Phase 3: Port Core UI & Shapes (3-4 weeks)

**Goal**: Implement main game UI with shapes and animations

#### Tasks
- [ ] Port `MainWindow.xaml` to `MainWindow.axaml`
  - Canvas layout for shapes
  - Info label (Ctrl+Alt+Shift+O, Alt+F4 instructions)
  - Update notification area
- [ ] Port 14 shape UserControls to Avalonia:
  - `CoolCircle`, `CoolOval`, `CoolSquare`, `CoolStar`, etc.
  - Convert animations to Avalonia `Animation` API
  - Test face animations and colors
- [ ] Port `Options.xaml` to `Options.axaml`
  - Complex 64KB XAML with many controls
  - May need to simplify or redesign for cross-platform
- [ ] Implement multi-monitor support
  - Use `Avalonia.Platform.Screen.AllScreens`
  - Create window per monitor (same as WPF)

**Deliverable**: Avalonia app displays shapes on keypress (without sound/speech)

---

### Phase 4: Platform Services Implementation (2-3 weeks)

**Goal**: Implement Linux-specific services (TTS, audio, keyboard hooks)

#### Text-to-Speech
- [ ] Linux: `TtsService.Linux.cs` using `espeak` or `speech-dispatcher`
- [ ] Test with multiple languages (en, ru, pt)
- [ ] Handle missing TTS gracefully (fallback to no speech)

#### Audio Playback
- [ ] Choose audio library (NAudio with ALSA/PulseAudio backend)
- [ ] Implement `AudioService.Linux.cs` for WAV playback
- [ ] Embed WAV resources (10 sound files)
- [ ] Test on Ubuntu, Fedora, Arch Linux

#### Keyboard Hooks
- [ ] Linux: Implement using X11 `XGrabKeyboard` or Wayland equivalent
- [ ] Handle both X11 and Wayland display servers
- [ ] Test Alt+Tab blocking, Super key blocking
- [ ] Handle permission requirements (user groups if needed)

#### Settings Storage
- [ ] Implement `SettingsService.Linux.cs`
- [ ] Use JSON storage in `~/.config/babysmash/settings.json`
- [ ] Test settings persist across app restarts

**Deliverable**: Fully functional Linux app with sound, speech, and keyboard hooks

---

### Phase 5: Auto-Update & Packaging (1-2 weeks)

**Goal**: Implement auto-update and Linux packaging

#### Auto-Update (Updatum)
- [ ] Configure Updatum for Linux
  - Linux: `BabySmash-linux-x64.zip` or `BabySmash-linux-x64.tar.gz`
- [ ] Port `UpdateDialog.xaml` to Avalonia (if not already done)
- [ ] Port `DownloadProgressDialog.xaml` to Avalonia (if not already done)
- [ ] Test update flow on Linux

#### Linux Packaging
- [ ] **AppImage**: Create portable AppImage (most compatible)
- [ ] **Optional**: Flatpak manifest for Flathub
- [ ] **Optional**: Snap package for Snap Store
- [ ] **Optional**: .deb package for Debian/Ubuntu
- [ ] **Optional**: .rpm package for Fedora/RHEL

**Deliverable**: Auto-updating Linux app with portable distribution format

---

### Phase 6: GitHub Actions Linux Build (1 week)

**Goal**: Automate Linux builds

#### Tasks
- [ ] Create `.github/workflows/build-linux.yml` or extend existing workflow
- [ ] Build matrix for Linux platforms:
  - `ubuntu-latest`: linux-x64
  - Optional: linux-arm64 for Raspberry Pi
- [ ] Create Linux packages:
  - AppImage (primary)
  - Tarball (.tar.gz)
  - Optional: .deb, .rpm, Flatpak, Snap
- [ ] Upload to GitHub Releases

**Sample Workflow Structure**:
```yaml
jobs:
  build-linux:
    runs-on: ubuntu-latest
    steps:
      - Build linux-x64
      - Create AppImage
      - Create tarball
      - Upload artifacts
```

**Deliverable**: Automated Linux releases on GitHub

---

### Phase 7: Testing & Polish (1-2 weeks)

**Goal**: Comprehensive testing on Linux and user experience improvements

#### Tasks
- [ ] **Functionality Testing on Linux**:
  - All shapes display correctly
  - Animations work smoothly (60 FPS)
  - Audio plays on keypress
  - Text-to-speech works (espeak/speech-dispatcher)
  - Multi-monitor support
  - Options dialog saves settings
  - Auto-update downloads and installs
  - Keyboard shortcuts work (Ctrl+Alt+Shift+O, Alt+F4)
- [ ] **Platform-Specific Testing**:
  - Test on Ubuntu 22.04+ (GNOME, X11 and Wayland)
  - Test on Fedora (GNOME, Wayland)
  - Test on Arch Linux (KDE, X11)
  - Test on Linux Mint (Cinnamon)
  - Optional: Test on Raspberry Pi OS (ARM64)
- [ ] **Performance Testing**:
  - No lag when mashing keyboard rapidly
  - Low CPU usage when idle (<5%)
  - Memory usage stays reasonable (<200MB)
- [ ] **Accessibility**:
  - Keyboard navigation
  - Screen reader compatibility (Orca on Linux)
- [ ] **UI Polish**:
  - Consistent look with Linux desktop themes
  - Native window decorations
  - Proper icon integration

**Deliverable**: Production-ready Linux version

---

## Maintenance Strategy

### 1. Shared Core Library
All business logic lives in `BabySmash.Core`:
- Shape generation logic
- Word finding
- Settings management (abstracted)
- Animation definitions
- Color palettes

**Benefit**: Fix once, both Windows (WPF) and Linux (Avalonia) benefit

### 2. Platform-Specific UI Projects
- `BabySmash.csproj`: Windows WPF version (existing, no changes)
- `BabySmash.Linux`: Linux Avalonia version (new)

**When to change**:
- **Core change** (new shape, word list update): Update `BabySmash.Core`
- **UI improvement**: Update both WPF and Avalonia (or decide platform priority)
- **Platform-specific feature**: Update only that platform's project

### 3. Feature Parity Strategy

**Approach**: WPF (Windows) remains the **primary** platform, Linux follows

- New features developed in WPF first (existing userbase)
- Port to Linux within same or next release cycle
- Eventually, both platforms reach feature parity
- Linux-specific features (if any) can be Linux-only

### 4. Documentation
- Update README with platform-specific installation instructions
- Create `BUILDING_AVALONIA.md` for contributors
- Maintain platform compatibility matrix

---

## Release Strategy

### Versioning
Use **semantic versioning** with platform indication in release notes:

- `v5.0.0` - First Linux release (major version bump)
- `v5.0.1` - Bugfix for both platforms
- `v5.1.0` - New feature in both WPF and Linux

### Release Artifacts

#### GitHub Releases
Each release includes:

| File | Platform | Description |
|------|----------|-------------|
| `BabySmash-Setup.exe` | Windows | WPF installer (existing) |
| `BabySmash-win-x64.zip` | Windows | WPF portable (existing, Updatum target) |
| `BabySmash-linux-x64.AppImage` | Linux | Avalonia AppImage (new) |
| `BabySmash-linux-x64.tar.gz` | Linux | Avalonia portable (new) |
| `BabySmash-linux-arm64.AppImage` | Linux (ARM) | Optional: Raspberry Pi support |

### Distribution Channels

#### Windows
- **Primary**: GitHub Releases (WPF version - existing, no changes)
- **Future**: Microsoft Store (if desired)

#### Linux
- **Primary**: GitHub Releases (AppImage + tarball)
- **Future**: Flatpak (Flathub) - community-maintained
- **Future**: Snap Store - community-maintained
- **Future**: Distribution repos (apt, dnf, pacman) - community-maintained

### Update Strategy

#### Updatum Configuration
Configure Updatum to support Windows (WPF) and Linux (Avalonia) flavors:

```csharp
// WPF version (Windows-only) - existing, no changes
var wpfUpdater = new UpdatumManager("shanselman", "babysmash")
{
    AssetRegexPattern = "BabySmash-win-x64.zip",
};

// Linux Avalonia version (new)
var linuxUpdater = new UpdatumManager("shanselman", "babysmash")
{
    AssetRegexPattern = "BabySmash-linux-x64.(AppImage|tar.gz)",
};
```

---

## Risk Assessment

### Technical Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|------------|------------|
| Avalonia XAML incompatibilities | Medium | Medium | Incremental porting, test each shape |
| Linux keyboard hook permissions | High | High | Clear documentation, graceful fallback |
| Linux TTS quality/availability | Medium | Medium | Provide alternative (visual-only mode) |
| X11 vs Wayland differences | Medium | Medium | Test both, provide compatibility layer |
| Audio issues on some Linux distros | Low | Low | Use well-tested library (NAudio with ALSA/PulseAudio) |
| Performance on older hardware | Low | Low | Profile early, optimize as needed |

### Business Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Maintenance burden (2 UI codebases) | Low | Shared Core library reduces duplication to ~20% |
| Limited Linux user demand | Low | Release as beta, gauge interest |
| Linux distro fragmentation | Medium | Use AppImage (universal), test on major distros |
| Support burden from Linux users | Medium | Community-driven support, clear docs |

### Mitigation Plan
1. **Shared Core Library**: 80% of code shared, only 20% platform-specific UI
2. **Gradual Rollout**: Release Linux version as "beta" initially, gather feedback
3. **Community Engagement**: Recruit Linux testers from community
4. **Fallback Options**: If a feature is unavailable on Linux, degrade gracefully (e.g., no TTS → visual-only mode)

---

## Success Criteria

### Functional
- [ ] All shapes display correctly on Linux
- [ ] Animations run smoothly (60 FPS)
- [ ] Audio plays on keypress
- [ ] Text-to-speech works (espeak/speech-dispatcher)
- [ ] Multi-monitor support works
- [ ] Options dialog saves settings
- [ ] Auto-update downloads and installs updates
- [ ] Keyboard shortcuts work (Ctrl+Alt+Shift+O, Alt+F4)

### Platform-Specific
- [ ] **Windows**: WPF version continues to work (no regressions)
- [ ] **Linux**: AppImage runs on Ubuntu, Fedora, Arch, Linux Mint
- [ ] **Linux**: Works on both X11 and Wayland
- [ ] **Linux (Optional)**: Works on Raspberry Pi (ARM64)

### Quality
- [ ] No crashes during 10-minute keyboard mashing session
- [ ] CPU usage < 5% when idle
- [ ] Memory usage < 200MB
- [ ] Startup time < 2 seconds

### Community
- [ ] 10+ downloads of Linux version in first month
- [ ] At least 1 community contribution (bug report or PR)
- [ ] Positive feedback from Linux users
- [ ] No regression issues from Windows WPF users

---

## Cost Estimate

### Development Time
- **Phase 1-3** (Core + Basic UI): 4-6 weeks
- **Phase 4** (Linux Services): 2-3 weeks
- **Phase 5-7** (Update, Packaging, Testing): 2-3 weeks

**Total**: ~2-3 months for v1.0 Linux release

### Ongoing Costs
- **GitHub Actions**: Free (within limits)
- **No code signing costs for Linux** (optional for Flatpak/Snap)

**Total**: ~$0/year incremental cost (Linux doesn't require paid code signing)

### Opportunity Cost
- Existing WPF development continues in parallel
- No blocking of current feature roadmap
- Can be done incrementally (community contributions welcome)

---

## Conclusion

The Avalonia port of BabySmash! for **Linux** is **technically feasible** and **strategically valuable** for reaching Linux users while keeping the existing Windows WPF version unchanged. The dual-codebase approach with a shared `BabySmash.Core` library minimizes maintenance burden.

### Recommended Approach
1. **Phase 1-2** (4-5 weeks): Extract Core, create Avalonia skeleton for Linux
2. **Phase 3-4** (4-6 weeks): Port UI and implement Linux-specific services
3. **Phase 5-7** (2-3 weeks): Auto-update, packaging, testing
4. **Release**: v5.0.0 with WPF (Windows) and Avalonia (Linux)

### Next Steps
1. **Approval**: Decide if Linux support aligns with project goals
2. **Resource Allocation**: Assign developer time or open to community contributions
3. **Pilot**: Start with Phase 1 (Core library extraction) as low-risk first step
4. **Community**: Announce intent, gauge interest from Linux users

**Decision Point**: Proceed with Phase 1 or defer until more demand is evident?

---

## Appendix: Avalonia Resources

### Documentation
- [Avalonia Docs](https://docs.avaloniaui.net/)
- [WPF to Avalonia Migration Guide](https://docs.avaloniaui.net/guides/migration/wpf)
- [Avalonia Samples](https://github.com/AvaloniaUI/Avalonia.Samples)

### Similar Projects
- [Avalonia Music Store](https://github.com/AvaloniaUI/Avalonia.MusicStore) - Complete sample app
- [Markdown Monster (WPF to Avalonia)](https://weblog.west-wind.com/posts/2023/Nov/07/Migrating-from-WPF-to-Avalonia) - Migration experience

### Community
- [Avalonia Discord](https://discord.gg/avalonia)
- [Avalonia GitHub Discussions](https://github.com/AvaloniaUI/Avalonia/discussions)

---

**Document Version**: 1.0  
**Created**: January 2026  
**Author**: GitHub Copilot Workspace (AI Planning Agent)  
**Status**: DRAFT - Awaiting Review
