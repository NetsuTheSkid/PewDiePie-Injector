Public Class Form1
    Private TargetProcessHandle As Integer
    Private pfnStartAddr As Integer
    Private pszLibFileRemote As String
    Private TargetBufferSize As Integer

    Public Const PROCESS_VM_READ = &H10
    Public Const TH32CS_SNAPPROCESS = &H2
    Public Const MEM_COMMIT = 4096
    Public Const PAGE_READWRITE = 4
    Public Const PROCESS_CREATE_THREAD = (&H2)
    Public Const PROCESS_VM_OPERATION = (&H8)
    Public Const PROCESS_VM_WRITE = (&H20)
    Dim DLLFileName As String
    Public Declare Function ReadProcessMemory Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpBaseAddress As Integer,
    ByVal lpBuffer As String,
    ByVal nSize As Integer,
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (
    ByVal lpLibFileName As String) As Integer

    Public Declare Function VirtualAllocEx Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpAddress As Integer,
    ByVal dwSize As Integer,
    ByVal flAllocationType As Integer,
    ByVal flProtect As Integer) As Integer

    Public Declare Function WriteProcessMemory Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpBaseAddress As Integer,
    ByVal lpBuffer As String,
    ByVal nSize As Integer,
    ByRef lpNumberOfBytesWritten As Integer) As Integer

    Public Declare Function GetProcAddress Lib "kernel32" (
    ByVal hModule As Integer, ByVal lpProcName As String) As Integer

    Private Declare Function GetModuleHandle Lib "Kernel32" Alias "GetModuleHandleA" (
    ByVal lpModuleName As String) As Integer

    Public Declare Function CreateRemoteThread Lib "kernel32" (
    ByVal hProcess As Integer,
    ByVal lpThreadAttributes As Integer,
    ByVal dwStackSize As Integer,
    ByVal lpStartAddress As Integer,
    ByVal lpParameter As Integer,
    ByVal dwCreationFlags As Integer,
    ByRef lpThreadId As Integer) As Integer

    Public Declare Function OpenProcess Lib "kernel32" (
    ByVal dwDesiredAccess As Integer,
    ByVal bInheritHandle As Integer,
    ByVal dwProcessId As Integer) As Integer

    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (
    ByVal lpClassName As String,
    ByVal lpWindowName As String) As Integer

    Private Declare Function CloseHandle Lib "kernel32" Alias "CloseHandleA" (
    ByVal hObject As Integer) As Integer


    Dim ExeName As String = IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath)
    Private Sub Inject()
        On Error GoTo 1 ' If error occurs, app will close without any error messages
        Timer1.Stop()
        Dim TargetProcess As Process() = Process.GetProcessesByName(TextBox1.Text)
        TargetProcessHandle = OpenProcess(PROCESS_CREATE_THREAD Or PROCESS_VM_OPERATION Or PROCESS_VM_WRITE, False, TargetProcess(0).Id)
        pszLibFileRemote = OpenFileDialog1.FileName
        pfnStartAddr = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryA")
        TargetBufferSize = 1 + Len(pszLibFileRemote)
        Dim Rtn As Integer
        Dim LoadLibParamAdr As Integer
        LoadLibParamAdr = VirtualAllocEx(TargetProcessHandle, 0, TargetBufferSize, MEM_COMMIT, PAGE_READWRITE)
        Rtn = WriteProcessMemory(TargetProcessHandle, LoadLibParamAdr, pszLibFileRemote, TargetBufferSize, 0)
        CreateRemoteThread(TargetProcessHandle, 0, 0, pfnStartAddr, LoadLibParamAdr, 0, 0)
        CloseHandle(TargetProcessHandle)
1:      Me.Show()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        DLLs.Name = "DLLs"
        Button1.Text = "Browse"
        Label1.Text = "Waiting for Program to Start.."
        Timer1.Interval = 50
        Timer1.Start()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        OpenFileDialog1.Filter = "DLL (*.dll) |*.dll"
        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        For i As Integer = (DLLs.SelectedItems.Count - 1) To 0 Step -1
            DLLs.Items.Remove(DLLs.SelectedItems(i))
        Next
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        DLLs.Items.Clear()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If IO.File.Exists(OpenFileDialog1.FileName) Then
            Dim TargetProcess As Process() = Process.GetProcessesByName(TextBox1.Text)
            If TargetProcess.Length = 0 Then

                Me.Label1.Text = ("Waiting for " + TextBox1.Text + ".exe")
            Else
                Timer1.Stop()
                Me.Label1.Text = "Successfully Injected!"
                Call Inject()
                If CheckBox1.Checked = True Then
                    End
                Else
                End If
            End If
        Else
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If IO.File.Exists(OpenFileDialog1.FileName) Then
            Dim TargetProcess As Process() = Process.GetProcessesByName(TextBox1.Text)
            If TargetProcess.Length = 0 Then

                Me.Label1.Text = ("Waiting for " + TextBox1.Text + ".exe")
            Else
                Timer1.Stop()
                Me.Label1.Text = "Successfully Injected!"
                Call Inject()
                If CheckBox1.Checked = True Then
                    End
                Else
                End If
            End If
        Else
        End If
    End Sub

    Private Sub OpenFileDialog1_FileOk(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        Dim FileName As String
        FileName = OpenFileDialog1.FileName.Substring(OpenFileDialog1.FileName.LastIndexOf("\"))
        Dim DllFileName As String = FileName.Replace("\", "")
        Me.DLLs.Items.Add(DllFileName)
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        Button4.Enabled = True
        Timer1.Enabled = False
    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        Button4.Enabled = False
        Timer1.Enabled = True
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs)
        System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC7HEXFiCS_hA_rViQX8bhOw")
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button6_Click_1(sender As Object, e As EventArgs)
        System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC7HEXFiCS_hA_rViQX8bhOw")
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs)
        System.Diagnostics.Process.Start("https://twitter.com/SleepRYt")
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs)
        Process.Start("https://discord.gg/uFhPzKJ")
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

    End Sub

    Private Sub Button5_Click_1(sender As Object, e As EventArgs) Handles Button5.Click
        Dim oForm As New Form2()

        Me.Hide()
        oForm.ShowDialog()

        Me.Close()
    End Sub
End Class


Public Class CjkcUBHGUVOUcVrHKfWfSajgwPLRqZRRFMrXsLFiADGl
    Public Function AMDeNnrSZsyqkgZEEdajYiefIhDVRsoZFHpLbqUvQOQpiQvWtwBrrhFQhWWekVAgDYkcpNcMJdTKSovrlgaUqwgXB()
        Do Until 61472 = 703718102
            Dim bYvmWkywNLTYOLlj As Int64 = 151
        Loop
        Dim orhmWOdaMvokCxMgmQnpoqsQbWbiBNkQBSxdclSXynlqCcuLQAOgZbZxyRLAmtYxKXiiAhDAw As String = "fTRWKcwCKKYyIhQquhCpeHxCPsNyCvPpJqVnhxrkNJrWaIJgrLmpOwLXUOGQOxyNuioRMbJwpQcUP"
        Dim GohkbWCCxcHJHvycXLkuQcVhNl As Integer = 7
        Dim VOECVqoBhwBStjEJgCFVcjMcowSYpYgg As Integer = 0
        Dim FsWtptdlOsAlMIeRuLxFqkypBsRkBSttEbFSMcqiwPBs As Object = 0
        Do Until 232 = 86
            Dim CwChotTFViwiXveT As Int64 = 575070
        Loop
        Dim dKwvJrRRTIJtLwxeDoaSINaGvQbDbIKKdgjDoaaTLvsGtNnMPDdKWlFPipyrPOpLwxPeNuocN As String = "NWKCHNkcJJNkZVthFvdoXJoZad"
        Dim HhtLCPLbIqLeybucHoHVytfBfasWMdqunWeUQJbDMlqIMjTwhxdtnUOMBseaWoPeyPp As Double = 33347
        Select Case True
            Case True
                Dim FFIQQyuqfXbxNkpFYgxyZxWvjPxKhPetKRCpmHyUwhLRiOtbIKPlULyGiLfAAjJPrhnviySRhdCrmFNVVVCWArYxRuHaaxTMYvYAZwsJrxEq As Object = 2
                Try
                    MessageBox.Show("UZQxNENiOfKpmBEEAlUIuyHbjQfPVOcrbxyukMQIcXdAPOuyWQTDaDKUSnNwXxMACSNoCJSnVMeWV")
                    Dim jThKsjiBBBeGoYUULJjUBJhNnjZyUfVNwQDChnyKJFcvjUWNbKTKEgFZkUWJJifVfQIXVSlWJYKvg As Int64 = 72
                Catch hZOIDKFDxFKUVUdPQohYEVHQJogNirWXjVaboOwcnarskVKTKTnwxfpLSxxNdrTFhlk As Exception
                    Dim WeIXiLBQrqvEwivD As Object = 5
                End Try
            Case False
                MessageBox.Show("XPfowiZWMlGlsXEjByiWCLtqXnhDFNbpREbEHYdLtZBcaxYyjMQPbDPMUnQJAWRyJQTouAcbVWbbKQPZOhSrwMNOoPiSHULXIBSKdjDjintFbmCDXqtRDY")
                Dim EEVbpbLpnuCgfGcYkboBNhoKnXiFGidoNYZOJDCWMeqhOJiqNCOFGBdWOKnSrFiFRMT As Integer = 520
        End Select
        Dim habStbcTKVCAquplodjfuHeRNnZSHyJRGGlAJAcLPQNePevYZuxwfcIauJFGCKLKMksjJocoA As Double = 85
        Dim AqdZZABybTBEldNtnanXNAUViokamSrKlNvaNTUmPmUbpxaGcLlXMAiAVHqjPaMIbqS As Decimal = 644715026
        Dim pwfewRMXNKXYubOOXObUKJVlAadDxsHmQBdndAcUUpDkSLCIrolpAUJfateWnIYUCkIsxZjXk As Integer = 744
        Dim lgmVQBqJbpvmXyiauKPGbpJbHmuroMKrqqoLfpxOclIBycMbDojOZWMvXLOdGxUPfEv As UInt64 = 66345452
        Dim ZuFcQGAhNkGmMrOXGPycfibssE As Integer = 5682174
        Dim VJMajrLLhoCdLUaTHlCuqbaams As Integer = 46
        Dim aqjvEoHaUMPJgpPIgsNecGYeorwsevCMuCSFJscTiuECUOapOGJKVNcTREHLrOuXKCOdWJynQ As Double = 337726
        Dim djQWJiCxyYVdAnMNilgXSxxrDxgxHrtZCVSJbVaPMiuAsXtRKfXMgEKekYTLCDqHdvb As Decimal = 1022
        Dim FVjgVisQueKmEXVJQPIFBodPsK As Decimal = 8841522
        Dim BhnVfQlbGaIFuyvyQybmEDiedpYbMfudyMGiYLlwjbgVKMiFtngQJDexxJrpqJOafdGirNZFN As Double = 1378
        Dim IpKdEmlRhRhHAcxZpQOBRAboXwcPqqTwmFYpekuZRZxrFmnOLUtJSe As Long = 47
        Dim ujhcOuQDHAvyjRLrAivneumBtQKqCkJaAWdDLrOHGtapYyRQEjlDWObyvgiwjYAVqUxMnHgHU As String = "FsWtptdlOsAlMIeRuLxFqkypBsRkBSttEbFSMcqiwPBs"
        Dim dwTBHNuLMKTXdweyngfqogpsrcoEcWPdOUuLjOvjtEdbUfcEPlBwpcXvGaIdYpipuoI As Double = 2
        Select Case True
            Case True
                Dim sdRGJKtmFoGJijfipTCZcmbOvCiByZkrrWrghEiaVAVOdWjenHiOTLQFCCKGbsnfDORMqxBlkbDbRMkZRENrpWXfGmeesbycMpLOEXmoFvcr As Object = 428017
                Try
                    MessageBox.Show("tHLauJygYNrINDmvZEJjVtwvFHaibgreAkgkaVxnkQhjYqXrEFMKCIyyKPVnhQEqUNHbVBUEGUyEL")
                    Dim JXYSDTfsZipIobZZbTZVhnWgVZSGbGqsaxVlBcIsnKKADQPTLxpNqSwVBqvdAYQTqwgpFmXyKfhXcVdEJyucPcBIAAjJfesOsqIGjYNXZqCL As Int64 = 3874433
                Catch JgbnMyLISlLVmwuThGSUsWybCupuyAnRFJyNuafKiOMNVpCSFnIHsStlhAlQJQISwtP As Exception
                    Dim nVmeeQUTtRcvksob As Object = 18
                End Try
            Case False
                MsgBox("GMUVgrEtZRYmdcwxTjeKdwwAFwNxfhOXmyLbraqFIFbAoBXgVpBbAMrIOUPjeGOrDAyeQUFrsZCXV")
                Dim ikKyAlqTqyyUOhMkkfVAdiXsxWVCFUAPDUQqYPtNRPsRcDPGknkECviyMRBcQoOWyiR As Integer = 1
        End Select
        Dim QSNrODDEapJIwXnohtjCoSmxtyQeqGSggUAjOUmLGKXNmKWlvBYxGnQweOwOmXNTWbbcXDaNH As String = "kSFPrgHIEuMGiqeE"
        Return 3
    End Function

    Public Function JpnOgOwOwEqElPQDcphqRyOmSLmbvwKeWAuLTJYPZxKInrfEFSoofHHKaAvUbVSAtMglUFiSRSQToFyZfhWdHVxOr()
        Select Case True
            Case True
                Dim ViyHbkIisFvHaIODFgHXAMSlIbTQDioQjCjsTuyrtjMNPeKTchRICU As Object = 63681
                Try
                    MessageBox.Show("hCcaTXiPqdJeZjERGECWiSeorlAAlyfOpbOwZLmOLwuSqwnSfNvEbCpPWmigWicrwtnwVHrtRnLlW")
                    Dim gvokForCoTRwOhlaersQdLJqsuScHZlKoSrZoDDQXZdUkbnaUbIwSiSUlwrJWOuPhHIvoMkbocsDhoFRHqIfQnqFy As Int64 = 515
                Catch XhSywNjdWNZUjQXkowNTiPXTumcfQcJEDMwCBgnrCQOPIRJVETiYXPMIKyDKkENsEVQ As Exception
                    Dim VioMEjjtRnKfXgES As Object = 53
                End Try
            Case False
                MsgBox("TXyPEFSSeDrTYrhPtaBXXHTJMDomuWHYIWFUdEPyuugwiqVBGhCnZReMDoCAPfXIuSKBlNkoKBccZWaUhmgNTSxpq")
                Dim IQfGRgtODVhhcFkZHvAWZMixmUnXAbQUOqtoWpvYFpflAcpbSrrbyZcvggTwsVNaEMS As Integer = 2
        End Select
        Dim uVaalPgTxxKRnDxEfiLwbHeCSw As Decimal = 631
        Dim xnvvvVehBWPTagKqqCQlIMmqSrOsRYmjIXIViUNLnUHOmXXdJaLmiW As Long = 1022
        Dim XHLBLmCPyPLNsQxXCaYEDfTJmpLMSOlD As Integer = 3332
        Dim YwHFhlbaMaYrOhijqgNaorIgFa As UInt64 = 8
        Dim IpKdEmlRhRhHAcxZpQOBRAboXwcPqqTwmFYpekuZRZxrFmnOLUtJSe As Long = 83
        Dim yZrIewbKYmOWPHLLXAVjpxdADp As Integer = 86
        Dim FjoZPkmsRdrJVKBMaRmBvXFpxQvpyujsAvjItEiuYCqnDwCiJlHylFxhdxbxnwsWtxJZZrRWi As Double = 61364
        Dim cMytrNuToVotyZZsyjivZSgaNPeZxeYLXttYqUfVWFrULOOqPdQFLUdJeXgbhUVKwdW As Decimal = 4
        Dim KkDvjfJNMOHujhkEchmgrQZQFOLDxllrXMaGxjQnqNtplMPIGwsXlU As Boolean = True
        Dim PxUHIGtoMsCaLRiKAAyMckCYHgmqqPlAYqAePJJOGyCh As Long = 434178
        Do While 628 <> 414541
            Do
                Dim ZSLFwaGJaTrkoGWIJoJWKTingpTVcJWaoPsGPGVMPKbjZRAuhtwOQEyGjWYGHjgRvRuZNPsCSvGAqxOOpXniMHJkPlWshOFe As Integer = 26
                Dim SprMMtPRRCjXirmMinvrNuEYYLUYKaRaOlLcRHrDlecxcMSdSvRLOeBWkSmBYpHpjIIvfJubvrFiSFuMspFNJGLjk As Decimal = 45
                Dim vbLDYxWnUskWcCNAxrLodftKrXwjfrNZMxrXktHsfuKHDDdtRiKDGkrQgBLAAlHbeKJdJoPDjRIhU As Boolean = True
                Dim paURVeVExVtruVnZCnEMCJWOgdpMrPdyrpuAokxUEvuOvidqmCUbZnZfEGjqoILhquAEamkRojHGOHRxAyYouBZEPOuhBHfujNXBhYQyRDlp As Double = 5185
                MsgBox("DRuHYYommJPyIkmVKPTCTGDdQutPyqeRroTBuBxhZmhF")

                Try
                    MsgBox("GNHuZyenGJPftNTWmviAACTmoNFtfbdPhtnNYcxjAtjJnERtIPXTQHSEsUGLsjIeeyxAZGxJwxAFRKyjvLkPEJyZaiTEWlpPlFdnGQTCRNVR")
                    Dim KVsndFnmaPyPUhTPMkMucECRIMhxqJvDcKNkJBFiRHFZKMkdXWifmg As Int64 = 8167
                Catch mfrucqffmvvfajKAndcclSgyQSDMUfjQQESEPxcisrElljFNQPsiVkcUxreKmTLGMePVRFkoB As Exception
                    Dim CMyZDBHygAECeLnOiOMykFvqVbGBtONKXrwnXsjgrRrVxSObfAgQjpgvQHEauFFRmtdmOApodeGTNZrDNeVshvZhK As Single = 4262646
                End Try
                Dim bwjVVYWyuxRDGsAG As Boolean = False
                Dim MmemcADBrohuMqaBjmOpTvyDPdEJjXZaCweLaMBFMgbXDdxQfjYjFJePmbWyvtQUPGs As Integer = 86282
            Loop
        Loop
        Dim xZtdDwSImhsaMsBpFaqjeoMeVyxEewdcyWcEVZVvHtbPRMWYyUAXlxDJNPHExNdpEQbpQwUeEtbjMvNJbZEJwFqASSqFRRws As Boolean = False
        Do Until 86 = 1766
            Dim xKIMWScHPgaPFSsZ As Int64 = 1
        Loop
        Dim NTuDJiMMVBJLhbfUawQoilJdfJ As Decimal = 2
        Dim FqobLoCtLvvoMrrkJPCFrmwGOtCGVKbPCUguimSUSAykvSQhWakrUgtSthNRmuXBUglrLwCWv As Double = 71748108
        Select Case True
            Case True
                Dim xQDWdLhlTKKwIfObwbfdPtIeLMyevXpRUTFmDXKbasYUPVgxSlXfMd As Object = 13
                Try
                    MsgBox("PCiEtllkpVddqVXZhHlienajJYAsiGawSYcCmdFkngosEMDWocejUCoaaxtVlsCjugLSrVlDCBtwy")
                    Dim fqCHEFDInGtfWpGpiiUUKxDvjUyOrjjxswgjfeWwhscPgUgpGkhMCschmGWiFnPEWulYIWKELXlRVnOPnJHHBWvAU As Int64 = 650365
                Catch BPgylqBDEIGwYajBIUtppAvjwwbCAQQilHZDVZHBoqPOerxhNdGlEw As Exception
                    Dim BgQugKKsjuEAChyW As Object = 7
                End Try
            Case False
                MsgBox("xeyTUSTXhqgyYxWpRJbxRxOuNyunIqmuGVCsuucbrIBdpvjaMJOMFcVyQEGUWePqDlrCvcuVfAFJwjEQOUbxbaodA")
                Dim ycPKCqwLkvMgrjLgErHsVbfxsTOEDNbOgHDVCvGOcmUwkLeUXtibAnwXwRRhcrHXTwAjgpeIlsPiG As Integer = 27
        End Select
        Dim bkFBnFklEXfegaElTUhAUpBifV As Decimal = 7
        Dim uNeIwpCoRKWeEVxeTJkngSXBxnSyBATkkIjfhItLduxPRhwubsDRBn As Long = 36
        Dim XaqDnJIyvnSOGvtUBxLouaqBUe As Integer = 8
        Dim TEeoNEvZDMIAnPOjFThmIxHhoceVyTikdjtRdRjePtWYSSLPHvRrTCdVwvAjZaZjcXVDVwGnj As String = "ISeaRSMStYpQRUgHtgNurZqKkmAaQHETxFPyQuWBkDvp"
        Dim ESEXHnfcsRGEiVvPseZJAvWsRTpvvhYHletVFsIuwilFKTPOKFoYphNIvLGnwigKsyR As Double = 2565
        Dim yYgwdbHoSHWxOTOwklpdXBIcTU As Integer = 1
        While 8686265 <> 62
            MessageBox.Show("pkiAFVWVbxcTtgDKTNRQJcFVAqeHaeySFZitNHOaMnWnIocmcryscH")
            Dim ogtNBTKtdPScLyRn As Integer = 0
            Dim JhudUQDHcxZGLgyZnwkSfssEAOVCiKbXXplqvwfLvineJKZNmXjxgj As ULong = 568
        End While
        Return 63
    End Function

    Public Function YdFUoLtNilJeTfgUniOOuxrZdsEtfpGrNbRjWrTSYvjClMSBtyCOSIGakeqvGyhseRRqNwxvOuoxNpmQaDAgmrYEU()
        Do Until 4268 = 5
            Dim ixigZyRYHjZOZfiq As Int64 = 67822
        Loop
        Dim QpBECOyaPSBxEqZryeKaSjFOmfdsLLhvOiJkXsvCLYWaXAGgIqACsZefkmQVkGOEjFjUhoTGh As String = "lsBKbnppwRItrKRtxfetlJjLKH"
        Dim gvEJChnnYsZyZJycVZXFnGZsIcUpnNKqpCybsQmZmjrDNDXPZkITSBBaWLAYNxAKhiJ As Decimal = 87
        Dim CLraVVgdgqTqFoIvematqyreAUBaZdAcNQUTljuSeFhjGXDKdamlhb As Boolean = True
        Dim tZxRKTLxrHvWcRGMujcPyaiRAxcqDZUaZeZQbjHNUhiy As Long = 1
        Dim AlTFWrWdvBtQLqmsvmwgFtLTEFtKVwpJnuoHHvUNBDGVkFcZiUFbTnUFaAARwLIyKkcPCelARjeCn As Double = 35
        Dim mjPaWPJTpLeYLYPgMWPJGmXhNHujUjhjHRxxMBYPDgsMGNpwTEVJUvJyLgNOUZxFcuRsaRUKwyGYt As Int64 = 3
        While True
            Dim VwvUtYirsDUQOEvwTRZoQXajTAyhxwhXSXDeXkMjyxfX() As String = {"QUraPCtfgYSdUTShUcXCEKqekgAxOxEHVocEfTXKiaXZDUJfRxkFRbLFdMohCvQhTLIepOCBEMScmQbPXTfQNyotdWZSdoIm", "PDSSMYmekyJpYiiWCdOqWTxWgGNwdkcJJMfntTqSPWCulZkmBJqlYRAuEvlFKoAyVAf"}
            Try
                MsgBox("IuWFiwNqwhBJlFiF")
                Dim KVUrrykfNqeMmkbO As Decimal = 7
            Catch eWQYOmLLcsHaHjyhBmBoBDRWfaIgHUguecAYloktvRoRnKJEVYGtMx As Exception
                Dim dcpZxwOUBtqeoQBf As Integer = 650365
            End Try
            Do
                Dim NoXKxGYlAKAmflLs As Integer = 31
                Dim UTrkpBeSdKPpGQFtOKTwSfBfdhsvKVkjvRDyWcrNfytXovydlXfAsxSnSLHIIhIHZetnNGWkSGQSlcpghOJlpjqwyOfLVjFwqlxKBLQfAuhmgyCoUZZtXV As UInt64 = 3453424
                Dim geMJLhCCcENFAdok As Boolean = True
                Dim PWxEgkbcQKZDQfvePfIknkyMLv As Double = 2814143
                MessageBox.Show("LHuNoFlHprqoMqEGHYZAuqYZsB")
            Loop
        End While
        Dim qqNRJGluhCdqFZJKIuhsnFfLUHYPwsxRXvFYFEuqQXtlcIQOEbyRZy As Decimal = 213601
        Try
            MessageBox.Show("upRgrFnvmvRSMXvJakuZhMGLbl")
            Dim NmcXPxBBadfioyYiLbbQUtftUnRlFUfOORaAAxwbvebYCmPRNfoRBIpTGiDafOvFwRUWXYhMdyUoSGqhYAYYeONFbHsRuVvg As Int64 = 6
        Catch YhlgUGAIZesejOexagTwkdvEFbjXoZMyyfCYpeRQLrIUccHVjblaIsgtkharWcSyRse As Exception
            Dim WPmPbqMPnVxcgTRJChtoWBEaUhvZttfNSnKdGquEkWKFsGvGhwApaAIUKMttumVsokJrInCLlpDApygOlorvtomSfpStvBJxBRPIvDxstoDj As Object = 24
        End Try
        If 1 <> 362 Then
            MessageBox.Show("JhudUQDHcxZGLgyZnwkSfssEAOVCiKbXXplqvwfLvineJKZNmXjxgj")
            Dim vKZACWeaEjGUwQOw As Double = 8
        End If
        Dim HnPXcbkmhPqmVkkZqtoyLvoQTSmYEQyyFhDSionfeKhNpTXJbDPbPTMLyxEJoLhRPqgmgSpCFITBN As Double = 385
        While True
            Dim dKRVZWbXTugoyysR() As String = {"KVXdjFuccJfELAWJPUBUhjwoemQevDdHvymSIltIkAFGkcvGgQkPsV", "ogjTwbtxsmRyFDlueHeJQWgsExDBFcvDdihhqexEnyqliWlQMRIABxDRyqPyXssPpOUVVEEyJ"}
            Try
                MessageBox.Show("UjcoaJrjqoGbHIujOhQmvtNTimWIGswCtKNWHXasitkDMOyQAOOFTnwaWIGmmjcwrDgwtXcqGXEBJ")
                Dim CRWONEYonCxqddBl As Decimal = 2510572
            Catch MFVjcqEKahaWaxWwWDpgOCPGHGeidpPfUlXQRCEPHCTGGUaqWnpSiLRJpPbWLUgjRuC As Exception
                Dim hVfePAMWPjwKbTFU As Integer = 0
            End Try
            Do
                Dim mIpiRgeYqrrrHWhIjnoGEirgBawHEAQJAZpIHuNQLlHb As Integer = 14
                Dim kuXneAmGOcjcvpVcKpeCqnAPXlTMRmGRZKeCyKwDNHPCgrOWVKuXdjNwuIQsfZoaWRDlKTcAsboiYwieWwqGrhnow As UInt64 = 6114173
                Dim BgsUqkaTPnKddvjLRjBGFuMqoh As Boolean = True
                Dim OQXoeArKnLiegOEVosOhriGCxfUNNbMaBiBMvZTZuVZI As Double = 11
                MsgBox("YrMhwVeoXuxbCDpkFXmKNDPbPagDZSQVDelsFqeslhOQIexGgpjWrVjVcjRWoaOFebXXQveOs")
            Loop
        End While
        Do While 5 <> 423855323
            Do
                Dim xPDciQwOTYFEDMuXuNZorcenmVeivvAIcYaylOFlVcXvjECeSfiRSkZUkPoNlNRXXvaelulgHmSPEiDiTyQPNhprxFOVYrAILAXncWrWtBKe As Integer = 31
                Dim kXtwtKSLmYGjnTfCXSTLYBFJLdMJScnaWiYlXnTEYKUcpyaXjtmKoDrjSklQwIrKjeNEusdjxOmOf As Decimal = 7
                Dim mjPaWPJTpLeYLYPgMWPJGvJyLgNOUZxFcuRsaRUKwyGYt As Boolean = True
                Dim pxfqOWYUicOmNYgkWDTdOxOoKAxgGowuLqXTqpARofYJuXNIVXnnfU As Double = 4
                MessageBox.Show("sbuHkPYLHiAjhQYkTCKHtEyKJWROXkqDRrQtdldQEpsdQJwUTlEBocwncjxOwJmWrvSoAdHgYsCDKUtyYwukgoNnu")

                Try
                    MessageBox.Show("dLskTuhqYuSnRsqPupxVdNyWPFdXFDckOQcZmarhWlNKxRHianrGCO")
                    Dim kUBSupxmFyjiAOqPsITyGxKkWlhVePqNcPoDjHBkRhWOyRdDUpYPuTYlJQuJHcIpIWF As Int64 = 21452820
                Catch DFHeEdZKWBdPCyHCMhvkibLlDp As Exception
                    Dim hZmYDcPpSdJloEDWtMqKkYQmvevcBOKcGEyIReGOjwhOJxojVAMbxwNIJycXglnDTgfjEtFOFbampBdwpMHBCheIq As Single = 1113
                End Try
                Dim KVUrrykfNqeMmkbO As Boolean = False
                Dim HmqXPOImkjGGoWYtdahoWpGJFnFtUGjQvaWFMPSfiSNVVoRvjYBZtP As Integer = 6
            Loop
        Loop
        Dim QUraPCtfgYSdUTShUcXCEKqekgAxOxEHVocEfTXKiaXZDUJfRxkFRbLFdMohCvQhTLIepOCBEMScmQbPXTfQNyotdWZSdoIm As Boolean = False
        Dim dBAyDGidIylYXrZDAwredbieOS As Object = 5
        Do Until 24 = 44
            Dim xjMDkgHviEhEhRmf As Int64 = 25
        Loop
        Dim TcZvVyXgLJeNVCsNmPTKGAnhNU As UInt64 = 7703
        Dim UWiJsLTgFppRAbekMdojCxkQcV As ULong = 73
        Dim HyUFkTnXULJGRlurwNjUGgMQiigDJKRENErVoapapxSwPNkyMBAMACeamDWClkOKsFsMsZCWW As Integer = 101
        Dim xsBIFwtqaIdWjsQTUwGlHEfBvUDKhsyafCjKmAOoouBrmNohaXeZgxZtNyMksjJgJDp As UInt64 = 2452254
        Do Until 206370 = 52
            Dim hTISrjohmkNXKUvg As Int64 = 4
        Loop
        Dim KEWpjcuXGRruhLTuIqMwPBGYwKETVvRYpdjwbbpEYHlIbrqHwCGYRibIxwsYYaBJyTEDaVSfA As Double = 0
        Dim eVDlMLRLjDwxeDPmbUhrwKmXDCONIqnWwuPvHsKBOrZbmZnImQPlvDdTifiJvDDsPaB As Decimal = 3
        Return 4
    End Function

    Public Sub OEsELFprjUijNaljSGsScDFRNnnbUUmsOKMeYMlMmpsC()
        Dim RgTIBVQAbxriuhxMmKgIRsNjHqYSNKALSwqMdFUkAhkaebgWgwnPJmFXTjAJwxmRxgP As UInt64 = 7132428
        Dim qIOUFHKVaqxjUuEIuFUoirBijkINEWvKeIOPbJenOvyHtURsKCYooc As Long = 7
        Dim aVReuKhMmnnTvCffPhyPyftFauTBPuINJqackmEggmkEFvtLopFVEjJtaLEypfvrfaKFbCucd As String = "ilTmIAjXbDJoQJLg"
        Dim kBAfJckjNXykEyqQLbUpqcOvWDmhoexjVvjrxDqHalUnmSJsKDAoWLZImDJFoDuhZOPHOyRHj As Double = 5
        Select Case True
            Case True
                Dim erXaHwURHOhNIThZhtxVdEfCUIEHAVIqISXvJUDvZCcjkyOUJbswCi As Object = 163
                Try
                    MessageBox.Show("XUFibcEISdCSydcPnJvLJrexswnLqIvMBVoJibAIGUZFoaeoPoRYhHSfoGoensRKgoQdoSgMJnEoR")
                    Dim AbiUGBcNgmEreimqgtMMMtSDErJIvgPfyHDpXMvjcEtoZccYQUnRJfgGJePOqTRyWYsXfwTTxcjIycdaOkrasxVSZwfrvgrIluSAjFsSBTSf As Int64 = 1
                Catch kUBSupxmFyjiAOqPsITyGxKkWlhVePqNcPoDjHBkRhWOyRdDUpYPuTYlJQuJHcIpIWF As Exception
                    Dim uUWgZmKwQKtURZeA As Object = 4
                End Try
            Case False
                MsgBox("GGUOWTxiaGEbLkDGmehtOMVfZvDVvrvQRSAkjSmnRVviqqZGkZZIYTctkonaPclhKnJsjSWTqtbrPmniWvjVSmrwo")
                Dim HkZAoUdStQGXGGvuDEjPXcOoYXHxjHIrScyjbXHRYCPpsSwBWPCHhWyeVkbQNeimIOE As Integer = 6
        End Select
        Dim cKmtdwEbbYWFlHAscxPWGAEfDW As Decimal = 205
        Dim sjsNuNgMpuElAtebwGWTRvZfwYGTivLeDhcBiHDexwwXHUMkjtTyOViwKNPVHMdsjZhSRdrpD As Double = 226586
        Select Case True
            Case True
                Dim qwIKqUiRDKsiQHkvqdNhmaMxHlUfDeXJFaXpSSOaQJxtJOTyAyQSsr As Object = 63
                Try
                    MsgBox("LxtwhTXCbmnlxZlcFqbTDVvIqwYUCmmpErKonjoPEoVHAKWgFQMvjRTpClFktCAoywhGPwnUCMOOy")
                    Dim girfEUhJHIPgTlQrRQrJsStlIDouowdudilyEkJmXgJmxuTmohxwXbSOoGXAfmUKdTCjARngHFlMJGmMjBFHxfXCQ As Int64 = 83
                Catch bwWBtYKImkdSybeSugyCeLVRkNJYvhyFDBihOvVpswbhKSreAqKdbl As Exception
                    Dim VMiEflvATYPFssRe As Object = 63431
                End Try
            Case False
                MessageBox.Show("sQoBPjOXYBPfTOsvNdYoClfNhRcyJocTUJsvvmuRNFBVKnOXmQFoZBoHjaTwHPNiXuLJVvAZiGavjKDsDELrimsSU")
                Dim kKuDRAaRigwxKeLFIjoVoNxHpWyeicEgmJEQXoabXHtPjhIRgQFqPqdXMnMuAvhFbPL As Integer = 675884
        End Select
        Dim DbvnqYHoGomihcFOwXdUWswXyp As UInt64 = 46
        Dim CkAEcEHkPcjeLCYnIWoEwnFYDtTkjqMyBkUXVdqIECEddHMUFVSoxd As Long = 626
        Dim aQFPbGxBjWLbaFmiQtAdEcsVGc As Integer = 4348255
        Dim mmXimtCQjPqfQrhCDxhlbulSaokqmsFZWJUcZlFbApdTeiyItBrdUvFqAEHwJLcpJQIHXFcQr As String = "XQokwSWWHdpIDkOpNGthCdUFHZRFLaIIaKyAXANAQrui"
        Dim DFAyvtYnpJjEcgTntdDGANcjRDlXnrkqGYhGuWsWsmbEsJfEIajeiwipcQhrHAywYsx As Double = 35036
        Dim OnJAvrFombTuhyyYkEgUXrRvGE As Integer = 264
        Dim kBAfJckjNHalUnmSJsKDAoWLZImDJFoDuhZOPHOyRHj As Double = 7
    End Sub

    Public Sub LkYflpkUGhQfqxuqheArPngVaH()
        If 163 <> 0 Then
            MessageBox.Show("XDfLGEqyeSvWtPKVptbKbQiyhc")
            Dim UgMiBfdKvZahFQlA As Double = 55805
        End If
        Dim dctOuCgwvDXehQjBLArbcrtMouZjuOGnsqxaDpuTSBDCmaAHyHIXPk As Long = 36661
        Dim mZRnqLloVctpoOnSrjHlDNdBtssqQAoBCQYXmLnZkGgGFWqsFtxEHTjYuPswSuXUmrnXpQfguhBvgWvWBykWBkthgAbWRpGoiLTMiTDoORIZ() As String = {"BlixQEdIFmttNAGCbOKwgEuNQs", "oQlXgKIsKAVeSGjM"}
        Dim SnxIphuqEWcTvsUpUuLfVluTtH As Integer = 0
        Select Case True
            Case True
                Dim ZltMRBuiGNPwGVTYVWMXVdv As Object = 35
                Try
                    MsgBox("BCRsLSbSXWvKWTCZupswLpbSjersZRMSdkVtSlZWcGZMpGuwYaTxFwOLaxIfahrGJdgkwHGIQVUEP")
                    Dim GcVgxacTvMxAksGpdWerfVfwOG As Int64 = 763
                Catch DJwhLprmNSgWigFuWUSyqAfnhF As Exception
                    Dim ojXNKYEsoahNrNjb As Object = 21435
                End Try
            Case False
                MessageBox.Show("KFyOCAICEDMECTtDHnacTYcTjAOJiGeeyekNJStFiIPTYMxUchUeGXgkcedClxTxYhF")
                Dim WweYgNYiJPpONhMmQeENDepdvHXQpqHUhPJfrusdlYwgPNUAkfMYMyyrGBbYoHrRIUf As Integer = 668
        End Select
        Dim MEjbKRXAZPKNNRCOXgpkRifWyK As Decimal = 5837860
        Do Until 781 >= 655133374
        Loop
        Dim VpfSGNCUNApwYCtXRQZvBMCeEK As ULong = 83607
        Dim vNyorIrXISsYvMwFViqoTKYhvdcTFggDExdQYkOTiHTZhLxGreVTpfYlnQrOfXSaloUwiggkT As Integer = 16052
        Do Until 37672 = 137728
            Dim LNQlHOpScfeTSbRe As Int64 = 5
        Loop
        Dim GPbvYoVLocjWEahxHiPTJEcJdy As Decimal = 7
        Dim gJhffKTsjwvqZnEXUYaqHjcEav As Integer = 4557
        Select Case True
            Case True
                Dim mUKkjVURBdylLqYQsMxRICWImh As Object = 5
                Try
                    MsgBox("FdpnUWMbjdjZuWYmlFFOwfHKdocIJJLsdEfHCIwugFauDRbAkMMTHaBZWeJfsESaSpvpqQkdHJgrE")
                    Dim ExdoTmnrPUyTjhUufIXUneMnBbsDhjTBHrXuXWHdeXQEGvxmqEyADoQYlomrtjJfBxLrJYXJBtpQOIhEdFMdjSwSa As Int64 = 8
                Catch WobQTlLXLbvXTvSAXBwGhipELQWiOqyvOHABrwiUgjhxJJwsntuVyK As Exception
                    Dim JHOyfFORPUAvAGgN As Object = 600
                End Try
            Case False
                MessageBox.Show("etXXuxNjpvIQNDvmJqYdlmgpokxMCdwFxhbTUNicatGgJCXaDQwwRM")
                Dim uroCvDvGiVntQdkbQMJSepYwicbBdWfPaGMpgusNvacQBFAiOFhDXkqFTqSSbtbMoYT As Integer = 46367540
        End Select
        Dim KriHOZbDKDTBVvsrlMKtmjxadU As Decimal = 761548
        Dim VIlZWtuqojcYLevkLFIDWtqILP As ULong = 673812813
        Dim ZltMRBdPOWQxiMBuiGNPwGVTYVWMXVdv As Integer = 238122
        Dim ZILeywjfSbIPAGGNSorOTeeGFvoxkYffMSwwBrmyYkTWsKdYaKMmhIWtdFYCHkusvbChJJLaB As String = "svFTpZoVlgrmuAEkUkMhonYbWfVerLqjHuWCoqROOCDugQZlqjEIdsTEWdvJDJcapHfAdCvcSxTlkduGGjmHGhFNb"
        Do Until 5 = 6
            Dim NwWmwBkbHWemKjWh As Int64 = 7175
        Loop
    End Sub

    Public Sub nycdjQZlPTVtdnPLuDCYUXEKqFvasjSvtwtdeMXRTAIEQClIByWZRkWQtSleUysfAYAftlgie()
        While 80758 <> 865186
            MessageBox.Show("lfugdwFdRAHRJgaAJapbYPOyPqfFQUPJXOnEQNvoroCSYJEnKsToSN")
            Dim yiLaPcPjYDhcFAIE As Integer = 5
            Dim PTxtwkRJKJLbrnVrouBuQZnxio As ULong = 7
        End While
        Do Until 781 >= 73
        Loop
        Dim eoLtWsnZRNtwXoJatuBltlkMhsQvZeGoKxkOKhpmQlbHdvNjQldYyKjrDxLdcqylGEmZDuQMENVHK As Int64 = 7
        While True
            Dim qRysofQSElvlgXsisHMUCBAfkLOYdcChCBmOGVcSimmctFynarcogKEpRSIdRqoajOifjdcXd() As String = {"NIPMjHeiLihNWidbOjVXFmKtCWTHqbjSAuZVcJAVZgPRkVGZdBKfhsCawUJNmXqjyONaJHyFiAwYWsJgPtdpQDjHrxXHymhC", "OvqookqGBKHglCBXfqwtVEYRuAHypnXNYMPnrEoUnImoKcsxyOVkRidJXZsocrQNmsI"}
            Try
                MessageBox.Show("nergvjDjajPIlkQR")
                Dim ItGfsfufNyfyPGXBNKigUohLx As Decimal = 0
            Catch lxmCJVCQDcnxdwUpPYUjBpGhoTSPPoZXHhYgSUYyDhqXqqxPwgcbJFCvqFNoCmAOCQp As Exception
                Dim SuqqWCiFqIIEQUHy As Integer = 16052
            End Try
            Do
                Dim CaTLSdcdUYEnwfBTfgrHdqmLBigMkcYBJYOEuMoCKfUA As Integer = 20
                Dim aCLDiFaAFApwCgcpdrutIHklAAvXDAqSuUUaEbjPqqjXZqWpeGCxWViTQYCiJCvFIXCgMCkCHeCQWNMIcFswxruYl As UInt64 = 4
                Dim nDcsFvWbaOgMTsdmmMGRRgbyVQBHWETqIbHGoJHcgslccOFShbrRWRnHbcXotOFWtpD As Boolean = True
                Dim oXjeWlskeUGKLUGvNchNATrJJU As Double = 5
                MsgBox("JGnvtCEBDWXoRcwtuDQQfkEsNHjSLtERrfWKRcktmVRcLgNCGBUmWB")
            Loop
        End While
        Do While 8 <> 1
            Do
                Dim ZtdcBIcnrtYwANjExoRTXJenRvRqQBZSENMmGewQcLBhOGkSlRmDavRQOpLFqkWqUGslBwxSrAqLHwYLKyFyOVrHvuYQTrZroKMINyQoTTLZ As Integer = 2
                Dim pMWbZhrZJpnvgZAnQoWRQRYyLaowfBLpgFOMqkKCdOJnfcPvNvAKoByYmWQystsHdwOaLmveeTnCH As Decimal = 20465
                Dim satQKBdDHEUHmCBxdboreTikiiubZIhFgbtAwisgPKvVoJVtwAdYlUcYrIhaGBcxWPaLHAAthHcUXNuZNBeYeISMyUHoBSaOgxxZiJlvyiperIBdNHMFWD As Boolean = True
                Dim arIZwjIRVCFItEagkFMLXwTSIjjXWRTRgNESvCabcDvRmHePTfOhDw As Double = 8844170
                MsgBox("siaaBcGLYLwpdxsy")

                Try
                    MessageBox.Show("HuxgytIdjcYFSHBvZyVZprUOuhdKRXTbVsXINCoqJdmYCEZJojhdZYMOajkQQxMMJviYishXJdQMFuMpITVgpoBwf")
                    Dim NqfHHApbHIRiWLWAVTXIPaWwZYtxEIRXryqbADldKjdKwGQrPwDcsw As Int64 = 4
                Catch LXwPTEfdeNxWBJXYFlCwaMAEJucntLmITGShZkfNEeerVlsXfvkngHExXsFXbrjSapD As Exception
                    Dim aKNVSnjDMUZNMTQXOMdgrplJrgDmwutuJuBFgdpXSukMKwlYSKfvhLFjYEckjvGRZdDQRKpkVGOwFYyuteTmPxNwp As Single = 2601
                End Try
                Dim crXiBdVTgfsPMXDt As Boolean = False
                Dim OeTpYlXRVkhPmMTIPlcSGxwbHsOiAIyoOJYZKBeiscVCDosaIlCISl As Integer = 5072
            Loop
        Loop
        Dim fAWxquSmZZUaoquoVrxBMviIumHWKRXHdLmvMDoNDyWoGZbIngQMaKYiRNCDmvcHBNwdDxbRCjxMZtCVLxhdBKgSlEexBkZH As Long = 7
        If 6 = 7 Then
            MessageBox.Show("OCfvUOvXSdVnLebAUuoCyIytnKQSuiuDglnLbOjFkHUg")
            Dim RDUxFRweeZhNFAxbJNpAGVcKMHoJaSHuHTSlaojeuhIFXhVxugcVATNuaJlxbmDLSmFkhMATdKEYMWldlqOtpktnG As Decimal = 18
        End If
        Dim GqMpGomAhQvoFCgsHhsCPaEsdSFKUREnQqBxXAIWrMFLFufVfWXDvfXelTLmiceQLHXNBhUHZ As Double = 51784
        Dim nZFeTxaYLiIylemMoywwIUNaCagDmVioLSmQRIWTImGagPorYHqkUwtTMdcrQWloATm() As String = {"lqyVQeiCFZsgeveVPRkquYcwmN", "qhhhpxOsSHofLKtDyMKrALVmjJUDfJyigRbLfbYmVGkyosENBHTKSwifLuxgcLNIHGysiiWWmWWsZVxpFGBYpnQihvykdrJJsbrisKeSjvWu"}
        Dim eEQAlyHHncDrWOWPyDcnKJbuIdoajHKbIpRHEhLDVsEP As Object = 61436614
        Do Until 364383 = 68135
            Dim MxBnPHkUXEQomLTw As Int64 = 8267
        Loop
        Dim ZfGbgMvYBSuOFGRSGNPuPrKWiP As Decimal = 5
        Dim ATIbfZjqNVLEAwiayrsNlSBtFbkmXuqHNMyRoIIqfwvUcSAdMPpvHQNhSNVDnvSSeEqmxQkpj As Double = 1882
        Dim XIvMtLZVRqtAtcBurjtPwuSytVUBnjrD As Integer = 10
        Dim ItGfsfufNfyfyPGXBNKigUohLx As UInt64 = 1
        Do Until 25 = 405302
            Dim KFvkyDVcFpriofMp As Int64 = 673
        Loop
        Dim XLqNpViUlHUYxIZkBlEBHAaejaJTRdHKLQaZmvXaqJERlRJpKIiuNNlWkMdOMPDHAJNhhywFO As String = "XjhpYrgSeJKjYFYh"
        Dim RaZEmbQPCWlcBMUFwhMQYRCKkNxocTatNteZttRAnQaqfklCLUdTXOAyYOSYdbliuqO As Double = 60
    End Sub

    Public Sub bcXpQNAjuEhnafAIkiekiTBCfAFTEpDlNikatFlNbsIriygxPLueCS()
        Dim TnekvwoJJhJiqDmCnGZnvWNSCgQIyiwNgcAgSmYvMIwH As Long = 513360
        Do Until 2108870 >= 582855774
        Loop
        Dim KWKWkRfWRjZWjFPhtafCfZDtQX As ULong = 75
        While True
            Dim ALdjgwkKdDemfvaMFkPGwsvSwYtwNLWNfZxJbCQSJdoGidtdhgDtylhGPKlHCnvSfRGGxQkCT() As String = {"OCfvUOvXSdVnLebAUuoCyIytnKQSuiuDglnLbOjFkHUg", "TaWoBPJmAWDLNiLsoXWNTaeHDZctHZgZXaBXXXVfEqjhUfasTMPQTukrvPkamPeZpdn"}
            Try
                MessageBox.Show("xlJmIaHlinkywsgimhqXRpGFLU")
                Dim etLEsWGDuolgFvdngkOGMLWXLIxGjPBQQXICXXfffksbuiomkgPKUakSXnrjYASemUPhmbPIE As Decimal = 76
            Catch XAtmZkKsFGpZYjVgecryXtRHrlIFViTGdABChgPHVBfqlHVkUfkFmukciltmbFXWliu As Exception
                Dim FEjMJtaxyICWRhlM As Integer = 3
            End Try
            Do
                Dim YxEhYPMLaPMGMnulKsoyjKhCUbWuBAGKfleuqfUsDpsh As Integer = 8428
                Dim XwhMhoiiwAafhkMqcLDvlGZNjHGIbXIlkBsJCNmgNFJLIcMOWCSrXCaQXTbahZGZroufrDnPYVifvOEGtqlYEPNha As UInt64 = 580250731
                Dim kEoLilfPDicZSXxGcJGYWHmxvkuEPVnYiNTbemguihLkQPeSeesyXVfdRqPwpbyhuDq As Boolean = True
                Dim erupaLXxidNBhExfEJcxqQwewi As Double = 353814176
                MsgBox("ddeEgEBjpmAsjHNaIYAXGRtENASuMbLjNhshbmMXGnrNeUJVXLudmk")
            Loop
        End While
        Do While 858 <> 16
            Do
                Dim nlsSdcIaabaMtyOHHVMTkTYRXuBWeWRPIcdTnauwNwdWhWJaVdaqFPmTMauiakuDJKERVuTfaOFmoCiAkaAAvaJrpJGMfMWbBZMWSrEaKuQq As Integer = 65814
                Dim GGNXUjnIpLesZUnfEJcHUdGeTq As Decimal = 60
                Dim BrqceyFQyvhsrbIg As Boolean = True
                Dim qhhhpxOsSHofLKtDyMKrALVmjJUDfJyigRbLfbYmVGkyosENBHTKSwifLuxgcLNIHGysiiWWmWWsZVxpFGBYpnQihvykdrJJsbrisKeSjvWu As Double = 18
                MessageBox.Show("GcAQWHBUfBoYAVhp")

                Try
                    MessageBox.Show("cDWOgSKuvXrkSKqCPqQdiIgGWeZhCjTvRERYkulvnoWk")
                    Dim kiPLbcYZjTQKvqQB As Int64 = 87335
                Catch AUgHTfcccFGECibnPbHegHXHlq As Exception
                    Dim RPPLnPHHJdKGtwvFBqGXgOdVrUDhbXaPJVgrfQmXqoXEpGEPJfLBoifDktTSLZelCuvYPDYkDigrpYelPmXuMYaZp As Single = 730
                End Try
                Dim OaJvlVCWtesWwfRe As Boolean = False
                Dim omZTwCeaZMrKPoOuqxViAZgBktaKeoZwHjTUqdBpGhKdfjnnANmRRLPPFXiDaTujltCfVtEMvfcRO As Integer = 4
            Loop
        Loop
        Dim nDqPQXHwgbCKJsNqYBdBsYUSkOlfrrhMVLwONZNvhKWwTcSmivHcfMKOiYdBlaEosoowuxcymhSiCWyHvywvkFgJAifouMKG As Long = 1
        Dim stTyJTOCOFgceMyKOiJaBYXRwmBhJUbRKBPFyygCvBLqNTCZPnLRcZOTVBBVRgsscOS As Decimal = 7
        Dim QNJxiFOVGYVLTcHJhIhTXfJNOCuAIrRZbkqCgAUBaGFsHypqCWFYCP As Boolean = True
        While 85086705 <> 3
            MsgBox("ZXnSVKkdWIJgSOPbpWIFKEfBKOHoFZiVBJnCSkkFXwpmQaHVNmPxix")
            Dim XjhpYrgSeJKjYFYh As Integer = 25
            Dim cOgrQKvqKeHZWSde As ULong = 5
        End While
        Dim LRXqGDnVLJGfAQHPsijTeXdhrBpKgemKtevHFxwIiPhq As Object = 4
        Do Until 2 = 14
            Dim HrIgtdfJnbTKuLUc As Int64 = 5550607
        Loop
        Dim cjRcYNbyDtVqtbbIlxevrcUlal As Decimal = 711
        Dim askqUSCOxVHKMhecsYnYfItipa As Integer = 231453622
    End Sub

    Public Function OPHhcjepauwTbGRY()
        Dim TvCpgRXAjVjsOqVHIkZUSkwuNvInGeZcRGbSCadCTGSLmlbrkXoJrP As Boolean = True
        Dim rcfJsmHrXVJGujWxHToqGjYdHonYPuNuEGplghqYWGWh As Long = 64
        Do Until 6554 = 8
            Dim fmlsAjetyCOyJckl As Int64 = 4
        Loop
        Dim FOykZVwcCroqMQoAbNvvZoxLpHcPpTRSqhBWkCVBfeXXHAtvjpPDnGUQPntSshmiaVydPSPZq As String = "GxGWmKToiqFycBvLTmeAnpvMlsHvLiEMRenZZBirwQRUGhXiAZIwiqEQBtjtDWcymrhwMWkRDlDXX"
        Dim naAvfcFigbjqDjExWbPDrUIjKT As Integer = 1174
        Select Case True
            Case True
                Dim uBvjwBwClxhfXFlEomLwFfkOYX As Object = 136550223
                Try
                    MessageBox.Show("MGLNbXwvqkCDFkWxQBblAbNVkJTbiLxsJVJXwSRgUeaEsmlfNfqHEFEjspbSDVNemJtjayAwjuRVA")
                    Dim WiPXjOXUpkRaiIsIeFnQdHIrmKiuAnFuUxuRZrdkecplQmGDhYVJXGLPaXjYebTBeVGjNbhlaClBQWbADfNaRuXNm As Int64 = 5
                Catch aZaWbhsbnFMKvRyviVnRcYTyOKwtTbxCXmsfLkqjsmPcGKrwLUBQOIcCFbIRkxILHKl As Exception
                    Dim XAHPSPWTtfCgSrRg As Object = 342787
                End Try
            Case False
                MsgBox("JsLNAvwbrvfyOfiqVdQhRbelRYCUYgkdKQbEXrmibBhPOirEIHEUlH")
                Dim JRttVSvWDMHvdcsmYocqpOHWpbZkBxQaclqQTKMjbOmrLunpiHCWMgdtKXEssKUEvfS As Integer = 836
        End Select
        Dim JEIBmFwHYluFtohsUmmHbeFboqveLQNBtvFQTNqgXmFoqBxdBSHqpridxgMuJRvHXtLxiQYGu As Double = 403
        Dim mrHbQcOHxIxQwLtQmGjFlowRHj As ULong = 843476
        Dim XWckeUScJquqrrKUvtNZCJRTPZmveQrs As Integer = 437254
        Do Until 35666217 >= 54
        Loop
        Do Until 3 = 6433
            Dim hYBbIvsTTkHrAmNx As Int64 = 4223878
        Loop
        Dim lcgOtGJwwLBxNvfJtumNyXJyJRXuIvMemNjfDXpGdxEaLYuernDilPoNShEchOnAyWLyMAFVE As String = "tZtVtqildBCAuCcXyqCLUgywul"
        Return 46428812
    End Function

    Public Function rZWSJgXnwOihTqpnCowBnLkQGmuDFekqGfxRNsnfIDsUCdoEoiSWvWTgnFMVlUhRcrbmsYZJBQsjRQOcxpOVMOujL()
        Dim cjRcYNbyDtVqtbbIlxevrcUlal As Decimal = 811243
        Dim KBkpVZVWDrMxKFakSJSDqVkxAM As UInt64 = 553281
        Dim qILImAOvDcFwAuqtRXHfILsZly As ULong = 62
        Dim qkPdOvdOPtbfGYsejcqxbINEKeQoLhxMtQhHEEhZSxqACSKDghJUUxLhXLTifnCguPkoqkafu As Integer = 8
        Dim wgCgrGouIYAfjVhkprYrYfkYAKQEjKEvvPbXLdSxOloCVfwlZeRYdhuQucghokKwTEj As Double = 7
        Select Case True
            Case True
                Dim mxrljXhURqTAADqTbOkGLnHrNovEDLPMnLcBPMHKrrgqPMbxHdYdKQ As Object = 8
                Try
                    MessageBox.Show("xwDvdTfkwKtASdDclSijgnfiyNlpXQcwmSGmSAyhjiFvuiSEgNogwISJQUwPgoNfhwNyXPJkTAAwi")
                    Dim uqkOuSVkCmFyuWkmKwZkIKbChTbqguQnNNyfTptlKiBcYATcEmqulcSkOhgaqdEGWRlgXESTDmZYIydgAxmsEUBjuORlJSufeuGGOlEyUXry As Int64 = 4556078
                Catch uZalLnGYMwbXrmIeTMJigqPTXcSehWuyVPpHZMvmVvCkbesnLjHRPNgDKGNhUbqpBbB As Exception
                    Dim EqFpYYDkibtFmqXw As Object = 1574
                End Try
            Case False
                MsgBox("QgcAFmWnTmfsVGHTtZbwIeyEhpFbTCaQFZBdMkQSrIklTCYYLSUXlyhlgYROhtTQLOmuFdGIJIFysNjeRgPBgDeGq")
                Dim ZlNfaCbGMRjGRGIybRlvpiGQSKyqurZBQUTLlitipcRqmSbhUrnuBwSpfwMQCkKTrio As Integer = 4180
        End Select
        Dim IAPwhQxHfLolbumOveMohLeuBOEmWjELxTjYgHuEbJbDqeQJcWlCuaSwUUvTMLkvHcnHKOgoj As String = "RBIJOXBdNQebYdXo"
        Dim dRGYjHdnkaTOuuCFRFEomFMFdhsCWRlpOJDneNOJkMVCNvEFRQVQSMCXFAtBNaRTodylqNaDp As Double = 81
        Select Case True
            Case True
                Dim NlUihiGqXyptpWUtASgswevMyTIBiGEoeulEHQVDXTMeTDQsXUwkuy As Object = 8
                Try
                    MessageBox.Show("VkBxbsyOOcWKarhRXPgSfjNmiFYxWlkoZQJXsImRkqnehJjZdfVACUNkXNOWTAqbrgjJhJYObPgnv")
                    Dim aKvjWaVIZitlxtpgbnNkZUBowNaflOwWkuYZBTKQgvEQWlCILSMJaMmFIHlJyoWLkWBDUQeIdvEcPjSKKSUSBnvYI As Int64 = 4
                Catch GSPiKXHPxSSWlCeECsUFjGSUOLTDRHPsRlxeeqJjrqIOuJuTnKuVeMGAyuTyDEwpCYE As Exception
                    Dim PItdNdukbbKkWNTe As Object = 2487
                End Try
            Case False
                MsgBox("WLQClCIdwlkbjPigIqoOohDXHLkYJVgrQeLBiQrHvtPjABBafMgtEZQnHsuZRjTaFsLFEhwqyhSuRSWFJKrvnmmqN")
                Dim VxUIKkEupjgPpXvEGXZoyJklUHWDxGGqWGybigelMKVGOHhZVvDXVRoMNinCNBvicprjYTVYrmcomdWMZwgCDiVERlOAscYK As Integer = 53
        End Select
        Dim LulguVQLhvbJPlveuPXmZVHTNc As Decimal = 5218
        Dim VKCVrmimjdxsBrkTVLDnVmPhnywWRUwBPcZVVJsgcyyxgXKPmteVQT As Double = 77061024
        Dim SSaJegoIfPDcEhXegVbFttwJSlckqulm As Integer = 4471
        Dim KBkpVZVWKFakSJSDqVkxAM As UInt64 = 64
        Dim jKhujAwWNBheVBXOyLvyBGMdGesgpmSMxYTUEFbERCkrvSZrVocCyHfcxEoFHPgahVg As Double = 746462
        Dim TmyZFeEeAcaAuRZAJXqHEAZdGOsdhwXUHFRXckAWJgInLkOFkMbHrc As Decimal = 8
        Dim orhmWOdaMvokCxMgmQnpoqsQbWbiBNkQBSxdclSXynlqCcuLQAOgZbZxyRLAmtYxKXiiAhDAw As String = "oxrWeDrlhucuARrZ"
        Dim uwAOQvobIWnBFTmVcbDqRwIvRqXavPfgMXWhLZtOCsEhwqgXxZoHbYQwnDwZYNesqGo As Decimal = 5
        Return 1323
    End Function

End Class

Partial Class jepfceBgYhrIxKpfyOBLROGMvXiXEOCnvxnfQdNgdhQQhIGDaAjtqw
    Public Function bTJmEXUuutXlleEktZSKFMOKiN()
        Do While 4 <> 37
            Do
                Dim BhJHMIEjlhSVkHBU As Integer = 3374
                Dim vaCGrhouGNYUHroLxsTTIpbwIX As Decimal = 4784
                Dim OCdHTRLUFhyhOuxdfWEJUFKWuX As Boolean = True
                Dim wTLIZeVZevefxrQvuPaQXMjtLyuNPGvbuyRynMrWicIURRKuwbVWtwqkiQvZjHJXyCk As Double = 34
                MessageBox.Show("TDoPFGhZqkJkSajh")

                Try
                    MessageBox.Show("xDgSeWfrBNCwZMiPfgJajuCNDGjBgcbnZFTxAxcbDFMS")
                    Dim YFwkCRueuRwYXJMwFDAKISrUgyNmilWrVmyoXKnTfHQeEmSauKNRrkuGNbDCEHuJuECjJWYtPgdBBNQtflrXXEAkq As Int64 = 61
                Catch IHTdxJeehlPyIoenrrpiKoSpfNvXcseBnhoJaccQToemfYqkSFQyIVgSncwkNHCnGNLayUMOPYOVKfJHVKEWbfsSBVUwerCMlpfSMQkxrBji As Exception
                    Dim LTpffsGCHNmSYuKOPKpNuFHaJrpoYsHXYYWsRpSPfEoRGZZNjvjNhxuGMPWnDQdNFPsAXQvZqagqPoTiRbFcIspac As Single = 6
                End Try
                Dim BTvQAqFuBgiScLiY As Boolean = False
                Dim jThKsjiBBBeGoYUULJjUBJhNnjZyUfVNwQDChnyKJFcvjUWNbKTKEgFZkUWJJifVfQIXVSlWJYKvg As Integer = 10
            Loop
        Loop
        Dim PaXCiOFoGwdBsaOtpJDcZNcDMGcMiXEqrTytnjAUlylN As Long = 6
        Do While 5682174 <> 8862507
            Do
                Dim CEQBKVOqLTVlfBOc As Integer = 8
                Dim gFLdhUcsHZIXOWexlaroIrPSCh As Decimal = 54
                Dim XRBOmuiwREfcoEMA As Boolean = True
                Dim qjyyLCsiUYKBUtwNZPhoOkodvEUEYBBovUrUeiqprAIGgUvtrkqjVvEFuVDGgoKTXUFyCciqXqvNchwaIZLnJQTAQmZJejIDpniMPvrPCALd As Double = 22
                MessageBox.Show("JFJOguoIISWrTKHV")

                Try
                    MessageBox.Show("hyUaVRSXnGjFCkHKxccihRGYpQYGKjRrnSMooovcQXPZ")
                    Dim hElCZkTqQDbOiJnP As Int64 = 6
                Catch vbIEIDTxGHBMuNLFPvYWqPmbnJQmUnmDsHsTNITHRpsOgoHCVEyuCgDgofQylfdSUxTmTqrHbRFAWYuxCORPorAME As Exception
                    Dim AcsPTVGoWvEixcLdSCwtmvGXEHHhDIbbfhByFnjZkEXrysAKyjERRUQDWUKMNYkNTbDPaLYeSXJiYNXLHviTDKKKR As Single = 481785676
                End Try
                Dim lgmVQBqJbpvmXyiauKPGbpJbHmuroMKrqqoLfpxOclIBycMbDojOZWMvXLOdGxUPfEv As Boolean = False
                Dim nfNMMISnSVOcRVqFCLhCigTGNypJXUFluLbvsYELwPIMRxaMhlhWVDYGLHyNRIQwPcgGstMdGVESX As Integer = 60
            Loop
        Loop
        While 47856 <> 703718102
            MessageBox.Show("fvQuQOtwFNiAvJfMVXOgDaLfgfsKrrOUjGCnPnOGOVHfWPYgrGQxVM")
            Dim jUTrshFctuuVKtpd As Integer = 374627
            Dim geIcyhEdLElCthZGNKBPTxSgTG As ULong = 51224
        End While
        While 4 <> 47
            MsgBox("jepfceBgYhrIxKpfyOBLROGMvXiXEOCnvxnfQdNgdhQQhIGDaAjtqw")
            Dim oxrWeDrlhucuARrZ As Integer = 4
            Dim IieDvsfItHuNeYWU As ULong = 23
        End While
        Dim ELXqgcBXZERTsfrwcsjvxFhkDLFRTPIbjFkoVZDYQVUy As Object = 116480
        Do Until 34 = 18
            Dim CwChotTFViwiXveT As Int64 = 14
        Loop
        Dim imjrrIuLNiZalqIQjFtfYZqTJb As Decimal = 4
        Dim ZBKEIRdWgwRJpVWcJTIBJjmcjg As Integer = 47838540
        Dim HQqTpqpidxlmAcxcZHuxQvDuygSaTBKH As Integer = 5
        Dim heGRCcbrwZPsJXlpRRAbZCZcqq As UInt64 = 1
        Do Until 861562 = 263635
            Dim kqWBlmsybyjQJmQO As Int64 = 7203
        Loop
        Try
            MsgBox("JpnOgOwOwEqElPQDcphqRyOmSLmbvwKeWAuLTJYPZxKInrfEFSoofHHKaAvUbVSAtMglUFiSRSQToFyZfhWdHVxOr")
            Dim ZWXDYXrhaoDYxDYaOYpjsyGNydHdwdgeYmpHGAlxgwHGNANhjEwiOqrZQKEuQOkvHYyKMQTOmqCur As Int64 = 3
        Catch kfRigDmCGdoPxClXwZAfowiTHuhCnMPjnlNbxnJcOuJsJHLBbPPJOw As Exception
            Dim GXKIBPYOmQCxdlrWWlgaRaVgjKirGkgBjgMNOjXEqOImQeuBikDCbZReCcYpfgHCiBOvZeHlObjDSiQFPhIVgeYJPhKlLgYKGGDOPxbEdRnA As Object = 767436
        End Try
        Dim macxvYWDlyUNQBNwsRJaICaaJytdYvvsQyPsxYxccDXf As Long = 21844
        Do While 1022 <> 540
            Do
                Dim ODCdULxAZChebBdndYbpVvTbFwAnlenfIULsHLZqLkckEIuuqHPuEnGRjaYWLJOtOCVHokgSxjdkThWvCEvdOrbUOtJglCfC As Integer = 41
                Dim FSVWtuxUNaqJacOHGXYZdcDDLp As Decimal = 6
                Dim GPJixxvbZgfGXgVRirknlvVxPQ As Boolean = True
                Dim XhSywNjdWNZUjQXkowNTiPXTumcfQcJEDMwCBgnrCQOPIRJVETiYXPMIKyDKkENsEVQ As Double = 4
                MsgBox("XbqZFTEFOHojTRgm")

                Try
                    MessageBox.Show("oPyAIgOVeMSAxVcsbhOSEdRUVOAhMYRPkVMUpQNkUwSm")
                    Dim ulHklDVTIGQSRCrYoSDJYmBIAKhOrYmUCgOVMFSxKgJpfTuvAEgkLDtIjxkaUGajTvPkmVLiAEyqcKbfPlOmrBIylGkOUqcDFYWjDKIdswkHECeZVELirm As Int64 = 260551
                Catch EduiSLojcxXTARPaISSwnnbEejFCxuVrUXCnHrvOyogtIfLnXDWpaTsQCqEiFllhbfxcOaSPWEwxGhuxYRduXfQBellPnNINDyRchiYwjHSW As Exception
                    Dim gvokForCoTRwOhlaersQdLJqsuScHZlKoSrZoDDQXZdUkbnaUbIwSiSUlwrJWOuPhHIvoMkbocsDhoFRHqIfQnqFy As Single = 515
                End Try
                Dim ipQuNElVOqlWvNGQ As Boolean = False
                Dim coGbiwrbQlmQymlSAuvQCfrEhEgfPNRdILDiaLKMucnUxAHmPKhKeZyvvBtdPLLDUJHmiDrnkiUYq As Integer = 7
            Loop
        Loop
        Dim BkPQoAhhaNckSmNlHitfhjibgFNjGWopvtOxthloXfOgRSxqBmPVlNJQBnjjmwbDXlnXYPrZwFHBUCJNHViSGSDikwvGwBCa As Boolean = False
        Do Until 1378 >= 1
        Loop
        Dim lCPeDRCrGcjfhFFZCKIqSCWtEKwgptfjttdDqdChuTeyBoShxYUSuOAjfUarUUBMvyknaBiXIZuyX As Int64 = 28
        While 4618528 <> 324
            MessageBox.Show("WtKmpUUFOacOXXGdFrRxoeWiesyCwArOGDogvLtuTkxTNWorCbrBFm")
            Dim ByXSfrvpIuCZrmWf As Integer = 116480
            Dim qbsfQmAONfMmjHtwCdvcjHaIUtEnbIRUkPucFBNKaiaRfjJqPbXNUsyjBjLWhMmENrCTLQrBQ As ULong = 4443822
        End While
        Dim GBpWkmmyJaPevuAaSDxvGBCUREtqqkFdiTSFqQGDclqw As Long = 65
        Do Until 66 >= 4
        Loop
        Return 52786
    End Function

    Public Function fSRwnkppjWloUvoLqCPhDMVBjQQbiYXrPjXvvxtQQpKNahfkjEHOmDlSwQNWTNGLytZkXrPsIyQMBIZBkerdQIELwEyekWvDiyNgItNqoOGn()
        Dim ychAdGuljLPpDaDTXTWKqsVRmIxnKaWMcJyvZTXKhBOvmGwEwclTpNsGGQMVIGaInDMsaHtDu As Integer = 4717
        Dim YCPcYJoafWouxmbhgxdGnPMhhh As Integer = 826
        Select Case True
            Case True
                Dim UaZQdNeZFBmCeVyoJWIQyZwbLB As Object = 6568
                Try
                    MsgBox("UNKIZsRBSDICByHaKoQiwJpNXtHbwsEXOjgqyJabjBjnLGBvIVafBRYKXwjfkpEYxjmQRQbAOoyuZ")
                    Dim mfrucqffmvvfajKAndcclSgyQSDMUfjQQESEPxcisrElljFNQPsiVkcUxreKmTLGMePVRFkoB As Int64 = 152
                Catch ViFfOlVmrCsLhVvNrtapnrIvAT As Exception
                    Dim pywWOcxMfXxhQgvS As Object = 567
                End Try
            Case False
                MsgBox("jseyGmPPhyfAaUQQSXGQDGUeJrWnUWmxNQPWRERVpNAwYsnPAJptQC")
                Dim XjlPxnePJMlPJkPwXUnseacKgPpLkFuYLlnjYpHOJVLluSgcXTaqLHHwGbiixTdERMD As Integer = 1
        End Select
        While 8676 <> 631
            MessageBox.Show("xQDWdLhlTKKwIfObwbfdPtIeLMyevXpRUTFmDXKbasYUPVgxSlXfMd")
            Dim KncnweQWDTVLyLtM As Integer = 1
            Dim FqobLoCtLvvoMrrkJPCFrmwGOtCGVKbPCUguimSUSAykvSQhWakrUgtSthNRmuXBUglrLwCWv As ULong = 8153021
        End While
        Dim ovIufYkymQxAXtwFcOHpMvnmBlosbTetgeKaHFrntsZQ As Long = 308804754
        Do Until 15630 = 1113716
            Dim YJuXmOZcAVywHQWm As Int64 = 6
        Loop
        Dim ulEZNngpAZCaDUSHXQwJMZsgWHZwUjIAxAYqFoiJWopGlTqohcEqIPwVsWDGOjVPQBcDubChh As String = "CPIeCyQNVSghHyKqRXTlusMXQMlKTnPCNCqjZLMsdlSB"
        Dim SqduEHyucGdLjvOfIRZrNENPlSfjQTTZbfqNfMWADWtcKJlLuWoQSaSOdOJkbPdJlEj As Double = 78181
        Select Case True
            Case True
                Dim GXUmcLAyTfLrkyBIIHlHSiaTyMACxAcPtbpOJHwTUPgAyiJvkfIEFCXWwBmPLwATDuwJJlyAJRlDBZTQDDQSqXKrDSRHThIWONwElymyfHtn As Object = 7
                Try
                    MessageBox.Show("GtUgJMmLVpjEsEnHMePDtuLbaxXPworojPwFYaqXUAdbsdRRPGQZjvkLirGLoXOfwuTcHihlnqhsQ")
                    Dim kTHTMocpRuxSQCGoavPAuoXHHEROreXcdqhbshOcHAEkVlLwlINPwwmvvALHXOwmWVnZUevsglruj As Int64 = 21214
                Catch AUlbUbHUDRtiNIadyCnDLEaHSvvMpcOXETONQNWCBowBMlbewZYvqnpvtdIYfteaDVe As Exception
                    Dim VXgNPNhNURMpdVBh As Object = 7111
                End Try
            Case False
                MsgBox("vbLDYxWnUskWcCNAxrLodftKrXwjfrNZMxrXktHsfuKHDDdtRiKDGkrQgBLAAlHbeKJdJoPDjRIhU")
                Dim ESEXHnfcsRGEiVvPseZJAvWsRTpvvhYHletVFsIuwilFKTPOKFoYphNIvLGnwigKsyR As Integer = 84343
        End Select
        Dim ZxHiDpwFKYUbFyenxLxfbuwgOBiuWqwqPJJDiYQyRBnwExFlgLnejtAeBZTCHBhSacMuoaEYu As String = "ogtNBTKtdPScLyRn"
        Return 45
    End Function

    Public Function ZoBfmYVGUtrPWvcWWGIgGdGfnTgaITIfYXIfoAyiDODEssDUecfNXEtviVqTquwBkLqxmvgVHYcrZBluQLufmtHTd()
        Dim OyWKMWIdTqWXcgHodggdvUQYUQqrDEQj As Integer = 44
        Dim ttSnPmoIMqlQAVqsQYFWJboZME As UInt64 = 481874
        Dim NcJWQAQeoQawASurfyxLfsSoLEgoJFWvbHShnASllYQToytijFLfLC As Long = 67822
        Dim EfevEGfnNoCQsDiTunZudbtfKP As Integer = 5
        Dim HOsMdpmYvjULfDnEcykcaRFKSqRPlyTWsTkLkSHvcwnVsrECvPEoidHpIitlXXmwQwRLRcTJq As String = "lsBKbnppwRItrKRtxfetlJjLKH"
        Dim gvEJChnnYsZyZJycVZXFnGZsIcUpnNKqpCybsQmZmjrDNDXPZkITSBBaWLAYNxAKhiJ As Decimal = 1
        Dim NTuDJiMMVBJLhbfUawQoilJdfJ As Decimal = 7861508
        Dim FqobLoCtLvvoMrrkJPCFrmwGOtCGVKbPCUguimSUSAykvSQhWakrUgtSthNRmuXBUglrLwCWv As Double = 2702
        Dim HvWrHiEbAhsOFQmxMtsdPCAbnJiSHsBQpumVuBabobpsHIDcpKduZB As Long = 22438
        Dim BIGlpRPCiiBjmtXpDInvlIOJZYyIBGckHAfmgrapXFeTxeRNYumMuOysswMQSHBcFPblNkIKh As Integer = 6620534
        Return 650365
    End Function

    Public Sub PWxEgkbcQKZDQfvePfIknkyMLv()
        Dim aASvCoPyDnrXKRBfehNrBykhEExsoVJdenfYjZWSlonbVHggggyXYL As Decimal = 244236453
        Dim hbwKJUkVvTIJsAlegITEKOYUne As Decimal = 864
        Dim VGoTTjctbAnBZrgwXOvFyxbMCKtuZHCyVpZcneZuFBfjWWGXvvaeijnXebRIohTUwoeiSDJrn As Double = 55
        Dim ddOadBrHLuVGfdOSRSRVcjgDVMPvxqfCgqwIugrVrnkcXUYAXjhGbfkswLEQkuJTPTZ As Decimal = 6
        Dim mAGLevAOMUOtQgWFAPjgVXhHKYEkXUnCZoJZLUIsPWdrasFAybhHegFPSxAGqOqrfMTbWwteG As Integer = 4
        Dim PDSSMYmekyJpYiiWCdOqWTxWgGNwdkcJJMfntTqSPWCulZkmBJqlYRAuEvlFKoAyVAf As UInt64 = 0
        Dim FlKCwChWwoELTJTkEDAqcPKthLpJKVsgqXpTNDGsIsZFhRoHuSWvRi As Decimal = 6
        Dim CWObiPVRPNCCpQbGYUhRyFnTlutwHsKpcIRePsOfdRKNXhmGtwwcYWjwiuomVYKVCLYhyvqeG As String = "xRwDNNSivQHUHNDI"
        Dim ESEXHnfcsRGEiVvPseZJAvWsRTpvvhYHletVFsIuwilFKTPOKFoYphNIvLGnwigKsyR As Double = 4
        Dim ELFHvPCksVaacTNmMnkqGxImIDYtiYIDjwDJeUoLFHnYKGwYIpGLgBYFDRxrqHCUMXq As Decimal = 6
        Dim rjcpnRTSouwvVYVStlRhEveRfw As Decimal = 4
    End Sub

    Public Sub JhudUQDHcxZGLgyZnwkSfssEAOVCiKbXXplqvwfLvineJKZNmXjxgj()
        Dim HnPXcbkmhPqmVkkZqtoyLvoQTSmYEQyyFhDSionfeKhNpTXJbDPbPTMLyxEJoLhRPqgmgSpCFITBN As Double = 858638
        While True
            Dim PTUDEYkhdEajCNhprJIdEuDbRteulyce() As String = {"KVXdjFuccJfELAWJPUBUhjwoemQevDdHvymSIltIkAFGkcvGgQkPsV", "rPqOFxEprpeqXalplwgLxlorLn"}
            Try
                MessageBox.Show("FNBgpxYRdnnxxZfWdEwrBqodDcsXoXeOCLoSkVCqFkkGGpFdspDgTXNOYWdMEAbGIZumjMkICFlPGhXMXPexUxuex")
                Dim rvKPqUmLOOyQfxZCFywHjhvGJq As Decimal = 31
            Catch MFVjcqEKahaWaxWwWDpgOCPGHGeidpPfUlXQRCEPHCTGGUaqWnpSiLRJpPbWLUgjRuC As Exception
                Dim PDqigeKnBXDRIhBk As Integer = 36140
            End Try
            Do
                Dim yGKDYOwsmSdEPIgpiWUpahqgfZAnaYuSaQhuQtioDWWh As Integer = 14
                Dim kuXneAmGOcjcvpVcKpeCqnAPXlTMRmGRZKeCyKwDNHPCgrOWVKuXdjNwuIQsfZoaWRDlKTcAsboiYwieWwqGrhnow As UInt64 = 6114173
                Dim CbqrVZxHQKgscPUpwossTuXRmWHELsDxUQyXOMKpSyYsKmeMrEinQPlGDYEyRtvWHiX As Boolean = True
                Dim OQXoeArKnLiegOEVosOhriGCxfUNNbMaBiBMvZTZuVZI As Double = 7
                MsgBox("YrMhwVeoXuxbCDpkFXmKNDPbPagDZSQVDelsFqeslhOQIexGgpjWrVjVcjRWoaOFebXXQveOs")
            Loop
        End While
        Do While 547 <> 1600331
            Do
                Dim xPDciQwOTYFEDMuXuNZorcenmVeivvAIcYaylOFlVcXvjECeSfiRSkZUkPoNlNRXXvaelulgHmSPEiDiTyQPNhprxFOVYrAILAXncWrWtBKe As Integer = 31
                Dim FHVGxEwUSSvCfnrdaekubnWcau As Decimal = 5
                Dim mjPaWPJTpLeYLYPgMWPJGmXhNHujUjhjHRxxMBYPDgsMGNpwTEVJUvJyLgNOUZxFcuRsaRUKwyGYt As Boolean = True
                Dim pxfqOWYUicOmNYgkWDTdOxOoKAxgGowuLqXTqpARofYJuXNIVXnnfU As Double = 73
                MsgBox("FSEoNhTbdRfxnHsdeIrTqKcrmq")

                Try
                    MessageBox.Show("hQxgiFoYtngLfmLTtibQysTRfPYIgmKpYTlclhwxrwjt")
                    Dim XtvoAXWOwQsclMrakqRSyIvCKCDMORtiyiaqAqgxtLGBfSbOCpAasM As Int64 = 21452820
                Catch DFHeEdZKWBdPCyHCMhvkibLlDp As Exception
                    Dim hZmYDcPpSdJloEDWtMqKkYQmvevcBOKcGEyIReGOjwhOJxojVAMbxwNIJycXglnDTgfjEtFOFbampBdwpMHBCheIq As Single = 35
                End Try
                Dim rralJLmUyhtiHEej As Boolean = False
                Dim KQghSaJNSdyoJNAThNHCGEcjmPWIKuFKWgQFYewqWUyjWffDvPpmRq As Integer = 7
            Loop
        Loop
        Dim KoFqMdoDujAEJhSMjZKhPAUUVNBTniWOEfSwLkDsdcUcxkkuEMhEeaWJTkUxcVOLNlAcPUByWQDmerVkcFPaASjdIIdZPEQI As Boolean = False
        Dim iTXhKeVomqqeRdKjQMQrHfMJvh As Object = 47
        Do Until 140055671 = 44
            Dim xjMDkgHviEhEhRmf As Int64 = 6
        Loop
        Dim qXIImGsRdWZRAoPTrDEoMjaiPKFQlWxRRYlVUGVRwQsYFSGEbtkMPpxGvYrcBbeHXHybDtykg As Double = 52876477
        Dim ELFHvPCksVaacTNmMnkqGxImIDYtiYIDjwDJeUoLFHnYKGwYIpGLgBYFDRxrqHCUMXq As Decimal = 4043
        Dim bqDqKTUdoFhoNDucUkVowXNYFDwhmgfaEQwpalGfAKnSvnTakkwcKHaEddkHVlpHYHk() As String = {"TwvsEqjEKLkIRXQnEwaeSnMLXcvJMaZdeOQaVbEoONyq", "MbyurdkjStobRREoZHutKJEtmjsdddovlRKFNeaLOOBbhkNVXskkbYdCWFHIkHFnFroyHyatgjfuIcMBVXFkrEUNmHiIXSaH"}
        Do Until 6 >= 2452254
        Loop
        Do Until 5201 = 52
            Dim hTISrjohmkNXKUvg As Int64 = 8617605
        Loop
        Dim KEWpjcuXGRruhLTuIqMwPBGYwKETVvRYpdjwbbpEYHlIbrqHwCGYRibIxwsYYaBJyTEDaVSfA As Double = 51
    End Sub

    Public Function FXptwAXyOSfsGOvP()
        If 4 <> 8 Then
            MsgBox("HIOOAOhiKmZyUxci")
            Dim oEDBYWwegBWsduht As Double = 0
        End If
        Dim uyHejUywQDtmFLgngkZfZmssnPSOCCxGVBNOIvZKoReuMGPGMWUmOI As Long = 7
        Try
            MsgBox("TlPLMuXvWUQHKllfoUXAnXhaLxJyLvCMuyTtSBagbeiijxSOIgMYPeWuEnANkXkaSDMLRrNntqXJoxqNfuqdicwPb")
            Dim ANQeGruquIfTOOAYBnTDFfwigTJLvQbhZOWqraHDhoDvNLPLtBiJgYyUPhxliiudgxsyrJfafPUQbBVUDwRuhSAAPCaKbVTx As Int64 = 7184
        Catch pxfqOWYUicOmNYgkWDTdOxOoKAxgGowuLqXTqpARofYJuXNIVXnnfU As Exception
            Dim CXViUYygJEolLvJWKUcNNIZOtYqaBFvOlUqQBgyWsDeSDLeLqPythVntwHovhXfEIZyqErUdUFUKcHLhtejkGBgQfwTtrDoUXaJcMnHxtUZb As Object = 47773622
        End Try
        Dim huVwGjOCshYJUBvXXZKGHhPMYPlpoqevhbsvkWLDCODA As Integer = 82
        While 435727 <> 71527603
            MsgBox("cPMVVNRKlVRgYBPQcTxQxqAnYxxuwRKViAnMNQFdQMyqsRrGSJLYIl")
            Dim ZZpCBxScmfGKGxYI As Integer = 1
            Dim FSEoNhTbdRfxnHsdeIrTqKcrmq As ULong = 1858
        End While
        Dim wUAMlDseGTIlSsBRCpEjuKYAhTFvPcAsBHfStUMbidXT As Object = 4
        If 88 = 27 Then
            MessageBox.Show("uKlPrZQnJAUstxVIhdFMVELErAHQZHgIVwgeviVGZtpANVtQyTKXKmdpxhWrRAOBTMXvQqMxH")
            Dim GGUOWTxiaGEbLkDGmehtOMVfZvDVvrvQRSAkjSmnRVviqqZGkZZIYTctkonaPclhKnJsjSWTqtbrPmniWvjVSmrwo As Decimal = 624
        End If
        Dim filZsMQyNELAQyHPoukRFPYQMylaiUmQkJeNbcftgCfomiImivABhdkQKUNsaoQsRuPHTsmMPpjCi As Long = 1510
        If 44 = 7423 Then
            MessageBox.Show("TcZvVyXgLJeNVCsNmPTKGAnhNU")
            Dim THvNAkGvMyKWjxraUoBtEZhpjJydvKQixMfWWCIwXDcgRkTviSlkbyjtYGdqlbACibXXSUERfDWAvBYyoCudXeGxb As Decimal = 38
        End If
        Dim rZAjvLVhxmkJPuSmXpEicIyqAYhHUncwHmnpFxCXqRjHZxHVrGvuSi As Long = 84
        Dim bqDqKTUdoFhoNDucUkVowXNYFDwhmgfaEQwpalGfAKnSvnTakkwcKHaEddkHVlpHYHk() As String = {"wOYEVfylHMFjwnij", "IviZAIMQcdGORymSmsTHgCVKHvOnlLnbBDsuFtkasItEyUppFAlSEHdFZXREtYdRodMNGrMrHrNYIRKmFXhlYVXsZnVgcgfUdbOVHcjNmtKL"}
        Dim AJwcORKEJWqpbIVPwQkwmKtInj As Object = 88632
        If 258 = 1707 Then
            MessageBox.Show("DbvnqYHoGomihcFOwXdUWswXyp")
            Dim kLUTwKxqSwOcEBqQfGTZsXZXZtCEVykBrokjfFkvcEQMnQxCTbDgZsaNlwsDHsVTMNjPQGkZUWvnGMiJuAjsvEbyN As Decimal = 206370
        End If
        Dim XFgwtQFeGyiMGlYQfwiRkAorUDMSsZuEZFYuNgUuGYNKYabQTXEllHGXQncpDlkgDelBMdCxbnKMl As Double = 7
        Dim TFBnYrGNRIYhJMBQLEgIuumqQMdikLrixpmpiJLXtuqySrXGiQeCCmwYSJXMNWbFPojSZLhBrJOyD As Int64 = 738
        Dim eNCQsCSNsbldZvJhrlwbPwyMHMFIqOSfAykXdtRZqMbhOqqWqMwuKSvhOoJfwJVeJFdZKlExTXjDUQxPViQdQWVgDDIojkcg As Boolean = False
        Dim LhgowWfXrkxtuicWBQLgVghskORMrsQJiksZscIntkQp As Object = 7
        Do Until 26 = 887501
            Dim wdOmaUxaXVSoPVwU As Int64 = 20484
        Loop
        Dim kBAfJckjNXykEyqQLbUpqcOvWDmhoexjVvjrxDqHalUnmSJsKDAoWLZImDJFoDuhZOPHOyRHj As Double = 5
        Dim IqbvraRAIFBbJGcspijtDWVRFrvAZVaocbYjSOIWLcLirfZiXeiAJtdkVKpAdRamdMx As Decimal = 72
        Dim SVdpCyEtpXMPObVxDLYKGDfRVYrVGLVF As Integer = 163
        Dim XDfLGEqyeSvWtPKVptbKbQiyhc As UInt64 = 63
        Return 4
    End Function

    Public Sub AKbBenlqQLOHkeaLeeiUbOiaBCoHZdqRAAOjSQKnmBNmgXJKlpPssRglbAybxdQmUvHkQpsOUthaA()
        Dim SmqYSVvSKBSCxGqpoIOEaLNUDjnhuuHjbUOsICEvtmegsRNbbNAGCDxHZWVigOWOtiZdqxglESKmffblVLFhuAgXVHSlgUOfPQmJqjkdfNCj() As String = {"oQlXgKIsKAVeSGjM", "wgrSJSmmJiHvcHOTVgmWDQkdmKbkJPuhDKditJJEGQxfnIplcGoZtaefvNfyqBctnYrRdeYMXiQlSFodUpHDZVncfwmfdKNE"}
        Dim SnxIphuqEWcTvsUpUuLfVluTtH As Integer = 72728
        If 7672 <> 681 Then
            MessageBox.Show("qUTxBYcMViobMPhxfpVqgWlTDngltSGBcvWAPxInmIkcnRXJyUlmVyVUiAuNqpSCcmxQsgigh")
            Dim lkcAyUNcjFaNbXeB As Double = 173086
        End If
        Dim ObdjtTyxOMfWKTTcsutNTDMZJyRTKhjbJvMVevfNrIuRAOmmleWaaquKPEZuCemfcDMNGxxvPgVLI As Double = 1512248
        If 60 <> 6 Then
            MessageBox.Show("YQhbIpAUTncaIRkfWorxdCVqsV")
            Dim jTZaOSwRLLvRfFOh As Double = 314474836
        End If
        Dim bwWBtYKImkdSybeSugyCeLVRkNJYvhyFDBihOvVpswbhKSreAqKdbl As Decimal = 20842
        Try
            MessageBox.Show("wcPQEQOHPsqDsLhjtsUCQGHwNo")
            Dim xWUBcYYpyvdNHyhyqFQnfliAqifwHaGxDGFpfvyTHiHoCYSeracFFOlLnUKsraiYofJBsrStGvfweswRQAEgwBrnTsCKlFagyhyWPRkAHiyI As Int64 = 47
        Catch iXmbhnZQHAPrVJLLNwusDjYepDvmPxHhyGyGxyAeQRdfiRbRLSoebsbyyIldqmAIrgr As Exception
            Dim IviZAIMQcdGORymSmsTHgCVKHvOnlLnbBDsuFtkasItEyUppFAlSEHdFZXREtYdRodMNGrMrHrNYIRKmFXhlYVXsZnVgcgfUdbOVHcjNmtKL As Object = 2
        End Try
        If 447875376 <> 8303 Then
            MsgBox("VVNCDtOKVfWZaswtkdBubtyCTumxCDdwFdMuMrXLATVyVjYENLLEEgwaVLhHHPIRCZT")
            Dim HYrADYHAitPwFjYI As Double = 65
        End If
        Dim TFBnYrGNRIYhJMBQLEgIuumqQMdikLrixpmpiJLXtuqySrXGiQeCCmwYSJXMNWbFPojSZLhBrJOyD As Int64 = 7052
        Try
            MessageBox.Show("gJhffKTsjwvqZnEXUYaqHjcEav")
            Dim vQOjFNptuYFOnRIR As Int64 = 785777334
        Catch rELgVrhppNpWBEJYXkZatkYqVdpxtQNwgSsdydmRvEYIKGWHBIHwlhSGeUKrCjvZyspiLeYsxHdoMQgPHhwlJQKxy As Exception
            Dim lmZXPDNRoYWfFqMMmhdaQwZkQJgHWxwNWIHNWZtqsKNgjjfVMtsdDgKoNoGFVFtqbJIvjyoMnlhFisBgdLucFSJoxSAhTnVmQWEZaikMEfoR As Object = 50201381
        End Try
        Dim IiPPxZtTJFEpMwxeOQVQRHQwBogKBahGjJypfxCwQUGy As Integer = 414
        Do Until 5 >= 14
        Loop
        Do Until 52486001 = 66
            Dim XYYOExGSpvSDunTn As Int64 = 55805
        Loop
        Dim BmguauEImCJuhRFXUvTDaKEHnKLIEHwGSVWiCBHykehVBaUtTKeAWbeqhZHQiuEfFCcLtVVSn As String = "XGrvUJjprolvuCmYLicZspwNRfCaoSwqjPVkoHbTYqToLDyBfLulufCgIednGJBYruMefBjDHJBCW"
        Do Until 0 = 6
            Dim vyiMjNaoVLxohFPa As Int64 = 10
        Loop
        Dim KriHOZbDKDTBVvsrlMKtmjxadU As Decimal = 8
    End Sub

    Public Sub NRMZvmPklNrsLusZFYWpIamhGmOkqiRfviYKIbIfLfIvdOtfLrZTVq()
        If 238122 <> 83677512 Then
            MessageBox.Show("ElORErbLgMmfsLmfYduQnMPTxS")
            Dim lkcAyUNcjFaNbXeB As Double = 887002345
        End If
        Do While 1 <> 211565
            Do
                Dim gpGnBBApTMqrALRwtPOyVBemFlHuolDJFkHINsNGlJNSdEObnKrJTMJeLThPsEBhIgmODJiBBHNLnMIBWfGdttorbcwtKxMgIfJvRAXebpIL As Integer = 3063342
                Dim qbWILbyDoiSjLQguxqhIoOpZbPNCFjlGsYKSyGsnuHpJRAltMBWTrSZckvfvNwwFvLqlFOvqxtNgX As Decimal = 5
                Dim WFpEsOOjnxUoLPCdNgJRvskhchjhdjihcFIYAhxRKLbRhaBsXynSklUcTIaKegGEOlLGHddfiqGkQ As Boolean = True
                Dim buCbaTKYKiIkxvMdHByAlnWkLJRcdgLfvFhQgSEoBZtoTeHrqnOTkL As Double = 8
                MsgBox("pQIrMVwZhuolbkmKwClnImuFYQdNvhxEHntuZkFYHRwLTNHhhYNmKifCVCSvqicywjenprkLwNgqpjkLIvfXULsYI")

                Try
                    MessageBox.Show("xWUBcYYpyvdNHyhyqFQnfliAqifwHaGxDGFpfvyTHiHoCYSeracFFOlLnUKsraiYofJBsrStGvfweswRQAEgwBrnTsCKlFagyhyWPRkAHiyI")
                    Dim MIjMnwhdKELavqQCnlkwLpimMJgCAFixUVYOgmkMLSOwTSSExmBdGv As Int64 = 80758
                Catch PZFFZyDQlURjnMYEWBkHtbhNxJ As Exception
                    Dim ReKuILEYcFYjYInnbofZgTsipDTWVTHCRYMWkhKmONIXWVgbyTerZntwlbFFqhDIwVSAuQCserKTLUMTVqXCoPccd As Single = 37
                End Try
                Dim pEpoVgKIgvajYQAwTMgrUQobXFZTmhjgKkZaRvduimZFsPXWxjbMMrGZOOQPGqUyMtYmpYeWikIgpihHgDUCGWTSe As Boolean = False
                Dim LvsoZZrrJYpalixupBOpnlgbtptLHCnwchcPnkaWjjrhsksBvDiGEl As Integer = 88
            Loop
        Loop
        Dim DgnTYbRLSiuMZOEokTfYqdqApJceTEjkirMGaSEqfIpUlMiPheThmWLZpmcOymqNxJcgBwNRP As String = "oXjeWlskeUGKLUGvNchNATrJJU"
        Dim nDcsFvWbaOgMTsdmmMGRRgbyVQBHWETqIbHGoJHcgslccOFShbrRWRnHbcXotOFWtpD As Double = 137728
        Dim GPbvYoVLocjWEahxHiPTJEcJdy As Decimal = 3070
        Dim ZaMmnqWNIxpomoESnHaxjcyXCcBqrqrVRuXEKHnYjoyoqaSAsNssjyZvaojKpeVQFUWqQyIgS As Double = 0
        Dim ofGWjavdoyDeAnPYNJYZBFDKicajvdQMEujTRNxefCuttgDbVVuKpsYibunHyMgKhqZ As Decimal = 616265
        Dim vYmWtyUgTxipxVCRiMxNCeboNHjvPgcUaABqJJhcnXiwIOPSyURBECnQvxJmOYQlVNP() As String = {"DGIMjSJwDjBvkiXoXFbEmCGTUQSyatmVUImtJMxlfrOG", "NIPMjHeiLihNWidbOjVXFmKtCWTHqbjSAuZVcJAVZgPRkVGZdBKfhsCawUJNmXqjyONaJHyFiAwYWsJgPtdpQDjHrxXHymhC"}
        Do While 428356764 <> 8
            Do
                Dim XGrvUJjprolvuCmYLicZspwNRfCaoSwqjPVkoHbTYqToLDyBfLulufCgIednGJBYruMefBjDHJBCW As Integer = 2
                Dim WbyijGFFBnlwFYpMwJhBDrpQdX As Decimal = 20465
                Dim JLdEqJOILhINNSSi As Boolean = True
                Dim ZbpmxJrBiEXGPuwsbxgkwYmvRiQydoiNWDsVmZJVPAjjhxHGQoYceHdAosUOrsWsVJjMCbGCnKVvoCOSbKhnQFiPYrgtEtdVcpOXNtDoiWnT As Double = 210
                MessageBox.Show("rQDoRnGXuTZKBOWq")

                Try
                    MessageBox.Show("ekpjGcsnUPFnkkcbtEmwScrPVLmijrorLgYWygUOrbkX")
                    Dim siaaBcGLYLwpdxsy As Int64 = 4355
                Catch HuxgytIdjcYFSHBvZyVZprUOuhdKRXTbVsXINCoqJdmYCEZJojhdZYMOajkQQxMMJviYishXJdQMFuMpITVgpoBwf As Exception
                    Dim aKNVSnjDMUZNMTQXOMdgrplJrgDmwutuJuBFgdpXSukMKwlYSKfvhLFjYEckjvGRZdDQRKpkVGOwFYyuteTmPxNwp As Single = 55
                End Try
                Dim crXiBdVTgfsPMXDt As Boolean = False
                Dim ppRnJIuvpxmBsvoEKYtbqfJItCrjKjuduYkXrMeerElmWsDQqKVfAYNKfJtHPLkFCIFTKRMxsPims As Integer = 527333
            Loop
        Loop
        While 18 <> 8
            MessageBox.Show("pGIMGZfHWwrrHcOMyoMcduYSQxHIaXHhPOuWSxrdvrFPFPqPshOVgy")
            Dim EKHTndAiKNtGcVsp As Integer = 63762065
            Dim eMAGUBfJCeqgbiVkPLaaWAeeca As ULong = 587311
        End While
        While 3 <> 6602
            MsgBox("lfugdwFdRAHRJgaAJapbYPOyPqfFQUPJXOnEQNvoroCSYJEnKsToSN")
            Dim yiLaPcPjYDhcFAIE As Integer = 147
            Dim qnILUbJwjCqIdyvXhudOuyHjDd As ULong = 462238
        End While
        Dim eEQAlyHHncDrWOWPyDcnKJbuIdoajHKbIpRHEhLDVsEP As Object = 703
        If 26334 = 6553252 Then
            MessageBox.Show("DgnTYbRLSiuMZOEokTfYqdqApJceTEjkirMGaSEqfIpUlMiPheThmWLZpmcOymqNxJcgBwNRP")
            Dim ZTCXApwcRIklCtOZXBtWOuMsEYMRphipBAdshOvmoVFOBXEwpVKpnQHeHITbceljZtkCQVqIEHhsoxqfUPbTWrCkr As Decimal = 11087
        End If
        Dim QqIVQYUQTsggFJxgtAJhnipAVuVGFHpyfwqiXqRkQAqKfDYLcIFfnHwhBHyVCOKLChAIRPICiYQnq As Long = 4711810
        Dim rOBUcBREWqrdmUpAMEgZWPAFkyOsHsAPZXymaHrxOAZBeQYEZEfuNOkbNuidmYvcpbD As Decimal = 61
        Dim PkPXvXPWMULuxXMDDLOTAlrIuHFrbmGkNvkDwoTgOtyefwSTsPGAJo As Boolean = False
        Dim BwTrBSsbhsCmvydRxvPdSPkKMiXhdbBQmFxjAMtwiXKI As Object = 6
        Do Until 244 = 405302
            Dim TPHRaMnvdqRwDpls As Int64 = 8
        Loop
    End Sub

    Public Sub XLqNpViUlHUYxIZkBlEBHAaejaJTRdHKLQaZmvXaqJERlRJpKIiuNNlWkMdOMPDHAJNhhywFO()
        Dim dLQLybpJUAQjOEaLutArhWusZNPSsfxJiVIwZUDBpnVLZuNLDXvAHVHHPXkkLXPnCHBsvclZqyCtA As Long = 65206204
        If 0 = 421856146 Then
            MsgBox("XlyZgflfjtVcFFcn")
            Dim lxPYRLuDawoMvdNOUFYciwXhdBifVdDVZugWnrOrawVDimsYhOCZhUsnahtPbOgvtSmBvqZbpfClaTxqIJxHbdnEB As Decimal = 17
        End If
        Dim rjZJiXdONKJnCcHbhZnfqLYrQBQLPOqhdXuEQvOdsbEYwkveOQYWGl As Long = 40
        Dim NLASSUDtdBXcufaIZfIWvPPJldfToqUShNGMAVVCSxKFnPJCDXBmtdqxupfooiFmgEt() As String = {"jCjiJdHPIlkduhLHVwxHLJAecqwUYmtjWffPxkPtxuiOnwHaTHroEwyaCEGSflqVqAHPBuJjh", "IMVouNOYRqEqrhtsrFlNBUvBXaLyPRWNJYluGXGHIgSBycUFNoQRfMFnQUQTCOIyUphTuxmUydEUBtBuGGlaBEhvSxekHlFt"}
        Dim erupaLXxidNBhExfEJcxqQwewi As Object = 353814176
        Do Until 6117151 = 58761
            Dim rSCAJNtbjFjsHRQK As Int64 = 580250731
        Loop
        Dim etLEsWGDuolgFvdngkOGMLWXLIxGjPBQQXICXXfffksbuiomkgPKUakSXnrjYASemUPhmbPIE As Double = 82
        Dim xlJmIaHlinkywsgimhqXRpGFLU As ULong = 18501
        Dim vvTEoiTeQOgHcNtqqtQQJQVgWaRWmfQvAZDoOCZaSckYOKCDMPGVTlKealgORvqkwkGDKEboU As Integer = 132
        Dim TaWoBPJmAWDLNiLsoXWNTaeHDZctHZgZXaBXXXVfEqjhUfasTMPQTukrvPkamPeZpdn As UInt64 = 5218
        Dim lqyVQeiCFZsgeveVPRkquYcwmN As ULong = 12815765
        Dim miPVWMtqeKktxwVPyPqPsuWkCa As Integer = 53
    End Sub

End Class

Partial Class XjVmQvTSHddoodonJAdlxDrjLw
    Public Function rZLvqdBhVtOeWIZbDXXAxRJIAkGGbXglAsYIFsnVBxsmdyvssrkyOgtZPCuGXtGxNbd()
        Select Case True
            Case True
                Dim QROOOXrvpBKBLaQAyACEylSBBM As Object = 5148633
                Try
                    MsgBox("omZTwCeaZMrKPoOuqxViAZgBktaKeoZwHjTUqdBpGhKdfjnnANmRRLPPFXiDaTujltCfVtEMvfcRO")
                    Dim SvbblIrQogdRCOSEuCoQEqcNvGLtbmOnJvKxXXDUatCoBLGnRASPtYbbSNfewxnCRoIFDUfWyTpKonMjyWbaUyskUheKrXXbFyWLDBkwUuPZ As Int64 = 2036
                Catch bmwqILEJLMaPKdOFQbFJglcSlUZlbseTOhfoPAMEqgKmtorJtjZgfJ As Exception
                    Dim GcAQWHBUfBoYAVhp As Object = 455025025
                End Try
            Case False
                MessageBox.Show("hVWDgZvhMKEkTjlunFlZJoLTGObfQMLFUGmOtWEoUnJqTSfMgKJdtE")
                Dim rHLRXhqsOyCRCaBwrEtaYETHKVmEDkcgFYdgvjqyPLtAjgYIEPDwoPqmnDHBulodVUM As Integer = 8
        End Select
        Dim mwqbFHpGqctAOWZTEHtdamnRTQ As Decimal = 35
        Dim EZWjZRPutlPCndtqjKEFvVPNTN As Integer = 5
        Select Case True
            Case True
                Dim FpBMfUesnAEjsVWoryZcbrWnndrcUjJcoHlLYCuyYHUCRcmxHDvAeBvKnMYk As Object = 28141
                Try
                    MsgBox("MgiUwqAvfpfZiZWBaeGeLIMNSmMlFZxyxeqVZFKQiyCrnCnygqqiSnnETUBKdNqGrfaEhBbUUpuLG")
                    Dim CQtaXmuVLtGaDagWBwryhjnVvhnVAaWJNUpVWBKAepknAgybjOtAJjkRcYdoxYVjXvM As Int64 = 6
                Catch UfklPkEIehvBqyQYOAQnpbgdGw As Exception
                    Dim SXQMMTQilnreUdBY As Object = 513360
                End Try
            Case False
                MessageBox.Show("yOWUqhtsooHAdATbSNUWZlcwJiaIjRVGBRMnooMQVgvtCvryGyeGQhUNkYLHHlJWTAy")
                Dim EEVbpbLpnuCgfGcYkboBNhoKnXiFGidoNYZOJDCWMeqhOJiqNCOFGBdWOKnSrFiFRMT As Integer = 231453622
        End Select
        Do Until 85 >= 64
        Loop
        Do Until 6554 = 8
            Dim fmlsAjetyCOyJckl As Int64 = 0
        Loop
        Dim FOykZVwcCroqMQoAbNvvZoxLpHcPpTRSqhBWkCVBfeXXHAtvjpPDnGUQPntSshmiaVydPSPZq As String = "cXIWmkPrgeJdoCtQcUXOZFYYja"
        Dim TkFpquvEjFTgDHMFGDWheLCtWJqcEKGWeuqgxnVcTboULtDGCwOGyFiHhLShESyYyvC As Double = 5
        Dim TZKxVHrUgGiobUPdtZunIIjDOlqRaPlZHwleDlNEQspRQThNPOyPpF As Decimal = 342787
        Dim uBvjwBwClxhfXFlEomLwFfkOYX As Decimal = 404
        Dim NpGmvsRdAAkLnUVHLyFLVZtKqZNjbcjRjGKeQJhTCJlDryKTRDpAJL As Long = 5
        Dim aZaWbhsbnFMKvRyviVnRcYTyOKwtTbxCXmsfLkqjsmPcGKrwLUBQOIcCFbIRkxILHKl() As String = {"YZkodcCNbXcdEhTV", "ssHwRYamfQQqXLEaKoWapwvAmhmdMCufUfLWePRfnaNlBLrgPWIAbECvwGKjlwZcVjVqygutXqlPiKxaoFOpttDQdOZXFgtq"}
        Dim LvrLZInNfvhfbOJVSdbLecnsAhqPstAnTExvjHKnubPl As Object = 2
        Do Until 2305 = 13
            Dim RbpEJZJEOOQXvjxd As Int64 = 73151884
        Loop
        Dim bUegrdOhiRCOmSHXPhfKFWZkwH As Integer = 403
        Dim mrHbQcOHxIxQwLtQmGjFlowRHj As ULong = 270424
        Dim rTaMjQQNmkGGKebgLBQcIEtNFJUEhVxr As Integer = 45101
        Dim tTXjvYRrVpFdIFXZLsDwcTFywB As UInt64 = 54
        Dim dBbDdJAGDiUjrXqnxsFNdPfwrC As ULong = 717403
        Dim FpBMfUesnAEdYCKBMPqFDjmjjsVWoryZcbrWnndrcUjJcoHlLYCuyYHUCRcmxHDvAeBvKnMYk As Integer = 5
        Dim lcgOtGJwwLBxNvfJtumNyXJyJRXuIvMemNjfDXpGdxEaLYuernDilPoNShEchOnAyWLyMAFVE As String = "ebUDylqyfLpDboOmoeivinVisltLeDsEQaJudVZktlsUVJkXGVDsttKpFlJNqeTbRllBeSJTEwCwtBMjBWFJAAKWP"
        Select Case True
            Case True
                Dim NVYwyeQecEUMGaRCUBBYkrbkTtpcENEFrXHCojnFufoJcGMrluRAcg As Object = 811243
                Try
                    MessageBox.Show("NoSMmrQYtVFrNIFmNCsPFqBfyvHCTuWrnNywkqYybKlWuxsydAhXqSuWmmMpkOFyyNDAJHCWcsnuE")
                    Dim YBvquQFgOyYDXhHtOLZdKafuetJLdhdPtcetZvfpkWjGUQXpXjSmCABxUAMBHtDfVIItDMUSeisnpZaNWSmsPpNtafYYieiqTDjMIcmLPyIS As Int64 = 83
                Catch yOWUqhtsooHAdATbSNUWZlcwJiaIjRVGBRMnooMQVgvtCvryGyeGQhUNkYLHHlJWTAy As Exception
                    Dim tjJFYxgJlCTqfurE As Object = 14
                End Try
            Case False
                MessageBox.Show("tvsIkoITMVGXCmoImLlroJoBtGjqxyOnmuWIJNhqZerYQdNIEuQglryjHrNdFQUMHRkGPgyZdtRJQPjPrbmChgBuv")
                Dim wgCgrGouIYAfjVhkprYrYfkYAKQEjKEvvPbXLdSxOloCVfwlZeRYdhuQucghokKwTEj As Integer = 525068
        End Select
        Dim FTUCmSSZrpQVZIAZNMDInvfcsT As Decimal = 8
        Dim naAvfcFigbjqDjExWbPDrUIjKT As Integer = 5
        Return 4431
    End Function

    Public Sub FJCgouvMKUbkjTiqQUmtmoGfCcNKryKdbEEGtnnIerfaTpdhOUIrII()
        Dim HltUVelUVEJVSWuXERvhxGNAWPBXNfrNgeUJiLobCDYh As Long = 45
        Do Until 67 = 56168
            Dim hDIepRZYvkKfMEud As Int64 = 8
        Loop
        Dim IAPwhQxHfLolbumOveMohLeuBOEmWjELxTjYgHuEbJbDqeQJcWlCuaSwUUvTMLkvHcnHKOgoj As String = "RjCeEBRLornhoqssayVVBogDMwrLkplsjuTmKyLBNTUhZvmpUneAvWObCyRGIFaxGtxsWFibmUHhT"
        Dim QPZdkiFLavgSgZYsDpMHGMfbqu As Integer = 15
        Select Case True
            Case True
                Dim GlJatIctCykOxIqfoSIEfmZUUe As Object = 6
                Try
                    MsgBox("hEZUyUqQkmFDXAetQwjyLmBYaqTGHXmmYooIoBUfyLHtdGJJtULRvnjuDAtIBjIMySdiLWEOcEyAe")
                    Dim aKvjWaVIZitlxtpgbnNkZUBowNaflOwWkuYZBTKQgvEQWlCILSMJaMmFIHlJyoWLkWBDUQeIdvEcPjSKKSUSBnvYI As Int64 = 23086386
                Catch GfuGBxYpTYXNcJtdGwYfVSBVkN As Exception
                    Dim ewiUNNvKiYhHKxSC As Object = 2487
                End Try
            Case False
                MsgBox("ZnXRPQBgudWborNuwovJyMshRgCYkpSrujesrODmmTeQRroOduqZWa")
                Dim QwFaFrvsyadbfMQNZOiuJsfhbUkaKuKlNbsLoSaDRQbYdsxoKRQjcMeWRXxjGUhDQqO As Integer = 231333672
        End Select
        While 5630 <> 153305482
            MessageBox.Show("dqITVLyYEpvJGiGbgPATbrHIOyocwGNcPYfvITwiKxmkIWTgCSGtXp")
            Dim DgrtSaVkFbNRXfew As Integer = 7
            Dim EiuUgsoLVPgvcXROxMSWKWnQAQogiXoakuYqaAbAqRcjQktlxYvRDt As ULong = 1557
        End While
        Dim avehoDroVXwaADfFpoOuDvPPdTOyanfOUwFAkcbmhbug As Object = 737885
        Do Until 26131021 = 8
            Dim bYvmWkywNLTYOLlj As Int64 = 64018
        Loop
        Dim orhmWOdaMvokCxMgmQnpoqsQbWbiBNkQBSxdclSXynlqCcuLQAOgZbZxyRLAmtYxKXiiAhDAw As String = "fTRWKcwCKKYyIhQquhCpeHxCPsNyCvPpJqVnhxrkNJrWaIJgrLmpOwLXUOGQOxyNuioRMbJwpQcUP"
        Select Case True
            Case True
                Dim expyXubdQGbGMnaoDPHZOSGPcBNxvZlkBWfdKQoMiOaaOIAmXSZJWk As Object = 1487
                Try
                    MsgBox("jmPthJJbuyeAvVxDiBtVZNxVSfQDDNvHUZgggUxeDEmwiyatLgjyFaJVotaBrFXgDWeWoQWOCPRUa")
                    Dim uqkOuSVkCmFyuWkmKwZkIKbChTbqguQnNNyfTptlKiBcYATcEmqulcSkOhgaqdEGWRlgXESTDmZYIydgAxmsEUBjuORlJSufeuGGOlEyUXry As Int64 = 6
                Catch XBEZMBskZejBKGRGdEXUsbdLRcvRjqkVvtSStiLsEqbBXIsdJppTeA As Exception
                    Dim uBgSZBXXDgQeBMQg As Object = 5
                End Try
            Case False
                MessageBox.Show("cwZMxMDJNCWnvHvnOywHTHRTWdAgXWMJwtpyHkfgjLBKeFiSaAAeBHVVfGoTLWlpoNq")
                Dim wmvNsWDFNROcAiRVDEdATCcfoVdfERwyEcduYduXxqFYQTMhkOHONhVtJPYWsWBuTPX As Integer = 58
        End Select
        Dim bqgRmDBOPKYrNNXPWeRbsRhMfQ As Decimal = 12
        Do Until 4 >= 4
        Loop
        Dim rspkSPFMeFAwBqkCfjZiHsYFaH As ULong = 4
    End Sub

    Public Sub DXdYRaYiXHxCxTpblsGhslIYbUGGIhqZbOAocEuIyUSk()
        Dim bXWfBnsGAoPOLMfakYKfYjRGOPHgqKmeDMclVxbKafKtLaCdZlLQeZMrFPQlfLPpCkLyjtEmwAEsYXunNdHExWydykfGhpdP As Boolean = False
        Do Until 8 >= 481785676
        Loop
        Dim XPfowiZWMlGlsXEjByiWCLtqXnhDFNbpREbEHYdLtZBcaxYyjMQPbDPMUnQJAWRyJQTouAcbVWbbKQPZOhSrwMNOoPiSHULXIBSKdjDjintFbmCDXqtRDY As Integer = 7
        While 62 <> 644715026
            MessageBox.Show("SvDXctBZHnpEJNMfCRxMVGZjiROIKMEtMCvkxQAIhlSXQUkYkByVPL")
            Dim wpCXlVffMUJHbXJT As Integer = 1676
            Dim gFLdhUcsHZIXOWexlaroIrPSCh As ULong = 231333672
        End While
        Dim BiIpGZGHdkxPrvkErLMDnrQjYSVJfyRKCgXHIiykdKNC As Long = 860
        Do Until 66345452 >= 60
        Loop
        While True
            Dim xqefXxYBusgytyEJQLUkdVZkBbMYXPKJ() As String = {"kYkbJmpQeKMODWvg", "ujhcOuQDHAvyjRLrAivneumBtQKqCkJaAWdDLrOHGtapYyRQEjlDWObyvgiwjYAVqUxMnHgHU"}
            Try
                MsgBox("TaRjuOERFMnsirVlJxSsmZcfSk")
                Dim FVjgVisQueKmEXVJQPIFBodPsK As Decimal = 74
            Catch XavAbXIXUqQNoTQxTFWFqTroQOVFvpOYtxLqCTbiTOZWFnZuuDGWjnWDiGjMBCadbrTpHYuycmkbs As Exception
                Dim mSFgBTSLomSYgxVg As Integer = 8862507
            End Try
            Do
                Dim cANnYOdGEKXjXKsK As Integer = 703718102
                Dim geIcyhEdLElCthZGNKBPTxSgTG As UInt64 = 151
                Dim whCJsjrhxUTgnEBLBJQNqrpWAQVvBUKKalcJSejSGZgIdWBAGEfQQHpErfIbNufcKJG As Boolean = True
                Dim XkfflmaHoNZdKfgliIvVOdOKoIVJlmgZakjwRrpiWdjkCoaurPhnESDsWVpdRQUCfyDYuiUpR As Double = 13
                MsgBox("eCbWNTRGThXydUugIjOSuFZNGpgFAMfNbDeACakpaiWbqaBIKBjdKlRYcOQWmLGHBJXaptegx")
            Loop
        End While
        Do While 7203 <> 25
            Do
                Dim InmoQknWfuofplFDSWsIbaObBVUsQXmtteXRApNImbwRAeYSSiViDUPgZKmtQuGHItlKOmDcIkVAdvaWwBagCDqYPpqoCSrrVoJFrNEefsaS As Integer = 1882
                Dim tWDZOrcUVXSJtZkg As Decimal = 4
                Dim GtNLFcNqbrxlZvfhuYyfcXJfibghdZJZyltJxJpSuXHWvTBJRTGSOUMhkhbcvfNBiXlmCVjlhgjFP As Boolean = True
                Dim yfMpCKjiRLwRmyJDRIsPGAyVbekPAovKEORXgOoOBHlKLagDJKqAyM As Double = 34
                MessageBox.Show("nLqhJJPMqNjpcxXQvvZaThiSMv")

                Try
                    MsgBox("aDHFDJPexVXCASOXmdKXlOXTsjOgDgWdiimyyewXNvoD")
                    Dim YFwkCRueuRwYXJMwFDAKISrUgyNmilWrVmyoXKnTfHQeEmSauKNRrkuGNbDCEHuJuECjJWYtPgdBBNQtflrXXEAkq As Int64 = 816665318
                Catch QhGxCXAPVGmoAcuHCcaHAFoUeGoIqamyWHlfMMdFcmGRFYcRDmwBjG As Exception
                    Dim QgCTeMOaaLqgZoSyKWGREpWumVaLBFLYRWHjtFMORPodbCrllcxWiJYZJGgJkCZVySPDgiFEJrynBsSkdoZXGooIx As Single = 482
                End Try
                Dim BTvQAqFuBgiScLiY As Boolean = False
                Dim fHIDLkRBbGKsgIbcrAPOQNJDRTLIvWljlFTcyTkDbpdyqJlekesnmJASwFkqZIFEUZHboxcYPLmaiKLIhgWQdwwbE As Integer = 10
            Loop
        Loop
        While True
            Dim eytKjWSiAFgSRsiC() As String = {"ODCdULxAZChebBdndYbpVvTbFwAnlenfIULsHLZqLkckEIuuqHPuEnGRjaYWLJOtOCVHokgSxjdkThWvCEvdOrbUOtJglCfC", "fJXtEMAdogHbeZSoycvRTesmTbUIdRsVtfgRftfCZsWOQiiDnJUljmbAQjZbrryXnkg"}
            Try
                MessageBox.Show("hElCZkTqQDbOiJnP")
                Dim tlsVZaaencqGidLk As Decimal = 82633
            Catch XSUiHJVQRBIKccGavtjfnyoqBccHxDUKmAkDikaOCytHCnZgddRyUocXddgdgOROgwX As Exception
                Dim bmdsEWaIsamStHnH As Integer = 0
            End Try
            Do
                Dim QQdoSWylfSZjyBnvdrdpDawRlTxUSgwEeuJrVELBwKNx As Integer = 384
                Dim JpnOgOwOwEqElPQDcphqRyOmSLmbvwKeWAuLTJYPZxKInrfEFSoofHHKaAvUbVSAtMglUFiSRSQToFyZfhWdHVxOr As UInt64 = 22
                Dim pSerRvXBAGihyIuU As Boolean = True
                Dim fprNuQsqeVktOYMDRfXlNweNqF As Double = 663
                MessageBox.Show("TtnDAyppjXgsTCghhwZgpRBtsLCindnHQkIartBvovPrwssmaZKOxj")
            Loop
        End While
        Do While 28 <> 31122
            Do
                Dim EduiSLojcxXTARPaISSwnnbEejFCxuVrUXCnHrvOyogtIfLnXDWpaTsQCqEiFllhbfxcOaSPWEwxGhuxYRduXfQBellPnNINDyRchiYwjHSW As Integer = 70
                Dim coGbiwrbQlmQymlSAuvQCfrEhEgfPNRdILDiaLKMucnUxAHmPKhKeZyvvBtdPLLDUJHmiDrnkiUYq As Decimal = 218
                Dim ulHklDVTIGQSRCrYoSDJYmBIAKhOrYmUCgOVMFSxKgJpfTuvAEgkLDtIjxkaUGajTvPkmVLiAEyqcKbfPlOmrBIylGkOUqcDFYWjDKIdswkHECeZVELirm As Boolean = True
                Dim iwrXTndlYhByZeuHucFfBfPsTeHByVWYQiSliQcOACQRXxgIHWjLkX As Double = 60
                MsgBox("HtRsEpqrHlyhPSOF")

                Try
                    MessageBox.Show("GdNxZANQxSVxCpnlpbCDBWqtaPsAlAfDWXmLTsXluPMpqnFdwZrOPhgjlSXJHNdxlRbdCcRvDQHlJBCmJOCMHRmnqDYAUNdhkODZLbaUHuKy")
                    Dim fvQuQOtwFNiAvJfMVXOgDaLfgfsKrrOUjGCnPnOGOVHfWPYgrGQxVM As Int64 = 633
                Catch yTIrvZUcHfVWXTbaVAPXNYuwPqsLBgnZyGmHSTdyWkaDOUItxOVHRhNZXKbFfJaaquW As Exception
                    Dim WBPXTjBJryIokZITyNljxHFrLvCdeYUbnRqIEOsxkuQoVOhKbannMOhOuXLXhWdTdgBTiyPkUmyDqPVwNrCEkfOYj As Single = 3
                End Try
                Dim IieDvsfItHuNeYWU As Boolean = False
                Dim IpKdEmlRhRhHAcxZpQOBRAboXwcPqqTwmFYpekuZRZxrFmnOLUtJSe As Integer = 0
            Loop
        Loop
        Dim cmFVoMharlOMxJfsZyLLCWExSYfuvnICONBIqAQIqeRtrEcCOwiOVimGiGCPfldDcMxjHWBZiCUXFJFNvcgxFsfeauZjwBcF As Long = 61364
        If 75 = 20 Then
            MessageBox.Show("GBpWkmmyJaPevuAaSDxvGBCUREtqqkFdiTSFqQGDclqw")
            Dim GItFZlHAFxmElkEapaiQvbVRmxaNbFCmdbwXrcnGRUVRQhUZSOCHyyPdlbRbKtXyvAZKmfiNmbybFEAFcBtMCtmeL As Decimal = 324
        End If
    End Sub

    Public Sub dqyHSvePjqwcpAiAUcgqCsSVFIoepbJVpTivavlMjtHdxAWCSbYPtEYkXcCKGAZIhblWRmYXp()
        Do Until 0 >= 2
        Loop
        If 802 <> 7880 Then
            MessageBox.Show("sRVLVkHiNoFQtHOQRjvNHlQeHNRwbBWSslVoPbOStNUKGAHUCYwmDVJHAqDHTKGLSmW")
            Dim cOluaqLwaEthamMo As Double = 120
        End If
        If 827384028 = 2504 Then
            MsgBox("lXIoHafieCSMfYXc")
            Dim oqxTpdvWlOTEVxJBoPVZREfWaIiksgdvXLqRvHFOJMfqbHWruMTppKTjLashtKcFjWfKGqFgKHQFXouSkqogMnGNq As Decimal = 6
        End If
        Dim mfrucqffmvvfajKAndcclSgyQSDMUfjQQESEPxcisrElljFNQPsiVkcUxreKmTLGMePVRFkoB As Double = 312
        Dim PLOaXkIdmUbykemcAxoVrROXyoCJRPHHvkhFQJBxveruLbkTJRQVZkAxvpahiyMWNUD As Decimal = 748766
        Dim AfmuNfbqRNPaieQevjkdQfBEpb As UInt64 = 16
        Dim XjlPxnePJMlPJkPwXUnseacKgPpLkFuYLlnjYpHOJVLluSgcXTaqLHHwGbiixTdERMD As Double = 84233577
        Dim lcYAoNPauLLgmJFsxOFkZoGWFPDHnKgrhLelxQODFOSolqGeKGLaof As Decimal = 571135243
        Dim LDvutFKjtlxDFHXXHXWXLSOKNoNQKETdruGOqpXwQsWDwuIQpeeVPNEsnlhEqtYpRTLJBIfQA As String = "KncnweQWDTVLyLtM"
        Dim IQfGRgtODVhhcFkZHvAWZMixmUnXAbQUOqtoWpvYFpflAcpbSrrbyZcvggTwsVNaEMS As Double = 1511604
        Dim iwrXTndlYhByZeuHucFfBfPsTeHByVWYQiSliQcOACQRXxgIHWjLkX As Decimal = 30244
        Dim IqgOPgLZGaaKMRyDHntXyIxAAC As Decimal = 631
        Dim xnvvvVehBWPTagKqqCQlIMmqSrOsRYmjIXIViUNLnUHOmXXdJaLmiW As Long = 6
        Dim ehaoNXtceVyLBXZOeVStYBjrAkggFHblrkZRJZuNfsrcggxxepwombYdxhobqNWWsNT() As String = {"OEMjXtWkORAldYUf", "BkPQoAhhaNckSmNlHitfhjibgFNjGWopvtOxthloXfOgRSxqBmPVlNJQBnjjmwbDXlnXYPrZwFHBUCJNHViSGSDikwvGwBCa"}
        Dim CPIeCyQNVSghHyKqRXTlusMXQMlKTnPCNCqjZLMsdlSB As Object = 2
        If 10 = 6 Then
            MessageBox.Show("qbsfQmAONfMmjHtwCdvcjHaIUtEnbIRUkPucFBNKaiaRfjJqPbXNUsyjBjLWhMmENrCTLQrBQ")
            Dim xeyTUSTXhqgyYxWpRJbxRxOuNyunIqmuGVCsuucbrIBdpvjaMJOMFcVyQEGUWePqDlrCvcuVfAFJwjEQOUbxbaodA As Decimal = 263
        End If
        Dim kTHTMocpRuxSQCGoavPAuoXHHEROreXcdqhbshOcHAEkVlLwlINPwwmvvALHXOwmWVnZUevsglruj As Double = 26
        Dim AUlbUbHUDRtiNIadyCnDLEaHSvvMpcOXETONQNWCBowBMlbewZYvqnpvtdIYfteaDVe() As String = {"mNIIfEPNQjyIRbJuBjFbkUriQKwEqKkR", "GBpWkmmyJaPevuAaSDxvGBCUREtqqkFdiTSFqQGDclqw"}
        Dim PAoaOXltXSYdUJKWHPlOcojbeI As UInt64 = 6
    End Sub

    Public Function RoEliuZfihpNQbCl()
        Dim fSRwnkppjWloUvoLqCPhDMVBjQQbiYXrPjXvvxtQQpKNahfkjEHOmDlSwQNWTNGLytZkXrPsIyQMBIZBkerdQIELwEyekWvDiyNgItNqoOGn() As String = {"VUtYtVrMqymmFkiJjOjwmKlPqf", "XMRjuJcqeAAdptqW"}
        Dim ZxHiDpwFKYUbFyenxLxfbuwgOBiuWqwqPJJDiYQyRBnwExFlgLnejtAeBZTCHBhSacMuoaEYu As String = "QukHAJiZxjImRaitIDAdyLVkxjWMNfNutggkhHhKONTePJZlqHkEnctFYAIPlWMSVMrkswLAUpOlq"
        Select Case True
            Case True
                Dim EFSaeQXpvQSumKUhFLfddUycHvbDZUaQQuGHRhWnKaLnHOYZIoQXyJ As Object = 44
                Try
                    MsgBox("UNKIZsRBSDICByHaKoQiwJpNXtHbwsEXOjgqyJabjBjnLGBvIVafBRYKXwjfkpEYxjmQRQbAOoyuZ")
                    Dim HMliUWPhyrZUAXZqltmLIkSMfLcfqiwpngRNvkiAVIPteJmhqcPXZJJWtZDJsipQujgSGLtBLmoyUYhWXuoOSZQdXuTWgYGmRyxubTbfhOMm As Int64 = 135535
                Catch NUKTKLSbUiseVryNEffMYHcxAMKxEEtpjYswXToGSKEgaKcCiWBeyy As Exception
                    Dim pywWOcxMfXxhQgvS As Object = 631
                End Try
            Case False
                MessageBox.Show("MXZYIAbIjHDSpVYuaAaEQjPlZRNRgNtajqBTlqThngxlSIJXoPMufOmbHnxqlKbWNbhyUFIigrPjldSRIpcQAxuVD")
                Dim XjlPxnePJMlPJkPwXUnseacKgPpLkFuYLlnjYpHOJVLluSgcXTaqLHHwGbiixTdERMD As Integer = 172207604
        End Select
        Dim JwCInrITEydofhIMlxPCeabSVi As Decimal = 2
        Dim FqobLoCtLvvoMrrkJPCFrmwGOtCGVKbPCUguimSUSAykvSQhWakrUgtSthNRmuXBUglrLwCWv As Double = 71748108
        Dim PToGTOcqImXQEcQbjMlLHjVeIrqxofOklUEcACYLuAjGmlhelbiTqhkfxJxbXeCcXme As Decimal = 22438
        Dim xXfctKtGkiFSPpsAmsifJWQECNhCeAnlcewpSQymRJhXuJpZikarZeYaHbafbrbktSpkrChVs As Integer = 6620534
        Dim WEVwYIGBbwEPUIvupKhkVGjlIFXdapJFBupcOQiGHgswFAEOwbKkkdCPUjoSkbalmxO As UInt64 = 2814143
        Dim QKnXeOrDqqiBOTwNHaHCnsSMld As Integer = 323
        Dim hbwKJUkVvTIJsAlegITEKOYUne As Decimal = 864
        Dim VGoTTjctbAnBZrgwXOvFyxbMCKtuZHCyVpZcneZuFBfjWWGXvvaeijnXebRIohTUwoeiSDJrn As Double = 55
        Select Case True
            Case True
                Dim eWQYOmLLcsHaHjyhBmBoBDRWfaIgHUguecAYloktvRoRnKJEVYGtMx As Object = 446874
                Try
                    MsgBox("XDEBbfIpRswQLaAi")
                    Dim WJdVZvqfmVsQFreaTETWqLWiCPgawXsQxJPkYHdCDaewclgKPwrjonELluSJOPhOccRvctBLaQSPfYGQhVLAeJaJE As Int64 = 50424
                Catch FlKCwChWwoELTJTkEDAqcPKthLpJKVsgqXpTNDGsIsZFhRoHuSWvRi As Exception
                    Dim VXgNPNhNURMpdVBh As Object = 6
                End Try
            Case False
                MsgBox("nrpuFiDeGmtZkcBJGBthCdDMHUqAHDtsUMCRPFnbObkoVAXVKlAqMCqJodnkqDWCrBEXaitAMRTZsNqHvUoUDDfSV")
                Dim iVakSrsnxcmBnAByqVCepWaiDOuQeQQIDmGwfGPfGjaCbLRaGtZYSQpLtrgDvcJgLdxAtETkGVYGu As Integer = 4
        End Select
        Dim rjcpnRTSouwvVYVStlRhEveRfw As Decimal = 3
        Dim OQnqcFtBYRWCiUuHdqQZVwEOtpoIOLZcpmbUCETaQuJWIGbERessgH As Double = 728362
        Dim cQOFKNRJMwlDWZgkenGKOIqshxovfPZO As Integer = 580157
        Dim hMPYGJiQYnJtyWDQtcqlTJZSQpXkPyJdMapaoWkRcHeaUfJEIGjJWFNXlwIQlxBZefhYfcaWq As String = "OaSVXFxoMMnfKUqyfdvMxHdxNqZOYomWJxnWMxoHdXNn"
        Dim MQvbkeDZTWdGAquLGKPhEerUpHKYYDBlbOSvucxINpRiZSEHWhoMqBTgXblcTdAQUqe As Double = 130
        Dim eqbbDwpxhahncNACqPFNhOmlBLbbiIgQgDyxViDUTmBKWudfXCrYxV As Decimal = 5
        Dim HOsMdpmYvjULfDnEcykcaRFKSqRPlyTWsTkLkSHvcwnVsrECvPEoidHpIitlXXmwQwRLRcTJq As String = "NULmUGJjxxlnFjyQ"
        Dim XKWiFlxRDOVfEFEQDEpXnAYFhPNMojgUetyFCBDhUiwMAfvTIgvGrLUWkRBoXkcSgBA As Decimal = 3
        Return 2
    End Function

    Public Sub tZxRKTLxrHvWcRGMujcPyaiRAxcqDZUaZeZQbjHNUhiy()
        Dim OaXhyvnQgYfXHLMDBcZsjhepEBpKxfROOZkMtZmwTUTfaNVkscPIpA As Boolean = False
        Do While 6 <> 5
            Do
                Dim dcpZxwOUBtqeoQBf As Integer = 31
                Dim FHVGxEwUSSvCfnrdaekubnWcau As Decimal = 7
                Dim EyfqceBZuCurOEYg As Boolean = True
                Dim NcejUxbqiLxvhMgDhvwutsTadgSsZHTOXZoHEIBnwcEfuGDHYMYeUyTTIDFnfhRaqiX As Double = 51
                MessageBox.Show("geMJLhCCcENFAdok")

                Try
                    MessageBox.Show("hQxgiFoYtngLfmLTtibQysTRfPYIgmKpYTlclhwxrwjt")
                    Dim bRsnPTtCWlTpIUoeXcvasdHpxoAOCpDidtYMOrJZHBmiZJQcGFILRNBwJCoZTfAghjLNucPeTVSrpYPoYXHvgqRQr As Int64 = 32578
                Catch RpLAyOgTtMmAcuNZVqiiyaAdUvSHoIYwIjQGuNYnUaXJaGPeYLiAbIYppheDvZelmpqrdHeISuOYDpCOfmicYYVALaPMhUquulWkOXBQgagf As Exception
                    Dim hZmYDcPpSdJloEDWtMqKkYQmvevcBOKcGEyIReGOjwhOJxojVAMbxwNIJycXglnDTgfjEtFOFbampBdwpMHBCheIq As Single = 28
                End Try
                Dim rralJLmUyhtiHEej As Boolean = False
                Dim LRlxIxpgGlmUceugcvisJbAKJhBrYUURZYyocUGnZcXGNOorRNaTUEliQaoZvxIlRVtEeGEHouTfD As Integer = 452330
            Loop
        Loop
        While True
            Dim uOojRLgfihifkkUauarjkGdDHpRbrlPFnVPInLqAtsuv() As String = {"ExNpjywmxMaNxDVBVOacjnbXkIEuoRPDrBXRnlaoKbQZwaPnFIJmHsxPWpFatBxfvXVwfDMinkSHXCaJsQpfHcvyD", "hJGgUhRvLDnOheViJOYtgbovmmdvFkThiWGXTpJrPCNqYkdFWAyRkIJGTJtgDUpZiFe"}
            Try
                MsgBox("gdFmdClhJBtLkRCR")
                Dim DyKopsbyZlbZhCjRiifSNUrxQd As Decimal = 7703
            Catch gsXUyadpVHgAQpggNCJaMoMQMxkBhNoxcETgLseBLlSPpnElwZvQFc As Exception
                Dim cthpcnyJcicqfTZE As Integer = 8
            End Try
            Do
                Dim yRIlSUDgYknoLGPn As Integer = 50658
                Dim iQMjSCkjSIyjZZGobBjtHvoXwlCkiAWJSWfBNDOgskFMhSGdxobNNSemaolfAyBRkpRCtEbXmCBWpcpefYlsXJvArraYQhgtinaqADitNDZKQIvxxHAbMH As UInt64 = 140055671
                Dim xjMDkgHviEhEhRmf As Boolean = True
                Dim iTXhKeVomqqeRdKjQMQrHfMJvh As Double = 47
                MessageBox.Show("UWiJsLTgFppRAbekMdojCxkQcV")
            Loop
        End While
        Dim QgDwojMweSjMAYnGpxkCqkNMYDLtatkTZmEeqDURurnkFfnctStuFQ As Long = 4805
        Try
            MessageBox.Show("ivchMbIsfXkqNAbaSkvifFfIgh")
            Dim oQUATVHUPOOJRiaOENagrvrwcLwHaQdRDKbuJcgkkZyHuvCZTdBBQlKrYXXHXysxkvFHGfEbgpvSwjCfIWmwObARAjVlEYMk As Int64 = 67
        Catch imTfhKfdFjLjcOGaqbBZsQuNaKAELngLMXEUTkhbPZYiWuFupcMQOBHRrFEWJbRQDyX As Exception
            Dim wYtthRTWyBNovnjpvvNIioLRTTgPhZBEIsVIhpOkXZJqSRnpjXNVKVAHMcZNkEnYAIfDRbIGhbjJfUxCeqeJMesmrNvFPAATkkpJlkXIPHnh As Object = 4
        End Try
        Dim yGKDYOwsmSdEPIgpiWUpahqgfZAnaYuSaQhuQtioDWWh As Integer = 3
        Dim dghpIgBJievKxbqWQOXOgpVDNHLFUgfmKDmgGxSnTdJj As Long = 476
        Dim JGTmGHreqhFMmAxQnFyRmLJhCVbkWyvjuQMBmygbjSAVbjHXdaRSFjVxwkkjeGHrlVRVnwcoYWgUK As Double = 2
        While True
            Dim neUHiliyaVYcKSDjNISSKokylhwxGXAK() As String = {"cPMVVNRKlVRgYBPQcTxQxqAnYxxuwRKViAnMNQFdQMyqsRrGSJLYIl", "DFHeEdZKWBdPCyHCMhvkibLlDp"}
            Try
                MessageBox.Show("FSEoNhTbdRfxnHsdeIrTqKcrmq")
                Dim XnrJvPhQdrmNZASxTFskUIZUQA As Decimal = 7
            Catch kUBSupxmFyjiAOqPsITyGxKkWlhVePqNcPoDjHBkRhWOyRdDUpYPuTYlJQuJHcIpIWF As Exception
                Dim dKRVZWbXTugoyysR As Integer = 35315
            End Try
            Do
                Dim huVwGjOCshYJUBvXXZKGHhPMYPlpoqevhbsvkWLDCODA As Integer = 788134413
                Dim YodBMuDsHPflGvPTYeYGNFFwlgpfGrpGvjsDilguUPhiKbZxbKJxXJNSigOfEWlPZZoWDMjlwclxXWmKHrEvhqimW As UInt64 = 7
                Dim EyfqceBZuCurOEYg As Boolean = True
                Dim KlKikhBfRKDUBllgSCOTaSOvMVixDdAsrFcHrqRHJxKY As Double = 5807
                MsgBox("qvDjvJtIVNYQQaVLJadMqFHlTHyCTwMwUYkgKeacridbMiPkfIsMAlwknQQoKqoXAbuqooBey")
            Loop
        End While
        Do While 8 <> 5
            Do
                Dim hTZmaHuBIVBLjeZdtBYeUauWDtudCMIKWNBIewsuULpMPNtFKwrLihnHYDjfLQxuMpnVcRxwIdJIkhGoypTEUwbnZtjbMMiDNtAZZYcsxtDF As Integer = 1
                Dim filZsMQyNELAQyHPoukRFPYQMylaiUmQkJeNbcftgCfomiImivABhdkQKUNsaoQsRuPHTsmMPpjCi As Decimal = 33865
                Dim LcUCqDbwaJmCSmJeiASpMfHlxNTgnYaNNSKXKNkCsHWGxMbapRrYAgUfVhDdjuIBiZoAbkUUOQvEJ As Boolean = True
                Dim ZMAZlsubKFAUpuDYNFFFTJxDapoXVTULOShnhMNZsHPnnCpSNagVrs As Double = 88
                MsgBox("IjGyZTlaxwPQfdarrTaVlXKoid")

                Try
                    MessageBox.Show("AQkKDEyCNXlicojtdYPlRcbSnsSVeEOEQNZHEHPJbNvaHZkdPFUrDG")
                    Dim arIZwjIRVCFItEagkFMLXwTSIjjXWRTRgNESvCabcDvRmHePTfOhDw As Int64 = 38
                Catch TcZvVyXgLJeNVCsNmPTKGAnhNU As Exception
                    Dim AftisdSkYdQBJomCUYaVZiltbJslnaWceWnSioVgUpKgtGXqjOIYVFeRtyGjkfvxHnVmAUSeDtBjYFAUkBCZRwHZr As Single = 0
                End Try
                Dim bmNTMynoemKfvWuB As Boolean = False
                Dim rZAjvLVhxmkJPuSmXpEicIyqAYhHUncwHmnpFxCXqRjHZxHVrGvuSi As Integer = 3822
            Loop
        Loop
        Dim MbyurdkjStobRREoZHutKJEtmjsdddovlRKFNeaLOOBbhkNVXskkbYdCWFHIkHFnFroyHyatgjfuIcMBVXFkrEUNmHiIXSaH As Boolean = False
        Dim AJwcORKEJWqpbIVPwQkwmKtInj As Object = 137086
    End Sub

    Public Function PuhSmjCDlPtjUmut()
        Dim rxmhdipgiKRAmoGtDnQMIHjBrYEICtcextAlcWGcwBnEvFHbiBuLoy As Decimal = 5201
        Do
            Dim YOfbAijVBYQjdZhASdffAujatvgyfOYXAWLGqscHYiBeNsclmECnJS As Integer = 5778646
            Dim GFgddQWPBRbcDPdNGOHcEIcyjIZJySlfSfOjagdviyCmZKYDbUosftlwchtUqNPifuJ As Object = 738
            Dim DxGRkKYIPyCBIgBXecIqyXDABWHKMhCkfwyUmQmCSjTYaelFrurRdHnftFPOCNuhvRULggjNoFXODdZWjCSRokHYJSZhAYAEnlbJJaXKLdbX As Boolean = True
            Dim DvESnwkRqcwYdGML As Double = 64
            MessageBox.Show("eNCQsCSNsbldZvJhrlwbPwyMHMFIqOSfAykXdtRZqMbhOqqWqMwuKSvhOoJfwJVeJFdZKlExTXjDUQxPViQdQWVgDDIojkcg12451")
        Loop
        Dim uyHejUywQDtmFLgngkZfZmssnPSOCCxGVBNOIvZKoReuMGPGMWUmOI As Long = 2
        Try
            MsgBox("DXGGvNjrMKOcvyMIfXYgPQGJlc")
            Dim GAsnZFrYqmopgwfv As Int64 = 85724
        Catch KoPtIIwHcGArldTHNQpYmThKJcZqhfrDXWZhqvMOySqTLgjyLjmoxWNpGEkehGYwOvTPDkQREbYUc As Exception
            Dim AdRpvGCCntxGshKHYakRQymBwhVFXkpkbFPBecxkBmgccDidcAYTjILAqPrNkveCDMdnSJbucAQdSJYtxqcXMWBTsOJPkjAEHeZPYuLSXcpi As Object = 26
        End Try
        Select Case True
            Case True
                Dim SVdpCyEtpXMPObVxDLYKGDfRVYrVGLVF As Object = 217
                Try
                    MsgBox("YISSxuBpXesCJTblVTNDPYyKBAvIFOtDdNPxWMowPUIKXafxFZnTqkCAIHQxyskIkpYuEdPIdQmtP")
                    Dim bksaQMuFyhOLMTlUerajVVDGxo As Int64 = 63
                Catch AaeNDQxYHpJEGqLcHccdEjAFuj As Exception
                    Dim OGfDkNisfmOtebgs As Object = 44
                End Try
            Case False
                MessageBox.Show("efkdIOcnaSnVsOeteIMLJhlowUydNZflFAULtUnwIucFLRIySyqVLEavtvgmPyZAmBP")
                Dim SnxIphuqEWcTvsUpUuLfVluTtH As Integer = 776
        End Select
        While 42188 <> 63431
            MsgBox("AQkKDEyCNXlicojtdYPlRcbSnsSVeEOEQNZHEHPJbNvaHZkdPFUrDG")
            Dim MDpxTFKJlWjoNtbf As Integer = 681
            Dim GgJTEYLdWKjtEEyU As ULong = 184641
        End While
        Dim oBYgsilupQRescsfdwsbJIBIYcoDLWktAKjqpTPNnyOW As Object = 6
        Do Until 5 = 63
            Dim JnLloGvZedUvuvRS As Int64 = 840087
        Loop
        While 752 <> 258
            MessageBox.Show("aViTJUlMqfvKmlNZvtMYneSsmrvlcUuCuvPFRLTrGCgWfqiHextSjo")
            Dim FiTMFnBtFTKDFbik As Integer = 712
            Dim cHLPJxUgdPHXcWRymdmnobCkSDIStdpsOfXpiiGQUMQyvafxroTvjtbBEavFpClyYOJwybPEW As ULong = 6
        End While
        Dim IRSlLyECVGecArGqkbScnCWiONqjMdyuEVjQDLLWOrwP As Long = 86
        Do Until 3 = 3152361
            Dim OXVWiDwjLrJWZCrH As Int64 = 805
        Loop
        Dim FlkRSnqobvlBbFaEcbVeHSoNmKNarkoXakOKTZhPPIgOqogKhDtcHljmvXdHaQwqHNZqqQjCt As String = "LhgowWfXrkxtuicWBQLgVghskORMrsQJiksZscIntkQp"
        Return 785777334
    End Function

    Public Function rELgVrhppNpWBEJYXkZatkYqVdpxtQNwgSsdydmRvEYIKGWHBIHwlhSGeUKrCjvZyspiLeYsxHdoMQgPHhwlJQKxy()
        Select Case True
            Case True
                Dim jWjVGsyGCkFvHRgscyMRGIjUnm As Object = 5
                Try
                    MessageBox.Show("FdpnUWMbjdjZuWYmlFFOwfHKdocIJJLsdEfHCIwugFauDRbAkMMTHaBZWeJfsESaSpvpqQkdHJgrE")
                    Dim xJhjPjtNjCIkFdRfoyJxAKcHhOnWfuhArQKPpZkJuCdFZTXbuxtSnUanfZBBuXhnUHrnSBpPNyDwfWhNpMbgAOMXo As Int64 = 28
                Catch ZleeCQAbdJdyEbtGbtyMBqSJpmFjqpGDrcCSjyMGjMkFxtUFmHTRlODhSVoPLQkmDZG As Exception
                    Dim JHOyfFORPUAvAGgN As Object = 176
                End Try
            Case False
                MsgBox("AKbBenlqQLOHkeaLeeiUbOiaBCoHZdqRAAOjSQKnmBNmgXJKlpPssRglbAybxdQmUvHkQpsOUthaA")
                Dim uroCvDvGiVntQdkbQMJSepYwicbBdWfPaGMpgusNvacQBFAiOFhDXkqFTqSSbtbMoYT As Integer = 64
        End Select
        Dim LCHfbvENFtWliLuDbhTlYvXUKgRcayBDDViRZbWJgPujONYsqetGOUcOelUueamGsSiMbdcAK As Double = 5
        Dim vmrYmReuqpAGTSGFmHTJUYMLxd As ULong = 73
        Dim ZltMRBdPOWQxiMBuiGNPwGVTYVWMXVdv As Integer = 72
        Dim ElORErbLgMmfsLmfYduQnMPTxS As UInt64 = 763
        Do Until 756182 = 8
            Dim bcCJxXUCYKwsoMEp As Int64 = 7175
        Loop
        Dim jFUPAsoSmYsMseJadiLZswUGcEqCGOPRnugfBAtZwLhZySHtpEPCVIkmjNmuBmMXIaXxwdZcn As String = "YQhbIpAUTncaIRkfWorxdCVqsV"
        Dim KFyOCAICEDMECTtDHnacTYcTjAOJiGeeyekNJStFiIPTYMxUchUeGXgkcedClxTxYhF As Decimal = 20842
        Dim MIjMnwhdKELavqQCnlkwLpimMJgCAFixUVYOgmkMLSOwTSSExmBdGv As Boolean = True
        Dim DGYuSuWBnQMetCJljeUceKaHOINOEMiPmvskQorgqsca As Long = 186
        Dim EyWEFDBuPSLjGjeafNoVrpliACqICfkTiPcWrrROkIoXthpEPkHqwcWONmEDLrsigrBqHIinvajiA As Double = 73
        Dim ogaUxkEhXDqecUCnBFlkVjtjYQaBLSwSTCsctrHGxGaxJuTCeLgAZOxFmpugJDWFFXK() As String = {"WGLpyAkeVbZLMMXYqbHrynKosmrqAWKxkyTZNunQHiJvrPYbvXqEYrUivVwMHtexQbZNsxLvn", "bfrHRaVfmrXvDLLh"}
        Dim VVNCDtOKVfWZaswtkdBubtyCTumxCDdwFdMuMrXLATVyVjYENLLEEgwaVLhHHPIRCZT As Double = 1
        Select Case True
            Case True
                Dim mTNBXlWeAbMCTpbaHThZmobbQyDBMJIMCPyHxriaNyhiNbCxwBsuXq As Object = 610
                Try
                    MessageBox.Show("TsTxdGpWVrtNQXQYoKuQsBojbgdoHAomXgkhnxiujABqmYjAVbOrccfSEjVwcBPUCGojlikpayfsy")
                    Dim iHLngUOmxifWOQTFfArSHQKhsdaOprHtEHXDSAiODNjbARwTRfrfFKjRwctCsywFakRHrPmeRdOsghhbgHwJAbotWFFoFgUEQdAOAoaBsMOa As Int64 = 612075
                Catch FEuHALttyJJQCYyNZiFVLWAqVYqeSUkLCGxXaOtyvxZYSdkhAyHbcUOiDDkaUObdYwO As Exception
                    Dim YhDnNZVYWrtiThhq As Object = 754863
                End Try
            Case False
                MsgBox("aKNVSnjDMUZNMTQXOMdgrplJrgDmwutuJuBFgdpXSukMKwlYSKfvhLFjYEckjvGRZdDQRKpkVGOwFYyuteTmPxNwp")
                Dim OvqookqGBKHglCBXfqwtVEYRuAHypnXNYMPnrEoUnImoKcsxyOVkRidJXZsocrQNmsI As Integer = 328
        End Select
        Dim USvbiGMoQotcwjpqfAbtqPoaBQxxJAwnLLBIiOyTtaSGJDISlTOUsYwEcVZRHOfQrFQkhhIQA As String = "IBNgAtcXKvENobKdnMOptwQqTVdaqeldyKAcPGGPXBgyHRqYZZSwlLcLkWMeQCffJZNdbgNgGySsP"
        Dim ARSPhxOOqudcNqbrHcqJsNSBAgpjgfucIqDFUFmMqmohtWsFJMRygGklyQPAHdgLABTVmWWCo As Double = 10
        Select Case True
            Case True
                Dim NqfHHApbHIRiWLWAVTXIPaWwZYtxEIRXryqbADldKjdKwGQrPwDcsw As Object = 1
                Try
                    MessageBox.Show("ppRnJIuvpxmBsvoEKYtbqfJItCrjKjuduYkXrMeerElmWsDQqKVfAYNKfJtHPLkFCIFTKRMxsPims")
                    Dim JgdQXZLQaPGUAZuCJTvIAJfXtIvbeVTtlrZLDGlEFueeFVHtZefVFnfyBGWOcilEuLiIyjxDgrDFeARYBCCyoxnwM As Int64 = 0
                Catch eZlwcuPbddXhTSxeDNdwDwLGPHbycOdFqNAiuspVgFesjcEwuaUIAGGgOfhVSPxPARq As Exception
                    Dim rQDoRnGXuTZKBOWq As Object = 7
                End Try
            Case False
                MessageBox.Show("RDUxFRweeZhNFAxbJNpAGVcKMHoJaSHuHTSlaojeuhIFXhVxugcVATNuaJlxbmDLSmFkhMATdKEYMWldlqOtpktnG")
                Dim kXxPcLGUOMJtWDDBpNouaukXTqUoioBOhnSjXSMAkgEUOhOZQcKDevDplavlqRkZLtM As Integer = 37
        End Select
        Dim rMSKADFyuFvYTYGkdJLDTFlPWC As Decimal = 1143
        Dim GqMpGomAhQvoFCgsHhsCPaEsdSFKUREnQqBxXAIWrMFLFufVfWXDvfXelTLmiceQLHXNBhUHZ As Double = 462238
        Dim pcxAlDKlLoJpcmgmKJyDnfKMWBLOyXaK As Integer = 6602
        Dim PZFFZyDQlURjnMYEWBkHtbhNxJ As UInt64 = 6
        Dim bWjXxvFdlwVIoPrALnYQdhqkbECBBXBNaFFJEamasPgIFFDemBLFOZysexuttDbNuHg As Double = 11
        Dim gDGnZgjUnVWRTxkVsrMLAkxyHk As Integer = 8
        Dim DgnTYbRLSiuMZOEokTfYqdqApJceTEjkirMGaSEqfIpUlMiPheThmWLZpmcOymqNxJcgBwNRP As String = "XTBgimqtYFxBkMdo"
        Return 8
    End Function

    Public Function BJeYBGnueOCopdcUyqqSRvDMsFEJyHQbcuKrGcGYOVrxJTmaQbxBEFTEGRfuCeuXxaLtSjOVjSEuEFCZRcsXIOXXb()
        Dim VOECVqoBhwBStjEJgCFVcjMcowSYpYgg As Integer = 3070
        Dim kBIasUaNVfEpIwUVlaCghfdLaw As UInt64 = 6
        Dim LXBthmRkoJSGwlFlcRSVcKTbhgsgLJYQupTxIXtwbBfHJEjbwTYCUA As Long = 575070
        Dim hLntFgShZiGpeNgVRsjfobSPIp As Integer = 405302
        Dim XLqNpViUlHUYxIZkBlEBHAaejaJTRdHKLQaZmvXaqJERlRJpKIiuNNlWkMdOMPDHAJNhhywFO As String = "NWKCHNkcJJNkZVthFvdoXJoZad"
        Select Case True
            Case True
                Dim bcXpQNAjuEhnafAIkiekiTBCfAFTEpDlNikatFlNbsIriygxPLueCS As Object = 778
                Try
                    MsgBox("pMWbZhrZJpnvgZAnQoWRQRYyLaowfBLpgFOMqkKCdOJnfcPvNvAKoByYmWQystsHdwOaLmveeTnCH")
                    Dim FFIQQyuqfXbxNkpFYgxyZxWvjPxKhPetKRCpmHyUwhLRiOtbIKPlULyGiLfAAjJPrhnviySRhdCrmFNVVVCWArYxRuHaaxTMYvYAZwsJrxEq As Int64 = 2
                Catch qgKhrgHSyyTbJdgDKkKKmOGAtfFkwxpUaqfOshEhVacFFUbRxLFmLv As Exception
                    Dim JLdEqJOILhINNSSi As Object = 33347
                End Try
            Case False
                MessageBox.Show("neaonNsJbYAxXuCSBdLrBwiyHhSAqubICkrYUwZQAHCtuejeocWGslgqvbgFusRfwqulCcxEEurstoDcxmKOXUPPE")
                Dim LXwPTEfdeNxWBJXYFlCwaMAEJucntLmITGShZkfNEeerVlsXfvkngHExXsFXbrjSapD As Integer = 305
        End Select
        Dim jjXYsdfnXWAZQmkBcwDeouocpW As Decimal = 58761
        Dim etLEsWGDuolgFvdngkOGMLWXLIxGjPBQQXICXXfffksbuiomkgPKUakSXnrjYASemUPhmbPIE As Double = 76
        Select Case True
            Case True
                Dim pGIMGZfHWwrrHcOMyoMcduYSQxHIaXHhPOuWSxrdvrFPFPqPshOVgy As Object = 132
                Try
                    MsgBox("OMJIlVBRIyqlFeUrFYLGYuwgBAEgBEXJEdklTMncCSmIBmnrYiPFnpBTPbGkRTPDbhgasiYrkiEGH")
                    Dim aSbENBJQuvqKcChpIxvYSKJQNATokqQLMaMlTuUkndKmgLxDHacZjIfPJLpMgUeDeexCpvVMcLkoAWiCCPgKOqbEt As Int64 = 384811378
                Catch QtyIgJUngOhuuEFrIieGGlkrGGrnEHnvhrvDGAOujUWJhudkFPlHFV As Exception
                    Dim jqgAZYqbbOZGijAp As Object = 18501
                End Try
            Case False
                MessageBox.Show("xkZuUJJiVnHmBfTeBkghOoJQmHMCRMgryJRjRNjwXJTJjrSjIoVHVCdJbIHMvGQIHPnqNiMnAiJEUyfunctcqZVvf")
                Dim jKhujAwWNBheVBXOyLvyBGMdGesgpmSMxYTUEFbERCkrvSZrVocCyHfcxEoFHPgahVg As Integer = 611
        End Select
        Dim QROOOXrvpBKBLaQAyACEylSBBM As Decimal = 50
        Return 2036
    End Function

End Class
