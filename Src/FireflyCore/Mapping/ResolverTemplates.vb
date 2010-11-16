'==========================================================================
'
'  File:        ObjectMapperResolvers.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射器解析器
'  Version:     2010.11.16.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Diagnostics

Namespace Mapping
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IGenericCollectionProjectorResolver(Of D)
        Function ResolveProjector(Of R, RCollection As {New, ICollection(Of R)})() As Func(Of D, RCollection)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IGenericCollectionAggregatorResolver(Of R)
        Function ResolveAggregator(Of D, DCollection As ICollection(Of D))() As Action(Of DCollection, R)
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IFieldOrPropertyProjectorResolver(Of D)
        ''' <returns>返回Func(Of ${DomainType}, ${FieldOrPropertyType})</returns>
        Function ResolveProjector(ByVal Info As FieldOrPropertyInfo) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IFieldOrPropertyAggregatorResolver(Of R)
        ''' <returns>返回Action(Of ${FieldOrPropertyType}, ${RangeType})</returns>
        Function ResolveAggregator(ByVal Info As FieldOrPropertyInfo) As [Delegate]
    End Interface

    <DebuggerNonUserCode()>
    Public Class CollectionUnpackerTemplate(Of D)
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
            If RangeType.IsProperCollectionType() Then
                Dim CollectionMapperGen = TryGetCollectionMapperGenerator(RangeType.GetGenericTypeDefinition)
                If CollectionMapperGen IsNot Nothing Then
                    Return CollectionMapperGen(RangeType.GetGenericArguments())
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private CollectionMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type(), [Delegate]))

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

        Public Sub PutGenericCollectionMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Func(Of ,)) Then Throw New ArgumentException
            Dim DummyCollectionType = FuncType.GetGenericArguments()(1)
            If Not DummyCollectionType.IsProperCollectionType() Then Throw New ArgumentException
            Dim CollectionType = DummyCollectionType.GetGenericTypeDefinition

            Dim Gen =
                Function(TypeArguments As Type()) As [Delegate]
                    Dim ConcreteCollectionType = CollectionType.MakeGenericType(TypeArguments)
                    Dim ElementType = ConcreteCollectionType.GetCollectionElementType()
                    Return GenericMapper.MakeDelegateMethodFromDummy(ElementType)
                End Function
            If CollectionMapperGeneratorCache.ContainsKey(CollectionType) Then
                CollectionMapperGeneratorCache(CollectionType) = Gen
            Else
                CollectionMapperGeneratorCache.Add(CollectionType, Gen)
            End If
        End Sub
        Public Sub PutGenericCollectionMapper(Of RCollection As {New, ICollection(Of DummyType)})(ByVal GenericMapper As Func(Of D, RCollection))
            PutGenericCollectionMapper(DirectCast(GenericMapper, [Delegate]))
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
        Public Function TryGetCollectionMapperGenerator(ByVal CollectionType As Type) As Func(Of Type(), [Delegate])
            If Not CollectionMapperGeneratorCache.ContainsKey(CollectionType) Then
                If Not CollectionType.IsProperCollectionType() Then Throw New ArgumentException
                If Not CollectionType.IsGenericTypeDefinition Then Throw New ArgumentException
                Dim Gen =
                    Function(TypeArguments As Type()) As [Delegate]
                        Dim ConcreteCollectionType = CollectionType.MakeGenericType(TypeArguments)
                        Dim ElementType = ConcreteCollectionType.GetCollectionElementType()
                        Dim DummyMethod = DirectCast(AddressOf Inner.ResolveProjector(Of DummyType, List(Of DummyType)), Func(Of Func(Of D, List(Of DummyType))))
                        Dim m = DummyMethod.MakeDelegateMethodFromDummy(
                            Function(Type As Type) As Type
                                Select Case Type
                                    Case GetType(DummyType)
                                        Return ElementType
                                    Case GetType(List(Of DummyType))
                                        Return ConcreteCollectionType
                                    Case Else
                                        Return Type
                                End Select
                            End Function
                        )
                        Return DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                    End Function
                CollectionMapperGeneratorCache.Add(CollectionType, Gen)
            End If
            Return CollectionMapperGeneratorCache(CollectionType)
        End Function

        Private Function ArrayToList(Of R)() As Func(Of D, R())
            Dim Mapper = Inner.ResolveProjector(Of R, List(Of R))()
            Return Function(Key As D) As R()
                       Dim l = Mapper(Key)
                       If l Is Nothing Then Return Nothing
                       Return l.ToArray()
                   End Function
        End Function

        Private Inner As IGenericCollectionProjectorResolver(Of D)
        Public Sub New(ByVal Inner As IGenericCollectionProjectorResolver(Of D))
            Me.Inner = Inner
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class CollectionPackerTemplate(Of R)
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
            If DomainType.IsProperCollectionType() Then
                Dim CollectionMapperGen = TryGetCollectionMapperGenerator(DomainType.GetGenericTypeDefinition)
                If CollectionMapperGen IsNot Nothing Then
                    Return CollectionMapperGen(DomainType.GetGenericArguments())
                End If
            End If
            Return Nothing
        End Function

        Private ArrayMapperGeneratorCache As New Dictionary(Of Integer, Func(Of Type, [Delegate]))
        Private CollectionMapperGeneratorCache As New Dictionary(Of Type, Func(Of Type(), [Delegate]))

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

        Public Sub PutGenericCollectionMapper(ByVal GenericMapper As [Delegate])
            Dim FuncType = GenericMapper.GetType()
            If Not FuncType.IsGenericType Then Throw New ArgumentException
            If FuncType.GetGenericTypeDefinition IsNot GetType(Action(Of ,)) Then Throw New ArgumentException
            Dim DummyCollectionType = FuncType.GetGenericArguments()(0)
            If Not DummyCollectionType.IsProperCollectionType() Then Throw New ArgumentException
            Dim CollectionType = DummyCollectionType.GetGenericTypeDefinition

            Dim Gen =
                Function(TypeArguments As Type()) As [Delegate]
                    Dim ConcreteCollectionType = CollectionType.MakeGenericType(TypeArguments)
                    Dim ElementType = ConcreteCollectionType.GetCollectionElementType()
                    Return GenericMapper.MakeDelegateMethodFromDummy(ElementType)
                End Function

            If CollectionMapperGeneratorCache.ContainsKey(CollectionType) Then
                CollectionMapperGeneratorCache(CollectionType) = Gen
            Else
                CollectionMapperGeneratorCache.Add(CollectionType, Gen)
            End If
        End Sub
        Public Sub PutGenericCollectionMapper(Of DCollection As ICollection(Of DummyType))(ByVal GenericMapper As Action(Of DCollection, R))
            PutGenericCollectionMapper(DirectCast(GenericMapper, [Delegate]))
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
        Public Overridable Function TryGetCollectionMapperGenerator(ByVal CollectionType As Type) As Func(Of Type(), [Delegate])
            If Not CollectionMapperGeneratorCache.ContainsKey(CollectionType) Then
                If Not CollectionType.IsProperCollectionType() Then Throw New ArgumentException
                If Not CollectionType.IsGenericTypeDefinition Then Throw New ArgumentException

                Dim Gen =
                    Function(TypeArguments As Type()) As [Delegate]
                        Dim ConcreteCollectionType = CollectionType.MakeGenericType(TypeArguments)
                        Dim ElementType = ConcreteCollectionType.GetCollectionElementType()
                        Dim DummyMethod = DirectCast(AddressOf Inner.ResolveAggregator(Of DummyType, List(Of DummyType)), Func(Of Action(Of List(Of DummyType), R)))
                        Dim m = DummyMethod.MakeDelegateMethodFromDummy(
                            Function(Type As Type) As Type
                                Select Case Type
                                    Case GetType(DummyType)
                                        Return ElementType
                                    Case GetType(List(Of DummyType))
                                        Return ConcreteCollectionType
                                    Case Else
                                        Return Type
                                End Select
                            End Function
                        )
                        Return DirectCast(m.MakeDelegateMethodFromDummy(ElementType).DynamicInvoke(), [Delegate])
                    End Function
                CollectionMapperGeneratorCache.Add(CollectionType, Gen)
            End If
            Return CollectionMapperGeneratorCache(CollectionType)
        End Function

        Private Function ArrayToList(Of D)() As Action(Of D(), R)
            Dim Mapper = Inner.ResolveAggregator(Of D, List(Of D))()
            Return Sub(Arr As D(), Value As R)
                       If Arr Is Nothing Then
                           Mapper(Nothing, Value)
                       Else
                           Mapper(Arr.ToList(), Value)
                       End If
                   End Sub
        End Function

        Private Inner As IGenericCollectionAggregatorResolver(Of R)
        Public Sub New(ByVal Inner As IGenericCollectionAggregatorResolver(Of R))
            Me.Inner = Inner
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class RecordUnpackerTemplate(Of D)
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

                Dim dParam = Expression.Variable(DomainType, "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    DelegateCalls.Add(CreatePair(Inner.ResolveProjector(Pair), New Expression() {dParam}))
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

        Private Inner As IFieldOrPropertyProjectorResolver(Of D)
        Public Sub New(ByVal Inner As IFieldOrPropertyProjectorResolver(Of D))
            Me.Inner = Inner
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class RecordPackerTemplate(Of R)
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
                Dim rParam = Expression.Variable(RangeType, "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, Pair.Member)
                    DelegateCalls.Add(New KeyValuePair(Of [Delegate], Expression())(Inner.ResolveAggregator(Pair), New Expression() {FieldOrPropertyExpr, rParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim Block As Expression
                If DelegateCalls.Count > 0 Then
                    Block = Expression.Block(Context.DelegateExpressions)
                Else
                    Block = Expression.Empty
                End If
                Dim FunctionLambda = Expression.Lambda(GetType(Action(Of ,)).MakeGenericType(DomainType, RangeType), Block, New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Inner As IFieldOrPropertyAggregatorResolver(Of R)
        Public Sub New(ByVal Inner As IFieldOrPropertyAggregatorResolver(Of R))
            Me.Inner = Inner
        End Sub
    End Class
End Namespace
