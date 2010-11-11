'==========================================================================
'
'  File:        ObjectTreeBuilderAndVisitor.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: Object树创建和访问
'  Version:     2010.11.11.
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

Public Interface IObjectTreeBuilderResolver
    ''' <returns>返回Func(Of ${Type})</returns>
    Function TryResolve(ByVal Type As Type) As [Delegate]
End Interface

Public Interface IObjectTreeVisitorResolver
    ''' <returns>返回Action(Of ${Type})</returns>
    Function TryResolve(ByVal Type As Type) As [Delegate]
End Interface

Public Interface IObjectTreeMapperResolver
    ''' <returns>返回Func(Of ${D}, ${R})</returns>
    Function TryResolve(ByVal D As Type, ByVal R As Type) As [Delegate]
End Interface

Public Class ObjectTreeBuilder
    Private BuilderCache As New Dictionary(Of Type, [Delegate])
    Private Resolvers As IObjectTreeBuilderResolver()

    Public Sub New(ByVal Resolvers As IEnumerable(Of IObjectTreeBuilderResolver))
        Me.Resolvers = Resolvers.ToArray()
    End Sub

    Public Sub PutBuilder(ByVal PhysicalType As Type, ByVal Builder As [Delegate])
        If BuilderCache.ContainsKey(PhysicalType) Then
            BuilderCache(PhysicalType) = Builder
        Else
            BuilderCache.Add(PhysicalType, Builder)
        End If
    End Sub
    Public Sub PutBuilder(Of T)(ByVal Builder As Func(Of T))
        PutBuilder(GetType(T), Builder)
    End Sub

    Public Function GetBuilder(ByVal PhysicalType As Type) As [Delegate]
        If BuilderCache.ContainsKey(PhysicalType) Then Return BuilderCache(PhysicalType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolve(PhysicalType)
            If Resolved IsNot Nothing Then
                BuilderCache.Add(PhysicalType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(PhysicalType.FullName))
    End Function
    Public Function GetBuilder(Of T)() As Func(Of T)
        Return DirectCast(GetBuilder(GetType(T)), Func(Of T))
    End Function

    Public Function Build(Of T)() As T
        Return GetBuilder(Of T)()()
    End Function

    'Public Class EnumBuilderResolver
    '    Implements IObjectTreeBuilderResolver

    '    Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeBuilderResolver.TryResolve
    '        If PhysicalType.IsEnum Then
    '            Dim Build = GetMethodFromDefinedGenericMethod(DirectCast(AddressOf tb.Build(Of DummyType), Func(Of DummyType)), GetType(Func(Of )).MakeGenericType(PhysicalType.GetEnumUnderlyingType))

    '            Dim UnderlyingBuilder As Func(Of DummyType) = 

    '            Dim bParam = Expression.Variable(GetType(ObjectTreeBuilder), "b")
    '            Dim FunctionBody = Expression.ConvertChecked(Expression.Call(bParam, Build.Method), PhysicalType)
    '            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {bParam})

    '            'TODO

    '            Return FunctionLambda.Compile()
    '        End If
    '        Return Nothing
    '    End Function

    '    Private tb As ObjectTreeBuilder
    '    Public Sub New(ByVal tb As ObjectTreeBuilder)
    '        Me.tb = tb
    '    End Sub
    'End Class

    'Public Class CollectionBuilderResolver
    '    Implements IObjectTreeBuilderResolver

    '    Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeBuilderResolver.TryResolve
    '        If PhysicalType.IsArray Then
    '            Dim ArrayBuilderGen = TryGetArrayBuilderGenerator(PhysicalType.GetArrayRank)
    '            If ArrayBuilderGen IsNot Nothing Then
    '                Return ArrayBuilderGen(PhysicalType.GetElementType)
    '            End If
    '        End If
    '        If PhysicalType.IsListType() Then
    '            Dim ListBuilderGen = TryGetListBuilderGenerator(PhysicalType.GetGenericTypeDefinition)
    '            If ListBuilderGen IsNot Nothing Then
    '                Return ListBuilderGen(PhysicalType.GetGenericArguments(0))
    '            End If
    '        End If
    '        Return Nothing
    '    End Function

    '    Private tb As ObjectTreeBuilder
    '    Public Sub New(ByVal tb As ObjectTreeBuilder)
    '        Me.tb = tb
    '    End Sub

    '    Private ArrayBuilderGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
    '    Private ListBuilderGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

    '    Public Sub PutArrayBuilderGenerator(ByVal Generator As [Delegate])
    '        Dim FuncType = Generator.GetType()
    '        If Not FuncType.IsGenericType Then Throw New ArgumentException
    '        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
    '        Dim DummyArrayType = FuncType.GetGenericArguments()(1)
    '        If Not DummyArrayType.IsArray Then Throw New ArgumentException
    '        If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
    '        Dim Dimension = DummyArrayType.GetArrayRank()

    '        Dim MakeArrayType =
    '            Function(ElementType As Type, n As Integer) As Type
    '                If n = 1 Then Return ElementType.MakeArrayType
    '                Return ElementType.MakeArrayType(n)
    '            End Function
    '        Dim Gen =
    '            Function(ElementType As Type) As [Delegate]
    '                Dim dt = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), MakeArrayType(ElementType, Dimension))
    '                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType)
    '            End Function

    '        If ArrayBuilderGeneratorCache.ContainsKey(Dimension) Then
    '            ArrayBuilderGeneratorCache(Dimension) = Gen
    '        Else
    '            ArrayBuilderGeneratorCache.Add(Dimension, Gen)
    '        End If
    '    End Sub
    '    Public Sub PutArrayBuilderGenerator(ByVal Generator As Func(Of StreamEx, DummyType()))
    '        PutArrayBuilderGenerator(DirectCast(Generator, [Delegate]))
    '    End Sub
    '    Public Sub PutArrayBuilderGenerator(ByVal Generator As Func(Of StreamEx, DummyType(,)))
    '        PutArrayBuilderGenerator(DirectCast(Generator, [Delegate]))
    '    End Sub

    '    Public Sub PutListBuilderGenerator(ByVal Generator As [Delegate])
    '        Dim FuncType = Generator.GetType()
    '        If Not FuncType.IsGenericType Then Throw New ArgumentException
    '        If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
    '        Dim DummyListType = FuncType.GetGenericArguments()(1)
    '        If Not DummyListType.IsListType() Then Throw New ArgumentException
    '        If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
    '        Dim ListType = DummyListType.GetGenericTypeDefinition

    '        Dim Gen =
    '            Function(ElementType As Type) As [Delegate]
    '                Dim dt = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), ListType.MakeGenericType(ElementType))
    '                Return Generator.GetMethodFromDefinedGenericMethod(dt, ElementType, ListType.MakeGenericType(ElementType))
    '            End Function

    '        If ListBuilderGeneratorCache.ContainsKey(ListType) Then
    '            ListBuilderGeneratorCache(ListType) = Gen
    '        Else
    '            ListBuilderGeneratorCache.Add(ListType, Gen)
    '        End If
    '    End Sub
    '    Public Sub PutListBuilderGenerator(Of TList As {New, ICollection(Of DummyType)})(ByVal Generator As Func(Of StreamEx, TList))
    '        PutListBuilderGenerator(DirectCast(Generator, [Delegate]))
    '    End Sub

    '    Public Overridable Function TryGetArrayBuilderGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
    '        If Not ArrayBuilderGeneratorCache.ContainsKey(Dimension) Then
    '            If Dimension <> 1 Then Return Nothing
    '            PutArrayBuilderGenerator(AddressOf DefaultArrayBuilder(Of DummyType))
    '        End If
    '        Return ArrayBuilderGeneratorCache(Dimension)
    '    End Function
    '    Public Overridable Function TryGetListBuilderGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
    '        If Not ListBuilderGeneratorCache.ContainsKey(ListType) Then
    '            If Not ListType.IsListType() Then Throw New ArgumentException
    '            If Not ListType.IsGenericType OrElse ListType.GetGenericArguments().Length <> 1 Then Return Nothing
    '            Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
    '            Dim DummyMethod As Func(Of StreamEx, List(Of DummyType)) = AddressOf DefaultListBuilder(Of DummyType, List(Of DummyType))
    '            Dim MethodType = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), DummyListType)
    '            Dim m = DummyMethod.GetMethodFromDefinedGenericMethod(MethodType, GetType(DummyType), DummyListType)
    '            PutListBuilderGenerator(m)
    '        End If
    '        Return ListBuilderGeneratorCache(ListType)
    '    End Function

    '    Public Overridable Function DefaultArrayBuilder(Of T)() As T()
    '        Dim Builder As Func(Of StreamEx, T) = tb.GetBuilder(Of T)()
    '        Dim NumElement = tb.Build(Of Integer)()
    '        Dim arr = New T(NumElement - 1) {}
    '        For n = 0 To NumElement - 1
    '            arr(n) = Builder(s)
    '        Next
    '        Return arr
    '    End Function
    '    Public Overridable Function DefaultListBuilder(Of T, TList As {New, ICollection(Of T)})(ByVal s As StreamEx) As TList
    '        Dim Builder As Func(Of StreamEx, T) = tb.GetBuilder(Of T)()
    '        Dim NumElement = tb.Build(Of Integer)(s)
    '        Dim list = New TList()
    '        For n = 0 To NumElement - 1
    '            list.Add(Builder(s))
    '        Next
    '        Return list
    '    End Function
    'End Class

    'Public Class ClassAndStructureBuilderResolver
    '    Implements IObjectTreeBuilderResolver

    '    Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeBuilderResolver.TryResolve
    '        If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
    '            If PhysicalType.IsClass Then
    '                Dim c = PhysicalType.GetConstructor(New Type() {})
    '                If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
    '            End If

    '            Dim sParam = Expression.Variable(GetType(StreamEx), "s")
    '            Dim ThisParam = Expression.Variable(PhysicalType, "This")
    '            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

    '            Dim Statements As New List(Of Expression)
    '            Dim CreateThis = Expression.Assign(ThisParam, Expression.[New](PhysicalType))
    '            Statements.Add(CreateThis)

    '            Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(ThisParam, f), .Type = f.FieldType})
    '            Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(ThisParam, f), .Type = f.PropertyType})
    '            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
    '            Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
    '            If PhysicalType.IsValueType Then
    '                If FieldsAndProperties.Length = 0 Then
    '                    Return Nothing
    '                End If
    '            End If

    '            Dim TypeToBuilder As New Dictionary(Of Type, [Delegate])
    '            Dim BuilderToClosureField As New Dictionary(Of [Delegate], Integer)
    '            Dim ClosureObjects As New List(Of Object)
    '            For Each Pair In FieldsAndProperties
    '                Dim Type = Pair.Type
    '                If TypeToBuilder.ContainsKey(Type) Then Continue For
    '                Dim Builder = tb.GetBuilder(Type)
    '                TypeToBuilder.Add(Type, Builder)
    '                If Builder.Target IsNot Nothing Then
    '                    Dim n = ClosureObjects.Count
    '                    BuilderToClosureField.Add(Builder, n)
    '                    ClosureObjects.Add(Builder)
    '                End If
    '            Next
    '            Dim Closure As Closure = Nothing
    '            If ClosureObjects.Count > 0 Then
    '                Closure = New Closure(Nothing, ClosureObjects.ToArray)
    '            End If
    '            For Each Pair In FieldsAndProperties
    '                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
    '                Dim Type = Pair.Type
    '                Dim Builder = TypeToBuilder(Type)
    '                Dim BuilderCall As Expression
    '                If Builder.Target Is Nothing Then
    '                    BuilderCall = Expression.Call(Builder.Method, sParam)
    '                Else
    '                    Dim n = BuilderToClosureField(Builder)
    '                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
    '                    Dim DelegateType = GetType(Func(Of ,)).MakeGenericType(GetType(StreamEx), Type)
    '                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
    '                    BuilderCall = Expression.Invoke(DelegateFunc, sParam)
    '                End If
    '                Dim Assign = Expression.Assign(FieldOrPropertyExpr, BuilderCall)
    '                Statements.Add(Assign)
    '            Next
    '            Statements.Add(ThisParam)

    '            Dim FunctionBody = Expression.Block(New ParameterExpression() {ThisParam}, Statements)
    '            Dim FunctionLambda As LambdaExpression = Expression.Lambda(FunctionBody, New ParameterExpression() {sParam})
    '            If Closure IsNot Nothing Then
    '                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
    '            End If

    '            Dim Compiled As [Delegate] = FunctionLambda.Compile()
    '            If Closure IsNot Nothing Then
    '                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
    '                Compiled = CompiledFunc(Closure)
    '            End If

    '            Return Compiled
    '        End If
    '        Return Nothing
    '    End Function

    '    Private tb As ObjectTreeBuilder
    '    Public Sub New(ByVal tb As ObjectTreeBuilder)
    '        Me.tb = tb
    '    End Sub
    'End Class
End Class

Public Class ObjectTreeVisitor

End Class
