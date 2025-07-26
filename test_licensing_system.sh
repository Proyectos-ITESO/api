#!/bin/bash

# Test script for MicroJack Licensing System
# This script checks that the licensing server and the client API work together.

API_BASE="http://localhost:5134"
LICENSE_SERVER_BASE="http://localhost:5101"
# Use the same key as in licenses.json
VALID_LICENSE_KEY="YOUR_LICENSE_KEY_HERE" 
MACHINE_ID="test-machine-123"

echo "🔄 Starting MicroJack Licensing System Test"
echo "=============================================="

# Function to make API calls with better error handling
# Usage: api_call <BASE_URL> <METHOD> <ENDPOINT> [DATA]
api_call() {
    local base_url=$1
    local method=$2
    local endpoint=$3
    local data=$4
    
    echo "📡 $method $base_url$endpoint" >&2

    if [ -n "$data" ]; then
        curl -s -X $method "$base_url$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data"
    else
        curl -s -X $method "$base_url$endpoint"
    fi
}

# Function to check HTTP status code
# Usage: get_status <BASE_URL> <METHOD> <ENDPOINT>
get_status() {
    local base_url=$1
    local method=$2
    local endpoint=$3
    
    curl -s -o /dev/null -w "%{http_code}" -X $method "$base_url$endpoint"
}

echo ""
echo "🌐 Step 1: Check Server Health"
echo "1.1 - Checking Licensing Server..."
license_status=$(get_status $LICENSE_SERVER_BASE GET "/api/validate")
if [ "$license_status" -eq 400 ] || [ "$license_status" -eq 404 ]; then
    echo "✅ Licensing Server is responding (Status: $license_status)"
else
    echo "❌ Licensing Server Health Check: FAILED (Status: $license_status)"
    echo "Is the licensing server running on $LICENSE_SERVER_BASE?"
    exit 1
fi

echo ""
echo "1.2 - Checking Main API Server..."
api_status=$(get_status $API_BASE GET "/api/auth/health")
if [ "$api_status" -eq 200 ]; then
    echo "✅ Main API is responding (Status: $api_status)"
    echo "✅ Implicit Test: Main API started up, which means initial license check passed!"
else
    echo "❌ Main API Health Check: FAILED (Status: $api_status)"
    echo "Did the main API fail to start due to a license check issue? Check its logs."
    exit 1
fi

echo ""
echo "🔑 Step 2: Test Licensing Server Logic"
echo "2.1 - Requesting a valid license"
valid_response=$(api_call $LICENSE_SERVER_BASE GET "/api/validate?licenseKey=$VALID_LICENSE_KEY&machineId=$MACHINE_ID")

if echo "$valid_response" | jq -e '.signature' > /dev/null 2>&1; then
    echo "$valid_response" | jq .
    echo "✅ Valid License Test: PASSED"
else
    echo "❌ Valid License Test: FAILED - No signature in response"
    echo "Response: $valid_response"
    exit 1
fi

echo ""
echo "2.2 - Requesting an invalid license"
invalid_status=$(get_status $LICENSE_SERVER_BASE GET "/api/validate?licenseKey=INVALID-KEY&machineId=$MACHINE_ID")
if [ "$invalid_status" -eq 404 ]; then
    echo "✅ Invalid License Test: PASSED (Correctly received 404 Not Found)"
else
    echo "❌ Invalid License Test: FAILED - Expected 404, but got $invalid_status"
    exit 1
fi

echo ""
echo "🎯 FINAL RESULTS"
echo "==============="
echo "✅ All licensing system tests passed successfully!"
echo ""
echo "📊 Summary:"
echo "- ✅ Licensing server is running and responding."
echo "- ✅ Main API is running, indicating a successful initial license validation."
echo "- ✅ Licensing server correctly issues signed responses for valid keys."
echo "- ✅ Licensing server correctly rejects invalid keys."
echo ""
echo "🚀 Licensing system is working as expected!"
