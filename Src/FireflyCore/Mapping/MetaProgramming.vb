'==========================================================================
'
'  File:        MetaProgramming.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 元编程
'  Version:     2010.11.15.
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

Namespace Mapping
    Public Class DummyType
    End Class

    Public Module MetaProgramming
        <Extension()> Public Function IsCollectionType(ByVal Type As Type) As Boolean
            Return Type.GetInterfaces().Any(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of )))
        End Function
        <Extension()> Public Function GetCollectionElementType(ByVal Type As Type) As Type
            Return Type.GetInterfaces().Where(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of ))).Single.GetGenericArguments()(0)
        End Function

        Public Class FieldOrPropertyInfo
            Public Member As MemberInfo
            Public Type As Type
        End Class
        Public Class ImmutableRecordInfo
            Public Members As FieldOrPropertyInfo()
            Public Constructor As ConstructorInfo
        End Class
        Public Class MutableRecordInfo
            Public Members As FieldOrPropertyInfo()
        End Class
        ''' <remarks>
        ''' 不变记录 ::= 类或结构(构造函数(参数(简单类型)*), 公共只读字段(简单类型)*, 公共可写属性{0}) AND (参数(简单类型)* = 公共只读字段(简单类型)*)
        '''            | 类或结构(构造函数(参数(简单类型)*), 公共可写字段{0}, 公共只读属性(简单类型)*) AND (参数(简单类型)* = 公共只读属性(简单类型)*)
        ''' </remarks>
        <Extension()> Public Function TryGetImmutableRecordInfo(ByVal Type As Type) As ImmutableRecordInfo
            If Not (Type.IsValueType OrElse Type.IsClass) Then Return Nothing

            Dim ReadableAndWritableFields = Type.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim WritableProperties = Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim ReadableFields = Type.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) f.IsInitOnly).ToArray
            Dim ReadableProperties = Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso Not p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray

            If ReadableAndWritableFields.Count > 0 Then Return Nothing
            If WritableProperties.Count > 0 Then Return Nothing
            If ReadableFields.Length > 0 AndAlso ReadableProperties.Length > 0 Then Return Nothing

            Dim FieldMembers = ReadableFields.Select(Function(f) New FieldOrPropertyInfo With {.Member = DirectCast(f, MemberInfo), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableProperties.Select(Function(f) New FieldOrPropertyInfo With {.Member = DirectCast(f, MemberInfo), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = Type.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If Type.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Dim FieldAndPropertyTypes = FieldsAndProperties.Select(Function(f) f.Type).ToArray
            Dim c = Type.GetConstructor(FieldAndPropertyTypes)
            If c Is Nothing OrElse Not c.IsPublic Then Return Nothing

            Return New ImmutableRecordInfo With {.Members = FieldsAndProperties, .Constructor = c}
        End Function
        ''' <remarks>
        ''' 可变记录 ::= 类或结构(无参构造函数, 公共可读写字段(简单类型)*, 公共可写属性{0})
        '''            | 类或结构(无参构造函数, 公共可写字段{0}, 公共可读写属性(简单类型)*)
        ''' </remarks>
        <Extension()> Public Function TryGetMutableRecordInfo(ByVal Type As Type) As MutableRecordInfo
            If Not (Type.IsValueType OrElse Type.IsClass) Then Return Nothing

            If Type.IsClass Then
                Dim c = Type.GetConstructor(New Type() {})
                If c Is Nothing OrElse Not c.IsPublic Then Return Nothing
            End If

            Dim ReadableAndWritableFields = Type.GetFields(BindingFlags.Public Or BindingFlags.Instance).Where(Function(f) Not f.IsInitOnly).ToArray
            Dim ReadableAndWritableProperties = Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            Dim WritableProperties = Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(p) p.CanWrite AndAlso p.GetIndexParameters.Length = 0).ToArray
            If Not ((ReadableAndWritableFields.Length > 0 AndAlso WritableProperties.Length = 0) OrElse (ReadableAndWritableFields.Length = 0 AndAlso ReadableAndWritableProperties.Length > 0)) Then Return Nothing

            Dim FieldMembers = ReadableAndWritableFields.Select(Function(f) New FieldOrPropertyInfo With {.Member = DirectCast(f, MemberInfo), .Type = f.FieldType}).ToArray
            Dim PropertyMembers = ReadableAndWritableProperties.Select(Function(f) New FieldOrPropertyInfo With {.Member = DirectCast(f, MemberInfo), .Type = f.PropertyType}).ToArray
            Dim MemberToIndex = Type.GetMembers.Select(Function(m, i) New With {.Member = m, .Index = i}).ToDictionary(Function(p) p.Member, Function(p) p.Index)
            Dim FieldsAndProperties = FieldMembers.Concat(PropertyMembers).OrderBy(Function(f) MemberToIndex(f.Member)).ToArray
            If Type.IsValueType Then
                If FieldsAndProperties.Length = 0 Then
                    Return Nothing
                End If
            End If

            Return New MutableRecordInfo With {.Members = FieldsAndProperties}
        End Function

        <Extension()> Public Function MakeArrayTypeFromRank(ByVal ElementType As Type, ByVal n As Integer) As Type
            If n = 1 Then Return ElementType.MakeArrayType()
            Return ElementType.MakeArrayType(n)
        End Function

        <Extension()> Public Function MakeGenericTypeFromDummy(ByVal Type As Type, ByVal Mapping As Func(Of Type, Type)) As Type
            Dim Mapped = Mapping(Type)
            If Mapped IsNot Type Then Return Mapped
            If Type.IsGenericTypeDefinition Then Throw New ArgumentException
            If Type.IsGenericType Then
                Return Type.GetGenericTypeDefinition.MakeGenericType(Type.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(Mapping)).ToArray)
            End If
            If Type.IsArray Then
                Return Type.GetElementType().MakeGenericTypeFromDummy(Mapping).MakeArrayTypeFromRank(Type.GetArrayRank())
            End If
            Return Type
        End Function
        <Extension()> Public Function MakeGenericTypeFromDummy(ByVal Type As Type, ByVal DummyType As Type, ByVal RealType As Type) As Type
            Dim Mapping =
                Function(t As Type) As Type
                    If t Is DummyType Then Return RealType
                    Return t
                End Function
            Return MakeGenericTypeFromDummy(Type, Mapping)
        End Function
        <Extension()> Public Function MakeGenericMethodFromDummy(ByVal Method As MethodInfo, ByVal Mapping As Func(Of Type, Type)) As MethodInfo
            If Not Method.IsGenericMethod Then Throw New ArgumentException
            If Method.IsGenericMethodDefinition Then Throw New ArgumentException
            Return Method.GetGenericMethodDefinition().MakeGenericMethod(Method.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(Mapping)).ToArray)
        End Function
        <Extension()> Public Function MakeGenericMethodFromDummy(ByVal Method As MethodInfo, ByVal DummyType As Type, ByVal RealType As Type) As MethodInfo
            If Not Method.IsGenericMethod Then Throw New ArgumentException
            If Method.IsGenericMethodDefinition Then Throw New ArgumentException
            Return Method.GetGenericMethodDefinition().MakeGenericMethod(Method.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(DummyType, RealType)).ToArray)
        End Function
        <Extension()> Public Function MakeDelegateMethodFromDummy(ByVal m As [Delegate], ByVal Mapping As Func(Of Type, Type)) As [Delegate]
            Dim Target = m.Target
            Dim Method = m.Method
            If Not Method.IsGenericMethod Then Throw New ArgumentException
            If Method.IsGenericMethodDefinition Then Throw New ArgumentException
            Dim GenericMethod = Method.MakeGenericMethodFromDummy(Mapping)
            Dim MethodType = m.GetType().MakeGenericTypeFromDummy(Mapping)
            Return [Delegate].CreateDelegate(MethodType, Target, GenericMethod)
        End Function
        <Extension()> Public Function MakeDelegateMethodFromDummy(ByVal m As [Delegate], ByVal DummyType As Type, ByVal RealType As Type) As [Delegate]
            Dim Mapping =
                Function(t As Type) As Type
                    If t Is DummyType Then Return RealType
                    Return t
                End Function
            Return MakeDelegateMethodFromDummy(m, Mapping)
        End Function
        <Extension()> Public Function MakeDelegateMethodFromDummy(ByVal m As [Delegate], ByVal RealType As Type) As [Delegate]
            Return MakeDelegateMethodFromDummy(m, GetType(DummyType), RealType)
        End Function

        Public Class DelegateExpressionContext
            Public ClosureParam As ParameterExpression
            Public Closure As Object
            Public DelegateExpressions As Expression()
        End Class
        Public Function CreateFieldOrPropertyExpression(ByVal Param As ParameterExpression, ByVal Member As MemberInfo) As MemberExpression
            Select Case Member.MemberType
                Case MemberTypes.Field
                    Return Expression.Field(Param, DirectCast(Member, FieldInfo))
                Case MemberTypes.Property
                    Return Expression.Property(Param, DirectCast(Member, PropertyInfo))
                Case Else
                    Throw New ArgumentException
            End Select
        End Function
        Public Function CreateDelegateExpressionContext(ByVal DelegateCalls As IEnumerable(Of KeyValuePair(Of [Delegate], Expression()))) As DelegateExpressionContext
            Dim ReaderToClosureField As New Dictionary(Of [Delegate], Integer)
            Dim ClosureObjects As New List(Of Object)
            For Each DelegateCall In DelegateCalls
                Dim d = DelegateCall.Key
                If d.Target IsNot Nothing Then
                    Dim n = ClosureObjects.Count
                    ReaderToClosureField.Add(d, n)
                    ClosureObjects.Add(d)
                End If
            Next
            Dim ClosureParam As ParameterExpression = Nothing
            Dim Closure As Object = Nothing
            Dim AccessClosure As Func(Of Integer, Expression) = Nothing
            If ClosureObjects.Count > 0 Then
                If ClosureObjects.Count = 1 Then
                    Closure = ClosureObjects(0)
                    ClosureParam = Expression.Variable(Closure.GetType(), "<>_Closure")
                    AccessClosure = Function(n) ClosureParam
                Else
                    Closure = New Closure(ClosureObjects.ToArray, Nothing)
                    ClosureParam = Expression.Variable(GetType(Closure), "<>_Closure")
                    Dim ArrayIndex = Function(cl As Closure, i As Integer) cl.Constants(i)
                    AccessClosure = Function(n) Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n))
                End If
            End If
            Dim DelegateExpressions As New List(Of Expression)
            For Each DelegateCall In DelegateCalls
                Dim d = DelegateCall.Key
                If d.Target Is Nothing Then
                    DelegateExpressions.Add(Expression.Call(d.Method, DelegateCall.Value))
                Else
                    Dim n = ReaderToClosureField(d)
                    Dim DelegateType = d.GetType()
                    Dim DelegateFunc = Expression.ConvertChecked(AccessClosure(n), DelegateType)
                    DelegateExpressions.Add(Expression.Invoke(DelegateFunc, DelegateCall.Value))
                End If
            Next
            Return New DelegateExpressionContext With {.ClosureParam = ClosureParam, .Closure = Closure, .DelegateExpressions = DelegateExpressions.ToArray()}
        End Function
        Public Function CreateDelegate(ByVal ClosureParam As ParameterExpression, ByVal Closure As Object, ByVal Expr As LambdaExpression) As [Delegate]
            Dim FunctionLambda = Expr
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Compiled = DirectCast(Compiled.DynamicInvoke(Closure), [Delegate])
            End If
            Return Compiled
        End Function
        <Extension()> Public Function Compose(Of D, M, R)(ByVal InnerFunction As Func(Of D, M), ByVal OuterFunction As Func(Of M, R)) As Func(Of D, R)
            Return Function(v) OuterFunction(InnerFunction(v))
        End Function
        <Extension()> Public Function Compose(ByVal InnerFunction As [Delegate], ByVal OuterFunction As [Delegate]) As [Delegate]
            Dim D = InnerFunction.Method.GetParameters.Single.ParameterType
            Dim MI = InnerFunction.Method.ReturnType
            Dim MO = OuterFunction.Method.GetParameters.Single.ParameterType
            Dim R = OuterFunction.Method.ReturnType

            Dim iParam = Expression.Variable(InnerFunction.GetType(), "<>_i")
            Dim oParam = Expression.Variable(OuterFunction.GetType(), "<>_o")

            Dim vParam = Expression.Variable(D, "<>_v")
            Dim l As LambdaExpression
            If MI Is MO Then
                l = Expression.Lambda(Expression.Invoke(oParam, Expression.Invoke(iParam, vParam)), vParam)
            Else
                l = Expression.Lambda(Expression.Invoke(oParam, Expression.ConvertChecked(Expression.Invoke(iParam, vParam), MO)), vParam)
            End If
            Return DirectCast(Expression.Lambda(l, iParam, oParam).Compile().DynamicInvoke(InnerFunction, OuterFunction), [Delegate])
        End Function

        Public Function CreatePair(Of TKey, TValue)(ByVal Key As TKey, ByVal Value As TValue) As KeyValuePair(Of TKey, TValue)
            Return New KeyValuePair(Of TKey, TValue)(Key, Value)
        End Function
    End Module
End Namespace
