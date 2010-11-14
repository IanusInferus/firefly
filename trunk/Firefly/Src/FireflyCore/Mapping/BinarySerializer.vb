'==========================================================================
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
        Private ReaderMapperValue As ObjectMapper
        Public ReadOnly Property ReaderMapper As ObjectMapper
            Get
                Return ReaderMapperValue
            End Get
        End Property
        Private WriterMapperValue As ObjectMapper
        Public ReadOnly Property WriterMapper As ObjectMapper
            Get
                Return WriterMapperValue
            End Get
        End Property
        Private CounterMapperValue As ObjectMapper
        Public ReadOnly Property CounterMapper As ObjectMapper
            Get
                Return CounterMapperValue
            End Get
        End Property

        Public Sub New()
            ReaderMapperValue = New ObjectMapper
            ReaderMapperValue.ProjectorResolvers.AddRange(New List(Of IObjectProjectorResolver) From {
                New PrimitiveMapperResolver(),
                New EnumUnpacker(Of StreamEx)(ReaderMapperValue),
                New CollectionUnpacker(Of StreamEx)(New GenericListProjectorResolver(Of StreamEx)(ReaderMapperValue)),
                New RecordUnpacker(Of StreamEx)(ReaderMapperValue)
            })
            WriterMapperValue = New ObjectMapper
            WriterMapperValue.AggregatorResolvers.AddRange(New List(Of IObjectAggregatorResolver) From {
                New PrimitiveMapperResolver(),
                New EnumPacker(Of StreamEx)(WriterMapperValue),
                New CollectionPacker(Of StreamEx)(New GenericListAggregatorResolver(Of StreamEx)(WriterMapperValue)),
                New RecordPacker(Of StreamEx)(WriterMapperValue)
            })
            CounterMapperValue = New ObjectMapper
            CounterMapperValue.AggregatorResolvers.AddRange(New List(Of IObjectAggregatorResolver) From {
                New PrimitiveMapperResolver(),
                New EnumPacker(Of CounterState)(CounterMapperValue),
                New CollectionPacker(Of CounterState)(New GenericListAggregatorResolver(Of CounterState)(CounterMapperValue)),
                New RecordPacker(Of CounterState)(CounterMapperValue)
            })
        End Sub

        Public Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
            ReaderMapperValue.PutProjector(Of StreamEx, T)(Reader)
        End Sub
        Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
            WriterMapperValue.PutAggregator(Of T, StreamEx)(Writer)
        End Sub
        Public Sub PutCounter(Of T)(ByVal Counter As Action(Of T, CounterState))
            CounterMapperValue.PutAggregator(Of T, CounterState)(Counter)
        End Sub

        Public Function GetReader(Of T)() As Func(Of StreamEx, T)
            Return ReaderMapperValue.GetProjector(Of StreamEx, T)()
        End Function
        Public Function GetWriter(Of T)() As Action(Of T, StreamEx)
            Return WriterMapperValue.GetAggregator(Of T, StreamEx)()
        End Function
        Public Function GetCounter(Of T)() As Action(Of T, CounterState)
            Return CounterMapperValue.GetAggregator(Of T, CounterState)()
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
        Public Sub Count(Of T)(ByVal Value As T, ByVal c As CounterState)
            GetCounter(Of T)()(Value, c)
        End Sub
        Public Function Count(Of T)(ByVal Value As T) As Integer
            Dim c As New CounterState With {.Number = 0}
            Count(Of T)(Value, c)
            Return c.Number
        End Function

        Public Class CounterState
            Public Number As Integer
        End Class

        Public Class PrimitiveMapperResolver
            Implements IObjectProjectorResolver
            Implements IObjectAggregatorResolver

            Public Function TryResolveReader(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If DomainType IsNot GetType(StreamEx) Then Return Nothing
                If Readers.ContainsKey(TypePair.Value) Then Return Readers(TypePair.Value)
                Return Nothing
            End Function

            Private Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
                Dim DomainType = TypePair.Key
                Dim RangeType = TypePair.Value
                If RangeType Is GetType(StreamEx) Then Return TryResolveWriter(DomainType)
                If RangeType Is GetType(CounterState) Then Return TryResolveCounter(DomainType)
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

            Private Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
                Readers.Add(GetType(T), Reader)
            End Sub
            Private Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
                Writers.Add(GetType(T), Writer)
            End Sub
            Private Sub PutCounter(Of T)(ByVal Counter As Action(Of T, CounterState))
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

                PutCounter(Sub(i As Byte, c As CounterState) c.Number += 1)
                PutCounter(Sub(i As UInt16, c As CounterState) c.Number += 2)
                PutCounter(Sub(i As UInt32, c As CounterState) c.Number += 4)
                PutCounter(Sub(i As UInt64, c As CounterState) c.Number += 8)
                PutCounter(Sub(i As SByte, c As CounterState) c.Number += 1)
                PutCounter(Sub(i As Int16, c As CounterState) c.Number += 2)
                PutCounter(Sub(i As Int32, c As CounterState) c.Number += 4)
                PutCounter(Sub(i As Int64, c As CounterState) c.Number += 8)
                PutCounter(Sub(f As Single, c As CounterState) c.Number += 4)
                PutCounter(Sub(f As Double, c As CounterState) c.Number += 8)
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

            Private AbsResolver As ObjectMapperAbsoluteResolver
            Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
                Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
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

            Private AbsResolver As ObjectMapperAbsoluteResolver
            Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
                Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
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

            Private AbsResolver As ObjectMapperAbsoluteResolver
            Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
                Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
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

            Private AbsResolver As ObjectMapperAbsoluteResolver
            Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
                Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
            End Sub
        End Class
    End Class
End Namespace
