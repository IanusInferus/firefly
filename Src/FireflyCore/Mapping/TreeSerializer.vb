'==========================================================================
'
'  File:        TreeSerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Tree序列化类
'  Version:     2019.04.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports Firefly
Imports Firefly.Mapping.MetaProgramming
Imports Firefly.Texting
Imports Firefly.Texting.TreeFormat
Imports Firefly.Texting.TreeFormat.Semantics

Namespace Mapping.TreeText
    Public Interface ITreeReader
        Function Read(Of T)(ByVal s As Forest) As T
        Function Read(Of T)(ByVal s As KeyValuePair(Of Forest, Dictionary(Of Object, Syntax.FileTextRange))) As KeyValuePair(Of T, Dictionary(Of Object, Syntax.FileTextRange))
    End Interface
    Public Interface ITreeWriter
        Function Write(Of T)(ByVal Value As T) As Forest
        Function Write(Of T)(ByVal Value As KeyValuePair(Of T, Dictionary(Of Object, Object))) As KeyValuePair(Of Forest, Dictionary(Of Object, Object))
    End Interface
    Public Interface ITreeSerializer
        Inherits ITreeReader
        Inherits ITreeWriter
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
    Public Class TreeSerializer
        Implements ITreeSerializer

        Private ReaderResolver As TreeReaderResolver
        Private WriterResolver As TreeWriterResolver

        Private ReaderCache As IMapperResolver
        Private WriterCache As IMapperResolver

        Public Sub New()
            MyClass.New(True)
        End Sub
        Public Sub New(ByVal UseByteArrayAndListTranslator As Boolean)
            Dim ReaderReference As New ReferenceMapperResolver
            ReaderCache = ReaderReference
            ReaderResolver = New TreeReaderResolver(ReaderReference)
            ReaderReference.Inner = ReaderResolver.AsCached

            Dim WriterReference As New ReferenceMapperResolver
            WriterCache = WriterReference
            WriterResolver = New TreeWriterResolver(WriterReference)
            WriterReference.Inner = WriterResolver.AsCached

            If UseByteArrayAndListTranslator Then
                Dim bat As New ByteArrayTranslator
                PutReaderTranslator(bat)
                PutWriterTranslator(bat)
                Dim blt As New ByteListTranslator
                PutReaderTranslator(blt)
                PutWriterTranslator(blt)
            End If
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
            ReaderResolver.PutReader(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
            WriterResolver.PutWriter(Writer)
        End Sub
        Public Sub PutReader(Of T)(ByVal Reader As Func(Of NodeContext, T))
            ReaderResolver.PutReader(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, Node))
            WriterResolver.PutWriter(Writer)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderResolver.PutReaderTranslator(Translator)
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            WriterResolver.PutWriterTranslator(Translator)
        End Sub
        Public Sub PutReaderTranslator(Of M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of NodeContext, M))
            ReaderResolver.PutReaderTranslator(Translator)
        End Sub
        Public Sub PutWriterTranslator(Of M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of Node, M))
            WriterResolver.PutWriterTranslator(Translator)
        End Sub

        Public Function Read(Of T)(ByVal s As Forest) As T Implements ITreeReader.Read
            Return Read(Of T)(New KeyValuePair(Of Forest, Dictionary(Of Object, Syntax.FileTextRange))(s, New Dictionary(Of Object, Syntax.FileTextRange))).Key
        End Function
        Public Function Read(Of T)(s As KeyValuePair(Of Forest, Dictionary(Of Object, Syntax.FileTextRange))) As KeyValuePair(Of T, Dictionary(Of Object, Syntax.FileTextRange)) Implements ITreeReader.Read
            Dim TargetPositions = New Dictionary(Of Object, Syntax.FileTextRange)
            Dim m = ReaderCache.ResolveProjector(Of NodeContext, T)()
            Dim Result = m(New NodeContext With {.Value = s.Key.Nodes.Single(), .SourcePositions = s.Value, .TargetPositions = TargetPositions})
            Return New KeyValuePair(Of T, Dictionary(Of Object, Syntax.FileTextRange))(Result, TargetPositions)
        End Function
        Public Function Write(Of T)(ByVal Value As T) As Forest Implements ITreeWriter.Write
            Return Write(Of T)(New KeyValuePair(Of T, Dictionary(Of Object, Object))(Value, New Dictionary(Of Object, Object))).Key
        End Function
        Public Function Write(Of T)(ByVal Value As KeyValuePair(Of T, Dictionary(Of Object, Object))) As KeyValuePair(Of Forest, Dictionary(Of Object, Object)) Implements ITreeWriter.Write
            Dim TargetPositions = New Dictionary(Of Object, Object)
            Dim m = WriterCache.ResolveProjector(Of Context(Of T), Node)()
            Dim Result = m(New Context(Of T) With {.Value = Value.Key, .SourceMappings = Value.Value, .TargetMappings = TargetPositions})
            Return New KeyValuePair(Of Forest, Dictionary(Of Object, Object))(New Forest With {.Nodes = New List(Of Node) From {Result}}, TargetPositions)
        End Function

        Public ReadOnly Property CurrentReadingNode As Node
            Get
                Return ReaderResolver.CurrentReadingNode
            End Get
        End Property
    End Class

    Public Class TreeReaderResolver
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

        Public Sub New(ByVal Root As IMapperResolver)
            Me.Root = Root

            PrimitiveResolver = New PrimitiveResolver

            PutReader(Function(s As String) InvariantParseUInt8(s))
            PutReader(Function(s As String) InvariantParseUInt16(s))
            PutReader(Function(s As String) InvariantParseUInt32(s))
            PutReader(Function(s As String) InvariantParseUInt64(s))
            PutReader(Function(s As String) InvariantParseInt8(s))
            PutReader(Function(s As String) InvariantParseInt16(s))
            PutReader(Function(s As String) InvariantParseInt32(s))
            PutReader(Function(s As String) InvariantParseInt64(s))
            PutReader(Function(s As String) InvariantParseFloat32(s))
            PutReader(Function(s As String) InvariantParseFloat64(s))
            PutReader(Function(s As String) InvariantParseBoolean(s))
            PutReader(Function(s As String) s)
            PutReader(Function(s As String) InvariantParseDecimal(s))

            'Reader
            'proj <- proj
            'PrimitiveResolver: (String|NodeContext proj Primitive) <- null
            'EnumResolver: (String proj Enum) <- null
            'ContextToStringDomainTranslator: (NodeContext proj R) <- (String proj R)
            'CollectionUnpacker: (Context proj {R}) <- (NodeContext.SubElement proj R)
            'FieldOrPropertyProjectorResolver: (Dictionary(String, Context) proj R) <- (NodeContext.SubElement proj R.Field)
            'ContextProjectorToProjectorDomainTranslator: (NodeContext proj R) <- (Dictionary(String, Context) proj R)

            ProjectorResolverList = New LinkedList(Of IProjectorResolver)({
                PrimitiveResolver,
                New EnumResolver,
                TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New ContextToStringDomainTranslator),
                New CollectionUnpackerTemplate(Of NodeContext)(New CollectionUnpacker(Root.AsRuntimeDomainNoncircular)),
                New RecordUnpackerTemplate(Of ElementUnpackerState)(
                    New FieldProjectorResolver(Root.AsRuntimeDomainNoncircular),
                    New AliasFieldProjectorResolver(Root.AsRuntimeDomainNoncircular),
                    New TagProjectorResolver(Root.AsRuntimeDomainNoncircular),
                    New TaggedUnionAlternativeProjectorResolver(Root.AsRuntimeDomainNoncircular),
                    New TupleElementProjectorResolver(Root.AsRuntimeDomainNoncircular)
                ),
                TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New ContextProjectorToProjectorDomainTranslator)
            })
            DebugResolver = New DebugReaderResolver(CreateMapper(ProjectorResolverList.Concatenated, EmptyAggregatorResolver))
            Resolver = DebugResolver
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of String, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutReader(Of T)(ByVal Reader As Func(Of NodeContext, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, Translator))
        End Sub
        Public Sub PutReaderTranslator(Of M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of NodeContext, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, Translator))
        End Sub

        Public ReadOnly Property CurrentReadingNode As Node
            Get
                Return DebugResolver.CurrentReadingNode
            End Get
        End Property
    End Class

    Public Class TreeWriterResolver
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

        Public Sub New(ByVal Root As IMapperResolver)
            Me.Root = Root

            PrimitiveResolver = New PrimitiveResolver

            PutWriter(Function(b As Byte) b.ToInvariantString())
            PutWriter(Function(i As UInt16) i.ToInvariantString())
            PutWriter(Function(i As UInt32) i.ToInvariantString())
            PutWriter(Function(i As UInt64) i.ToInvariantString())
            PutWriter(Function(i As SByte) i.ToInvariantString())
            PutWriter(Function(i As Int16) i.ToInvariantString())
            PutWriter(Function(i As Int32) i.ToInvariantString())
            PutWriter(Function(i As Int64) i.ToInvariantString())
            PutWriter(Function(f As Single) f.ToInvariantString())
            PutWriter(Function(f As Double) f.ToInvariantString())
            PutWriter(Function(b As Boolean) b.ToInvariantString())
            PutWriter(Function(s As String) s)
            PutWriter(Function(d As Decimal) d.ToInvariantString())

            'Writer
            'proj <- proj/aggr
            'PrimitiveResolver: (Primitive proj String|Node) <- null
            'EnumResolver: (Enum proj String) <- null
            'ContextDomainTranslatorProjectorResolver: (Context(D) proj String) <- (D proj String)
            'NodeToStringRangeTranslator: (Context(D) proj Node) <- (Context(D) proj String)
            'NodeAggregatorToProjectorRangeTranslator: (Context(D) proj Node) <- (Context(D) aggr List(Node))
            '
            'Writer
            'aggr <- proj/aggr
            'ContextDomainTranslatorAggregatorResolver: (Context(D) aggr List(Node)) <- (D aggr List(Node))
            'CollectionPacker: ({D} aggr Collection(Node)) <- (Context(D) proj Node)
            'FieldOrPropertyAggregatorResolver: (D aggr List(Node)) <- (Context(D.Field) proj Node)
            'NodeProjectorToAggregatorRangeTranslator: (Context(D) aggr List(Node)) <- (Context(D) proj Node)

            ProjectorResolverList = New LinkedList(Of IProjectorResolver)({
                PrimitiveResolver,
                New EnumResolver,
                New ContextDomainTranslatorProjectorResolver(Root.AsRuntimeDomainNoncircular),
                TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New NodeToStringRangeTranslator),
                TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New NodeAggregatorToProjectorRangeTranslator)
            })
            AggregatorResolverList = New LinkedList(Of IAggregatorResolver)({
                New ContextDomainTranslatorAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                New CollectionPackerTemplate(Of ElementPackerState)(New CollectionPacker(Root.AsRuntimeDomainNoncircular)),
                New RecordPackerTemplate(Of ElementPackerState)(
                    New FieldAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                    New AliasFieldAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                    New TagAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                    New TaggedUnionAlternativeAggregatorResolver(Root.AsRuntimeDomainNoncircular),
                    New TupleElementAggregatorResolver(Root.AsRuntimeDomainNoncircular)
                ),
                TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, New NodeProjectorToAggregatorRangeTranslator)
            })
            Resolver = CreateMapper(ProjectorResolverList.Concatenated, AggregatorResolverList.Concatenated)
        End Sub

        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, String))
            PrimitiveResolver.PutProjector(Writer)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Func(Of T, Node))
            PrimitiveResolver.PutProjector(Writer)
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of Node, M))
            ProjectorResolverList.AddFirst(TranslatorResolver.Create(Root.AsRuntimeDomainNoncircular, Translator))
        End Sub
    End Class

    Public Module MappingTree
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

    Public Class NodeContext
        Public Value As Node
        Public SourcePositions As Dictionary(Of Object, Syntax.FileTextRange)
        Public TargetPositions As Dictionary(Of Object, Syntax.FileTextRange)
    End Class
    Public Interface IContext
        ReadOnly Property Value As Object
        Property SourceMappings As Dictionary(Of Object, Object)
        Property TargetMappings As Dictionary(Of Object, Object)
    End Interface
    Public Class Context(Of T)
        Implements IContext
        Public Value As T
        Private ReadOnly Property ContextValue As Object Implements IContext.Value
            Get
                Return Value
            End Get
        End Property

        Public Property SourceMappings As Dictionary(Of Object, Object) Implements IContext.SourceMappings
        Public Property TargetMappings As Dictionary(Of Object, Object) Implements IContext.TargetMappings
    End Class
    Public Class ElementUnpackerState
        Public Parent As NodeContext
        Public List As List(Of NodeContext)
        Public Dict As Dictionary(Of String, NodeContext)
    End Class
    Public Class ElementPackerState
        Public UseParent As Boolean
        Public Parent As Node
        Public List As List(Of Node)
        Public SourceMappings As Dictionary(Of Object, Object)
        Public TargetMappings As Dictionary(Of Object, Object)
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
        Implements IGenericCollectionProjectorResolver(Of NodeContext)

        Public Function ResolveProjector(Of R, RCollection As {New, ICollection(Of R)})() As Func(Of NodeContext, RCollection) Implements IGenericCollectionProjectorResolver(Of NodeContext).ResolveProjector
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(NodeContext), GetType(R))), Func(Of NodeContext, R))
            Dim F =
                Function(Key As NodeContext) As RCollection
                    If Key.Value.OnEmpty Then Return Nothing
                    If Key.Value.OnLeaf Then Throw New InvalidOperationException
                    If Key.Value.OnStem Then
                        Dim List = New RCollection()
                        For Each k In Key.Value.Stem.Children
                            List.Add(Mapper(New NodeContext With {.Value = k, .SourcePositions = Key.SourcePositions, .TargetPositions = Key.TargetPositions}))
                        Next
                        Return List
                    Else
                        Throw New InvalidOperationException
                    End If
                End Function
            Return F
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class
    Public Class CollectionPacker
        Implements IGenericCollectionAggregatorResolver(Of ElementPackerState)

        Public Function ResolveAggregator(Of D, DCollection As ICollection(Of D))() As Action(Of DCollection, ElementPackerState) Implements IGenericCollectionAggregatorResolver(Of ElementPackerState).ResolveAggregator
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(Context(Of D)), GetType(Node))), Func(Of Context(Of D), Node))
            Dim F =
                Sub(c As DCollection, Value As ElementPackerState)
                    Dim k = 0
                    For Each v In c
                        Value.List.Add(Mapper(New Context(Of D) With {.Value = v, .SourceMappings = Value.SourceMappings, .TargetMappings = Value.TargetMappings}))
                        k += 1
                    Next
                End Sub
            Return F
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class NodeToStringRangeTranslator
        Implements IProjectorToProjectorRangeTranslator(Of Node, String)

        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, Node) Implements IProjectorToProjectorRangeTranslator(Of Node, String).TranslateProjectorToProjectorRange
            Dim FriendlyName = GetTypeFriendlyName(GetType(D).GetGenericArguments().Single())
            Return Function(v)
                       Dim c = DirectCast(v, IContext)
                       Dim s = Projector(v)
                       Dim x = Node.CreateStem(New Stem With {.Name = FriendlyName, .Children = New List(Of Node) From {Node.CreateLeaf(s)}})
                       If c.SourceMappings.ContainsKey(c.Value) Then
                           c.TargetMappings.Add(x, c.SourceMappings(c.Value))
                       End If
                       Return x
                   End Function
        End Function
    End Class

    Public Class ContextToStringDomainTranslator
        Implements IProjectorToProjectorDomainTranslator(Of NodeContext, String)

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of NodeContext, R) Implements IProjectorToProjectorDomainTranslator(Of NodeContext, String).TranslateProjectorToProjectorDomain
            Return Function(v)
                       If v.Value.OnEmpty Then Return Nothing
                       If v.Value.OnLeaf Then Throw New InvalidOperationException
                       Dim Element = v.Value.Stem.Children.Single()
                       If Not Element.OnLeaf Then Throw New InvalidOperationException
                       Return Projector(Element.Leaf)
                   End Function
        End Function
    End Class

    Public Class ContextProjectorToProjectorDomainTranslator
        Implements IProjectorToProjectorDomainTranslator(Of NodeContext, ElementUnpackerState)

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of ElementUnpackerState, R)) As Func(Of NodeContext, R) Implements IProjectorToProjectorDomainTranslator(Of NodeContext, ElementUnpackerState).TranslateProjectorToProjectorDomain
            Return Function(Element) As R
                       If Element.Value.OnEmpty Then Return Nothing
                       If Element.Value.OnLeaf Then Throw New InvalidOperationException
                       If Element.Value.OnStem Then
                           Dim l As New List(Of NodeContext)
                           Dim d As New Dictionary(Of String, NodeContext)(StringComparer.OrdinalIgnoreCase)
                           For Each e In Element.Value.Stem.Children
                               If e.OnEmpty Then Continue For
                               If Not e.OnStem Then Throw New InvalidOperationException
                               Dim LocalName = e.Stem.Name
                               Dim c = New NodeContext With {.Value = e, .SourcePositions = Element.SourcePositions, .TargetPositions = Element.TargetPositions}
                               l.Add(c)
                               If Not d.ContainsKey(LocalName) Then
                                   d.Add(LocalName, c)
                               End If
                           Next
                           Dim Value = Projector(New ElementUnpackerState With {.Parent = Element, .List = l, .Dict = d})
                           If GetType(R).IsClass Then
                               If Element.SourcePositions.ContainsKey(Element.Value) Then
                                   Element.TargetPositions.Add(Value, Element.SourcePositions(Element.Value))
                               End If
                           End If
                           Return Value
                       Else
                           Throw New InvalidOperationException
                       End If
                   End Function
        End Function
    End Class

    Public Class NodeAggregatorToProjectorRangeTranslator
        Implements IAggregatorToProjectorRangeTranslator(Of Node, ElementPackerState)

        Public Function TranslateAggregatorToProjectorRange(Of D)(ByVal Aggregator As Action(Of D, ElementPackerState)) As Func(Of D, Node) Implements IAggregatorToProjectorRangeTranslator(Of Node, ElementPackerState).TranslateAggregatorToProjectorRange
            Dim FriendlyName = GetTypeFriendlyName(GetType(D).GetGenericArguments().Single())
            Return Function(v)
                       Dim c = DirectCast(v, IContext)
                       Dim x As Node
                       Dim l As New List(Of Node)
                       If v IsNot Nothing Then
                           Dim s As New ElementPackerState With {.UseParent = False, .Parent = Nothing, .List = l, .SourceMappings = c.SourceMappings, .TargetMappings = c.TargetMappings}
                           Aggregator(v, s)
                           If s.UseParent Then
                               x = s.Parent
                               x.Stem.Name = FriendlyName
                           ElseIf l.Count = 0 Then
                               x = Node.CreateStem(New Stem With {.Name = FriendlyName, .Children = New List(Of Node) From {}})
                           Else
                               x = Node.CreateStem(New Stem With {.Name = FriendlyName, .Children = l})
                           End If
                       Else
                           x = Node.CreateStem(New Stem With {.Name = FriendlyName, .Children = Nothing})
                       End If
                       If c.SourceMappings.ContainsKey(c.Value) Then
                           c.TargetMappings.Add(x, c.SourceMappings(c.Value))
                       End If
                       Return x
                   End Function
        End Function
    End Class

    Public Class NodeProjectorToAggregatorRangeTranslator
        Implements IProjectorToAggregatorRangeTranslator(Of ElementPackerState, Node)

        Public Function TranslateProjectorToAggregatorRange(Of D)(ByVal Projector As Func(Of D, Node)) As Action(Of D, ElementPackerState) Implements IProjectorToAggregatorRangeTranslator(Of ElementPackerState, Node).TranslateProjectorToAggregatorRange
            Return Sub(v, s) s.List.Add(Projector(v))
        End Function
    End Class

    Public Class ContextDomainTranslatorProjectorResolver
        Implements IProjectorResolver

        Public Shared Function ContextUnpack(Of D)(ByVal Inner As Func(Of D, String)) As Func(Of Context(Of D), String)
            Return Function(ByVal c As Context(Of D)) Inner(c.Value)
        End Function

        Public Function TryResolveProjector(TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType.IsGenericType() AndAlso DomainType.GetGenericTypeDefinition() Is GetType(Context(Of )) AndAlso RangeType Is GetType(String) Then
                Dim InnerDomainType = DomainType.GetGenericArguments().Single()
                Dim Inner = InnerResolver.TryResolveProjector(New KeyValuePair(Of Type, Type)(InnerDomainType, RangeType))
                If Inner Is Nothing Then Return Nothing
                Dim DummyMethod = DirectCast(AddressOf ContextUnpack(Of DummyType), Func(Of Func(Of DummyType, String), Func(Of Context(Of DummyType), String)))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(InnerDomainType)
                Dim d = m.StaticDynamicInvoke(Of [Delegate], [Delegate])(Inner)
                Return d
            End If
            Return Nothing
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class ContextDomainTranslatorAggregatorResolver
        Implements IAggregatorResolver

        Public Shared Function ContextUnpack(Of D)(ByVal Inner As Action(Of D, ElementPackerState)) As Action(Of Context(Of D), ElementPackerState)
            Return Sub(ByVal c As Context(Of D), ByVal s As ElementPackerState) Inner(c.Value, s)
        End Function

        Public Function TryResolveAggregator(TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType.IsGenericType() AndAlso DomainType.GetGenericTypeDefinition() Is GetType(Context(Of )) AndAlso RangeType Is GetType(ElementPackerState) Then
                Dim InnerDomainType = DomainType.GetGenericArguments().Single()
                Dim Inner = InnerResolver.TryResolveAggregator(New KeyValuePair(Of Type, Type)(InnerDomainType, RangeType))
                If Inner Is Nothing Then Return Nothing
                Dim DummyMethod = DirectCast(AddressOf ContextUnpack(Of DummyType), Func(Of Action(Of DummyType, ElementPackerState), Action(Of Context(Of DummyType), ElementPackerState)))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(InnerDomainType)
                Dim d = m.StaticDynamicInvoke(Of [Delegate], [Delegate])(Inner)
                Return d
            End If
            Return Nothing
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class FieldProjectorResolver
        Implements IFieldProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Name As String) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(NodeContext), GetType(R))), Func(Of NodeContext, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim d = s.Dict
                    If Not d.ContainsKey(Name) Then
                        Dim i As New FileLocationInformation
                        If s.Parent.SourcePositions.ContainsKey(s.Parent) Then
                            Dim p = s.Parent.SourcePositions(s.Parent)
                            i.Path = p.Text.Path
                            If p.Range.OnSome Then
                                Dim Range = p.Range.Value
                                i.LineNumber = Range.Start.Row
                                i.ColumnNumber = Range.Start.Column
                            End If
                        End If
                        Throw New InvalidTextFormatException("FieldNameNotFound: {0}".Formats(Name), i)
                    End If
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
            Me.InnerResolver = Resolver
        End Sub
    End Class
    Public Class FieldAggregatorResolver
        Implements IFieldAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(Context(Of D)), GetType(Node))), Func(Of Context(Of D), Node))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(New Context(Of D) With {.Value = k, .SourceMappings = s.SourceMappings, .TargetMappings = s.TargetMappings})
                    e.Stem.Name = Name
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
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class AliasFieldProjectorResolver
        Implements IAliasFieldProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)() As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(NodeContext), GetType(R))), Func(Of NodeContext, R))
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
            Me.InnerResolver = Resolver
        End Sub
    End Class
    Public Class AliasFieldAggregatorResolver
        Implements IAliasFieldAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)() As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(Context(Of D)), GetType(Node))), Func(Of Context(Of D), Node))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(New Context(Of D) With {.Value = k, .SourceMappings = s.SourceMappings, .TargetMappings = s.TargetMappings})
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
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class TagProjectorResolver
        Implements ITagProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)() As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(String), GetType(R))), Func(Of String, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim TagValue = s.List.Single().Value.Stem.Name
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
            Me.InnerResolver = Resolver
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
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class TaggedUnionAlternativeProjectorResolver
        Implements ITaggedUnionAlternativeProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Name As String) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(NodeContext), GetType(R))), Func(Of NodeContext, R))
            Dim F =
                Function(s As ElementUnpackerState) As R
                    Dim d = s.Dict
                    If Not d.ContainsKey(Name) Then
                        Dim i As New FileLocationInformation
                        If s.Parent.SourcePositions.ContainsKey(s.Parent) Then
                            Dim p = s.Parent.SourcePositions(s.Parent)
                            i.Path = p.Text.Path
                            If p.Range.OnSome Then
                                Dim Range = p.Range.Value
                                i.LineNumber = Range.Start.Row
                                i.ColumnNumber = Range.Start.Column
                            End If
                        End If
                        Throw New InvalidTextFormatException("AlternativeNameNotFound: {0}".Formats(Name), i)
                    End If
                    Return Mapper(d(Name))
                End Function
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements ITaggedUnionAlternativeProjectorResolver(Of ElementUnpackerState).ResolveProjector
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
            Me.InnerResolver = Resolver
        End Sub
    End Class
    Public Class TaggedUnionAlternativeAggregatorResolver
        Implements ITaggedUnionAlternativeAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Name As String) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(Context(Of D)), GetType(Node))), Func(Of Context(Of D), Node))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(New Context(Of D) With {.Value = k, .SourceMappings = s.SourceMappings, .TargetMappings = s.TargetMappings})
                    e.Stem.Name = Name
                    s.List.Add(e)
                End Sub
            Return F
        End Function

        Private Dict As New Dictionary(Of Type, Func(Of String, [Delegate]))
        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements ITaggedUnionAlternativeAggregatorResolver(Of ElementPackerState).ResolveAggregator
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
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class TupleElementProjectorResolver
        Implements ITupleElementProjectorResolver(Of ElementUnpackerState)

        Private Function Resolve(Of R)(ByVal Index As Integer) As Func(Of ElementUnpackerState, R)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(NodeContext), GetType(R))), Func(Of NodeContext, R))
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
            Me.InnerResolver = Resolver
        End Sub
    End Class
    Public Class TupleElementAggregatorResolver
        Implements ITupleElementAggregatorResolver(Of ElementPackerState)

        Private Function Resolve(Of D)(ByVal Index As Integer) As Action(Of D, ElementPackerState)
            Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(Context(Of D)), GetType(Node))), Func(Of Context(Of D), Node))
            Dim F =
                Sub(k As D, s As ElementPackerState)
                    Dim e = Mapper(New Context(Of D) With {.Value = k, .SourceMappings = s.SourceMappings, .TargetMappings = s.TargetMappings})
                    s.List.Add(e)
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
            Me.InnerResolver = Resolver
        End Sub
    End Class

    Public Class ByteArrayTranslator
        Implements IProjectorToProjectorRangeTranslator(Of Byte(), String) 'Reader
        Implements IProjectorToProjectorDomainTranslator(Of Byte(), String) 'Writer

        Private Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, Byte()) Implements IProjectorToProjectorRangeTranslator(Of Byte(), String).TranslateProjectorToProjectorRange
            Return Function(k)
                       If k Is Nothing Then Return Nothing
                       Dim Trimmed = Projector(k).Trim(" \t\r\n".Descape.ToCharArray)
                       If Trimmed = "" Then Return New Byte() {}
                       Return Regex.Split(Trimmed, "( |\t|\r|\n)+", RegexOptions.ExplicitCapture).Select(Function(s) Byte.Parse(s, Globalization.NumberStyles.HexNumber)).ToArray()
                   End Function
        End Function

        Private Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of Byte(), R) Implements IProjectorToProjectorDomainTranslator(Of Byte(), String).TranslateProjectorToProjectorDomain
            Return Function(ba)
                       If ba Is Nothing Then Return Nothing
                       Return Projector(String.Join(" ", (ba.Select(Function(b) b.ToString("X2")).ToArray)))
                   End Function
        End Function
    End Class

    Public Class ByteListTranslator
        Implements IProjectorToProjectorRangeTranslator(Of List(Of Byte), String) 'Reader
        Implements IProjectorToProjectorDomainTranslator(Of List(Of Byte), String) 'Writer

        Private Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, List(Of Byte)) Implements IProjectorToProjectorRangeTranslator(Of List(Of Byte), String).TranslateProjectorToProjectorRange
            Return Function(k)
                       Dim Trimmed = Projector(k).Trim(" \t\r\n".Descape.ToCharArray)
                       If Trimmed = "" Then Return New List(Of Byte)()
                       Return Regex.Split(Trimmed, "( |\t|\r|\n)+", RegexOptions.ExplicitCapture).Select(Function(s) Byte.Parse(s, Globalization.NumberStyles.HexNumber)).ToList()
                   End Function
        End Function

        Private Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of List(Of Byte), R) Implements IProjectorToProjectorDomainTranslator(Of List(Of Byte), String).TranslateProjectorToProjectorDomain
            Return Function(ba) Projector(String.Join(" ", (ba.Select(Function(b) b.ToString("X2")).ToArray)))
        End Function
    End Class

    Public Class DebugReaderResolver
        Implements IMapperResolver

        Private InnerResolver As IMapperResolver
        Public Sub New(ByVal InnerResolver As IMapperResolver)
            Me.InnerResolver = InnerResolver
        End Sub

        Private CurrentReadingNodeValue As Node
        Private Sub SetCurrentNode(ByVal c As NodeContext)
            CurrentReadingNodeValue = c.Value
        End Sub
        Public ReadOnly Property CurrentReadingNode As Node
            Get
                Return CurrentReadingNodeValue
            End Get
        End Property

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim m = InnerResolver.TryResolveProjector(TypePair)
            If TypePair.Key IsNot GetType(NodeContext) Then Return m
            If m Is Nothing Then Return Nothing

            Dim Parameters = m.GetParameters().Select(Function(p) Expression.Parameter(p.Type, p.Name)).ToArray()
            Dim DebugDelegate = DirectCast(DirectCast(AddressOf Me.SetCurrentNode, Action(Of NodeContext)), [Delegate])
            Dim DebugCall = CreatePair(DebugDelegate, New Expression() {Parameters.First})
            Dim OriginalCall = CreatePair(m, Parameters.Select(Function(p) DirectCast(p, Expression)).ToArray())
            Dim Context = CreateDelegateExpressionContext({DebugCall, OriginalCall})
            Dim FunctionLambda = Expression.Lambda(m.GetType(), Expression.Block(Context.DelegateExpressions), Parameters)

            Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
        End Function

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Dim m = InnerResolver.TryResolveAggregator(TypePair)
            If TypePair.Key IsNot GetType(NodeContext) Then Return m
            If m Is Nothing Then Return Nothing

            Dim Parameters = m.GetParameters().Select(Function(p) Expression.Parameter(p.Type, p.Name)).ToArray()
            Dim DebugDelegate = DirectCast(DirectCast(AddressOf Me.SetCurrentNode, Action(Of NodeContext)), [Delegate])
            Dim DebugCall = CreatePair(DebugDelegate, New Expression() {Parameters.First})
            Dim OriginalCall = CreatePair(m, Parameters.Select(Function(p) DirectCast(p, Expression)).ToArray())
            Dim Context = CreateDelegateExpressionContext({DebugCall, OriginalCall})
            Dim FunctionLambda = Expression.Lambda(m.GetType(), Expression.Block(Context.DelegateExpressions), Parameters)

            Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
        End Function
    End Class
End Namespace
