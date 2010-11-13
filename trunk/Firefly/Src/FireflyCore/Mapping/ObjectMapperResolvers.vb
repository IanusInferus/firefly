'==========================================================================
'
'  File:        ObjectMapper.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射
'  Version:     2010.11.14.
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

Namespace Mapping
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ICollectionDefaultUnpacker(Of D)
        Function DefaultArrayMapper(Of R)(ByVal Key As D) As R()
        Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList
    End Interface

    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ICollectionDefaultPacker(Of R)
        Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R)
        Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R)
    End Interface

    Public Class CollectionUnpacker(Of D)
        Implements IObjectProjectorResolver

        Public Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType IsNot GetType(D) Then Return Nothing
            If RangeType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(RangeType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(RangeType.GetElementType)
                End If
            End If
            If RangeType.IsListType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(RangeType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(RangeType.GetGenericArguments(0))
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private ListMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

        Public Sub PutArrayMapperGenerator(ByVal Generator As [Delegate])
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
                    Return Generator.MakeDelegateMethodFromDummy(ElementType)
                End Function

            If ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                ArrayMapperGeneratorCache(Dimension) = Gen
            Else
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
        End Sub
        Public Sub PutArrayMapperGenerator(ByVal Generator As Func(Of D, DummyType()))
            PutArrayMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub
        Public Sub PutArrayMapperGenerator(ByVal Generator As Func(Of D, DummyType(,)))
            PutArrayMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub

        Public Sub PutListMapperGenerator(ByVal Generator As [Delegate])
            Dim FuncType = Generator.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
            Dim DummyListType = FuncType.GetGenericArguments()(1)
            If Not DummyListType.IsListType() Then Throw New ArgumentException
            If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim ListType = DummyListType.GetGenericTypeDefinition

            Dim Gen =
                Function(ElementType As Type) As [Delegate]
                    Return Generator.MakeDelegateMethodFromDummy(ElementType)
                End Function

            If ListMapperGeneratorCache.ContainsKey(ListType) Then
                ListMapperGeneratorCache(ListType) = Gen
            Else
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
        End Sub
        Public Sub PutListMapperGenerator(Of RList As {New, ICollection(Of DummyType)})(ByVal Generator As Func(Of D, RList))
            PutListMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub

        Public Overridable Function TryGetArrayMapperGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
            If Not ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                If Dimension <> 1 Then Return Nothing
                PutArrayMapperGenerator(DirectCast(AddressOf Provider.DefaultArrayMapper(Of DummyType), Func(Of D, DummyType())))
            End If
            Return ArrayMapperGeneratorCache(Dimension)
        End Function
        Public Overridable Function TryGetListMapperGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
            If Not ListMapperGeneratorCache.ContainsKey(ListType) Then
                If Not ListType.IsListType() Then Throw New ArgumentException
                If Not ListType.IsGenericType Then Throw New ArgumentException
                If Not ListType.IsGenericTypeDefinition Then Throw New ArgumentException
                If ListType.GetGenericArguments().Length <> 1 Then Return Nothing
                Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
                Dim DummyMethod As Func(Of D, List(Of DummyType)) = AddressOf Provider.DefaultListMapper(Of DummyType, List(Of DummyType))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                PutListMapperGenerator(m)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Private Provider As ICollectionDefaultUnpacker(Of D)
        Public Sub New(ByVal Provider As ICollectionDefaultUnpacker(Of D))
            Me.Provider = Provider
        End Sub
    End Class

    Public Class RecordUnpacker(Of D)
        Implements IObjectProjectorResolver

        Public Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType IsNot GetType(D) Then Return Nothing
            If RangeType.IsValueType OrElse RangeType.IsClass Then
                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                Dim Constructor As ConstructorInfo = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim iri = RangeType.TryGetImmutableRecordInfo()
                    If iri IsNot Nothing Then
                        FieldsAndProperties = iri.Members
                        Constructor = iri.Constructor
                    End If
                End If
                If FieldsAndProperties Is Nothing Then
                    Dim mri = RangeType.TryGetMutableRecordInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = mri.Members
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Variable(GetType(D), "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
                For Each Pair In FieldsAndProperties
                    Dim Type = Pair.Type
                    Dim Mapper As [Delegate]
                    If TypeToMapper.ContainsKey(Type) Then
                        Mapper = TypeToMapper(Type)
                    Else
                        Mapper = AbsResolver.ResolveProjector(CreatePair(DomainType, Type))
                        TypeToMapper.Add(Type, Mapper)
                    End If
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {dParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                If Constructor IsNot Nothing Then
                    Dim CreateThis = Expression.[New](Constructor, Context.DelegateExpressions)
                    Dim FunctionLambda = Expression.Lambda(Expression.Block(CreateThis), New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                Else
                    Dim rParam = Expression.Variable(RangeType, "Value")
                    Dim CreateThis = Expression.Assign(rParam, Expression.[New](RangeType))
                    Dim Statements As New List(Of Expression)
                    Statements.Add(CreateThis)
                    For Each Pair In FieldsAndProperties.Zip(Context.DelegateExpressions, Function(m, e) New With {.Member = m.Member, .Type = m.Type, .MapperCall = e})
                        Dim Type = Pair.Type
                        Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(rParam, Pair.Member)
                        Statements.Add(Expression.Assign(FieldOrPropertyExpr, Pair.MapperCall))
                    Next
                    Statements.Add(rParam)

                    Dim FunctionLambda = Expression.Lambda(Expression.Block(New ParameterExpression() {rParam}, Statements), New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
            End If
            Return Nothing
        End Function

        Private AbsResolver As ObjectMapperAbsoluteResolver
        Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
            Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
        End Sub
    End Class

    Public Class CollectionPacker(Of R)
        Implements IObjectAggregatorResolver

        Public Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If RangeType IsNot GetType(R) Then Return Nothing
            If DomainType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(DomainType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(DomainType.GetElementType)
                End If
            End If
            If DomainType.IsListType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(DomainType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(DomainType.GetGenericArguments(0))
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private ListMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

        Public Sub PutArrayMapperGenerator(ByVal Generator As [Delegate])
            Dim FuncType = Generator.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
            Dim DummyArrayType = FuncType.GetGenericArguments()(0)
            If Not DummyArrayType.IsArray Then Throw New ArgumentException
            If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim Dimension = DummyArrayType.GetArrayRank()

            Dim Gen = Function(ElementType As Type) Generator.MakeDelegateMethodFromDummy(ElementType)
            If ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                ArrayMapperGeneratorCache(Dimension) = Gen
            Else
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
        End Sub
        Public Sub PutArrayMapperGenerator(ByVal Generator As Action(Of DummyType(), R))
            PutArrayMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub
        Public Sub PutArrayMapperGenerator(ByVal Generator As Action(Of DummyType(,), R))
            PutArrayMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub

        Public Sub PutListMapperGenerator(ByVal Generator As [Delegate])
            Dim FuncType = Generator.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
            Dim DummyListType = FuncType.GetGenericArguments()(0)
            If Not DummyListType.IsListType() Then Throw New ArgumentException
            If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim ListType = DummyListType.GetGenericTypeDefinition

            Dim Gen =
                Function(ElementType As Type) As [Delegate]
                    Return Generator.MakeDelegateMethodFromDummy(ElementType)
                End Function

            If ListMapperGeneratorCache.ContainsKey(ListType) Then
                ListMapperGeneratorCache(ListType) = Gen
            Else
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
        End Sub
        Public Sub PutListMapperGenerator(Of DList As ICollection(Of DummyType))(ByVal Generator As Action(Of DList, R))
            PutListMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub

        Public Overridable Function TryGetArrayMapperGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
            If Not ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                If Dimension <> 1 Then Return Nothing
                PutArrayMapperGenerator(DirectCast(AddressOf Provider.DefaultArrayMapper(Of DummyType), Action(Of DummyType(), R)))
            End If
            Return ArrayMapperGeneratorCache(Dimension)
        End Function
        Public Overridable Function TryGetListMapperGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
            If Not ListMapperGeneratorCache.ContainsKey(ListType) Then
                If Not ListType.IsListType() Then Throw New ArgumentException
                If Not ListType.IsGenericType Then Throw New ArgumentException
                If Not ListType.IsGenericTypeDefinition Then Throw New ArgumentException
                If ListType.GetGenericArguments().Length <> 1 Then Return Nothing
                Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))
                Dim DummyMethod As Action(Of List(Of DummyType), R) = AddressOf Provider.DefaultListMapper(Of DummyType, List(Of DummyType))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                PutListMapperGenerator(m)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Private Provider As ICollectionDefaultPacker(Of R)
        Public Sub New(ByVal Provider As ICollectionDefaultPacker(Of R))
            Me.Provider = Provider
        End Sub
    End Class

    Public Class RecordPacker(Of R)
        Implements IObjectAggregatorResolver

        Public Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If RangeType IsNot GetType(R) Then Return Nothing
            If DomainType.IsValueType OrElse DomainType.IsClass Then
                If Not (DomainType.IsValueType OrElse DomainType.IsClass) Then Return Nothing

                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim iri = DomainType.TryGetImmutableRecordInfo()
                    If iri IsNot Nothing Then FieldsAndProperties = iri.Members
                End If
                If FieldsAndProperties Is Nothing Then
                    Dim mri = DomainType.TryGetMutableRecordInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = mri.Members
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Variable(DomainType, "Key")
                Dim rParam = Expression.Variable(GetType(R), "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
                For Each Pair In FieldsAndProperties
                    Dim Type = Pair.Type
                    Dim Mapper As [Delegate]
                    If TypeToMapper.ContainsKey(Type) Then
                        Mapper = TypeToMapper(Type)
                    Else
                        Mapper = AbsResolver.ResolveAggregator(CreatePair(Type, RangeType))
                        TypeToMapper.Add(Type, Mapper)
                    End If
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, Pair.Member)
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Mapper, New Expression() {FieldOrPropertyExpr, rParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim FunctionLambda = Expression.Lambda(Expression.Block(Context.DelegateExpressions), New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private AbsResolver As ObjectMapperAbsoluteResolver
        Public Sub New(ByVal AbsResolver As IObjectMapperResolver)
            Me.AbsResolver = New ObjectMapperAbsoluteResolver(AbsResolver)
        End Sub
    End Class
End Namespace
