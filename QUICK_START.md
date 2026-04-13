# SAPUpload - Quick Reference Card

## ✅ What's Fixed

The application now has **built-in SMB credentials** and will:
1. **Try SMB first** (with credentials: `admin` / `@2021PmPcN@$Adm!n*.*`)
2. **Fallback to FTP** automatically if SMB fails
3. **Show clear messages** about which mode is active

---

## 🚀 Quick Start

### Step 1: Build the Application
```
Open SAPUpload.sln in Visual Studio
Press Ctrl+Shift+B (Build Solution)
```

### Step 2: Place Test File
Put a test file in the SMB share:
```
\\10.87.36.63\sap-ftp\OUTBOUND\WMSOB_TEST_001.txt
```

### Step 3: Run the Application
```
Double-click SAPUpload.exe
```

You should see one of these messages:
- ✅ **"SMB processing complete. Processed 1 file(s)."** (SMB mode)
- ⚠️ **"SMB connection failed. Falling back to FTP mode."** then FTP processing
- ❌ Error message if both modes fail

---

## 🔧 Configuration Locations

### SMB Credentials (Form1.vb, line ~93-95)
```vb
Dim smbServerPath As String = "\\10.87.36.63\sap-ftp\OUTBOUND\"
Dim smbUsername As String = "admin"
Dim smbPassword As String = "@2021PmPcN@$Adm!n*.*"
```

### FTP Credentials (Form1.vb, line ~86-88)
```vb
Dim ftpServer As String = "ftp://10.87.36.63/"
Dim ftpUsername As String = "admin"
Dim ftpPassword As String = "@2021PmPcN@$Adm!n*.*"
```

### Force FTP Mode (Form1.vb, line ~35)
```vb
Private useSMB As Boolean = False  'Change to True for SMB
```

---

## 📁 Directory Structure

### Remote (Server .63)
```
\\10.87.36.63\sap-ftp\
└── OUTBOUND\
    ├── WMSOB_*.txt  ← Place incoming files here
    └── UPLOADED\
        └── 2026-04\ ← Processed files moved here
```

### Local (Server .89)
```
C:\barcode\sapdownload\
└── 2026-04\ ← Downloaded files stored here
```

---

## 🔍 Test Commands

### Test SMB Access
```cmd
dir \\10.87.36.63\sap-ftp\OUTBOUND
```

### Test FTP Access
```cmd
ftp 10.87.36.63
User: admin
Password: @2021PmPcN@$Adm!n*.*
```

### Connect SMB with Credentials
```cmd
net use \\10.87.36.63 /user:admin "@2021PmPcN@$Adm!n*.*"
```

---

## ❌ Common Errors & Solutions

| Error | Solution |
|-------|----------|
| "SMB connection failed" | Run: `net use \\10.87.36.63 /user:admin "@2021PmPcN@$Adm!n*.*"` |
| "Could not find path" | Create directory: `\\10.87.36.63\sap-ftp\OUTBOUND\` |
| "FTP 550 error" | SMB mode will handle this automatically |
| "No WMSOB files found" | Place file starting with "WMSOB" in OUTBOUND folder |

---

## 📊 How It Works

```
Application Starts
        ↓
Connect to SMB Share?
    ↙         ↘
  YES          NO
   ↓            ↓
SMB Mode    FTP Mode
   ↓            ↓
Process      Process
Files        Files
   ↓            ↓
Move to      Move to
UPLOADED     UPLOADED
```

---

## 🆘 Need Help?

1. Check logs in **Output Window** or **Console**
2. Review error messages (they now show exact details)
3. Run diagnostic: `test_ftp_paths.bat`
4. See full guide: `DUAL_MODE_SMB_FTP_GUIDE.md`

---

**Version:** 2.0 - Dual Mode with Credentials  
**Last Updated:** 2026-04-10
