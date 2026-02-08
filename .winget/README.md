# WinGet Package Manifests for BabySmash

This directory contains manifest templates for submitting BabySmash to the Windows Package Manager (winget) community repository.

## What is WinGet?

WinGet is Microsoft's official package manager for Windows. Users can install BabySmash with a simple command:

```powershell
winget install ScottHanselman.BabySmash
```

## Manifest Files

The manifest consists of three YAML files following the [WinGet manifest schema v1.10.0](https://learn.microsoft.com/en-us/windows/package-manager/package/manifest):

1. **ScottHanselman.BabySmash.yaml** - Version manifest (root)
2. **ScottHanselman.BabySmash.installer.yaml** - Installer details and download URL
3. **ScottHanselman.BabySmash.locale.en-US.yaml** - Package metadata and localization

These templates contain placeholders that need to be replaced for each release:
- `{VERSION}` - Package version (e.g., `4.0.0`)
- `{SHA256}` - SHA256 hash of the installer
- `{RELEASE_DATE}` - Release date in YYYY-MM-DD format

## How to Submit a New Release to WinGet

### Prerequisites

1. Install the WinGet Create tool:
   ```powershell
   winget install Microsoft.WingetCreate
   ```

2. Ensure you have a GitHub account and are logged in.

### Option 1: Automated Submission with wingetcreate (Recommended)

For new releases, use the `wingetcreate update` command:

```powershell
wingetcreate update ScottHanselman.BabySmash --urls https://github.com/shanselman/babysmash/releases/download/v4.0.0/BabySmash-Setup.exe --version 4.0.0 --submit
```

This will:
- Download the installer and calculate its SHA256 hash
- Update all three manifest files with the new version
- Validate the manifests against the schema
- Automatically create a Pull Request to the [winget-pkgs repository](https://github.com/microsoft/winget-pkgs)

### Option 2: Manual Submission

If you prefer manual control:

#### Step 1: Generate Updated Manifests

Replace the placeholders in the manifest files:

```bash
# Set your version
VERSION="4.0.0"

# Download the installer to calculate hash
wget https://github.com/shanselman/babysmash/releases/download/v${VERSION}/BabySmash-Setup.exe

# Calculate SHA256 hash (PowerShell)
$hash = (Get-FileHash BabySmash-Setup.exe -Algorithm SHA256).Hash

# Or on Linux/Mac
sha256sum BabySmash-Setup.exe

# Get release date
RELEASE_DATE=$(date +%Y-%m-%d)

# Replace placeholders in all three manifest files
sed -i "s/{VERSION}/${VERSION}/g" .winget/*.yaml
sed -i "s/{SHA256}/${hash}/g" .winget/ScottHanselman.BabySmash.installer.yaml
sed -i "s/{RELEASE_DATE}/${RELEASE_DATE}/g" .winget/ScottHanselman.BabySmash.installer.yaml
```

#### Step 2: Validate Manifests

```powershell
winget validate --manifest .winget/
```

#### Step 3: Submit to WinGet Repository

```powershell
wingetcreate submit --manifest .winget/
```

Or manually:
1. Fork the [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs) repository
2. Create a directory: `manifests/s/ScottHanselman/BabySmash/{VERSION}/`
3. Copy the three manifest files to that directory
4. Create a Pull Request

### First-Time Submission

For the initial submission to WinGet, use:

```powershell
wingetcreate new --urls https://github.com/shanselman/babysmash/releases/download/v4.0.0/BabySmash-Setup.exe --version 4.0.0 --submit
```

Follow the prompts to fill in package details. The tool will guide you through the process.

## Automation in CI/CD

To automate WinGet manifest updates in the GitHub Actions workflow, you could add a job after release creation:

```yaml
- name: Update WinGet package
  if: startsWith(github.ref, 'refs/tags/')
  run: |
    # Install wingetcreate
    winget install Microsoft.WingetCreate
    
    # Get version from tag
    $version = $env:GITHUB_REF -replace 'refs/tags/v', ''
    
    # Update and submit
    wingetcreate update ScottHanselman.BabySmash `
      --urls https://github.com/shanselman/babysmash/releases/download/v${version}/BabySmash-Setup.exe `
      --version $version `
      --submit `
      --token ${{ secrets.GITHUB_TOKEN }}
```

**Note:** This requires appropriate GitHub permissions and may need manual review depending on repository policies.

## Package Identifier

The package identifier `ScottHanselman.BabySmash` follows WinGet conventions:
- Format: `Publisher.PackageName`
- Must be unique in the WinGet repository
- Cannot be changed after initial submission

## Installer Details

- **Type:** Inno Setup installer (`inno`)
- **Architecture:** x64
- **Scope:** User installation (no admin required)
- **Silent Install:** Supported via Inno Setup defaults
- **Source:** GitHub Releases (signed executable)

## Resources

- [WinGet Documentation](https://learn.microsoft.com/en-us/windows/package-manager/)
- [WinGet Manifest Schema](https://learn.microsoft.com/en-us/windows/package-manager/package/manifest)
- [WinGet Packages Repository](https://github.com/microsoft/winget-pkgs)
- [WinGet Create Tool](https://github.com/microsoft/winget-create)

## Support

For issues with the WinGet package:
- WinGet package issues: [microsoft/winget-pkgs issues](https://github.com/microsoft/winget-pkgs/issues)
- BabySmash application issues: [shanselman/babysmash issues](https://github.com/shanselman/babysmash/issues)
