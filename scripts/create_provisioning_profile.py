#!/usr/bin/env python3
"""
Apple Developer API - Provisioning Profile Creator

This script uses the App Store Connect API to:
1. Create or find an App ID for your bundle identifier
2. Create a new provisioning profile
3. Download the .mobileprovision file

Requirements:
    pip install pyjwt cryptography requests

Usage:
    python create_provisioning_profile.py

Before running, set the following environment variables or edit the CONFIG section:
    - APPLE_KEY_ID: Your API Key ID from App Store Connect
    - APPLE_ISSUER_ID: Your Issuer ID from App Store Connect
    - APPLE_PRIVATE_KEY_PATH: Path to your .p8 private key file
"""

import os
import sys
import json
import time
import base64
import requests
from datetime import datetime, timedelta

try:
    import jwt
except ImportError:
    print("Error: PyJWT not installed. Run: pip install pyjwt cryptography")
    sys.exit(1)

# ============================================
# CONFIGURATION - Your Apple Developer credentials
# ============================================
CONFIG = {
    # Your App Store Connect API Key details
    "KEY_ID": "5Q78MLL5J3",
    
    # Two possible Issuer IDs - will try both as fallback
    "ISSUER_IDS": [
        "e07e7416-7902-48a0-9239-6b66f0ad3cdb",
        "d21d6bd0-389d-4807-8fa6-2040ac9a8819"
    ],
    
    # Private key path - update this to your .p8 file location
    "PRIVATE_KEY_PATH": r"C:\Users\amoha\Desktop\game kw 2026\AuthKey_5Q78MLL5J3.p8",
    
    # App details
    "BUNDLE_ID": "com.failaka.games.adventure",
    "APP_NAME": "Failaka Museum Adventure",
    
    # Profile settings
    "PROFILE_NAME": "Failaka Museum Adventure App Store",
    "PROFILE_TYPE": "IOS_APP_STORE",  # Options: IOS_APP_STORE, IOS_APP_DEVELOPMENT, IOS_APP_ADHOC
    
    # Output
    "OUTPUT_PATH": "profile.mobileprovision",
}

# API Base URL
API_BASE = "https://api.appstoreconnect.apple.com/v1"


def load_private_key(key_path):
    """Load the private key from .p8 file"""
    try:
        with open(key_path, 'r') as f:
            return f.read()
    except FileNotFoundError:
        print(f"Error: Private key file not found: {key_path}")
        print("\nTo get your API key:")
        print("1. Go to https://appstoreconnect.apple.com/access/api")
        print("2. Click '+' to create a new key")
        print("3. Download the .p8 file")
        print("4. Note your Key ID and Issuer ID")
        sys.exit(1)


def generate_jwt_token(key_id, issuer_id, private_key):
    """Generate a JWT token for Apple API authentication"""
    # Token expires in 20 minutes (Apple's maximum)
    expiration = datetime.utcnow() + timedelta(minutes=20)
    
    payload = {
        "iss": issuer_id,
        "iat": int(time.time()),
        "exp": int(expiration.timestamp()),
        "aud": "appstoreconnect-v1"
    }
    
    headers = {
        "alg": "ES256",
        "kid": key_id,
        "typ": "JWT"
    }
    
    token = jwt.encode(payload, private_key, algorithm="ES256", headers=headers)
    return token


def make_api_request(method, endpoint, token, data=None, verbose=True):
    """Make an API request to App Store Connect"""
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    url = f"{API_BASE}{endpoint}"
    
    if verbose:
        print(f"  [DEBUG] {method} {url}")
    
    try:
        if method == "GET":
            response = requests.get(url, headers=headers, timeout=30)
        elif method == "POST":
            response = requests.post(url, headers=headers, json=data, timeout=30)
        elif method == "DELETE":
            response = requests.delete(url, headers=headers, timeout=30)
        else:
            raise ValueError(f"Unsupported method: {method}")
        
        if verbose:
            print(f"  [DEBUG] Response status: {response.status_code}")
            if response.status_code >= 400:
                print(f"  [DEBUG] Response body: {response.text[:500]}")
        
        return response
    except requests.exceptions.Timeout:
        print(f"  [ERROR] Request timed out")
        return None
    except requests.exceptions.ConnectionError as e:
        print(f"  [ERROR] Connection error: {e}")
        return None
    except requests.exceptions.RequestException as e:
        print(f"  [ERROR] Request error: {e}")
        return None


def find_bundle_id(token, identifier):
    """Find an existing Bundle ID"""
    print(f"Searching for Bundle ID: {identifier}")
    
    response = make_api_request(
        "GET",
        f"/bundleIds?filter[identifier]={identifier}",
        token
    )
    
    if response is None:
        print("  No response from API - authentication may have failed")
        return None
    
    if response.status_code == 200:
        data = response.json()
        if data.get("data") and len(data["data"]) > 0:
            bundle_id = data["data"][0]
            print(f"  Found Bundle ID: {bundle_id['id']}")
            return bundle_id["id"]
        else:
            print("  Bundle ID not found (will create new one)")
    elif response.status_code == 401:
        print("  ERROR: 401 Unauthorized - Check your API key and Issuer ID")
        print(f"  Response: {response.text[:300]}")
    elif response.status_code == 403:
        print("  ERROR: 403 Forbidden - API key may not have proper permissions")
        print(f"  Response: {response.text[:300]}")
    else:
        print(f"  ERROR: Unexpected status {response.status_code}")
        print(f"  Response: {response.text[:300]}")
    
    return None


def create_bundle_id(token, identifier, name):
    """Create a new Bundle ID"""
    print(f"Creating Bundle ID: {identifier}")
    
    data = {
        "data": {
            "type": "bundleIds",
            "attributes": {
                "identifier": identifier,
                "name": name,
                "platform": "IOS"
            }
        }
    }
    
    response = make_api_request("POST", "/bundleIds", token, data)
    
    if response is None:
        print("  No response from API")
        return None
    
    if response.status_code == 201:
        bundle_id = response.json()["data"]
        print(f"  Created Bundle ID: {bundle_id['id']}")
        return bundle_id["id"]
    elif response.status_code == 409:
        # Bundle ID already exists - try to find it
        print("  Bundle ID already exists, fetching existing one...")
        return find_bundle_id(token, identifier)
    elif response.status_code == 401:
        print("  ERROR: 401 Unauthorized - Invalid API credentials")
    elif response.status_code == 403:
        print("  ERROR: 403 Forbidden - Insufficient permissions")
    else:
        print(f"  Error creating Bundle ID: {response.status_code}")
    
    print(f"  Response: {response.text[:500]}")
    return None


def get_certificates(token, cert_type="IOS_DISTRIBUTION"):
    """Get available certificates"""
    print(f"Fetching {cert_type} certificates...")
    
    response = make_api_request(
        "GET",
        f"/certificates?filter[certificateType]={cert_type}",
        token
    )
    
    if response and response.status_code == 200:
        data = response.json()
        certs = data.get("data", [])
        print(f"  Found {len(certs)} certificates")
        return [cert["id"] for cert in certs]
    
    return []


def get_devices(token):
    """Get registered devices (for development/ad-hoc profiles)"""
    print("Fetching registered devices...")
    
    response = make_api_request("GET", "/devices?filter[platform]=IOS", token)
    
    if response and response.status_code == 200:
        data = response.json()
        devices = data.get("data", [])
        print(f"  Found {len(devices)} devices")
        return [device["id"] for device in devices]
    
    return []


def find_existing_profile(token, name):
    """Find an existing profile by name"""
    print(f"Checking for existing profile: {name}")
    
    response = make_api_request("GET", f"/profiles?filter[name]={name}", token)
    
    if response and response.status_code == 200:
        data = response.json()
        profiles = data.get("data", [])
        if profiles:
            print(f"  Found existing profile: {profiles[0]['id']}")
            return profiles[0]["id"]
    
    return None


def delete_profile(token, profile_id):
    """Delete an existing profile"""
    print(f"Deleting existing profile: {profile_id}")
    
    response = make_api_request("DELETE", f"/profiles/{profile_id}", token)
    
    if response and response.status_code == 204:
        print("  Profile deleted successfully")
        return True
    
    return False


def create_profile(token, name, profile_type, bundle_id, certificate_ids, device_ids=None):
    """Create a new provisioning profile"""
    print(f"Creating profile: {name}")
    print(f"  Type: {profile_type}")
    print(f"  Bundle ID: {bundle_id}")
    print(f"  Certificates: {len(certificate_ids)}")
    
    # Build relationships
    certificates = [{"type": "certificates", "id": cert_id} for cert_id in certificate_ids]
    
    data = {
        "data": {
            "type": "profiles",
            "attributes": {
                "name": name,
                "profileType": profile_type
            },
            "relationships": {
                "bundleId": {
                    "data": {"type": "bundleIds", "id": bundle_id}
                },
                "certificates": {
                    "data": certificates
                }
            }
        }
    }
    
    # Add devices for development/ad-hoc profiles
    if device_ids and profile_type in ["IOS_APP_DEVELOPMENT", "IOS_APP_ADHOC"]:
        devices = [{"type": "devices", "id": device_id} for device_id in device_ids]
        data["data"]["relationships"]["devices"] = {"data": devices}
        print(f"  Devices: {len(device_ids)}")
    
    response = make_api_request("POST", "/profiles", token, data)
    
    if response and response.status_code == 201:
        profile = response.json()["data"]
        print(f"  Profile created: {profile['id']}")
        return profile
    else:
        print(f"  Error creating profile: {response.status_code if response else 'No response'}")
        if response:
            print(f"  Response: {response.text}")
        return None


def download_profile(profile_data, output_path):
    """Download and save the provisioning profile"""
    print(f"Saving profile to: {output_path}")
    
    # The profile content is base64 encoded in the response
    profile_content = profile_data["attributes"].get("profileContent")
    
    if not profile_content:
        print("  Error: No profile content in response")
        return False
    
    # Decode and save
    try:
        decoded = base64.b64decode(profile_content)
        with open(output_path, 'wb') as f:
            f.write(decoded)
        print(f"  Profile saved successfully!")
        print(f"  File size: {len(decoded)} bytes")
        return True
    except Exception as e:
        print(f"  Error saving profile: {e}")
        return False


def test_api_connection(token):
    """Test if the API token works by making a simple request"""
    print("Testing API connection...")
    response = make_api_request("GET", "/bundleIds?limit=1", token, verbose=False)
    
    if response is None:
        print("  Connection failed - no response")
        return False, "No response from API"
    
    if response.status_code == 200:
        print("  API connection successful!")
        return True, None
    elif response.status_code == 401:
        error_msg = "401 Unauthorized - Invalid Issuer ID or API key"
        print(f"  {error_msg}")
        return False, error_msg
    elif response.status_code == 403:
        error_msg = "403 Forbidden - API key lacks permissions"
        print(f"  {error_msg}")
        return False, error_msg
    else:
        error_msg = f"Unexpected status: {response.status_code}"
        print(f"  {error_msg}")
        try:
            error_data = response.json()
            if "errors" in error_data:
                for err in error_data["errors"]:
                    print(f"    - {err.get('detail', err)}")
        except:
            pass
        return False, error_msg


def try_with_issuer(issuer_id, private_key):
    """Try to create profile with a specific issuer ID"""
    print(f"\n{'='*60}")
    print(f"Trying with Issuer ID: {issuer_id}")
    print("=" * 60)
    
    # Generate JWT token
    print("Generating API token...")
    token = generate_jwt_token(CONFIG["KEY_ID"], issuer_id, private_key)
    print("  Token generated")
    print()
    
    # Test the API connection first
    print("-" * 40)
    success, error = test_api_connection(token)
    if not success:
        print(f"  Skipping this Issuer ID: {error}")
        return False
    print()
    
    # Step 1: Find or create Bundle ID
    print("-" * 40)
    bundle_id = find_bundle_id(token, CONFIG["BUNDLE_ID"])
    if not bundle_id:
        bundle_id = create_bundle_id(token, CONFIG["BUNDLE_ID"], CONFIG["APP_NAME"])
        if not bundle_id:
            print("Error: Failed to create Bundle ID")
            return False
    print()
    
    # Step 2: Get certificates
    print("-" * 40)
    cert_type = "IOS_DISTRIBUTION" if "STORE" in CONFIG["PROFILE_TYPE"] else "IOS_DEVELOPMENT"
    certificate_ids = get_certificates(token, cert_type)
    if not certificate_ids:
        print(f"Error: No {cert_type} certificates found")
        return False
    print()
    
    # Step 3: Get devices (for dev/ad-hoc profiles)
    device_ids = None
    if CONFIG["PROFILE_TYPE"] in ["IOS_APP_DEVELOPMENT", "IOS_APP_ADHOC"]:
        print("-" * 40)
        device_ids = get_devices(token)
        print()
    
    # Step 4: Check for existing profile and delete if exists
    print("-" * 40)
    existing_profile_id = find_existing_profile(token, CONFIG["PROFILE_NAME"])
    if existing_profile_id:
        delete_profile(token, existing_profile_id)
    print()
    
    # Step 5: Create new profile
    print("-" * 40)
    profile = create_profile(
        token,
        CONFIG["PROFILE_NAME"],
        CONFIG["PROFILE_TYPE"],
        bundle_id,
        certificate_ids,
        device_ids
    )
    
    if not profile:
        print("Error: Failed to create profile")
        return False
    print()
    
    # Step 6: Download profile
    print("-" * 40)
    success = download_profile(profile, CONFIG["OUTPUT_PATH"])
    
    return success


def main():
    print("=" * 60)
    print("Apple Developer API - Provisioning Profile Creator")
    print("=" * 60)
    print()
    print(f"API Key ID: {CONFIG['KEY_ID']}")
    print(f"Bundle ID: {CONFIG['BUNDLE_ID']}")
    print(f"Profile Type: {CONFIG['PROFILE_TYPE']}")
    print()
    
    # Load private key
    print(f"Loading private key from: {CONFIG['PRIVATE_KEY_PATH']}")
    private_key = load_private_key(CONFIG["PRIVATE_KEY_PATH"])
    print("  Private key loaded")
    
    # Try each issuer ID until one works
    success = False
    for issuer_id in CONFIG["ISSUER_IDS"]:
        try:
            success = try_with_issuer(issuer_id, private_key)
            if success:
                print(f"\n✅ SUCCESS with Issuer ID: {issuer_id}")
                break
            else:
                print(f"\n❌ Failed with Issuer ID: {issuer_id}")
                print("Trying next issuer ID...")
        except Exception as e:
            print(f"\n❌ Error with Issuer ID {issuer_id}: {e}")
            print("Trying next issuer ID...")
            continue
    
    if success:
        print()
        print("=" * 60)
        print("SUCCESS!")
        print("=" * 60)
        print(f"Profile saved to: {CONFIG['OUTPUT_PATH']}")
        print()
        print("To install on your Mac:")
        print(f"  1. Double-click {CONFIG['OUTPUT_PATH']}")
        print("  2. Or run: open " + CONFIG['OUTPUT_PATH'])
        print()
        print("To use in Xcode:")
        print("  1. The profile should auto-install")
        print("  2. Select it in your project's Signing settings")
    else:
        print()
        print("=" * 60)
        print("FAILED")
        print("=" * 60)
        print("Could not create profile with any of the Issuer IDs")
        print("Please check:")
        print("  1. Your API Key has Admin or App Manager role")
        print("  2. The Issuer ID is correct")
        print("  3. You have a valid Distribution Certificate")
        sys.exit(1)


if __name__ == "__main__":
    main()
