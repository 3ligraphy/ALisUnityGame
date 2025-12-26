# Unity 6 Installation Guide for CodeMagic

## Problem
Unity 6 (6000.x) is **not supported** via CodeMagic's automatic `unity:` environment configuration. CodeMagic only supports Unity versions up to 2023.x automatically.

## Solution Options

### Option 1: Manual Installation (Current Implementation)
The `codemagic.yaml` now includes a manual installation script that:
1. Downloads Unity 6 directly from Unity's servers
2. Installs it to the standard location
3. Verifies the installation

**Status**: This may fail if Unity 6 download URLs are different or restricted.

### Option 2: Use Supported Unity Version (Recommended)
**Best solution**: Downgrade to a supported Unity version:

1. **Check CodeMagic supported versions**: 
   - Visit: https://docs.codemagic.io/knowledge-others/install-unity-version/
   - Latest supported versions are typically 2022.3.x or 2023.x

2. **Update your Unity project**:
   - Open in Unity Hub
   - Install a supported version (e.g., 2022.3.20f1)
   - Open your project with that version
   - Test that everything works
   - Update `UNITY_VERSION` in `codemagic.yaml`

3. **Update codemagic.yaml**:
   ```yaml
   environment:
     unity: 2022.3.20f1  # Use supported version
   ```

### Option 3: Contact CodeMagic Support
- Request Unity 6 support
- They may add it in the future
- Or provide alternative installation methods

### Option 4: Use Unity Cloud Build (Alternative)
- Unity's own cloud build service
- Supports Unity 6 natively
- Alternative to CodeMagic

## Current Configuration

The `codemagic.yaml` file now:
- ✅ Removed `unity: 6000.2.15f1` (not supported)
- ✅ Added manual installation script
- ✅ Uses `UNITY_VERSION` environment variable

## If Manual Installation Fails

If the download fails, you'll see:
```
❌ Failed to download Unity 6000.2.15f1
```

**Recommended action**: Switch to Option 2 (use supported Unity version)

## How to Switch to Supported Version

1. **In Unity**:
   - Install Unity 2022.3.20f1 (or latest 2022.3.x)
   - Open your project
   - Fix any compatibility issues
   - Test the build

2. **In codemagic.yaml**:
   ```yaml
   environment:
     unity: 2022.3.20f1  # Supported version
   vars:
     # Remove UNITY_VERSION - not needed with automatic installation
   ```

3. **Remove manual installation script**:
   - Delete the "Install Unity 6" step
   - CodeMagic will install Unity automatically

## Unity Version Compatibility

- **Unity 6 (6000.x)**: Not supported by CodeMagic automatic installation
- **Unity 2023.x**: Likely supported (check CodeMagic docs)
- **Unity 2022.3.x**: Fully supported
- **Unity 2021.3.x**: Fully supported

## Recommendation

**For fastest deployment**: Use Unity 2022.3.20f1 or 2023.3.x (if supported)

1. It's fully supported by CodeMagic
2. Automatic installation works
3. No manual scripts needed
4. More reliable builds

---

**Next Steps**: Try the current manual installation first. If it fails, switch to a supported Unity version.

