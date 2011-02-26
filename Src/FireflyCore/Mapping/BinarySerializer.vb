'==========================================================================
'
'  File:        BinarySerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 二进制序列化类
'  Version:     2011.02.27.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports Firefly
Imports Firefly.Streaming

Namespace Mapping
    Public Class BinarySerializer
        Inherits BinarySerializer(Of IReadableStream, IWritableStream)
    End Class

    ''' <remarks>
    ''' 对于非简单类型，应提供自定义序列化器
    ''' 简单类型 ::= 简单类型
    '''           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
    '''           | Boolean
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
    ''' 此外，对象树中不应有空引用，否则应提供自定义序列化器
    ''' </remarks>
    Public Class BinarySerializer(Of TReadStream As IReadableStream, TWriteStream As IWritableStream)
        Private PrimitiveResolver As PrimitiveResolver
        Private ReaderCache As IObjectMapperResolver
        Private WriterCache As IObjectMapperResolver
        Private CounterCache As IObjectMapperResolver
        Private ReaderProjectorResolverList As LinkedList(Of IObjectProjectorResolver)
        Private WriterAggregatorResolverList As LinkedList(Of IObjectAggregatorResolver)
        Private CounterProjectorResolverList As LinkedList(Of IObjectProjectorResolver)
        Private CounterAggregatorResolverList As LinkedList(Of IObjectAggregatorResolver)

        Public ReadOnly Property ReaderResolver As IObjectMapperResolver
            Get
                Return ReaderCache
            End Get
        End Property
        Public ReadOnly Property WriterResolver As IObjectMapperResolver
            Get
                Return WriterCache
            End Get
        End Property
        Public ReadOnly Property CounterResolver As IObjectMapperResolver
            Get
                Return CounterCache
            End Get
        End Property

        Public Sub New()
            PrimitiveResolver = New PrimitiveResolver

            PutReader(Function(s As TReadStream) s.ReadByte)
            PutReader(Function(s As TReadStream) s.ReadUInt16)
            PutReader(Function(s As TReadStream) s.ReadUInt32)
            PutReader(Function(s As TReadStream) s.ReadUInt64)
            PutReader(Function(s As TReadStream) s.ReadInt8)
            PutReader(Function(s As TReadStream) s.ReadInt16)
            PutReader(Function(s As TReadStream) s.ReadInt32)
            PutReader(Function(s As TReadStream) s.ReadInt64)
            PutReader(Function(s As TReadStream) s.ReadFloat32)
            PutReader(Function(s As TReadStream) s.ReadFloat64)
            PutReader(Function(s As TReadStream) s.ReadByte <> 0)

            PutWriter(Sub(b As Byte, s As TWriteStream) s.WriteByte(b))
            PutWriter(Sub(i As UInt16, s As TWriteStream) s.WriteUInt16(i))
            PutWriter(Sub(i As UInt32, s As TWriteStream) s.WriteUInt32(i))
            PutWriter(Sub(i As UInt64, s As TWriteStream) s.WriteUInt64(i))
            PutWriter(Sub(i As SByte, s As TWriteStream) s.WriteInt8(i))
            PutWriter(Sub(i As Int16, s As TWriteStream) s.WriteInt16(i))
            PutWriter(Sub(i As Int32, s As TWriteStream) s.WriteInt32(i))
            PutWriter(Sub(i As Int64, s As TWriteStream) s.WriteInt64(i))
            PutWriter(Sub(f As Single, s As TWriteStream) s.WriteFloat32(f))
            PutWriter(Sub(f As Double, s As TWriteStream) s.WriteFloat64(f))
            PutWriter(Sub(b As Boolean, s As TWriteStream) s.WriteByte(CByte(b)))

            PutCounter(Function(b As Byte) 1)
            PutCounter(Function(i As UInt16) 2)
            PutCounter(Function(i As UInt32) 4)
            PutCounter(Function(i As UInt64) 8)
            PutCounter(Function(i As SByte) 1)
            PutCounter(Function(i As Int16) 2)
            PutCounter(Function(i As Int32) 4)
            PutCounter(Function(i As Int64) 8)
            PutCounter(Function(f As Single) 4)
            PutCounter(Function(f As Double) 8)
            PutCounter(Function(b As Boolean) 1)

            Dim ReaderReference As New ReferenceMapperResolver
            ReaderCache = ReaderReference
            ReaderProjectorResolverList = New LinkedList(Of IObjectProjectorResolver)({
                PrimitiveResolver,
                New EnumUnpacker(Of TReadStream)(ReaderCache),
                New CollectionUnpackerTemplate(Of TReadStream)(New GenericCollectionProjectorResolver(Of TReadStream)(ReaderCache)),
                New RecordUnpackerTemplate(Of TReadStream)(New FieldOrPropertyProjectorResolver(Of TReadStream)(ReaderCache))
            })
            ReaderReference.Inner = CreateMapper(ReaderProjectorResolverList.Concatenated.AsCached, EmptyAggregatorResolver)

            Dim WriterReference As New ReferenceMapperResolver
            WriterCache = WriterReference
            WriterAggregatorResolverList = New LinkedList(Of IObjectAggregatorResolver)({
                PrimitiveResolver,
                New EnumPacker(Of TWriteStream)(WriterCache),
                New CollectionPackerTemplate(Of TWriteStream)(New GenericCollectionAggregatorResolver(Of TWriteStream)(WriterCache)),
                New RecordPackerTemplate(Of TWriteStream)(New FieldOrPropertyAggregatorResolver(Of TWriteStream)(WriterCache))
            })
            WriterReference.Inner = CreateMapper(EmptyProjectorResolver, WriterAggregatorResolverList.Concatenated.AsCached)

            Dim CounterReference As New ReferenceMapperResolver
            CounterCache = CounterReference
            CounterProjectorResolverList = New LinkedList(Of IObjectProjectorResolver)({
                PrimitiveResolver,
                TranslatorResolver.Create(CounterCache, New CounterStateToIntRangeTranslator)
            })
            CounterAggregatorResolverList = New LinkedList(Of IObjectAggregatorResolver)({
                New EnumPacker(Of CounterState)(CounterCache),
                New CollectionPackerTemplate(Of CounterState)(New GenericCollectionAggregatorResolver(Of CounterState)(CounterCache)),
                New RecordPackerTemplate(Of CounterState)(New FieldOrPropertyAggregatorResolver(Of CounterState)(CounterCache)),
                TranslatorResolver.Create(CounterCache, New IntToCounterStateRangeTranslator)
            })
            CounterReference.Inner = CreateMapper(CounterProjectorResolverList.Concatenated.AsCached, CounterAggregatorResolverList.Concatenated.AsCached)
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of TReadStream, T))
            PrimitiveResolver.PutProjector(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, TWriteStream))
            PrimitiveResolver.PutAggregator(Writer)
        End Sub
        Public Sub PutCounter(Of T)(ByVal Counter As Func(Of T, Integer))
            PrimitiveResolver.PutProjector(Counter)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderProjectorResolverList.AddFirst(TranslatorResolver.Create(ReaderCache, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IAggregatorToAggregatorDomainTranslator(Of D, M))
            WriterAggregatorResolverList.AddFirst(TranslatorResolver.Create(WriterCache, Translator))
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            Dim t = New PP2AADomainTranslatorTranslator(Of D, M)(Translator)
            WriterAggregatorResolverList.AddFirst(TranslatorResolver.Create(WriterCache, t))
        End Sub
        Public Sub PutCounterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            CounterProjectorResolverList.AddFirst(TranslatorResolver.Create(CounterCache, Translator))
        End Sub

        Public Function GetReader(Of T)() As Func(Of TReadStream, T)
            Return ReaderCache.ResolveProjector(Of TReadStream, T)()
        End Function
        Public Function GetWriter(Of T)() As Action(Of T, TWriteStream)
            Return WriterCache.ResolveAggregator(Of T, TWriteStream)()
        End Function
        Public Function GetCounter(Of T)() As Func(Of T, Integer)
            Return CounterCache.ResolveProjector(Of T, Integer)()
        End Function

        Public Function Read(Of T)(ByVal s As TReadStream) As T
            Dim m = GetReader(Of T)()
            Return m(s)
        End Function
        Public Sub Write(Of T)(ByVal Value As T, ByVal s As TWriteStream)
            Dim m = GetWriter(Of T)()
            m(Value, s)
        End Sub
        Public Sub Write(Of T)(ByVal s As TWriteStream, ByVal Value As T)
            Write(Of T)(Value, s)
        End Sub
        Public Function Count(Of T)(ByVal Value As T) As Integer
            Dim m = GetCounter(Of T)()
            Return m(Value)
        End Function

        Private Class CounterState
            Public Number As Integer
        End Class

        Public Class EnumUnpacker(Of D)
            Implements IObjectProjectorResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType IsNot GetType(D) Then Return Nothing
                If RangeType.IsEnum Then
                    Dim UnderlyingType = RangeType.GetEnumUnderlyingType
                    Dim Mapper = InnerResolver.ResolveProjector(CreatePair(DomainType, UnderlyingType))
                    Dim dParam = Expression.Parameter(GetType(D), "Key")

                    Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {dParam}))
                    Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                    Dim FunctionBody = Expression.ConvertChecked(Context.DelegateExpressions.Single, RangeType)
                    Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
                Return Nothing
            End Function

            Private InnerResolver As IObjectProjectorResolver
            Public Sub New(ByVal Resolver As IObjectProjectorResolver)
                Me.InnerResolver = Resolver.AsNoncircular
            End Sub
        End Class

        Public Class EnumPacker(Of R)
            Implements IObjectAggregatorResolver

            Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType IsNot GetType(R) Then Return Nothing
                If DomainType.IsEnum Then
                    Dim UnderlyingType = DomainType.GetEnumUnderlyingType
                    Dim Mapper = InnerResolver.ResolveAggregator(CreatePair(UnderlyingType, RangeType))
                    Dim dParam = Expression.Parameter(DomainType, "Key")
                    Dim rParam = Expression.Parameter(GetType(R), "Value")

                    Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {Expression.ConvertChecked(dParam, UnderlyingType), rParam}))
                    Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                    Dim FunctionLambda = Expression.Lambda(Context.DelegateExpressions.Single, New ParameterExpression() {dParam, rParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
                Return Nothing
            End Function

            Private InnerResolver As IObjectAggregatorResolver
            Public Sub New(ByVal Resolver As IObjectAggregatorResolver)
                Me.InnerResolver = Resolver.AsNoncircular
            End Sub
        End Class

        Public Class GenericCollectionProjectorResolver(Of D)
            Implements IGenericCollectionProjectorResolver(Of D)

            Public Function ResolveProjector(Of R, RCollection As {New, ICollection(Of R)})() As Func(Of D, RCollection) Implements IGenericCollectionProjectorResolver(Of D).ResolveProjector
                Dim Mapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(R))), Func(Of D, R))
                Dim IntMapper = DirectCast(InnerResolver.ResolveProjector(CreatePair(GetType(D), GetType(Integer))), Func(Of D, Integer))
                Dim F =
                    Function(Key As D) As RCollection
                        Dim NumElement = IntMapper(Key)
                        Dim c = New RCollection()
                        For n = 0 To NumElement - 1
                            c.Add(Mapper(Key))
                        Next
                        Return c
                    End Function
                Return F
            End Function

            Private InnerResolver As IObjectProjectorResolver
            Public Sub New(ByVal Resolver As IObjectProjectorResolver)
                Me.InnerResolver = Resolver.AsNoncircular
            End Sub
        End Class

        Public Class GenericCollectionAggregatorResolver(Of R)
            Implements IGenericCollectionAggregatorResolver(Of R)

            Public Function ResolveAggregator(Of D, DCollection As ICollection(Of D))() As Action(Of DCollection, R) Implements IGenericCollectionAggregatorResolver(Of R).ResolveAggregator
                Dim Mapper = DirectCast(InnerResolver.ResolveAggregator(CreatePair(GetType(D), GetType(R))), Action(Of D, R))
                Dim IntMapper = DirectCast(InnerResolver.ResolveAggregator(CreatePair(GetType(Integer), GetType(R))), Action(Of Integer, R))
                Dim F =
                    Sub(c As DCollection, Value As R)
                        Dim NumElement = c.Count
                        IntMapper(NumElement, Value)
                        For Each v In c
                            Mapper(v, Value)
                        Next
                    End Sub
                Return F
            End Function

            Private InnerResolver As IObjectAggregatorResolver
            Public Sub New(ByVal Resolver As IObjectAggregatorResolver)
                Me.InnerResolver = Resolver.AsNoncircular
            End Sub
        End Class

        Public Class FieldOrPropertyProjectorResolver(Of D)
            Implements IFieldOrPropertyProjectorResolver(Of D)

            Public Function ResolveProjector(ByVal Info As FieldOrPropertyInfo) As [Delegate] Implements IFieldOrPropertyProjectorResolver(Of D).ResolveProjector
                Return InnerResolver.ResolveProjector(CreatePair(GetType(D), Info.Type))
            End Function

            Private InnerResolver As IObjectProjectorResolver
            Public Sub New(ByVal Resolver As IObjectProjectorResolver)
                Me.InnerResolver = Resolver.AsNoncircular.AsCached
            End Sub
        End Class
        Public Class FieldOrPropertyAggregatorResolver(Of R)
            Implements IFieldOrPropertyAggregatorResolver(Of R)

            Public Function ResolveAggregator(ByVal Info As FieldOrPropertyInfo) As [Delegate] Implements IFieldOrPropertyAggregatorResolver(Of R).ResolveAggregator
                Return InnerResolver.ResolveAggregator(CreatePair(Info.Type, GetType(R)))
            End Function

            Private InnerResolver As IObjectAggregatorResolver
            Public Sub New(ByVal Resolver As IObjectAggregatorResolver)
                Me.InnerResolver = Resolver.AsNoncircular.AsCached
            End Sub
        End Class

        Private Class CounterStateToIntRangeTranslator
            Implements IAggregatorToProjectorRangeTranslator(Of Integer, CounterState)
            Public Function TranslateAggregatorToProjector(Of D)(ByVal Aggregator As Action(Of D, CounterState)) As Func(Of D, Integer) Implements IAggregatorToProjectorRangeTranslator(Of Integer, CounterState).TranslateAggregatorToProjectorRange
                Return Function(Key)
                           Dim c As New CounterState
                           Aggregator(Key, c)
                           Return c.Number
                       End Function
            End Function
        End Class
        Private Class IntToCounterStateRangeTranslator
            Implements IProjectorToAggregatorRangeTranslator(Of CounterState, Integer)
            Public Function TranslateProjectorToAggregator(Of D)(ByVal Projector As Func(Of D, Integer)) As Action(Of D, CounterState) Implements IProjectorToAggregatorRangeTranslator(Of CounterState, Integer).TranslateProjectorToAggregatorRange
                Return Sub(Key, c)
                           c.Number += Projector(Key)
                       End Sub
            End Function
        End Class

        'AA(D, M)(R): (M aggr R) -> (D aggr R) = (D proj M) @ (M aggr R)
        'PP(D, M)(M): (M proj M) -> (D proj M) = (D proj M) @ (M proj M)
        'PP2AA(D, M)(R): (M aggr R) -> (D aggr R) = PP(D, M)(M)(M -> M: m |-> m) @ AA(D, M)(R) = (D proj M) @ (M -> M: m |-> m) @ (M aggr R) = (D proj M) @ (M aggr R)
        Private Class PP2AADomainTranslatorTranslator(Of D, M)
            Implements IAggregatorToAggregatorDomainTranslator(Of D, M)

            Public Function TranslateAggregatorToAggregatorDomain(Of R)(ByVal Aggregator As Action(Of M, R)) As Action(Of D, R) Implements IAggregatorToAggregatorDomainTranslator(Of D, M).TranslateAggregatorToAggregatorDomain
                Dim Identity = Function(k As M) k
                Return Sub(k As D, v As R) Aggregator(Inner.TranslateProjectorToProjectorDomain(Of M)(Identity)(k), v)
            End Function

            Private Inner As IProjectorToProjectorDomainTranslator(Of D, M)
            Public Sub New(ByVal Inner As IProjectorToProjectorDomainTranslator(Of D, M))
                Me.Inner = Inner
            End Sub
        End Class
    End Class
End Namespace
