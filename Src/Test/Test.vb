Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Diagnostics.Debug
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.Compressing
Imports Firefly.TextEncoding
Imports Firefly.Glyphing
Imports Firefly.Texting
Imports Firefly.Setting
Imports Firefly.GUI

Public Module Test
    Public Sub TestStreamExStreamReadWrite()
        Using a As New StreamEx
            For n = 0 To 15 * (1 << 20) - 1
                a.WriteByte((n * 7 + 13) And &HFF)
            Next
            a.Position = 0
            Using b As New StreamEx
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

        Using s As New StreamEx
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

    Public Enum SerializerTestEnum
        E1
        E2
        E3
    End Enum

    Public Class SerializerTestObject
        Public i As Integer
        Public s As Byte
        Public o As SerializerTestObject2
        Public a As Byte()
        Public l As List(Of Int16)
        Public l2 As LinkedList(Of Int32)
        Public l3 As HashSet(Of UInt64)
        Public e1 As SerializerTestEnum

        Public Shared Operator =(ByVal Left As SerializerTestObject, ByVal Right As SerializerTestObject) As Boolean
            Return Left.i = Right.i AndAlso Left.s = Right.s AndAlso Left.o.h = Right.o.h AndAlso Left.a.ArrayEqual(Right.a) AndAlso Left.l.ToArray.ArrayEqual(Right.l.ToArray) AndAlso Left.l2.ToArray.ArrayEqual(Right.l2.ToArray) AndAlso Left.l3.ToArray.ArrayEqual(Right.l3.ToArray) AndAlso Left.e1 = Right.e1
        End Operator
        Public Shared Operator <>(ByVal Left As SerializerTestObject, ByVal Right As SerializerTestObject) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class

    Public Structure SerializerTestObject2
        Public h As Integer
    End Structure

    Public Class OneToManyFixedCollectionMapperResolver(Of D)
        Inherits ObjectTreeOneToManyMapper(Of D).CollectionMapperResolver

        Public Overrides Function DefaultArrayMapper(Of R)(ByVal Key As D) As R()
            Dim Size = 3
            Dim l = New R(Size - 1) {}
            For n = 0 To Size - 1
                l(n) = mp.Map(Of R)(Key)
            Next
            Return l
        End Function

        Public Overrides Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList
            Dim Size = 3
            Dim l = New RList()
            For n = 0 To Size - 1
                l.Add(mp.Map(Of R)(Key))
            Next
            Return l
        End Function

        Private mp As ObjectTreeOneToManyMapper(Of D)
        Public Sub New(ByVal mp As ObjectTreeOneToManyMapper(Of D))
            Me.mp = mp
        End Sub
    End Class

    Public Class ManyToOneFixedCollectionMapperResolver(Of R)
        Inherits ObjectTreeManyToOneMapper(Of R).CollectionMapperResolver

        Public Overrides Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R)
            Dim Size = 3
            For n = 0 To Size - 1
                mp.Map(Of D)(arr(n), Value)
            Next
        End Sub
        Public Overrides Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R)
            Dim Size = 3
            For n = 0 To Size - 1
                mp.Map(Of D)(list(n), Value)
            Next
        End Sub

        Private mp As ObjectTreeManyToOneMapper(Of R)
        Public Sub New(ByVal mp As ObjectTreeManyToOneMapper(Of R))
            Me.mp = mp
        End Sub
    End Class

    Public Sub TestObjectTreeMapper()
        Dim TestObject As New SerializerTestObject With {.i = 1, .s = 2, .o = New SerializerTestObject2 With {.h = 3}, .a = New Byte() {4, 5, 6}, .l = New List(Of Int16) From {7, 8, 9}, .l2 = New LinkedList(Of Int32)(New Int32() {10, 11, 12}), .l3 = New HashSet(Of UInt64) From {13, 14, 15}, .e1 = 16}

        Dim Count = 0

        With Nothing
            Dim mp As New ObjectTreeOneToManyMapper(Of Integer)
            Dim er = New ObjectTreeOneToManyMapper(Of Integer).EnumMapperResolver(AddressOf mp.Map)
            mp.Resolvers.Add(er)
            Dim cr = New OneToManyFixedCollectionMapperResolver(Of Integer)(mp)
            mp.Resolvers.Add(cr)
            Dim csr = New ObjectTreeOneToManyMapper(Of Integer).ClassAndStructureMapperResolver(AddressOf mp.Map)
            mp.Resolvers.Add(csr)
            mp.PutMapper(
                Function(i) As Byte
                    Count += 1
                    Return Count
                End Function
            )
            mp.PutMapper(
                Function(i) As Int16
                    Count += 1
                    Return Count
                End Function
            )
            mp.PutMapper(
                Function(i) As Int32
                    Count += 1
                    Return Count
                End Function
            )
            mp.PutMapper(
                Function(i) As UInt64
                    Count += 1
                    Return Count
                End Function
            )

            Dim BuiltObject = mp.Map(Of SerializerTestObject)(0)
            Assert(TestObject = BuiltObject)
        End With

        Dim Count2 = 0
        With Nothing
            Dim mp As New ObjectTreeManyToOneMapper(Of Integer)
            Dim er = New ObjectTreeManyToOneMapper(Of Integer).EnumMapperResolver(AddressOf mp.Map)
            mp.Resolvers.Add(er)
            Dim cr = New ManyToOneFixedCollectionMapperResolver(Of Integer)(mp)
            mp.Resolvers.Add(cr)
            Dim csr = New ObjectTreeManyToOneMapper(Of Integer).ClassAndStructureMapperResolver(AddressOf mp.Map)
            mp.Resolvers.Add(csr)
            mp.PutMapper(
                Sub(Key As Byte, Value As Integer)
                    Count2 += 1
                End Sub
            )
            mp.PutMapper(
                Sub(Key As Int16, Value As Integer)
                    Count2 += 1
                End Sub
            )
            mp.PutMapper(
                Sub(Key As Int32, Value As Integer)
                    Count2 += 1
                End Sub
            )
            mp.PutMapper(
                Sub(Key As UInt64, Value As Integer)
                    Count2 += 1
                End Sub
            )

            mp.Map(Of SerializerTestObject)(TestObject, 1)
            Assert(Count = Count2)
        End With
    End Sub

    Public Sub TestSerializer()
        Dim TestObject As New SerializerTestObject With {.i = 1, .s = 2, .o = New SerializerTestObject2 With {.h = 3}, .a = New Byte() {4, 5, 6}, .l = New List(Of Int16) From {7, 8, 9}, .l2 = New LinkedList(Of Int32)(New Int32() {10, 11, 12}), .l3 = New HashSet(Of UInt64) From {13, 14, 15}, .e1 = 16}

        Dim XmlRoundTripped As SerializerTestObject

        Using s As New StreamEx
            Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, 0, False)
                Using sw = Txt.CreateTextWriter(ps, UTF16)
                    Xml.WriteFile(sw, TestObject)
                End Using
            End Using
            s.Position = 0
            Using ps As New PartialStreamEx(s, 0, s.Length, False)
                Using sr = Txt.CreateTextReader(ps, UTF16)
                    XmlRoundTripped = Xml.ReadFile(Of SerializerTestObject)(sr)
                End Using
            End Using
        End Using
        Assert(TestObject = XmlRoundTripped)

        Dim BinaryRoundTripped As SerializerTestObject

        Using s As New StreamEx
            Dim bs As New BinarySerializer
            Dim Size = bs.Count(TestObject)
            bs.Write(s, TestObject)
            Assert(Size = s.Length)
            s.Position = 0
            BinaryRoundTripped = bs.Read(Of SerializerTestObject)(s)
        End Using
        Assert(TestObject = BinaryRoundTripped)
    End Sub

    Public Sub Main()
        'TestStreamExStreamReadWrite()
        'TestListPartStringEx()
        'For Each Method In Methods()
        '    Dim Data = Method()
        '    If Data Is Nothing Then Continue For
        '    TestLZ77(Data)
        '    TestLZ77Reversed(Data)
        'Next
        'TestFileDialog()
        'TestBitStreamReadWrite()
        'TestCommandLine()
        'TestMessageDialog()
        TestObjectTreeMapper()
        TestSerializer()
    End Sub
End Module
