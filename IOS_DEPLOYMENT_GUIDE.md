# iOS Deployment Guide for Unity Project using CodeMagic

This guide will walk you through deploying your Unity game to the iOS App Store using CodeMagic CI/CD.

## Prerequisites

Before you begin, ensure you have:

1. **Apple Developer Account** ($99/year)
   - Sign up at [developer.apple.com](https://developer.apple.com)
   - Enroll in the Apple Developer Program

2. **App Store Connect Access**
   - Create an app record in App Store Connect
   - Note your App ID and Bundle ID

3. **CodeMagic Account**
   - Sign up at [codemagic.io](https://codemagic.io)
   - Connect your repository (GitHub, GitLab, Bitbucket, etc.)

4. **Unity License**
   - Personal/Plus license serial number
   - Or Unity Pro license for cloud builds

## Step 1: Configure Your Unity Project for iOS

### 1.1 Set Bundle Identifier

1. Open your project in Unity
2. Go to **Edit > Project Settings > Player**
3. Under **Other Settings**, set:
   - **Bundle Identifier**: `com.yourcompany.plgy` (replace with your actual bundle ID)
   - **Version**: `1.0.0` (or your version)
   - **Build Number**: Start with `1` (increments automatically)

### 1.2 Configure iOS Settings

In **Player Settings > iOS**:

- **Target minimum iOS Version**: 13.0 (or higher)
- **Target Device**: iPhone + iPad (or as needed)
- **Architecture**: ARM64 (required for App Store)
- **Requires ARKit**: Set if your app uses AR
- **Camera Usage Description**: Add if your app uses camera
- **Location Usage Description**: Add if your app uses location

### 1.3 Configure Signing (Optional for CodeMagic)

You can configure signing in Unity or let CodeMagic handle it:
- **Automatic Signing**: Recommended for CodeMagic
- **Team ID**: Your Apple Developer Team ID
- **Provisioning Profile**: CodeMagic can manage this

### 1.4 Set App Icons

1. Go to **Player Settings > iOS > Icons**
2. Add all required icon sizes:
   - App Store: 1024x1024
   - iPhone: 180x180, 120x120, 87x87, 80x80, 60x60, 58x58, 40x40, 29x29, 20x20
   - iPad: 167x167, 152x152, 80x80, 76x76, 58x58, 40x40, 29x29, 20x20

### 1.5 Configure Build Settings

1. Go to **File > Build Settings**
2. Select **iOS** platform
3. Click **Switch Platform** (if needed)
4. Ensure all required scenes are added to **Scenes in Build**

## Step 2: Set Up CodeMagic

### 2.1 Connect Your Repository

1. Log in to [CodeMagic](https://codemagic.io)
2. Click **Add application**
3. Connect your Git provider (GitHub, GitLab, etc.)
4. Select your repository
5. Choose **Unity** as the project type

### 2.2 Configure Environment Variables

In CodeMagic dashboard, go to your app settings and add these environment variables:

**Required:**
- `UNITY_SERIAL`: Your Unity license serial number
- `UNITY_EMAIL`: Your Unity account email
- `UNITY_PASSWORD`: Your Unity account password
- `BUNDLE_ID`: Your app's bundle identifier (e.g., `com.yourcompany.plgy`)

**Optional:**
- `APP_STORE_ID`: Your App Store Connect app ID
- `APP_STORE_CONNECT_API_KEY`: For automated TestFlight uploads
- `APP_STORE_CONNECT_ISSUER_ID`: For automated TestFlight uploads
- `APP_STORE_CONNECT_KEY_ID`: For automated TestFlight uploads

### 2.3 Configure Code Signing

1. In CodeMagic, go to **App settings > Code signing**
2. Upload your **Apple Distribution Certificate** (.p12 file)
3. Upload your **Provisioning Profile** (.mobileprovision file)
4. Or enable **Automatic code signing** (recommended)

**To get certificates:**
1. Go to [Apple Developer Portal](https://developer.apple.com/account)
2. Navigate to **Certificates, Identifiers & Profiles**
3. Create an **Apple Distribution** certificate
4. Create an **App ID** with your bundle identifier
5. Create a **Distribution Provisioning Profile** for App Store

### 2.4 Update codemagic.yaml

1. Open `codemagic.yaml` in your project root
2. Update the following values:
   - `BUNDLE_ID`: Your actual bundle identifier
   - `UNITY_SERIAL`: Your Unity license serial
   - Email in publishing section
   - Team ID in exportOptions.plist (if using manual signing)

### 2.5 Update exportOptions.plist

1. Open `exportOptions.plist` in your project root
2. Replace:
   - `YOUR_TEAM_ID`: Your Apple Developer Team ID
   - `YOUR_BUNDLE_ID`: Your app's bundle identifier
   - `YOUR_PROVISIONING_PROFILE_NAME`: Your provisioning profile name

## Step 3: Create App in App Store Connect

1. Go to [App Store Connect](https://appstoreconnect.apple.com)
2. Click **My Apps > + (New App)**
3. Fill in:
   - **Platform**: iOS
   - **Name**: Your app name
   - **Primary Language**: Your app's primary language
   - **Bundle ID**: Select the one you created
   - **SKU**: Unique identifier (e.g., `plgy-001`)
   - **User Access**: Full Access (or Limited)
4. Click **Create**

## Step 4: Configure App Store Listing

In App Store Connect, prepare:

1. **App Information**:
   - Category
   - Subtitle
   - Privacy Policy URL (required)

2. **Pricing and Availability**:
   - Price tier
   - Availability countries

3. **App Privacy**:
   - Complete privacy questionnaire
   - List data collection practices

4. **App Store Assets**:
   - Screenshots (required for all device sizes)
   - App Preview videos (optional)
   - App Icon (1024x1024)
   - Description
   - Keywords
   - Support URL
   - Marketing URL (optional)

## Step 5: Build and Test

### 5.1 First Build

1. In CodeMagic, click **Start new build**
2. Select your workflow
3. Select branch (usually `main` or `master`)
4. Click **Start new build**

### 5.2 Monitor Build

- Watch build logs in real-time
- Fix any errors that occur
- Common issues:
  - Missing Unity modules
  - Code signing errors
  - Missing dependencies

### 5.3 Test on Device

1. Download the `.ipa` file from CodeMagic
2. Install on test device using:
   - TestFlight (recommended)
   - Xcode
   - Apple Configurator

## Step 6: Submit to App Store

### 6.1 Upload Build

**Option A: Using CodeMagic (Recommended)**
1. In `codemagic.yaml`, uncomment App Store Connect section
2. Configure API keys in CodeMagic
3. Builds will automatically upload to TestFlight

**Option B: Manual Upload**
1. Download `.ipa` from CodeMagic
2. Use **Transporter** app (macOS) or **Xcode Organizer**
3. Upload to App Store Connect

### 6.2 TestFlight Testing

1. In App Store Connect, go to **TestFlight** tab
2. Wait for processing (10-30 minutes)
3. Add internal testers (up to 100)
4. Add external testers (up to 10,000) - requires Beta App Review
5. Distribute build to testers

### 6.3 Submit for Review

1. In App Store Connect, go to your app version
2. Complete all required information:
   - Version information
   - What's New in This Version
   - Screenshots
   - Description
   - Keywords
   - Support URL
   - Privacy Policy URL
3. Answer **Export Compliance** questions
4. Set **Age Rating**
5. Add **App Review Information** (demo account if needed)
6. Click **Submit for Review**

## Step 7: Monitor Submission

1. Check **App Store Connect** for status updates
2. Common statuses:
   - **Waiting for Review**: In queue
   - **In Review**: Being reviewed
   - **Pending Developer Release**: Approved, waiting for release
   - **Ready for Sale**: Live on App Store
   - **Rejected**: Review feedback provided

## Troubleshooting

### Build Fails

- **Unity License Issues**: Verify serial number and credentials
- **Code Signing Errors**: Check certificates and provisioning profiles
- **Missing Dependencies**: Ensure all Unity modules are installed
- **Xcode Errors**: Check Xcode version compatibility

### App Store Rejection

- **Guideline Violations**: Review [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- **Missing Information**: Complete all required fields
- **Privacy Issues**: Update privacy policy and questionnaire
- **Performance**: Test on multiple devices

### Common Issues

1. **"Invalid Bundle"**: Check bundle identifier matches App Store Connect
2. **"Missing Compliance"**: Answer export compliance questions
3. **"Invalid Icons"**: Ensure all required icon sizes are provided
4. **"Missing Privacy Policy"**: Add privacy policy URL

## Additional Resources

- [CodeMagic Unity Documentation](https://docs.codemagic.io/getting-started/unity/)
- [Apple App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- [Unity iOS Build Guide](https://docs.unity3d.com/Manual/iphone-GettingStarted.html)
- [App Store Connect Help](https://help.apple.com/app-store-connect/)

## Next Steps

After your first successful deployment:

1. Set up automated builds on every release
2. Configure TestFlight beta testing
3. Set up crash reporting (e.g., Firebase Crashlytics)
4. Implement analytics (e.g., Unity Analytics)
5. Plan update releases

## Support

If you encounter issues:
- Check CodeMagic build logs
- Review Unity console for errors
- Consult Apple Developer Forums
- Contact CodeMagic support

Good luck with your iOS deployment! 🚀


