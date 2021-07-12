Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Text
Public Class Form1
    '*****************             T E S T             *****************
    Dim con As New SqlConnection("Data Source=137.40.16.100;Initial Catalog=barcode_db_test;User Id=sa;Password=Pmpc2020")  'test
    Dim ftpServer As String = "ftp://137.40.28.24/"                                                                         'test
    Dim ftpUsername As String = "sapftp1"                                                                                   'test
    Dim ftpPassword As String = "pphsap01"                                                                                  'test
    Dim outboundDirectory As String = "SAP-FTP/TEST/OUTBOUND/"                                                              'test
    Dim localDownloadDirectory As String = "C:\barcode\TEST\sapdownload\"                                                   'test
    '*****************             T E S T             *****************


    '*****************             L I V E             *****************
    'Dim con As New SqlConnection("Data Source=137.40.28.22;Initial Catalog=forSAP_test;User Id=sa;Password=Delivers4U") 'live
    'Dim ftpServer As String = "ftp://137.40.28.24/"                        'added alvz 20210713
    'Dim ftpUsername As String = "sapftp1"                                  'sadded alvz 20210713
    'Dim ftpPassword As String = "pphsap01"                                 'added alvz 20210713
    'Dim outboundDirectory As String = "SAP-FTP/OUTBOUND/"                  'added alvz 20210713
    'Dim localDownloadDirectory As String = "C:\barcode\sapdownload\"       'added alvz 20210713
    '*****************             L I V E             *****************







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
        Button1.PerformClick()
        End
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ftpServer & outboundDirectory), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.ListDirectory
        request.Credentials = New NetworkCredential(ftpUsername, ftpPassword)
        Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
        Dim responseStream As Stream = response.GetResponseStream()
        Dim reader As New StreamReader(responseStream)

        REM AUTO live
        strlabel = (reader.ReadToEnd())    'live
        rcount = Split(strlabel, vbCrLf) REM row count

        For Me.loopcount = 0 To CInt(rcount.Length) - 1
            If rcount(loopcount).Contains("WMSOB") Then REM And rcount(loopcount).Contains(".PPH") Then
                REM copy file to disk
                copyfilefromftp()
            Else
                REM no uploadingader As New StreamReader(responseStream)

                REM manual upload test
                ''uploadtodatabasemanual()    'test

            End If
        Next
        count = strlabel.Count REM length
        Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription)
        reader.Close()
        response.Close()
    End Sub


    Private Sub copyfilefromftp()
        Dim buffer(1023) As Byte
        Dim bytesIn As Integer
        Dim totalBytesIn As Integer
        Dim output As IO.Stream

        Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create(ftpServer & outboundDirectory & rcount(loopcount)), System.Net.FtpWebRequest)
        FTPRequest.Credentials = New System.Net.NetworkCredential(ftpUsername, ftpPassword)
        FTPRequest.Method = System.Net.WebRequestMethods.Ftp.DownloadFile

        Dim stream As System.IO.Stream = FTPRequest.GetResponse.GetResponseStream
        output = System.IO.File.Create(localDownloadDirectory & rcount(loopcount))
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

        stream.Close()
    End Sub


    Private Sub uploadtodatabaseauto()
        Dim convweight, convcbm As Double
        Dim inputFile = localDownloadDirectory & rcount(loopcount)
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
                        MsgBox("Line " & ex.Message & " is invalid.  Skipping")
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
                        Dim drnumber, ordernumber, customercode, shiptoname, shiptoaddress, orderdate, plant, plantdescription, soldtocode, soldtoname, _
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
                            cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," & _
                                                                    "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," & _
                                                                    "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid) VALUES " & _
                                                                    "('" & drnumber1 & "','" & varordertype & "','" & ordernumber1 & "', " & _
                                                                    "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " & _
                                                                    "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " & _
                                                                    "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " & _
                                                                    "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " & _
                                                                    "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "')"
                            cmd.ExecuteNonQuery()
                        Catch
                        End Try

                        '' Insert "SI" records - 131801 11/12/2018
                        If varordertype = "SO" Then
                            ordertypesi = "SI"
                            Try
                                cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," & _
                                                                        "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," & _
                                                                        "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid) VALUES " & _
                                                                        "('" & drnumber1 & "','" & ordertypesi & "','" & ordernumber1 & "', " & _
                                                                        "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " & _
                                                                        "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " & _
                                                                        "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " & _
                                                                        "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " & _
                                                                        "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "')"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try
                        End If
                        '' Insert "SI" records - 131801 11/12/2018

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
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " & _
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " & _
                                                    "'" & varordernumber & "', '1', '0')"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                            '' Insert "SI" records - 131801 11/12/2018
                            If varordertype = "SO" Then
                                ordertypesi = "SI"
                                Try REM insert CS , current row 10 = if > 0 / 1000000
                                    cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                        "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                        "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                        "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " & _
                                                        "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                        "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                        "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                        "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                        "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " & _
                                                        "'" & varordernumber & "', '1', '0')"
                                    cmd.ExecuteNonQuery()
                                Catch
                                End Try
                            End If
                            '' Insert "SI" records - 131801 11/12/2018


                            itemscan = currentRow(3)
                            itemscan = Mid(currentRow(3), 4, 2) & Mid(currentRow(3), 6, 25)
                            intposition = strposition + 1
                            strposition = intposition.ToString().PadLeft(6, "0"c)
                            refid = currentRow(1) & strposition
                            Try REM insert CU, current row 10 = if > 0 / 1000000
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                    "(' " & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " & _
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " & _
                                                    "'" & varordernumber & "', '1', '0')"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                            '' Insert "SI" records - 131801 11/12/2018
                            If varordertype = "SO" Then
                                ordertypesi = "SI"
                                Try REM insert CU, current row 10 = if > 0 / 1000000
                                    cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                        "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                        "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                        "(' " & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " & _
                                                        "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                        "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                        "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                        "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                        "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " & _
                                                        "'" & varordernumber & "', '1', '0')"
                                    cmd.ExecuteNonQuery()
                                Catch
                                End Try
                            End If
                            '' Insert "SI" records - 131801 11/12/2018

                        Else
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " & _
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " & _
                                                    "'" & varordernumber & "', '1', '0')"
                                cmd.ExecuteNonQuery()
                                'updatereferenceid()
                            Catch
                            End Try

                            '' Insert "SI" records - 131801 11/12/2018
                            If varordertype = "SO" Then
                                ordertypesi = "SI"
                                Try
                                    cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " & _
                                                        "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " & _
                                                        "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" & _
                                                        "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " & _
                                                        "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " & _
                                                        "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " & _
                                                        "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " & _
                                                        "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " & _
                                                        "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & ordertypesi & "', " & _
                                                        "'" & varordernumber & "', '1', '0')"
                                    cmd.ExecuteNonQuery()
                                    'updatereferenceid()
                                Catch
                                End Try
                            End If
                            '' Insert "SI" records - 131801 11/12/2018

                            End If

                        End If

                End While

        MyReader.Close()
        con.Close()
            End Using
        Catch
        End Try
    End Sub







    '************************************ NOT USED CODE IN THE SYSTEM *****************************
    Private Sub UploadToFTP()
        Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://137.40.28.90/SAP-FTP/INBOUND/WMSOB160628182329.PPH"), System.Net.FtpWebRequest)
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
                        MsgBox("Line " & ex.Message & " is invalid.  Skipping")
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
                                                                     "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO, vendorcode, containerid) VALUES " &
                                                                     "('" & drnumber1 & "','" & varordertype & "','" & ordernumber1 & "', " &
                                                                     "'" & customercode1 & "', '" & shiptoname1 & "', '" & shiptoaddress1 & "', " &
                                                                     "'" & orderdate1 & "', '" & plant1 & "', '" & plantdescription1 & "', " &
                                                                     "'" & soldtocode1 & "', '" & soldtoname1 & "', '" & soldtoaddress1 & "', " &
                                                                     "'" & soldtotin1 & "', '" & customertext1 & "', '" & remarkstext1 & "', " &
                                                                     "'" & referencetext1 & "', '" & taxclassification1 & "', '" & customerpo1 & "', '" & vendorcode1 & "', '" & container1 & "')"
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
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0')"
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
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                                                    "('" & currentRow(1) & "', '" & strposition & "', '" & currentRow(3) & "', '" & itemscan & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0')"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try

                        Else
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemdocn, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalweight, weightunit, totalvolume, volumeunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', '" & currentRow(3) & "', " &
                                                    "'" & itemdesc1 & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & convcbm & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & refid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                    "'" & varordernumber & "', '1', '0')"
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
        'Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://137.40.28.90/SAP-FTP/OUTBOUND/" & varcurfname), System.Net.FtpWebRequest) 'test
        Dim FTPRequest As System.Net.FtpWebRequest = DirectCast(System.Net.WebRequest.Create("ftp://137.40.28.24/SAP-FTP/OUTBOUND/" & varcurfname), System.Net.FtpWebRequest) 'live
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
                        MsgBox("Line " & ex.Message & " is invalid.  Skipping")
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
                                                                    "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO) VALUES " &
                                                                    "('" & currentRow(1) & "','" & varordertype & "','" & currentRow(3) & "', " &
                                                                    "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                                    "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & currentRow(18) & "')"
                            cmd.ExecuteNonQuery()
                        Catch
                        End Try

                        '' Insert "SI" records - 131801 11/12/2018
                        If varordertype = "SO" Then
                            Try
                                ordertypesi = "SI"
                                cmd.CommandText = "INSERT INTO tbl_SAPHeader(DRNumber, DeliveryType, OrderNumber, CustomerCode, ShipToName, ShipToAddress," &
                                                                        "OrderDate, Plant, PlantDescription, SoldToCode, SoldToName, SoldToAddress, SoldToTin," &
                                                                        "CustomerText, RemarksText, ReferenceText, TaxClassification, CustomerPO) VALUES " &
                                                                        "('" & currentRow(1) & "','" & ordertypesi & "','" & currentRow(3) & "', " &
                                                                        "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                                        "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                                        "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                                        "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                                        "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & currentRow(18) & "')"
                                cmd.ExecuteNonQuery()
                            Catch
                            End Try
                        End If
                        '' Insert "SI" records - 131801 11/12/2018

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
                                                "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                                                "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', " &
                                                "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & referenceid & "', 'TDC', '7960', '" & varordertype & "', " &
                                                "'" & varordernumber & "', '1', '0')"
                            cmd.ExecuteNonQuery()
                            'updatereferenceid()
                        Catch
                        End Try

                        '' Insert "SI" records - 131801 11/12/2018
                        If varordertype = "SO" Then
                            Try
                                cmd.CommandText = "INSERT INTO baanreference(pickernumber, position, itemscan, itemdescription, warehouse, quantity, salesunit, " &
                                                    "totalvolume, volumeunit, totalweight, weightunit, materialgroup, materialgroupdescription, materialtype, " &
                                                    "wbs, convertedquantity, convertedunit, refid, distributioncenter, company, ordertype, ordernumber, ordersequence, srnb) VALUES" &
                                                    "('" & currentRow(1) & "', '" & currentRow(2) & "', '" & currentRow(3) & "', " &
                                                    "'" & currentRow(4) & "', '" & currentRow(5) & "', '" & currentRow(6) & "', " &
                                                    "'" & currentRow(7) & "', '" & currentRow(8) & "', '" & currentRow(9) & "', " &
                                                    "'" & currentRow(10) & "', '" & currentRow(11) & "', '" & currentRow(12) & "', " &
                                                    "'" & currentRow(13) & "', '" & currentRow(14) & "', '" & currentRow(15) & "', " &
                                                    "'" & currentRow(16) & "', '" & currentRow(17) & "', '" & referenceid & "', 'TDC', '7960', '" & ordertypesi & "', " &
                                                    "'" & varordernumber & "', '1', '0')"
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
