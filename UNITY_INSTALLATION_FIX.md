# Unity Installation Fix

## Problem
The build was failing because Unity wasn't found at the expected path. CodeMagic doesn't automatically install Unity, so we need to install it first.

## Solution
I've added a **"Install Unity"** step before the build step that:
1. Checks if Unity is already installed
2. Downloads Unity installer if needed
3. Installs Unity to the correct location
4. Verifies the installation

## What Changed

### Added: Unity Installation Step
- New step runs before "Build Unity Project for iOS"
- Downloads Unity ${UNITY_VERSION} from Unity's servers
- Installs it to `/Applications/Unity/Hub/Editor/${UNITY_VERSION}/`
- Handles errors gracefully with multiple download URL attempts

### Improved: Unity Path Detection
- Better error messages if Unity isn't found
- Searches multiple locations
- Provides helpful troubleshooting information

## How It Works Now

1. **Install Unity Step**:
   - Checks if Unity is already installed
   - If not, downloads and installs it
   - Verifies installation

2. **Build Unity Project Step**:
   - Finds Unity (should be installed by previous step)
   - Builds your iOS project
   - Creates Xcode project

3. **Rest of the build** continues as before

## If Build Still Fails

### Error: "Failed to download Unity installer"
**Possible causes:**
- Unity version ${UNITY_VERSION} might not be available for direct download
- Network issues
- Unity changed their download URLs

**Solutions:**
1. Check if Unity version is correct: `6000.2.15f1`
2. Try a different Unity version (update `UNITY_VERSION` in codemagic.yaml)
3. Unity might need to be pre-installed on CodeMagic machines

### Error: "Unity executable not found"
**Possible causes:**
- Installation step failed silently
- Unity installed to different location

**Solutions:**
1. Check the "Install Unity" step logs
2. The script will search for Unity in all common locations
3. If found elsewhere, it will use that location

## Alternative: Use Pre-installed Unity

If CodeMagic machines have Unity pre-installed, you might be able to:
1. Remove the "Install Unity" step
2. Update the Unity path detection to find pre-installed Unity

But the current solution should work for most cases.

## Testing

Try building again - the Unity installation step should now:
1. ✅ Download Unity if needed
2. ✅ Install it correctly
3. ✅ Verify it's available for the build step

---

**The build should now work!** 🚀

If you still get errors, check the "Install Unity" step logs for specific error messages.

