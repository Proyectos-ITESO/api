# MicroJack PRO v1.0.0 Installation

## Package Contents
- **MicroJack.API.dll** - Main application
- **MicroJack.Updater** - Auto-updater executable  
- **MicroJack.Updater.dll** - Auto-updater library
- **Dependencies** - All required .NET libraries

## Installation Steps

1. **Extract Files:**
   ```bash
   unzip microjack-pro-v1.0.0.zip
   cd microjack-pro-v1.0.0
   ```

2. **Configure License Server:**
   - Edit `appsettings.json`
   - Set `LicenseSettings:UpdateServerUrl` to your license server
   - Set `LicenseSettings:LicenseKey` to your license key

3. **Run Application:**
   ```bash
   dotnet MicroJack.API.dll
   ```

## For License Server Setup

Update your `licenses.json` with:

```json
{
  "LicenseKey": "YOUR_LICENSE_KEY",
  "Owner": "Client Name",
  "Type": "Standard", 
  "ExpirationDate": "2025-12-31",
  "EnabledFeatures": ["Basic", "Advanced"],
  "LatestVersion": "1.0.0",
  "MinimumRequiredVersion": "1.0.0",
  "DownloadUrl": "https://yourserver.com/updates/microjack-pro-v1.0.0.zip",
  "FileHash": "5c389abb7c08811d77521f7d4ecf493aed67080613325e851f8b9777133ddb9f"
}
```

## Auto-Update Ready
This installation includes the auto-updater. Future updates will be handled automatically
when announced through the license server.

