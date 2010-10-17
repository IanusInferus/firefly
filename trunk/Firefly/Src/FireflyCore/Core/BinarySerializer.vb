'==========================================================================
'
'  File:        BinarySerializer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 二进制序列化类
'  Version:     2010.10.17.
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

Public Class BinarySerializer
    Public ReadOnly ReaderCache As New Dictionary(Of Type, [Delegate])
    Public ReadOnly WriterCache As New Dictionary(Of Type, [Delegate])
    Public ReadOnly CounterCache As New Dictionary(Of Type, [Delegate])

    Public Sub New()
        ReaderCache.Add(GetType(Byte), Function(s As StreamEx) s.ReadByte)
        ReaderCache.Add(GetType(UInt16), Function(s As StreamEx) s.ReadUInt16)
        ReaderCache.Add(GetType(UInt32), Function(s As StreamEx) s.ReadUInt32)
        ReaderCache.Add(GetType(UInt64), Function(s As StreamEx) s.ReadUInt64)
        ReaderCache.Add(GetType(SByte), Function(s As StreamEx) s.ReadInt8)
        ReaderCache.Add(GetType(Int16), Function(s As StreamEx) s.ReadInt16)
        ReaderCache.Add(GetType(Int32), Function(s As StreamEx) s.ReadInt32)
        ReaderCache.Add(GetType(Int64), Function(s As StreamEx) s.ReadInt64)
        ReaderCache.Add(GetType(Single), Function(s As StreamEx) s.ReadFloat32)
        ReaderCache.Add(GetType(Double), Function(s As StreamEx) s.ReadFloat64)

        WriterCache.Add(GetType(Byte), Sub(s As StreamEx, b As Byte) s.WriteByte(b))
        WriterCache.Add(GetType(UInt16), Sub(s As StreamEx, i As UInt16) s.WriteUInt16(i))
        WriterCache.Add(GetType(UInt32), Sub(s As StreamEx, i As UInt32) s.WriteUInt32(i))
        WriterCache.Add(GetType(UInt64), Sub(s As StreamEx, i As UInt64) s.WriteUInt64(i))
        WriterCache.Add(GetType(SByte), Sub(s As StreamEx, i As SByte) s.WriteInt8(i))
        WriterCache.Add(GetType(Int16), Sub(s As StreamEx, i As Int16) s.WriteInt16(i))
        WriterCache.Add(GetType(Int32), Sub(s As StreamEx, i As Int32) s.WriteInt32(i))
        WriterCache.Add(GetType(Int64), Sub(s As StreamEx, i As Int64) s.WriteInt64(i))
        WriterCache.Add(GetType(Single), Sub(s As StreamEx, f As Single) s.WriteFloat32(f))
        WriterCache.Add(GetType(Double), Sub(s As StreamEx, f As Double) s.WriteFloat64(f))

        CounterCache.Add(GetType(Byte), Function(i As Byte) 1)
        CounterCache.Add(GetType(UInt16), Function(i As UInt16) 2)
        CounterCache.Add(GetType(UInt32), Function(i As UInt32) 4)
        CounterCache.Add(GetType(UInt64), Function(i As UInt64) 8)
        CounterCache.Add(GetType(SByte), Function(i As SByte) 1)
        CounterCache.Add(GetType(Int16), Function(i As Int16) 2)
        CounterCache.Add(GetType(Int32), Function(i As Int32) 4)
        CounterCache.Add(GetType(Int64), Function(i As Int64) 8)
        CounterCache.Add(GetType(Single), Function(f As Single) 4)
        CounterCache.Add(GetType(Double), Function(f As Double) 8)
    End Sub

    Public Shared ReadOnly Property GlobalCache As BinarySerializer
        Get
            Static Cache As BinarySerializer
            If Cache Is Nothing Then Cache = New BinarySerializer
            Return Cache
        End Get
    End Property

    Public Shared Function GetReader(ByVal PhysicalType As Type, ByVal CachedSerializer As BinarySerializer) As [Delegate]
        Dim Cache = CachedSerializer.ReaderCache

        If Cache.ContainsKey(PhysicalType) Then Return Cache(PhysicalType)

        If PhysicalType.IsPrimitive Then Throw New NotSupportedException("NoSupportedPrimitive: {0}".Formats(PhysicalType.FullName))

        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = GetReader(PhysicalType.GetEnumUnderlyingType, CachedSerializer)

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim FunctionBody = Expression.ConvertChecked(Expression.Call(UnderlyingReader.Method, sParam), PhysicalType)
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam})

            Dim Compiled = FunctionLambda.Compile()

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        Else
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then
                Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
            End If

            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Throw New NotSupportedException("NoPublicDefaultConstructor: {0}".Formats(PhysicalType.FullName))

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)
            Dim CreateThis = Expression.Assign(ThisParam, Expression.[New](c))
            Statements.Add(CreateThis)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim FieldsAndProperties = Fields.Concat(Properties).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
                End If
            End If

            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                Dim Reader As [Delegate]
                If Cache.ContainsKey(Type) Then
                    Reader = Cache(Type)
                Else
                    Reader = GetReader(Type, CachedSerializer)
                End If
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
                Dim Reader As [Delegate] = Cache(Type)
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

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        End If
    End Function
    Public Shared Function GetReader(Of T)(ByVal CachedSerializer As BinarySerializer) As Func(Of StreamEx, T)
        Dim PhysicalType = GetType(T)
        Return CType(GetReader(PhysicalType, CachedSerializer), Func(Of StreamEx, T))
    End Function
    Public Function GetReader(Of T)() As Func(Of StreamEx, T)
        Return GetReader(Of T)(Me)
    End Function
    Public Function Read(Of T)(ByVal s As StreamEx) As T
        Return GetReader(Of T)()(s)
    End Function

    Public Shared Function GetWriter(ByVal PhysicalType As Type, ByVal CachedSerializer As BinarySerializer) As [Delegate]
        Dim Cache = CachedSerializer.WriterCache

        If Cache.ContainsKey(PhysicalType) Then Return Cache(PhysicalType)

        If PhysicalType.IsPrimitive Then Throw New NotSupportedException("NoSupportedPrimitive: {0}".Formats(PhysicalType.FullName))

        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = GetWriter(PhysicalType.GetEnumUnderlyingType, CachedSerializer)

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim FunctionBody = Expression.Call(UnderlyingReader.Method, sParam, Expression.ConvertChecked(ThisParam, PhysicalType.GetEnumUnderlyingType))
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam, ThisParam})

            Dim Compiled = FunctionLambda.Compile()

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        Else
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then
                Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
            End If

            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Throw New NotSupportedException("NoPublicDefaultConstructor: {0}".Formats(PhysicalType.FullName))

            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim FieldsAndProperties = Fields.Concat(Properties).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
                End If
            End If

            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                Dim Writer As [Delegate]
                If Cache.ContainsKey(Type) Then
                    Writer = Cache(Type)
                Else
                    Writer = GetWriter(Type, CachedSerializer)
                End If
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
                Dim Writer As [Delegate] = Cache(Type)
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

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        End If
    End Function
    Public Shared Function GetWriter(Of T)(ByVal CachedSerializer As BinarySerializer) As Action(Of StreamEx, T)
        Dim PhysicalType = GetType(T)
        Return CType(GetWriter(PhysicalType, CachedSerializer), Action(Of StreamEx, T))
    End Function
    Public Function GetWriter(Of T)() As Action(Of StreamEx, T)
        Return GetWriter(Of T)(Me)
    End Function
    Public Sub Write(Of T)(ByVal s As StreamEx, ByVal Value As T)
        GetWriter(Of T)()(s, Value)
    End Sub

    Public Shared Function GetCounter(ByVal PhysicalType As Type, ByVal CachedSerializer As BinarySerializer) As [Delegate]
        Dim Cache = CachedSerializer.CounterCache

        If Cache.ContainsKey(PhysicalType) Then Return Cache(PhysicalType)

        If PhysicalType.IsPrimitive Then Throw New NotSupportedException("NoSupportedPrimitive: {0}".Formats(PhysicalType.FullName))

        If PhysicalType.IsEnum Then
            Dim UnderlyingReader = GetCounter(PhysicalType.GetEnumUnderlyingType, CachedSerializer)

            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim FunctionBody = Expression.Call(UnderlyingReader.Method, Expression.ConvertChecked(ThisParam, PhysicalType.GetEnumUnderlyingType))
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {ThisParam})

            Dim Compiled = FunctionLambda.Compile()

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        Else
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then
                Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
            End If

            Dim c = PhysicalType.GetConstructor(New Type() {})
            If c Is Nothing OrElse Not c.IsPublic Then Throw New NotSupportedException("NoPublicDefaultConstructor: {0}".Formats(PhysicalType.FullName))

            Dim ThisParam = Expression.Variable(PhysicalType, "This")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim FunctionBody As Expression = Expression.Constant(0)

            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
            Dim FieldsAndProperties = Fields.Concat(Properties).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Throw New NotSupportedException("NoReader: {0}".Formats(PhysicalType.FullName))
                End If
            End If

            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Counter As [Delegate]
                If Cache.ContainsKey(Type) Then
                    Counter = Cache(Type)
                Else
                    Counter = GetCounter(Type, CachedSerializer)
                End If
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
                Dim Counter As [Delegate] = Cache(Type)
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

            Cache.Add(PhysicalType, Compiled)
            Return Compiled
        End If
    End Function
    Public Shared Function GetCounter(Of T)(ByVal CachedSerializer As BinarySerializer) As Func(Of T, Integer)
        Dim PhysicalType = GetType(T)
        Return CType(GetCounter(PhysicalType, CachedSerializer), Func(Of T, Integer))
    End Function
    Public Function GetCounter(Of T)() As Func(Of T, Integer)
        Return GetCounter(Of T)(Me)
    End Function
    Public Function Count(Of T)(ByVal Value As T) As Integer
        Return GetCounter(Of T)()(Value)
    End Function
End Class
