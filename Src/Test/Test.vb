Imports System
Imports System.Collections.Generic
Imports System.Diagnostics.Debug
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Compressing
Imports Firefly.TextEncoding
Imports Firefly.GUI

Public Module Test
    Public Sub TestUnicode()
        Dim c As String = Firefly.TextEncoding.Char32.ToString(&H20C30)
        Assert(Microsoft.VisualBasic.AscW(c(0)) = &HD883)
        Assert(Microsoft.VisualBasic.AscW(c(1)) = &HDC30)
        Dim code As Int32 = Firefly.TextEncoding.Char32.FromString(c)
        Assert(AscQ(c) = &H20C30)
    End Sub

    Public Sub TestFileDialog()
        Application.EnableVisualStyles()

        Static d As FilePicker
        If d Is Nothing Then d = New FilePicker(False)
        d.InitialDirectory = "H:\"
        d.FilePath = "H:\$RECYCLE.BIN"
        d.Filter = "*.*(*.iso)|*.iso"
        d.ModeSelection = FilePicker.ModeSelectionEnum.FileWithFolder
        d.Multiselect = True
        If d.ShowDialog() = DialogResult.OK Then
            Dim FilePath = d.FilePath
            Dim FilePaths = d.FilePaths
        End If
    End Sub

    Public Sub TestBitStreamReadWrite()
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ 24
                For n = 0 To 7
                    a.WriteFromByte(n, 3)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ 24
                For n = 0 To 7
                    Assert(a.ReadToByte(3) = n)
                Next
            Next
        End Using
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ (8 * 8)
                For r = 0 To 7
                    Dim n = r * 15 + 13
                    a.WriteFromByte(n, 8)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ (8 * 8)
                For r = 0 To 7
                    Dim n = r * 15 + 13
                    Assert(a.ReadToByte(8) = n)
                Next
            Next
        End Using
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ 24
                For n = 0 To 7
                    a.WriteFromInt32(n, 3)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ 24
                For n = 0 To 7
                    Assert(a.ReadToInt32(3) = n)
                Next
            Next
        End Using
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ (8 * 8)
                For r = 0 To 7
                    Dim n = r * 15 + 13
                    a.WriteFromInt32(n, 8)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ (8 * 8)
                For r = 0 To 7
                    Dim n = r * 15 + 13
                    Assert(a.ReadToInt32(8) = n)
                Next
            Next
        End Using
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ (31 * 8)
                For r = 0 To 7
                    Dim n = r * 306783378 + 1
                    a.WriteFromInt32(n, 31)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ (31 * 8)
                For r = 0 To 7
                    Dim n = r * 306783378 + 1
                    Assert(a.ReadToInt32(31) = n)
                Next
            Next
        End Using
        Using a As New BitStream(1024)
            For k = 0 To 1024 \ (32 * 8)
                For r = 0 To 7
                    Dim n = CID(r * 306783378L + 1)
                    a.WriteFromInt32(n, 32)
                Next
            Next
            a.Position = 0
            For k = 0 To 1024 \ (32 * 8)
                For r = 0 To 7
                    Dim n = CID(r * 306783378L + 1)
                    Assert(a.ReadToInt32(32) = n)
                Next
            Next
        End Using
    End Sub

    Public Sub TestCommandLine()
        Dim CmdLine = CommandLine.ParseCmdLine("  test.exe  ""1 ,"" /t123:123,"", "",234  ", True)

        Assert(CmdLine.Arguments.Length = 1)
        Assert(CmdLine.Arguments(0) = "1 ,")
        Assert(CmdLine.Options.Length = 1)
        Assert(CmdLine.Options(0).Name = "t123")
        Assert(CmdLine.Options(0).Arguments.Length = 3)
        Assert(CmdLine.Options(0).Arguments(0) = "123")
        Assert(CmdLine.Options(0).Arguments(1) = ", ")
        Assert(CmdLine.Options(0).Arguments(2) = "234")
    End Sub

    Public Sub TestUI()
        Application.EnableVisualStyles()
        ExceptionHandler.PopupInfo(1)
        Dim r = MessageDialog.Show("123", "234", "345", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.None, MessageBoxDefaultButton.Button2)
        ExceptionHandler.PopupException(New Exception("Test"))
        Application.Run(New FilePicker)
    End Sub

    Public Sub Main()
        'TestCompressing()
        'TestFileDialog()
        'TestBitStreamReadWrite()
        'TestCommandLine()
        'TestUI()
        TestMapping()
    End Sub
End Module
