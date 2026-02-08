<#
.SYNOPSIS
    Generates WinGet manifests for BabySmash release
.DESCRIPTION
    This script generates WinGet manifest files for a specific version of BabySmash
    by downloading the installer, calculating its SHA256 hash, and replacing placeholders
    in the template files.
.PARAMETER Version
    The version number to generate manifests for (e.g., "4.0.0")
.PARAMETER SkipDownload
    Skip downloading the installer if you already have it
.EXAMPLE
    .\generate-manifests.ps1 -Version "4.0.0"
.EXAMPLE
    .\generate-manifests.ps1 -Version "4.0.0" -SkipDownload
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDownload
)

$ErrorActionPreference = "Stop"

# Paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$installerUrl = "https://github.com/shanselman/babysmash/releases/download/v$Version/BabySmash-Setup.exe"
$installerPath = Join-Path $scriptDir "BabySmash-Setup.exe"
$outputDir = Join-Path $scriptDir "output"

# Create output directory
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Host "Generating WinGet manifests for BabySmash v$Version" -ForegroundColor Cyan
Write-Host ""

# Download installer if needed
if (!$SkipDownload) {
    Write-Host "Downloading installer from $installerUrl..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath
        Write-Host "✓ Downloaded installer" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to download installer: $_"
        exit 1
    }
}
elseif (!(Test-Path $installerPath)) {
    Write-Error "Installer not found at $installerPath. Remove -SkipDownload or download manually."
    exit 1
}

# Calculate SHA256 hash
Write-Host "Calculating SHA256 hash..." -ForegroundColor Yellow
$hash = (Get-FileHash -Path $installerPath -Algorithm SHA256).Hash
Write-Host "✓ SHA256: $hash" -ForegroundColor Green
Write-Host ""

# Get current date for release date
$releaseDate = Get-Date -Format "yyyy-MM-dd"

# Process each manifest template
$templates = @(
    "ScottHanselman.BabySmash.yaml",
    "ScottHanselman.BabySmash.installer.yaml",
    "ScottHanselman.BabySmash.locale.en-US.yaml"
)

Write-Host "Generating manifest files..." -ForegroundColor Yellow

foreach ($template in $templates) {
    $templatePath = Join-Path $scriptDir $template
    $outputPath = Join-Path $outputDir $template
    
    # Read template
    $content = Get-Content -Path $templatePath -Raw
    
    # Replace placeholders
    $content = $content -replace '\{VERSION\}', $Version
    $content = $content -replace '\{SHA256\}', $hash
    $content = $content -replace '\{RELEASE_DATE\}', $releaseDate
    
    # Write output
    Set-Content -Path $outputPath -Value $content -NoNewline
    Write-Host "✓ Generated $template" -ForegroundColor Green
}

Write-Host ""
Write-Host "✓ All manifests generated successfully in: $outputDir" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the generated manifests in the output directory"
Write-Host "2. Validate with: winget validate --manifest $outputDir"
Write-Host "3. Submit with: wingetcreate submit --manifest $outputDir"
Write-Host ""
Write-Host "Or use the automated command:" -ForegroundColor Yellow
Write-Host "wingetcreate update ScottHanselman.BabySmash --urls $installerUrl --version $Version --submit" -ForegroundColor White
