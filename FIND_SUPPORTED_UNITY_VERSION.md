# Finding Supported Unity Version for CodeMagic

## Problem
CodeMagic has a limited list of supported Unity versions. Even 2022.3.62f2 might not be available.

## Solution: Check CodeMagic Documentation

**Visit this page to see the exact list:**
https://docs.codemagic.io/knowledge-others/install-unity-version/

## Common Supported Versions

Based on CodeMagic's typical support, try these in order:

1. **2022.3.62f2** (latest 2022.3 LTS) - Currently set in codemagic.yaml
2. **2022.3.20f1** (earlier 2022.3 LTS)
3. **2021.3.47f1** (2021.3 LTS)
4. **2020.3.48f1** (2020.3 LTS)

## How to Update

1. **Check CodeMagic docs** for the exact supported version
2. **Update codemagic.yaml**:
   ```yaml
   environment:
     unity: 2021.3.47f1  # Replace with supported version
   ```
3. **Update your Unity project** to match that version
4. **Try building again**

## Alternative: Use Unity Hub Installation

If automatic installation doesn't work, CodeMagic docs mention installing via Unity Hub. This is more complex but might work for unsupported versions.

## Quick Fix

Try changing to 2021.3.47f1 (very common LTS version):

```yaml
environment:
  unity: 2021.3.47f1
```

Then update your Unity project to 2021.3.47f1.

---

**Next Step**: Check the CodeMagic documentation link above for the exact supported versions list.

