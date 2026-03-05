# Validating WinGet Manifests

This document explains how to validate the BabySmash WinGet manifests.

## Prerequisites for Validation

Validation requires Windows 10 version 1709 or later with WinGet installed.

## Validation Steps

### 1. Automatic Validation (Recommended)

The easiest way to validate manifests is using `wingetcreate`, which validates automatically:

```powershell
# This command validates before submission
wingetcreate update ScottHanselman.BabySmash --urls https://github.com/shanselman/babysmash/releases/download/v4.0.0/BabySmash-Setup.exe --version 4.0.0
```

### 2. Manual Validation

If you generated manifests using the helper scripts, validate them with:

```powershell
# From the .winget directory
winget validate --manifest ./output/
```

This checks:
- ✅ YAML syntax is valid
- ✅ All required fields are present
- ✅ Field values meet schema requirements
- ✅ Manifest version matches schema version
- ✅ Package identifier is properly formatted

### 3. Test Local Installation (Optional)

To test the actual installation process locally before submitting:

```powershell
# Enable local manifest files
winget settings --enable LocalManifestFiles

# Test install using your local manifests
winget install --manifest ./output/
```

This will:
- Download the installer from the URL specified
- Verify the SHA256 hash matches
- Attempt installation using the specified installer type
- Test silent installation if configured

**Note:** This actually installs BabySmash on your system. Uninstall afterward if testing.

## Common Validation Errors

### Schema Validation Errors

**Error:** "Property 'PackageVersion' does not match pattern"
- **Fix:** Ensure version follows semantic versioning (e.g., `4.0.0`, not `v4.0.0`)

**Error:** "Property 'InstallerSha256' is required"
- **Fix:** Run the generate-manifests script to calculate the hash, or manually add it

**Error:** "Property 'ManifestVersion' must be 1.10.0"
- **Fix:** Ensure all three YAML files use the same ManifestVersion

### URL and Hash Errors

**Error:** "Hash validation failed"
- **Fix:** Re-download the installer and recalculate the hash
- PowerShell: `(Get-FileHash BabySmash-Setup.exe -Algorithm SHA256).Hash`

**Error:** "Failed to download installer"
- **Fix:** Ensure the release exists and the URL is publicly accessible

### Installer Type Errors

**Error:** "InstallerType 'inno' not detected"
- **Fix:** BabySmash uses Inno Setup. Verify the installer is built correctly.

## Validation Checklist

Before submitting to WinGet:

- [ ] All three manifest files exist (version, installer, locale)
- [ ] PackageIdentifier matches across all files: `ScottHanselman.BabySmash`
- [ ] PackageVersion matches across all files and the release tag
- [ ] SHA256 hash is correctly calculated from the installer
- [ ] Installer URL points to the correct GitHub release
- [ ] Release date is accurate (YYYY-MM-DD format)
- [ ] `winget validate --manifest ./output/` passes without errors
- [ ] (Optional) Local installation test succeeds

## Automated Checks

When you submit a Pull Request to microsoft/winget-pkgs:

1. **Automated validation** runs immediately
2. **SmartScreen check** verifies the installer signature
3. **Binary validation** downloads and scans the installer
4. **Community review** may provide additional feedback

These checks ensure package quality and security.

## Troubleshooting

### I don't have Windows to validate

- Use GitHub Codespaces or a Windows VM
- Submit via `wingetcreate` which validates for you
- The PR validation will catch errors when you submit

### Validation passes but installation fails

- Test the installer manually on a clean Windows system
- Check installer logs in `%TEMP%`
- Verify installer switches in the manifest match Inno Setup behavior

### How do I update after validation errors?

1. Fix the manifest files
2. Re-run `winget validate --manifest ./output/`
3. Update your PR with the corrected manifests
4. Automated validation will re-run

## Resources

- [WinGet Validate Command](https://learn.microsoft.com/en-us/windows/package-manager/winget/validate)
- [Manifest Schema](https://learn.microsoft.com/en-us/windows/package-manager/package/manifest)
- [Common Validation Errors](https://github.com/microsoft/winget-pkgs/blob/master/AUTHORING_MANIFESTS.md)
