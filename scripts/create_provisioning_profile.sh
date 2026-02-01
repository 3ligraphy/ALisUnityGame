#!/bin/bash
#
# Apple Developer API - Provisioning Profile Creator (Shell Version)
#
# This script uses the App Store Connect API to create and download
# a provisioning profile for your iOS app.
#
# Requirements:
#   - openssl (for JWT signing)
#   - curl (for API requests)
#   - jq (for JSON parsing) - install with: brew install jq
#
# Usage:
#   ./create_provisioning_profile.sh
#
# Before running, set the following environment variables:
#   APPLE_KEY_ID      - Your API Key ID
#   APPLE_ISSUER_ID   - Your Issuer ID  
#   APPLE_PRIVATE_KEY - Path to your .p8 file OR the key content itself
#

set -e

# ============================================
# CONFIGURATION
# ============================================
BUNDLE_ID="${BUNDLE_ID:-com.failaka.games.adventure}"
APP_NAME="${APP_NAME:-Failaka Museum Adventure}"
PROFILE_NAME="${PROFILE_NAME:-Failaka Museum Adventure App Store}"
PROFILE_TYPE="${PROFILE_TYPE:-IOS_APP_STORE}"  # IOS_APP_STORE, IOS_APP_DEVELOPMENT, IOS_APP_ADHOC
OUTPUT_FILE="${OUTPUT_FILE:-profile.mobileprovision}"

API_BASE="https://api.appstoreconnect.apple.com/v1"

# ============================================
# FUNCTIONS
# ============================================

check_requirements() {
    echo "Checking requirements..."
    
    if ! command -v jq &> /dev/null; then
        echo "Error: jq is required. Install with: brew install jq"
        exit 1
    fi
    
    if ! command -v openssl &> /dev/null; then
        echo "Error: openssl is required"
        exit 1
    fi
    
    if [ -z "$APPLE_KEY_ID" ]; then
        echo "Error: APPLE_KEY_ID environment variable not set"
        echo ""
        echo "To get your API key:"
        echo "  1. Go to https://appstoreconnect.apple.com/access/api"
        echo "  2. Click '+' to create a new key with 'Admin' access"
        echo "  3. Download the .p8 file"
        echo "  4. Note your Key ID and Issuer ID"
        echo ""
        echo "Then set environment variables:"
        echo "  export APPLE_KEY_ID='your_key_id'"
        echo "  export APPLE_ISSUER_ID='your_issuer_id'"
        echo "  export APPLE_PRIVATE_KEY='path/to/AuthKey.p8'"
        exit 1
    fi
    
    if [ -z "$APPLE_ISSUER_ID" ]; then
        echo "Error: APPLE_ISSUER_ID environment variable not set"
        exit 1
    fi
    
    if [ -z "$APPLE_PRIVATE_KEY" ]; then
        echo "Error: APPLE_PRIVATE_KEY environment variable not set"
        exit 1
    fi
    
    echo "✅ All requirements met"
}

get_private_key() {
    # Check if it's a file path or the key content
    if [ -f "$APPLE_PRIVATE_KEY" ]; then
        cat "$APPLE_PRIVATE_KEY"
    else
        echo "$APPLE_PRIVATE_KEY"
    fi
}

generate_jwt() {
    echo "Generating JWT token..."
    
    local KEY_ID="$APPLE_KEY_ID"
    local ISSUER_ID="$APPLE_ISSUER_ID"
    local PRIVATE_KEY=$(get_private_key)
    
    # Current time and expiration (20 minutes)
    local NOW=$(date +%s)
    local EXP=$((NOW + 1200))
    
    # Create JWT header
    local HEADER=$(echo -n '{"alg":"ES256","kid":"'"$KEY_ID"'","typ":"JWT"}' | base64 | tr -d '=' | tr '/+' '_-' | tr -d '\n')
    
    # Create JWT payload
    local PAYLOAD=$(echo -n '{"iss":"'"$ISSUER_ID"'","iat":'"$NOW"',"exp":'"$EXP"',"aud":"appstoreconnect-v1"}' | base64 | tr -d '=' | tr '/+' '_-' | tr -d '\n')
    
    # Create signature
    local SIGNATURE=$(echo -n "${HEADER}.${PAYLOAD}" | openssl dgst -sha256 -sign <(echo "$PRIVATE_KEY") | base64 | tr -d '=' | tr '/+' '_-' | tr -d '\n')
    
    JWT_TOKEN="${HEADER}.${PAYLOAD}.${SIGNATURE}"
    echo "✅ JWT token generated"
}

api_request() {
    local METHOD="$1"
    local ENDPOINT="$2"
    local DATA="$3"
    
    if [ -n "$DATA" ]; then
        curl -s -X "$METHOD" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: application/json" \
            -d "$DATA" \
            "${API_BASE}${ENDPOINT}"
    else
        curl -s -X "$METHOD" \
            -H "Authorization: Bearer $JWT_TOKEN" \
            -H "Content-Type: application/json" \
            "${API_BASE}${ENDPOINT}"
    fi
}

find_bundle_id() {
    echo "Searching for Bundle ID: $BUNDLE_ID"
    
    local RESPONSE=$(api_request "GET" "/bundleIds?filter[identifier]=$BUNDLE_ID")
    local BUNDLE_ID_RESULT=$(echo "$RESPONSE" | jq -r '.data[0].id // empty')
    
    if [ -n "$BUNDLE_ID_RESULT" ]; then
        echo "  Found: $BUNDLE_ID_RESULT"
        BUNDLE_ID_API_ID="$BUNDLE_ID_RESULT"
        return 0
    fi
    
    return 1
}

create_bundle_id() {
    echo "Creating Bundle ID: $BUNDLE_ID"
    
    local DATA='{
        "data": {
            "type": "bundleIds",
            "attributes": {
                "identifier": "'"$BUNDLE_ID"'",
                "name": "'"$APP_NAME"'",
                "platform": "IOS"
            }
        }
    }'
    
    local RESPONSE=$(api_request "POST" "/bundleIds" "$DATA")
    local BUNDLE_ID_RESULT=$(echo "$RESPONSE" | jq -r '.data.id // empty')
    
    if [ -n "$BUNDLE_ID_RESULT" ]; then
        echo "  Created: $BUNDLE_ID_RESULT"
        BUNDLE_ID_API_ID="$BUNDLE_ID_RESULT"
        return 0
    else
        echo "  Error: $(echo "$RESPONSE" | jq -r '.errors[0].detail // "Unknown error"')"
        return 1
    fi
}

get_certificates() {
    local CERT_TYPE="$1"
    echo "Fetching $CERT_TYPE certificates..."
    
    local RESPONSE=$(api_request "GET" "/certificates?filter[certificateType]=$CERT_TYPE")
    CERTIFICATE_IDS=$(echo "$RESPONSE" | jq -r '[.data[].id] | @json')
    local COUNT=$(echo "$RESPONSE" | jq '.data | length')
    
    echo "  Found: $COUNT certificates"
    
    if [ "$COUNT" -eq 0 ]; then
        return 1
    fi
    return 0
}

get_devices() {
    echo "Fetching registered devices..."
    
    local RESPONSE=$(api_request "GET" "/devices?filter[platform]=IOS")
    DEVICE_IDS=$(echo "$RESPONSE" | jq -r '[.data[].id] | @json')
    local COUNT=$(echo "$RESPONSE" | jq '.data | length')
    
    echo "  Found: $COUNT devices"
}

find_existing_profile() {
    echo "Checking for existing profile: $PROFILE_NAME"
    
    local ENCODED_NAME=$(echo -n "$PROFILE_NAME" | jq -sRr @uri)
    local RESPONSE=$(api_request "GET" "/profiles?filter[name]=$ENCODED_NAME")
    EXISTING_PROFILE_ID=$(echo "$RESPONSE" | jq -r '.data[0].id // empty')
    
    if [ -n "$EXISTING_PROFILE_ID" ]; then
        echo "  Found: $EXISTING_PROFILE_ID"
        return 0
    fi
    return 1
}

delete_profile() {
    local PROFILE_ID="$1"
    echo "Deleting existing profile: $PROFILE_ID"
    
    api_request "DELETE" "/profiles/$PROFILE_ID" > /dev/null
    echo "  Deleted"
}

create_profile() {
    echo "Creating profile: $PROFILE_NAME"
    echo "  Type: $PROFILE_TYPE"
    
    # Build certificate relationships
    local CERT_RELATIONSHIPS=$(echo "$CERTIFICATE_IDS" | jq '[.[] | {"type": "certificates", "id": .}]')
    
    # Base data
    local DATA='{
        "data": {
            "type": "profiles",
            "attributes": {
                "name": "'"$PROFILE_NAME"'",
                "profileType": "'"$PROFILE_TYPE"'"
            },
            "relationships": {
                "bundleId": {
                    "data": {"type": "bundleIds", "id": "'"$BUNDLE_ID_API_ID"'"}
                },
                "certificates": {
                    "data": '"$CERT_RELATIONSHIPS"'
                }
            }
        }
    }'
    
    # Add devices for development/ad-hoc profiles
    if [[ "$PROFILE_TYPE" == "IOS_APP_DEVELOPMENT" || "$PROFILE_TYPE" == "IOS_APP_ADHOC" ]]; then
        local DEVICE_RELATIONSHIPS=$(echo "$DEVICE_IDS" | jq '[.[] | {"type": "devices", "id": .}]')
        DATA=$(echo "$DATA" | jq '.data.relationships.devices = {"data": '"$DEVICE_RELATIONSHIPS"'}')
    fi
    
    local RESPONSE=$(api_request "POST" "/profiles" "$DATA")
    PROFILE_CONTENT=$(echo "$RESPONSE" | jq -r '.data.attributes.profileContent // empty')
    NEW_PROFILE_ID=$(echo "$RESPONSE" | jq -r '.data.id // empty')
    
    if [ -n "$PROFILE_CONTENT" ]; then
        echo "  Created: $NEW_PROFILE_ID"
        return 0
    else
        echo "  Error: $(echo "$RESPONSE" | jq -r '.errors[0].detail // "Unknown error"')"
        return 1
    fi
}

download_profile() {
    echo "Saving profile to: $OUTPUT_FILE"
    
    echo "$PROFILE_CONTENT" | base64 -d > "$OUTPUT_FILE"
    
    local SIZE=$(wc -c < "$OUTPUT_FILE" | tr -d ' ')
    echo "  Saved: $SIZE bytes"
}

# ============================================
# MAIN
# ============================================

echo "=========================================="
echo "Apple Developer API - Profile Creator"
echo "=========================================="
echo ""

check_requirements
echo ""

generate_jwt
echo ""

echo "------------------------------------------"
if ! find_bundle_id; then
    if ! create_bundle_id; then
        echo "Error: Failed to create Bundle ID"
        exit 1
    fi
fi
echo ""

echo "------------------------------------------"
CERT_TYPE="IOS_DISTRIBUTION"
if [[ "$PROFILE_TYPE" == "IOS_APP_DEVELOPMENT" ]]; then
    CERT_TYPE="IOS_DEVELOPMENT"
fi

if ! get_certificates "$CERT_TYPE"; then
    echo "Error: No $CERT_TYPE certificates found"
    echo "Please create a certificate in Apple Developer portal first"
    exit 1
fi
echo ""

if [[ "$PROFILE_TYPE" == "IOS_APP_DEVELOPMENT" || "$PROFILE_TYPE" == "IOS_APP_ADHOC" ]]; then
    echo "------------------------------------------"
    get_devices
    echo ""
fi

echo "------------------------------------------"
if find_existing_profile; then
    delete_profile "$EXISTING_PROFILE_ID"
fi
echo ""

echo "------------------------------------------"
if ! create_profile; then
    echo "Error: Failed to create profile"
    exit 1
fi
echo ""

echo "------------------------------------------"
download_profile
echo ""

echo "=========================================="
echo "✅ SUCCESS!"
echo "=========================================="
echo "Profile saved to: $OUTPUT_FILE"
echo ""
echo "To install on Mac:"
echo "  open $OUTPUT_FILE"
echo ""
