# Quick Start Checklist for iOS Deployment

Use this checklist to quickly set up your iOS deployment with CodeMagic.

## Pre-Deployment Checklist

### Unity Project Setup
- [ ] Open Unity project
- [ ] Go to **Edit > Project Settings > Player**
- [ ] Set **Bundle Identifier**: `com.yourcompany.plgy` (update with your actual bundle ID)
- [ ] Set **Version**: `1.0.0`
- [ ] Set **Build Number**: `1`
- [ ] Under **iOS Settings**:
  - [ ] Set **Target minimum iOS Version**: `13.0` or higher
  - [ ] Set **Target Device**: iPhone + iPad (or as needed)
  - [ ] Set **Architecture**: ARM64
  - [ ] Add **App Icons** (all required sizes)
  - [ ] Add **Launch Screen** (if custom)
- [ ] Go to **File > Build Settings**
- [ ] Select **iOS** platform
- [ ] Ensure all scenes are added to build
- [ ] Save project

### Apple Developer Account
- [ ] Sign up for [Apple Developer Program](https://developer.apple.com) ($99/year)
- [ ] Note your **Team ID** (found in Apple Developer Portal)
- [ ] Create **App ID** with your bundle identifier
- [ ] Create **Distribution Certificate** (Apple Distribution)
- [ ] Create **Distribution Provisioning Profile** for App Store
- [ ] Download certificate (.p12) and provisioning profile (.mobileprovision)

### App Store Connect
- [ ] Log in to [App Store Connect](https://appstoreconnect.apple.com)
- [ ] Create new app:
  - [ ] Name
  - [ ] Bundle ID (select the one you created)
  - [ ] SKU (unique identifier)
- [ ] Prepare app listing:
  - [ ] App description
  - [ ] Keywords
  - [ ] Screenshots (all required sizes)
  - [ ] App icon (1024x1024)
  - [ ] Privacy Policy URL
  - [ ] Support URL

### CodeMagic Setup
- [ ] Sign up at [codemagic.io](https://codemagic.io)
- [ ] Connect your Git repository (GitHub/GitLab/Bitbucket)
- [ ] Add application in CodeMagic
- [ ] Configure environment variables:
  - [ ] `UNITY_SERIAL` - Your Unity license serial
  - [ ] `UNITY_EMAIL` - Your Unity account email
  - [ ] `UNITY_PASSWORD` - Your Unity account password
  - [ ] `BUNDLE_ID` - Your app bundle identifier
- [ ] Upload code signing certificates:
  - [ ] Apple Distribution Certificate (.p12)
  - [ ] Provisioning Profile (.mobileprovision)
  - [ ] Or enable automatic code signing
- [ ] Update `codemagic.yaml`:
  - [ ] Replace `BUNDLE_ID` with your actual bundle ID
  - [ ] Add Unity serial number
  - [ ] Update email in publishing section
- [ ] Update `exportOptions.plist`:
  - [ ] Replace `YOUR_TEAM_ID` with your Team ID
  - [ ] Replace `YOUR_BUNDLE_ID` with your bundle ID
  - [ ] Replace `YOUR_PROVISIONING_PROFILE_NAME` with profile name

### First Build
- [ ] Commit and push `codemagic.yaml` and `exportOptions.plist` to repository
- [ ] In CodeMagic, click **Start new build**
- [ ] Select branch and workflow
- [ ] Monitor build progress
- [ ] Fix any build errors
- [ ] Download `.ipa` file when build succeeds

### TestFlight Testing
- [ ] Upload `.ipa` to App Store Connect (via Transporter or Xcode)
- [ ] Wait for processing (10-30 minutes)
- [ ] Add internal testers
- [ ] Test on physical iOS device
- [ ] Fix any runtime issues
- [ ] Add external testers (optional, requires Beta App Review)

### App Store Submission
- [ ] Complete all App Store Connect information:
  - [ ] Version information
  - [ ] What's New in This Version
  - [ ] Screenshots for all device sizes
  - [ ] Description
  - [ ] Keywords
  - [ ] Support URL
  - [ ] Privacy Policy URL
- [ ] Answer Export Compliance questions
- [ ] Set Age Rating
- [ ] Add App Review Information (demo account if needed)
- [ ] Submit for Review
- [ ] Monitor review status

## Common Values to Update

### In codemagic.yaml:
```yaml
BUNDLE_ID: "com.yourcompany.plgy"  # Your actual bundle ID
UNITY_SERIAL: "YOUR_SERIAL"         # Your Unity license serial
```

### In exportOptions.plist:
```xml
<key>teamID</key>
<string>YOUR_TEAM_ID</string>  # Your Apple Developer Team ID

<key>YOUR_BUNDLE_ID</key>
<string>YOUR_PROVISIONING_PROFILE_NAME</string>
```

## Troubleshooting Quick Reference

| Issue | Solution |
|-------|----------|
| Build fails - Unity license | Check UNITY_SERIAL, UNITY_EMAIL, UNITY_PASSWORD |
| Code signing error | Verify certificates and provisioning profiles |
| Invalid bundle | Ensure bundle ID matches App Store Connect |
| Missing icons | Add all required icon sizes in Unity |
| App rejected | Review App Store Review Guidelines |

## Next Steps After First Release

- [ ] Set up automated builds on git tags
- [ ] Configure TestFlight beta testing workflow
- [ ] Set up crash reporting
- [ ] Implement analytics
- [ ] Plan update schedule

## Support Resources

- CodeMagic Docs: https://docs.codemagic.io/getting-started/unity/
- Apple Guidelines: https://developer.apple.com/app-store/review/guidelines/
- Unity iOS Docs: https://docs.unity3d.com/Manual/iphone-GettingStarted.html

---

**Estimated Time**: 2-4 hours for initial setup, 1-2 days for App Store review

**Cost**: $99/year for Apple Developer Program + CodeMagic build minutes (free tier available)


