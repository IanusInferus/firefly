'==========================================================================
'
'  File:        BinarySerializer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 二进制序列化类
'  Version:     2010.11.12.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic

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
    Private ReaderMapperValue As ObjectOneToManyMapper(Of StreamEx)
    Public ReadOnly Property ReaderMapper As ObjectOneToManyMapper(Of StreamEx)
        Get
            Return ReaderMapperValue
        End Get
    End Property
    Private WriterMapperValue As ObjectManyToOneMapper(Of StreamEx)
    Public ReadOnly Property WriterMapper As ObjectManyToOneMapper(Of StreamEx)
        Get
            Return WriterMapperValue
        End Get
    End Property
    Private CounterMapperValue As ObjectManyToOneMapper(Of CounterState)
    Public ReadOnly Property CounterMapper As ObjectManyToOneMapper(Of CounterState)
        Get
            Return CounterMapperValue
        End Get
    End Property

    Public Sub New()
        ReaderMapperValue = New ObjectOneToManyMapper(Of StreamEx)
        ReaderMapperValue.Resolvers.AddRange(New List(Of IObjectOneToManyMapperResolver(Of StreamEx)) From {
            New PrimitiveMapperResolver(),
            New ObjectOneToManyMapper(Of StreamEx).EnumMapperResolver(AddressOf ReaderMapperValue.Map),
            New ObjectOneToManyMapper(Of StreamEx).CollectionMapperResolver(New CollectionOneToManyMapperResolverDefaultProvider(Of StreamEx)(AddressOf ReaderMapperValue.Map)),
            New ObjectOneToManyMapper(Of StreamEx).ClassAndStructureMapperResolver(AddressOf ReaderMapperValue.Map)
        })
        WriterMapperValue = New ObjectManyToOneMapper(Of StreamEx)
        WriterMapperValue.Resolvers.AddRange(New List(Of IObjectManyToOneMapperResolver(Of StreamEx)) From {
            New PrimitiveMapperResolver(),
            New ObjectManyToOneMapper(Of StreamEx).EnumMapperResolver(AddressOf Me.Write),
            New ObjectManyToOneMapper(Of StreamEx).CollectionMapperResolver(New CollectionManyToOneMapperResolverDefaultProvider(Of StreamEx)(AddressOf Me.Write)),
            New ObjectManyToOneMapper(Of StreamEx).ClassAndStructureMapperResolver(AddressOf Me.Write)
        })
        CounterMapperValue = New ObjectManyToOneMapper(Of CounterState)
        CounterMapperValue.Resolvers.AddRange(New List(Of IObjectManyToOneMapperResolver(Of CounterState)) From {
            New PrimitiveMapperResolver(),
            New ObjectManyToOneMapper(Of CounterState).EnumMapperResolver(AddressOf Me.Count),
            New ObjectManyToOneMapper(Of CounterState).CollectionMapperResolver(New CollectionManyToOneMapperResolverDefaultProvider(Of CounterState)(AddressOf Me.Count)),
            New ObjectManyToOneMapper(Of CounterState).ClassAndStructureMapperResolver(AddressOf Me.Count)
        })
    End Sub

    Public Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
        ReaderMapperValue.PutMapper(Of T)(Reader)
    End Sub
    Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
        WriterMapperValue.PutMapper(Of T)(Writer)
    End Sub
    Public Sub PutCounter(Of T)(ByVal Counter As Action(Of T, CounterState))
        CounterMapperValue.PutMapper(Of T)(Counter)
    End Sub

    Public Function GetReader(Of T)() As Func(Of StreamEx, T)
        Return ReaderMapperValue.GetMapper(Of T)()
    End Function
    Public Function GetWriter(Of T)() As Action(Of T, StreamEx)
        Return WriterMapperValue.GetMapper(Of T)()
    End Function
    Public Function GetCounter(Of T)() As Action(Of T, CounterState)
        Return CounterMapperValue.GetMapper(Of T)()
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
        Implements IObjectOneToManyMapperResolver(Of StreamEx)
        Implements IObjectManyToOneMapperResolver(Of StreamEx)
        Implements IObjectManyToOneMapperResolver(Of CounterState)

        Public Function TryResolveReader(ByVal RangeType As System.Type) As [Delegate] Implements IObjectOneToManyMapperResolver(Of StreamEx).TryResolve
            If Readers.ContainsKey(RangeType) Then Return Readers(RangeType)
            Return Nothing
        End Function

        Public Function TryResolveWriter(ByVal DomainType As System.Type) As [Delegate] Implements IObjectManyToOneMapperResolver(Of StreamEx).TryResolve
            If Writers.ContainsKey(DomainType) Then Return Writers(DomainType)
            Return Nothing
        End Function

        Public Function TryResolveCounter(ByVal DomainType As System.Type) As System.Delegate Implements IObjectManyToOneMapperResolver(Of CounterState).TryResolve
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

    Public Class CollectionOneToManyMapperResolverDefaultProvider(Of D)
        Implements ICollectionOneToManyMapperResolverDefaultProvider(Of D)

        Public Function DefaultArrayMapper(Of R)(ByVal Key As D) As R() Implements ICollectionOneToManyMapperResolverDefaultProvider(Of D).DefaultArrayMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(R)), Func(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Func(Of D, Integer))
            Dim NumElement = IntMapper(Key)
            Dim arr = New R(NumElement - 1) {}
            For n = 0 To NumElement - 1
                arr(n) = Mapper(Key)
            Next
            Return arr
        End Function
        Public Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList Implements ICollectionOneToManyMapperResolverDefaultProvider(Of D).DefaultListMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(R)), Func(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Func(Of D, Integer))
            Dim NumElement = IntMapper(Key)
            Dim list = New RList()
            For n = 0 To NumElement - 1
                list.Add(Mapper(Key))
            Next
            Return list
        End Function

        Private Map As Func(Of D, DummyType)
        Public Sub New(ByVal Map As Func(Of D, DummyType))
            Me.Map = Map
        End Sub
    End Class

    Public Class CollectionManyToOneMapperResolverDefaultProvider(Of R)
        Implements ICollectionMapperResolverDefaultProvider(Of R)

        Public Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R) Implements ICollectionMapperResolverDefaultProvider(Of R).DefaultArrayMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(D)), Action(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Action(Of Integer, R))
            Dim NumElement = arr.Length
            IntMapper(NumElement, Value)
            For n = 0 To NumElement - 1
                Mapper(arr(n), Value)
            Next
        End Sub
        Public Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R) Implements ICollectionMapperResolverDefaultProvider(Of R).DefaultListMapper
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(D)), Action(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Action(Of Integer, R))
            Dim NumElement = list.Count
            IntMapper(NumElement, Value)
            For Each v In list
                Mapper(v, Value)
            Next
        End Sub

        Private Map As Action(Of DummyType, R)
        Public Sub New(ByVal Map As Action(Of DummyType, R))
            Me.Map = Map
        End Sub
    End Class
End Class
