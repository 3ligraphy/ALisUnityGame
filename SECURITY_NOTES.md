# Security Notes for codemagic.yaml

## ⚠️ Important Security Warning

You've chosen to include your Unity credentials directly in `codemagic.yaml`. This means:

1. **Your password will be visible in your Git repository**
2. **Anyone with access to your repository can see your credentials**
3. **If you push to a public repository, your password will be public**

## Your Options

### Option 1: Keep credentials in YAML (Current Choice)

**Pros:**
- ✅ Simple - everything in one file
- ✅ Easy to see what's configured

**Cons:**
- ❌ Password visible in Git history
- ❌ Security risk if repository is shared
- ❌ If repository is public, password is public

**What to do:**
1. Replace the placeholder values in `codemagic.yaml`:
   ```yaml
   UNITY_EMAIL: "your-email@gmail.com"  # Replace with your Gmail
   UNITY_PASSWORD: "your-password"      # Replace with your password
   ```

2. **If using a private repository:**
   - Only share with trusted team members
   - Consider using environment variables for better security

3. **If using a public repository:**
   - ⚠️ **DO NOT** include credentials in the file
   - Use environment variables instead (see Option 2)

### Option 2: Use Environment Variables (Recommended)

**Pros:**
- ✅ Secure - passwords never in Git
- ✅ Can be shared safely
- ✅ Works with public repositories
- ✅ Can be different per team member

**Cons:**
- ❌ Need to set up in CodeMagic dashboard
- ❌ Slightly more setup

**How to switch:**
1. In `codemagic.yaml`, change:
   ```yaml
   UNITY_EMAIL: "your-email@gmail.com"
   ```
   To:
   ```yaml
   UNITY_EMAIL: ${UNITY_EMAIL}
   ```

2. Do the same for `UNITY_PASSWORD`

3. Set environment variables in CodeMagic dashboard

### Option 3: Add to .gitignore (NOT Recommended)

**Why NOT recommended:**
- CodeMagic reads `codemagic.yaml` from your Git repository
- If it's in `.gitignore`, CodeMagic won't be able to use it
- You'd need to upload it manually each time

**If you still want to do this:**
1. Add to `.gitignore`:
   ```
   codemagic.yaml
   ```

2. Create `codemagic.yaml.example` with placeholder values:
   ```yaml
   UNITY_EMAIL: "your-email@gmail.com"
   UNITY_PASSWORD: "your-password"
   ```

3. Upload `codemagic.yaml` directly to CodeMagic (not recommended)

## Best Practices

### If You Must Use Direct Credentials:

1. **Use a Private Repository**
   - Only share with trusted team members
   - Use GitHub/GitLab private repos

2. **Change Password Regularly**
   - If credentials are exposed, change your Unity password
   - Update the file with new password

3. **Use a Separate Unity Account**
   - Create a Unity account just for CI/CD
   - Don't use your personal Unity account

4. **Monitor Repository Access**
   - Regularly review who has access
   - Remove access when team members leave

5. **Consider Using Environment Variables**
   - Even for private repos, environment variables are safer
   - Easy to rotate credentials without changing code

## What to Do Right Now

1. **Open `codemagic.yaml`**
2. **Find these lines:**
   ```yaml
   UNITY_EMAIL: "your-email@gmail.com"
   UNITY_PASSWORD: "your-password"
   ```
3. **Replace with your actual values:**
   ```yaml
   UNITY_EMAIL: "yourname@gmail.com"      # Your actual Gmail
   UNITY_PASSWORD: "YourActualPassword"   # Your actual password
   ```
4. **Save the file**
5. **Commit and push** (only if repository is private!)

## If Your Credentials Are Exposed

If you've already pushed credentials to a public repository:

1. **Change your Unity password immediately:**
   - Go to: https://id.unity.com/
   - Change password

2. **Remove credentials from Git history:**
   ```bash
   # Remove file from Git history (advanced)
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch codemagic.yaml" \
     --prune-empty --tag-name-filter cat -- --all
   ```

3. **Switch to environment variables:**
   - Update `codemagic.yaml` to use `${UNITY_EMAIL}` and `${UNITY_PASSWORD}`
   - Set values in CodeMagic dashboard

## Recommendation

**For most users, I recommend using environment variables (Option 2)** because:
- More secure
- Works with any repository type
- Easy to manage
- Can be rotated without code changes

But if you're using a **private repository** and prefer simplicity, direct values in YAML are acceptable.

---

**Remember:** Security is important! Choose the option that best fits your needs and security requirements.

