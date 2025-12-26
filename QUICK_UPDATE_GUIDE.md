# Quick Update: Add Credentials to codemagic.yaml

## ✅ Simple Steps

1. **Open `codemagic.yaml`** in your editor

2. **Find these lines** (around line 35-36):
   ```yaml
   UNITY_EMAIL: "your-email@gmail.com"  # ⚠️ REPLACE with your Gmail address
   UNITY_PASSWORD: "your-password"      # ⚠️ REPLACE with your Unity password
   ```

3. **Replace with your actual values:**
   ```yaml
   UNITY_EMAIL: "yourname@gmail.com"      # Your actual Gmail address
   UNITY_PASSWORD: "YourActualPassword"   # Your actual Unity password
   ```

4. **Save the file**

5. **That's it!** The file is ready to use.

## ⚠️ Important Security Note

- Your password will be in your Git repository
- Only commit if using a **private repository**
- If using a **public repository**, use environment variables instead (see `CODEMAGIC_SETUP_STEPS.md`)

## Example

**Before:**
```yaml
UNITY_EMAIL: "your-email@gmail.com"
UNITY_PASSWORD: "your-password"
```

**After (with your actual values):**
```yaml
UNITY_EMAIL: "john.doe@gmail.com"
UNITY_PASSWORD: "MySecurePassword123"
```

## Next Steps

1. Update the values in `codemagic.yaml`
2. Update email on line 196: `user@example.com` → your email
3. Commit and push (if private repo)
4. Start build in CodeMagic!

---

**Done!** Your `codemagic.yaml` is now configured with your credentials.

