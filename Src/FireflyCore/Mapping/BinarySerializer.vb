﻿'==========================================================================
'
'  File:        BinarySerializer.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 二进制序列化类
'  Version:     2010.11.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions

Namespace Mapping
    ''' <remarks>
    ''' 对于非简单类型，应提供自定义序列化器
    ''' 简单类型 ::= 简单类型
    '''           | Byte(UInt8) | UInt16 | UInt32 | UInt64 | Int8(SByte) | Int16 | Int32 | Int64 | Float32(Single) | Float64(Double)
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
    ''' </remarks>
    Public Class BinarySerializer
        Private PrimRes As PrimitiveResolver
        Private ReaderMapper As ObjectMapper
        Private WriterMapper As ObjectMapper
        Private CounterMapper As ObjectMapper
        Private ReaderTranslatorResolver As TranslatorResolver
        Private WriterTranslatorResolver As TranslatorResolver
        Private CounterTranslatorResolver As TranslatorResolver
        Private ReaderResolverSet As AlternativeResolver
        Private WriterResolverSet As AlternativeResolver
        Private CounterResolverSet As AlternativeResolver

        Public ReadOnly Property ReaderResolver As AlternativeResolver
            Get
                Return ReaderResolverSet
            End Get
        End Property
        Public ReadOnly Property WriterResolver As AlternativeResolver
            Get
                Return WriterResolverSet
            End Get
        End Property
        Public ReadOnly Property CounterResolver As AlternativeResolver
            Get
                Return CounterResolverSet
            End Get
        End Property

        Public Sub New()
            PrimRes = New PrimitiveResolver
            ReaderResolverSet = New AlternativeResolver
            Dim ReaderCache As New CachedResolver(ReaderResolverSet)
            ReaderTranslatorResolver = New TranslatorResolver(ReaderCache)
            ReaderResolverSet.ProjectorResolvers.AddRange(New List(Of IObjectProjectorResolver) From {
                PrimRes,
                New EnumUnpacker(Of StreamEx)(ReaderCache),
                New CollectionUnpacker(Of StreamEx)(New GenericListProjectorResolver(Of StreamEx)(ReaderCache)),
                New RecordUnpacker(Of StreamEx)(ReaderCache),
                ReaderTranslatorResolver
            })
            WriterResolverSet = New AlternativeResolver
            Dim WriterCache As New CachedResolver(WriterResolverSet)
            WriterTranslatorResolver = New TranslatorResolver(WriterCache)
            WriterResolverSet.AggregatorResolvers.AddRange(New List(Of IObjectAggregatorResolver) From {
                PrimRes,
                New EnumPacker(Of StreamEx)(WriterCache),
                New CollectionPacker(Of StreamEx)(New GenericListAggregatorResolver(Of StreamEx)(WriterCache)),
                New RecordPacker(Of StreamEx)(WriterCache),
                WriterTranslatorResolver
            })
            CounterResolverSet = New AlternativeResolver
            Dim CounterCache As New CachedResolver(CounterResolverSet)
            CounterTranslatorResolver = New TranslatorResolver(CounterCache)
            CounterResolverSet.ProjectorResolvers.AddRange(New List(Of IObjectProjectorResolver) From {
                PrimRes,
                CounterTranslatorResolver
            })
            CounterResolverSet.AggregatorResolvers.AddRange(New List(Of IObjectAggregatorResolver) From {
                New EnumPacker(Of CounterState)(CounterCache),
                New CollectionPacker(Of CounterState)(New GenericListAggregatorResolver(Of CounterState)(CounterCache)),
                New RecordPacker(Of CounterState)(CounterCache),
                CounterTranslatorResolver
            })
            CounterTranslatorResolver.PutAggregatorToProjectorTranslatorRange(New CounterStateToIntRangeTranslator)
            CounterTranslatorResolver.PutProjectorToAggregatorTranslatorRange(New IntToCounterStateRangeTranslator)
            ReaderMapper = New ObjectMapper(ReaderCache)
            WriterMapper = New ObjectMapper(WriterCache)
            CounterMapper = New ObjectMapper(CounterCache)
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
            PrimRes.PutReader(Of T)(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
            PrimRes.PutWriter(Of T)(Writer)
        End Sub
        Public Sub PutCounter(Of T)(ByVal Counter As Func(Of T, Integer))
            PrimRes.PutCounter(Of T)(Counter)
        End Sub
        Public Sub PutReaderTranslator(Of R, M)(ByVal Translator As IProjectorToProjectorRangeTranslator(Of R, M))
            ReaderTranslatorResolver.PutProjectorToProjectorTranslatorRange(Translator)
        End Sub
        Public Sub PutWriterTranslator(Of D, M)(ByVal Translator As IAggregatorToAggregatorDomainTranslator(Of D, M))
            WriterTranslatorResolver.PutAggregatorToAggregatorTranslatorDomain(Translator)
        End Sub
        Public Sub PutCounterTranslator(Of D, M)(ByVal Translator As IProjectorToProjectorDomainTranslator(Of D, M))
            CounterTranslatorResolver.PutProjectorToProjectorTranslatorDomain(Translator)
        End Sub

        Public Function GetReader(Of T)() As Func(Of StreamEx, T)
            Return ReaderMapper.GetProjector(Of StreamEx, T)()
        End Function
        Public Function GetWriter(Of T)() As Action(Of T, StreamEx)
            Return WriterMapper.GetAggregator(Of T, StreamEx)()
        End Function
        Public Function GetCounter(Of T)() As Func(Of T, Integer)
            Return CounterMapper.GetProjector(Of T, Integer)()
        End Function

        Public Function Read(Of T)(ByVal s As StreamEx) As T
            Return GetReader(Of T)()(s)
        End Function
        Public Sub Write(Of T)(ByVal Value As T, ByVal s As StreamEx)
            GetWriter(Of T)()(Value, s)
        End Sub
        Public Sub Write(Of T)(ByVal s As StreamEx, ByVal Value As T)
            Write(Of T)(Value, s)
        End Sub
        Public Function Count(Of T)(ByVal Value As T) As Integer
            Return GetCounter(Of T)()(Value)
        End Function

        Public Class CounterState
            Public Number As Integer
        End Class

        Public Class PrimitiveResolver
            Implements IObjectMapperResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType Is GetType(StreamEx) Then Return TryResolveReader(RangeType)
                If RangeType Is GetType(Integer) Then Return TryResolveCounter(DomainType)
                Return Nothing
            End Function
            Private Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(StreamEx) Then Return TryResolveWriter(DomainType)
                Return Nothing
            End Function

            Public Function TryResolveReader(ByVal RangeType As Type) As [Delegate]
                If Readers.ContainsKey(RangeType) Then Return Readers(RangeType)
                Return Nothing
            End Function

            Public Function TryResolveWriter(ByVal DomainType As Type) As [Delegate]
                If Writers.ContainsKey(DomainType) Then Return Writers(DomainType)
                Return Nothing
            End Function

            Public Function TryResolveCounter(ByVal DomainType As Type) As [Delegate]
                If Counters.ContainsKey(DomainType) Then Return Counters(DomainType)
                Return Nothing
            End Function

            Public Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
                Readers.Add(GetType(T), Reader)
            End Sub
            Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
                Writers.Add(GetType(T), Writer)
            End Sub
            Public Sub PutCounter(Of T)(ByVal Counter As Func(Of T, Integer))
                Counters.Add(GetType(T), Counter)
            End Sub

            Private Readers As New Dictionary(Of Type, [Delegate])
            Private Writers As New Dictionary(Of Type, [Delegate])
            Private Counters As New Dictionary(Of Type, [Delegate])

            Public Sub New()
                PutReader(Function(s As StreamEx) s.ReadByte)
                PutReader(Function(s As StreamEx) s.ReadUInt16)
                PutReader(Function(s As StreamEx) s.ReadUInt32)
                PutReader(Function(s As StreamEx) s.ReadUInt64)
                PutReader(Function(s As StreamEx) s.ReadInt8)
                PutReader(Function(s As StreamEx) s.ReadInt16)
                PutReader(Function(s As StreamEx) s.ReadInt32)
                PutReader(Function(s As StreamEx) s.ReadInt64)
                PutReader(Function(s As StreamEx) s.ReadFloat32)
                PutReader(Function(s As StreamEx) s.ReadFloat64)

                PutWriter(Sub(b As Byte, s As StreamEx) s.WriteByte(b))
                PutWriter(Sub(i As UInt16, s As StreamEx) s.WriteUInt16(i))
                PutWriter(Sub(i As UInt32, s As StreamEx) s.WriteUInt32(i))
                PutWriter(Sub(i As UInt64, s As StreamEx) s.WriteUInt64(i))
                PutWriter(Sub(i As SByte, s As StreamEx) s.WriteInt8(i))
                PutWriter(Sub(i As Int16, s As StreamEx) s.WriteInt16(i))
                PutWriter(Sub(i As Int32, s As StreamEx) s.WriteInt32(i))
                PutWriter(Sub(i As Int64, s As StreamEx) s.WriteInt64(i))
                PutWriter(Sub(f As Single, s As StreamEx) s.WriteFloat32(f))
                PutWriter(Sub(f As Double, s As StreamEx) s.WriteFloat64(f))

                PutCounter(Function(i As Byte) 1)
                PutCounter(Function(i As UInt16) 2)
                PutCounter(Function(i As UInt32) 4)
                PutCounter(Function(i As UInt64) 8)
                PutCounter(Function(i As SByte) 1)
                PutCounter(Function(i As Int16) 2)
                PutCounter(Function(i As Int32) 4)
                PutCounter(Function(i As Int64) 8)
                PutCounter(Function(f As Single) 4)
                PutCounter(Function(f As Double) 8)
            End Sub
        End Class

        Public Class EnumUnpacker(Of D)
            Implements IObjectProjectorResolver

            Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType IsNot GetType(D) Then Return Nothing
                If RangeType.IsEnum Then
                    Dim UnderlyingType = RangeType.GetEnumUnderlyingType
                    Dim Mapper = AbsResolver.ResolveProjector(CreatePair(DomainType, UnderlyingType))
                    Dim dParam = Expression.Variable(GetType(D), "Key")

                    Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {dParam}))
                    Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                    Dim FunctionBody = Expression.ConvertChecked(Context.DelegateExpressions.Single, RangeType)
                    Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
                Return Nothing
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
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
                    Dim Mapper = AbsResolver.ResolveAggregator(CreatePair(UnderlyingType, RangeType))
                    Dim dParam = Expression.Variable(DomainType, "Key")
                    Dim rParam = Expression.Variable(GetType(R), "Value")

                    Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {Expression.ConvertChecked(dParam, UnderlyingType), rParam}))
                    Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                    Dim FunctionLambda = Expression.Lambda(Context.DelegateExpressions.Single, New ParameterExpression() {dParam, rParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
                Return Nothing
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Public Class GenericListProjectorResolver(Of D)
            Implements IGenericListProjectorResolver(Of D)

            Public Function ResolveProjector(Of R, RList As {New, ICollection(Of R)})() As Func(Of D, RList) Implements IGenericListProjectorResolver(Of D).ResolveProjector
                Dim Mapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(R))), Func(Of D, R))
                Dim IntMapper = DirectCast(AbsResolver.ResolveProjector(CreatePair(GetType(D), GetType(Integer))), Func(Of D, Integer))
                Dim F =
                    Function(Key As D) As RList
                        Dim NumElement = IntMapper(Key)
                        Dim List = New RList()
                        For n = 0 To NumElement - 1
                            List.Add(Mapper(Key))
                        Next
                        Return List
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
                Dim IntMapper = DirectCast(AbsResolver.ResolveAggregator(CreatePair(GetType(Integer), GetType(R))), Action(Of Integer, R))
                Dim F =
                    Sub(List As DList, Value As R)
                        Dim NumElement = List.Count
                        IntMapper(NumElement, Value)
                        For Each v In List
                            Mapper(v, Value)
                        Next
                    End Sub
                Return F
            End Function

            Private AbsResolver As AbsoluteResolver
            Public Sub New(ByVal Resolver As IObjectMapperResolver)
                Me.AbsResolver = New AbsoluteResolver(New NoncircularResolver(Resolver))
            End Sub
        End Class

        Public Class CounterStateToIntRangeTranslator
            Implements IAggregatorToProjectorRangeTranslator(Of Integer, CounterState)
            Public Function TranslateAggregatorToProjector(Of D)(ByVal Aggregator As Action(Of D, CounterState)) As Func(Of D, Integer) Implements IAggregatorToProjectorRangeTranslator(Of Integer, CounterState).TranslateAggregatorToProjectorRange
                Return Function(Key)
                           Dim c As New CounterState
                           Aggregator(Key, c)
                           Return c.Number
                       End Function
            End Function
        End Class
        Public Class IntToCounterStateRangeTranslator
            Implements IProjectorToAggregatorRangeTranslator(Of CounterState, Integer)
            Public Function TranslateProjectorToAggregator(Of D)(ByVal Projector As Func(Of D, Integer)) As Action(Of D, CounterState) Implements IProjectorToAggregatorRangeTranslator(Of CounterState, Integer).TranslateProjectorToAggregatorRange
                Return Sub(Key, c)
                           c.Number += Projector(Key)
                       End Sub
            End Function
        End Class
    End Class
End Namespace
