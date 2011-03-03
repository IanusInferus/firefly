'==========================================================================
'
'  File:        ResolverTemplates.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: Object映射器解析器
'  Version:     2011.03.03.
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
    Public Interface IFieldProjectorResolver(Of D)
        ''' <returns>返回Func(Of ${DomainType}, ${FieldOrPropertyType})</returns>
        Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IFieldAggregatorResolver(Of R)
        ''' <returns>返回Action(Of ${FieldOrPropertyType}, ${RangeType})</returns>
        Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IAliasFieldProjectorResolver(Of D)
        ''' <returns>返回Func(Of ${DomainType}, ${FieldOrPropertyType})</returns>
        Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface IAliasFieldAggregatorResolver(Of R)
        ''' <returns>返回Action(Of ${FieldOrPropertyType}, ${RangeType})</returns>
        Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ITagProjectorResolver(Of D)
        ''' <returns>返回Func(Of ${DomainType}, ${TagType})</returns>
        Function ResolveProjector(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ITagAggregatorResolver(Of R)
        ''' <returns>返回Action(Of ${TagType}, ${RangeType})</returns>
        Function ResolveAggregator(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ITupleElementProjectorResolver(Of D)
        ''' <returns>返回Func(Of ${DomainType}, ${Type})</returns>
        Function ResolveProjector(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate]
    End Interface
    ''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
    Public Interface ITupleElementAggregatorResolver(Of R)
        ''' <returns>返回Action(Of ${Type}, ${RangeType})</returns>
        Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate]
    End Interface

    <DebuggerNonUserCode()>
    Public Class CollectionUnpackerTemplate(Of D)
        Implements IProjectorResolver

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
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
                Dim Gen = Function(ElementType As Type) m.MakeDelegateMethodFromDummy(ElementType).StaticDynamicInvoke(Of [Delegate])()
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
                                If Type Is GetType(DummyType) Then Return ElementType
                                If Type Is GetType(List(Of DummyType)) Then Return ConcreteCollectionType
                                Return Type
                            End Function
                        )
                        Return m.MakeDelegateMethodFromDummy(ElementType).StaticDynamicInvoke(Of [Delegate])()
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
        Implements IAggregatorResolver

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
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

        Public Function TryGetArrayMapperGenerator(ByVal Dimension As Integer) As Func(Of Type, [Delegate])
            If Not ArrayMapperGeneratorCache.ContainsKey(Dimension) Then
                If Dimension <> 1 Then Return Nothing
                Dim m = DirectCast(AddressOf ArrayToList(Of DummyType), Func(Of Action(Of DummyType(), R)))
                Dim Gen = Function(ElementType As Type) m.MakeDelegateMethodFromDummy(ElementType).StaticDynamicInvoke(Of [Delegate])()
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
                        Dim DummyMethod = DirectCast(AddressOf Inner.ResolveAggregator(Of DummyType, List(Of DummyType)), Func(Of Action(Of List(Of DummyType), R)))
                        Dim m = DummyMethod.MakeDelegateMethodFromDummy(
                            Function(Type As Type) As Type
                                If Type Is GetType(DummyType) Then Return ElementType
                                If Type Is GetType(List(Of DummyType)) Then Return ConcreteCollectionType
                                Return Type
                            End Function
                        )
                        Return m.MakeDelegateMethodFromDummy(ElementType).StaticDynamicInvoke(Of [Delegate])()
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
        Implements IProjectorResolver

        Public Function TryResolveProjector(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IProjectorResolver.TryResolveProjector
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If DomainType IsNot GetType(D) Then Return Nothing
            With Nothing
                Dim d = TryResolveAlias(RangeType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveTaggedUnion(RangeType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveTuple(RangeType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveRecord(RangeType)
                If d IsNot Nothing Then Return d
            End With
            Return Nothing
        End Function

        Private Function TryResolveAlias(ByVal RangeType As Type) As [Delegate]
            Dim DomainType = GetType(D)
            If RangeType.IsValueType OrElse RangeType.IsClass Then
                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = RangeType.TryGetMutableAliasInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = {mri.Member}
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    DelegateCalls.Add(CreatePair(AliasFieldResolver.ResolveProjector(Pair.Member, Pair.Type), New Expression() {dParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                Dim CreateThis = Expression.[New](RangeType)
                Dim MemberBindings As New List(Of MemberBinding)
                For Each Pair In FieldsAndProperties.ZipStrict(Context.DelegateExpressions, Function(m, e) New With {.Member = m.Member, .MapperCall = e})
                    MemberBindings.Add(Expression.Bind(Pair.Member, Pair.MapperCall))
                Next
                Dim FunctionLambda = Expression.Lambda(Expression.MemberInit(CreateThis, MemberBindings.ToArray()), New ParameterExpression() {dParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Class CallClosure
            Public CallDelegate As Func(Of [Delegate])
            Public Function Invoke(Of CD, CR)(ByVal Key As CD) As CR
                Return DirectCast(CallDelegate(), Func(Of CD, CR))(Key)
            End Function
        End Class
        Private Function TryResolveTaggedUnion(ByVal RangeType As Type) As [Delegate]
            Dim DomainType = GetType(D)
            If RangeType.IsValueType OrElse RangeType.IsClass Then
                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                Dim TagMember As FieldOrPropertyInfo = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = RangeType.TryGetMutableTaggedUnionInfo()
                    If mri IsNot Nothing Then
                        FieldsAndProperties = mri.Members
                        TagMember = mri.TagMember
                    End If
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                DelegateCalls.Add(CreatePair(TagResolver.ResolveProjector(TagMember.Member, TagMember.Type), New Expression() {dParam}))
                For Each Pair In FieldsAndProperties
                    Dim p = Pair
                    Dim f = Function() FieldResolver.ResolveProjector(p.Member, p.Type)
                    Dim c As New CallClosure With {.CallDelegate = f}
                    Dim cf = DirectCast(AddressOf c.Invoke(Of DummyType, DummyType), Func(Of DummyType, DummyType)).MakeDelegateMethod({DomainType, p.Type}, GetType(Func(Of ,)).MakeGenericType(DomainType, p.Type))
                    DelegateCalls.Add(CreatePair(cf, New Expression() {dParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                Dim CreateThis = Expression.[New](RangeType)
                Dim Cases As New List(Of SwitchCase)
                Dim n = 0
                Dim EnumValues = TagMember.Type.GetEnumValues
                For Each Pair In FieldsAndProperties.ZipStrict(Context.DelegateExpressions.Skip(1), Function(m, e) New With {.Member = m.Member, .MapperCall = e})
                    Dim EnumValue = Expression.Constant(EnumValues.GetValue(n), TagMember.Type)
                    Dim Init = Expression.MemberInit(CreateThis, {Expression.Bind(TagMember.Member, EnumValue), Expression.Bind(Pair.Member, Pair.MapperCall)})
                    Cases.Add(Expression.SwitchCase(Init, EnumValue))
                    n += 1
                Next
                Dim DefaultCase = Expression.Block(Expression.Throw(Expression.[New](GetType(InvalidOperationException))), CreateThis)
                Dim SelectCase = Expression.Switch(Context.DelegateExpressions(0), DefaultCase, Cases.ToArray())
                Dim FunctionLambda = Expression.Lambda(SelectCase, New ParameterExpression() {dParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Function TryResolveTuple(ByVal RangeType As Type) As [Delegate]
            Dim DomainType = GetType(D)
            If RangeType.IsValueType OrElse RangeType.IsClass Then
                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = RangeType.TryGetMutableTupleInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = mri.Members
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                Dim n = 0
                For Each Pair In FieldsAndProperties
                    DelegateCalls.Add(CreatePair(TupleElementResolver.ResolveProjector(Pair.Member, n, Pair.Type), New Expression() {dParam}))
                    n += 1
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                Dim CreateThis = Expression.[New](RangeType)
                Dim MemberBindings As New List(Of MemberBinding)
                For Each Pair In FieldsAndProperties.ZipStrict(Context.DelegateExpressions, Function(m, e) New With {.Member = m.Member, .MapperCall = e})
                    MemberBindings.Add(Expression.Bind(Pair.Member, Pair.MapperCall))
                Next
                Dim FunctionLambda = Expression.Lambda(Expression.MemberInit(CreateThis, MemberBindings.ToArray()), New ParameterExpression() {dParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Function TryResolveRecord(ByVal RangeType As Type) As [Delegate]
            Dim DomainType = GetType(D)
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

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    DelegateCalls.Add(CreatePair(FieldResolver.ResolveProjector(Pair.Member, Pair.Type), New Expression() {dParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)

                If Constructor IsNot Nothing Then
                    Dim CreateThis = Expression.[New](Constructor, Context.DelegateExpressions)
                    Dim FunctionLambda = Expression.Lambda(CreateThis, New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                Else
                    Dim CreateThis = Expression.[New](RangeType)
                    Dim MemberBindings As New List(Of MemberBinding)
                    For Each Pair In FieldsAndProperties.ZipStrict(Context.DelegateExpressions, Function(m, e) New With {.Member = m.Member, .MapperCall = e})
                        MemberBindings.Add(Expression.Bind(Pair.Member, Pair.MapperCall))
                    Next
                    Dim FunctionLambda = Expression.Lambda(Expression.MemberInit(CreateThis, MemberBindings.ToArray()), New ParameterExpression() {dParam})

                    Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
                End If
            End If
            Return Nothing
        End Function

        Private FieldResolver As IFieldProjectorResolver(Of D)
        Private AliasFieldResolver As IAliasFieldProjectorResolver(Of D)
        Private TagResolver As ITagProjectorResolver(Of D)
        Private TupleElementResolver As ITupleElementProjectorResolver(Of D)
        Public Sub New(ByVal FieldResolver As IFieldProjectorResolver(Of D), ByVal AliasFieldResolver As IAliasFieldProjectorResolver(Of D), ByVal TagResolver As ITagProjectorResolver(Of D), ByVal TupleElementResolver As ITupleElementProjectorResolver(Of D))
            Me.FieldResolver = FieldResolver
            Me.AliasFieldResolver = AliasFieldResolver
            Me.TagResolver = TagResolver
            Me.TupleElementResolver = TupleElementResolver
        End Sub
    End Class
    <DebuggerNonUserCode()>
    Public Class RecordPackerTemplate(Of R)
        Implements IAggregatorResolver

        Public Function TryResolveAggregator(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate] Implements IAggregatorResolver.TryResolveAggregator
            Dim DomainType = TypePair.Key
            Dim RangeType = TypePair.Value
            If RangeType IsNot GetType(R) Then Return Nothing
            With Nothing
                Dim d = TryResolveAlias(DomainType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveTaggedUnion(DomainType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveTuple(DomainType)
                If d IsNot Nothing Then Return d
            End With
            With Nothing
                Dim d = TryResolveRecord(DomainType)
                If d IsNot Nothing Then Return d
            End With
            Return Nothing
        End Function

        Private Function TryResolveAlias(ByVal DomainType As Type) As [Delegate]
            Dim RangeType = GetType(R)
            If DomainType.IsValueType OrElse DomainType.IsClass Then
                If Not (DomainType.IsValueType OrElse DomainType.IsClass) Then Return Nothing

                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = DomainType.TryGetMutableAliasInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = {mri.Member}
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim rParam = Expression.Parameter(RangeType, "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, Pair.Member)
                    DelegateCalls.Add(CreatePair(AliasFieldResolver.ResolveAggregator(Pair.Member, Pair.Type), New Expression() {FieldOrPropertyExpr, rParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim Body As Expression
                If DelegateCalls.Count > 0 Then
                    Body = Expression.Block(Context.DelegateExpressions)
                Else
                    Body = Expression.Empty
                End If
                Dim FunctionLambda = Expression.Lambda(GetType(Action(Of ,)).MakeGenericType(DomainType, RangeType), Body, New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Class CallClosure
            Public CallDelegate As Func(Of [Delegate])
            Public Sub Invoke(Of CD, CR)(ByVal Key As CD, ByVal Value As CR)
                DirectCast(CallDelegate(), Action(Of CD, CR))(Key, Value)
            End Sub
        End Class
        Private Function TryResolveTaggedUnion(ByVal DomainType As Type) As [Delegate]
            Dim RangeType = GetType(R)
            If DomainType.IsValueType OrElse DomainType.IsClass Then
                If Not (DomainType.IsValueType OrElse DomainType.IsClass) Then Return Nothing

                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                Dim TagMember As FieldOrPropertyInfo = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = DomainType.TryGetMutableTaggedUnionInfo()
                    If mri IsNot Nothing Then
                        FieldsAndProperties = mri.Members
                        TagMember = mri.TagMember
                    End If
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim rParam = Expression.Parameter(RangeType, "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                Dim TagMemberExpr = CreateFieldOrPropertyExpression(dParam, TagMember.Member)
                DelegateCalls.Add(CreatePair(TagResolver.ResolveAggregator(TagMember.Member, TagMember.Type), New Expression() {TagMemberExpr, rParam}))
                For Each Pair In FieldsAndProperties
                    Dim p = Pair
                    Dim f = Function() FieldResolver.ResolveAggregator(p.Member, p.Type)
                    Dim c As New CallClosure With {.CallDelegate = f}
                    Dim cf = DirectCast(AddressOf c.Invoke(Of DummyType, DummyType), Action(Of DummyType, DummyType)).MakeDelegateMethod({p.Type, RangeType}, GetType(Action(Of ,)).MakeGenericType(p.Type, RangeType))
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, p.Member)
                    DelegateCalls.Add(CreatePair(cf, New Expression() {FieldOrPropertyExpr, rParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim Cases As New List(Of SwitchCase)
                Dim n = 0
                Dim EnumValues = TagMember.Type.GetEnumValues
                For Each Pair In FieldsAndProperties.ZipStrict(Context.DelegateExpressions.Skip(1), Function(m, e) New With {.Member = m.Member, .MapperCall = e})
                    Dim EnumValue = Expression.Constant(EnumValues.GetValue(n), TagMember.Type)
                    Cases.Add(Expression.SwitchCase(Pair.MapperCall, EnumValue))
                    n += 1
                Next
                Dim Body = Expression.Block(Context.DelegateExpressions(0), Expression.Switch(CreateFieldOrPropertyExpression(dParam, TagMember.Member), Cases.ToArray()))
                Dim FunctionLambda = Expression.Lambda(GetType(Action(Of ,)).MakeGenericType(DomainType, RangeType), Body, New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Function TryResolveTuple(ByVal DomainType As Type) As [Delegate]
            Dim RangeType = GetType(R)
            If DomainType.IsValueType OrElse DomainType.IsClass Then
                If Not (DomainType.IsValueType OrElse DomainType.IsClass) Then Return Nothing

                Dim FieldsAndProperties As FieldOrPropertyInfo() = Nothing
                If FieldsAndProperties Is Nothing Then
                    Dim mri = DomainType.TryGetMutableTupleInfo()
                    If mri IsNot Nothing Then FieldsAndProperties = mri.Members
                End If
                If FieldsAndProperties Is Nothing Then Return Nothing

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim rParam = Expression.Parameter(RangeType, "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                Dim n = 0
                For Each Pair In FieldsAndProperties
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, Pair.Member)
                    DelegateCalls.Add(CreatePair(TupleElementResolver.ResolveAggregator(Pair.Member, n, Pair.Type), New Expression() {FieldOrPropertyExpr, rParam}))
                    n += 1
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim Body As Expression
                If DelegateCalls.Count > 0 Then
                    Body = Expression.Block(Context.DelegateExpressions)
                Else
                    Body = Expression.Empty
                End If
                Dim FunctionLambda = Expression.Lambda(GetType(Action(Of ,)).MakeGenericType(DomainType, RangeType), Body, New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private Function TryResolveRecord(ByVal DomainType As Type) As [Delegate]
            Dim RangeType = GetType(R)
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

                Dim dParam = Expression.Parameter(DomainType, "Key")
                Dim rParam = Expression.Parameter(RangeType, "Value")
                Dim DelegateCalls As New List(Of KeyValuePair(Of [Delegate], Expression()))
                For Each Pair In FieldsAndProperties
                    Dim FieldOrPropertyExpr = CreateFieldOrPropertyExpression(dParam, Pair.Member)
                    DelegateCalls.Add(CreatePair(FieldResolver.ResolveAggregator(Pair.Member, Pair.Type), New Expression() {FieldOrPropertyExpr, rParam}))
                Next
                Dim Context = CreateDelegateExpressionContext(DelegateCalls)
                Dim Body As Expression
                If DelegateCalls.Count > 0 Then
                    Body = Expression.Block(Context.DelegateExpressions)
                Else
                    Body = Expression.Empty
                End If
                Dim FunctionLambda = Expression.Lambda(GetType(Action(Of ,)).MakeGenericType(DomainType, RangeType), Body, New ParameterExpression() {dParam, rParam})

                Return CreateDelegate(Context.ClosureParam, Context.Closure, FunctionLambda)
            End If
            Return Nothing
        End Function

        Private FieldResolver As IFieldAggregatorResolver(Of R)
        Private AliasFieldResolver As IAliasFieldAggregatorResolver(Of R)
        Private TagResolver As ITagAggregatorResolver(Of R)
        Private TupleElementResolver As ITupleElementAggregatorResolver(Of R)
        Public Sub New(ByVal FieldResolver As IFieldAggregatorResolver(Of R), ByVal AliasFieldResolver As IAliasFieldAggregatorResolver(Of R), ByVal TagResolver As ITagAggregatorResolver(Of R), ByVal TupleElementResolver As ITupleElementAggregatorResolver(Of R))
            Me.FieldResolver = FieldResolver
            Me.AliasFieldResolver = AliasFieldResolver
            Me.TagResolver = TagResolver
            Me.TupleElementResolver = TupleElementResolver
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class FieldProjectorResolver(Of D)
        Implements IFieldProjectorResolver(Of D)

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IFieldProjectorResolver(Of D).ResolveProjector
            Return InnerResolver.ResolveProjector(CreatePair(GetType(D), Type))
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    <DebuggerNonUserCode()>
    Public Class FieldAggregatorResolver(Of R)
        Implements IFieldAggregatorResolver(Of R)

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IFieldAggregatorResolver(Of R).ResolveAggregator
            Return InnerResolver.ResolveAggregator(CreatePair(Type, GetType(R)))
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class AliasFieldProjectorResolver(Of D)
        Implements IAliasFieldProjectorResolver(Of D)

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IAliasFieldProjectorResolver(Of D).ResolveProjector
            Return InnerResolver.ResolveProjector(CreatePair(GetType(D), Type))
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    <DebuggerNonUserCode()>
    Public Class AliasFieldAggregatorResolver(Of R)
        Implements IAliasFieldAggregatorResolver(Of R)

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Type As Type) As [Delegate] Implements IAliasFieldAggregatorResolver(Of R).ResolveAggregator
            Return InnerResolver.ResolveAggregator(CreatePair(Type, GetType(R)))
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class TagProjectorResolver(Of D)
        Implements ITagProjectorResolver(Of D)

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate] Implements ITagProjectorResolver(Of D).ResolveProjector
            Return InnerResolver.ResolveProjector(CreatePair(GetType(D), TagType))
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    <DebuggerNonUserCode()>
    Public Class TagAggregatorResolver(Of R)
        Implements ITagAggregatorResolver(Of R)

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal TagType As Type) As [Delegate] Implements ITagAggregatorResolver(Of R).ResolveAggregator
            Return InnerResolver.ResolveAggregator(CreatePair(TagType, GetType(R)))
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class

    <DebuggerNonUserCode()>
    Public Class TupleElementProjectorResolver(Of D)
        Implements ITupleElementProjectorResolver(Of D)

        Public Function ResolveProjector(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate] Implements ITupleElementProjectorResolver(Of D).ResolveProjector
            Return InnerResolver.ResolveProjector(CreatePair(GetType(D), Type))
        End Function

        Private InnerResolver As IProjectorResolver
        Public Sub New(ByVal Resolver As IProjectorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
    <DebuggerNonUserCode()>
    Public Class TupleElementAggregatorResolver(Of R)
        Implements ITupleElementAggregatorResolver(Of R)

        Public Function ResolveAggregator(ByVal Member As MemberInfo, ByVal Index As Integer, ByVal Type As Type) As [Delegate] Implements ITupleElementAggregatorResolver(Of R).ResolveAggregator
            Return InnerResolver.ResolveAggregator(CreatePair(Type, GetType(R)))
        End Function

        Private InnerResolver As IAggregatorResolver
        Public Sub New(ByVal Resolver As IAggregatorResolver)
            Me.InnerResolver = Resolver.AsNoncircular
        End Sub
    End Class
End Namespace
