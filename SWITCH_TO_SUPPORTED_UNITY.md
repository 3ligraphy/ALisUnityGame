# Switching to Supported Unity Version

## Problem
Unity 6 (6000.2.15f1) is **not available** for download and not supported by CodeMagic's automatic installation.

## Solution
I've updated your `codemagic.yaml` to use **Unity 2022.3.20f1**, which is fully supported by CodeMagic.

## What Changed

### ✅ Updated Configuration
- Changed from Unity 6 (6000.2.15f1) → Unity 2022.3.20f1
- Removed manual installation script
- Using CodeMagic's automatic Unity installation
- Simplified build process

### ⚠️ Important: Update Your Unity Project

**You need to update your Unity project to use Unity 2022.3.20f1:**

1. **Install Unity 2022.3.20f1**:
   - Open Unity Hub
   - Go to **Installs** tab
   - Click **Install Editor**
   - Select **2022.3.20f1** (or latest 2022.3.x)
   - Install it

2. **Open Your Project with Unity 2022.3.20f1**:
   - In Unity Hub, go to **Projects** tab
   - Click **Add** and select your project folder
   - When opening, select **Unity 2022.3.20f1**
   - Unity will upgrade your project (it may take a few minutes)

3. **Fix Any Compatibility Issues**:
   - Check the Console for any errors
   - Some Unity 6 features might not be available in 2022.3
   - Update any Unity 6-specific code/features

4. **Test Your Project**:
   - Make sure everything works
   - Test the build locally if possible
   - Fix any issues

5. **Update Bundle ID** (if needed):
   - **Edit** > **Project Settings** > **Player**
   - Set **Bundle Identifier**: `com.failaka.games.adventure`
   - Set **Version**: `1.0.0`
   - Configure iOS settings

## Alternative Unity Versions

If 2022.3.20f1 doesn't work for you, try:

- **2022.3.21f1** (newer patch)
- **2023.2.x** (if supported by CodeMagic)
- **2021.3.x** (older but stable)

Check CodeMagic docs for latest supported versions:
https://docs.codemagic.io/knowledge-others/install-unity-version/

## If You Must Use Unity 6

If you absolutely need Unity 6, your options are:

1. **Unity Cloud Build** (Unity's own service)
   - Supports Unity 6 natively
   - Alternative to CodeMagic
   - https://build.cloud.unity3d.com/

2. **Wait for CodeMagic Support**
   - Contact CodeMagic support
   - Request Unity 6 support
   - May be added in future

3. **Manual Upload** (Complex)
   - Download Unity 6 manually
   - Upload to CodeMagic storage
   - Very complex setup

## Current Configuration

Your `codemagic.yaml` now uses:
```yaml
environment:
  unity: 2022.3.20f1  # Supported version
```

CodeMagic will automatically:
- ✅ Install Unity 2022.3.20f1
- ✅ No manual scripts needed
- ✅ Reliable and fast

## Next Steps

1. **Update Unity Project** (see above)
2. **Commit and Push**:
   ```bash
   git add codemagic.yaml
   git commit -m "Switch to Unity 2022.3.20f1 for CodeMagic compatibility"
   git push
   ```
3. **Start Build in CodeMagic**
   - Should work now!
   - Unity will install automatically
   - Build should succeed

## Testing

After updating:
1. Test your project in Unity 2022.3.20f1
2. Make sure iOS build works
3. Fix any compatibility issues
4. Then push to CodeMagic

---

**The build should now work!** 🚀

If you encounter any issues with Unity 2022.3.20f1, let me know and we can try a different supported version.

