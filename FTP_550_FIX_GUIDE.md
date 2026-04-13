# FTP 550 Error - RESOLVED Guide

## Problem
```
FTP Error ActionNotTakenFileUnavailable: 550
SAP-FTP/OUTBOUND: No such file or directory
Server: ftp://10.87.36.63/SAP-FTP/OUTBOUND/
```

## Root Cause
**FTP paths are NOT the same as SMB paths!**

- SMB Path: `\\10.87.36.63\sap-ftp\OUTBOUND\` ✅ EXISTS
- FTP Path: `ftp://10.87.36.63/SAP-FTP/OUTBOUND/` ❌ NOT FOUND

The FTP server on `.63` is likely **chrooted** (locked) to a specific directory, so what you see via SMB (`\\10.87.36.63\sap-ftp\`) is different from what the FTP server sees as its root.

---

## SOLUTIONS (Try in Order)

### **Solution 1: Run the FTP Diagnostic Script** ⭐ RECOMMENDED

I created a diagnostic script that will test the FTP server and show you the actual directory structure.

**On server 10.87.36.89, run:**
```
C:\Users\70R1581\source\repos\SAPUpload\test_ftp_paths.bat
```

This will:
1. Connect to FTP server
2. Show the FTP root directory
3. Try different path combinations
4. Show which paths work and which return 550

**Review the output** and look for:
- What folders are visible at the FTP root
- Which path successfully lists files

---

### **Solution 2: Try Different Path Formats**

Based on common FTP server configurations, try these in `Form1.vb`:

#### **Option A: FTP root is already at `sap-ftp` folder**
If the FTP server's root is set to `C:\sap-ftp\` on the .63 server, then:

**Change in Form1.vb (line ~63):**
```vb
'FROM:
Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"

'TO:
Dim outboundDirectory As String = "OUTBOUND/"
```

#### **Option B: Use leading slash (absolute path)**
```vb
Dim outboundDirectory As String = "/SAP-FTP/OUTBOUND/"
```

#### **Option C: All lowercase**
```vb
Dim outboundDirectory As String = "sap-ftp/outbound/"
```

**After each change:**
1. Rebuild the application
2. Run it and check if the error changes

---

### **Solution 3: Verify SMB Path Works from .89 Server**

Test if the SMB share is actually accessible from server .89:

**Open Command Prompt on .89 and run:**
```cmd
REM Test SMB access
dir \\10.87.36.63\sap-ftp\OUTBOUND

REM If this works, you should see files listed
REM If it fails, you'll get a network error
```

**If SMB works but FTP doesn't**, the issue is definitely the FTP server's path mapping.

---

### **Solution 4: Use SMB Instead of FTP (Alternative Approach)**

If FTP continues to fail, you can modify the code to use **SMB directly** instead of FTP. This bypasses FTP entirely.

**Advantages:**
- No FTP protocol overhead
- Direct file access via network share
- Easier to troubleshoot

**Disadvantages:**
- Requires SMB permissions
- May be slower over network

**Let me know if you want me to create an SMB version of the code.**

---

## Quick Diagnostic Commands

Run these on **server 10.87.36.89**:

### Test 1: SMB Access
```cmd
dir \\10.87.36.63\sap-ftp\OUTBOUND
```

### Test 2: FTP Access (Manual)
```cmd
ftp 10.87.36.63
```
Then enter:
```
User: admin
Password: @2021PmPcN@$Adm!n*.*
ftp> ls
ftp> cd SAP-FTP
ftp> ls
ftp> cd OUTBOUND
ftp> ls
```

### Test 3: Check if SMB and FTP show same content
Compare what you see via:
- SMB: `\\10.87.36.63\sap-ftp\`
- FTP: After connecting, run `ls` at root

**If they show different things**, the FTP server is chrooted to a different directory.

---

## Architecture Summary

```
┌─────────────────────────────────────────────────┐
│  Server: 10.87.36.89                            │
│  - Runs SAPUpload application                   │
│  - Connects to .63 via FTP and SMB              │
└───────────────────┬─────────────────────────────┘
                    │
         ┌──────────┴──────────┐
         │ FTP (port 21)       │
         │ ftp://10.87.36.63/  │
         └─────────────────────┘
         │
         │ SMB (port 445)
         └─────────────────────┐
                               │
┌──────────────────────────────┴──────────────┐
│  Server: 10.87.36.63                        │
│  - FTP Server (root unknown)                │
│  - SMB Share: \\10.87.36.63\sap-ftp\       │
│  - Expected structure:                      │
│    sap-ftp/                                 │
│    └── OUTBOUND/                            │
│        └── UPLOADED/                        │
│            └── 2026-04/                     │
└─────────────────────────────────────────────┘
```

---

## Next Steps

1. ✅ **Run `test_ftp_paths.bat`** to discover the actual FTP structure
2. ✅ **Share the output** with me and I'll tell you the exact fix
3. ✅ **Update `outboundDirectory`** in Form1.vb based on findings
4. ✅ **Rebuild and test** the application

---

## Files Created for You

| File | Purpose |
|------|---------|
| `test_ftp_paths.bat` | Diagnostic script to test FTP paths |
| `create_ftp_dirs.bat` | Script to create missing directories (if needed) |
| `FTP_550_FIX_GUIDE.md` | This guide |

---

**Updated:** 2026-04-10
**Status:** Awaiting FTP path diagnostic results
