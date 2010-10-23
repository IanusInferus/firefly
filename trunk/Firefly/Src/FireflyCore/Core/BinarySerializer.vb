'==========================================================================
'
'  File:        BinarySerializer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 二进制序列化类
'  Version:     2010.10.24.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Class BinarySerializer
    Private ReaderCache As New Dictionary(Of Type, [Delegate])
    Private WriterCache As New Dictionary(Of Type, [Delegate])
    Private CounterCache As New Dictionary(Of Type, [Delegate])
    Private Resolvers As IBinarySerializerResolver()

    Public Sub New()
        Me.Resolvers = New IBinarySerializerResolver() {New PrimitiveSerializerResolver(), New EnumSerializerResolver(Me), New CollectionSerializerResolver(Me), New ClassAndStructureSerializerResolver(Me)}
    End Sub
    Public Sub New(ByVal Resolvers As IEnumerable(Of IBinarySerializerResolver))
        Me.Resolvers = Resolvers.ToArray()
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
    Public Sub PutWriter(Of T)(ByVal Writer As Action(Of StreamEx, T))
        PutWriter(GetType(T), Writer)
    End Sub

    Public Sub PutCounter(ByVal PhysicalType As Type, ByVal Counter As [Delegate])
        If CounterCache.ContainsKey(PhysicalType) Then
            CounterCache(PhysicalType) = Counter
        Else
            CounterCache.Add(PhysicalType, Counter)
        End If
    End Sub
    Public Sub PutCounter(Of T)(ByVal Counter As Func(Of T, Integer))
        PutCounter(GetType(T), Counter)
    End Sub

    Public Function GetReader(ByVal PhysicalType As Type) As [Delegate]
        If ReaderCache.ContainsKey(PhysicalType) Then Return ReaderCache(PhysicalType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolveReader(PhysicalType)
            If Resolved IsNot Nothing Then Return Resolved
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetReader(Of T)() As Func(Of StreamEx, T)
        Return DirectCast(GetReader(GetType(T)), Func(Of StreamEx, T))
    End Function

    Public Function GetWriter(ByVal PhysicalType As Type) As [Delegate]
        If WriterCache.ContainsKey(PhysicalType) Then Return WriterCache(PhysicalType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolveWriter(PhysicalType)
            If Resolved IsNot Nothing Then Return Resolved
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetWriter(Of T)() As Action(Of StreamEx, T)
        Return DirectCast(GetWriter(GetType(T)), Action(Of StreamEx, T))
    End Function

    Public Function GetCounter(ByVal PhysicalType As Type) As [Delegate]
        If CounterCache.ContainsKey(PhysicalType) Then Return CounterCache(PhysicalType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolveCounter(PhysicalType)
            If Resolved IsNot Nothing Then Return Resolved
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetCounter(Of T)() As Func(Of T, Integer)
        Return DirectCast(GetCounter(GetType(T)), Func(Of T, Integer))
    End Function

    Public Function Read(Of T)(ByVal s As StreamEx) As T
        Return GetReader(Of T)()(s)
    End Function
    Public Sub Write(Of T)(ByVal s As StreamEx, ByVal Value As T)
        GetWriter(Of T)()(s, Value)
    End Sub
    Public Function Count(Of T)(ByVal Value As T) As Integer
        Return GetCounter(Of T)()(Value)
    End Function
End Class

Public Class DummyType
End Class

Public Interface IBinarySerializerResolver
    Function TryResolveReader(ByVal Type As Type) As [Delegate]
    Function TryResolveWriter(ByVal Type As Type) As [Delegate]
    Function TryResolveCounter(ByVal Type As Type) As [Delegate]
End Interface

Public Module MetaProgramming
    <Extension()> Public Function IsListType(ByVal PhysicalType As Type) As Boolean
        Return PhysicalType.GetInterfaces().Any(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of )))
    End Function
    <Extension()> Public Function GetListElementType(ByVal PhysicalType As Type) As Type
        Return PhysicalType.GetInterfaces().Where(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of ))).Single.GetGenericArguments()(0)
    End Function
    <Extension()> Public Function GetMethodFromDefinedGenericMethod(ByVal m As [Delegate], ByVal MethodType As Type, ByVal ParamArray ParameterTypes As Type()) As [Delegate]
        Dim Target = m.Target
        Dim gm = m.Method.GetGenericMethodDefinition().MakeGenericMethod(ParameterTypes)
        Return [Delegate].CreateDelegate(MethodType, Target, gm)
    End Function
End Module

Public Class PrimitiveSerializerResolver
    Implements IBinarySerializerResolver

    Public Function TryResolveReader(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveReader
        If Readers.ContainsKey(PhysicalType) Then Return Readers(PhysicalType)
        Return Nothing
    End Function

    Public Function TryResolveWriter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveWriter
        If Writers.ContainsKey(PhysicalType) Then Return Writers(PhysicalType)
        Return Nothing
    End Function

    Public Function TryResolveCounter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveCounter
        If Counters.ContainsKey(PhysicalType) Then Return Counters(PhysicalType)
        Return Nothing
    End Function

    Private Sub PutReader(Of T)(ByVal Reader As Func(Of StreamEx, T))
        Readers.Add(GetType(T), Reader)
    End Sub
    Private Sub PutWriter(Of T)(ByVal Writer As Action(Of StreamEx, T))
        Writers.Add(GetType(T), Writer)
    End Sub
    Private Sub PutCounter(Of T)(ByVal Counter As Func(Of T, Integer))
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

        PutWriter(Sub(s As StreamEx, b As Byte) s.WriteByte(b))
        PutWriter(Sub(s As StreamEx, i As UInt16) s.WriteUInt16(i))
        PutWriter(Sub(s As StreamEx, i As UInt32) s.WriteUInt32(i))
        PutWriter(Sub(s As StreamEx, i As UInt64) s.WriteUInt64(i))
        PutWriter(Sub(s As StreamEx, i As SByte) s.WriteInt8(i))
        PutWriter(Sub(s As StreamEx, i As Int16) s.WriteInt16(i))
        PutWriter(Sub(s As StreamEx, i As Int32) s.WriteInt32(i))
        PutWriter(Sub(s As StreamEx, i As Int64) s.WriteInt64(i))
        PutWriter(Sub(s As StreamEx, f As Single) s.WriteFloat32(f))
        PutWriter(Sub(s As StreamEx, f As Double) s.WriteFloat64(f))

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

Public Class EnumSerializerResolver
    Implements IBinarySerializerResolver

    Public Function TryResolveReader(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveReader
        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = bs.GetReader(PhysicalType.GetEnumUnderlyingType)

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim FunctionBody = Expression.ConvertChecked(Expression.Call(UnderlyingReader.Method, sParam), PhysicalType)
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam})

            Return FunctionLambda.Compile()
        End If
        Return Nothing
    End Function

    Public Function TryResolveWriter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveWriter
        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = bs.GetWriter(PhysicalType.GetEnumUnderlyingType)

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim FunctionBody = Expression.Call(UnderlyingReader.Method, sParam, Expression.ConvertChecked(ThisParam, PhysicalType.GetEnumUnderlyingType))
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam, ThisParam})

            Return FunctionLambda.Compile()
        End If
        Return Nothing
    End Function

    Public Function TryResolveCounter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveCounter
        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = bs.GetCounter(PhysicalType.GetEnumUnderlyingType)

            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim FunctionBody = Expression.Call(UnderlyingReader.Method, Expression.ConvertChecked(ThisParam, PhysicalType.GetEnumUnderlyingType))
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {ThisParam})

            Return FunctionLambda.Compile()
        End If
        Return Nothing
    End Function

    Private bs As BinarySerializer
    Public Sub New(ByVal bs As BinarySerializer)
        Me.bs = bs
    End Sub
End Class

Public Class CollectionSerializerResolver
    Implements IBinarySerializerResolver

    Public Function TryResolveReader(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveReader
        If PhysicalType.IsArray Then
            Dim ArrayReaderGen = TryGetArrayReaderGenerator(PhysicalType.GetArrayRank)
            If ArrayReaderGen IsNot Nothing Then
                Return ArrayReaderGen(PhysicalType.GetElementType)
            End If
        End If
        If PhysicalType.IsListType() Then
            Dim ListReaderGen = TryGetListReaderGenerator(PhysicalType.GetGenericTypeDefinition)
            If ListReaderGen IsNot Nothing Then
                Return ListReaderGen(PhysicalType.GetGenericArguments(0))
            End If
        End If
        Return Nothing
    End Function

    Public Function TryResolveWriter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveWriter
        If PhysicalType.IsArray Then
            Dim ArrayWriterGen = TryGetArrayWriterGenerator(PhysicalType.GetArrayRank)
            If ArrayWriterGen IsNot Nothing Then
                Return ArrayWriterGen(PhysicalType.GetElementType)
            End If
        End If
        If PhysicalType.IsListType() Then
            Dim ListWriterGen = TryGetListWriterGenerator(PhysicalType.GetGenericTypeDefinition)
            If ListWriterGen IsNot Nothing Then
                Return ListWriterGen(PhysicalType.GetGenericArguments(0))
            End If
        End If
        Return Nothing
    End Function

    Public Function TryResolveCounter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveCounter
        If PhysicalType.IsArray Then
            Dim ArrayCounterGen = TryGetArrayCounterGenerator(PhysicalType.GetArrayRank)
            If ArrayCounterGen IsNot Nothing Then
                Return ArrayCounterGen(PhysicalType.GetElementType)
            End If
        End If
        If PhysicalType.IsListType() Then
            Dim ListCounterGen = TryGetListCounterGenerator(PhysicalType.GetGenericTypeDefinition)
            If ListCounterGen IsNot Nothing Then
                Return ListCounterGen(PhysicalType.GetGenericArguments(0))
            End If
        End If
        Return Nothing
    End Function

    Private bs As BinarySerializer
    Public Sub New(ByVal bs As BinarySerializer)
        Me.bs = bs
    End Sub

    Private ArrayReaderGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
    Private ArrayWriterGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
    Private ArrayCounterGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
    Private ListReaderGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))
    Private ListWriterGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))
    Private ListCounterGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

    Public Sub PutArrayReaderGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
        Dim DummyArrayType = FuncType.GetGenericArguments()(1)
        If Not DummyArrayType.IsArray Then Throw New ArgumentException
        If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim Dimension = DummyArrayType.GetArrayRank()

        Dim MakeArrayType =
            Function(ElementType As Type, n As Integer) As Type
                If n = 1 Then Return ElementType.MakeArrayType
                Return ElementType.MakeArrayType(n)
            End Function
        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), MakeArrayType(ElementType, Dimension))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType)
            End Function

        If ArrayReaderGeneratorCache.ContainsKey(Dimension) Then
            ArrayReaderGeneratorCache(Dimension) = Gen
        Else
            ArrayReaderGeneratorCache.Add(Dimension, Gen)
        End If
    End Sub
    Public Sub PutArrayReaderGenerator(ByVal Generator As Func(Of StreamEx, DummyType()))
        PutArrayReaderGenerator(DirectCast(Generator, [Delegate]))
    End Sub
    Public Sub PutArrayReaderGenerator(ByVal Generator As Func(Of StreamEx, DummyType(,)))
        PutArrayReaderGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Sub PutListReaderGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
        Dim DummyListType = FuncType.GetGenericArguments()(1)
        If Not DummyListType.IsListType() Then Throw New ArgumentException
        If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim ListType = DummyListType.GetGenericTypeDefinition

        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), ListType.MakeGenericType(ElementType))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType, ListType.MakeGenericType(ElementType))
            End Function

        If ListReaderGeneratorCache.ContainsKey(ListType) Then
            ListReaderGeneratorCache(ListType) = Gen
        Else
            ListReaderGeneratorCache.Add(ListType, Gen)
        End If
    End Sub
    Public Sub PutListReaderGenerator(Of TList As {New, ICollection(Of DummyType)})(ByVal Generator As Func(Of StreamEx, TList))
        PutListReaderGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Sub PutArrayWriterGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
        Dim DummyArrayType = FuncType.GetGenericArguments()(1)
        If Not DummyArrayType.IsArray Then Throw New ArgumentException
        If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim Dimension = DummyArrayType.GetArrayRank()

        Dim MakeArrayType =
            Function(ElementType As Type, n As Integer) As Type
                If n = 1 Then Return ElementType.MakeArrayType
                Return ElementType.MakeArrayType(n)
            End Function
        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Action(Of ,)).MakeGenericType(GetType(StreamEx), MakeArrayType(ElementType, Dimension))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType)
            End Function

        If ArrayWriterGeneratorCache.ContainsKey(Dimension) Then
            ArrayWriterGeneratorCache(Dimension) = Gen
        Else
            ArrayWriterGeneratorCache.Add(Dimension, Gen)
        End If
    End Sub
    Public Sub PutArrayWriterGenerator(ByVal Generator As Action(Of StreamEx, DummyType()))
        PutArrayWriterGenerator(DirectCast(Generator, [Delegate]))
    End Sub
    Public Sub PutArrayWriterGenerator(ByVal Generator As Action(Of StreamEx, DummyType(,)))
        PutArrayWriterGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Sub PutListWriterGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
        Dim DummyListType = FuncType.GetGenericArguments()(1)
        If Not DummyListType.IsListType() Then Throw New ArgumentException
        If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim ListType = DummyListType.GetGenericTypeDefinition

        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Action(Of ,)).MakeGenericType(GetType(StreamEx), ListType.MakeGenericType(ElementType))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType, ListType.MakeGenericType(ElementType))
            End Function

        If ListWriterGeneratorCache.ContainsKey(ListType) Then
            ListWriterGeneratorCache(ListType) = Gen
        Else
            ListWriterGeneratorCache.Add(ListType, Gen)
        End If
    End Sub
    Public Sub PutListWriterGenerator(Of TList As ICollection(Of DummyType))(ByVal Generator As Func(Of StreamEx, TList))
        PutListWriterGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Sub PutArrayCounterGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
        Dim DummyArrayType = FuncType.GetGenericArguments()(0)
        If Not DummyArrayType.IsArray Then Throw New ArgumentException
        If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim Dimension = DummyArrayType.GetArrayRank()

        Dim MakeArrayType =
            Function(ElementType As Type, n As Integer) As Type
                If n = 1 Then Return ElementType.MakeArrayType
                Return ElementType.MakeArrayType(n)
            End Function
        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Func(Of ,)).MakeGenericType(MakeArrayType(ElementType, Dimension), GetType(Integer))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType)
            End Function

        If ArrayCounterGeneratorCache.ContainsKey(Dimension) Then
            ArrayCounterGeneratorCache(Dimension) = Gen
        Else
            ArrayCounterGeneratorCache.Add(Dimension, Gen)
        End If
    End Sub
    Public Sub PutArrayCounterGenerator(ByVal Generator As Func(Of DummyType(), Integer))
        PutArrayCounterGenerator(DirectCast(Generator, [Delegate]))
    End Sub
    Public Sub PutArrayCounterGenerator(ByVal Generator As Func(Of DummyType(,), Integer))
        PutArrayCounterGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Sub PutListCounterGenerator(ByVal Generator As [Delegate])
        Dim FuncType = Generator.GetType()
        If Not FuncType.IsGenericType Then Throw New ArgumentException
        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
        Dim DummyListType = FuncType.GetGenericArguments()(0)
        If Not DummyListType.IsListType() Then Throw New ArgumentException
        If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
        Dim ListType = DummyListType.GetGenericTypeDefinition

        Dim Gen =
            Function(ElementType As Type) As [Delegate]
                Dim dt = GetType(Func(Of ,)).MakeGenericType(ListType.MakeGenericType(ElementType), GetType(Integer))
                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType, ListType.MakeGenericType(ElementType))
            End Function

        If ListCounterGeneratorCache.ContainsKey(ListType) Then
            ListCounterGeneratorCache(ListType) = Gen
        Else
            ListCounterGeneratorCache.Add(ListType, Gen)
        End If
    End Sub
    Public Sub PutListCounterGenerator(Of TList As ICollection(Of DummyType))(ByVal Generator As Func(Of StreamEx, TList))
        PutListCounterGenerator(DirectCast(Generator, [Delegate]))
    End Sub

    Public Overridable Function TryGetArrayReaderGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
        If Not ArrayReaderGeneratorCache.ContainsKey(Dimension) Then
            If Dimension <> 1 Then Return Nothing
            PutArrayReaderGenerator(AddressOf DefaultArrayReader(Of DummyType))
        End If
        Return ArrayReaderGeneratorCache(Dimension)
    End Function
    Public Overridable Function TryGetListReaderGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
        If Not ListReaderGeneratorCache.ContainsKey(ListType) Then
            If Not ListType.IsListType() Then Throw New ArgumentException
            If Not ListType.IsGenericType OrElse ListType.GetGenericArguments().Length <> 1 Then Return Nothing
            Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
            Dim DummyMethod As Func(Of StreamEx, List(Of DummyType)) = AddressOf DefaultListReader(Of DummyType, List(Of DummyType))
            Dim MethodType = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), DummyListType)
            Dim m = DummyMethod.GetMethodFromDefinedGenericMethod(MethodType, GetType(DummyType), DummyListType)
            PutListReaderGenerator(m)
        End If
        Return ListReaderGeneratorCache(ListType)
    End Function
    Public Overridable Function TryGetArrayWriterGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
        If Not ArrayWriterGeneratorCache.ContainsKey(Dimension) Then
            If Dimension <> 1 Then Return Nothing
            PutArrayWriterGenerator(AddressOf DefaultArrayWriter(Of DummyType))
        End If
        Return ArrayWriterGeneratorCache(Dimension)
    End Function
    Public Overridable Function TryGetListWriterGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
        If Not ListWriterGeneratorCache.ContainsKey(ListType) Then
            If Not ListType.IsListType() Then Throw New ArgumentException
            If Not ListType.IsGenericType OrElse ListType.GetGenericArguments().Length <> 1 Then Return Nothing
            Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
            Dim DummyMethod As Action(Of StreamEx, List(Of DummyType)) = AddressOf DefaultListWriter(Of DummyType, List(Of DummyType))
            Dim MethodType = GetType(Action(Of ,)).MakeGenericType(GetType(StreamEx), DummyListType)
            Dim m = DummyMethod.GetMethodFromDefinedGenericMethod(MethodType, GetType(DummyType), DummyListType)
            PutListWriterGenerator(m)
        End If
        Return ListWriterGeneratorCache(ListType)
    End Function
    Public Overridable Function TryGetArrayCounterGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
        If Not ArrayCounterGeneratorCache.ContainsKey(Dimension) Then
            If Dimension <> 1 Then Return Nothing
            PutArrayCounterGenerator(AddressOf DefaultArrayCounter(Of DummyType))
        End If
        Return ArrayCounterGeneratorCache(Dimension)
    End Function
    Public Overridable Function TryGetListCounterGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
        If Not ListCounterGeneratorCache.ContainsKey(ListType) Then
            If Not ListType.IsListType() Then Throw New ArgumentException
            If Not ListType.IsGenericType OrElse ListType.GetGenericArguments().Length <> 1 Then Return Nothing
            Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
            Dim DummyMethod As Func(Of List(Of DummyType), Integer) = AddressOf DefaultListCounter(Of DummyType, List(Of DummyType))
            Dim MethodType = GetType(Func(Of ,)).MakeGenericType(DummyListType, GetType(Integer))
            Dim m = DummyMethod.GetMethodFromDefinedGenericMethod(MethodType, GetType(DummyType), DummyListType)
            PutListCounterGenerator(m)
        End If
        Return ListCounterGeneratorCache(ListType)
    End Function

    Public Overridable Function DefaultArrayReader(Of T)(ByVal s As StreamEx) As T()
        Dim Reader As Func(Of StreamEx, T) = bs.GetReader(Of T)()
        Dim NumElement = bs.Read(Of Integer)(s)
        Dim arr = New T(NumElement - 1) {}
        For n = 0 To NumElement - 1
            arr(n) = Reader(s)
        Next
        Return arr
    End Function
    Public Overridable Function DefaultListReader(Of T, TList As {New, ICollection(Of T)})(ByVal s As StreamEx) As TList
        Dim Reader As Func(Of StreamEx, T) = bs.GetReader(Of T)()
        Dim NumElement = bs.Read(Of Integer)(s)
        Dim list = New TList()
        For n = 0 To NumElement - 1
            list.Add(Reader(s))
        Next
        Return list
    End Function
    Public Overridable Sub DefaultArrayWriter(Of T)(ByVal s As StreamEx, ByVal arr As T())
        Dim Writer As Action(Of StreamEx, T) = bs.GetWriter(Of T)()
        Dim NumElement = arr.Length
        bs.Write(s, NumElement)
        For n = 0 To NumElement - 1
            Writer(s, arr(n))
        Next
    End Sub
    Public Overridable Sub DefaultListWriter(Of T, TList As ICollection(Of T))(ByVal s As StreamEx, ByVal list As TList)
        Dim Writer As Action(Of StreamEx, T) = bs.GetWriter(Of T)()
        Dim NumElement = list.Count
        bs.Write(Of Integer)(s, NumElement)
        For Each v In list
            Writer(s, v)
        Next
    End Sub
    Public Overridable Function DefaultArrayCounter(Of T)(ByVal arr As T()) As Integer
        Dim Counter As Func(Of T, Integer) = bs.GetCounter(Of T)()
        Dim NumElement = arr.Length
        Dim Length = bs.Count(Of Integer)(0)
        For n = 0 To NumElement - 1
            Length += Counter(arr(n))
        Next
        Return Length
    End Function
    Public Overridable Function DefaultListCounter(Of T, TList As ICollection(Of T))(ByVal list As TList) As Integer
        Dim Counter As Func(Of T, Integer) = bs.GetCounter(Of T)()
        Dim Length = bs.Count(Of Integer)(0)
        For Each v In list
            Length += Counter(v)
        Next
        Return Length
    End Function
End Class

Public Class ClassAndStructureSerializerResolver
    Implements IBinarySerializerResolver

    Public Function TryResolveReader(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveReader
        If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)
            Dim CreateThis = Expression.Assign(ThisParam, Expression.[New](c))
            Statements.Add(CreateThis)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim TypeToReader As New Dictionary(Of Type, [Delegate])
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToReader.ContainsKey(Type) Then Continue For
                Dim Reader = bs.GetReader(Type)
                TypeToReader.Add(Type, Reader)
                If Reader.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(Reader, n)
                    ClosureObjects.Add(Reader)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Reader = TypeToReader(Type)
                Dim ReaderCall As Expression
                If Reader.Target Is Nothing Then
                    ReaderCall = Expression.Call(Reader.Method, sParam)
                Else
                    Dim n = ReaderToClosureField(Reader)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), Type)
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    ReaderCall = Expression.Invoke(DelegateFunc, sParam)
                End If
                Dim Assign = Expression.Assign(FieldOrPropertyExpr, ReaderCall)
                Statements.Add(Assign)
            Next
            Statements.Add(ThisParam)

            Dim FunctionBody = Expression.Block(New ParameterExpression() {ThisParam}, Statements)
            Dim FunctionLambda As LambdaExpression = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled As [Delegate] = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If

            Return Compiled
        End If
        Return Nothing
    End Function

    Public Function TryResolveWriter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveWriter
        If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim TypeToWriter As New Dictionary(Of Type, [Delegate])
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToWriter.ContainsKey(Type) Then Continue For
                Dim Writer = bs.GetWriter(Type)
                TypeToWriter.Add(Type, Writer)
                If Writer.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(Writer, n)
                    ClosureObjects.Add(Writer)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Writer = TypeToWriter(Type)
                Dim WriterCall As Expression
                If Writer.Target Is Nothing Then
                    WriterCall = Expression.Call(Writer.Method, sParam, FieldOrPropertyExpr)
                Else
                    Dim n = ReaderToClosureField(Writer)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Action(Of ,)).MakeGenericType(GetType(StreamEx), Type)
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    WriterCall = Expression.Invoke(DelegateFunc, sParam, FieldOrPropertyExpr)
                End If
                Statements.Add(WriterCall)
            Next

            Dim FunctionBody = Expression.Block(Statements)
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam, ThisParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If
            Return Compiled
        End If
        Return Nothing
    End Function

    Public Function TryResolveCounter(ByVal PhysicalType As Type) As [Delegate] Implements IBinarySerializerResolver.TryResolveCounter
        If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim FunctionBody As Expression = Expression.Constant(0)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim TypeToCounter As New Dictionary(Of Type, [Delegate])
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                If TypeToCounter.ContainsKey(Type) Then Continue For
                Dim Counter = bs.GetCounter(Type)
                TypeToCounter.Add(Type, Counter)
                If Counter.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(Counter, n)
                    ClosureObjects.Add(Counter)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Counter = TypeToCounter(Type)
                Dim CounterCall As Expression
                If Counter.Target Is Nothing Then
                    CounterCall = Expression.Call(Counter.Method, FieldOrPropertyExpr)
                Else
                    Dim n = ReaderToClosureField(Counter)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Func(Of ,)).MakeGenericType(Type, GetType(Integer))
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    CounterCall = Expression.Invoke(DelegateFunc, FieldOrPropertyExpr)
                End If
                FunctionBody = Expression.AddChecked(FunctionBody, CounterCall)
            Next

            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {ThisParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If

            Return Compiled
        End If
        Return Nothing
    End Function

    Private bs As BinarySerializer
    Public Sub New(ByVal bs As BinarySerializer)
        Me.bs = bs
    End Sub
End Class
