# Step-by-Step Quick Guide - Get All Required Data

Follow these steps in order to collect all the information you need.

## 🎯 Step 1: Get Unity Information (5 minutes)

### A. Find Your Unity Account Email & Password
1. Go to: https://id.unity.com/
2. If you don't remember your password, click **Forgot Password**
3. **Write down:**
   - ✅ Your Unity email: `_________________`
   - ✅ Your Unity password: `_________________`

### B. Check Your Unity License Type

**Option 1: Check in Unity Hub**
1. Open Unity Hub
2. Go to **Installs** tab
3. Look at your Unity version - does it say "Personal" or "Plus/Pro"?

**Option 2: Check Online**
1. Go to: https://id.unity.com/en/subscriptions
2. Log in
3. Do you see an active subscription?
   - **YES** = You have Plus/Pro → Continue to Step C
   - **NO** = You have Personal → Skip to Step 2

### C. Get Unity Serial Number (Only if Plus/Pro)
1. On the same page (https://id.unity.com/en/subscriptions)
2. Click on your active subscription
3. Find the **Serial Number**
4. **Write down:**
   - ✅ Unity Serial Number: `_________________`

**If Personal License:** Leave `UNITY_SERIAL` empty in CodeMagic!

---

## 🍎 Step 2: Set Up Apple Developer Account (30-60 minutes)

### A. Create Apple Developer Account
1. Go to: https://developer.apple.com/programs/
2. Click **Enroll** or **Start Your Enrollment**
3. Sign in with your Apple ID (or create one at appleid.apple.com)
4. Pay $99/year fee
5. **Wait for approval** (can take 24-48 hours)
6. **Write down:**
   - ✅ Apple ID email: `_________________`

### B. Get Your Team ID (After Approval)
1. Go to: https://developer.apple.com/account
2. Log in
3. Look at the **top right corner** - you'll see your Team ID
4. It's a 10-character code like: `ABC123DEF4`
5. **Write down:**
   - ✅ Apple Team ID: `_________________`

### C. Create App ID (Bundle Identifier)
1. Go to: https://developer.apple.com/account/resources/identifiers/list
2. Click the **+** button (top left)
3. Select **App IDs** → **Continue**
4. Select **App** → **Continue**
5. Fill in:
   - **Description**: `Adventure Game` (or your app name)
   - **Bundle ID**: Select **Explicit**
   - **Bundle ID**: Enter `com.failaka.games.adventure`
6. Click **Continue** → **Register**
7. ✅ **Done!** Your App ID is created

---

## 🔐 Step 3: Set Up Code Signing (Choose ONE method)

### Method A: Automatic Code Signing (EASIEST - Recommended!)

**You'll do this in CodeMagic later, but here's what you need:**

1. **Create App-Specific Password:**
   - Go to: https://appleid.apple.com/account/manage
   - Sign in
   - Go to **Security** section
   - Under **App-Specific Passwords**, click **Generate Password**
   - Name it: `CodeMagic`
   - **Copy the password** (you'll only see it once!)
   - **Write down:**
     - ✅ App-Specific Password: `_________________`

**That's it!** CodeMagic will handle certificates automatically.

### Method B: Manual Code Signing (If you have a Mac)

**Only do this if you prefer manual control or don't want to use automatic signing.**

1. **Create Certificate Signing Request (CSR):**
   - On Mac: Open **Keychain Access**
   - **Keychain Access** > **Certificate Assistant** > **Request a Certificate From a Certificate Authority**
   - Enter your email and name
   - Select **Saved to disk**
   - Save the `.certSigningRequest` file

2. **Create Distribution Certificate:**
   - Go to: https://developer.apple.com/account/resources/certificates/list
   - Click **+** button
   - Select **Apple Distribution** → **Continue**
   - Upload your `.certSigningRequest` file
   - Download the `.cer` file
   - Double-click to install in Keychain

3. **Export Certificate as .p12:**
   - In Keychain Access, find **Apple Distribution** certificate
   - Right-click > **Export**
   - Save as `.p12` format
   - Set a password
   - **Write down:**
     - ✅ Certificate password: `_________________`

4. **Create Provisioning Profile:**
   - Go to: https://developer.apple.com/account/resources/profiles/list
   - Click **+** button
   - Select **App Store** (under Distribution) → **Continue**
   - Select App ID: `com.failaka.games.adventure` → **Continue**
   - Select your **Apple Distribution** certificate → **Continue**
   - Name it: `Adventure Game Distribution`
   - Click **Generate** → **Download**
   - Save the `.mobileprovision` file

---

## 📱 Step 4: Create App in App Store Connect (10 minutes)

1. Go to: https://appstoreconnect.apple.com
2. Sign in with your Apple ID
3. Click **My Apps** → **+** button → **New App**
4. Fill in:
   - **Platform**: iOS
   - **Name**: `Adventure Game` (or your app name)
   - **Primary Language**: Your language
   - **Bundle ID**: Select `com.failaka.games.adventure`
   - **SKU**: `adventure-game-001` (unique identifier)
   - **User Access**: Full Access
5. Click **Create**
6. ✅ **Done!** Your app is created

---

## ⚙️ Step 5: Configure CodeMagic (15 minutes)

### A. Sign Up for CodeMagic
1. Go to: https://codemagic.io/signup
2. Sign up with GitHub/GitLab/Bitbucket (or email)
3. Click **Add application**
4. Connect your repository
5. Select your Unity project repository

### B. Add Environment Variables
1. In CodeMagic, go to your app
2. Click **Settings** → **Environment variables**
3. Click **+ Add variable** for each:

   **Variable 1:**
   - Name: `UNITY_EMAIL`
   - Value: (your Unity email from Step 1A)
   - ✅ Check **Secure**

   **Variable 2:**
   - Name: `UNITY_PASSWORD`
   - Value: (your Unity password from Step 1A)
   - ✅ Check **Secure**

   **Variable 3:**
   - Name: `UNITY_SERIAL`
   - Value: (your serial from Step 1C, or leave empty for Personal)
   - ✅ Check **Secure**
   - **Note:** If Personal license, leave this empty!

   **Variable 4:**
   - Name: `BUNDLE_ID`
   - Value: `com.failaka.games.adventure`
   - ❌ Don't check Secure (it's not sensitive)

### C. Configure Code Signing

**If using Automatic (Recommended):**
1. Go to **Code signing** tab
2. Enable **Automatic code signing**
3. Enter your Apple ID (from Step 2A)
4. Enter your App-Specific Password (from Step 3A)
5. Select your Team
6. ✅ **Done!**

**If using Manual:**
1. Go to **Code signing** tab
2. Under **iOS code signing**:
   - Upload your `.p12` certificate file
   - Enter certificate password
   - Upload your `.mobileprovision` file
3. ✅ **Done!**

### D. Update Configuration Files

**1. Update codemagic.yaml:**
   - Open `codemagic.yaml` in your project
   - The bundle ID is already set: `com.failaka.games.adventure`
   - Update email in publishing section (line ~87):
     ```yaml
     recipients:
       - your-email@example.com  # Change this!
     ```

**2. Update exportOptions.plist:**
   - Open `exportOptions.plist`
   - Replace `YOUR_TEAM_ID` with your Team ID (from Step 2B)
   - Replace `YOUR_BUNDLE_ID` with `com.failaka.games.adventure`
   - Replace `YOUR_PROVISIONING_PROFILE_NAME` with your profile name (if manual signing)

---

## 🎮 Step 6: Configure Unity Project (5 minutes)

1. **Open Unity:**
   - Open your project in Unity

2. **Set Bundle Identifier:**
   - **Edit** > **Project Settings** > **Player**
   - Under **Other Settings**:
     - **Bundle Identifier**: `com.failaka.games.adventure`
     - **Version**: `1.0.0`
     - **Build Number**: `1`

3. **Configure iOS Settings:**
   - Click **iOS** tab (under Platform settings)
   - **Target minimum iOS Version**: `13.0`
   - **Architecture**: `ARM64`

4. **Add App Icons:**
   - Go to **iOS** > **Icons**
   - **IMPORTANT:** Add at least the **App Store icon (1024x1024)**
   - Add other icon sizes if you have them

5. **Save:**
   - Unity saves automatically

---

## 🚀 Step 7: First Build (20-30 minutes)

1. **Commit and Push:**
   ```bash
   git add codemagic.yaml exportOptions.plist
   git commit -m "Add CodeMagic iOS configuration"
   git push
   ```

2. **Start Build in CodeMagic:**
   - Go to CodeMagic dashboard
   - Click **Start new build**
   - Select **ios-workflow**
   - Select your branch
   - Click **Start new build**

3. **Wait:**
   - Build takes 15-30 minutes
   - You'll get an email when done

4. **Download IPA:**
   - When build succeeds, download the `.ipa` file

---

## 📤 Step 8: Upload to TestFlight (10 minutes)

1. **Download Transporter App (Mac only):**
   - From Mac App Store: https://apps.apple.com/app/transporter/id1450874784
   - Or use Xcode Organizer

2. **Upload IPA:**
   - Open Transporter
   - Sign in with your Apple ID
   - Drag and drop your `.ipa` file
   - Click **Deliver**

3. **Wait for Processing:**
   - Go to App Store Connect → **TestFlight** tab
   - Wait 10-30 minutes for processing

4. **Add Testers:**
   - Click **Internal Testing**
   - Add yourself as a tester
   - Test on your iPhone/iPad

---

## ✅ Step 9: Submit to App Store (30 minutes)

1. **Complete App Information:**
   - In App Store Connect, go to your app
   - Click **App Store** tab
   - Fill in:
     - Description
     - Keywords
     - Screenshots (required!)
     - Support URL
     - Privacy Policy URL (required!)

2. **Create Version:**
   - Click **+ Version or Platform**
   - Enter version: `1.0.0`
   - Select your build from TestFlight

3. **Answer Questions:**
   - Export Compliance
   - Age Rating
   - App Review Information

4. **Submit:**
   - Click **Submit for Review**
   - Wait 24-48 hours for review

---

## 📝 Quick Reference: All Your Data

Fill this out as you go:

```
UNITY INFORMATION:
  Email: _________________
  Password: _________________
  Serial Number: _________________ (or leave empty for Personal)

APPLE DEVELOPER:
  Apple ID: _________________
  Team ID: _________________
  App-Specific Password: _________________ (for automatic signing)

BUNDLE ID:
  com.failaka.games.adventure (already set!)

CODE SIGNING:
  Method: [ ] Automatic  [ ] Manual
  Certificate: _________________ (if manual)
  Provisioning Profile: _________________ (if manual)
```

---

## 🆘 Need Help?

- **Unity Issues**: https://forum.unity.com/
- **Apple Developer**: https://developer.apple.com/support/
- **CodeMagic Docs**: https://docs.codemagic.io/getting-started/unity/
- **Detailed Guide**: See `DETAILED_SETUP_INSTRUCTIONS.md`

---

**Estimated Total Time:**
- Setup: 2-3 hours
- First Build: 30 minutes
- TestFlight Testing: 1-2 days
- App Store Review: 1-2 days

**Total Cost:**
- Apple Developer: $99/year
- CodeMagic: Free tier available
- Unity: Free (Personal) or paid license

Good luck! 🎉


