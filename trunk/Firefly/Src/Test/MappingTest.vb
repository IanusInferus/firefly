Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Diagnostics.Debug
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Texting
Imports Firefly.Mapping
Imports Firefly.Mapping.MetaProgramming
Imports Firefly.Mapping.XmlText
Imports Firefly.Setting

Public Module MappingTest
    Public Sub TestMetaProgramming()
        Dim g = Function(i As Integer) i
        Dim h = Function(i As Integer) i

        Dim hg = DirectCast(g.Compose(h), Func(Of Integer, Integer))

        Assert(hg(1) = 1)
        Assert(hg(2) = 2)

        Dim k = Function(i As Integer, j As Integer) i + j
        Dim l = k.Curry(1).AdaptFunction(Of Integer, Integer)()

        Assert(l(1) = 2)
        Assert(l(2) = 3)
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
            Return Left.i = Right.i AndAlso Left.s = Right.s AndAlso Left.o.h = Right.o.h AndAlso Left.a.SequenceEqual(Right.a) AndAlso Left.l.ToArray.SequenceEqual(Right.l.ToArray) AndAlso Left.l2.ToArray.SequenceEqual(Right.l2.ToArray) AndAlso Left.l3.ToArray.SequenceEqual(Right.l3.ToArray) AndAlso Left.e1 = Right.e1 AndAlso Left.p.Key = Right.p.Key AndAlso Left.p.Value = Right.p.Value AndAlso Left.str = Right.str
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
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(R))), Func(Of D, R))
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

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class GenericListAggregatorResolver(Of R)
        Implements IGenericCollectionAggregatorResolver(Of R)

        Public Function ResolveAggregator(Of D, DList As ICollection(Of D))() As Action(Of DList, R) Implements IGenericCollectionAggregatorResolver(Of R).ResolveAggregator
            Dim Mapper = DirectCast(InnerResolver.ResolveAggregator(CreatePair(GetType(D), GetType(R))), Action(Of D, R))
            Dim F =
                Sub(list As DList, Value As R)
                    Dim Size = 3
                    For n = 0 To Size - 1
                        Mapper(list(n), Value)
                    Next
                End Sub
            Return F
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Sub TestObjectTreeMapper()
        Dim Count = 0

        With Nothing
            Dim mp As New ReferenceProjectorResolver
            Dim pr = New PrimitiveResolver
            Dim er = New Binary.EnumUnpacker(Of Integer)(mp)
            Dim cr = New CollectionUnpackerTemplate(Of Integer)(New GenericCollectionProjectorResolver(Of Integer)(mp))
            Dim csr = New RecordUnpackerTemplate(Of Integer)(mp)
            Dim mprs As New List(Of IProjectorResolver) From {pr, er, cr, csr}
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
            mp.Inner = mprs.Concatenated

            Dim BuiltObject = mp.Project(Of Integer, SerializerTestObject)(0)
            Assert(TestObject = BuiltObject)
        End With

        Dim Count2 = 0
        With Nothing
            Dim mp As New ReferenceAggregatorResolver
            Dim pr = New PrimitiveResolver
            Dim er = New Binary.EnumPacker(Of Integer)(mp)
            Dim cr = New CollectionPackerTemplate(Of Integer)(New GenericListAggregatorResolver(Of Integer)(mp))
            Dim csr = New RecordPackerTemplate(Of Integer)(mp)
            Dim mprs As New List(Of IAggregatorResolver) From {pr, er, cr, csr}
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
            mp.Inner = mprs.Concatenated

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

        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer

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

        Dim XmlRoundTripped As SerializerTestObject

        Using s = Streams.CreateMemoryStream
            Using ps = s.Partialize(0, Int64.MaxValue, 0, False)
                Using sw = Txt.CreateTextWriter(ps.AsNewWriting, UTF16)
                    Xml.WriteFile(sw, TestObject, New Xml.IMapper() {New ByteArrayEncoder})
                End Using
            End Using
            s.Position = 0
            Using ps = s.Partialize(0, s.Length, False)
                Using sr = Txt.CreateTextReader(ps.AsNewReading, UTF16)
                    XmlRoundTripped = Xml.ReadFile(Of SerializerTestObject)(sr, New Xml.IMapper() {New ByteArrayEncoder})
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
            Using s = Streams.CreateMemoryStream
                Using ps = s.Partialize(0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps.AsNewWriting, UTF16)
                        oxs.Serialize(sw, v)
                    End Using
                End Using
                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)
                Dim RoundTripped As CompatibilityTestObject
                Using ps = s.Partialize(0, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps.AsNewReading, UTF16)
                        RoundTripped = oxs.Deserialize(sr)
                    End Using
                End Using
                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With

        '.Net Internal to XmlSerializer
        With Nothing
            Using s = Streams.CreateMemoryStream
                Dim oxs As New System.Xml.Serialization.XmlSerializer(GetType(CompatibilityTestObject))
                Using ps = s.Partialize(0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps.AsNewWriting, UTF16)
                        oxs.Serialize(sw, v)
                    End Using
                End Using

                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)

                s.Position = 0
                Dim RoundTripped As CompatibilityTestObject
                Using ps = s.Partialize(0, Int64.MaxValue, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps.AsNewReading, UTF16)
                        RoundTripped = XmlCompatibility.ReadFile(Of CompatibilityTestObject)(sr)
                    End Using
                End Using

                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With

        '.Net Internal to XmlSerializer
        With Nothing
            Using s = Streams.CreateMemoryStream
                Using ps = s.Partialize(0, Int64.MaxValue, 0, False)
                    Using sw = Txt.CreateTextWriter(ps.AsNewWriting, UTF16)
                        XmlCompatibility.WriteFile(sw, v)
                    End Using
                End Using

                s.Position = 0
                Dim Text = s.ReadStringWithNull(s.Length, UTF16)

                s.Position = 0
                Dim RoundTripped As CompatibilityTestObject
                Dim oxs As New System.Xml.Serialization.XmlSerializer(GetType(CompatibilityTestObject))
                Using ps = s.Partialize(0, Int64.MaxValue, s.Length, False)
                    Using sr = Txt.CreateTextReader(ps.AsNewReading, UTF16)
                        RoundTripped = oxs.Deserialize(sr)
                    End Using
                End Using
                RoundTripped.o14 = Nothing '这里.Net的原始实现不是很正确

                Assert(Object.Equals(v, RoundTripped))
            End Using
        End With
    End Sub

    <MetaSchema.Alias()> Public Class AliasObject
        Public i As Integer = 3
        Public Shared Operator =(ByVal Left As AliasObject, ByVal Right As AliasObject) As Boolean
            Return Left.i = Right.i
        End Operator
        Public Shared Operator <>(ByVal Left As AliasObject, ByVal Right As AliasObject) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class

    Public Enum TaggedUnionObjectTag
        Item1
        Item2
        Item3
        Item4
    End Enum
    <MetaSchema.TaggedUnion()> Public Class TaggedUnionObject
        <MetaSchema.Tag()> Public _Tag As TaggedUnionObjectTag
        Public Item1 As Integer
        Public Item2 As Int16
        Public Item3 As Byte
        Public Item4 As TaggedUnionObject
        Public Shared Function Equal(ByVal Left As TaggedUnionObject, ByVal Right As TaggedUnionObject) As Boolean
            If Left._Tag <> Right._Tag Then Return False
            Select Case Left._Tag
                Case TaggedUnionObjectTag.Item1
                    Return Left.Item1 = Right.Item1
                Case TaggedUnionObjectTag.Item2
                    Return Left.Item2 = Right.Item2
                Case TaggedUnionObjectTag.Item3
                    Return Left.Item3 = Right.Item3
                Case TaggedUnionObjectTag.Item4
                    Return Equal(Left.Item4, Right.Item4)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Public Shared Operator =(ByVal Left As TaggedUnionObject, ByVal Right As TaggedUnionObject) As Boolean
            Return Equal(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As TaggedUnionObject, ByVal Right As TaggedUnionObject) As Boolean
            Return Not Equal(Left, Right)
        End Operator
    End Class

    <MetaSchema.Tuple()> Public Class TupleObject
        Public Item1 As Integer = 1
        Public Item2 As Int16 = 2
        Public Item3 As Byte = 3
        Public Shared Operator =(ByVal Left As TupleObject, ByVal Right As TupleObject) As Boolean
            Return Left.Item1 = Right.Item1 AndAlso Left.Item2 = Right.Item2 AndAlso Left.Item3 = Right.Item3
        End Operator
        Public Shared Operator <>(ByVal Left As TupleObject, ByVal Right As TupleObject) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class

    <MetaSchema.Alias()> Public Class Alias2Object
        Public i As MixedObject = New MixedObject With {._Tag = MixedObjectTag.Item3, .Item3 = New Alias3Object With {.i = 1}}
        Public Shared Operator =(ByVal Left As Alias2Object, ByVal Right As Alias2Object) As Boolean
            Return Left.i = Right.i
        End Operator
        Public Shared Operator <>(ByVal Left As Alias2Object, ByVal Right As Alias2Object) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class
    <MetaSchema.Alias()> Public Class Alias3Object
        Public i As Byte = 123
        Public Shared Operator =(ByVal Left As Alias3Object, ByVal Right As Alias3Object) As Boolean
            Return Left.i = Right.i
        End Operator
        Public Shared Operator <>(ByVal Left As Alias3Object, ByVal Right As Alias3Object) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class
    <MetaSchema.Tuple()> Public Class Tuple2Object
        Public Item1 As Alias2Object = New Alias2Object
        Public Item2 As MixedObject = New MixedObject With {._Tag = MixedObjectTag.Item3, .Item3 = New Alias3Object With {.i = 2}}
        Public Item3 As New Alias3Object With {.i = 3}
        Public Shared Operator =(ByVal Left As Tuple2Object, ByVal Right As Tuple2Object) As Boolean
            Return Left.Item1 = Right.Item1 AndAlso Left.Item2 = Right.Item2 AndAlso Left.Item3 = Right.Item3
        End Operator
        Public Shared Operator <>(ByVal Left As Tuple2Object, ByVal Right As Tuple2Object) As Boolean
            Return Not (Left = Right)
        End Operator
    End Class
    Public Enum MixedObjectTag
        Item1
        Item2
        Item3
        Item4
    End Enum
    <MetaSchema.TaggedUnion()> Public Class MixedObject
        <MetaSchema.Tag()> Public _Tag As MixedObjectTag
        Public Item1 As Alias2Object
        Public Item2 As Tuple2Object
        Public Item3 As Alias3Object
        Public Item4 As MixedObject
        Public Shared Function Equal(ByVal Left As MixedObject, ByVal Right As MixedObject) As Boolean
            If Left._Tag <> Right._Tag Then Return False
            Select Case Left._Tag
                Case MixedObjectTag.Item1
                    Return Left.Item1 = Right.Item1
                Case MixedObjectTag.Item2
                    Return Left.Item2 = Right.Item2
                Case MixedObjectTag.Item3
                    Return Left.Item3 = Right.Item3
                Case MixedObjectTag.Item4
                    Return Equal(Left.Item4, Right.Item4)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End Function
        Public Shared Operator =(ByVal Left As MixedObject, ByVal Right As MixedObject) As Boolean
            Return Equal(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As MixedObject, ByVal Right As MixedObject) As Boolean
            Return Not Equal(Left, Right)
        End Operator
    End Class

    Public Sub TestAlias()
        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer
            Dim xs As New XmlSerializer

            Dim a1 As New AliasObject
            Dim a2 As AliasObject

            bs.Write(s, a1)
            s.Position = 0
            a2 = bs.Read(Of AliasObject)(s)
            Assert(a1 = a2)

            Dim x = xs.Write(a1)
            Dim a3 = xs.Read(Of AliasObject)(x)
            Assert(a1 = a3)
        End Using
    End Sub

    Public Sub TestTaggedUnion()
        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer
            Dim xs As New XmlSerializer

            Dim a1 As New TaggedUnionObject With {._Tag = TaggedUnionObjectTag.Item4, .Item4 = New TaggedUnionObject With {._Tag = TaggedUnionObjectTag.Item2, .Item2 = 2}}
            Dim a2 As TaggedUnionObject

            bs.Write(s, a1)
            Assert(s.Length = 10)

            s.Position = 0
            a2 = bs.Read(Of TaggedUnionObject)(s)
            Assert(a1 = a2)

            Dim x = xs.Write(a1)
            Dim a3 = xs.Read(Of TaggedUnionObject)(x)
            Assert(a1 = a3)
        End Using
    End Sub

    Public Sub TestTuple()
        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer
            Dim xs As New XmlSerializer

            Dim a1 As New TupleObject
            Dim a2 As TupleObject

            bs.Write(s, a1)
            s.Position = 0
            a2 = bs.Read(Of TupleObject)(s)
            Assert(a1 = a2)

            Dim x = xs.Write(a1)
            Dim a3 = xs.Read(Of TupleObject)(x)
            Assert(a1 = a3)
        End Using
    End Sub

    Public Sub TestMixed()
        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer
            Dim xs As New XmlSerializer

            Dim a1 As New MixedObject With {._Tag = MixedObjectTag.Item2, .Item2 = New Tuple2Object}
            Dim a2 As MixedObject

            bs.Write(s, a1)

            s.Position = 0
            a2 = bs.Read(Of MixedObject)(s)
            Assert(a1 = a2)

            Dim x = xs.Write(a1)
            Dim a3 = xs.Read(Of MixedObject)(x)
            Assert(a1 = a3)
        End Using
    End Sub

    Public Class RecursiveObject
        Public Items As RecursiveObject()
    End Class

    Public Sub TestDebuggerDisplayer()
        Dim o As New RecursiveObject With {.Items = {New RecursiveObject With {.Items = New RecursiveObject() {}}, New RecursiveObject With {.Items = Nothing}}}
        Dim s = MetaSchema.DebuggerDisplayer.ConvertToString(o)

        Dim o2 As Double? = 3
        Dim s2 = MetaSchema.DebuggerDisplayer.ConvertToString(o2)
    End Sub

    Public Sub TestRecursive()
        Using s = Streams.CreateMemoryStream
            Dim bs As New Binary.BinarySerializer
            Dim xs As New XmlSerializer

            Dim o As New RecursiveObject With {.Items = {New RecursiveObject With {.Items = New RecursiveObject() {}}}}

            Dim o2 As New RecursiveObject
            o2.Items = New RecursiveObject() {o2}

            bs.Write(s, o)
            s.Position = 0
            Dim o_2 = bs.Read(Of RecursiveObject)(s)

            Dim x = xs.Write(o)
            Dim o_3 = xs.Read(Of RecursiveObject)(x)

            Try
                bs.Write(s, o2)
                Assert(False)
            Catch ex As InvalidOperationException
                Assert(True)
            End Try

            Try
                Dim x_2 = xs.Write(o2)
                Assert(False)
            Catch ex As InvalidOperationException
                Assert(True)
            End Try
        End Using
    End Sub

    Public Sub TestMapping()
        TestMetaProgramming()
        TestObjectTreeMapper()
        TestBinarySerializer()
        TestXmlSerializer()
        TestXmlSerializerForDict()
        TestXmlSerializerCompatibility()
        TestAlias()
        TestTaggedUnion()
        TestTuple()
        TestMixed()
        TestDebuggerDisplayer()
        TestRecursive()
    End Sub
End Module
