# SAPUpload FTP 550 Error - Troubleshooting Guide

## What is FTP 550 Error?
FTP 550 error means: **"File unavailable, not found, or access denied"**

## Current Configuration (Test Environment)
```
FTP Server: ftp://10.87.36.63/
Username: admin
Password: @2021PmPcN@$Adm!n*.*
Outbound Directory: SAP-FTP/OUTBOUND/
Local Download: C:\barcode\sapdownload\
```

## Common Causes & Solutions

### 1. **Directory Does Not Exist**
**Problem:** The path `SAP-FTP/OUTBOUND/` doesn't exist on the FTP server

**Solution:**
- Connect to the FTP server manually using an FTP client (FileZilla, WinSCP)
- Navigate to `ftp://10.87.36.63/`
- Check if the directory structure `SAP-FTP/OUTBOUND/` exists
- If not, create it manually or update the configuration

### 2. **Insufficient Permissions**
**Problem:** The `admin` user doesn't have read/write permissions

**Solution:**
- Verify that the `admin` user has:
  - **Read** access to list and download files
  - **Write** access to move/upload files
  - **Execute** access to traverse directories
- Contact your FTP server administrator to grant proper permissions

### 3. **Incorrect Path Format**
**Problem:** The FTP server requires absolute paths or different format

**Solution:**
Try these variations in `Form1.vb`:
```vb
' Option A: Add leading slash
Dim outboundDirectory As String = "/SAP-FTP/OUTBOUND/"

' Option B: Full path from root
Dim outboundDirectory As String = "~/SAP-FTP/OUTBOUND/"

' Option C: Check if directory is case-sensitive
Dim outboundDirectory As String = "sap-ftp/outbound/"
```

### 4. **FTP Server Not Running or Unreachable**
**Problem:** The FTP server at `10.87.36.63` is down or unreachable

**Solution:**
- Test connectivity: Open Command Prompt and run:
  ```
  ping 10.87.36.63
  ```
- Test FTP connection:
  ```
  ftp 10.87.36.63
  ```
  Then login with username: `admin` and password: `@2021PmPcN@$Adm!n*.*`

### 5. **Firewall or Network Blocking**
**Problem:** Firewall is blocking FTP connections (port 21)

**Solution:**
- Ensure port 21 (or the custom FTP port) is open
- Check Windows Firewall settings
- Verify with your network administrator

## Testing Steps

### Step 1: Manual FTP Connection Test
```
1. Open Command Prompt (cmd.exe)
2. Type: ftp 10.87.36.63
3. Enter username: admin
4. Enter password: @2021PmPcN@$Adm!n*.*
5. Once connected, type: ls
6. Check if you can see the directory structure
7. Try: cd SAP-FTP/OUTBOUND
8. Try: ls (to list files)
```

### Step 2: Verify Directory Structure
The FTP server should have this structure:
```
/
└── SAP-FTP/
    └── OUTBOUND/           <- Where WMSOB files are placed
        └── UPLOADED/       <- Where processed files are moved
            └── 2026-04/    <- Auto-created by application
```

### Step 3: Check Local Directory
Ensure the local directory exists and has write permissions:
```
C:\barcode\sapdownload\
```

### Step 4: Review Error Messages
After the code update, the application will now show detailed error messages:
- **FTP Error Code**: The exact 5xx code
- **Status Description**: Server's explanation
- **File/Path**: Which file or path caused the error

## Environment Configurations (Commented in Code)

The code has 4 environment configurations (currently using TEST):

1. **TEST** (lines 6-14) - Old test server: 137.40.28.82
2. **TEST 2** (lines 16-24) - Another test server: 137.40.28.83
3. **LIVE** (lines 26-34) - Live server: 10.87.36.77
4. **LIVE 2** (lines 36-44) - Alternative live config
5. **TEST (Current)** (lines 46-54) - **ACTIVE**: Using 137.40.28.85 with FTP 10.87.36.63

## Quick Fixes to Try

### Fix 1: Verify FTP Server Exists
Contact your IT team to confirm:
- Is `10.87.36.63` the correct FTP server IP?
- Is the FTP service running on this server?
- Has the password been changed recently?

### Fix 2: Use FileZilla to Test
1. Download FileZilla (free FTP client)
2. Connect to:
   - Host: `10.87.36.63`
   - Username: `admin`
   - Password: `@2021PmPcN@$Adm!n*.*`
   - Port: `21` (or ask your admin)
3. Try to navigate to `SAP-FTP/OUTBOUND/`
4. Try to upload/download a test file

### Fix 3: Check FTP Passive Mode
Add this to the code if behind a firewall:
```vb
FTPRequest.UsePassive = True  ' or False, try both
```

## Next Steps

1. **Rebuild the application** with the updated error handling
2. **Run the application** and note the exact error message
3. **Share the error message** for further troubleshooting
4. **Test FTP connectivity** manually using FileZilla or command line

## Contact Information
Based on the code comments, contact:
- **Judith** - Added the current configuration (2026-02-27)
- **IT Team** - For FTP server access and permissions

---

**Updated**: 2026-04-10
**Status**: Enhanced with better error handling to diagnose 550 errors
