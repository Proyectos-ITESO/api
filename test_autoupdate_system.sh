#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

echo "--- Auto-Update System Integration Test ---"

# --- 1. Configuration ---
LICENSE_SERVER_URL="http://localhost:5101"
LICENSE_SERVER_DIR="../licensing-server"
UPDATER_PROJECT_DIR="../MicroJack.Updater"
API_PROJECT_FILE="MicroJack.API.csproj"

# --- 2. Build Everything ---
echo "[STEP 1/7] Building all projects..."
# Clean all obj and bin directories first (but keep source files)
rm -rf obj/ bin/
# Build API project
dotnet build api.sln --configuration Release
echo "[SUCCESS] API build complete."

# Build Updater project separately
echo "Building MicroJack.Updater..."
(cd "$UPDATER_PROJECT_DIR" && dotnet build --configuration Release)
echo "[SUCCESS] Updater build complete."

# --- 3. Setup Test Environment ---
echo "[STEP 2/7] Setting up test environment..."
# Create a temporary directory for our "installed" old version
INSTALL_DIR=$(mktemp -d)
# Create a temp directory for our "release" new version
RELEASE_DIR=$(mktemp -d)
# Create a dummy file representing the new version
echo "v2.0.0" > "$RELEASE_DIR/version.txt"
# Create the zip file that the updater will download
ZIP_FILE="$RELEASE_DIR/update.zip"
(cd "$RELEASE_DIR" && zip "$ZIP_FILE" version.txt)
# Calculate the hash of the zip file
FILE_HASH=$(sha256sum "$ZIP_FILE" | awk '{ print $1 }')

# Copy the "old" application to the install directory
# We need the API DLL and dependencies, the Updater DLL and dependencies, and runtime config files
cp -r "bin/Release/net8.0/"* "$INSTALL_DIR/"
cp -r "$UPDATER_PROJECT_DIR/bin/Release/net8.0/"* "$INSTALL_DIR/" 
# No need to make DLLs executable - they're run with dotnet

echo "  - Install Dir: $INSTALL_DIR"
echo "  - Release Zip: $ZIP_FILE"
echo "  - Release Hash: $FILE_HASH"
echo "[SUCCESS] Test environment created."

# --- 4. Configure and Start License Server ---
echo "[STEP 3/7] Configuring and starting license server..."
# Create a temporary, local web server to serve the update.zip
(cd "$RELEASE_DIR" && python3 -m http.server 8080 &)
HTTP_SERVER_PID=$!

# Modify the licenses.json to use our dynamic hash and URL
# IMPORTANT: Using a different delimiter for sed because paths contain '/'
sed -i "s|http://localhost:8080/update.zip|http://localhost:8080/$(basename $ZIP_FILE)|g" "$LICENSE_SERVER_DIR/licenses.json"
sed -i "s/dummy_hash_needs_to_be_replaced_by_test_script/$FILE_HASH/g" "$LICENSE_SERVER_DIR/licenses.json"

# Start the license server in the background
(cd "$LICENSE_SERVER_DIR" && dotnet run --configuration Release --urls="$LICENSE_SERVER_URL" &)
LICENSE_SERVER_PID=$!
# Give it a moment to start up
sleep 5
echo "[SUCCESS] License server is running."

# --- 5. Run the Outdated Application ---
echo "[STEP 4/7] Running the 'outdated' application..."
echo "This should trigger the auto-updater. Expect the app to start and then exit."
# We run the app from its install directory using dotnet
# We need to override the current version to be '1.0.0' to trigger the update
# We also need to set the license key to the one that forces an update
(cd "$INSTALL_DIR" && \
    ASPNETCORE_ENVIRONMENT="Development" \
    DOTNET_ASSEMBLY_VERSION="1.0.0" \
    LicenseSettings__LicenseKey="LICENSE-OBSOLETE-VERSION" \
    dotnet MicroJack.API.dll)

# The updater runs in the background. We need to give it time to work.
echo "[STEP 5/7] Waiting for updater to complete..."
sleep 10 # Wait for download, unzip, and relaunch

# --- 6. Verification ---
echo "[STEP 6/7] Verifying update..."
if [ -f "$INSTALL_DIR/version.txt" ]; then
    content=$(cat "$INSTALL_DIR/version.txt")
    if [ "$content" == "v2.0.0" ]; then
        echo "[SUCCESS] Update successful! 'version.txt' found with correct content."
    else
        echo "[FAILURE] Update failed! 'version.txt' has wrong content: $content"
        exit 1
    fi
else
    echo "[FAILURE] Update failed! 'version.txt' not found in install directory."
    exit 1
fi

# --- 7. Cleanup ---
echo "[STEP 7/7] Cleaning up..."
kill $LICENSE_SERVER_PID || echo "License server already stopped."
kill $HTTP_SERVER_PID || echo "HTTP server already stopped."
# Find and kill any remaining API or Updater processes
pkill -f MicroJack.API || true
pkill -f MicroJack.Updater || true
# Revert changes to licenses.json
git checkout -- "$LICENSE_SERVER_DIR/licenses.json"
# Remove temp directories
rm -rf "$INSTALL_DIR"
rm -rf "$RELEASE_DIR"
echo "[SUCCESS] Cleanup complete."

echo "--- TEST PASSED ---"
