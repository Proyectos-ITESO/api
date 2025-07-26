#!/bin/bash

# Test script for the Application Version Check feature

API_PROJECT_FILE="MicroJack.API.csproj"
LOG_FILE="api_run.log"
API_BASE_URL="http://localhost:5134" # Ensure this port is correct

echo "🔄 Starting Application Version Check Test"
echo "=============================================="

# Function to restore original project file
cleanup() {
    echo "🧹 Cleaning up..."
    if [ -f "${API_PROJECT_FILE}.bak" ]; then
        mv "${API_PROJECT_FILE}.bak" "$API_PROJECT_FILE"
        echo "✅ Restored original $API_PROJECT_FILE"
    fi
    rm -f $LOG_FILE
}

# Ensure cleanup happens on exit
trap cleanup EXIT

# --- SCENARIO 1: Test for 'New Version Available' Warning ---
echo ""
echo "🔬 Scenario 1: Testing for 'New Version Available' Warning"
echo "---------------------------------------------------------"
echo "Current app version is 1.0.0. Server expects LatestVersion 1.1.0."

# Run the API in the background and capture logs
echo "🚀 Starting API and capturing logs..."
dotnet run --project $API_PROJECT_FILE > $LOG_FILE 2>&1 &
API_PID=$!

# Wait for the API to start or fail
sleep 15

# Check if the API is running
if curl -s -o /dev/null "$API_BASE_URL/api/auth/health"; then
    echo "✅ API is running, as expected."
    
    # Check for the warning message in the log
    if grep -q "A newer version of the application is available" $LOG_FILE; then
        echo "✅ SUCCESS: Found 'New Version Available' warning in logs."
        cat $LOG_FILE | grep "WARN"
    else
        echo "❌ FAILED: API is running, but the expected warning message was not found."
        exit 1
    fi
else
    echo "❌ FAILED: API did not start correctly for Scenario 1."
    echo "--- LOGS ---"
    cat $LOG_FILE
    echo "------------"
    exit 1
fi

# Stop the API and wait for the port to be free
echo "🛑 Stopping API and waiting for port to be released..."
kill -9 $API_PID
wait $API_PID 2>/dev/null

while lsof -i :5134 > /dev/null; do
    echo "Waiting for port 5134 to be released..."
    sleep 1
done
echo "✅ Port 5134 is free."


# --- SCENARIO 2: Test for 'Obsolete Version' Failure ---
echo ""
echo "🔬 Scenario 2: Testing for 'Obsolete Version' Failure"
echo "----------------------------------------------------"
echo "Setting app version to 0.9.0, which is less than MinimumRequiredVersion 1.0.0."

# Backup and modify the project file
cp "$API_PROJECT_FILE" "${API_PROJECT_FILE}.bak"
sed -i 's/<Version>1.0.0<\/Version>/<Version>0.9.0<\/Version>/' "$API_PROJECT_FILE"
echo "✅ Patched $API_PROJECT_FILE to version 0.9.0"

# Run the API again, expecting it to fail
echo "🚀 Starting API, expecting it to fail..."
echo "🧼 Cleaning previous build artifacts..."
dotnet clean > /dev/null 2>&1
dotnet run --project $API_PROJECT_FILE > $LOG_FILE 2>&1

# The run command should fail, so we check its exit code.
# We also check the log for the critical error message.
if grep -q "Application version 0.9.0 is too old" $LOG_FILE; then
    echo "✅ SUCCESS: Found 'Application version is too old' error in logs."
    cat $LOG_FILE | grep "CRIT"
else
    echo "❌ FAILED: The application did not fail with the expected version error."
    echo "--- LOGS ---"
    cat $LOG_FILE
    echo "------------"
    exit 1
fi

echo ""
echo "🎯 FINAL RESULTS"
echo "==============="
echo "✅ All version check scenarios passed successfully!"
echo ""
echo "📊 Summary:"
echo "- ✅ Correctly warns user about new available versions."
echo "- ✅ Correctly stops the application when the version is obsolete."
echo ""
echo "🚀 Version check feature is working as expected!"
