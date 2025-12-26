# CodeMagic Setup Steps - Unity Personal License with Gmail

This guide will help you set up CodeMagic with your Unity Personal (free) license and Gmail account.

## ✅ What You Need Before Starting

- [ ] Unity account (logged in with Gmail)
- [ ] Your Unity account password
- [ ] CodeMagic account (sign up at https://codemagic.io)
- [ ] Your Git repository connected to CodeMagic

---

## Step 1: Sign Up for CodeMagic

1. Go to: https://codemagic.io/signup
2. Sign up with:
   - **GitHub** (recommended if your project is on GitHub)
   - **GitLab** (if your project is on GitLab)
   - **Bitbucket** (if your project is on Bitbucket)
   - Or use **Email** signup

3. Complete the signup process

---

## Step 2: Connect Your Repository

1. In CodeMagic dashboard, click **Add application**
2. Select your Git provider (GitHub/GitLab/Bitbucket)
3. Authorize CodeMagic to access your repositories
4. Find and select your Unity project repository
5. Click **Add application**

---

## Step 3: Add Environment Variables

**This is the most important step!** You need to add your Unity credentials.

### A. Go to Environment Variables

1. In CodeMagic, click on your app
2. Click **Settings** (gear icon on the left)
3. Click **Environment variables** (under Configuration)

### B. Add UNITY_EMAIL

1. Click **+ Add variable** button
2. Fill in:
   - **Variable name**: `UNITY_EMAIL`
   - **Variable value**: Your Gmail address (the one you use for Unity)
     - Example: `yourname@gmail.com`
   - **Secure**: ✅ **Check this box** (important for security!)
3. Click **Add**

### C. Add UNITY_PASSWORD

1. Click **+ Add variable** button again
2. Fill in:
   - **Variable name**: `UNITY_PASSWORD`
   - **Variable value**: Your Unity account password
   - **Secure**: ✅ **Check this box** (very important!)
3. Click **Add**

### D. Verify Variables

You should now see:
- ✅ `UNITY_EMAIL` (with a lock icon - means it's secure)
- ✅ `UNITY_PASSWORD` (with a lock icon - means it's secure)

**Note:** The `BUNDLE_ID` is already set in `codemagic.yaml` as `com.failaka.games.adventure`, so you don't need to add it as an environment variable unless you want to override it.

---

## Step 4: Configure Code Signing

You have two options:

### Option A: Automatic Code Signing (EASIEST - Recommended!)

1. In CodeMagic, go to your app
2. Click **Code signing** tab
3. Under **iOS code signing**, enable **Automatic code signing**
4. Enter:
   - **Apple ID**: Your Apple ID email (same as Developer account)
   - **App-specific password**: 
     - Go to: https://appleid.apple.com/account/manage
     - Sign in
     - Go to **Security** section
     - Under **App-Specific Passwords**, click **Generate Password**
     - Name it: `CodeMagic`
     - **Copy the password** (you'll only see it once!)
     - Paste it into CodeMagic
5. Select your **Team** from the dropdown
6. Click **Save**

CodeMagic will automatically manage certificates and provisioning profiles for you!

### Option B: Manual Code Signing

If you prefer to upload certificates manually:

1. In **Code signing** tab
2. Under **iOS code signing**:
   - Upload your **Distribution Certificate** (.p12 file)
   - Enter certificate password
   - Upload your **Provisioning Profile** (.mobileprovision file)
3. Click **Save**

---

## Step 5: Update Email in codemagic.yaml

1. Open `codemagic.yaml` in your project
2. Find this section (near the end):
   ```yaml
   email:
     recipients:
       - user@example.com  # ⚠️ CHANGE THIS to your email address!
   ```
3. Replace `user@example.com` with your actual email
4. Save the file

---

## Step 6: Commit and Push

1. Make sure `codemagic.yaml` is in your project root
2. Commit the file:
   ```bash
   git add codemagic.yaml
   git commit -m "Add CodeMagic iOS build configuration"
   git push
   ```

---

## Step 7: Start Your First Build

1. In CodeMagic dashboard, go to your app
2. Click **Start new build** button
3. Select:
   - **Workflow**: `ios-workflow`
   - **Branch**: Your main branch (usually `main` or `master`)
4. Click **Start new build**

---

## Step 8: Monitor the Build

1. Watch the build progress in real-time
2. You'll see each step:
   - ✅ Verify Unity Installation
   - ✅ Build Unity Project for iOS
   - ✅ Configure Xcode Project
   - ✅ Install CocoaPods Dependencies
   - ✅ Build Xcode Archive
   - ✅ Export IPA

3. **Build time**: Usually 15-30 minutes

4. **You'll get an email** when the build completes (success or failure)

---

## Step 9: Download Your IPA

1. When build succeeds, go to **Builds** tab
2. Click on your successful build
3. Scroll down to **Artifacts**
4. Download the `.ipa` file

---

## Troubleshooting

### Build Fails: "Unity license not found"

**Solution:**
- Double-check `UNITY_EMAIL` and `UNITY_PASSWORD` are correct
- Make sure they're marked as "Secure" in CodeMagic
- Verify your Unity account is active at https://id.unity.com/

### Build Fails: "Code signing error"

**Solution:**
- Enable **Automatic code signing** in CodeMagic (easiest)
- Or upload certificates manually
- Make sure your Apple Developer account is active

### Build Succeeds but No IPA

**Solution:**
- Check the "Export IPA" step in build logs
- Verify code signing completed
- Check that `exportOptions.plist` exists in your project

### Unity Build Takes Too Long

**Solution:**
- This is normal! Unity builds can take 15-30 minutes
- Large projects may take longer
- Be patient and let it complete

---

## Important Notes

1. **Unity Personal License**: 
   - ✅ No serial number needed
   - ✅ Free to use
   - ✅ Works perfectly with CodeMagic

2. **Gmail Account**:
   - Use the exact Gmail address you use for Unity
   - Make sure you remember your Unity password
   - If you forgot, reset at https://id.unity.com/

3. **Security**:
   - Always mark `UNITY_EMAIL` and `UNITY_PASSWORD` as "Secure" in CodeMagic
   - Never commit passwords to Git
   - Use environment variables only

4. **Code Signing**:
   - Automatic code signing is recommended
   - CodeMagic handles everything automatically
   - You just need your Apple ID and app-specific password

---

## Next Steps After First Successful Build

1. ✅ Upload `.ipa` to TestFlight
2. ✅ Test on your iOS device
3. ✅ Submit to App Store Connect
4. ✅ Set up automatic builds on git tags (optional)

---

## Quick Checklist

Before your first build, make sure:

- [ ] CodeMagic account created
- [ ] Repository connected
- [ ] `UNITY_EMAIL` environment variable added (marked as Secure)
- [ ] `UNITY_PASSWORD` environment variable added (marked as Secure)
- [ ] Code signing configured (automatic or manual)
- [ ] Email updated in `codemagic.yaml`
- [ ] `codemagic.yaml` committed and pushed to repository
- [ ] Apple Developer account ready ($99/year)

---

## Support

- **CodeMagic Docs**: https://docs.codemagic.io/getting-started/unity/
- **CodeMagic Support**: support@codemagic.io
- **Unity Support**: https://support.unity.com/

---

**You're all set!** 🚀

Start with Step 1 and work through each step. Your first build should be ready in about 30 minutes!

