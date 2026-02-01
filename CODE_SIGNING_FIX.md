# Code Signing Fix - App Store Connect API Integration

## Problem

The error "Cannot save Signing Certificates without certificate private key" occurs because the `app-store-connect fetch-signing-files --create` command needs a private key to create new certificates.

## Solution

The configuration now:
1. References a `code-signing` environment group that contains `CERTIFICATE_PRIVATE_KEY`
2. Uses App Store Connect API integration to automatically fetch or create certificates and provisioning profiles

## ⚠️ REQUIRED: Set Up CERTIFICATE_PRIVATE_KEY

**You MUST set up the private key in CodeMagic for automatic certificate creation to work:**

### Step 1: Generate a Private Key

On your Mac, run:
```bash
ssh-keygen -t rsa -b 2048 -m PEM -f cert_key -q -N ""
cat cert_key  # Copy the entire output including BEGIN/END lines
```

### Step 2: Add to CodeMagic

1. Go to **CodeMagic dashboard** → **Teams** → **Your Team**
2. Click on **Global variables & secrets**
3. Click **Add group** and name it: `code-signing`
4. Click **Add variable**:
   - **Variable name**: `CERTIFICATE_PRIVATE_KEY`
   - **Variable value**: Paste the entire private key (including `-----BEGIN RSA PRIVATE KEY-----` and `-----END RSA PRIVATE KEY-----` lines)
   - **Mark as secure**: ✅ Yes
5. Click **Save**

### Step 3: Verify App Store Connect Integration

Make sure your **App Store Connect integration** is properly configured:

1. Go to **CodeMagic dashboard** → **Your app** → **Settings** → **Integrations**
2. Verify "Code Magic illusionaire" integration is active
3. If not set up, add it with:
   - **Issuer ID** (from App Store Connect → Users and Access → Keys)
   - **Key ID** (from your API key)
   - **API Key** (.p8 file from App Store Connect)

## How It Works Now

CodeMagic will:
1. Use your App Store Connect integration ("Code Magic illusionaire")
2. Use `CERTIFICATE_PRIVATE_KEY` to generate a Certificate Signing Request (CSR)
3. Call `app-store-connect fetch-signing-files --create` to:
   - Fetch existing certificates/profiles, OR
   - Create new ones using the CSR
4. Initialize the keychain and add the certificates
5. Apply the profiles to your Xcode project
6. Build and sign the IPA

### Build Steps for Code Signing:

```yaml
environment:
  groups:
    - code-signing  # Must contain CERTIFICATE_PRIVATE_KEY

scripts:
  - name: Fetch signing files using App Store Connect API
    script: |
      app-store-connect fetch-signing-files "${BUNDLE_ID}" \
        --type IOS_APP_STORE \
        --create \
        --verbose

  - name: Set up keychain for code signing
    script: |
      keychain initialize
      keychain add-certificates

  - name: Set up code signing settings on Xcode project
    script: |
      xcode-project use-profiles
```

## Files Changed

- ✅ `codemagic.yaml` - Main build configuration
- ✅ `codemagic-simulator.yaml` - Simulator build configuration  
- ✅ `codemagic-working-final-sure.yaml` - Working final configuration

All files now:
1. Reference the `code-signing` group containing `CERTIFICATE_PRIVATE_KEY`
2. Use App Store Connect API integration for automatic certificate management

## Troubleshooting

### Error: "Cannot save Signing Certificates without certificate private key"

This means `CERTIFICATE_PRIVATE_KEY` is not set. Follow Step 1 and Step 2 above.

### Error: "No valid signing certificates found"

1. Verify your App Store Connect integration is active
2. Check that your Apple Developer account has Admin or App Manager role
3. Ensure you haven't exceeded the certificate limit (Apple allows 3 distribution certificates)

### Error: "Bundle ID not found"

1. Verify `BUNDLE_ID` in codemagic.yaml matches your app's bundle identifier
2. Ensure the App ID is registered in Apple Developer Portal

## Need Help?

If you still encounter issues:

1. **Check CodeMagic build logs** for specific error messages
2. **Verify the `code-signing` group** contains `CERTIFICATE_PRIVATE_KEY`
3. **Consult CodeMagic docs**: https://docs.codemagic.io/yaml-code-signing/signing-ios/

---

**Status: ✅ UPDATED**

Your builds should now work correctly once you set up `CERTIFICATE_PRIVATE_KEY`!
