'==========================================================================
'
'  File:        ObjectMapper.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: Object映射
'  Version:     2010.11.12.
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

Public Interface IObjectMapperResolver
    ''' <param name="TypePair">(DomainType, RangeType)</param>
    ''' <returns>返回Func(Of ${DomainType}, ${RangeType})</returns>
    Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
End Interface

Public Interface IObjectOneToManyMapperResolver(Of D)
    ''' <returns>返回Func(Of ${DomainType}, ${RangeType})</returns>
    Function TryResolve(ByVal RangeType As Type) As [Delegate]
End Interface

Public Interface IObjectManyToOneMapperResolver(Of R)
    ''' <returns>返回Action(Of ${DomainType}, ${RangeType})</returns>
    Function TryResolve(ByVal DomainType As Type) As [Delegate]
End Interface

''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
Public Interface ICollectionOneToManyMapperResolverDefaultProvider(Of D)
    Function DefaultArrayMapper(Of R)(ByVal Key As D) As R()
    Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList
End Interface

''' <remarks>实现带泛型约束的接口会导致代码分析无效。</remarks>
Public Interface ICollectionMapperResolverDefaultProvider(Of R)
    Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R)
    Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R)
End Interface

Public Class ObjectMapper
    Private MapperCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
    Private ResolversValue As New List(Of IObjectMapperResolver)
    Public ReadOnly Property Resolvers As List(Of IObjectMapperResolver)
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectMapper, IEnumerable(Of IObjectMapperResolver)))
        Me.ResolversValue = GetResolvers(Me).ToList()
    End Sub

    Public Sub PutMapper(ByVal TypePair As KeyValuePair(Of Type, Type), ByVal Mapper As [Delegate])
        If MapperCache.ContainsKey(TypePair) Then
            MapperCache(TypePair) = Mapper
        Else
            MapperCache.Add(TypePair, Mapper)
        End If
    End Sub
    Public Sub PutMapper(ByVal DomainType As Type, ByVal RangeType As Type, ByVal Mapper As [Delegate])
        PutMapper(New KeyValuePair(Of Type, Type)(DomainType, RangeType), Mapper)
    End Sub
    Public Sub PutMapper(Of D, R)(ByVal Mapper As Func(Of D, R))
        PutMapper(GetType(D), GetType(R), Mapper)
    End Sub

    Public Function GetMapper(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
        If MapperCache.ContainsKey(TypePair) Then Return MapperCache(TypePair)
        For Each r In ResolversValue
            Dim Resolved = r.TryResolve(TypePair)
            If Resolved IsNot Nothing Then
                MapperCache.Add(TypePair, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: ({0}, {1})".Formats(TypePair.Key.FullName, TypePair.Value.FullName))
    End Function
    Public Function GetMapper(ByVal DomainType As Type, ByVal RangeType As Type) As [Delegate]
        Return GetMapper(New KeyValuePair(Of Type, Type)(DomainType, RangeType))
    End Function
    Public Function GetMapper(Of D, R)() As Func(Of D, R)
        Return DirectCast(GetMapper(GetType(D), GetType(R)), Func(Of D, R))
    End Function

    Public Function Map(Of D, R)(ByVal Key As D) As R
        Return GetMapper(Of D, R)()(Key)
    End Function
End Class

Public Class ObjectOneToManyMapper(Of D)
    Private MapperCache As New Dictionary(Of Type, [Delegate])
    Private ResolversValue As New List(Of IObjectOneToManyMapperResolver(Of D))
    Public ReadOnly Property Resolvers As List(Of IObjectOneToManyMapperResolver(Of D))
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectOneToManyMapper(Of D), IEnumerable(Of IObjectOneToManyMapperResolver(Of D))))
        Me.ResolversValue = GetResolvers(Me).ToList()
    End Sub

    Public Sub PutMapper(ByVal RangeType As Type, ByVal Mapper As [Delegate])
        If MapperCache.ContainsKey(RangeType) Then
            MapperCache(RangeType) = Mapper
        Else
            MapperCache.Add(RangeType, Mapper)
        End If
    End Sub
    Public Sub PutMapper(Of R)(ByVal Mapper As Func(Of D, R))
        PutMapper(GetType(R), Mapper)
    End Sub

    Public Function GetMapper(ByVal RangeType As Type) As [Delegate]
        If MapperCache.ContainsKey(RangeType) Then Return MapperCache(RangeType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolve(RangeType)
            If Resolved IsNot Nothing Then
                MapperCache.Add(RangeType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(RangeType.FullName))
    End Function
    Public Function GetMapper(Of R)() As Func(Of D, R)
        Return DirectCast(GetMapper(GetType(R)), Func(Of D, R))
    End Function

    Public Function Map(Of R)(ByVal Key As D) As R
        Return GetMapper(Of R)()(Key)
    End Function

    Public Class EnumMapperResolver
        Implements IObjectOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectOneToManyMapperResolver(Of D).TryResolve
            If PhysicalType.IsEnum Then
                Dim UnderlyingType = PhysicalType.GetEnumUnderlyingType
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(UnderlyingType)

                Dim ClosureParam As ParameterExpression = Nothing
                Dim dParam = Expression.Variable(GetType(D), "Key")

                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, dParam)
                Else
                    ClosureParam = Expression.Variable(Mapper.GetType(), "<>_Closure")
                    MapperCall = Expression.Invoke(ClosureParam, dParam)
                End If

                Dim FunctionBody = Expression.ConvertChecked(MapperCall, PhysicalType)
                Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam})
                If Mapper.Target IsNot Nothing Then
                    FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
                End If

                Dim Compiled As [Delegate] = FunctionLambda.Compile()
                If Mapper.Target IsNot Nothing Then
                    Compiled = CType(Compiled.DynamicInvoke(Mapper), [Delegate])
                End If

                Return Compiled
            End If
            Return Nothing
        End Function

        Private Map As Func(Of D, DummyType)
        Public Sub New(ByVal Map As Func(Of D, DummyType))
            Me.Map = Map
        End Sub
    End Class

    Public Class CollectionMapperResolver
        Implements IObjectOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectOneToManyMapperResolver(Of D).TryResolve
            If PhysicalType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(PhysicalType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(PhysicalType.GetElementType)
                End If
            End If
            If PhysicalType.IsListType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(PhysicalType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(PhysicalType.GetGenericArguments(0))
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

        Private Provider As ICollectionOneToManyMapperResolverDefaultProvider(Of D)
        Public Sub New(ByVal Provider As ICollectionOneToManyMapperResolverDefaultProvider(Of D))
            Me.Provider = Provider
        End Sub
    End Class

    Public Class ClassAndStructureMapperResolver
        Implements IObjectOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectOneToManyMapperResolver(Of D).TryResolve
            If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
                Dim Resolved = TryParametricConstructor(PhysicalType)
                If Resolved IsNot Nothing Then Return Resolved
                Resolved = TryNonparametricConstructor(PhysicalType)
                If Resolved IsNot Nothing Then Return Resolved
            End If
            Return Nothing
        End Function

        Public Function TryParametricConstructor(ByVal PhysicalType As Type) As [Delegate]
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then Return Nothing

            Dim ReadableAndWritableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim WritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim ReadableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) f.IsInitOnly).ToArray
            Dim ReadableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso Not p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray

            If ReadableAndWritableFields.Count > 0 Then Return Nothing
            If WritableProperties.Count > 0 Then Return Nothing
            If ReadableFields.Length > 0 AndAlso ReadableProperties.Length > 0 Then Return Nothing

            Dim dParam = Expression.Variable(GetType(D), "Key")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim FieldMembers = ReadableFields.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableProperties.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim FieldAndPropertyTypes = FieldsAndProperties.Select(Function(f) f.Type).ToArray
            Dim c = PhysicalType.GetConstructor(FieldAndPropertyTypes)
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Dim Expressions As New List(Of Expression)

            Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
            Dim MapperToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToMapper.ContainsKey(Type) Then Continue For
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(Type)
                TypeToMapper.Add(Type, Mapper)
                If Mapper.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    MapperToClosureField.Add(Mapper, n)
                    ClosureObjects.Add(Mapper)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                Dim Mapper = TypeToMapper(Type)
                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, dParam)
                Else
                    Dim n = MapperToClosureField(Mapper)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Func(Of ,)).MakeGenericType(GetType(D), Type)
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    MapperCall = Expression.Invoke(DelegateFunc, dParam)
                End If
                Expressions.Add(MapperCall)
            Next
            Dim CreateThis = Expression.[New](c, Expressions)

            Dim FunctionBody = Expression.Block(New ParameterExpression() {}, CreateThis)
            Dim FunctionLambda As LambdaExpression = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled As [Delegate] = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If

            Return Compiled
        End Function

        Public Function TryNonparametricConstructor(ByVal PhysicalType As Type) As [Delegate]
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then Return Nothing

            If PhysicalType.IsClass Then
                Dim c = PhysicalType.GetConstructor(New Type() {})
                If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
            End If

            Dim ReadableAndWritableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim ReadableAndWritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim WritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            If Not ((ReadableAndWritableFields.Length > 0 AndAlso WritableProperties.Length = 0) OrElse (ReadableAndWritableFields.Length = 0 AndAlso ReadableAndWritableProperties.Length > 0)) Then Return Nothing

            Dim dParam = Expression.Variable(GetType(D), "Key")
            Dim rParam = Expression.Variable(PhysicalType, "Value")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)
            Dim CreateThis = Expression.Assign(rParam, Expression.[New](PhysicalType))
            Statements.Add(CreateThis)

            Dim FieldMembers = ReadableAndWritableFields.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(rParam, f), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableAndWritableProperties.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(rParam, f), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
            Dim MapperToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToMapper.ContainsKey(Type) Then Continue For
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(Type)
                TypeToMapper.Add(Type, Mapper)
                If Mapper.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    MapperToClosureField.Add(Mapper, n)
                    ClosureObjects.Add(Mapper)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Mapper = TypeToMapper(Type)
                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, dParam)
                Else
                    Dim n = MapperToClosureField(Mapper)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Func(Of ,)).MakeGenericType(GetType(D), Type)
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    MapperCall = Expression.Invoke(DelegateFunc, dParam)
                End If
                Dim Assign = Expression.Assign(FieldOrPropertyExpr, MapperCall)
                Statements.Add(Assign)
            Next
            Statements.Add(rParam)

            Dim FunctionBody = Expression.Block(New ParameterExpression() {rParam}, Statements)
            Dim FunctionLambda As LambdaExpression = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled As [Delegate] = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If

            Return Compiled
        End Function

        Private Map As Func(Of D, DummyType)
        Public Sub New(ByVal Map As Func(Of D, DummyType))
            Me.Map = Map
        End Sub
    End Class
End Class

Public Class ObjectManyToOneMapper(Of R)
    Private MapperCache As New Dictionary(Of Type, [Delegate])
    Private ResolversValue As New List(Of IObjectManyToOneMapperResolver(Of R))
    Public ReadOnly Property Resolvers As List(Of IObjectManyToOneMapperResolver(Of R))
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectManyToOneMapper(Of R), IEnumerable(Of IObjectManyToOneMapperResolver(Of R))))
        Me.ResolversValue = GetResolvers(Me).ToList()
    End Sub

    Public Sub PutMapper(ByVal DomainType As Type, ByVal Mapper As [Delegate])
        If MapperCache.ContainsKey(DomainType) Then
            MapperCache(DomainType) = Mapper
        Else
            MapperCache.Add(DomainType, Mapper)
        End If
    End Sub
    Public Sub PutMapper(Of D)(ByVal Mapper As Action(Of D, R))
        PutMapper(GetType(D), Mapper)
    End Sub

    Public Function GetMapper(ByVal DomainType As Type) As [Delegate]
        If MapperCache.ContainsKey(DomainType) Then Return MapperCache(DomainType)
        For Each r In Resolvers
            Dim Resolved = r.TryResolve(DomainType)
            If Resolved IsNot Nothing Then
                MapperCache.Add(DomainType, Resolved)
                Return Resolved
            End If
        Next
        Throw New NotSupportedException("NotResolved: {0}".Formats(DomainType.FullName))
    End Function
    Public Function GetMapper(Of D)() As Action(Of D, R)
        Return DirectCast(GetMapper(GetType(D)), Action(Of D, R))
    End Function

    Public Sub Map(Of D)(ByVal Key As D, ByVal Value As R)
        GetMapper(Of D)()(Key, Value)
    End Sub

    Public Class EnumMapperResolver
        Implements IObjectManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectManyToOneMapperResolver(Of R).TryResolve
            If PhysicalType.IsEnum Then
                Dim UnderlyingType = PhysicalType.GetEnumUnderlyingType
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(UnderlyingType)

                Dim ClosureParam As ParameterExpression = Nothing
                Dim dParam = Expression.Variable(PhysicalType, "Key")
                Dim rParam = Expression.Variable(GetType(R), "Value")

                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, Expression.ConvertChecked(dParam, UnderlyingType), rParam)
                Else
                    ClosureParam = Expression.Variable(Mapper.GetType(), "<>_Closure")
                    MapperCall = Expression.Invoke(ClosureParam, Expression.ConvertChecked(dParam, UnderlyingType), rParam)
                End If

                Dim FunctionBody = MapperCall
                Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam, rParam})
                If Mapper.Target IsNot Nothing Then
                    FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
                End If

                Dim Compiled As [Delegate] = FunctionLambda.Compile()
                If Mapper.Target IsNot Nothing Then
                    Compiled = CType(Compiled.DynamicInvoke(Mapper), [Delegate])
                End If

                Return Compiled
            End If
            Return Nothing
        End Function

        Private Map As Action(Of DummyType, R)
        Public Sub New(ByVal Map As Action(Of DummyType, R))
            Me.Map = Map
        End Sub
    End Class

    Public Class CollectionMapperResolver
        Implements IObjectManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectManyToOneMapperResolver(Of R).TryResolve
            If PhysicalType.IsArray Then
                Dim ArrayMapperGen = TryGetArrayMapperGenerator(PhysicalType.GetArrayRank)
                If ArrayMapperGen IsNot Nothing Then
                    Return ArrayMapperGen(PhysicalType.GetElementType)
                End If
            End If
            If PhysicalType.IsListType() Then
                Dim ListMapperGen = TryGetListMapperGenerator(PhysicalType.GetGenericTypeDefinition)
                If ListMapperGen IsNot Nothing Then
                    Return ListMapperGen(PhysicalType.GetGenericArguments(0))
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
        Public Sub PutArrayMapperGenerator(ByVal Generator As Action(Of R(), DummyType))
            PutArrayMapperGenerator(DirectCast(Generator, [Delegate]))
        End Sub
        Public Sub PutArrayMapperGenerator(ByVal Generator As Action(Of R(,), DummyType))
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
        Public Sub PutListMapperGenerator(Of DList As ICollection(Of DummyType))(ByVal Generator As Func(Of DList, R))
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

        Private Provider As ICollectionMapperResolverDefaultProvider(Of R)
        Public Sub New(ByVal Provider As ICollectionMapperResolverDefaultProvider(Of R))
            Me.Provider = Provider
        End Sub
    End Class

    Public Class ClassAndStructureMapperResolver
        Implements IObjectManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectManyToOneMapperResolver(Of R).TryResolve
            If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
                Dim Resolved = TryParametricConstructor(PhysicalType)
                If Resolved IsNot Nothing Then Return Resolved
                Resolved = TryNonparametricConstructor(PhysicalType)
                If Resolved IsNot Nothing Then Return Resolved
            End If
            Return Nothing
        End Function

        Public Function TryParametricConstructor(ByVal PhysicalType As Type) As [Delegate]
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then Return Nothing

            Dim ReadableAndWritableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim WritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim ReadableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) f.IsInitOnly).ToArray
            Dim ReadableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso Not p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray

            If ReadableAndWritableFields.Count > 0 Then Return Nothing
            If WritableProperties.Count > 0 Then Return Nothing
            If ReadableFields.Length > 0 AndAlso ReadableProperties.Length > 0 Then Return Nothing

            Dim dParam = Expression.Variable(PhysicalType, "Key")
            Dim rParam = Expression.Variable(GetType(R), "Value")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)

            Dim FieldMembers = ReadableFields.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(dParam, f), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableProperties.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(dParam, f), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim FieldAndPropertyTypes = FieldsAndProperties.Select(Function(f) f.Type).ToArray
            Dim c = PhysicalType.GetConstructor(FieldAndPropertyTypes)
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToMapper.ContainsKey(Type) Then Continue For
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(Type)
                TypeToMapper.Add(Type, Mapper)
                If Mapper.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(Mapper, n)
                    ClosureObjects.Add(Mapper)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Mapper = TypeToMapper(Type)
                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, FieldOrPropertyExpr, rParam)
                Else
                    Dim n = ReaderToClosureField(Mapper)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Action(Of ,)).MakeGenericType(Type, GetType(R))
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    MapperCall = Expression.Invoke(DelegateFunc, FieldOrPropertyExpr, rParam)
                End If
                Statements.Add(MapperCall)
            Next

            Dim FunctionBody = Expression.Block(Statements)
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam, rParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If
            Return Compiled
        End Function

        Public Function TryNonparametricConstructor(ByVal PhysicalType As Type) As [Delegate]
            If Not (PhysicalType.IsValueType OrElse PhysicalType.IsClass) Then Return Nothing

            If PhysicalType.IsClass Then
                Dim c = PhysicalType.GetConstructor(New Type() {})
                If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
            End If

            Dim ReadableAndWritableFields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim ReadableAndWritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim WritableProperties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            If Not ((ReadableAndWritableFields.Length > 0 AndAlso WritableProperties.Length = 0) OrElse (ReadableAndWritableFields.Length = 0 AndAlso ReadableAndWritableProperties.Length > 0)) Then Return Nothing

            Dim dParam = Expression.Variable(PhysicalType, "Key")
            Dim rParam = Expression.Variable(GetType(R), "Value")
            Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

            Dim Statements As New List(Of Expression)

            Dim FieldMembers = ReadableAndWritableFields.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(dParam, f), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableAndWritableProperties.Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(dParam, f), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If PhysicalType.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim TypeToMapper As New Dictionary(Of Type, [Delegate])
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each Pair In FieldsAndProperties
                Dim Type = Pair.Type
                If TypeToMapper.ContainsKey(Type) Then Continue For
                Dim MapperMethod = Map
                Dim Mapper = MapperMethod.MakeDelegateMethodFromDummy(Type)
                TypeToMapper.Add(Type, Mapper)
                If Mapper.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(Mapper, n)
                    ClosureObjects.Add(Mapper)
                End If
            Next
            Dim Closure As Closure = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = New Closure(Nothing, ClosureObjects.ToArray)
            End If
            For Each Pair In FieldsAndProperties
                Dim FieldOrPropertyExpr = Pair.FieldOrPropertyExpr
                Dim Type = Pair.Type
                Dim Mapper = TypeToMapper(Type)
                Dim MapperCall As Expression
                If Mapper.Target Is Nothing Then
                    MapperCall = Expression.Call(Mapper.Method, FieldOrPropertyExpr, rParam)
                Else
                    Dim n = ReaderToClosureField(Mapper)
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Locals(i)
                    Dim DelegateType = GetType(Action(Of ,)).MakeGenericType(Type, GetType(R))
                    Dim DelegateFunc = Expression.ConvertChecked(Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n)), DelegateType)
                    MapperCall = Expression.Invoke(DelegateFunc, FieldOrPropertyExpr, rParam)
                End If
                Statements.Add(MapperCall)
            Next

            Dim FunctionBody = Expression.Block(Statements)
            Dim FunctionLambda = Expression.Lambda(FunctionBody, New ParameterExpression() {dParam, rParam})
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Dim CompiledFunc = CType(Compiled, Func(Of Closure, [Delegate]))
                Compiled = CompiledFunc(Closure)
            End If
            Return Compiled
        End Function

        Private Map As Action(Of DummyType, R)
        Public Sub New(ByVal Map As Action(Of DummyType, R))
            Me.Map = Map
        End Sub
    End Class
End Class
