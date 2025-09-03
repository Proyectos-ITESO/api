#!/bin/bash

# Test script for the new admin endpoints
# This script tests user and role management functionality

echo "🔐 Testing Admin Endpoints - User & Role Management"
echo "===================================================="

BASE_URL="http://localhost:5134"
TOKEN=""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

print_admin() {
    echo -e "${BLUE}🔧 $1${NC}"
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

echo ""
print_admin "=== TESTING ROLE MANAGEMENT ==="

# 1. Get all roles
print_info "Testing get all roles..."
ROLES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/roles" \
    -H "$AUTH_HEADER")

if [[ $ROLES_RESPONSE == *"success"* ]]; then
    print_success "Get all roles works"
    echo "Response: $ROLES_RESPONSE" | head -c 300
    echo ""
else
    print_error "Get all roles failed"
    echo "Response: $ROLES_RESPONSE"
fi

# 2. Create a new role (SuperAdmin only)
print_info "Testing create new role..."
CREATE_ROLE_DATA='{
    "name": "TestRole",
    "description": "Role for testing purposes",
    "permissions": "[1, 2, 3]"
}'

CREATE_ROLE_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/admin/roles" \
    -H "$AUTH_HEADER" \
    -H "$JSON_HEADER" \
    -d "$CREATE_ROLE_DATA")

if [[ $CREATE_ROLE_RESPONSE == *"success"* ]]; then
    print_success "Create role works"
    echo "Response: $CREATE_ROLE_RESPONSE" | head -c 200
    echo ""
    
    # Extract role ID for future tests
    ROLE_ID=$(echo $CREATE_ROLE_RESPONSE | grep -o '"id":[0-9]*' | sed 's/"id"://')
else
    print_error "Create role failed (might need SuperAdmin permissions)"
    echo "Response: $CREATE_ROLE_RESPONSE"
fi

echo ""
print_admin "=== TESTING PERMISSION MANAGEMENT ==="

# 3. Get role permissions
if [[ ! -z "$ROLE_ID" ]]; then
    print_info "Testing get role permissions..."
    PERMISSIONS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/roles/${ROLE_ID}/permissions" \
        -H "$AUTH_HEADER")

    if [[ $PERMISSIONS_RESPONSE == *"success"* ]]; then
        print_success "Get role permissions works"
        echo "Response: $PERMISSIONS_RESPONSE" | head -c 200
        echo ""
    else
        print_error "Get role permissions failed"
        echo "Response: $PERMISSIONS_RESPONSE"
    fi

    # 4. Add permission to role
    print_info "Testing add permission to role..."
    ADD_PERMISSION_DATA='{
        "permission": 10
    }'

    ADD_PERMISSION_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/admin/roles/${ROLE_ID}/permissions" \
        -H "$AUTH_HEADER" \
        -H "$JSON_HEADER" \
        -d "$ADD_PERMISSION_DATA")

    if [[ $ADD_PERMISSION_RESPONSE == *"success"* ]]; then
        print_success "Add permission to role works"
        echo "Response: $ADD_PERMISSION_RESPONSE"
    else
        print_error "Add permission to role failed (might need SuperAdmin permissions)"
        echo "Response: $ADD_PERMISSION_RESPONSE"
    fi
fi

echo ""
print_admin "=== TESTING USER MANAGEMENT ==="

# 5. Get all users (guards)
print_info "Testing get all users..."
USERS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/guards/" \
    -H "$AUTH_HEADER")

if [[ $USERS_RESPONSE == *"success"* ]]; then
    print_success "Get all users works"
    echo "Response: $USERS_RESPONSE" | head -c 300
    echo ""
    
    # Extract first user ID for future tests
    USER_ID=$(echo $USERS_RESPONSE | grep -o '"id":[0-9]*' | head -1 | sed 's/"id"://')
else
    print_error "Get all users failed"
    echo "Response: $USERS_RESPONSE"
fi

# 6. Get user roles
if [[ ! -z "$USER_ID" ]]; then
    print_info "Testing get user roles..."
    USER_ROLES_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/users/${USER_ID}/roles" \
        -H "$AUTH_HEADER")

    if [[ $USER_ROLES_RESPONSE == *"success"* ]]; then
        print_success "Get user roles works"
        echo "Response: $USER_ROLES_RESPONSE" | head -c 200
        echo ""
    else
        print_error "Get user roles failed"
        echo "Response: $USER_ROLES_RESPONSE"
    fi

    # 7. Get user permissions
    print_info "Testing get user permissions..."
    USER_PERMISSIONS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/users/${USER_ID}/permissions" \
        -H "$AUTH_HEADER")

    if [[ $USER_PERMISSIONS_RESPONSE == *"success"* ]]; then
        print_success "Get user permissions works"
        echo "Response: $USER_PERMISSIONS_RESPONSE" | head -c 200
        echo ""
    else
        print_error "Get user permissions failed"
        echo "Response: $USER_PERMISSIONS_RESPONSE"
    fi
fi

echo ""
print_admin "=== TESTING UTILITY ENDPOINTS ==="

# 8. Get all available permissions
print_info "Testing get all permissions..."
ALL_PERMISSIONS_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/permissions" \
    -H "$AUTH_HEADER")

if [[ $ALL_PERMISSIONS_RESPONSE == *"success"* ]]; then
    print_success "Get all permissions works"
    echo "Response: $ALL_PERMISSIONS_RESPONSE" | head -c 300
    echo ""
else
    print_error "Get all permissions failed"
    echo "Response: $ALL_PERMISSIONS_RESPONSE"
fi

# 9. Search users
print_info "Testing search users..."
SEARCH_RESPONSE=$(curl -s -X GET "${BASE_URL}/api/admin/users/search?searchTerm=admin" \
    -H "$AUTH_HEADER")

if [[ $SEARCH_RESPONSE == *"success"* ]]; then
    print_success "Search users works"
    echo "Response: $SEARCH_RESPONSE" | head -c 200
    echo ""
else
    print_error "Search users failed"
    echo "Response: $SEARCH_RESPONSE"
fi

echo ""
print_admin "=== TESTING PASSWORD MANAGEMENT ==="

# 10. Test change own password
print_info "Testing change own password..."
CHANGE_PASSWORD_DATA='{
    "currentPassword": "admin123",
    "newPassword": "newAdmin123!"
}'

CHANGE_PASSWORD_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/auth/change-password" \
    -H "$AUTH_HEADER" \
    -H "$JSON_HEADER" \
    -d "$CHANGE_PASSWORD_DATA")

if [[ $CHANGE_PASSWORD_RESPONSE == *"success"* ]]; then
    print_success "Change password works"
    echo "Response: $CHANGE_PASSWORD_RESPONSE"
    
    # Change back to original password
    print_info "Reverting password change..."
    REVERT_PASSWORD_DATA='{
        "currentPassword": "newAdmin123!",
        "newPassword": "admin123"
    }'
    
    REVERT_RESPONSE=$(curl -s -X POST "${BASE_URL}/api/auth/change-password" \
        -H "$AUTH_HEADER" \
        -H "$JSON_HEADER" \
        -d "$REVERT_PASSWORD_DATA")
    
    if [[ $REVERT_RESPONSE == *"success"* ]]; then
        print_success "Password reverted successfully"
    else
        print_error "Failed to revert password"
        echo "Response: $REVERT_RESPONSE"
    fi
else
    print_error "Change password failed"
    echo "Response: $CHANGE_PASSWORD_RESPONSE"
fi

echo ""
echo "🎯 Admin System Test Complete"
echo "============================"
print_info "Admin Management System includes:"
echo "  🔐 Role Management (CRUD operations)"
echo "  🔐 Permission Management by Role"
echo "  👥 User-Role Assignment"
echo "  🔍 User Search and Permissions View"
echo "  🔑 Password Change (own password)"
echo "  🛡️ SuperAdmin-only operations"
echo ""
print_info "Permission Levels:"
echo "  🔵 GuardLevel: Guard, Admin, SuperAdmin"
echo "  🟡 AdminLevel: Admin, SuperAdmin"
echo "  🔴 SuperAdminLevel: Only SuperAdmin"
echo ""
print_admin "SuperAdmin capabilities:"
echo "  • Create/update/delete roles"
echo "  • Manage role permissions"
echo "  • Assign roles to users"
echo "  • Create other SuperAdmins"
echo "  • Reset user passwords"
echo "  • Activate/deactivate users"