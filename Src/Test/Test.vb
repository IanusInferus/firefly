Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Diagnostics.Debug
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.Compressing
Imports Firefly.TextEncoding
Imports Firefly.Glyphing
Imports Firefly.Texting
Imports Firefly.Mapping
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
        Public p As KeyValuePair(Of Byte, Integer)
        Public str As String

        Public Shared Operator =(ByVal Left As SerializerTestObject, ByVal Right As SerializerTestObject) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            Return Left.i = Right.i AndAlso Left.s = Right.s AndAlso Left.o.h = Right.o.h AndAlso Left.a.ArrayEqual(Right.a) AndAlso Left.l.ToArray.ArrayEqual(Right.l.ToArray) AndAlso Left.l2.ToArray.ArrayEqual(Right.l2.ToArray) AndAlso Left.l3.ToArray.ArrayEqual(Right.l3.ToArray) AndAlso Left.e1 = Right.e1 AndAlso Left.p.Key = Right.p.Key AndAlso Left.p.Value = Right.p.Value AndAlso Left.str = Right.str
        End Operator
        Public Shared Operator <>(ByVal Left As SerializerTestObject, ByVal Right As SerializerTestObject) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, SerializerTestObject)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class
    Public TestObject As New SerializerTestObject With {.i = 1, .s = 2, .o = New SerializerTestObject2 With {.h = 3}, .a = New Byte() {4, 5, 6}, .l = New List(Of Int16) From {7, 8, 9}, .l2 = New LinkedList(Of Int32)(New Int32() {10, 11, 12}), .l3 = New HashSet(Of UInt64) From {13, 14, 15}, .e1 = 16, .p = New KeyValuePair(Of Byte, Integer)(17, 18), .str = "19"}

    Public Structure SerializerTestObject2
        Public h As Integer
    End Structure

    Public Class GenericListProjectorResolver(Of D)
        Implements IGenericListProjectorResolver(Of D)

        Public Function ResolveProjector(Of R, RList As {New, ICollection(Of R)})() As Func(Of D, RList) Implements IGenericListProjectorResolver(Of D).ResolveProjector
            Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(R))), Func(Of D, R))
            Dim F =
                Function(Key As D) As RList
                    Dim Size = 3
                    Dim l = New RList()
                    For n = 0 To Size - 1
                        l.Add(Mapper(Key))
                    Next
                    Return l
                End Function
            Return F
        End Function

        Private AbsResolver As AbsoluteResolver
        Public Sub New(ByVal Resolver As IObjectMapperResolver)
            Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
        End Sub
    End Class

    Public Class GenericListAggregatorResolver(Of R)
        Implements IGenericListAggregatorResolver(Of R)

        Public Function ResolveAggregator(Of D, DList As ICollection(Of D))() As Action(Of DList, R) Implements IGenericListAggregatorResolver(Of R).ResolveAggregator
            Dim Mapper = DirectCast(AbsResolver.ResolveAggregator(CreatePair(GetType(D), GetType(R))), Action(Of D, R))
            Dim F =
                Sub(list As DList, Value As R)
                    Dim Size = 3
                    For n = 0 To Size - 1
                        Mapper(list(n), Value)
                    Next
                End Sub
            Return F
        End Function

        Private AbsResolver As AbsoluteResolver
        Public Sub New(ByVal Resolver As IObjectMapperResolver)
            Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
        End Sub
    End Class

    Public Sub TestObjectTreeMapper()
        Dim Count = 0

        With Nothing
            Dim mprs As New AlternativeResolver
            Dim pr = New PrimitiveResolver
            mprs.ProjectorResolvers.AddLast(pr)
            Dim er = New BinarySerializer.EnumUnpacker(Of Integer)(mprs)
            mprs.ProjectorResolvers.AddLast(er)
            Dim cr = New CollectionUnpackerTemplate(Of Integer)(New GenericListProjectorResolver(Of Integer)(mprs))
            mprs.ProjectorResolvers.AddLast(cr)
            Dim csr = New RecordUnpackerTemplate(Of Integer)(New BinarySerializer.FieldOrPropertyProjectorResolver(Of Integer)(mprs))
            mprs.ProjectorResolvers.AddLast(csr)
            pr.PutProjector(
                Function(i As Integer) As Byte
                    Count += 1
                    Return Count
                End Function
            )
            pr.PutProjector(
                Function(i As Integer) As Int16
                    Count += 1
                    Return Count
                End Function
            )
            pr.PutProjector(
                Function(i As Integer) As Int32
                    Count += 1
                    Return Count
                End Function
            )
            pr.PutProjector(
                Function(i As Integer) As UInt64
                    Count += 1
                    Return Count
                End Function
            )
            pr.PutProjector(
                Function(i As Integer) As String
                    Count += 1
                    Return Count.ToString()
                End Function
            )
            Dim mp As New ObjectMapper(mprs)

            Dim BuiltObject = mp.Project(Of Integer, SerializerTestObject)(0)
            Assert(TestObject = BuiltObject)
        End With

        Dim Count2 = 0
        With Nothing
            Dim mprs As New AlternativeResolver
            Dim pr = New PrimitiveResolver
            mprs.AggregatorResolvers.AddLast(pr)
            Dim er = New BinarySerializer.EnumPacker(Of Integer)(mprs)
            mprs.AggregatorResolvers.AddLast(er)
            Dim cr = New CollectionPackerTemplate(Of Integer)(New GenericListAggregatorResolver(Of Integer)(mprs))
            mprs.AggregatorResolvers.AddLast(cr)
            Dim csr = New RecordPackerTemplate(Of Integer)(New BinarySerializer.FieldOrPropertyAggregatorResolver(Of Integer)(mprs))
            mprs.AggregatorResolvers.AddLast(csr)
            pr.PutAggregator(
                Sub(Key As Byte, Value As Integer)
                    Count2 += 1
                End Sub
            )
            pr.PutAggregator(
                Sub(Key As Int16, Value As Integer)
                    Count2 += 1
                End Sub
            )
            pr.PutAggregator(
                Sub(Key As Int32, Value As Integer)
                    Count2 += 1
                End Sub
            )
            pr.PutAggregator(
                Sub(Key As UInt64, Value As Integer)
                    Count2 += 1
                End Sub
            )
            pr.PutAggregator(
                Sub(Key As String, Value As Integer)
                    Count2 += 1
                End Sub
            )
            Dim mp As New ObjectMapper(mprs)

            mp.Aggregate(TestObject, 1)
            Assert(Count = Count2)
        End With
    End Sub

    Public Class StringAndBytesTranslator
        Implements IProjectorToProjectorRangeTranslator(Of String, Byte()) 'Reader
        Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()) 'Writer
        Implements IProjectorToProjectorDomainTranslator(Of String, Byte()) 'Counter

        Public Function TranslateProjectorToProjector(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
            Return Function(v) UTF16.GetString(Projector(v))
        End Function
        Public Function TranslateAggregatorToAggregator(Of R)(ByVal Aggregator As Action(Of Byte(), R)) As Action(Of String, R) Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()).TranslateAggregatorToAggregatorDomain
            Return Sub(s, v) Aggregator(UTF16.GetBytes(s), v)
        End Function
        Public Function TranslateProjectorToProjector(Of R)(ByVal Projector As Func(Of Byte(), R)) As Func(Of String, R) Implements IProjectorToProjectorDomainTranslator(Of String, Byte()).TranslateProjectorToProjectorDomain
            Return Function(s) Projector(UTF16.GetBytes(s))
        End Function
    End Class

    Public Sub TestBinarySerializer()
        Dim BinaryRoundTripped As SerializerTestObject

        Using s As New StreamEx
            Dim bs As New BinarySerializer

            Dim sbr As New StringAndBytesTranslator

            bs.PutReaderTranslator(sbr)
            bs.PutWriterTranslator(sbr)
            bs.PutCounterTranslator(sbr)

            Dim Size = bs.Count(TestObject)
            bs.Write(s, TestObject)
            Assert(Size = s.Length)
            s.Position = 0
            BinaryRoundTripped = bs.Read(Of SerializerTestObject)(s)
        End Using
        Assert(TestObject = BinaryRoundTripped)
    End Sub

    Public Class XmlTestObject
        Public Test As XmlTestObject2
        Public o As Object

        Public Shared Operator =(ByVal Left As XmlTestObject, ByVal Right As XmlTestObject) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            Return Left.Test = Right.Test AndAlso ((Left.o Is Nothing) = (Right.o Is Nothing))
        End Operator
        Public Shared Operator <>(ByVal Left As XmlTestObject, ByVal Right As XmlTestObject) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, XmlTestObject)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class
    Public Class XmlTestObject2
        Public i As Integer = 1

        Public Shared Operator =(ByVal Left As XmlTestObject2, ByVal Right As XmlTestObject2) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            Return Left.i = Right.i
        End Operator
        Public Shared Operator <>(ByVal Left As XmlTestObject2, ByVal Right As XmlTestObject2) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, XmlTestObject2)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class
    Public Class XmlBaseObject
        Public i As Integer = 1
    End Class
    Public Class XmlDerivedObject
        Inherits XmlBaseObject
        Public i2 As Integer = 2

        Public Shared Operator =(ByVal Left As XmlDerivedObject, ByVal Right As XmlDerivedObject) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            Return Left.i = Right.i AndAlso Left.i2 = Right.i2
        End Operator
        Public Shared Operator <>(ByVal Left As XmlDerivedObject, ByVal Right As XmlDerivedObject) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, XmlDerivedObject)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class
    Public Class XmlInheritanceObjectContainer
        Public b As XmlBaseObject

        Public Shared Operator =(ByVal Left As XmlInheritanceObjectContainer, ByVal Right As XmlInheritanceObjectContainer) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            Return Object.Equals(Left.b, Right.b)
        End Operator
        Public Shared Operator <>(ByVal Left As XmlInheritanceObjectContainer, ByVal Right As XmlInheritanceObjectContainer) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, XmlInheritanceObjectContainer)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class
    Public Class ByteArrayEncoder
        Inherits Xml.Mapper(Of Byte(), String)

        Public Overrides Function GetMappedObject(ByVal o As Byte()) As String
            Return String.Join(" ", (From b In o Select b.ToString("X2")).ToArray)
        End Function

        Public Overrides Function GetInverseMappedObject(ByVal o As String) As Byte()
            Return (From s In Regex.Split(o.Trim(" \t\r\n".Descape.ToCharArray), "( |\t|\r|\n)+", RegexOptions.ExplicitCapture) Select Byte.Parse(s, Globalization.NumberStyles.HexNumber)).ToArray
        End Function
    End Class
    Public Sub XmlRoundTrip(Of T)(ByVal xs As XmlSerializer, ByVal v As T)
        Dim xe = xs.Write(v)
        Dim RoundTripped = xs.Read(Of T)(xe)
        Assert(Object.Equals(v, RoundTripped))
    End Sub
    Public Sub XmlRoundTripCollection(Of E, T As IEnumerable(Of E))(ByVal xs As XmlSerializer, ByVal v As T)
        Dim xe = xs.Write(v)
        Dim RoundTripped = xs.Read(Of T)(xe)
        Dim va = v.ToArray()
        Dim ra = RoundTripped.ToArray()
        Assert(Enumerable.SequenceEqual(va, ra))
    End Sub
    Public Sub XmlRoundTripInheritance(Of B, D As B)(ByVal v As D)
        Dim xs As New XmlSerializer(New Type() {GetType(D)})
        Dim xe = xs.Write(Of B)(v)
        Dim RoundTripped = xs.Read(Of B)(xe)
        Assert(Object.Equals(v, RoundTripped))
    End Sub
    Public Sub XmlRoundTripInheritance2(Of T)(ByVal v As T, ByVal ExternalTypes As IEnumerable(Of Type))
        Dim xs As New XmlSerializer(ExternalTypes)
        Dim xe = xs.Write(Of T)(v)
        Dim RoundTripped = xs.Read(Of T)(xe)
        Assert(Object.Equals(v, RoundTripped))
    End Sub
    Public Sub TestXmlSerializer()
        Dim xs As New XmlSerializer

        XmlRoundTrip(xs, 123123)
        XmlRoundTrip(xs, "123123")
        XmlRoundTrip(xs, SerializerTestEnum.E3)
        XmlRoundTrip(xs, 123.123)
        XmlRoundTrip(xs, CType(123.123, Decimal))
        XmlRoundTrip(xs, True)

        XmlRoundTripCollection(Of Byte, Byte())(xs, New Byte() {1, 2, 3})
        XmlRoundTripCollection(Of Byte, LinkedList(Of Byte))(xs, New LinkedList(Of Byte)(New Byte() {1, 2, 3}))

        XmlRoundTrip(xs, TestObject)

        XmlRoundTrip(Of XmlTestObject)(xs, Nothing)
        XmlRoundTrip(xs, New XmlTestObject With {.Test = Nothing})
        XmlRoundTrip(xs, New XmlTestObject With {.o = New Object})
        XmlRoundTrip(Of Byte())(xs, Nothing)
        XmlRoundTripCollection(Of Byte, Byte())(xs, New Byte() {})

        XmlRoundTripInheritance(Of XmlBaseObject, XmlDerivedObject)(New XmlDerivedObject)
        XmlRoundTripInheritance2(New XmlInheritanceObjectContainer With {.b = New XmlDerivedObject}, New Type() {GetType(XmlDerivedObject)})

        Dim XmlRoundTripped As SerializerTestObject

        Using s As New StreamEx
            Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, 0, False)
                Using sw = Txt.CreateTextWriter(ps, UTF16)
                    Xml.WriteFile(sw, TestObject, New Type() {}, New Xml.IMapper() {New ByteArrayEncoder})
                End Using
            End Using
            s.Position = 0
            Using ps As New PartialStreamEx(s, 0, s.Length, False)
                Using sr = Txt.CreateTextReader(ps, UTF16)
                    XmlRoundTripped = Xml.ReadFile(Of SerializerTestObject)(sr, New Type() {}, New Xml.IMapper() {New ByteArrayEncoder})
                End Using
            End Using
        End Using
        Assert(TestObject = XmlRoundTripped)
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
        'TestObjectTreeMapper()
        'TestBinarySerializer()
        TestXmlSerializer()
    End Sub
End Module
