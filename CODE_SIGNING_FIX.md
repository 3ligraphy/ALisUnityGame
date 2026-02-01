# Code Signing Fix - App Store Connect API Integration

## Problem

The previous `codemagic.yaml` configuration used the `ios_signing` section which relies on pre-configured/pre-uploaded certificates and provisioning profiles. This approach doesn't use the App Store Connect API to automatically create new signing assets.

## Solution

The configuration now uses App Store Connect API integration to automatically fetch or create certificates and provisioning profiles on-the-fly.

## How It Works Now

CodeMagic will now:
1. Use your App Store Connect integration ("Code Magic illusionaire")
2. Call `app-store-connect fetch-signing-files` with `--create` flag to automatically create certificates and provisioning profiles if they don't exist
3. Initialize the keychain and add the fetched certificates
4. Apply the profiles to your Xcode project
5. Build and sign the IPA

### New Build Steps for Code Signing:

```yaml
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

## What You Need To Do

### Required: App Store Connect API Integration

Make sure your **App Store Connect integration** is properly configured in CodeMagic:

1. **Go to CodeMagic dashboard**
2. **Click on your app** (ALisUnityGame)
3. **Go to Settings → Integrations**
4. **Verify "Code Magic illusionaire" integration is active**

#### If the integration is not set up:

1. In CodeMagic, go to **Team integrations**
2. Click **Add integration**
3. Select **App Store Connect**
4. Enter your App Store Connect credentials:
   - **Issuer ID** (from App Store Connect → Users and Access → Keys)
   - **Key ID** (from your API key)
   - **API Key** (download the .p8 file from App Store Connect)
5. Name it: `Code Magic illusionaire` (or update the name in codemagic.yaml line 25)
6. Click **Save**

## Key Differences from Previous Approach

| Previous Approach | New Approach |
|------------------|--------------|
| Used `ios_signing` section | Uses `app-store-connect fetch-signing-files` |
| Required pre-uploaded certificates | Creates certificates automatically via API |
| Manual provisioning profile management | Automatic provisioning profile creation |
| Static configuration | Dynamic, API-driven signing |

## Files Changed

- ✅ `codemagic.yaml` - Main build configuration
- ✅ `codemagic-simulator.yaml` - Simulator build configuration  
- ✅ `codemagic-working-final-sure.yaml` - Working final configuration

All files now use the App Store Connect API integration for automatic certificate and provisioning profile management.

## Testing Your Fix

To verify the fix works:

1. **Push these changes** to your repository
2. **Trigger a new build** in CodeMagic
3. **Watch the build logs** - you should see:
   - "Fetching/Creating signing files via App Store Connect API..."
   - Automatic certificate and profile creation/fetching
   - "Setting up keychain with certificates..."
   - "Applying provisioning profiles to Xcode..."
   - Successful code signing

The build should now use the App Store Connect API to manage signing assets automatically!

## Additional Notes

- The `--create` flag is crucial - it enables automatic creation of signing assets if they don't exist
- This approach is more dynamic and doesn't require pre-uploading certificates
- The integration with App Store Connect API handles everything automatically
- No need to manually manage certificate private keys

## Need Help?

If you still encounter issues:

1. **Check the App Store Connect integration** is active
2. **Verify your Apple Developer account** has the necessary permissions
3. **Check CodeMagic build logs** for specific error messages
4. **Consult CodeMagic docs**: https://docs.codemagic.io/yaml-code-signing/signing-ios/

---

**Status: ✅ UPDATED**

Your builds should now work correctly with API-integrated code signing!
