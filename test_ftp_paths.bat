@echo off
echo ========================================
echo FTP Server Diagnostic Tool
echo Testing: 10.87.36.63
echo ========================================
echo.

REM Create FTP script to test various paths
echo open 10.87.36.63 > ftp_test.txt
echo admin >> ftp_test.txt
echo @2021PmPcN@$Adm!n*.* >> ftp_test.txt
echo. >> ftp_test.txt
echo ==== Current Directory ==== >> ftp_test.txt
echo pwd >> ftp_test.txt
echo. >> ftp_test.txt
echo ==== List Root Directory ==== >> ftp_test.txt
echo ls >> ftp_test.txt
echo. >> ftp_test.txt
echo ==== Try SAP-FTP/OUTBOUND ==== >> ftp_test.txt
echo cd SAP-FTP/OUTBOUND >> ftp_test.txt
echo pwd >> ftp_test.txt
echo ls >> ftp_test.txt
echo. >> ftp_test.txt
echo ==== Try OUTBOUND (if already in SAP-FTP) ==== >> ftp_test.txt
echo cd .. >> ftp_test.txt
echo cd OUTBOUND >> ftp_test.txt
echo pwd >> ftp_test.txt
echo ls >> ftp_test.txt
echo. >> ftp_test.txt
echo ==== Try /SAP-FTP/OUTBOUND (absolute path) ==== >> ftp_test.txt
echo cd / >> ftp_test.txt
echo cd SAP-FTP >> ftp_test.txt
echo cd OUTBOUND >> ftp_test.txt
echo pwd >> ftp_test.txt
echo ls >> ftp_test.txt
echo. >> ftp_test.txt
echo bye >> ftp_test.txt

echo Running FTP tests...
echo.
ftp -s:ftp_test.txt

del ftp_test.txt

echo.
echo ========================================
echo Diagnostic complete!
echo Review the output above to see:
echo  - What directory the FTP root is
echo  - Which paths exist or return 550 errors
echo ========================================
pause
