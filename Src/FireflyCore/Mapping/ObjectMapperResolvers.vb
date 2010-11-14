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
    Public Interface IGenericListProjectorResolver(Of D)
        Function ResolveProjector(Of R, RList As {New, ICollection(Of R)})() As Func(Of D, RList)
    End Interface

    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IGenericListAggregatorResolver(Of R)
        Function ResolveAggregator(Of D, DList As ICollection(Of D))() As Action(Of DList, R)
    End Interface

    Public Class CollectionUnpacker(Of D)
        Implements IObjectProjectorResolver

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType IsNot GetType(D) Then Return Nothing
            If RangeType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(RangeType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(RangeType.GetElementType)
                End If
            End If
            If RangeType.IsCollectionType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(RangeType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(RangeType.GetGenericArguments(0))
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private ListMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

        Public Sub PutGenericArrayMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
            Dim DummyArrayType = FuncType.GetGenericArguments()(1)
            If Not DummyArrayType.IsArray Then Throw New ArgumentException
            If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim Dimension = DummyArrayType.GetArrayRank()

            Dim Gen = Function(ElementType As Type) GenericMapper.MakeDelegateMethodFromDummy(ElementType)
            If ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                ArrayMapperGeneratorCache(Dimension) = Gen
            Else
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
        End Sub
        Public Sub PutGenericArrayMapper(ByVal GenericMapper As Func(Of D, DummyType()))
            PutGenericArrayMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub
        Public Sub PutGenericArrayMapper(ByVal GenericMapper As Func(Of D, DummyType(,)))
            PutGenericArrayMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub

        Public Sub PutGenericListMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
            Dim DummyListType = FuncType.GetGenericArguments()(1)
            If Not DummyListType.IsCollectionType() Then Throw New ArgumentException
            If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim ListType = DummyListType.GetGenericTypeDefinition

            Dim Gen = Function(ElementType As Type) GenericMapper.MakeDelegateMethodFromDummy(ElementType)
            If ListMapperGeneratorCache.ContainsKey(ListType) Then
                ListMapperGeneratorCache(ListType) = Gen
            Else
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
        End Sub
        Public Sub PutGenericListMapper(Of RList As {New, ICollection(Of DummyType)})(ByVal GenericMapper As Func(Of D, RList))
            PutGenericListMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub

        Private Shared Function ArrayToListGenericMapperAdapter(Of T)(ByVal DummyMethod As Func(Of D, List(Of T))) As Func(Of D, T())
            Return Function(Key As D) DummyMethod(Key).ToArray()
        End Function
        Public Function TryGetArrayMapperGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
            If Not ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                If Dimension <> 1 Then Return Nothing
                If Inner Is Nothing Then Return Nothing
                Dim m = DirectCast(AddressOf ArrayToList(Of DummyType), Func(Of Func(Of D, DummyType())))
                Dim Gen = Function(ElementType As Type) DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
            Return ArrayMapperGeneratorCache(Dimension)
        End Function
        Public Function TryGetListMapperGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
            If Not ListMapperGeneratorCache.ContainsKey(ListType) Then
                If Not ListType.IsCollectionType() Then Throw New ArgumentException
                If Not ListType.IsGenericTypeDefinition Then Throw New ArgumentException
                If ListType.GetGenericArguments().Length <> 1 Then Return Nothing
                Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))

                Dim DummyMethod = DirectCast(AddressOf Inner.ResolveProjector(Of DummyType, List(Of DummyType)), Func(Of Func(Of D, List(Of DummyType))))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                Dim Gen = Function(ElementType As Type) DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Private Function ArrayToList(Of R)() As Func(Of D, R())
            Dim Mapper = Inner.ResolveProjector(Of R, List(Of R))()
            Return Function(Key As D) Mapper(Key).ToArray()
        End Function

        Private Inner As IGenericListProjectorResolver(Of D)
        Public Sub New(ByVal Inner As IGenericListProjectorResolver(Of D))
            Me.Inner = Inner
        End Sub
    End Class

    Public Class RecordUnpacker(Of D)
        Implements IObjectProjectorResolver

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectProjectorResolver.TryResolveProjector
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

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If RangeType IsNot GetType(R) Then Return Nothing
            If DomainType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(DomainType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(DomainType.GetElementType)
                End If
            End If
            If DomainType.IsCollectionType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(DomainType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(DomainType.GetGenericArguments(0))
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private ListMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type, [Delegate]))

        Public Sub PutGenericArrayMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
            Dim DummyArrayType = FuncType.GetGenericArguments()(0)
            If Not DummyArrayType.IsArray Then Throw New ArgumentException
            If DummyArrayType.GetElementType IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim Dimension = DummyArrayType.GetArrayRank()

            Dim Gen = Function(ElementType As Type) GenericMapper.MakeDelegateMethodFromDummy(ElementType)
            If ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                ArrayMapperGeneratorCache(Dimension) = Gen
            Else
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
        End Sub
        Public Sub PutGenericArrayMapper(ByVal GenericMapper As Action(Of DummyType(), R))
            PutGenericArrayMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub
        Public Sub PutGenericArrayMapper(ByVal GenericMapper As Action(Of DummyType(,), R))
            PutGenericArrayMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub

        Public Sub PutGenericListMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
            Dim DummyListType = FuncType.GetGenericArguments()(0)
            If Not DummyListType.IsCollectionType() Then Throw New ArgumentException
            If DummyListType.GetGenericArguments(0) IsNot GetType(DummyType) Then Throw New ArgumentException
            Dim ListType = DummyListType.GetGenericTypeDefinition

            Dim Gen =
                Function(ElementType As Type) As [Delegate]
                    Return GenericMapper.MakeDelegateMethodFromDummy(ElementType)
                End Function

            If ListMapperGeneratorCache.ContainsKey(ListType) Then
                ListMapperGeneratorCache(ListType) = Gen
            Else
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
        End Sub
        Public Sub PutGenericListMapper(Of DList As ICollection(Of DummyType))(ByVal GenericMapper As Action(Of DList, R))
            PutGenericListMapper(DirectCast(GenericMapper, [Delegate]))
        End Sub

        Public Overridable Function TryGetArrayMapperGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
            If Not ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                If Dimension <> 1 Then Return Nothing
                Dim m = DirectCast(AddressOf ArrayToList(Of DummyType), Func(Of Action(Of DummyType(), R)))
                Dim Gen = Function(ElementType As Type) DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                ArrayMapperGeneratorCache.Add(Dimension, Gen)
            End If
            Return ArrayMapperGeneratorCache(Dimension)
        End Function
        Public Overridable Function TryGetListMapperGenerator(ByVal ListType As Type) As Func(Of Type, [Delegate])
            If Not ListMapperGeneratorCache.ContainsKey(ListType) Then
                If Not ListType.IsCollectionType() Then Throw New ArgumentException
                If Not ListType.IsGenericTypeDefinition Then Throw New ArgumentException
                If ListType.GetGenericArguments().Length <> 1 Then Return Nothing
                Dim DummyListType = ListType.MakeGenericType(GetType(DummyType))

                Dim DummyMethod = DirectCast(AddressOf Inner.ResolveAggregator(Of DummyType, List(Of DummyType)), Func(Of Action(Of List(Of DummyType), R)))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                Dim Gen = Function(ElementType As Type) DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                ListMapperGeneratorCache.Add(ListType, Gen)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Private Function ArrayToList(Of D)() As Action(Of D(), R)
            Dim Mapper = Inner.ResolveAggregator(Of D, List(Of D))()
            Return Sub(List As D(), Value As R) Mapper(List.ToList(), Value)
        End Function

        Private Inner As IGenericListAggregatorResolver(Of R)
        Public Sub New(ByVal Inner As IGenericListAggregatorResolver(Of R))
            Me.Inner = Inner
        End Sub
    End Class

    Public Class RecordPacker(Of R)
        Implements IObjectAggregatorResolver

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IObjectAggregatorResolver.TryResolveAggregator
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
