'==========================================================================
'
'  File:        XmlSerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Xml序列化类
'  Version:     2011.03.06.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Text.RegularExpressions
Imports System.Xml.Linq
Imports System.Reflection
Imports Firefly

Namespace Mapping.XmlText
    Public Interface IXmlReader
        Function Read(Of T)(ByVal s As XElement) As T
    End Interface
    Public Interface IXmlWriter
        Function Write(Of T)(ByVal Value As T) As XElement
    End Interface
    Public Interface IXmlSerializer
        Inherits IXmlReader
        Inherits IXmlWriter
    End Interface

    ''' <remarks>
    ''' 对于非简单类型，应提供自定义序列化器
    ''' 简单类型 ::= 简单类型
    '''           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
    '''           | Boolean
    '''           | String | Decimal
    '''           | 枚举
    '''           | 数组(简单类型)
    '''           | ICollection(简单类型)
    '''           | 简单类或结构
    ''' 简单类或结构 ::= 
    '''               ( 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
    '''               | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
    '''               | 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
    '''               | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
    '''               ) AND 类型结构为树状
    ''' 对于类对象，允许出现null。
    ''' </remarks>
    Public Class XmlSerializer
        Implements IXmlSerializer

        Private ReaderResolver As XmlReaderResolver
        Private WriterResolver As XmlWriterResolver

        Private ReaderCache As IMapperResolver
        Private WriterCache As IMapperResolver

        Public Sub New()
            MyClass.New(New Type() {})
        End Sub
        Public Sub New(ByVal ExternalTypes As IEnumerable(Of Type))
            Dim ReaderReference As New ReferenceMapperResolver
            ReaderCache = ReaderReference
            ReaderResolver = New XmlReaderResolver(ReaderReference, ExternalTypes)
            ReaderReference.Inner = ReaderResolver.AsCached

            Dim WriterReference As New ReferenceMapperResolver
            WriterCache = WriterReference
            WriterResolver = New XmlWriterResolver(WriterReference, ExternalTypes)
            WriterReference.Inner = WriterResolver.AsCached
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
            ReaderResolver.PutReader(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
            WriterResolver.PutWriter(Writer)
        End Sub
        Public Sub PutReader(Of T)(ByVal Reader As Func(Of XElement, T))
            ReaderResolver.PutReader(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, XElement))
            WriterResolver.PutWriter(Writer)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderResolver.PutReaderTranslator(Translator)
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            WriterResolver.PutWriterTranslator(Translator)
        End Sub
        Public Sub PutReaderTranslator(Of M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of XElement, M))
            ReaderResolver.PutReaderTranslator(Translator)
        End Sub
        Public Sub PutWriterTranslator(Of M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of XElement, M))
            WriterResolver.PutWriterTranslator(Translator)
        End Sub

        Public Function Read(Of T)(ByVal s As XElement) As T Implements IXmlReader.Read
            Dim m = ReaderCache.ResolveProjector(Of XElement, T)()
            Return m(s)
        End Function
        Public Function Write(Of T)(ByVal Value As T) As XElement Implements IXmlWriter.Write
            Dim m = WriterCache.ResolveProjector(Of T, XElement)()
            Return m(Value)
        End Function

        Public ReadOnly Property CurrentReadingXElement As XElement
            Get
                Return ReaderResolver.CurrentReadingXElement
            End Get
        End Property
    End Class

    Public Class XmlReaderResolver
        Implements IMapperResolver

        Private Root As IMapperResolver
        Private PrimitiveResolver As PrimitiveResolver
        Private Resolver As IMapperResolver
        Private ProjectorResolverList As LinkedList(Of IProjectorResolver)
        Private DebugResolver As DebugReaderResolver

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Return Resolver.TryResolveProjector(TypePair)
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Return Resolver.TryResolveAggregator(TypePair)
        End Function

        Public Sub New(ByVal Root As IMapperResolver, ByVal ExternalTypes As IEnumerable(Of Type))
            Me.Root = Root

            PrimitiveResolver = New PrimitiveResolver

            PutReader(Function(s As String) Byte.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) UInt16.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) UInt32.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) UInt64.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) SByte.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Int16.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Int32.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Int64.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Single.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Double.Parse(s, Globalization.CultureInfo.InvariantCulture))
            PutReader(Function(s As String) Boolean.Parse(s))
            PutReader(Function(s As String) s)
            PutReader(Function(s As String) Decimal.Parse(s, Globalization.CultureInfo.InvariantCulture))

            'Reader
            'proj <- proj
            'PrimitiveResolver: (String|XElement proj Primitive) <- null
            'EnumResolver: (String proj Enum) <- null
            'XElementToStringDomainTranslator: (XElement proj R) <- (String proj R)
            'CollectionUnpacker: (XElement proj {R}) <- (XElement.SubElement proj R)
            'FieldOrPropertyProjectorResolver: (Dictionary(String, XElement) proj R) <- (XElement.SubElement proj R.Field)
            'InheritanceResolver: (XElement proj R) <- (XElement proj R.Derived)
            'XElementProjectorToProjectorDomainTranslator: (XElement proj R) <- (Dictionary(String, XElement) proj R)

            ProjectorResolverList = New LinkedList(Of IProjectorResolver)({
                PrimitiveResolver,
                New EnumResolver,
                TranslatorResolver.Create(Root, New XElementToStringDomainTranslator),
                New CollectionUnpackerTemplate(Of XElement)(New CollectionUnpacker(Root)),
                New RecordUnpackerTemplate(Of ElementUnpackerState)(
                    New FieldProjectorResolver(Root),
                    New AliasFieldProjectorResolver(Root),
                    New TagProjectorResolver(Root),
                    New TaggedUnionFieldProjectorResolver(Root),
                    New TupleElementProjectorResolver(Root)
                ),
                New InheritanceResolver(Root, ExternalTypes),
                TranslatorResolver.Create(Root, New XElementProjectorToProjectorDomainTranslator)
            })
            DebugResolver = New DebugReaderResolver(CreateMapper(ProjectorResolverList.Concatenated, EmptyAggregatorResolver))
            Resolver = DebugResolver
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutReader(Of T)(ByVal Reader As Func(Of XElement, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root, Translator))
        End Sub
        Public Sub PutReaderTranslator(Of M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of XElement, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root, Translator))
        End Sub

        Public ReadOnly Property CurrentReadingXElement As XElement
            Get
                Return DebugResolver.CurrentReadingXElement
            End Get
        End Property
    End Class

    Public Class XmlWriterResolver
        Implements IMapperResolver

        Private Root As IMapperResolver
        Private PrimitiveResolver As PrimitiveResolver
        Private Resolver As IMapperResolver
        Private ProjectorResolverList As LinkedList(Of IProjectorResolver)
        Private AggregatorResolverList As LinkedList(Of IAggregatorResolver)

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Return Resolver.TryResolveProjector(TypePair)
        End Function
        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Return Resolver.TryResolveAggregator(TypePair)
        End Function

        Public Sub New(ByVal Root As IMapperResolver, ByVal ExternalTypes As IEnumerable(Of Type))
            Me.Root = Root

            PrimitiveResolver = New PrimitiveResolver

            PutWriter(Function(b As Byte) b.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As UInt16) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As UInt32) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As UInt64) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As SByte) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As Int16) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As Int32) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(i As Int64) i.ToString(Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(f As Single) f.ToString("r", Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(f As Double) f.ToString("r", Globalization.CultureInfo.InvariantCulture))
            PutWriter(Function(b As Boolean) b.ToString())
            PutWriter(Function(s As String) s)
            PutWriter(Function(d As Decimal) d.ToString(Globalization.CultureInfo.InvariantCulture))

            'Writer
            'proj <- proj/aggr
            'PrimitiveResolver: (Primitive proj String|XElement) <- null
            'EnumResolver: (Enum proj String) <- null
            'XElementToStringRangeTranslator: (D proj XElement) <- (D proj String)
            'InheritanceResolver: (D proj XElement) <- (D.Derived proj XElement)
            'XElementAggregatorToProjectorRangeTranslator: (D proj XElement) <- (D aggr List(XElement))
            '
            'Writer
            'aggr <- proj/aggr
            'CollectionPacker: ({D} aggr Collection(XElement)) <- (D proj XElement)
            'FieldOrPropertyAggregatorResolver: (D aggr List(XElement)) <- (D.Field proj XElement)
            'XElementProjectorToAggregatorRangeTranslator: (D aggr List(XElement)) <- (D proj XElement)

            ProjectorResolverList = New LinkedList(Of IProjectorResolver)({
                PrimitiveResolver,
                New EnumResolver,
                TranslatorResolver.Create(Root, New XElementToStringRangeTranslator),
                New InheritanceResolver(Root, ExternalTypes),
                TranslatorResolver.Create(Root, New XElementAggregatorToProjectorRangeTranslator)
            })
            AggregatorResolverList = New LinkedList(Of IAggregatorResolver)({
                New CollectionPackerTemplate(Of ElementPackerState)(New CollectionPacker(Root)),
                New RecordPackerTemplate(Of ElementPackerState)(
                    New FieldAggregatorResolver(Root),
                    New AliasFieldAggregatorResolver(Root),
                    New TagAggregatorResolver(Root),
                    New TaggedUnionFieldAggregatorResolver(Root),
                    New TupleElementAggregatorResolver(Root)
                ),
                TranslatorResolver.Create(Root, New XElementProjectorToAggregatorRangeTranslator)
            })
            Resolver = CreateMapper(ProjectorResolverList.Concatenated, AggregatorResolverList.Concatenated)
        End Sub

        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
            PrimitiveResolver.PutProjector(Writer)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, XElement))
            PrimitiveResolver.PutProjector(Writer)
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of XElement, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root, Translator))
        End Sub
    End Class

    Public Module MappingXml
        Public Function GetTypeFriendlyName(ByVal Type As Type) As String
            If Type.IsArray Then
                Dim n = Type.GetArrayRank
                Dim ElementTypeName = GetTypeFriendlyName(Type.GetElementType)
                If n = 1 Then
                    Return "ArrayOf" & ElementTypeName
                End If
                Return "Array" & n & "Of" & ElementTypeName
            End If
            If Type.IsGenericType Then
                Dim Name = Regex.Match(Type.Name, "^(?<Name>.*?)`.*$", RegexOptions.ExplicitCapture).Result("${Name}")
                Return Name & "Of" & String.Join("And", (From t In Type.GetGenericArguments() Select GetTypeFriendlyName(t)).ToArray)
            End If
            Return Type.Name
        End Function
    End Module

    Public Class ElementUnpackerState
        Public Parent As XElement
        Public List As List(Of XElement)
        Public Dict As Dictionary(Of String, XElement)
        Public AttributeDict As Dictionary(Of String, XAttribute)
    End Class
    Public Class ElementPackerState
        Public UseParent As Boolean
        Public Parent As XElement
        Public List As List(Of XElement)
        Public AttributeList As List(Of XAttribute)
    End Class

    Public Class EnumResolver
        Implements IProjectorResolver

        Public Shared Function StringToEnum(Of R)(ByVal s As String) As R
            Return DirectCast([Enum].Parse(GetType(R), s), R)
        End Function
        Public Shared Function EnumToString(Of D)(ByVal v As D) As String
            Return v.ToString()
        End Function

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType Is GetType(String) AndAlso RangeType.IsEnum Then
                Dim DummyMethod = DirectCast(AddressOf StringToEnum(Of DummyType), Func(Of String, DummyType))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                Return m
            End If
            If RangeType Is GetType(String) AndAlso DomainType.IsEnum Then
                Dim DummyMethod = DirectCast(AddressOf EnumToString(Of DummyType), Func(Of DummyType, String))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                Return m
            End If
            Return Nothing
        End Function
    End Class

    Public Class CollectionUnpacker
        Implements IGenericCollectionProjectorResolver(Of XElement)

        Public Function ResolveProjector(Of R, RCollection As {New, ICollection(Of R)})() As Func(Of XElement, RCollection) Implements IGenericCollectionProjectorResolver(Of XElement).ResolveProjector
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim F =
                Function(Key As XElement) As RCollection
                    If Not Key.IsEmpty Then
                        Dim List = New RCollection()
                        For Each k In Key.Elements
                            List.Add(Mapper(k))
                        Next
                        Return List
                    Else
                        Return Nothing
                    End If
                End Function
            Return F
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class CollectionPacker
        Implements IGenericCollectionAggregatorResolver(Of ElementPackerState)

        Public Function ResolveAggregator(Of D, DCollection As ICollection(Of D))() As Action(Of DCollection, ElementPackerState) Implements IGenericCollectionAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim F =
                Sub(c As DCollection, Value As ElementPackerState)
                    For Each v In c
                        Value.List.Add(Mapper(v))
                    Next
                End Sub
            Return F
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class XElementToStringRangeTranslator
        Implements IProjectorToProjectorRangeTranslator(Of XElement, String)

        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, XElement) Implements IProjectorToProjectorRangeTranslator(Of XElement, String).TranslateProjectorToProjectorRange
            Dim FriendlyName = GetTypeFriendlyName(GetType(D))
            Return Function(v)
                       Dim s = Projector(v)
                       Return New XElement(FriendlyName, s)
                   End Function
        End Function
    End Class

    Public Class XElementToStringDomainTranslator
        Implements IProjectorToProjectorDomainTranslator(Of XElement, String)

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of XElement, R) Implements IProjectorToProjectorDomainTranslator(Of XElement, String).TranslateProjectorToProjectorDomain
            Return Function(v)
                       If v.IsEmpty Then Return Nothing
                       Return Projector(v.Value)
                   End Function
        End Function
    End Class

    Public Class XElementProjectorToProjectorDomainTranslator
        Implements IProjectorToProjectorDomainTranslator(Of XElement, ElementUnpackerState)

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of ElementUnpackerState, R)) As Func(Of XElement, R) Implements IProjectorToProjectorDomainTranslator(Of XElement, ElementUnpackerState).TranslateProjectorToProjectorDomain
            Return Function(Element) As R
                       If Not Element.IsEmpty Then
                           Dim l = Element.Elements.ToList()
                           Dim d As New Dictionary(Of String, XElement)(StringComparer.OrdinalIgnoreCase)
                           For Each e In l
                               Dim LocalName = e.Name.LocalName
                               If Not d.ContainsKey(LocalName) Then
                                   d.Add(LocalName, e)
                               End If
                           Next
                           Dim ad As New Dictionary(Of String, XAttribute)(StringComparer.OrdinalIgnoreCase)
                           For Each a In Element.Attributes
                               Dim LocalName = a.Name.LocalName
                               If Not ad.ContainsKey(LocalName) Then
                                   ad.Add(LocalName, a)
                               End If
                           Next
                           Return Projector(New ElementUnpackerState With {.Parent = Element, .List = l, .Dict = d, .AttributeDict = ad})
                       Else
                           Return Nothing
                       End If
                   End Function
        End Function
    End Class

    Public Class XElementAggregatorToProjectorRangeTranslator
        Implements IAggregatorToProjectorRangeTranslator(Of XElement, ElementPackerState)

        Public Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, ElementPackerState)) As Func(Of D, XElement) Implements IAggregatorToProjectorRangeTranslator(Of XElement, ElementPackerState).TranslateAggregatorToProjectorRange
            Dim FriendlyName = GetTypeFriendlyName(GetType(D))
            Return Function(v)
                       Dim x As XElement
                       Dim l As New List(Of XElement)
                       Dim al As New List(Of XAttribute)
                       If v IsNot Nothing Then
                           Dim s As New ElementPackerState With {.UseParent = False, .Parent = Nothing, .List = l, .AttributeList = al}
                           Aggregator(v, s)
                           If s.UseParent Then
                               x = s.Parent
                               x.Name = FriendlyName
                           ElseIf l.Count = 0 Then
                               x = New XElement(FriendlyName, "")
                           Else
                               x = New XElement(FriendlyName, l.ToArray())
                           End If
                       Else
                           x = New XElement(FriendlyName, Nothing)
                       End If
                       For Each a In al
                           x.SetAttributeValue(a.Name, a.Value)
                       Next
                       Return x
                   End Function
        End Function
    End Class

    Public Class XElementProjectorToAggregatorRangeTranslator
        Implements IProjectorToAggregatorRangeTranslator(Of ElementPackerState, XElement)

        Public Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, XElement)) As Action(Of D, ElementPackerState) Implements IProjectorToAggregatorRangeTranslator(Of ElementPackerState, XElement).TranslateProjectorToAggregatorRange
            Dim FriendlyName = GetTypeFriendlyName(GetType(D))
            Return Sub(v, s) s.List.Add(Projector(v))
        End Function
    End Class

    Public Class FieldProjectorResolver
        Implements IFieldProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Name As String) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim d = s.Dict
                    Return Mapper(d(Name))
                End Function
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IFieldProjectorResolver(Of ElementUnpackerState).ResolveProjector
            Dim Name = Member.Name
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Name)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Func(Of ElementUnpackerState, DummyType)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                Dict.Add(Type, m)
                Return m(Name)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class FieldAggregatorResolver
        Implements IFieldAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(k)
                    e.Name = Name
                    s.List.Add(e)
                End Sub
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IFieldAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim Name = Member.Name
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Name)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Action(Of DummyType, ElementPackerState)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                Dict.Add(Type, m)
                Return m(Name)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class AliasFieldProjectorResolver
        Implements IAliasFieldProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)() As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Return Mapper(s.Parent)
                End Function
            Return F
        End Function

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IAliasFieldProjectorResolver(Of ElementUnpackerState).ResolveProjector
            Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Func(Of ElementUnpackerState, DummyType)))
            Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of [Delegate])()
            Return m()
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class AliasFieldAggregatorResolver
        Implements IAliasFieldAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)() As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(k)
                    s.UseParent = True
                    s.Parent = e
                End Sub
            Return F
        End Function

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IAliasFieldAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Action(Of DummyType, ElementPackerState)))
            Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of [Delegate])()
            Return m()
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class TagProjectorResolver
        Implements ITagProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)() As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(String), GetType(R))), Func(Of String, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim TagValue = s.List.Single.Name.LocalName
                    Return Mapper(TagValue)
                End Function
            Return F
        End Function

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate] Implements ITagProjectorResolver(Of ElementUnpackerState).ResolveProjector
            Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Func(Of ElementUnpackerState, DummyType)))
            Dim m = GenericMapper.MakeDelegateMethodFromDummy(TagType).AdaptFunction(Of [Delegate])()
            Return m()
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class TagAggregatorResolver
        Implements ITagAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)() As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(String))), Func(Of D, String))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                End Sub
            Return F
        End Function

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate] Implements ITagAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Action(Of DummyType, ElementPackerState)))
            Dim m = GenericMapper.MakeDelegateMethodFromDummy(TagType).AdaptFunction(Of [Delegate])()
            Return m()
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class TaggedUnionFieldProjectorResolver
        Implements ITaggedUnionFieldProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Name As String) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim d = s.Dict
                    Return Mapper(d(Name))
                End Function
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements ITaggedUnionFieldProjectorResolver(Of ElementUnpackerState).ResolveProjector
            Dim Name = Member.Name
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Name)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Func(Of ElementUnpackerState, DummyType)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                Dict.Add(Type, m)
                Return m(Name)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class TaggedUnionFieldAggregatorResolver
        Implements ITaggedUnionFieldAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(k)
                    e.Name = Name
                    s.List.Add(e)
                End Sub
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements ITaggedUnionFieldAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim Name = Member.Name
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Name)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of String, Action(Of DummyType, ElementPackerState)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of String, [Delegate])()
                Dict.Add(Type, m)
                Return m(Name)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class TupleElementProjectorResolver
        Implements ITupleElementProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Index As Integer) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim l = s.List
                    Return Mapper(l(Index))
                End Function
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of Integer, [Delegate]))
        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate] Implements ITupleElementProjectorResolver(Of ElementUnpackerState).ResolveProjector
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Index)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Integer, Func(Of ElementUnpackerState, DummyType)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of Integer, [Delegate])()
                Dict.Add(Type, m)
                Return m(Index)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    Public Class TupleElementAggregatorResolver
        Implements ITupleElementAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Index As Integer) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    s.List.Add(Mapper(k))
                End Sub
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of Integer, [Delegate]))
        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate] Implements ITupleElementAggregatorResolver(Of ElementPackerState).ResolveAggregator
            If Dict.ContainsKey(Type) Then
                Dim m = Dict(Type)
                Return m(Index)
            Else
                Dim GenericMapper = DirectCast(AddressOf Resolve(Of DummyType), Func(Of Integer, Action(Of DummyType, ElementPackerState)))
                Dim m = GenericMapper.MakeDelegateMethodFromDummy(Type).AdaptFunction(Of Integer, [Delegate])()
                Dict.Add(Type, m)
                Return m(Index)
            End If
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    Public Class InheritanceResolver
        Implements IProjectorResolver

        Private Function ResolveRange(Of R)() As Func(Of XElement, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(XElement), GetType(R))), Func(Of XElement, R))
            Dim Dict As New Dictionary(Of Type, Func(Of XElement, R))
            Dim F =
                Function(k As XElement) As R
                    If k.IsEmpty Then Return Mapper(k)
                    If k.Attribute("Type") Is Nothing Then Return Mapper(k)
                    Dim RealTypeName = k.Attribute("Type").Value
                    If Not ExternalTypeDict.ContainsKey(RealTypeName) Then Throw New InvalidOperationException("ExternalTypeNotFound: {0}".Formats(RealTypeName))
                    Dim RealType = ExternalTypeDict(RealTypeName)

                    If Dict.ContainsKey(RealType) Then
                        Dim DynamicMapper = Dict(RealType)
                        Return DynamicMapper(k)
                    End If

                    Dim TypePair = CreatePair(GetType(XElement), RealType)
                    ProjectorCache.Add(TypePair)
                    Try
                        Dim DynamicMapper = InnerResolver.ResolveProjector(TypePair).AdaptFunction(Of XElement, R)()
                        Dict.Add(RealType, DynamicMapper)
                        Return DynamicMapper(k)
                    Finally
                        ProjectorCache.Remove(TypePair)
                    End Try
                End Function
            Return F
        End Function
        Private Function ResolveDomain(Of D)() As Func(Of D, XElement)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(XElement))), Func(Of D, XElement))
            Dim TypeName = GetTypeFriendlyName(GetType(D))
            Dim Dict As New Dictionary(Of Type, Func(Of D, XElement))
            Dim F =
                Function(k As D) As XElement
                    If k Is Nothing Then Return Mapper(k)
                    Dim RealType = k.GetType()
                    If RealType Is GetType(D) Then Return Mapper(k)
                    Dim RealTypeName = GetTypeFriendlyName(RealType)
                    If Not ExternalTypeDict.ContainsKey(RealTypeName) Then Throw New InvalidOperationException("ExternalTypeNotFound: {0}".Formats(RealTypeName))
                    If ExternalTypeDict(RealTypeName) IsNot RealType Then Throw New InvalidOperationException("ExternalTypeMismatched: {0}".Formats(RealTypeName))

                    If Dict.ContainsKey(RealType) Then
                        Dim DynamicMapper = Dict(RealType)
                        Return DynamicMapper(k)
                    End If

                    Dim TypePair = CreatePair(RealType, GetType(XElement))
                    ProjectorCache.Add(TypePair)
                    Try
                        Dim DynamicMapper = InnerResolver.ResolveProjector(TypePair).AdaptFunction(Of D, XElement)()
                        Dict.Add(RealType, DynamicMapper)
                        Dim e = DynamicMapper(k)
                        If e.Name = RealTypeName Then e.Name = TypeName
                        e.SetAttributeValue("Type", RealTypeName)
                        Return e
                    Finally
                        ProjectorCache.Remove(TypePair)
                    End Try
                End Function
            Return F
        End Function

        Private ProjectorCache As New HashSet(Of KeyValuePair(Of Type, Type))
        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType Is GetType(XElement) AndAlso RangeType.IsClass Then
                If ProjectorCache.Contains(TypePair) Then Return Nothing
                ProjectorCache.Add(TypePair)
                Try
                    Dim DummyMethod = DirectCast(AddressOf ResolveRange(Of DummyType), Func(Of Func(Of XElement, DummyType)))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(RangeType)
                    Return m.StaticDynamicInvoke(Of [Delegate])()
                Finally
                    ProjectorCache.Remove(TypePair)
                End Try
            End If
            If RangeType Is GetType(XElement) AndAlso DomainType.IsClass Then
                If ProjectorCache.Contains(TypePair) Then Return Nothing
                ProjectorCache.Add(TypePair)
                Try
                    Dim DummyMethod = DirectCast(AddressOf ResolveDomain(Of DummyType), Func(Of Func(Of DummyType, XElement)))
                    Dim m = DummyMethod.MakeDelegateMethodFromDummy(DomainType)
                    Return m.StaticDynamicInvoke(Of [Delegate])()
                Finally
                    ProjectorCache.Remove(TypePair)
                End Try
            End If
            Return Nothing
        End Function

        Private InnerResolver As IProjectorResolver
        Private ExternalTypeDict As Dictionary(Of String, Type)
        Public Sub New(ByVal Resolver As IProjectorResolver, ByVal ExternalTypes As IEnumerable(Of Type))
            Me.InnerResolver = Resolver.AsNoncircular
            Me.ExternalTypeDict = ExternalTypes.ToDictionary(Function(type) GetTypeFriendlyName(type), StringComparer.OrdinalIgnoreCase)
        End Sub
    End Class

    Public Class DebugReaderResolver
        Implements IMapperResolver

        Private InnerResolver As IMapperResolver
        Public Sub New(ByVal InnerResolver As IMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Private CurrentReadingXElementValue As XElement
        Private Sub SetCurrentXElement(ByVal x As XElement)
            CurrentReadingXElementValue = x
        End Sub
        Public ReadOnly Property CurrentReadingXElement As XElement
            Get
                Return CurrentReadingXElementValue
            End Get
        End Property

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim m = InnerResolver.TryResolveProjector(TypePair)
            If TypePair.Key IsNot GetType(XElement) Then Return m
            If m Is Nothing Then Return Nothing

            Dim Parameters = m.GetParameters().Select(Function(p) Expression.Parameter(p.ParameterType, p.Name)).ToArray()
            Dim DebugDelegate = DirectCast(DirectCast(AddressOf Me.SetCurrentXElement, Action(Of XElement)), [Delegate])
            Dim DebugCall = CreatePair(DebugDelegate, New Expression() {Parameters.First})
            Dim OriginalCall = CreatePair(m, Parameters.Select(Function(p) DirectCast(p, Expression)).ToArray())
            Dim Context = CreateDelegateExpressionContext({DebugCall, OriginalCall})
            Dim FunctionLambda = Expression.Lambda(m.GetType(), Expression.Block(Context.DelegateExpressions), Parameters)

            Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
        End Function

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Dim m = InnerResolver.TryResolveAggregator(TypePair)
            If TypePair.Key IsNot GetType(XElement) Then Return m
            If m Is Nothing Then Return Nothing

            Dim Parameters = m.GetParameters().Select(Function(p) Expression.Parameter(p.ParameterType, p.Name)).ToArray()
            Dim DebugDelegate = DirectCast(DirectCast(AddressOf Me.SetCurrentXElement, Action(Of XElement)), [Delegate])
            Dim DebugCall = CreatePair(DebugDelegate, New Expression() {Parameters.First})
            Dim OriginalCall = CreatePair(m, Parameters.Select(Function(p) DirectCast(p, Expression)).ToArray())
            Dim Context = CreateDelegateExpressionContext({DebugCall, OriginalCall})
            Dim FunctionLambda = Expression.Lambda(m.GetType(), Expression.Block(Context.DelegateExpressions), Parameters)

            Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
        End Function
    End Class
End Namespace
