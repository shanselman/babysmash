# BabySmash! Upgrade Plan: .NET 3.5 → .NET 10

## Overview
BabySmash! is an ~18-year-old WPF application targeting .NET Framework 3.5. This plan outlines the migration to .NET 10 with modern features including single-file deployment, auto-update via **Updatum**, and **Azure Trusted Signing**.

**Reference Implementation**: [WindowsEdgeLight](https://github.com/shanselman/WindowsEdgeLight) - Follow the same patterns for Updatum and code signing.

## Current State Analysis

### Project Structure
- **Framework**: .NET Framework 3.5 (WPF)
- **Project Format**: Legacy `.csproj` (non-SDK style)
- **Dependencies**: 
  - `Newtonsoft.Json 9.0.1` (via packages.config)
  - `System.Speech` (for text-to-speech)
  - `System.Deployment` (ClickOnce auto-update - **replacing with Updatum**)
  - P/Invoke calls to `user32.dll`, `winmm.dll`

### Key Components
| Component | Files | Notes |
|-----------|-------|-------|
| Main App | `App.xaml`, `MainWindow.xaml` | WPF entry point |
| Controller | `Controller.cs` | Main logic, uses ClickOnce `ApplicationDeployment` |
| Shapes | 14 XAML UserControls in `/Shapes` | Custom shapes with animations |
| Audio | `Audio.cs` | P/Invoke to `winmm.dll` for WAV playback |
| Keyboard Hook | `KeyboardHook.cs`, `App.xaml.cs` | Low-level keyboard interception |
| Settings | `Settings.cs`, `Properties/Settings.settings` | User preferences |
| Options UI | `Options.xaml` | 63KB XAML - complex settings dialog |
| Localization | `/Resources/Strings/*.json` | en-EN, ru-RU |
| Sounds | `/Resources/Sounds/*.wav` | 10 WAV files (embedded resources) |

### Known Issues to Address
1. **ClickOnce Dependency**: `System.Deployment.ApplicationDeployment` doesn't exist in .NET Core/.NET 5+ → **Replace with Updatum**
2. **BitmapEffect**: `BitmapEffectGroup`, `OuterGlowBitmapEffect` in `MainWindow.xaml` are deprecated
3. **PresentationFramework.Luna**: Windows XP theme assembly - dead code, just remove the xmlns reference in `Options.xaml`
4. **Legacy app.config**: Uses old `supportedRuntime` for v2.0.50727

---

## Phase 1: Create New SDK-Style Project ✅

### Tasks
- [ ] Create new `BabySmash.csproj` (SDK-style, targeting `net10.0-windows`)
- [ ] Configure for WPF: `<UseWPF>true</UseWPF>`
- [ ] Set output type: `<OutputType>WinExe</OutputType>`
- [ ] Configure single-file publishing
- [ ] Add modern package references (replace `packages.config`)
- [ ] Preserve embedded resources configuration
- [ ] Remove old `packages.config` after migration

### New csproj Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>App.ico</ApplicationIcon>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>
</Project>
```

---

## Phase 2: Fix XAML Compatibility Issues

### Tasks
- [ ] Remove `BitmapEffectGroup` / `OuterGlowBitmapEffect` from `MainWindow.xaml`
  - Replace with `Effect` property using `DropShadowEffect` or remove entirely
- [ ] Remove `PresentationFramework.Luna` xmlns from `Options.xaml` (line 10)
  - This was a Windows XP theme - just delete the xmlns declaration
- [ ] Verify all shape XAML files compile (14 files in `/Shapes`)
- [ ] Test animation storyboards work correctly

### MainWindow.xaml Changes Required
```xml
<!-- OLD (deprecated) -->
<TextBlock.BitmapEffect>
    <BitmapEffectGroup>
        <OuterGlowBitmapEffect GlowColor="Yellow" GlowSize="3"/>
    </BitmapEffectGroup>
</TextBlock.BitmapEffect>

<!-- NEW (use Effect instead) -->
<TextBlock.Effect>
    <DropShadowEffect Color="Yellow" BlurRadius="6" ShadowDepth="0"/>
</TextBlock.Effect>
```

---

## Phase 3: Update C# Code for .NET 10

### Tasks
- [ ] **Remove ClickOnce code** from `Controller.cs`:
  - Remove `using System.Deployment.Application`
  - Remove `ApplicationDeployment` usage (lines 49, 104-118, 155-162)
  - Keep update UI but stub it out for now
- [ ] Update `Newtonsoft.Json` to latest version or switch to `System.Text.Json`
- [ ] Verify `System.Speech` works (should be available via Windows compatibility pack)
- [ ] Verify P/Invoke signatures are compatible
- [ ] Update `AssemblyInfo.cs` or move attributes to csproj
- [ ] Remove/update `app.config` for modern .NET

### Files Requiring Code Changes
| File | Change |
|------|--------|
| `Controller.cs` | Remove ClickOnce, stub auto-update |
| `App.xaml.cs` | Verify keyboard hook works |
| `Properties/AssemblyInfo.cs` | May remove (use csproj properties) |

---

## Phase 4: Resource Configuration

### Tasks
- [ ] Configure embedded resources for WAV files in new csproj
- [ ] Ensure `Words.txt` copies to output
- [ ] Configure JSON localization files to copy to output
- [ ] Verify `App.ico` is set correctly

### Resource Configuration
```xml
<ItemGroup>
  <!-- Embedded WAV sounds -->
  <EmbeddedResource Include="Resources\Sounds\*.wav" />
  <EmbeddedResource Include="App.ico" />
  
  <!-- Content files -->
  <Content Include="Words.txt">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="Resources\Strings\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

## Phase 5: Build & Test

### Tasks
- [ ] Run `dotnet build` and fix any compilation errors
- [ ] Run `dotnet publish` for single-file output
- [ ] Test application launches
- [ ] Test keyboard input displays shapes
- [ ] Test audio playback
- [ ] Test text-to-speech
- [ ] Test Options dialog opens (Ctrl+Alt+Shift+O)
- [ ] Test multi-monitor support

---

## Phase 6: Updatum Auto-Update Integration

**Reference**: [WindowsEdgeLight Updatum Integration](https://github.com/shanselman/WindowsEdgeLight/blob/master/docs/UPDATUM_INTEGRATION.md)

### What is Updatum?
[Updatum](https://github.com/sn4k3/Updatum) is a lightweight C# library that automates application updates using GitHub Releases. It replaces the legacy ClickOnce deployment.

### Tasks
- [ ] Add Updatum NuGet package: `<PackageReference Include="Updatum" Version="1.2.1" />`
- [ ] Create `UpdateDialog.xaml` - Update notification dialog
- [ ] Create `DownloadProgressDialog.xaml` - Download progress UI
- [ ] Add `UpdatumManager` configuration in `App.xaml.cs`
- [ ] Remove all ClickOnce code from `Controller.cs`
- [ ] Update `MainWindow.xaml` to remove ClickOnce update progress UI

### Updatum Configuration (App.xaml.cs)
```csharp
using Updatum;

internal static readonly UpdatumManager AppUpdater = new("shanselman", "babysmash")
{
    FetchOnlyLatestRelease = true,
    InstallUpdateSingleFileExecutableName = "BabySmash",
};

protected override async void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    _ = CheckForUpdatesAsync();
}

private async Task CheckForUpdatesAsync()
{
    try
    {
        await Task.Delay(2000); // Let app load first
        var updateFound = await AppUpdater.CheckForUpdatesAsync();
        if (!updateFound) return;
        
        await Dispatcher.InvokeAsync(async () =>
        {
            var release = AppUpdater.LatestRelease!;
            var changelog = AppUpdater.GetChangelog(true) ?? "No release notes available.";
            var dialog = new UpdateDialog(release.TagName, changelog);
            if (dialog.ShowDialog() == true)
            {
                await DownloadAndInstallUpdateAsync();
            }
        });
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
    }
}
```

### Asset Naming Convention for GitHub Releases
```
BabySmash-v1.0.0-win-x64.zip   (contains BabySmash.exe + README.md)
```

**Critical**: ZIP must contain **at least 2 files** (exe + README) for Updatum to handle correctly.

**x64 only**: ARM64 Windows has excellent x64 emulation - no need for separate ARM build for this app.

### Code to Remove from Controller.cs
```csharp
// DELETE these lines:
using System.Deployment.Application;
private ApplicationDeployment deployment = null;
// DELETE all deployment_* event handlers
// DELETE ApplicationDeployment.IsNetworkDeployed checks
```

---

## Phase 7: Azure Trusted Signing

**Reference**: [WindowsEdgeLight Code Signing](https://github.com/shanselman/WindowsEdgeLight/blob/master/docs/CODESIGNING.md)

### Why Code Sign?
- Eliminates Microsoft Defender SmartScreen warnings
- Verifies authenticity of the application
- Establishes trust with Windows security features

### Azure Resources Required
- Azure Subscription
- Azure Trusted Signing resource (Code Signing Account)
- Certificate Profile configured
- Service Principal with "Trusted Signing Certificate Profile Signer" role

### GitHub Secrets Required
| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Service principal client ID |
| `AZURE_CLIENT_SECRET` | Service principal secret |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

### Estimated Cost
- **Public Trust Certificate Profile**: ~$9.99/month
- **First 5,000 signing operations/month**: Included
- **Total for occasional releases**: ~$10-15/month

---

## Phase 8: GitHub Actions CI/CD

### Tasks
- [ ] Create `.github/workflows/build.yml`
- [ ] Configure GitVersion for semantic versioning
- [ ] Build for win-x64 and win-arm64
- [ ] Sign executables with Azure Trusted Signing
- [ ] Create GitHub Release with ZIP assets
- [ ] Auto-generate release notes

### GitHub Actions Workflow (build.yml)
```yaml
name: Build Release

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

permissions:
  contents: write

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0
      
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        
    - name: Restore dependencies
      run: dotnet restore BabySmash.csproj

    - uses: gittools/actions/gitversion/setup@v4
      with:
        versionSpec: '6.4.x'

    - uses: gittools/actions/gitversion/execute@v4
      id: gitversion

    - name: Build x64
      run: |
        dotnet publish BabySmash.csproj `
          -c Release -r win-x64 `
          /p:Version=${{ steps.gitversion.outputs.semVer }} `
          --self-contained

    - uses: azure/login@v2
      with:
        creds: '{"clientId":"${{ secrets.AZURE_CLIENT_ID }}","clientSecret":"${{ secrets.AZURE_CLIENT_SECRET }}","subscriptionId":"${{ secrets.AZURE_SUBSCRIPTION_ID }}","tenantId":"${{ secrets.AZURE_TENANT_ID }}"}'

    - uses: azure/trusted-signing-action@v0
      with:
        azure-tenant-id: ${{ secrets.AZURE_TENANT_ID }}
        azure-client-id: ${{ secrets.AZURE_CLIENT_ID }}
        azure-client-secret: ${{ secrets.AZURE_CLIENT_SECRET }}
        endpoint: https://wus2.codesigning.azure.net/
        trusted-signing-account-name: hanselman
        certificate-profile-name: BabySmash
        files-folder: ${{ github.workspace }}\bin\Release\net10.0-windows\win-x64\publish
        files-folder-filter: exe
        file-digest: SHA256
        timestamp-rfc3161: http://timestamp.acs.microsoft.com
        timestamp-digest: SHA256
          
    - name: Prepare artifacts (ZIP with exe + README)
      shell: pwsh
      run: |
        $version = "${{ steps.gitversion.outputs.semVer }}"
        New-Item -ItemType Directory -Path "artifacts" -Force
        New-Item -ItemType Directory -Path "temp-x64" -Force
        Copy-Item "bin/Release/net10.0-windows/win-x64/publish/BabySmash.exe" -Destination "temp-x64/"
        Copy-Item "README.md" -Destination "temp-x64/"
        Compress-Archive -Path "temp-x64/*" -DestinationPath "artifacts/BabySmash-v$version-win-x64.zip"
        Remove-Item "temp-x64" -Recurse -Force
                  
    - uses: softprops/action-gh-release@v1
      if: startsWith(github.ref, 'refs/tags/')
      with:
        files: artifacts/*.zip
        generate_release_notes: true
```

### Release Process
```powershell
# 1. Update version in csproj (or let GitVersion handle it)
# 2. Commit and push
git add -A
git commit -m "Release v1.0.0 - Description"
git tag v1.0.0
git push && git push --tags
# 3. GitHub Actions automatically builds, signs, and creates release
```

---

## Files to Delete After Migration
- `packages.config` (replaced by PackageReference)
- `BabySmash_TemporaryKey.pfx` (old signing key)
- `BuildProcessTemplates/` (old TFS build templates)
- `private.pfx` reference in csproj (if not needed)

## Files to Keep/Backup
- Original `BabySmash.csproj` → rename to `BabySmash.csproj.old`
- `BabySmash.sln` (update for new project format)

---

## Execution Order

```
┌─────────────────────────────────────────────────────────────────────┐
│ Phase 1: New SDK-Style csproj                                       │
│   └─> Phase 2: Fix XAML (BitmapEffect, Luna theme)                 │
│         └─> Phase 3: Update C# (remove ClickOnce)                  │
│               └─> Phase 4: Configure Resources                     │
│                     └─> Phase 5: Build & Test                      │
│                           └─> Phase 6: Updatum Auto-Update         │
│                                 └─> Phase 7: Azure Trusted Signing │
│                                       └─> Phase 8: GitHub Actions  │
└─────────────────────────────────────────────────────────────────────┘
```

**Phases 1-5**: Core migration (must complete first)  
**Phases 6-8**: Modern deployment features (can be done incrementally)

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| XAML won't compile | Fix one file at a time, test incrementally |
| P/Invoke breaks | Windows platform target ensures Win32 APIs available |
| Missing dependencies | Use Windows Compatibility Pack if needed |
| Audio doesn't work | Fallback to `System.Media.SoundPlayer` if needed |
| Speech fails | `System.Speech` requires Windows TTS voices installed |

---

## Success Criteria
- [ ] Application compiles with `dotnet build`
- [ ] Single executable produced (~20-50MB expected)
- [ ] All shapes display correctly
- [ ] Audio plays on keypress
- [ ] Speech synthesis works
- [ ] Options dialog functional
- [ ] Multi-monitor support works
- [ ] No runtime errors in normal usage
- [ ] Updatum auto-update checks for new versions on startup
- [ ] GitHub Actions builds and signs releases automatically
- [ ] SmartScreen does NOT warn when running signed executable

---

## Notes
- .NET 10 SDK detected: `10.0.100`
- **x64 only** - ARM64 not needed; x64 emulation on Windows ARM works fine for this non-intensive app
- Target single-file with self-contained for maximum portability
- **ClickOnce → Updatum**: Complete replacement using GitHub Releases
- **Code Signing**: Azure Trusted Signing (~$10/month) eliminates SmartScreen warnings
- **PresentationFramework.Luna**: Dead code from Windows XP era - just remove the xmlns reference
- **Reference repo**: [WindowsEdgeLight](https://github.com/shanselman/WindowsEdgeLight) has working examples of all patterns
