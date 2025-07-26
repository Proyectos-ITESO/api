#!/bin/bash

# MicroJack PRO - First Release Creation Script
# This script creates a complete installation package including both the main application and auto-updater

set -e

echo "=== MicroJack PRO - First Release Creator ==="
echo

# Configuration
VERSION="1.0.0"
RELEASE_NAME="microjack-pro-v${VERSION}"
RELEASE_DIR="releases"
API_DIR="."
UPDATER_DIR="../MicroJack.Updater"

# Create releases directory
mkdir -p "$RELEASE_DIR"
RELEASE_PATH="$RELEASE_DIR/$RELEASE_NAME"
mkdir -p "$RELEASE_PATH"

echo "[STEP 1/6] Building MicroJack.API..."
# Build the main application
dotnet build "$API_DIR/MicroJack.API.csproj" --configuration Release

echo "[STEP 2/6] Building MicroJack.Updater..."
# Build the updater
(cd "$UPDATER_DIR" && dotnet build --configuration Release)

echo "[STEP 3/6] Copying main application files..."
# Copy main application
cp -r "bin/Release/net8.0/"* "$RELEASE_PATH/"

echo "[STEP 4/6] Copying auto-updater..."
# Copy the updater executable
cp "$UPDATER_DIR/bin/Release/net8.0/MicroJack.Updater" "$RELEASE_PATH/"
cp "$UPDATER_DIR/bin/Release/net8.0/MicroJack.Updater.runtimeconfig.json" "$RELEASE_PATH/"
cp "$UPDATER_DIR/bin/Release/net8.0/MicroJack.Updater.dll" "$RELEASE_PATH/"

echo "[STEP 5/6] Creating deployment package..."
# Create the ZIP file for distribution
(cd "$RELEASE_DIR" && zip -r "${RELEASE_NAME}.zip" "$RELEASE_NAME")

# Calculate hash
HASH=$(sha256sum "$RELEASE_DIR/${RELEASE_NAME}.zip" | awk '{print $1}')

echo "[STEP 6/6] Generating installation information..."

# Create installation instructions
cat > "$RELEASE_DIR/${RELEASE_NAME}-INSTALL.md" << EOF
# MicroJack PRO v${VERSION} Installation

## Package Contents
- **MicroJack.API.dll** - Main application
- **MicroJack.Updater** - Auto-updater executable  
- **MicroJack.Updater.dll** - Auto-updater library
- **Dependencies** - All required .NET libraries

## Installation Steps

1. **Extract Files:**
   \`\`\`bash
   unzip ${RELEASE_NAME}.zip
   cd ${RELEASE_NAME}
   \`\`\`

2. **Configure License Server:**
   - Edit \`appsettings.json\`
   - Set \`LicenseSettings:UpdateServerUrl\` to your license server
   - Set \`LicenseSettings:LicenseKey\` to your license key

3. **Run Application:**
   \`\`\`bash
   dotnet MicroJack.API.dll
   \`\`\`

## For License Server Setup

Update your \`licenses.json\` with:

\`\`\`json
{
  "LicenseKey": "YOUR_LICENSE_KEY",
  "Owner": "Client Name",
  "Type": "Standard", 
  "ExpirationDate": "2025-12-31",
  "EnabledFeatures": ["Basic", "Advanced"],
  "LatestVersion": "${VERSION}",
  "MinimumRequiredVersion": "${VERSION}",
  "DownloadUrl": "https://yourserver.com/updates/${RELEASE_NAME}.zip",
  "FileHash": "${HASH}"
}
\`\`\`

## Auto-Update Ready
This installation includes the auto-updater. Future updates will be handled automatically
when announced through the license server.

EOF

echo
echo "✅ SUCCESS! First release created:"
echo "   📦 Package: $RELEASE_DIR/${RELEASE_NAME}.zip"
echo "   📋 Instructions: $RELEASE_DIR/${RELEASE_NAME}-INSTALL.md"
echo "   🔍 SHA256: $HASH"
echo
echo "🚀 Next steps:"
echo "   1. Upload ${RELEASE_NAME}.zip to your download server"
echo "   2. Update license server with the hash above"
echo "   3. Distribute to clients"
echo