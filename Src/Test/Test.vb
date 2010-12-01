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
    Public Sub TestStreamExStreamReadWrite()
        Using a = StreamEx.Create
            For n = 0 To 15 * (1 << 20) - 1
                a.WriteByte((n * 7 + 13) And &HFF)
            Next
            a.Position = 0
            Using b = StreamEx.Create
                b.WriteFromStream(a, 15 * (1 << 20))
                b.Position = 0
                For n = 0 To 15 * (1 << 20) - 1
                    Assert(b.ReadByte = ((n * 7 + 13) And &HFF))
                Next
                b.Position = 0
                a.Position = 0
                b.ReadToStream(a, 15 * (1 << 20))
                a.Position = 0
                For n = 0 To 15 * (1 << 20) - 1
                    Assert(a.ReadByte = ((n * 7 + 13) And &HFF))
                Next
            End Using
        End Using

        Using s = StreamEx.Create
            Dim Flag As Boolean = False
            Try
                s.WriteSimpleString("我", 10)
            Catch ex As Exception
                Flag = True
            End Try
            Assert(Flag)

            Flag = False
            Try
                s.WriteSimpleString("ABCDE", 4)
            Catch ex As Exception
                Flag = True
            End Try
            Assert(Flag)

            Flag = False
            Try
                s.WriteSimpleString("ABCDE", 5)
                Flag = True
            Catch ex As Exception
            End Try
            Assert(Flag)

            Flag = False
            Try
                s.Position = 0
                Dim str = s.ReadSimpleString(5)
                Flag = True
            Catch ex As Exception
            End Try
            Assert(Flag)

            Flag = False
            Try
                s.Position = 0
                s.WriteString("我", 5, UTF8)
                s.Position = 0
                Dim str = s.ReadSimpleString(5)
            Catch ex As Exception
                Flag = True
            End Try
            Assert(Flag)

            Flag = False
            Try
                s.Position = 0
                s.WriteString("我", 5, UTF8)
                s.Position = 0
                Dim str = s.ReadString(5, UTF8)
                Assert(str = "我")
                Flag = True
            Catch ex As Exception
            End Try
            Assert(Flag)
        End Using
    End Sub

    Public Sub TestListPartStringEx()
        Dim a As New ListPartStringEx(Of Byte)(New Byte() {1, 2, 3, 4, 5, 6})
        Dim b As New ListPartStringEx(Of Byte)(New Byte() {1, 2, 4, 3, 5, 6})
        Dim c As New ListPartStringEx(Of Byte)(New Byte() {1, 2, 4, 3, 5, 6})
        Assert(a < b)
        Assert(b = c)
        Assert(b.GetHashCode = c.GetHashCode)
        Dim d As New ListPartStringEx(Of Byte)(New Byte() {4, 3, 5})
        Assert(b.IndexOf(d) = 2)
        Assert(b.LastIndexOf(d) = 2)
    End Sub

    Public Function ZeroArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("零数据")
        Dim Data = New Byte(65535) {}
        Return Data
    End Function

    Public Function FixedArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("0-255循环数据")
        Dim Data = New Byte(65535) {}
        For n = 0 To Data.Length - 1
            Data(n) = n And &HFF
        Next
        Return Data
    End Function

    Public Function RandomArrayGen() As Byte()
        System.Diagnostics.Debug.WriteLine("随机数据")
        Dim Data = New Byte(65535) {}
        Dim r As New Random
        r.NextBytes(Data)
        Return Data
    End Function

    Public Delegate Function Method() As Byte()

    Public Function Methods() As Method()
        Return New Method() {AddressOf ZeroArrayGen, AddressOf FixedArrayGen, AddressOf RandomArrayGen}
    End Function

    Public Sub TestLZ77(ByVal Data As Byte())
        Dim time = System.Environment.TickCount
        Dim lz As New LZ77(Data.Clone, 1024, 10, 2, 2)
        Dim MatchSeq As New Queue(Of Pointer)
        While lz.Position < Data.Length
            Dim m = lz.Match()
            If m Is Nothing OrElse m.Length <= 1 Then
                MatchSeq.Enqueue(New Literal())
                lz.Proceed()
            Else
                MatchSeq.Enqueue(m)
                lz.Proceed(m.Length)
            End If
        End While

        Dim NewData = New Byte(Data.Length - 1) {}
        Dim p = 0
        Dim CompressionLength = 0
        While MatchSeq.Count > 0
            Dim m = MatchSeq.Dequeue
            If TypeOf m Is Literal Then
                NewData(p) = Data(p)
                p += 1
                CompressionLength += 2
            Else
                For i = 0 To m.Length - 1
                    NewData(p) = NewData(p - CType(m, LZ77.LZPointer).NumBack)
                    p += 1
                Next
                CompressionLength += 3
            End If
        End While

        System.Diagnostics.Debug.WriteLine(CompressionLength)

        For n = 0 To Data.Length - 1
            Assert(Data(n) = NewData(n))
        Next
        System.Diagnostics.Debug.WriteLine(System.Environment.TickCount - time)
    End Sub

    Public Sub TestLZ77Reversed(ByVal Data As Byte())
        Dim time = System.Environment.TickCount
        Dim LZ As New LZ77Reversed(Data.Clone, 1024, 10, 2, 2)
        Dim States As New LinkedList(Of LZ77Reversed.AccPointer)
        Dim PreviousStateAccLength As Integer = 0
        While LZ.Position >= 0
            'If LZ.Position <= 2000 Then Stop
            Dim m = LZ.Match(States)
            If m Is Nothing OrElse m.AccLength + 3 >= PreviousStateAccLength + 2 Then
                States.AddFirst(New LZ77Reversed.Literal(PreviousStateAccLength + 2))
            Else
                m.AccLength += 3
                States.AddFirst(m)
            End If
            PreviousStateAccLength = States.First.Value.AccLength

            LZ.Proceed()
        End While

        Dim MatchSeq As New Queue(Of LZ77Reversed.AccPointer)

        Dim Holds = 0
        For Each m In States
            If Holds = 0 Then
                MatchSeq.Enqueue(m)
                If Not TypeOf m Is LZ77Reversed.Literal Then
                    Holds = m.Length - 1
                End If
            Else
                Holds -= 1
            End If
        Next

        States = Nothing

        Dim NewData = New Byte(Data.Length - 1) {}
        Dim p = 0
        Dim CompressionLength = 0
        While MatchSeq.Count > 0
            Dim m = MatchSeq.Dequeue
            If TypeOf m Is LZ77Reversed.Literal Then
                NewData(p) = Data(p)
                p += 1
                CompressionLength += 2
            Else
                For i = 0 To m.Length - 1
                    NewData(p) = NewData(p - CType(m, LZ77Reversed.LZPointer).NumBack)
                    Assert(Data(p) = NewData(p))
                    p += 1
                Next
                CompressionLength += 3
            End If
        End While

        If CompressionLength > Data.Length * 2 Then Stop

        System.Diagnostics.Debug.WriteLine(CompressionLength)

        For n = 0 To Data.Length - 1
            Assert(Data(n) = NewData(n))
        Next
        System.Diagnostics.Debug.WriteLine(System.Environment.TickCount - time)
    End Sub

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
        TestStreamExStreamReadWrite()
        'TestListPartStringEx()
        'For Each Method In Methods()
        '    Dim Data = Method()
        '    If Data Is Nothing Then Continue For
        '    TestLZ77(Data)
        '    TestLZ77Reversed(Data)
        'Next
        'TestFileDialog()
        TestBitStreamReadWrite()
        'TestCommandLine()
        'TestMessageDialog()
        TestMapping()
    End Sub
End Module
