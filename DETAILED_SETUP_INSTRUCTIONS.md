# Detailed Setup Instructions - Where to Get All Required Data

This guide will walk you through exactly where to find and how to obtain every piece of information needed for iOS deployment.

## 📋 Required Information Checklist

You need to collect the following information:

- [ ] Unity License Serial Number (or Personal License)
- [ ] Unity Account Email
- [ ] Unity Account Password
- [ ] Apple Developer Team ID
- [ ] Bundle Identifier (you already have: `com.failaka.games.adventure`)
- [ ] Apple Distribution Certificate
- [ ] Provisioning Profile
- [ ] App Store Connect API Keys (optional, for automation)

---

## Part 1: Unity License Information

### Option A: Unity Personal License (Free)

**If you're using Unity Personal (free version):**

1. **Unity Email & Password:**
   - This is the email and password you used to create your Unity account
   - If you don't remember, go to: https://id.unity.com/
   - Click "Forgot Password" if needed

2. **Unity Serial Number:**
   - **For Personal License: Leave this EMPTY or don't set it**
   - Personal licenses don't use serial numbers
   - In `codemagic.yaml`, you can leave `UNITY_SERIAL` empty or comment it out

**How to verify you have Personal License:**
- Open Unity Hub
- Go to **Installs** tab
- Click on your Unity version
- Look for "Personal" license type

### Option B: Unity Plus/Pro License (Paid)

**If you have Unity Plus or Pro:**

1. **Get Unity Serial Number:**
   - Log in to Unity: https://id.unity.com/
   - Go to **My Account** > **Licenses**
   - Find your active license
   - Copy the **Serial Number** (format: usually starts with letters/numbers)
   - This is what goes in `UNITY_SERIAL`

2. **Unity Email & Password:**
   - Same as above - your Unity account credentials

**How to find Serial Number:**
1. Visit: https://id.unity.com/en/subscriptions
2. Log in with your Unity account
3. Click on your active subscription
4. The serial number will be displayed there

---

## Part 2: Apple Developer Account Setup

### Step 1: Create Apple Developer Account

1. **Go to Apple Developer Portal:**
   - Visit: https://developer.apple.com/programs/
   - Click **Enroll** or **Start Your Enrollment**

2. **Sign Up Process:**
   - You need an Apple ID (create one at appleid.apple.com if you don't have one)
   - Pay $99/year fee
   - Complete enrollment (can take 24-48 hours for approval)

3. **Once Approved:**
   - Log in at: https://developer.apple.com/account

### Step 2: Get Your Team ID

1. **Log in to Apple Developer:**
   - Go to: https://developer.apple.com/account
   - Sign in with your Apple ID

2. **Find Team ID:**
   - In the top right corner, you'll see your **Team ID**
   - It's a 10-character alphanumeric code (e.g., `ABC123DEF4`)
   - **Copy this** - you'll need it for `exportOptions.plist`

**Alternative way to find Team ID:**
- Go to: https://developer.apple.com/account/#/membership/
- Your Team ID is displayed at the top of the page

### Step 3: Create App ID (Bundle Identifier)

1. **Go to Certificates, Identifiers & Profiles:**
   - Visit: https://developer.apple.com/account/resources/identifiers/list
   - Click the **+** button (top left)

2. **Select App IDs:**
   - Choose **App IDs**
   - Click **Continue**

3. **Register New App ID:**
   - Select **App** (not App Clip or other)
   - Click **Continue**

4. **Enter App Information:**
   - **Description**: Your app name (e.g., "Adventure Game")
   - **Bundle ID**: Select **Explicit**
   - **Bundle ID**: Enter `com.failaka.games.adventure` (your bundle ID)
   - Click **Continue**

5. **Select Capabilities (if needed):**
   - Check any capabilities your app uses:
     - Push Notifications
     - In-App Purchase
     - Game Center
     - etc.
   - Click **Continue**

6. **Review and Register:**
   - Review your settings
   - Click **Register**
   - Your App ID is now created!

---

## Part 3: Create Code Signing Certificates

### Step 1: Create Distribution Certificate

1. **Go to Certificates:**
   - Visit: https://developer.apple.com/account/resources/certificates/list
   - Click the **+** button

2. **Select Certificate Type:**
   - Under **Software**, select **Apple Distribution**
   - Click **Continue**

3. **Upload Certificate Signing Request (CSR):**
   - **On Mac:**
     - Open **Keychain Access** (Applications > Utilities)
     - Go to **Keychain Access** > **Certificate Assistant** > **Request a Certificate From a Certificate Authority**
     - Enter your email and name
     - Select **Saved to disk**
     - Click **Continue**
     - Save the `.certSigningRequest` file
   - **On Windows (Alternative):**
     - You'll need to use a Mac or use CodeMagic's automatic signing (recommended)
     - Or use online CSR generators

4. **Upload CSR:**
   - Back in Apple Developer Portal
   - Click **Choose File**
   - Select your `.certSigningRequest` file
   - Click **Continue**

5. **Download Certificate:**
   - Click **Download**
   - Save the `.cer` file
   - Double-click to install in Keychain (on Mac)

6. **Export as .p12 (for CodeMagic):**
   - **On Mac:**
     - Open **Keychain Access**
     - Find your **Apple Distribution** certificate
     - Right-click > **Export**
     - Save as `.p12` format
     - Set a password (remember this!)
     - This `.p12` file is what you upload to CodeMagic

**Note:** If you don't have a Mac, CodeMagic can handle automatic code signing (see Part 5).

### Step 2: Create Provisioning Profile

1. **Go to Profiles:**
   - Visit: https://developer.apple.com/account/resources/profiles/list
   - Click the **+** button

2. **Select Profile Type:**
   - Under **Distribution**, select **App Store**
   - Click **Continue**

3. **Select App ID:**
   - Choose `com.failaka.games.adventure` (the one you created)
   - Click **Continue**

4. **Select Certificate:**
   - Select your **Apple Distribution** certificate
   - Click **Continue**

5. **Name Your Profile:**
   - Enter a name (e.g., "Adventure Game Distribution")
   - Click **Generate**

6. **Download Profile:**
   - Click **Download**
   - Save the `.mobileprovision` file
   - This is what you upload to CodeMagic

---

## Part 4: App Store Connect Setup

### Step 1: Create App in App Store Connect

1. **Log in to App Store Connect:**
   - Visit: https://appstoreconnect.apple.com
   - Sign in with your Apple ID (same as Developer account)

2. **Create New App:**
   - Click **My Apps**
   - Click the **+** button
   - Select **New App**

3. **Fill App Information:**
   - **Platform**: iOS
   - **Name**: Your app name (e.g., "Adventure Game")
   - **Primary Language**: Your app's primary language
   - **Bundle ID**: Select `com.failaka.games.adventure`
   - **SKU**: Unique identifier (e.g., `adventure-game-001`)
   - **User Access**: Full Access (recommended)

4. **Click Create**
   - Your app is now created in App Store Connect!

### Step 2: Get App Store Connect API Keys (Optional, for Automation)

**Only needed if you want automatic TestFlight uploads:**

1. **Go to Users and Access:**
   - In App Store Connect: https://appstoreconnect.apple.com/access/api
   - Click **Keys** tab

2. **Generate API Key:**
   - Click **Generate API Key** or **+** button
   - Enter a name (e.g., "CodeMagic Integration")
   - Select **App Manager** or **Admin** access
   - Click **Generate**

3. **Download Key:**
   - Click **Download API Key** (only available once!)
   - Save the `.p8` file securely
   - Note the **Key ID** and **Issuer ID**

4. **Add to CodeMagic:**
   - Key ID: Goes in `APP_STORE_CONNECT_KEY_ID`
   - Issuer ID: Goes in `APP_STORE_CONNECT_ISSUER_ID`
   - .p8 file: Upload to CodeMagic

---

## Part 5: CodeMagic Setup

### Step 1: Create CodeMagic Account

1. **Sign Up:**
   - Visit: https://codemagic.io/signup
   - Sign up with GitHub, GitLab, or Bitbucket
   - Or use email signup

2. **Connect Repository:**
   - Click **Add application**
   - Connect your Git provider
   - Select your repository
   - Choose **Unity** as project type

### Step 2: Configure Environment Variables

1. **Go to App Settings:**
   - In CodeMagic dashboard, select your app
   - Go to **Settings** > **Environment variables**

2. **Add Variables:**
   Click **+ Add variable** for each:

   **UNITY_EMAIL:**
   - Name: `UNITY_EMAIL`
   - Value: Your Unity account email
   - Secure: ✅ (check this box)

   **UNITY_PASSWORD:**
   - Name: `UNITY_PASSWORD`
   - Value: Your Unity account password
   - Secure: ✅ (check this box)

   **UNITY_SERIAL:**
   - Name: `UNITY_SERIAL`
   - Value: Your Unity serial number (or leave empty for Personal license)
   - Secure: ✅ (check this box)
   - **Note:** If using Personal license, you can leave this empty or set to empty string

   **BUNDLE_ID:**
   - Name: `BUNDLE_ID`
   - Value: `com.failaka.games.adventure`
   - Secure: ❌ (no need to secure this)

### Step 3: Configure Code Signing

**Option A: Automatic Code Signing (Recommended - Easier!)**

1. **In CodeMagic Dashboard:**
   - Go to your app
   - Click **Code signing** tab
   - Enable **Automatic code signing**

2. **Add Apple Developer Credentials:**
   - Enter your Apple ID (same as Developer account)
   - Enter your App-Specific Password:
     - Go to: https://appleid.apple.com/account/manage
     - Sign in
     - Go to **Security** section
     - Under **App-Specific Passwords**, click **Generate Password**
     - Name it "CodeMagic"
     - Copy the password (you'll only see it once!)
     - Paste into CodeMagic

3. **Select Team:**
   - Choose your Apple Developer Team
   - CodeMagic will automatically manage certificates and profiles!

**Option B: Manual Code Signing (If you have certificates)**

1. **Upload Certificate:**
   - Go to **Code signing** tab
   - Under **iOS code signing**, click **Upload certificate**
   - Upload your `.p12` file
   - Enter the password you set when exporting

2. **Upload Provisioning Profile:**
   - Click **Upload provisioning profile**
   - Upload your `.mobileprovision` file

### Step 4: Update codemagic.yaml

1. **Open codemagic.yaml in your project**

2. **Update these values:**
   ```yaml
   UNITY_SERIAL: # Leave empty if Personal license, or add your serial
   UNITY_EMAIL: # This will use the environment variable you set
   UNITY_PASSWORD: # This will use the environment variable you set
   BUNDLE_ID: "com.failaka.games.adventure" # Already set!
   ```

3. **Update email notification:**
   ```yaml
   email:
     recipients:
       - your-email@example.com # Change this to your email
   ```

### Step 5: Update exportOptions.plist

1. **Open exportOptions.plist**

2. **Update Team ID:**
   ```xml
   <key>teamID</key>
   <string>YOUR_TEAM_ID</string>  <!-- Replace with your Team ID from Part 2, Step 2 -->
   ```

3. **Update Bundle ID:**
   ```xml
   <key>YOUR_BUNDLE_ID</key>
   <string>YOUR_PROVISIONING_PROFILE_NAME</string>
   ```
   Change to:
   ```xml
   <key>com.failaka.games.adventure</key>
   <string>Adventure Game Distribution</string>  <!-- Or your profile name -->
   ```

**Note:** If using automatic code signing in CodeMagic, you can use a simpler exportOptions.plist (CodeMagic will handle signing).

---

## Part 6: Unity Project Configuration

### Step 1: Set Bundle Identifier in Unity

1. **Open Unity:**
   - Open your project in Unity

2. **Go to Player Settings:**
   - **Edit** > **Project Settings** > **Player**

3. **Configure iOS Settings:**
   - Under **Other Settings**:
     - **Bundle Identifier**: `com.failaka.games.adventure`
     - **Version**: `1.0.0` (or your version)
     - **Build Number**: Start with `1`

4. **iOS Specific Settings:**
   - Click **iOS** tab (under Platform settings)
   - **Target minimum iOS Version**: `13.0` or higher
   - **Target Device**: iPhone + iPad (or as needed)
   - **Architecture**: ARM64 (required for App Store)

5. **Add App Icons:**
   - Go to **iOS** > **Icons**
   - Add all required icon sizes:
     - App Store: 1024x1024 (required!)
     - iPhone icons: 180x180, 120x120, etc.
     - iPad icons: 167x167, 152x152, etc.

6. **Save:**
   - Unity will save automatically

### Step 2: Configure Build Settings

1. **Go to Build Settings:**
   - **File** > **Build Settings**

2. **Select iOS:**
   - Choose **iOS** platform
   - Click **Switch Platform** (if needed)

3. **Add Scenes:**
   - Ensure all your scenes are in **Scenes In Build**
   - Your scenes should already be there based on EditorBuildSettings.asset

4. **Don't build yet** - CodeMagic will handle the build!

---

## Part 7: First Build and Test

### Step 1: Commit and Push

1. **Commit your files:**
   ```bash
   git add codemagic.yaml exportOptions.plist
   git commit -m "Add CodeMagic iOS build configuration"
   git push
   ```

### Step 2: Start Build in CodeMagic

1. **Go to CodeMagic Dashboard:**
   - Select your app
   - Click **Start new build**

2. **Select Workflow:**
   - Choose **ios-workflow**
   - Select your branch (usually `main` or `master`)

3. **Start Build:**
   - Click **Start new build**
   - Monitor the build progress

### Step 3: Download and Test

1. **Wait for Build:**
   - Build typically takes 15-30 minutes
   - You'll get an email when complete

2. **Download IPA:**
   - In CodeMagic, go to **Builds**
   - Click on your successful build
   - Download the `.ipa` file

3. **Test on Device:**
   - Upload to TestFlight (see Part 8)
   - Or install via Xcode (if you have a Mac)

---

## Part 8: Upload to TestFlight

### Step 1: Upload IPA

**Option A: Using Transporter App (macOS)**

1. **Download Transporter:**
   - From Mac App Store: https://apps.apple.com/app/transporter/id1450874784

2. **Open Transporter:**
   - Sign in with your Apple ID
   - Drag and drop your `.ipa` file
   - Click **Deliver**

**Option B: Using Xcode (macOS)**

1. **Open Xcode:**
   - **Window** > **Organizer**

2. **Upload:**
   - Click **+** button
   - Select your `.ipa` file
   - Follow the prompts

**Option C: Using CodeMagic (Automatic)**

1. **In codemagic.yaml, uncomment:**
   ```yaml
   app_store_connect:
     auth: integration
     submit_to_testflight: true
   ```

2. **Configure API Keys:**
   - Add API keys to CodeMagic environment variables
   - Builds will automatically upload to TestFlight!

### Step 2: Wait for Processing

1. **In App Store Connect:**
   - Go to **TestFlight** tab
   - Wait 10-30 minutes for processing

2. **Add Testers:**
   - **Internal Testing**: Up to 100 testers (immediate)
   - **External Testing**: Up to 10,000 (requires Beta App Review)

---

## Part 9: Submit to App Store

### Step 1: Complete App Information

1. **In App Store Connect:**
   - Go to your app
   - Click **App Store** tab

2. **Fill Required Information:**
   - **App Information**: Category, subtitle
   - **Pricing**: Set price (or Free)
   - **App Privacy**: Complete privacy questionnaire
   - **App Store Assets**:
     - Screenshots (required for all device sizes)
     - Description
     - Keywords
     - Support URL
     - Privacy Policy URL (required!)

### Step 2: Submit for Review

1. **Create Version:**
   - Click **+ Version or Platform**
   - Enter version number (e.g., 1.0.0)

2. **Select Build:**
   - Choose your uploaded build from TestFlight

3. **Complete Version Information:**
   - What's New in This Version
   - Screenshots
   - Description
   - Keywords

4. **Answer Questions:**
   - Export Compliance
   - Age Rating
   - App Review Information (demo account if needed)

5. **Submit:**
   - Click **Submit for Review**
   - Wait for review (typically 24-48 hours)

---

## Troubleshooting Common Issues

### Issue: "Unity license not found"
**Solution:**
- Verify `UNITY_EMAIL` and `UNITY_PASSWORD` are correct
- For Personal license, leave `UNITY_SERIAL` empty
- Check Unity account is active

### Issue: "Code signing failed"
**Solution:**
- Verify certificates are valid (not expired)
- Check Team ID matches
- Ensure provisioning profile matches bundle ID
- Try automatic code signing in CodeMagic

### Issue: "Invalid bundle identifier"
**Solution:**
- Ensure bundle ID in Unity matches App Store Connect
- Check App ID exists in Apple Developer Portal
- Verify bundle ID in `codemagic.yaml` matches

### Issue: "Build succeeded but no IPA"
**Solution:**
- Check export step in build logs
- Verify `exportOptions.plist` is correct
- Check code signing completed successfully

---

## Quick Reference: Where to Find Everything

| What You Need | Where to Find It |
|---------------|------------------|
| Unity Email/Password | https://id.unity.com/ |
| Unity Serial Number | https://id.unity.com/en/subscriptions (or leave empty for Personal) |
| Apple Developer Team ID | https://developer.apple.com/account (top right) |
| Create App ID | https://developer.apple.com/account/resources/identifiers/list |
| Create Certificate | https://developer.apple.com/account/resources/certificates/list |
| Create Provisioning Profile | https://developer.apple.com/account/resources/profiles/list |
| App Store Connect | https://appstoreconnect.apple.com |
| CodeMagic | https://codemagic.io |

---

## Summary Checklist

Before your first build, ensure you have:

- [ ] Unity account email and password
- [ ] Unity serial number (or confirmed Personal license)
- [ ] Apple Developer account ($99/year)
- [ ] Apple Developer Team ID
- [ ] App ID created with bundle ID: `com.failaka.games.adventure`
- [ ] Distribution certificate (or automatic signing enabled)
- [ ] Provisioning profile (or automatic signing enabled)
- [ ] App created in App Store Connect
- [ ] CodeMagic account and repository connected
- [ ] Environment variables set in CodeMagic
- [ ] Code signing configured in CodeMagic
- [ ] `codemagic.yaml` updated with bundle ID
- [ ] `exportOptions.plist` updated with Team ID
- [ ] Unity project configured with bundle ID and icons

---

**Need Help?**
- CodeMagic Docs: https://docs.codemagic.io/getting-started/unity/
- Apple Developer Support: https://developer.apple.com/support/
- Unity Forums: https://forum.unity.com/

Good luck with your deployment! 🚀


