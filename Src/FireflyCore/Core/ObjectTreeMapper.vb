'==========================================================================
'
'  File:        ObjectTreeMapper.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: Object树映射
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

Public Interface IObjectTreeMapperResolver
    ''' <param name="TypePair">(DomainType, RangeType)</param>
    ''' <returns>返回Func(Of ${DomainType}, ${RangeType})</returns>
    Function TryResolve(ByVal TypePair As KeyValuePair(Of Type, Type)) As [Delegate]
End Interface

Public Interface IObjectTreeOneToManyMapperResolver(Of D)
    ''' <returns>返回Func(Of ${Type})</returns>
    Function TryResolve(ByVal RangeType As Type) As [Delegate]
End Interface

Public Interface IObjectTreeManyToOneMapperResolver(Of R)
    ''' <returns>返回Action(Of ${Type})</returns>
    Function TryResolve(ByVal DomainType As Type) As [Delegate]
End Interface

Public Class ObjectTreeMapper
    Private MapperCache As New Dictionary(Of KeyValuePair(Of Type, Type), [Delegate])
    Private ResolversValue As List(Of IObjectTreeMapperResolver)
    Public ReadOnly Property Resolvers As List(Of IObjectTreeMapperResolver)
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectTreeMapper, IEnumerable(Of IObjectTreeMapperResolver)))
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

Public Class ObjectTreeOneToManyMapper(Of D)
    Private MapperCache As New Dictionary(Of Type, [Delegate])
    Private ResolversValue As List(Of IObjectTreeOneToManyMapperResolver(Of D))
    Public ReadOnly Property Resolvers As List(Of IObjectTreeOneToManyMapperResolver(Of D))
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectTreeOneToManyMapper(Of D), IEnumerable(Of IObjectTreeOneToManyMapperResolver(Of D))))
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
        Implements IObjectTreeOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeOneToManyMapperResolver(Of D).TryResolve
            If PhysicalType.IsEnum Then
                Dim UnderlyingType = PhysicalType.GetEnumUnderlyingType
                Dim MapperMethod = DirectCast(AddressOf mp.Map(Of DummyType), Func(Of D, DummyType))
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

        Private mp As ObjectTreeOneToManyMapper(Of D)
        Public Sub New(ByVal mp As ObjectTreeOneToManyMapper(Of D))
            Me.mp = mp
        End Sub
    End Class

    Public MustInherit Class CollectionMapperResolver
        Implements IObjectTreeOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeOneToManyMapperResolver(Of D).TryResolve
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

        Private mp As ObjectTreeOneToManyMapper(Of D)
        Public Sub New(ByVal mp As ObjectTreeOneToManyMapper(Of D))
            Me.mp = mp
        End Sub

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
                PutArrayMapperGenerator(DirectCast(AddressOf DefaultArrayMapper(Of DummyType), Func(Of D, DummyType())))
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
                Dim DummyMethod As Func(Of D, List(Of DummyType)) = AddressOf DefaultListMapper(Of DummyType, List(Of DummyType))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                PutListMapperGenerator(m)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Public MustOverride Function DefaultArrayMapper(Of R)(ByVal Key As D) As R()
        Public MustOverride Function DefaultListMapper(Of R, RList As {New, ICollection(Of R)})(ByVal Key As D) As RList
    End Class

    Public Class ClassAndStructureMapperResolver
        Implements IObjectTreeOneToManyMapperResolver(Of D)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeOneToManyMapperResolver(Of D).TryResolve
            If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
                If PhysicalType.IsClass Then
                    Dim c = PhysicalType.GetConstructor(New Type() {})
                    If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
                End If

                Dim dParam = Expression.Variable(GetType(D), "Key")
                Dim rParam = Expression.Variable(PhysicalType, "Value")
                Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

                Dim Statements As New List(Of Expression)
                Dim CreateThis = Expression.Assign(rParam, Expression.[New](PhysicalType))
                Statements.Add(CreateThis)

                Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).Where(Function(p) Not p.IsInitOnly).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(rParam, f), .Type = f.FieldType}).ToArray
                Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(rParam, f), .Type = f.PropertyType}).ToArray
                If Fields.Length > 0 AndAlso Properties.Length > 0 Then Return Nothing
                Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
                Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
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
                    Dim MapperMethod = DirectCast(AddressOf mp.Map(Of DummyType), Func(Of D, DummyType))
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
            End If
            Return Nothing
        End Function

        Private mp As ObjectTreeOneToManyMapper(Of D)
        Public Sub New(ByVal mp As ObjectTreeOneToManyMapper(Of D))
            Me.mp = mp
        End Sub
    End Class
End Class

Public Class ObjectTreeManyToOneMapper(Of R)
    Private MapperCache As New Dictionary(Of Type, [Delegate])
    Private ResolversValue As List(Of IObjectTreeManyToOneMapperResolver(Of R))
    Public ReadOnly Property Resolvers As List(Of IObjectTreeManyToOneMapperResolver(Of R))
        Get
            Return ResolversValue
        End Get
    End Property
    Public Sub New()
    End Sub
    Public Sub New(ByVal GetResolvers As Func(Of ObjectTreeManyToOneMapper(Of R), IEnumerable(Of IObjectTreeManyToOneMapperResolver(Of R))))
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
        Implements IObjectTreeManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeManyToOneMapperResolver(Of R).TryResolve
            If PhysicalType.IsEnum Then
                Dim UnderlyingType = PhysicalType.GetEnumUnderlyingType
                Dim MapperMethod = DirectCast(AddressOf mp.Map(Of DummyType), Action(Of DummyType, R))
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

        Private mp As ObjectTreeManyToOneMapper(Of R)
        Public Sub New(ByVal mp As ObjectTreeManyToOneMapper(Of R))
            Me.mp = mp
        End Sub
    End Class

    Public MustInherit Class CollectionMapperResolver
        Implements IObjectTreeManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeManyToOneMapperResolver(Of R).TryResolve
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

        Private mp As ObjectTreeManyToOneMapper(Of R)
        Public Sub New(ByVal mp As ObjectTreeManyToOneMapper(Of R))
            Me.mp = mp
        End Sub

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
                PutArrayMapperGenerator(DirectCast(AddressOf DefaultArrayMapper(Of DummyType), Action(Of DummyType(), R)))
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
                Dim DummyMethod As Action(Of List(Of DummyType), R) = AddressOf DefaultListMapper(Of DummyType, List(Of DummyType))
                Dim m = DummyMethod.MakeDelegateMethodFromDummy(GetType(List(Of DummyType)), DummyListType)
                PutListMapperGenerator(m)
            End If
            Return ListMapperGeneratorCache(ListType)
        End Function

        Public MustOverride Sub DefaultArrayMapper(Of D)(ByVal arr As D(), ByVal Value As R)
        Public MustOverride Sub DefaultListMapper(Of D, DList As ICollection(Of D))(ByVal list As DList, ByVal Value As R)
    End Class

    Public Class ClassAndStructureMapperResolver
        Implements IObjectTreeManyToOneMapperResolver(Of R)

        Public Function TryResolve(ByVal PhysicalType As Type) As [Delegate] Implements IObjectTreeManyToOneMapperResolver(Of R).TryResolve
            If PhysicalType.IsValueType OrElse PhysicalType.IsClass Then
                If PhysicalType.IsClass Then
                    Dim c = PhysicalType.GetConstructor(New Type() {})
                    If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
                End If

                Dim dParam = Expression.Variable(PhysicalType, "Key")
                Dim rParam = Expression.Variable(GetType(R), "Value")
                Dim ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")

                Dim Statements As New List(Of Expression)

                Dim Fields = PhysicalType.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Field(dParam, f), .Type = f.FieldType}).ToArray
                Dim Properties = PhysicalType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).Select(Function(f) New With {.Member = DirectCast(f, MemberInfo), .FieldOrPropertyExpr = Expression.Property(dParam, f), .Type = f.PropertyType}).ToArray
                If Fields.Length > 0 AndAlso Properties.Length > 0 Then Return Nothing
                Dim MemberToIndex = PhysicalType.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
                Dim FieldsAndProperties = Fields.Concat(Properties).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
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
                    Dim MapperMethod = DirectCast(AddressOf mp.Map(Of DummyType), Action(Of DummyType, R))
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
            End If
            Return Nothing
        End Function

        Private mp As ObjectTreeManyToOneMapper(Of R)
        Public Sub New(ByVal mp As ObjectTreeManyToOneMapper(Of R))
            Me.mp = mp
        End Sub
    End Class
End Class
