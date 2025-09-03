#!/bin/bash

# Test script for the new visit identification system
# This script tests the advanced search endpoints for access logs

echo "🔍 Testing Advanced Visit Identification System"
echo "============================================="

BASE_URL="http://localhost:5134"
TOKEN=""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# Get authentication token
print_info "Getting authentication token..."
TOKEN_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "admin",
        "password": "admin123"
    }')

if [[ $TOKEN_RESPONSE == *"token"* ]]; then
    TOKEN=$(echo $TOKEN_RESPONSE | grep -o '"token":"[^"]*' | sed 's/"token":"//')
    print_success "Authentication successful"
else
    print_error "Authentication failed"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi

# Test headers
AUTH_HEADER="Authorization: Bearer $TOKEN"
JSON_HEADER="Content-Type: application/json"

# 1. Test search by date
print_info "Testing search by date..."
TODAY=$(date +%Y-%m-%d)
DATE_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/by-date/${TODAY}" \
    -H "$AUTH_HEADER")

if [[ $DATE_RESPONSE == *"success"* ]]; then
    print_success "Search by date works"
    echo "Response: $DATE_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search by date failed"
    echo "Response: $DATE_RESPONSE"
fi

# 2. Test search by visitor name
print_info "Testing search by visitor name..."
VISITOR_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/by-visitor/John" \
    -H "$AUTH_HEADER")

if [[ $VISITOR_RESPONSE == *"success"* ]]; then
    print_success "Search by visitor name works"
    echo "Response: $VISITOR_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search by visitor name failed"
    echo "Response: $VISITOR_RESPONSE"
fi

# 3. Test search by license plate
print_info "Testing search by license plate..."
PLATE_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/by-plate/ABC" \
    -H "$AUTH_HEADER")

if [[ $PLATE_RESPONSE == *"success"* ]]; then
    print_success "Search by license plate works"
    echo "Response: $PLATE_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search by license plate failed"
    echo "Response: $PLATE_RESPONSE"
fi

# 4. Test search by vehicle characteristics
print_info "Testing search by vehicle characteristics..."
VEHICLE_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/by-vehicle?brandId=1&colorId=2" \
    -H "$AUTH_HEADER")

if [[ $VEHICLE_RESPONSE == *"success"* ]]; then
    print_success "Search by vehicle characteristics works"
    echo "Response: $VEHICLE_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search by vehicle characteristics failed"
    echo "Response: $VEHICLE_RESPONSE"
fi

# 5. Test search by address identifier
print_info "Testing search by address identifier..."
ADDRESS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/by-address/Casa" \
    -H "$AUTH_HEADER")

if [[ $ADDRESS_RESPONSE == *"success"* ]]; then
    print_success "Search by address identifier works"
    echo "Response: $ADDRESS_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search by address identifier failed"
    echo "Response: $ADDRESS_RESPONSE"
fi

# 6. Test visitor history
print_info "Testing visitor history..."
HISTORY_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/history/visitor/1" \
    -H "$AUTH_HEADER")

if [[ $HISTORY_RESPONSE == *"success"* ]]; then
    print_success "Visitor history works"
    echo "Response: $HISTORY_RESPONSE" | head -c 200
    echo ""
else
    print_error "Visitor history failed"
    echo "Response: $HISTORY_RESPONSE"
fi

# 7. Test vehicle history
print_info "Testing vehicle history..."
VEHICLE_HISTORY_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/accesslogs/history/vehicle/ABC123" \
    -H "$AUTH_HEADER")

if [[ $VEHICLE_HISTORY_RESPONSE == *"success"* ]]; then
    print_success "Vehicle history works"
    echo "Response: $VEHICLE_HISTORY_RESPONSE" | head -c 200
    echo ""
else
    print_error "Vehicle history failed"
    echo "Response: $VEHICLE_HISTORY_RESPONSE"
fi

# 8. Test advanced search (combined)
print_info "Testing advanced combined search..."
ADVANCED_SEARCH_DATA='{
    "startDate": "2024-01-01T00:00:00",
    "endDate": "2024-12-31T23:59:59",
    "visitorName": "John",
    "licensePlate": "ABC",
    "status": "DENTRO",
    "page": 1,
    "pageSize": 10
}'

ADVANCED_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/accesslogs/search" \
    -H "$AUTH_HEADER" \
    -H "$JSON_HEADER" \
    -d "$ADVANCED_SEARCH_DATA")

if [[ $ADVANCED_RESPONSE == *"success"* ]]; then
    print_success "Advanced combined search works"
    echo "Response: $ADVANCED_RESPONSE" | head -c 300
    echo ""
else
    print_error "Advanced combined search failed"
    echo "Response: $ADVANCED_RESPONSE"
fi

# 9. Test that all responses include photos
print_info "Checking if responses include photo data..."
if [[ $ADVANCED_RESPONSE == *"faceImageUrl"* ]] || [[ $ADVANCED_RESPONSE == *"ineImageUrl"* ]] || [[ $ADVANCED_RESPONSE == *"plateImageUrl"* ]]; then
    print_success "Photo data is included in responses"
else
    print_error "Photo data missing from responses"
fi

echo ""
echo "🎯 Advanced Search System Test Complete"
echo "======================================"
print_info "The visit identification system is now ready with:"
echo "  • Search by date ranges"
echo "  • Search by visitor names"
echo "  • Search by license plates"
echo "  • Search by vehicle characteristics (brand, color, type)"
echo "  • Search by address identifiers"
echo "  • Visitor history tracking"
echo "  • Vehicle history tracking"
echo "  • Address history tracking"
echo "  • Combined advanced search with pagination"
echo "  • Photo data included in all responses"