#!/bin/bash
# Generates WinGet manifests for BabySmash release

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Check arguments
if [ "$#" -lt 1 ]; then
    echo "Usage: $0 <version> [--skip-download]"
    echo "Example: $0 4.0.0"
    exit 1
fi

VERSION="$1"
SKIP_DOWNLOAD=false

if [ "$#" -eq 2 ] && [ "$2" == "--skip-download" ]; then
    SKIP_DOWNLOAD=true
fi

# Paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INSTALLER_URL="https://github.com/shanselman/babysmash/releases/download/v${VERSION}/BabySmash-Setup.exe"
INSTALLER_PATH="${SCRIPT_DIR}/BabySmash-Setup.exe"
OUTPUT_DIR="${SCRIPT_DIR}/output"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo -e "${CYAN}Generating WinGet manifests for BabySmash v${VERSION}${NC}"
echo ""

# Download installer if needed
if [ "$SKIP_DOWNLOAD" = false ]; then
    echo -e "${YELLOW}Downloading installer from ${INSTALLER_URL}...${NC}"
    if wget -q -O "$INSTALLER_PATH" "$INSTALLER_URL"; then
        echo -e "${GREEN}✓ Downloaded installer${NC}"
    else
        echo -e "${RED}Failed to download installer${NC}"
        exit 1
    fi
elif [ ! -f "$INSTALLER_PATH" ]; then
    echo -e "${RED}Installer not found at ${INSTALLER_PATH}. Remove --skip-download or download manually.${NC}"
    exit 1
fi

# Calculate SHA256 hash
echo -e "${YELLOW}Calculating SHA256 hash...${NC}"
if command -v sha256sum &> /dev/null; then
    HASH=$(sha256sum "$INSTALLER_PATH" | awk '{print toupper($1)}')
elif command -v shasum &> /dev/null; then
    HASH=$(shasum -a 256 "$INSTALLER_PATH" | awk '{print toupper($1)}')
else
    echo -e "${RED}Neither sha256sum nor shasum found. Please install one.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ SHA256: ${HASH}${NC}"
echo ""

# Get current date for release date
RELEASE_DATE=$(date +%Y-%m-%d)

# Process each manifest template
echo -e "${YELLOW}Generating manifest files...${NC}"

TEMPLATES=(
    "ScottHanselman.BabySmash.yaml"
    "ScottHanselman.BabySmash.installer.yaml"
    "ScottHanselman.BabySmash.locale.en-US.yaml"
)

for template in "${TEMPLATES[@]}"; do
    TEMPLATE_PATH="${SCRIPT_DIR}/${template}"
    OUTPUT_PATH="${OUTPUT_DIR}/${template}"
    
    # Read template and replace placeholders
    sed -e "s/{VERSION}/${VERSION}/g" \
        -e "s/{SHA256}/${HASH}/g" \
        -e "s/{RELEASE_DATE}/${RELEASE_DATE}/g" \
        "$TEMPLATE_PATH" > "$OUTPUT_PATH"
    
    echo -e "${GREEN}✓ Generated ${template}${NC}"
done

echo ""
echo -e "${GREEN}✓ All manifests generated successfully in: ${OUTPUT_DIR}${NC}"
echo ""
echo -e "${CYAN}Next steps:${NC}"
echo "1. Review the generated manifests in the output directory"
echo "2. Validate with: winget validate --manifest ${OUTPUT_DIR}"
echo "3. Submit with: wingetcreate submit --manifest ${OUTPUT_DIR}"
echo ""
echo -e "${YELLOW}Or use the automated command:${NC}"
echo "wingetcreate update ScottHanselman.BabySmash --urls ${INSTALLER_URL} --version ${VERSION} --submit"
