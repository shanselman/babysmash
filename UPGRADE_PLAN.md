# BabySmash! Migration Guide: .NET Framework 3.5 → .NET 10

> **A practical guide for migrating legacy WPF applications to modern .NET**

This document chronicles the complete migration of BabySmash!, an 18-year-old WPF application, from .NET Framework 3.5 to .NET 10. It serves as both a historical record and an educational resource for developers facing similar migrations.

## 📋 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Before You Start](#before-you-start)
3. [Phase 1: Project File Migration](#phase-1-project-file-migration)
4. [Phase 2: XAML Compatibility](#phase-2-xaml-compatibility)
5. [Phase 3: C# Code Modernization](#phase-3-c-code-modernization)
6. [Phase 4: Resource Handling](#phase-4-resource-handling)
7. [Phase 5: Build & Test](#phase-5-build--test)
8. [Phase 6: Auto-Updates with Updatum](#phase-6-auto-updates-with-updatum)
9. [Phase 7: Code Signing](#phase-7-code-signing)
10. [Phase 8: CI/CD with GitHub Actions](#phase-8-cicd-with-github-actions)
11. [Lessons Learned](#lessons-learned)

---

## Executive Summary

| Metric | Before | After |
|--------|--------|-------|
| Framework | .NET Framework 3.5 | .NET 10 |
| Project Format | Legacy csproj (500+ lines) | SDK-style (50 lines) |
| Deployment | ClickOnce | Inno Setup + Updatum auto-update |
| Code Signing | Self-signed PFX | Azure Trusted Signing |
| CI/CD | Manual builds | GitHub Actions |
| Executable Size | ~2MB + .NET Framework | ~68MB self-contained |
| .NET Required | Yes (.NET 3.5) | No (self-contained) |

**Time to complete**: ~2 days of focused work

---

## Before You Start

### Assess Your Application

Before migrating, understand what you're working with:

```powershell
# Check for ClickOnce dependencies (must be replaced)
Select-String -Path "*.cs" -Pattern "System.Deployment"

# Check for deprecated WPF features
Select-String -Path "*.xaml" -Pattern "BitmapEffect|Luna"

# Check for Newtonsoft.Json (consider System.Text.Json)
Select-String -Path "*.cs" -Pattern "Newtonsoft"
```

### BabySmash's Starting Point

| Component | Status | Action Needed |
|-----------|--------|---------------|
| WPF UI | ✅ Works | Minor XAML fixes |
| P/Invoke (user32, winmm) | ✅ Works | No changes |
| System.Speech | ✅ Works | No changes |
| ClickOnce | ❌ Removed in .NET Core | Replace with Updatum |
| BitmapEffect | ⚠️ Deprecated | Keep or replace with Effect |
| Newtonsoft.Json | ⚠️ Optional | Replaced with System.Text.Json |

---

## Phase 1: Project File Migration

### The Problem

Legacy .csproj files are verbose XML nightmares that mix project configuration with build logic. SDK-style projects are clean and convention-based.

### The Solution

**Delete** your old .csproj and create a new one:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>App.ico</ApplicationIcon>
    
    <!-- Single-file deployment -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Speech" Version="9.0.0" />
    <PackageReference Include="Updatum" Version="1.2.1" />
  </ItemGroup>
</Project>
```

### Key Decisions

1. **Single-file**: Users get one .exe, no scattered DLLs
2. **Self-contained**: No .NET runtime required on user's machine
3. **x64 only**: ARM64 Windows has excellent x64 emulation - one binary is enough for a simple app like this

### What About packages.config?

Delete it. Package references go directly in .csproj now:

```xml
<!-- Old way (packages.config) -->
<package id="Newtonsoft.Json" version="9.0.1" targetFramework="net35" />

<!-- New way (in .csproj) -->
<PackageReference Include="System.Text.Json" Version="9.0.0" />
```

---

## Phase 2: XAML Compatibility

### BitmapEffect: Deprecated But Still Works

.NET Framework had `BitmapEffect` (software-rendered) and `Effect` (GPU-accelerated). BitmapEffect was deprecated but **still compiles and runs** in .NET 10 WPF.

```xml
<!-- This still works in .NET 10! (but uses CPU) -->
<TextBlock.BitmapEffect>
    <OuterGlowBitmapEffect GlowColor="Yellow" GlowSize="3"/>
</TextBlock.BitmapEffect>

<!-- Modern replacement (uses GPU) -->
<TextBlock.Effect>
    <DropShadowEffect Color="Yellow" BlurRadius="6" ShadowDepth="0"/>
</TextBlock.Effect>
```

**Our decision**: Keep BitmapEffect for visual continuity, but use modern `Effect` class for new features. Added GPU tier detection to auto-enable effects only on capable hardware.

### Remove Dead Theme References

Windows XP themes don't exist anymore:

```xml
<!-- DELETE THIS LINE from your XAML -->
xmlns:theme="clr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Luna"
```

---

## Phase 3: C# Code Modernization

### Removing ClickOnce

ClickOnce (`System.Deployment.Application`) doesn't exist in .NET Core/.NET 5+. Search and destroy:

```csharp
// DELETE all of this:
using System.Deployment.Application;

if (ApplicationDeployment.IsNetworkDeployed)
{
    deployment = ApplicationDeployment.CurrentDeployment;
    deployment.CheckForUpdateAsync();
}
```

**Replacement**: [Updatum](https://github.com/sn4k3/Updatum) - covered in Phase 6.

### JSON Migration

```csharp
// Old (Newtonsoft.Json)
var obj = JsonConvert.DeserializeObject<MyType>(json);

// New (System.Text.Json) - built into .NET
var obj = JsonSerializer.Deserialize<MyType>(json);
```

System.Text.Json is faster and has no external dependency.

### Type Ambiguities

When you add `<UseWPF>true</UseWPF>`, you get both WPF and WinForms types. Add explicit aliases:

```csharp
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using WinForms = System.Windows.Forms;
```

---

## Phase 4: Resource Handling

### Embedded Resources vs Content

| Type | Use Case | Configuration |
|------|----------|---------------|
| Embedded Resource | WAV sounds, icons | `<EmbeddedResource Include="..." />` |
| Content | JSON config, text files | `<Content Include="..." CopyToOutputDirectory="PreserveNewest" />` |

### Single-File Gotcha

With `PublishSingleFile`, embedded resources work fine, but `Content` files need special handling. We auto-extract `Words.txt` on first run:

```csharp
private string GetWordsFilePath()
{
    // Check next to executable first
    string exeDir = AppContext.BaseDirectory;
    string localPath = Path.Combine(exeDir, _wordsFileName);
    if (File.Exists(localPath)) return localPath;

    // For single-file publish, extract from embedded resource
    string appDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BabySmash", _wordsFileName);
    
    if (!File.Exists(appDataPath))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(appDataPath)!);
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("BabySmash.Words.txt");
        using var file = File.Create(appDataPath);
        stream!.CopyTo(file);
    }
    return appDataPath;
}
```

---

## Phase 5: Build & Test

### Build Commands

```powershell
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained

# Output location
# bin/Release/net10.0-windows/win-x64/publish/BabySmash.exe
```

### Testing Checklist

- [ ] App launches without crash
- [ ] Keyboard input shows shapes
- [ ] Audio plays (WAV sounds)
- [ ] Speech synthesis works
- [ ] Options dialog opens (Ctrl+Shift+Alt+O)
- [ ] Multi-monitor support works
- [ ] Settings persist between runs

---

## Phase 6: Auto-Updates with Updatum

### Why Updatum?

[Updatum](https://github.com/sn4k3/Updatum) is a lightweight library that uses GitHub Releases for auto-updates. No server infrastructure needed.

### Integration

```csharp
// App.xaml.cs
internal static readonly UpdatumManager AppUpdater = new("owner", "repo")
{
    FetchOnlyLatestRelease = true,
    InstallUpdateSingleFileExecutableName = "BabySmash",
};

private async void Application_Startup(object sender, StartupEventArgs e)
{
    var shouldLaunch = await CheckForUpdatesBeforeLaunchAsync();
    if (shouldLaunch)
    {
        Controller.Instance.Launch();
    }
}

private async Task<bool> CheckForUpdatesBeforeLaunchAsync()
{
    try
    {
        var updateFound = await AppUpdater.CheckForUpdatesAsync();
        if (!updateFound) return true;

        var dialog = new UpdateDialog(
            AppUpdater.LatestRelease!.TagName,
            AppUpdater.GetChangelog(true));
        dialog.ShowDialog();

        if (dialog.Result == UpdateDialogResult.Download)
        {
            await DownloadAndInstallUpdateAsync();
            return false; // App will restart
        }
        return true;
    }
    catch
    {
        return true; // Don't block app on update failure
    }
}
```

### Critical: Asset Naming

Updatum's default `AssetRegexPattern` looks for the platform identifier (e.g., `win-x64`) in the asset name:

```
✅ BabySmash-win-x64.zip    <- Updatum finds this
❌ BabySmash-Portable.zip   <- Updatum can't find this!
```

### ZIP Contents

Include at least 2 files in the ZIP for Updatum to handle it correctly:

```
BabySmash-win-x64.zip
├── BabySmash.exe
└── README.md
```

---

## Phase 7: Code Signing

### The Problem

Without code signing, Windows SmartScreen shows scary warnings that make users think your app is malware.

### Azure Trusted Signing

Microsoft's cloud-based code signing service (~$10/month):

1. Create Azure Trusted Signing resource
2. Create Certificate Profile (Public Trust)
3. Create Service Principal with signing permissions
4. Store credentials as GitHub Secrets

### GitHub Action for Signing

```yaml
- uses: azure/trusted-signing-action@v0
  with:
    azure-tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    azure-client-id: ${{ secrets.AZURE_CLIENT_ID }}
    azure-client-secret: ${{ secrets.AZURE_CLIENT_SECRET }}
    endpoint: https://wus2.codesigning.azure.net/
    trusted-signing-account-name: your-account
    certificate-profile-name: your-profile
    files-folder: ${{ github.workspace }}/publish
    files-folder-filter: exe
    file-digest: SHA256
    timestamp-rfc3161: http://timestamp.acs.microsoft.com
    timestamp-digest: SHA256
```

---

## Phase 8: CI/CD with GitHub Actions

### Complete Workflow

Our workflow:
1. Builds on every push
2. Creates releases only on tags (`v*`)
3. Uses GitVersion for semantic versioning
4. Signs both EXE and installer
5. Creates Inno Setup installer with Start Menu shortcuts

### Inno Setup Installer

For a proper Windows experience, we added an installer:

```iss
[Setup]
AppName=BabySmash!
AppVersion={#MyAppVersion}
DefaultDirName={localappdata}\BabySmash
PrivilegesRequired=lowest

[Files]
Source: "publish\BabySmash.exe"; DestDir: "{app}"

[Icons]
Name: "{userprograms}\BabySmash!"; Filename: "{app}\BabySmash.exe"
Name: "{userstartup}\BabySmash!"; Filename: "{app}\BabySmash.exe"; Tasks: startupicon
```

### Release Process

```powershell
git tag v3.9.9
git push origin v3.9.9
# GitHub Actions handles the rest!
```

---

## Lessons Learned

### 1. BitmapEffect Still Works
Don't panic about deprecated APIs. Test them first - they might still work fine.

### 2. Updatum Asset Naming is Critical
The default regex pattern looks for `win-x64` in asset names. Name your ZIPs accordingly or set a custom pattern.

### 3. Check for Updates BEFORE the App Takes Over
For a baby-smashing app, show the update dialog before the baby starts smashing keys!

### 4. GPU Detection for Effects
WPF's `RenderCapability.Tier` tells you if effects are hardware-accelerated:
- Tier 0: Software rendering (disable effects)
- Tier 1: Partial hardware
- Tier 2: Full GPU acceleration (enable effects)

### 5. Self-Contained = Larger But Simpler
Yes, the EXE is 68MB. But users don't need to install .NET, and you don't get "which .NET version?" support tickets.

### 6. Inno Setup + Updatum = Best of Both Worlds
- Installer handles first-time setup (Start Menu, shortcuts)
- Updatum handles all future updates (in-place, no reinstall)

---

## Resources

- [Updatum](https://github.com/sn4k3/Updatum) - GitHub-based auto-update library
- [Azure Trusted Signing](https://learn.microsoft.com/azure/trusted-signing/) - Code signing service
- [WindowsEdgeLight](https://github.com/shanselman/WindowsEdgeLight) - Reference implementation with same patterns
- [Inno Setup](https://jrsoftware.org/isinfo.php) - Windows installer creator

---

## Final Status

**All phases complete!** ✅

| Success Criteria | Status |
|-----------------|--------|
| Application compiles | ✅ |
| Single executable | ✅ 68MB self-contained |
| All shapes display | ✅ |
| Audio plays | ✅ |
| Speech synthesis | ✅ |
| Options dialog | ✅ |
| Multi-monitor | ✅ |
| Auto-updates | ✅ Updatum working |
| CI/CD | ✅ GitHub Actions |
| Code signed | ✅ No SmartScreen warnings |

**Current release: v3.9.9**

---

*This migration was completed in January 2026. The app was originally written in 2008 for .NET Framework 3.5.*
