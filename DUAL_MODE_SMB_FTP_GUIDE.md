# SAPUpload - Dual Mode (SMB + FTP with Fallback)

## What Changed

The application now supports **BOTH SMB and FTP** connection modes with automatic fallback:

1. **SMB Mode (Primary)** - Uses Windows file sharing with built-in credentials
2. **FTP Mode (Fallback)** - Falls back to FTP if SMB connection fails

## Architecture

```
┌─────────────────────────────────────────────────┐
│  Server: 10.87.36.89                            │
│  SAPUpload Application                          │
│                                                  │
│  1. Try SMB first (with credentials)            │
│     \\10.87.36.63\sap-ftp\OUTBOUND\             │
│     Username: admin                              │
│     Password: @2021PmPcN@$Adm!n*.*              │
│                                                  │
│  2. If SMB fails → Fallback to FTP              │
│     ftp://10.87.36.63/SAP-FTP/OUTBOUND/         │
│     Username: admin                              │
│     Password: @2021PmPcN@$Adm!n*.*              │
└───────────────────┬─────────────────────────────┘
                    │
         ┌──────────┴──────────┐
         │                     │
    SMB (Port 445)        FTP (Port 21)
         │                     │
         └──────────┬──────────┘
                    │
┌───────────────────┴───────────────────────────┐
│  Server: 10.87.36.63                          │
│  - SMB Share: \\10.87.36.63\sap-ftp\         │
│  - FTP Server: ftp://10.87.36.63/            │
│  - Expected structure:                        │
│    sap-ftp/                                   │
│    └── OUTBOUND/                              │
│        ├── WMSOB*.txt (incoming files)        │
│        └── UPLOADED/                          │
│            └── 2026-04/ (auto-created)        │
└───────────────────────────────────────────────┘
```

## Configuration

### SMB Configuration (Primary)
```vb
Dim smbServerPath As String = "\\10.87.36.63\sap-ftp\OUTBOUND\"
Dim smbUsername As String = "admin"
Dim smbPassword As String = "@2021PmPcN@$Adm!n*.*"
```

### FTP Configuration (Fallback)
```vb
Dim ftpServer As String = "ftp://10.87.36.63/"
Dim ftpUsername As String = "admin"
Dim ftpPassword As String = "@2021PmPcN@$Adm!n*.*"
Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"
```

### Switch Connection Mode

To force a specific mode, change this variable:

```vb
'Try SMB first (default)
Private useSMB As Boolean = True

'Force FTP only
Private useSMB As Boolean = False
```

## How It Works

### On Application Startup:

1. **Creates local directories** (if not exist)
   - `C:\barcode\sapdownload\`
   - `C:\barcode\sapdownload\2026-04\` (current month)

2. **Attempts SMB Connection**
   - Connects to `\\10.87.36.63\sap-ftp\OUTBOUND\`
   - Authenticates with SMB credentials
   - If successful → Sets `connectionMode = "SMB"`

3. **Fallback to FTP (if SMB fails)**
   - Shows warning message
   - Sets `connectionMode = "FTP"`
   - Continues with FTP protocol

4. **Processes files** based on connection mode

### File Processing:

#### **SMB Mode:**
- Lists files using `Directory.GetFiles()`
- Copies files using `File.Copy()`
- Moves files using `File.Move()`
- **No FTP protocol overhead**
- **Direct file access via network share**

#### **FTP Mode:**
- Lists files using `FtpWebRequest.ListDirectory`
- Downloads files using `FtpWebRequest.DownloadFile`
- Moves files using `FtpWebRequest.Rename`
- **Standard FTP operations**

## New Functions Added

| Function | Purpose |
|----------|---------|
| `ConnectToSMBShare()` | Authenticate and connect to SMB share |
| `DisconnectFromSMB()` | Disconnect from SMB share |
| `ProcessFilesViaSMB()` | Process files using SMB protocol |
| `ProcessFilesViaFTP()` | Process files using FTP protocol |
| `copyfilefromsmb()` | Copy file from SMB to local |
| `moveuploadedfileSMB()` | Move processed file in SMB share |

## Advantages of SMB Mode

✅ **Simpler** - No FTP protocol complexity  
✅ **Faster** - Direct file access  
✅ **No 550 errors** - Uses actual Windows paths  
✅ **Credentials handled** - Built into the code  
✅ **Easy troubleshooting** - Test with File Explorer  

## Advantages of FTP Mode

✅ **Standard protocol** - Works across firewalls  
✅ **Audit trail** - FTP logging  
✅ **Bandwidth control** - FTP server features  

## Troubleshooting

### SMB Connection Fails

**Check 1: Network Connectivity**
```cmd
ping 10.87.36.63
```

**Check 2: SMB Share Access**
```cmd
dir \\10.87.36.63\sap-ftp\OUTBOUND
```

**Check 3: Manual Connection**
```cmd
net use \\10.87.36.63 /user:admin "@2021PmPcN@$Adm!n*.*"
```

**Check 4: Firewall**
- Ensure port **445** is open (SMB)
- Check Windows Firewall on both servers

### FTP Fallback Also Fails

**Run diagnostic:**
```
C:\Users\70R1581\source\repos\SAPUpload\test_ftp_paths.bat
```

**Common FTP issues:**
- Directory path mismatch (550 error)
- Wrong credentials (530 error)
- FTP service not running
- Firewall blocking port 21

## Testing the Application

### Test SMB Mode:
1. Ensure SMB share is accessible:
   ```
   \\10.87.36.63\sap-ftp\OUTBOUND\
   ```
2. Place test file: `WMSOB_TEST_001.txt` in the OUTBOUND folder
3. Run SAPUpload application
4. Check for success message

### Test FTP Mode:
1. Force FTP mode in code:
   ```vb
   Private useSMB As Boolean = False
   ```
2. Rebuild and run
3. Check if FTP connection works

### Test Fallback:
1. Block SMB access (temporarily disable port 445)
2. Run application
3. Should see: "SMB connection failed. Falling back to FTP mode."
4. Should continue processing via FTP

## Error Messages

| Message | Cause | Solution |
|---------|-------|----------|
| "SMB connection failed. Falling back to FTP mode." | SMB credentials invalid or share unreachable | Check SMB credentials and network |
| "SMB Error: Could not find a part of the path..." | Directory structure doesn't exist | Create `\\10.87.36.63\sap-ftp\OUTBOUND\` |
| "FTP Error ActionNotTakenFileUnavailable: 550" | FTP path doesn't exist | Try different path format (see FTP_550_FIX_GUIDE.md) |
| "FTP Error NotLoggedIn: 530" | Wrong FTP credentials | Update ftpUsername/ftpPassword |

## Files Modified

| File | Changes |
|------|---------|
| `Form1.vb` | Added SMB support, dual-mode logic, credential handling |
| `FTP_550_FIX_GUIDE.md` | Comprehensive troubleshooting guide |
| `test_ftp_paths.bat` | FTP diagnostic tool |

## Security Notes

⚠️ **Credentials are stored in plain text** in the code

**Recommendations:**
1. Restrict access to the executable file
2. Consider moving credentials to encrypted config file
3. Use Windows authentication if possible
4. Regularly rotate passwords

## Next Steps

1. **Build the application** in Visual Studio
2. **Test SMB mode** by placing a test file in the SMB share
3. **Verify** that files are processed correctly
4. **Monitor** the application logs to confirm connection mode

---

**Created:** 2026-04-10  
**Version:** 2.0 - Dual Mode with Fallback  
**Status:** Ready for Testing
