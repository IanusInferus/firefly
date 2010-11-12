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
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

''' <remarks>
''' 对于非简单类型，应提供自定义序列化器
''' 简单类型是指数据均全部存储在公开可读写字段或全部存储在公开可读写属性中，字段或属性的类型均为简单类型组合，并且类型结构为树状的类型
''' 简单类型组合 ::= 简单类型
'''              | 数组(简单类型组合)
'''              | ICollection(简单类型组合)
''' </remarks>
Public Class BinarySerializer
    Private ReaderCache As New Dictionary(Of Type, [Delegate])
    Private WriterCache As New Dictionary(Of Type, [Delegate])
    Private CounterCache As New Dictionary(Of Type, [Delegate])
    Private ReaderResolversValue As New List(Of IObjectTreeOneToManyMapperResolver(Of StreamEx))
    Private WriterResolversValue As New List(Of IObjectTreeManyToOneMapperResolver(Of StreamEx))
    Private CounterResolversValue As New List(Of IObjectTreeManyToOneMapperResolver(Of CounterState))
    Public ReadOnly Property ReaderResolvers As List(Of IObjectTreeOneToManyMapperResolver(Of StreamEx))
        Get
            Return ReaderResolversValue
        End Get
    End Property
    Public ReadOnly Property WriterResolvers As List(Of IObjectTreeManyToOneMapperResolver(Of StreamEx))
        Get
            Return WriterResolversValue
        End Get
    End Property
    Public ReadOnly Property CounterResolvers As List(Of IObjectTreeManyToOneMapperResolver(Of CounterState))
        Get
            Return CounterResolversValue
        End Get
    End Property
    Public Sub New()
        Me.ReaderResolversValue = New List(Of IObjectTreeOneToManyMapperResolver(Of StreamEx)) From {
            New PrimitiveSerializerResolver(),
            New ObjectTreeOneToManyMapper(Of StreamEx).EnumMapperResolver(AddressOf Me.Read),
            New CollectionOneToManySerializerResolver(Of StreamEx)(AddressOf Me.Read),
            New ObjectTreeOneToManyMapper(Of StreamEx).ClassAndStructureMapperResolver(AddressOf Me.Read)
        }
        Me.WriterResolversValue = New List(Of IObjectTreeManyToOneMapperResolver(Of StreamEx)) From {
            New PrimitiveSerializerResolver(),
            New ObjectTreeManyToOneMapper(Of StreamEx).EnumMapperResolver(AddressOf Me.Write),
            New CollectionManyToOneSerializerResolver(Of StreamEx)(AddressOf Me.Write),
            New ObjectTreeManyToOneMapper(Of StreamEx).ClassAndStructureMapperResolver(AddressOf Me.Write)
        }
        Me.CounterResolversValue = New List(Of IObjectTreeManyToOneMapperResolver(Of CounterState)) From {
            New PrimitiveSerializerResolver(),
            New ObjectTreeManyToOneMapper(Of CounterState).EnumMapperResolver(AddressOf Me.Count),
            New CollectionManyToOneSerializerResolver(Of CounterState)(AddressOf Me.Count),
            New ObjectTreeManyToOneMapper(Of CounterState).ClassAndStructureMapperResolver(AddressOf Me.Count)
        }
    End Sub

    Public Sub PutReader(ByVal PhysicalType As Type, ByVal Reader As [Delegate])
        If ReaderCache.ContainsKey(PhysicalType) Then
            ReaderCache(PhysicalType) = Reader
        Else
            ReaderCache.Add(PhysicalType, Reader)
        End If
    End Sub
    Public Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
        PutReader(GetType(T), Reader)
    End Sub

    Public Sub PutWriter(ByVal PhysicalType As Type, ByVal Writer As [Delegate])
        If WriterCache.ContainsKey(PhysicalType) Then
            WriterCache(PhysicalType) = Writer
        Else
            WriterCache.Add(PhysicalType, Writer)
        End If
    End Sub
    Public Sub PutWriter(Of T)(ByVal Writer As Action(Of T, StreamEx))
        PutWriter(GetType(T), Writer)
    End Sub

    Public Sub PutCounter(ByVal PhysicalType As Type, ByVal Counter As [Delegate])
        If CounterCache.ContainsKey(PhysicalType) Then
            CounterCache(PhysicalType) = Counter
        Else
            CounterCache.Add(PhysicalType, Counter)
        End If
    End Sub
    Public Sub PutCounter(Of T)(ByVal Counter As Action(Of T, CounterState))
        PutCounter(GetType(T), Counter)
    End Sub

    Public Function GetReader(ByVal PhysicalType As Type) As [Delegate]
        If ReaderCache.ContainsKey(PhysicalType) Then Return ReaderCache(PhysicalType)
        For Each r In ReaderResolversValue
            Dim Resolved = r.TryResolve(PhysicalType)
            If Resolved IsNot Nothing Then
                ReaderCache.Add(PhysicalType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetReader(Of T)() As Func(Of StreamEx, T)
        Return DirectCast(GetReader(GetType(T)), Func(Of StreamEx, T))
    End Function

    Public Function GetWriter(ByVal PhysicalType As Type) As [Delegate]
        If WriterCache.ContainsKey(PhysicalType) Then Return WriterCache(PhysicalType)
        For Each r In WriterResolversValue
            Dim Resolved = r.TryResolve(PhysicalType)
            If Resolved IsNot Nothing Then
                WriterCache.Add(PhysicalType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetWriter(Of T)() As Action(Of T, StreamEx)
        Return DirectCast(GetWriter(GetType(T)), Action(Of T, StreamEx))
    End Function

    Public Function GetCounter(ByVal PhysicalType As Type) As [Delegate]
        If CounterCache.ContainsKey(PhysicalType) Then Return CounterCache(PhysicalType)
        For Each r In CounterResolversValue
            Dim Resolved = r.TryResolve(PhysicalType)
            If Resolved IsNot Nothing Then
                CounterCache.Add(PhysicalType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetCounter(Of T)() As Action(Of T, CounterState)
        Return DirectCast(GetCounter(GetType(T)), Action(Of T, CounterState))
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

    Public Class PrimitiveSerializerResolver
        Implements IObjectTreeOneToManyMapperResolver(Of StreamEx)
        Implements IObjectTreeManyToOneMapperResolver(Of StreamEx)
        Implements IObjectTreeManyToOneMapperResolver(Of CounterState)

        Public Function TryResolveReader(ByVal RangeType As System.Type) As [Delegate] Implements IObjectTreeOneToManyMapperResolver(Of StreamEx).TryResolve
            If Readers.ContainsKey(RangeType) Then Return Readers(RangeType)
            Return Nothing
        End Function

        Public Function TryResolveWriter(ByVal DomainType As System.Type) As [Delegate] Implements IObjectTreeManyToOneMapperResolver(Of StreamEx).TryResolve
            If Writers.ContainsKey(DomainType) Then Return Writers(DomainType)
            Return Nothing
        End Function

        Public Function TryResolveCounter(ByVal DomainType As System.Type) As System.Delegate Implements IObjectTreeManyToOneMapperResolver(Of CounterState).TryResolve
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

    Public Class CollectionOneToManySerializerResolver(Of D)
        Inherits ObjectTreeOneToManyMapper(Of D).CollectionMapperResolver

        Public Overrides Function DefaultArrayMapper(Of R)(ByVal Key As D) As R()
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(R)), Func(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Func(Of D, Integer))
            Dim NumElement = IntMapper(Key)
            Dim arr = New R(NumElement - 1) {}
            For n = 0 To NumElement - 1
                arr(n) = Mapper(Key)
            Next
            Return arr
        End Function
        Public Overrides Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList
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

    Public Class CollectionManyToOneSerializerResolver(Of R)
        Inherits ObjectTreeManyToOneMapper(Of R).CollectionMapperResolver

        Public Overrides Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R)
            Dim Mapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(D)), Action(Of D, R))
            Dim IntMapper = DirectCast(Map.MakeDelegateMethodFromDummy(GetType(Integer)), Action(Of Integer, R))
            Dim NumElement = arr.Length
            IntMapper(NumElement, Value)
            For n = 0 To NumElement - 1
                Mapper(arr(n), Value)
            Next
        End Sub
        Public Overrides Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R)
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
