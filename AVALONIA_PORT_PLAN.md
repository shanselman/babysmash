# BabySmash! Avalonia Port - Planning Document

## Executive Summary

This document outlines the strategy for creating, maintaining, and releasing an **Avalonia port** of BabySmash!, enabling the application to run cross-platform on **Windows, macOS, and Linux** while maintaining feature parity with the current WPF version.

**Current State**: WPF application targeting .NET 10, Windows-only  
**Target State**: Dual-codebase approach with WPF (Windows) and Avalonia (cross-platform)  
**Timeline**: 3-4 month phased implementation  
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

## Why Avalonia?

### Benefits
- **Cross-Platform**: Run on Windows, macOS, Linux with native look-and-feel
- **Modern .NET**: Built for .NET 6+ with excellent performance
- **XAML Similarity**: Minimal learning curve for WPF developers
- **Active Development**: Strong community and commercial backing
- **Mobile Potential**: Future iOS/Android support possible

### Use Cases for Cross-Platform BabySmash
- **macOS**: Parents using MacBooks want the same experience
- **Linux**: Education environments (Raspberry Pi, ChromeOS Linux mode)
- **Tablets**: Future mobile port for iPad/Android tablets
- **Community Growth**: Broader platform support increases adoption

---

## Architecture Strategy

### Option 1: Dual Codebase (RECOMMENDED)
Maintain separate WPF and Avalonia projects with **shared business logic**.

```
BabySmash.sln
├── BabySmash.WPF (existing)
│   ├── WPF-specific UI (XAML, code-behind)
│   └── Windows-specific features (keyboard hooks, speech)
├── BabySmash.Avalonia (new)
│   ├── Avalonia UI (XAML with minimal changes)
│   └── Cross-platform equivalents
└── BabySmash.Core (new - shared library)
    ├── Controller.cs (platform-agnostic logic)
    ├── Shapes (core shape definitions)
    ├── WordFinder.cs
    ├── Settings.cs (abstracted storage)
    └── Models & ViewModels
```

**Pros**:
- Clean separation of concerns
- Platform-specific optimizations possible
- Lower risk (WPF remains unchanged)
- Can ship incrementally

**Cons**:
- More code to maintain
- Need to sync features across platforms

### Option 2: Full Migration (NOT RECOMMENDED)
Replace WPF entirely with Avalonia.

**Pros**: Single codebase  
**Cons**: Lose Windows-specific features, high risk, all-or-nothing approach

### Decision: **Option 1 - Dual Codebase**

---

## Technical Analysis

### Current WPF Dependencies

| Component | WPF Implementation | Avalonia Equivalent | Complexity |
|-----------|-------------------|---------------------|------------|
| **XAML UI** | WPF XAML | Avalonia XAML | ⚠️ Medium (95% compatible) |
| **Shapes** | 14 custom UserControls | Port to Avalonia UserControls | ✅ Low |
| **Animations** | WPF Storyboards | Avalonia Animations | ✅ Low |
| **Text-to-Speech** | System.Speech (Windows) | Platform-specific TTS | 🔴 High |
| **Keyboard Hooks** | Win32 P/Invoke | Platform-specific | 🔴 High |
| **Audio** | P/Invoke winmm.dll | Cross-platform audio library | ⚠️ Medium |
| **Multi-Monitor** | System.Windows.Forms.Screen | Avalonia.Platform.Screen | ✅ Low |
| **Settings** | .NET Settings (Windows) | Cross-platform storage | ⚠️ Medium |
| **Auto-Update** | Updatum (GitHub Releases) | Same (Updatum works cross-platform) | ✅ Low |

### Platform-Specific Challenges

#### 1. Text-to-Speech
**WPF**: `System.Speech.Synthesis.SpeechSynthesizer` (Windows-only)

**Avalonia Options**:
- **Windows**: Keep `System.Speech`
- **macOS**: Use `AVSpeechSynthesizer` (via Xamarin.iOS or native interop)
- **Linux**: Use `espeak` or `speech-dispatcher` via P/Invoke or CLI
- **Cross-platform library**: Consider `System.Speech` alternatives or plugin architecture

**Recommendation**: Create `ITtsService` interface with platform implementations

```csharp
public interface ITtsService
{
    void Speak(string text);
    void SetLanguage(CultureInfo culture);
}

// Implementations:
// - TtsService.Windows.cs (System.Speech)
// - TtsService.MacOS.cs (AVFoundation)
// - TtsService.Linux.cs (espeak wrapper)
```

#### 2. Keyboard Hooks
**WPF**: Low-level keyboard hook via Win32 API to intercept ALL keys (prevent Alt+Tab, etc.)

**Avalonia Options**:
- **Windows**: Keep existing Win32 P/Invoke hooks
- **macOS**: Use `CGEventTap` (requires Accessibility permissions)
- **Linux**: Use `XGrabKeyboard` (X11) or `libinput` (Wayland)

**Challenge**: Permission requirements vary by platform  
**Recommendation**: Platform-specific `IKeyboardHookService` implementations

#### 3. Audio Playback
**WPF**: Direct P/Invoke to `winmm.dll` for WAV playback

**Avalonia Options**:
- **Cross-platform**: Use **NAudio** (Windows/Linux) or **System.Media.SoundPlayer** (.NET cross-platform)
- **macOS**: Native `AVAudioPlayer` or NAudio
- **Alternative**: **BASS.NET** (free for non-commercial, excellent cross-platform support)

**Recommendation**: Use `NAudio` or create `IAudioService` abstraction

```csharp
public interface IAudioService
{
    void PlaySound(string resourceName);
}
```

#### 4. Settings Storage
**WPF**: Uses `Properties.Settings` (Windows Registry or user.config XML)

**Avalonia Options**:
- **Cross-platform**: Store JSON settings in user directory
  - Windows: `%APPDATA%\BabySmash\settings.json`
  - macOS: `~/Library/Application Support/BabySmash/settings.json`
  - Linux: `~/.config/babysmash/settings.json`
- **Library**: Use `Avalonia.Storage` or simple `System.Text.Json` serialization

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

**Goal**: Create basic Avalonia project structure and verify it builds

#### Tasks
- [ ] Create `BabySmash.Avalonia` project
  - Target: `net10.0` with Avalonia 11.x
  - Platforms: `win-x64`, `osx-x64`, `osx-arm64`, `linux-x64`
- [ ] Add Avalonia NuGet packages:
  ```xml
  <PackageReference Include="Avalonia" Version="11.2.0" />
  <PackageReference Include="Avalonia.Desktop" Version="11.2.0" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.0" />
  ```
- [ ] Reference `BabySmash.Core` project
- [ ] Create basic `App.axaml` and `MainWindow.axaml`
- [ ] Embed resources (sounds, Words.txt, localization JSON)
- [ ] Configure multi-platform publishing

**Deliverable**: Empty Avalonia app that launches on all platforms

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

### Phase 4: Platform Services Implementation (3-4 weeks)

**Goal**: Implement platform-specific services (TTS, audio, keyboard hooks)

#### Text-to-Speech
- [ ] Windows: `TtsService.Windows.cs` using `System.Speech`
- [ ] macOS: `TtsService.MacOS.cs` using `AVSpeechSynthesizer`
- [ ] Linux: `TtsService.Linux.cs` using `espeak` wrapper
- [ ] Test with multiple languages (en, ru, pt)

#### Audio Playback
- [ ] Choose cross-platform audio library (NAudio or System.Media)
- [ ] Implement `AudioService` for WAV playback
- [ ] Embed WAV resources (10 sound files)
- [ ] Test on all platforms

#### Keyboard Hooks
- [ ] Windows: Keep existing Win32 hooks (`KeyboardHook.cs`)
- [ ] macOS: Implement using `CGEventTap` (requires code signing + permissions)
- [ ] Linux: Implement using X11 `XGrabKeyboard` or Wayland equivalent
- [ ] Handle permission prompts gracefully
- [ ] Test Alt+Tab blocking, Windows key blocking

#### Settings Storage
- [ ] Implement cross-platform `SettingsService`
- [ ] Use JSON storage in platform-specific directories
- [ ] Migrate settings schema from WPF

**Deliverable**: Fully functional Avalonia app with sound, speech, and keyboard hooks

---

### Phase 5: Auto-Update & Signing (2 weeks)

**Goal**: Implement auto-update and code signing for all platforms

#### Auto-Update (Updatum)
- [ ] Configure Updatum for multi-platform
  - Windows: `BabySmash-win-x64.zip`
  - macOS: `BabySmash-osx-x64.zip` and `BabySmash-osx-arm64.zip`
  - Linux: `BabySmash-linux-x64.zip`
- [ ] Port `UpdateDialog.xaml` to Avalonia
- [ ] Port `DownloadProgressDialog.xaml` to Avalonia
- [ ] Test update flow on each platform

#### Code Signing
- [ ] **Windows**: Azure Trusted Signing (already configured)
- [ ] **macOS**: Apple Developer Program ($99/year)
  - Create Developer ID Application certificate
  - Sign with `codesign` utility
  - Notarize with Apple's notarization service
  - Staple notarization ticket
- [ ] **Linux**: Optional (AppImage or Flatpak signing)

**Deliverable**: Auto-updating Avalonia app on all platforms

---

### Phase 6: GitHub Actions Multi-Platform Build (1 week)

**Goal**: Automate builds for all platforms

#### Tasks
- [ ] Create `.github/workflows/build-avalonia.yml`
- [ ] Build matrix for platforms:
  - `windows-latest`: win-x64
  - `macos-latest`: osx-x64, osx-arm64
  - `ubuntu-latest`: linux-x64
- [ ] Sign binaries:
  - Windows: Azure Trusted Signing
  - macOS: Apple codesign + notarization
  - Linux: No signing (or Flatpak/Snap if packaging)
- [ ] Create platform-specific packages:
  - Windows: EXE + ZIP
  - macOS: `.app` bundle + DMG
  - Linux: AppImage or tarball
- [ ] Upload to GitHub Releases

**Sample Workflow Structure**:
```yaml
jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - Build win-x64
      - Sign with Azure Trusted Signing
      - Create ZIP
      
  build-macos:
    runs-on: macos-latest
    steps:
      - Build osx-x64 + osx-arm64
      - Sign with Apple Developer ID
      - Notarize
      - Create .app + DMG
      
  build-linux:
    runs-on: ubuntu-latest
    steps:
      - Build linux-x64
      - Create AppImage
```

**Deliverable**: Automated multi-platform releases

---

### Phase 7: Testing & Polish (2 weeks)

**Goal**: Comprehensive testing and user experience improvements

#### Tasks
- [ ] **Functionality Testing**:
  - All shapes display correctly
  - Animations work smoothly
  - Audio plays on keypress
  - Text-to-speech works in multiple languages
  - Multi-monitor support
  - Options dialog saves settings
  - Auto-update downloads and installs
- [ ] **Platform-Specific Testing**:
  - Windows: Test keyboard hooks, transparency
  - macOS: Test permission prompts, native look
  - Linux: Test on Ubuntu, Fedora, Arch
- [ ] **Performance Testing**:
  - No lag when mashing keyboard rapidly
  - Low CPU usage when idle
  - Memory usage stays reasonable
- [ ] **Accessibility**:
  - Keyboard navigation
  - Screen reader compatibility (bonus)
- [ ] **UI Polish**:
  - Consistent look across platforms
  - Native window decorations
  - Platform-appropriate icons

**Deliverable**: Production-ready Avalonia version

---

## Maintenance Strategy

### 1. Shared Core Library
All business logic lives in `BabySmash.Core`:
- Shape generation logic
- Word finding
- Settings management (abstracted)
- Animation definitions
- Color palettes

**Benefit**: Fix once, both platforms benefit

### 2. Platform-Specific UI Projects
- `BabySmash.WPF`: Windows-specific optimizations
- `BabySmash.Avalonia`: Cross-platform UI

**When to change**:
- **Core change** (new shape, word list update): Update `BabySmash.Core`
- **UI improvement**: Update both WPF and Avalonia (or decide platform priority)
- **Platform-specific feature**: Update only that platform's project

### 3. Feature Parity Strategy

**Approach**: WPF remains the **primary** platform initially, Avalonia follows

- New features developed in WPF first (faster iteration)
- Port to Avalonia within same release cycle
- Eventually, both platforms reach feature parity

**Alternative**: Avalonia becomes primary once mature (cross-platform users > Windows-only users)

### 4. Documentation
- Update README with platform-specific installation instructions
- Create `BUILDING_AVALONIA.md` for contributors
- Maintain platform compatibility matrix

---

## Release Strategy

### Versioning
Use **semantic versioning** with platform tags:

- `v5.0.0` - First Avalonia release (major version bump)
- `v5.0.1` - Bugfix for all platforms
- `v5.1.0` - New feature in both WPF and Avalonia

### Release Artifacts

#### GitHub Releases
Each release includes:

| File | Platform | Description |
|------|----------|-------------|
| `BabySmash-Setup.exe` | Windows | WPF installer (existing) |
| `BabySmash-win-x64.zip` | Windows | WPF portable (existing, Updatum target) |
| `BabySmash-Avalonia-win-x64.zip` | Windows | Avalonia Windows build |
| `BabySmash-osx-x64.zip` | macOS Intel | Avalonia macOS Intel |
| `BabySmash-osx-arm64.zip` | macOS Apple Silicon | Avalonia macOS ARM |
| `BabySmash-osx-universal.dmg` | macOS | Universal macOS installer |
| `BabySmash-linux-x64.AppImage` | Linux | Avalonia AppImage |
| `BabySmash-linux-x64.zip` | Linux | Avalonia portable |

### Distribution Channels

#### Windows
- **Primary**: GitHub Releases (WPF version)
- **Secondary**: Microsoft Store (future consideration)
- **Avalonia**: Separate download for testing/cross-platform users

#### macOS
- **Primary**: GitHub Releases (DMG)
- **Future**: Homebrew (`brew install babysmash`)
- **Future**: Mac App Store (requires $99/year + review process)

#### Linux
- **Primary**: GitHub Releases (AppImage)
- **Future**: Flatpak (Flathub)
- **Future**: Snap Store
- **Future**: Distribution repos (apt, dnf, pacman)

### Update Strategy

#### Updatum Configuration
Modify Updatum to support multiple "flavors":

```csharp
// WPF version (Windows-only)
var wpfUpdater = new UpdatumManager("shanselman", "babysmash")
{
    AssetRegexPattern = "BabySmash-win-x64.zip",
};

// Avalonia version (cross-platform)
var avaloniaUpdater = new UpdatumManager("shanselman", "babysmash")
{
    AssetRegexPattern = GetPlatformAssetPattern(), // Returns platform-specific pattern
};

string GetPlatformAssetPattern()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return "BabySmash-Avalonia-win-x64.zip";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        return RuntimeInformation.OSArchitecture == Architecture.Arm64 
            ? "BabySmash-osx-arm64.zip"
            : "BabySmash-osx-x64.zip";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        return "BabySmash-linux-x64.zip";
}
```

---

## Risk Assessment

### Technical Risks

| Risk | Severity | Likelihood | Mitigation |
|------|----------|------------|------------|
| Avalonia XAML incompatibilities | Medium | Medium | Incremental porting, test each shape |
| macOS keyboard hook permissions | High | High | Clear documentation, graceful fallback |
| Linux TTS quality/availability | Medium | Medium | Provide alternative (visual-only mode) |
| Cross-platform audio issues | Low | Low | Use well-tested library (NAudio) |
| Performance degradation | Low | Low | Profile early, optimize as needed |

### Business Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Maintenance burden (2 codebases) | Medium | Shared Core library reduces duplication |
| Limited macOS/Linux user demand | Low | Release as beta, gauge interest |
| Apple Developer Program cost ($99/year) | Low | Justifiable for notarization (security) |
| Cross-platform support complexity | Medium | Community contributions, platform champions |

### Mitigation Plan
1. **Shared Core Library**: 80% of code shared, only 20% platform-specific
2. **Gradual Rollout**: Release Avalonia as "beta" initially, gather feedback
3. **Community Engagement**: Recruit platform testers (macOS, Linux users)
4. **Fallback Options**: If a platform feature is unavailable, degrade gracefully

---

## Success Criteria

### Functional
- [ ] All shapes display correctly on all platforms
- [ ] Animations run smoothly (60 FPS)
- [ ] Audio plays on keypress
- [ ] Text-to-speech works (platform-specific voices)
- [ ] Multi-monitor support works
- [ ] Options dialog saves settings
- [ ] Auto-update downloads and installs updates
- [ ] Keyboard shortcuts work (Ctrl+Alt+Shift+O, Alt+F4)

### Platform-Specific
- [ ] **Windows**: WPF and Avalonia versions both work
- [ ] **macOS**: Notarized .app launches without warnings
- [ ] **Linux**: AppImage runs on Ubuntu, Fedora, Arch

### Quality
- [ ] No crashes during 10-minute keyboard mashing session
- [ ] CPU usage < 5% when idle
- [ ] Memory usage < 200MB
- [ ] Startup time < 2 seconds

### Community
- [ ] 10+ GitHub stars on first Avalonia release
- [ ] At least 1 community contribution (bug report or PR)
- [ ] Positive feedback from macOS/Linux users

---

## Cost Estimate

### Development Time
- **Phase 1-3** (Core + Basic UI): 6-8 weeks
- **Phase 4** (Platform Services): 3-4 weeks
- **Phase 5-7** (Update, Signing, Testing): 3-4 weeks

**Total**: ~3-4 months for v1.0 Avalonia release

### Ongoing Costs
- **Apple Developer Program**: $99/year (required for macOS notarization)
- **Azure Trusted Signing**: ~$10-15/month (existing, shared with WPF)
- **GitHub Actions**: Free (within limits)

**Total**: ~$120/year incremental cost

### Opportunity Cost
- Existing WPF development continues in parallel
- No blocking of current feature roadmap
- Can be done incrementally (community contributions welcome)

---

## Conclusion

The Avalonia port of BabySmash! is **technically feasible** and **strategically valuable** for reaching cross-platform users. The dual-codebase approach with a shared `BabySmash.Core` library minimizes maintenance burden while allowing platform-specific optimizations.

### Recommended Approach
1. **Phase 1-2** (6-8 weeks): Extract Core, create Avalonia skeleton
2. **Phase 3-4** (6-8 weeks): Port UI and implement platform services
3. **Phase 5-7** (3-4 weeks): Auto-update, signing, testing
4. **Release**: v5.0.0 with both WPF (primary) and Avalonia (beta)

### Next Steps
1. **Approval**: Decide if cross-platform support aligns with project goals
2. **Resource Allocation**: Assign developer time or open to community contributions
3. **Pilot**: Start with Phase 1 (Core library extraction) as low-risk first step
4. **Community**: Announce intent, gauge interest from macOS/Linux users

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
