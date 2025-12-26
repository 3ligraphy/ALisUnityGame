# iOS Deployment with CodeMagic - Overview

This repository is configured for iOS deployment to the App Store using CodeMagic CI/CD.

## 📁 Files Created

1. **`codemagic.yaml`** - CodeMagic CI/CD configuration file
2. **`exportOptions.plist`** - Xcode export options for IPA generation
3. **`IOS_DEPLOYMENT_GUIDE.md`** - Comprehensive step-by-step deployment guide
4. **`QUICK_START_CHECKLIST.md`** - Quick reference checklist

## 🚀 Quick Start

1. **Read the Quick Start Checklist**: Open `QUICK_START_CHECKLIST.md` and follow the steps
2. **Update Configuration Files**:
   - Edit `codemagic.yaml` - Update bundle ID, Unity credentials
   - Edit `exportOptions.plist` - Update Team ID and bundle ID
3. **Set Up CodeMagic**: Follow the guide in `IOS_DEPLOYMENT_GUIDE.md`
4. **Build and Deploy**: Push to your repository and trigger a build in CodeMagic

## 📋 What You Need

### Required:
- ✅ Apple Developer Account ($99/year)
- ✅ Unity License (Personal/Plus/Pro)
- ✅ CodeMagic Account (free tier available)
- ✅ Git Repository (GitHub/GitLab/Bitbucket)

### Recommended:
- ✅ TestFlight for beta testing
- ✅ App Store Connect API keys for automation

## 🔧 Configuration Steps

### 1. Update Bundle Identifier
In Unity:
- Edit > Project Settings > Player
- Set Bundle Identifier: `com.yourcompany.plgy` (or your own)

### 2. Configure CodeMagic
1. Sign up at [codemagic.io](https://codemagic.io)
2. Connect your repository
3. Add environment variables:
   - `UNITY_SERIAL`
   - `UNITY_EMAIL`
   - `UNITY_PASSWORD`
   - `BUNDLE_ID`

### 3. Set Up Code Signing
- Upload certificates in CodeMagic dashboard
- Or enable automatic code signing

### 4. Update Files
- `codemagic.yaml`: Update bundle ID and credentials
- `exportOptions.plist`: Update Team ID and bundle ID

## 📖 Documentation

- **Full Guide**: See `IOS_DEPLOYMENT_GUIDE.md` for detailed instructions
- **Quick Reference**: See `QUICK_START_CHECKLIST.md` for a checklist
- **CodeMagic Docs**: https://docs.codemagic.io/getting-started/unity/

## 🎯 Build Process

1. **Unity Build**: Unity builds the iOS project
2. **Xcode Configuration**: Project is configured with bundle ID and version
3. **CocoaPods**: Dependencies are installed (if needed)
4. **Xcode Archive**: Project is archived
5. **IPA Export**: Archive is exported as IPA file
6. **Artifacts**: IPA is available for download or TestFlight upload

## 🔍 Troubleshooting

### Build Fails
- Check Unity license credentials
- Verify bundle ID matches App Store Connect
- Check code signing certificates

### Code Signing Issues
- Verify certificates are uploaded to CodeMagic
- Check Team ID matches
- Ensure provisioning profile is valid

### App Store Rejection
- Review App Store Review Guidelines
- Complete all required information
- Test on multiple devices

## 📞 Support

- CodeMagic Support: https://docs.codemagic.io/
- Apple Developer Support: https://developer.apple.com/support/
- Unity iOS Docs: https://docs.unity3d.com/Manual/iphone-GettingStarted.html

## ⚠️ Important Notes

1. **Never commit**:
   - Unity license serial numbers
   - Passwords
   - Private keys
   - Use environment variables instead

2. **Always test**:
   - Build locally first if possible
   - Test on TestFlight before App Store submission
   - Test on multiple iOS devices

3. **Keep updated**:
   - Unity version
   - Xcode version
   - iOS SDK version

## 🎉 Next Steps

After successful deployment:
1. Set up automated builds on git tags
2. Configure TestFlight beta testing
3. Set up crash reporting
4. Implement analytics
5. Plan regular updates

---

**Good luck with your iOS deployment!** 🚀

For detailed instructions, see `IOS_DEPLOYMENT_GUIDE.md`


