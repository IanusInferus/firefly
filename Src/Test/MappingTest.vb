Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml.Linq
Imports System.IO
Imports System.Diagnostics.Debug
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Mapping
Imports Firefly.Setting
Imports System.Xml

Public Module MappingTest
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

    Public Class GenericCollectionProjectorResolver(Of D)
        Implements IGenericCollectionProjectorResolver(Of D)

        Public Function ResolveProjector(Of R, RCollection As {New, ICollection(Of R)})() As Func(Of D, RCollection) Implements IGenericCollectionProjectorResolver(Of D).ResolveProjector
            Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(R))), Func(Of D, R))
            Dim F =
                Function(Key As D) As RCollection
                    Dim Size = 3
                    Dim l = New RCollection()
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
        Implements IGenericCollectionAggregatorResolver(Of R)

        Public Function ResolveAggregator(Of D, DList As ICollection(Of D))() As Action(Of DList, R) Implements IGenericCollectionAggregatorResolver(Of R).ResolveAggregator
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
            Dim cr = New CollectionUnpackerTemplate(Of Integer)(New GenericCollectionProjectorResolver(Of Integer)(mprs))
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
        Implements IProjectorToProjectorDomainTranslator(Of String, Byte()) 'Writer Counter

        Public Function TranslateProjectorToProjector(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
            Return Function(v) UTF16.GetString(Projector(v))
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
    Public Sub TestXmlSerializerForDict()
        Dim xs As New XmlSerializer

        Dim dict As New Dictionary(Of String, Integer)
        dict.Add("123", 0)
        dict.Add("234", 1)

        Dim xe = xs.Write(dict)
        Dim RoundTripped = xs.Read(Of Dictionary(Of String, Integer))(xe)
        Dim va = dict.ToArray()
        Dim ra = RoundTripped.ToArray()
        Assert(Enumerable.SequenceEqual(va, ra))
    End Sub

    Public Class CompatibilityTestObject
        Public o01 As Byte() = {0, 1, 2}
        Public o02 As UInt16() = {3, 4}
        Public o03 As UInt32() = {5}
        Public o04 As UInt64() = {6, 7, 8}
        Public o05 As SByte() = {9, 10}
        Public o06 As Int16() = {11}
        Public o07 As Int32() = {12, 13, 14}
        Public o08 As Int64() = {15, 16}
        Public o09 As Single() = {17}
        Public o10 As Double() = {18, 19, 20}
        Public o11 As Boolean() = {21, 22}
        Public o12 As String() = {23}
        Public o13 As Decimal() = {24, 25, 26}
        Public o14 As Byte() = Nothing
        Public o15 As DateTime = DateTime.Now

        Public Shared Operator =(ByVal Left As CompatibilityTestObject, ByVal Right As CompatibilityTestObject) As Boolean
            If Left Is Nothing AndAlso Right Is Nothing Then Return True
            If Left Is Nothing OrElse Right Is Nothing Then Return False
            If Not Left.o01.SequenceEqual(Right.o01) Then Return False
            If Not Left.o02.SequenceEqual(Right.o02) Then Return False
            If Not Left.o03.SequenceEqual(Right.o03) Then Return False
            If Not Left.o04.SequenceEqual(Right.o04) Then Return False
            If Not Left.o05.SequenceEqual(Right.o05) Then Return False
            If Not Left.o06.SequenceEqual(Right.o06) Then Return False
            If Not Left.o07.SequenceEqual(Right.o07) Then Return False
            If Not Left.o08.SequenceEqual(Right.o08) Then Return False
            If Not Left.o09.SequenceEqual(Right.o09) Then Return False
            If Not Left.o10.SequenceEqual(Right.o10) Then Return False
            If Not Left.o11.SequenceEqual(Right.o11) Then Return False
            If Not Left.o12.SequenceEqual(Right.o12) Then Return False
            If Not Left.o13.SequenceEqual(Right.o13) Then Return False
            If (Left.o14 Is Nothing) <> (Right.o14 Is Nothing) Then Return False
            If Left.o14 IsNot Nothing AndAlso Right.o14 IsNot Nothing Then
                If Not Left.o14.SequenceEqual(Right.o14) Then Return False
            End If
            If Not Left.o15.Equals(Right.o15) Then Return False
            Return True
        End Operator
        Public Shared Operator <>(ByVal Left As CompatibilityTestObject, ByVal Right As CompatibilityTestObject) As Boolean
            Return Not (Left = Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim o = TryCast(obj, CompatibilityTestObject)
            If o Is Nothing Then Return False
            Return Me = o
        End Function
    End Class

    Public Sub TestXmlSerializerCompatibility()
        Dim v As New CompatibilityTestObject
        v.o05 = New SByte() {11, 12}

        '.Net Internal roundtrip
        With Nothing
            Dim oxs As New System.Xml.Serialization.XmlSerializer(GetType(CompatibilityTestObject))
            Using s As New StreamEx
                Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps, UTF16)
                        oxs.Serialize(sw, v)
                    End Using
                End Using
                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)
                Dim RoundTripped As CompatibilityTestObject
                Using ps As New PartialStreamEx(s, 0, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps, UTF16)
                        RoundTripped = oxs.Deserialize(sr)
                    End Using
                End Using
                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With

        '.Net Internal to XmlSerializer
        With Nothing
            Using s As New StreamEx
                Dim oxs As New System.Xml.Serialization.XmlSerializer(GetType(CompatibilityTestObject))
                Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps, UTF16)
                        oxs.Serialize(sw, v)
                    End Using
                End Using

                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)

                s.Position = 0
                Dim RoundTripped As CompatibilityTestObject
                Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps, UTF16)
                        RoundTripped = XmlCompatibility.ReadFile(Of CompatibilityTestObject)(sr)
                    End Using
                End Using

                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With

        '.Net Internal to XmlSerializer
        With Nothing
            Using s As New StreamEx
                Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps, UTF16)
                        XmlCompatibility.WriteFile(sw, v)
                    End Using
                End Using

                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)

                s.Position = 0
                Dim RoundTripped As CompatibilityTestObject
                Dim oxs As New System.Xml.Serialization.XmlSerializer(GetType(CompatibilityTestObject))
                Using ps As New PartialStreamEx(s, 0, Int64.MaxValue, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps, UTF16)
                        RoundTripped = oxs.Deserialize(sr)
                    End Using
                End Using
                RoundTripped.o14 = Nothing '这里.Net的原始实现不是很正确

                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With
    End Sub

    Public Sub TestMapping()
        TestObjectTreeMapper()
        TestBinarySerializer()
        TestXmlSerializer()
        TestXmlSerializerForDict()
        TestXmlSerializerCompatibility()
    End Sub
End Module
