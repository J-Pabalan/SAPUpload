Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Runtime.InteropServices

Public Class Form1

    'Windows API for SMB credential management
    <DllImport("mpr.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function WNetAddConnection2(ByRef netResource As NETRESOURCE, ByVal password As String, ByVal username As String, ByVal flags As UInteger) As Integer
    End Function

    <DllImport("mpr.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function WNetCancelConnection2(ByVal name As String, ByVal flags As UInteger, ByVal force As Boolean) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure NETRESOURCE
        Public dwScope As UInteger
        Public dwType As UInteger
        Public dwDisplayType As UInteger
        Public dwUsage As UInteger
        <MarshalAs(UnmanagedType.LPWStr)>
        Public lpLocalName As String
        <MarshalAs(UnmanagedType.LPWStr)>
        Public lpRemoteName As String
        <MarshalAs(UnmanagedType.LPWStr)>
        Public lpComment As String
        <MarshalAs(UnmanagedType.LPWStr)>
        Public lpProvider As String
    End Structure

    'Connection mode tracking
    Private useSMB As Boolean = True   'Use SMB/SMB share since it's already working
    Private connectionMode As String = ""
    ''*****************             T E S T             *****************
    'Dim con As New SqlConnection("Data Source=137.40.28.82;Initial Catalog=BarcodeSystem_test;User Id=sa;Password=Pmpc2020")  'test
    'Dim ftpServer As String = "ftp://137.40.28.24/"                                                                         'test
    'Dim ftpUsername As String = "sapftp1"                                                                                   'test
    'Dim ftpPassword As String = "pphsap01"                                                                                  'test
    'Dim outboundDirectory As String = "SAP-FTP/TEST/OUTBOUND/"                                                              'test
    'Dim localDownloadDirectory As String = "C:\barcode\TEST\sapdownload\"                                                   'test
    'Dim UploadedDirectory As String = "UPLOADED/"                                                                           'test
    'Dim SapDownloadDir As String = "C:\barcode\TEST\sapdownload\"                                                           'test
    ''*****************             T E S T             *****************

    ''*****************             T E S T 2           *****************
    'Dim con As New SqlConnection("Data Source=137.40.28.83;Initial Catalog=BarcodeSystem_test;User Id=sa;Password=Pmpceapps2022")  'test
    'Dim ftpServer As String = "ftp://137.40.28.24/"                                                                         'test
    'Dim ftpUsername As String = "sapftp1"                                                                                   'test
    'Dim ftpPassword As String = "pphsap01"                                                                                  'test
    'Dim outboundDirectory As String = "SAP-FTP/TEST/OUTBOUND/"                                                              'test
    'Dim localDownloadDirectory As String = "C:\barcode\TEST\sapdownload\"                                                   'test
    'Dim UploadedDirectory As String = "UPLOADED/"                                                                           'test
    'Dim SapDownloadDir As String = "C:\barcode\TEST\sapdownload\"                                                           'test
    ''*****************             T E S T  2          *****************


    ''*****************             L I V E             *****************
    'Dim con As New SqlConnection("Data Source=137.40.28.83;Initial Catalog=BarcodeSystem_Live;User Id=sa;Password=Pmpceapps2022") 'live
    'Dim ftpServer As String = "ftp://137.40.28.24/"                        'added alvz 20210713
    'Dim ftpUsername As String = "sapftp1"                                  'added alvz 20210713
    'Dim ftpPassword As String = "pphsap01"                                 'added alvz 20210713
    'Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"                  'added alvz 20210713
    'Dim localDownloadDirectory As String = "C:\barcode\sapdownload\"       'added alvz 20210713
    'Dim UploadedDirectory As String = "UPLOADED/"                          'added alvz 20210713
    'Dim SapDownloadDir As String = "C:\barcode\sapdownload\"               'added alvz 20210713
    ''*****************             L I V E             *****************



    ''*****************             L I V E             *****************
    'Dim con As New SqlConnection("Data Source=10.87.36.77;Initial Catalog=BarcodeSystem_Live;User Id=sa;Password=Pmpceapps2022") 'live
    'Dim ftpServer As String = "ftp://10.87.36.63/"                        'added judith 20260227
    'Dim ftpUsername As String = "admin"                                   'added judith 20260227
    'Dim ftpPassword As String = "@2021PmPcN@$Adm!n*.*"                   'added judith 20260227
    'Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"                   'added judith 20260227
    'Dim localDownloadDirectory As String = "C:\barcode\sapdownload\"        'added judith 20260227
    'Dim UploadedDirectory As String = "UPLOADED/"                           'added judith 20260227
    'Dim SapDownloadDir As String = "C:\barcode\sapdownload\"               'added judith 20260227
    ''*****************             L I V E             *****************

    '*****************             T E S T             *****************
    Dim con As New SqlConnection("Data Source=10.87.36.77;Initial Catalog=BarcodeSystem_Live;User Id=sa;Password=Pmpceapps2022") 'live

    ' FTP Configuration
    Dim ftpServer As String = "ftp://10.87.36.63/"                        'added judith 20260227
    Dim ftpUsername As String = "admin"                                   'added judith 20260227
    Dim ftpPassword As String = "@2021PmPcN@$Adm!n*.*"                   'added judith 20260227

    ' FTP Path Options (uncomment the one that works)
    'Dim outboundDirectory As String = "OUTBOUND/"                        'Try this first (FTP root may already be at sap-ftp)
    Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"              'Confirmed correct path
    'Dim outboundDirectory As String = "/OUTBOUND/"                     'Absolute path variant
    'Dim outboundDirectory As String = "sap-ftp/OUTBOUND/"              'Lowercase path
    'Dim outboundDirectory As String = ""                               'DEBUG: list root directory
    'Dim outboundDirectory As String = "SAPSGST/"                       'DEBUG: check SAPSGST folder

    ' SMB Configuration
    Dim smbServerPath As String = "\\10.87.36.63\sap-ftp\OUTBOUND\"      'SMB share path with credentials
    Dim smbUsername As String = "admin"                                   'SMB username
    Dim smbPassword As String = "@2021PmPcN@$Adm!n*.*"                   'SMB password

    Dim localDownloadDirectory As String = "C:\barcode\sapdownload\"        'added judith 20260227
    Dim UploadedDirectory As String = "UPLOADED/"                           'added judith 20260227
    Dim SapDownloadDir As String = "C:\barcode\sapdownload\"               'added judith 20260227
    '*****************             T E S T             *****************


    Dim CurrentYearMonth As String
    Dim cmd As New SqlCommand
    Dim varordertype As String = ""
    Dim ordertypesi As String = ""
    Dim varordernumber As String = ""
    Dim referenceid As Integer = 0
    Dim referencerow As Integer = 0
    Dim fileseqn As Integer = 0
    Dim strfileseqn As String = ""
    Dim yrmoday As String
    Dim yr, mo, day As String
    Dim strlabel As String
    Dim rcount() As String
    Dim count As Integer = 0
    Dim loopcount As Integer = 0
    Dim refid As String = ""
    Dim varcurfname As String = ""

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CreateCurrentYearMonthDirectory()

        'Try to connect to SMB share first
        If useSMB Then
            If Not ConnectToSMBShare() Then
                'SMB connection failed, fallback to FTP
                useSMB = False
                connectionMode = "FTP"
                'MessageBox.Show("SMB connection failed. Falling back to FTP mode.", "Connection Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Console.WriteLine("SMB connection failed. Falling back to FTP mode.")
            Else
                connectionMode = "SMB"
            End If
        Else
            connectionMode = "FTP"
        End If

        Button1.PerformClick()
        Me.Close()
    End Sub

    ''' <summary>
    ''' Connect to SMB share with credentials
    ''' </summary>
    Private Function ConnectToSMBShare() As Boolean
        Try
            'Extract server name from UNC path (\\10.87.36.63\sap-ftp -> \\10.87.36.63)
            Dim smbRoot As String = smbServerPath.Substring(0, smbServerPath.IndexOf("\", 2))

            'Error 1219: Cancel any existing connection to this server first
            WNetCancelConnection2(smbRoot, 0, True)
            System.Threading.Thread.Sleep(500) 'Brief wait for disconnect

            Dim nr As New NETRESOURCE()
            nr.dwType = &H1  ' RESOURCETYPE_DISK
            nr.lpRemoteName = smbRoot
            nr.lpLocalName = Nothing
            nr.lpProvider = Nothing

            'Try to connect (0x0 = no special flags)
            Dim result As Integer = WNetAddConnection2(nr, smbPassword, smbUsername, 0)

            If result = 0 Then
                Console.WriteLine($"Successfully connected to SMB share: {smbServerPath}")
                Return True
            Else
                'MessageBox.Show($"SMB connection failed with error code: {result}" & vbCrLf & "Common codes: 87=Bad path, 1219=Credential conflict, 53=Network not found", "SMB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Console.WriteLine($"SMB connection failed with error code: {result}")
                Return False
            End If
        Catch ex As Exception
            'MessageBox.Show($"SMB connection error: {ex.Message}", "SMB Exception", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"SMB connection error: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Disconnect from SMB share
    ''' </summary>
    Private Sub DisconnectFromSMB()
        Try
            Dim smbRoot As String = smbServerPath.Substring(0, smbServerPath.IndexOf("\", 2))
            WNetCancelConnection2(smbRoot, 0, True)
        Catch ex As Exception
            'Ignore errors during disconnect
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If connectionMode = "SMB" Then
            ProcessFilesViaSMB()
        Else
            ProcessFilesViaFTP()
        End If
    End Sub

    ''' <summary>
    ''' Process files using SMB share
    ''' </summary>
    Private Sub ProcessFilesViaSMB()
        Try
            'List all files in the SMB directory
            Dim files() As String = Directory.GetFiles(smbServerPath, "WMSOB*.*")

            If files.Length = 0 Then
                Console.WriteLine("No WMSOB files found in the SMB share. Standing by...")
                Return
            End If

            For Each filePath As String In files
                Dim fileName As String = Path.GetFileName(filePath)
                rcount = New String() {fileName}  'Mock array for compatibility
                loopcount = 0

                'Copy file to local download directory
                copyfilefromsmb(filePath)

                'Move uploaded file
                moveuploadedfileSMB(fileName)
            Next

            'MessageBox.Show($"SMB processing complete. Processed {files.Length} file(s).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Console.WriteLine($"SMB processing complete. Processed {files.Length} file(s).")

        Catch ex As Exception
            'MessageBox.Show($"SMB Error: {ex.Message}", "SMB Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"SMB Error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Process files using FTP
    ''' </summary>
    Private Sub ProcessFilesViaFTP()
        Try
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ftpServer & outboundDirectory), FtpWebRequest)
            request.Method = WebRequestMethods.Ftp.ListDirectory
            request.Credentials = New NetworkCredential(ftpUsername, ftpPassword)
            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
            Dim responseStream As Stream = response.GetResponseStream()
            Dim reader As New StreamReader(responseStream)

            strlabel = (reader.ReadToEnd())
            rcount = Split(strlabel, vbCrLf)

            For Me.loopcount = 0 To CInt(rcount.Length) - 1
                If Not String.IsNullOrEmpty(rcount(loopcount)) AndAlso rcount(loopcount).Contains("WMSOB") Then
                    copyfilefromftp()
                End If
            Next

            Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription)
            reader.Close()
            response.Close()
        Catch ex As WebException
            Dim ftpResponse As FtpWebResponse = CType(ex.Response, FtpWebResponse)
            'MessageBox.Show($"FTP Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}" & vbCrLf &
            '              $"Server: {ftpServer & outboundDirectory}" & vbCrLf &
            '              $"Details: {ex.Message}", "FTP Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"FTP Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}")
        Catch ex As Exception
            'MessageBox.Show($"Error: {ex.Message}", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error: {ex.Message}")
        End Try
    End Sub


    ''' <summary>
    ''' Copy file from SMB share to local directory
    ''' </summary>
    Private Sub copyfilefromsmb(sourceFilePath As String)
        Try
            Dim fileName As String = Path.GetFileName(sourceFilePath)
            Dim destPath As String = localDownloadDirectory & CurrentYearMonth & "\" & fileName

            'Ensure destination directory exists
            Directory.CreateDirectory(localDownloadDirectory & CurrentYearMonth)

            'Copy file
            File.Copy(sourceFilePath, destPath, True)
            varcurfname = fileName

            'Upload to database
            uploadtodatabaseauto()

            Console.WriteLine($"File copied via SMB: {fileName}")
        Catch ex As Exception
            'MessageBox.Show($"Error copying file via SMB: {ex.Message}", "SMB Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error copying file via SMB: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Move uploaded file in SMB share
    ''' </summary>
    Private Sub moveuploadedfileSMB(txtFileName As String)
        Try
            Dim uploadedDir As String = smbServerPath & UploadedDirectory & CurrentYearMonth & "\"

            'Create uploaded directory if it doesn't exist
            If Not Directory.Exists(uploadedDir) Then
                Directory.CreateDirectory(uploadedDir)
            End If

            'Move file
            Dim sourcePath As String = smbServerPath & txtFileName
            Dim destPath As String = uploadedDir & txtFileName

            If File.Exists(sourcePath) Then
                File.Move(sourcePath, destPath)
                Console.WriteLine($"File moved via SMB: {txtFileName} -> UPLOADED/{CurrentYearMonth}/")
            End If
        Catch ex As Exception
            'MessageBox.Show($"Error moving file via SMB: {ex.Message}", "SMB Move Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error moving file via SMB: {ex.Message}")
        End Try
    End Sub

    Private Sub copyfilefromftp()
        Try
            Dim buffer(1023) As Byte
            Dim bytesIn As Integer
            Dim totalBytesIn As Integer
            Dim output As IO.Stream

            Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create(ftpServer & outboundDirectory & rcount(loopcount)), System.Net.FtpWebRequest)
            FTPRequest.Credentials = New System.Net.NetworkCredential(ftpUsername, ftpPassword)
            FTPRequest.Method = System.Net.WebRequestMethods.Ftp.DownloadFile

            Dim stream As System.IO.Stream = FTPRequest.GetResponse.GetResponseStream
            output = System.IO.File.Create(localDownloadDirectory & CurrentYearMonth & "\" & rcount(loopcount))
            varcurfname = rcount(loopcount)
            bytesIn = 1
            Do Until bytesIn < 1
                bytesIn = stream.Read(buffer, 0, 1024)
                If bytesIn > 0 Then
                    output.Write(buffer, 0, bytesIn)
                    totalBytesIn += bytesIn
                    'Label1.Text = totalBytesIn.ToString + " Bytes Downloaded"
                    Application.DoEvents()
                End If
            Loop
            output.Close()
            REM upload to database
            uploadtodatabaseauto()
            'deleteftpfile() do not activate code, may have error in deletion of running files

            moveuploadedfile(rcount(loopcount))

            stream.Close()
        Catch ex As WebException
            Dim ftpResponse As FtpWebResponse = CType(ex.Response, FtpWebResponse)
            'MessageBox.Show($"FTP Download Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}" & vbCrLf &
            '              $"File: {rcount(loopcount)}" & vbCrLf &
            '              $"Details: {ex.Message}", "FTP Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"FTP Download Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}")
        Catch ex As Exception
            'MessageBox.Show($"Error copying file '{rcount(loopcount)}': {ex.Message}", "File Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error copying file '{rcount(loopcount)}': {ex.Message}")
        End Try
    End Sub
    Private Sub moveuploadedfile(txtFileName As String)

        Dim ForUploadDirectory As String = ftpServer & outboundDirectory

        FtpFolderCreate(ForUploadDirectory & UploadedDirectory & CurrentYearMonth & "/", ftpUsername, ftpPassword)

        FtpMoveFile(ForUploadDirectory & txtFileName,
                    UploadedDirectory & CurrentYearMonth & "/" & txtFileName,
                    ftpUsername, ftpPassword)
    End Sub
    Private Sub FtpMoveFile(oldPath As String, newPath As String, username As String, password As String)
        Try
            Dim ftpStream As Stream = Nothing
            Dim request As FtpWebRequest = CType(FtpWebRequest.Create(oldPath), FtpWebRequest)
            request.Credentials = New NetworkCredential(username, password)
            request.Method = WebRequestMethods.Ftp.Rename
            request.UseBinary = True
            request.RenameTo = newPath
            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
            ftpStream = response.GetResponseStream()
            'ftpStream.Close()
            'response.Close()
        Catch ex As WebException
            Dim ftpResponse As FtpWebResponse = CType(ex.Response, FtpWebResponse)
            'MessageBox.Show($"FTP Move Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}" & vbCrLf &
            '              $"File: {oldPath}" & vbCrLf &
            '              $"Details: {ex.Message}", "FTP Move Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"FTP Move Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}")
        Catch ex As Exception
            'MessageBox.Show($"Error moving file '{oldPath}': {ex.Message}", "File Move Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error moving file '{oldPath}': {ex.Message}")
        End Try
    End Sub
    Private Function FtpFolderCreate(folder_name As String, username As String, password As String) As Boolean
        Dim request As Net.FtpWebRequest = CType(FtpWebRequest.Create(folder_name), FtpWebRequest)
        request.Credentials = New NetworkCredential(username, password)
        request.Method = WebRequestMethods.Ftp.MakeDirectory

        Try
            Using response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
                ' Folder created
            End Using
        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            ' an error occurred
            If response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                Return False
            End If
        End Try
        Return True
    End Function

    Private Sub uploadtodatabaseauto()
        Dim convweight, convcbm As Double
        Dim inputFile = localDownloadDirectory & CurrentYearMonth & "\" & rcount(loopcount)
        Dim parentmodel As String = ""
        Try
            Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(inputFile)
                MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
                MyReader.Delimiters = New String() {"|"}
                MyReader.HasFieldsEnclosedInQuotes = False
                Dim currentRow As String()
                con.Open()
                cmd.Connection = con

                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()
                        If currentRow.Count >= 0 Then
                            currentRow(0) = currentRow(0)
                        End If
                    Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                        'MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                        Console.WriteLine("Line " & ex.Message & " is invalid. Skipping")
                    End Try

                    If currentRow(0) = "IBD_HEADER" Then
                        varordertype = currentRow(2)
                        Select Case varordertype
                            Case "ZLF"
                                varordertype = "SO"
                            Case "ZLR"
                                varordertype = "SI"
                            Case "NL"
                                varordertype = "RO"
                        End Select
                        varordernumber = currentRow(3)
                        Dim drnumber, ordernumber, customercode, shiptoname, shiptoaddress, orderdate, plant, plantdescription, soldtocode, soldtoname,
                            soldtoaddress, soldtotin, customertext, remarkstext, referencetext, taxclassification, customerpo, vendorcode, container As String
                        drnumber = currentRow(1)
                        ordernumber = currentRow(3)
                        customercode = currentRow(4)
                        shiptoname = currentRow(5)
                        shiptoaddress = currentRow(6)
                        orderdate = currentRow(7)
                        plant = currentRow(8)
                        plantdescription = currentRow(9)
                        soldtocode = currentRow(10)
                        soldtoname = currentRow(11)
                        soldtoaddress = currentRow(12)
                        soldtotin = currentRow(13)
                        customertext = currentRow(14)
                        remarkstext = currentRow(15)
                        referencetext = currentRow(16)
                        taxclassification = currentRow(17)
                        customerpo = currentRow(18)
                        vendorcode = currentRow(19)
                        container = currentRow(20)
                        Dim drnumber1 As String = drnumber.Replace("'", "")
                        Dim ordernumber1 As String = ordernumber.Replace("'", "")
                        Dim customercode1 As String = customercode.Replace("'", "")
                        Dim shiptoname1 As String = shiptoname.Replace("'", "")
                        Dim shiptoaddress1 As String = shiptoaddress.Replace("'", "")
                        Dim orderdate1 As String = orderdate.Replace("'", "")
                        Dim plant1 As String = plant.Replace("'", "")
                        Dim plantdescription1 As String = plantdescription.Replace("'", "")
                        Dim soldtocode1 As String = soldtocode.Replace("'", "")
                        Dim soldtoname1 As String = soldtoname.Replace("'", "")
                        Dim soldtoaddress1 As String = soldtoaddress.Replace("'", "")
                        Dim soldtotin1 As String = soldtotin.Replace("'", "")
                        Dim customertext1 As String = customertext.Replace("'", "")
                        Dim remarkstext1 As String = remarkstext.Replace("'", "")
                        Dim referencetext1 As String = referencetext.Replace("'", "")
                        Dim taxclassification1 As String = taxclassification.Replace("'", "")
                        Dim customerpo1 As String = customerpo.Replace("'", "")
                        Dim vendorcode1 As String = vendorcode.Replace("'", "")
                        Dim container1 As String = container.Replace("'", "")
                        Try
                            cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                                                                    "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                                                                    "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid, DateUploaded) VALUES " &
                                                                    "('" & drnumber1 & "','" & varordertype & "','" & ordernumber1 & "', " &
                                                                    "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " &
                                                                    "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " &
                                                                    "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " &
                                                                    "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " &
                                                                    "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "', GETDATE())"
                            cmd.ExecuteNonQuery()
                        Catch
                        End Try

                        'Double Inserting SO and SI
                        '' Insert "SI" records - 131801 11/12/2018
                        'If varordertype = "SO" Then
                        '    ordertypesi = "SI"
                        '    Try
                        '        cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                        '                                                "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                        '                                                "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid) VALUES " &
                        '                                                "('" & drnumber1 & "','" & ordertypesi & "','" & ordernumber1 & "', " &
                        '                                                "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " &
                        '                                                "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " &
                        '                                                "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " &
                        '                                                "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " &
                        '                                                "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "')"
                        '        cmd.ExecuteNonQuery()
                        '    Catch
                        '    End Try
                        'End If
                        ''' Insert "SI" records - 131801 11/12/2018

                    ElseIf currentRow(0) = "IBD_DETAIL" Then
                        Dim stringreader As String
                        stringreader = Mid(currentRow(3), 1, 5)
                        refid = currentRow(1) & currentRow(2)
                        If currentRow(10) > 0 Then
                            convcbm = FormatNumber(CDbl(currentRow(10) / 1000000), 2)
                        End If
                        convweight = FormatNumber(CDbl(currentRow(8)), 2)
                        Dim itemdesc As String
                        itemdesc = currentRow(4)
                        Dim itemdesc1 As String = itemdesc.Replace("'", "")
                        REM on going CS-CU uploading, position and item scan insertion, tatlo dapat ang insertion na mangyayari = parent 10, child 11, child 12
                        REM                                                              dalawa nalang pala hindi na bukod ng record yung parent
                        If stringreader = "CS/CU" Then

                            Dim itemscan As String = ""
                            Dim intposition As Integer
                            Dim strposition As String

                            itemscan = currentRow(3)
                            itemscan = Mid(currentRow(3), 1, 2) & Mid(currentRow(3), 6, 25)
                            strposition = currentRow(2)
                            intposition = strposition + 1
                            strposition = intposition.ToString().PadLeft(6, "0")
                            refid = currentRow(1) & strposition
                            Try REM insert CS , current row 10 = if > 0 / 1000000
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                            'Double Inserting SO and SI
                            ''' Insert "SI" records - 131801 11/12/2018
                            'If varordertype = "SO" Then
                            '    ordertypesi = "SI"
                            '    Try REM insert CS , current row 10 = if > 0 / 1000000
                            '        cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                            '                            "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                            '                            "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                            '                            "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                            '                            "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                            '                            "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                            '                            "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                            '                            "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                            '                            "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " &
                            '                            "'" & varordernumber & "', '1', '0')"
                            '        cmd.ExecuteNonQuery()
                            '    Catch
                            '    End Try
                            'End If
                            ''' Insert "SI" records - 131801 11/12/2018


                            itemscan = currentRow(3)
                            itemscan = Mid(currentRow(3), 4, 2) & Mid(currentRow(3), 6, 25)
                            intposition = strposition + 1
                            strposition = intposition.ToString().PadLeft(6, "0"c)
                            refid = currentRow(1) & strposition
                            Try REM insert CU, current row 10 = if > 0 / 1000000
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "(' " & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                            'Double Inserting SO and SI
                            ''' Insert "SI" records - 131801 11/12/2018
                            'If varordertype = "SO" Then
                            '    ordertypesi = "SI"
                            '    Try REM insert CU, current row 10 = if > 0 / 1000000
                            '        cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                            '                            "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                            '                            "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                            '                            "(' " & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                            '                            "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                            '                            "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                            '                            "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                            '                            "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                            '                            "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " &
                            '                            "'" & varordernumber & "', '1', '0')"
                            '        cmd.ExecuteNonQuery()
                            '    Catch
                            '    End Try
                            'End If
                            ''' Insert "SI" records - 131801 11/12/2018

                        Else
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                                'updatereferenceid()
                            Catch
                            End Try

                            'Double Inserting SO and SI
                            ''' Insert "SI" records - 131801 11/12/2018
                            'If varordertype = "SO" Then
                            '    ordertypesi = "SI"
                            '    Try
                            '        cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                            '                            "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                            '                            "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                            '                            "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " &
                            '                            "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                            '                            "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                            '                            "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                            '                            "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                            '                            "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " &
                            '                            "'" & varordernumber & "', '1', '0')"
                            '        cmd.ExecuteNonQuery()
                            '        'updatereferenceid()
                            '    Catch
                            '    End Try
                            'End If
                            ''' Insert "SI" records - 131801 11/12/2018

                        End If

                    End If

                End While

                MyReader.Close()
                con.Close()
            End Using
        Catch
        End Try
    End Sub
    Private Sub CreateCurrentYearMonthDirectory()
        Dim CurrentFolder As String
        CurrentYearMonth = GetCurrentYearMonth()
        CurrentFolder = $"{SapDownloadDir}{CurrentYearMonth}\"
        If (Not Directory.Exists(CurrentFolder)) Then
            Directory.CreateDirectory(CurrentFolder)
        End If
    End Sub
    Private Function GetCurrentYearMonth() As String
        Return DateTime.Now.ToString("yyyy-MM")
    End Function







    '************************************ NOT USED CODE IN THE SYSTEM *****************************
    Private Sub UploadToFTP()
        Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://10.87.36.89/SAP-FTP/INBOUND/WMSOB160628182329.PPH"), System.Net.FtpWebRequest)
        FTPRequest.Credentials = New System.Net.NetworkCredential("sapftp1", "ortigas1")
        FTPRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile

        Dim sourceStream As New StreamReader("C:\barcode\sapdownload\WMSOB160628182329.PPH")
        Dim fileContents As Byte() = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd())
        sourceStream.Close()
        FTPRequest.ContentLength = fileContents.Length

        Dim requestStream As Stream = FTPRequest.GetRequestStream()
        requestStream.Write(fileContents, 0, fileContents.Length)
        requestStream.Close()

        Dim response As FtpWebResponse = DirectCast(FTPRequest.GetResponse(), FtpWebResponse)
    End Sub

    Private Sub uploadtodatabasemanual()
        Dim convweight, convcbm As Double
        Dim inputFile = "C:\barcode\sapdownload\test ftp sample data\WMSOB_9627020419_181025170007727.TXT"
        Dim parentmodel As String = ""
        Try
            Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(inputFile)
                MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
                MyReader.Delimiters = New String() {"|"}
                MyReader.HasFieldsEnclosedInQuotes = False
                Dim currentRow As String()
                con.Open()
                cmd.Connection = con

                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()
                        If currentRow.Count >= 0 Then
                            currentRow(0) = currentRow(0)
                        End If
                    Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                        'MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                        Console.WriteLine("Line " & ex.Message & " is invalid. Skipping")
                    End Try

                    If currentRow(0) = "IBD_HEADER" Then
                        varordertype = currentRow(2)
                        Select Case varordertype
                            Case "ZLF"
                                varordertype = "SO"
                            Case "ZLR"
                                varordertype = "SI"
                            Case "NL"
                                varordertype = "RO"
                        End Select
                        varordernumber = currentRow(3)

                        Dim drnumber, ordernumber, customercode, shiptoname, shiptoaddress, orderdate, plant, plantdescription,
                            soldtocode, soldtoname, soldtoaddress, soldtotin, customertext, remarkstext, referencetext, taxclassification, customerpo, vendorcode, container As String
                        drnumber = currentRow(1)
                        ordernumber = currentRow(3)
                        customercode = currentRow(4)
                        shiptoname = currentRow(5)
                        shiptoaddress = currentRow(6)
                        orderdate = currentRow(7)
                        plant = currentRow(8)
                        plantdescription = currentRow(9)
                        soldtocode = currentRow(10)
                        soldtoname = currentRow(11)
                        soldtoaddress = currentRow(12)
                        soldtotin = currentRow(13)
                        customertext = currentRow(14)
                        remarkstext = currentRow(15)
                        referencetext = currentRow(16)
                        taxclassification = currentRow(17)
                        customerpo = currentRow(18)
                        vendorcode = currentRow(19)
                        container = currentRow(20)
                        Dim drnumber1 As String = drnumber.Replace("'", "")
                        Dim ordernumber1 As String = ordernumber.Replace("'", "")
                        Dim customercode1 As String = customercode.Replace("'", "")
                        Dim shiptoname1 As String = shiptoname.Replace("'", "")
                        Dim shiptoaddress1 As String = shiptoaddress.Replace("'", "")
                        Dim orderdate1 As String = orderdate.Replace("'", "")
                        Dim plant1 As String = plant.Replace("'", "")
                        Dim plantdescription1 As String = plantdescription.Replace("'", "")
                        Dim soldtocode1 As String = soldtocode.Replace("'", "")
                        Dim soldtoname1 As String = soldtoname.Replace("'", "")
                        Dim soldtoaddress1 As String = soldtoaddress.Replace("'", "")
                        Dim soldtotin1 As String = soldtotin.Replace("'", "")
                        Dim customertext1 As String = customertext.Replace("'", "")
                        Dim remarkstext1 As String = remarkstext.Replace("'", "")
                        Dim referencetext1 As String = referencetext.Replace("'", "")
                        Dim taxclassification1 As String = taxclassification.Replace("'", "")
                        Dim customerpo1 As String = customerpo.Replace("'", "")
                        Dim vendorcode1 As String = vendorcode.Replace("'", "")
                        Dim container1 As String = container.Replace("'", "")
                        Try
                            cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                                                                     "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                                                                     "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid, DateUploaded) VALUES " &
                                                                     "('" & drnumber1 & "','" & varordertype & "','" & ordernumber1 & "', " &
                                                                     "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " &
                                                                     "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " &
                                                                     "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " &
                                                                     "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " &
                                                                     "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "', GETDATE())"
                            cmd.ExecuteNonQuery()
                        Catch
                        End Try
                    ElseIf currentRow(0) = "IBD_DETAIL" Then
                        Dim stringreader As String
                        stringreader = Mid(currentRow(3), 1, 5)
                        refid = currentRow(1) & currentRow(2)
                        If currentRow(10) > 0 Then
                            convcbm = FormatNumber(CDbl(currentRow(10) / 1000000), 2)
                        End If

                        convweight = FormatNumber(CDbl(currentRow(8)), 2)
                        Dim itemdesc As String
                        itemdesc = currentRow(4)
                        Dim itemdesc1 As String = itemdesc.Replace("'", "")
                        REM on going CS-CU uploading, position and item scan insertion, tatlo dapat ang insertion na mangyayari = parent 10, child 11, child 12
                        REM                                                              dalawa nalang pala hindi na bukod ng record yung parent
                        If stringreader = "CS/CU" Then

                            Dim itemscan As String = ""
                            Dim intposition As Integer
                            Dim strposition As String

                            itemscan = currentRow(3)
                            itemscan = Mid(currentRow(3), 1, 2) & Mid(currentRow(3), 6, 25)
                            strposition = currentRow(2)
                            intposition = strposition + 1
                            strposition = intposition.ToString().PadLeft(6, "0")
                            refid = currentRow(1) & strposition
                            Try REM insert CS
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                            itemscan = currentRow(3)
                            itemscan = Mid(currentRow(3), 4, 2) & Mid(currentRow(3), 6, 25)
                            intposition = strposition + 1
                            strposition = intposition.ToString().PadLeft(6, "0"c)
                            refid = currentRow(1) & strposition
                            Try REM insert CU
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                        Else
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                                'updatereferenceid()
                            Catch
                            End Try
                        End If

                    End If

                End While

                MyReader.Close()
                con.Close()
            End Using
        Catch
        End Try
    End Sub

    Private Sub updatereferenceid()
        Try
            If referencerow = 0 Then
                cmd.CommandText = "insert into tbl_referenceid(referenceid) values ('" & referenceid & "')"
            Else
                cmd.CommandText = "update tbl_referenceid set referenceid='" & referenceid & "'"
            End If
            cmd.ExecuteNonQuery()
        Catch
        End Try
    End Sub

    Private Sub getreferenceid()
        Try
            referencerow = 0
            Dim strSQL As String = "SELECT referenceid FROM tbl_referenceid"
            Dim da As New SqlDataAdapter(strSQL, con)
            Dim ds As New DataSet
            ds.Clear()
            da.Fill(ds, "referenceid")
            referenceid = ds.Tables("referenceid").Rows(0)("referenceid").ToString
            referencerow = ds.Tables("referenceid").Rows.Count
        Catch
            referenceid = 0
            referencerow = 0
        End Try
    End Sub

    Private Sub deleteftpfile()
        'Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://10.87.36.89/SAP-FTP/OUTBOUND/" & varcurfname), System.Net.FtpWebRequest) 'test
        Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://10.87.36.63/SAP-FTP/OUTBOUND/" & varcurfname), System.Net.FtpWebRequest) 'live
        FTPRequest.Credentials = New System.Net.NetworkCredential("sapftp1", "pphsap01") 'live
        'FTPRequest.Credentials = New System.Net.NetworkCredential("sapftp1", "ortigas1") 'test
        FTPRequest.Method = System.Net.WebRequestMethods.Ftp.DeleteFile
        Dim stream As System.IO.Stream = FTPRequest.GetResponse.GetResponseStream
    End Sub
    Private Sub uploadtodatabase()

        Dim inputFile = "C:\barcode\sapdownload\" & rcount(loopcount)
        Dim parentmodel As String = ""
        Try
            Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(inputFile)
                MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
                MyReader.Delimiters = New String() {"|"}
                Dim currentRow As String()
                con.Open()
                cmd.Connection = con

                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()
                        If currentRow.Count >= 0 Then
                            currentRow(0) = currentRow(0)
                        End If
                    Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                        'MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                        Console.WriteLine("Line " & ex.Message & " is invalid. Skipping")
                    End Try

                    If currentRow(0) = "IBD_HEADER" Then
                        varordertype = currentRow(2)
                        Select Case varordertype
                            Case "ZLF"
                                varordertype = "SO"
                            Case "ZLR"
                                varordertype = "SI"
                            Case "NL"
                                varordertype = "RO"
                        End Select
                        varordernumber = currentRow(3)
                        Try
                            cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                                                                    "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                                                                    "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, DateUploaded) VALUES " &
                                                                    "('" & currentRow(1) & "','" & varordertype & "','" & currentRow(3) & "', " &
                                                                    "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                                    "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & currentRow(18) & "', GETDATE())"
                            cmd.ExecuteNonQuery()
                        Catch
                        End Try

                        ''' Insert "SI" records - 131801 11/12/2018
                        'If varordertype = "SO" Then
                        '    Try
                        '        ordertypesi = "SI"
                        '        cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                        '                                                "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                        '                                                "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO) VALUES " &
                        '                                                "('" & currentRow(1) & "','" & ordertypesi & "','" & currentRow(3) & "', " &
                        '                                                "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                        '                                                "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                        '                                                "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                        '                                                "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                        '                                                "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & currentRow(18) & "')"
                        '        cmd.ExecuteNonQuery()
                        '    Catch
                        '    End Try
                        'End If
                        ''' Insert "SI" records - 131801 11/12/2018

                    ElseIf currentRow(0) = "IBD_DETAIL" Then
                        'Dim remainder As Integer
                        'Dim splittype As String = Mid(currentRow(3), 1, 4)
                        'splittype = splittype
                        'remainder = currentRow(2) Mod 10

                        REM 
                        'If remainder <> 0 Then
                        '    REM insert to baanreference 11 - 12 cs/u
                        'ElseIf remainder = 0 And splittype <> "CS/U" Then
                        '    REM insert to baanreference not cs/u and 10 20 30, other mo    del NR
                        'ElseIf remainder = 0 Then
                        '    REM copy parent model ignore inserting
                        '    parentmodel = currentRow(3)
                        'End If
                        'getreferenceid()
                        'referenceid += 1



                        Try
                            cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                "totalvolume, volumeunit, totalweight, weightunit, materialgroup, materialgroupdescription, materialtype, " &
                                                "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', " &
                                                "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & referenceid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                "'" & varordernumber & "', '1', '0', GETDATE())"
                            cmd.ExecuteNonQuery()
                            'updatereferenceid()
                        Catch
                        End Try

                        '' Insert "SI" records - 131801 11/12/2018
                        If varordertype = "SO" Then
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalvolume, volumeunit, totalweight, weightunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb, DateUploaded) VALUES" &
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', " &
                                                    "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & referenceid & "', 'TDC', '7960', '" & ordertypesi & "', " &
                                                    "'" & varordernumber & "', '1', '0', GETDATE())"
                                cmd.ExecuteNonQuery()
                                'updatereferenceid()
                            Catch
                            End Try
                        End If
                        '' Insert "SI" records - 131801 11/12/2018

                    End If

                End While

                MyReader.Close()
                con.Close()
            End Using
        Catch
        End Try
    End Sub
End Class
