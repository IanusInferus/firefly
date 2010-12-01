'==========================================================================
'
'  File:        MetaProgramming.vb
'  Location:    Firefly.Mapping <Visual Basic .Net>
'  Description: 元编程
'  Version:     2010.12.01.
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
Imports System.Diagnostics

Namespace Mapping
    Public Class DummyType
    End Class

    <DebuggerNonUserCode()>
    Public Module MetaProgramming
        <Extension()> Public Function IsProperCollectionType(ByVal Type As Type) As Boolean
            Return Type.GetInterfaces().Where(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of ))).Count = 1
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
        <Extension()> Public Function MakeGenericTypeFromDummy(ByVal Type As Type, ByVal RealType As Type) As Type
            Return MakeGenericTypeFromDummy(Type, GetType(DummyType), RealType)
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
            Public Closure As Object()
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
            Dim DelegateCallsArray = DelegateCalls.ToArray
            Dim ClosureFieldIndices As New Dictionary(Of Integer, Integer)
            Dim ClosureObjects As New List(Of Object)
            With Nothing
                Dim k = 0
                For Each DelegateCall In DelegateCallsArray
                    Dim d = DelegateCall.Key
                    If d.Target IsNot Nothing Then
                        Dim n = ClosureObjects.Count
                        ClosureFieldIndices.Add(k, n)
                        ClosureObjects.Add(d)
                    End If
                    k += 1
                Next
            End With
            Dim ClosureParam As ParameterExpression = Nothing
            Dim Closure As Object() = Nothing
            Dim AccessClosure As Func(Of Integer, Expression) = Nothing
            If ClosureObjects.Count > 0 Then
                Closure = ClosureObjects.ToArray
                ClosureParam = Expression.Parameter(GetType(Object()), "<>_Closure")
                Dim ArrayIndex = Function(cl As Object(), i As Integer) cl(i)
                AccessClosure = Function(n) Expression.Call(ArrayIndex.Method, ClosureParam, Expression.Constant(n))
            End If
            Dim DelegateExpressions As New List(Of Expression)
            With Nothing
                Dim k = 0
                For Each DelegateCall In DelegateCallsArray
                    Dim d = DelegateCall.Key
                    If d.Target Is Nothing Then
                        DelegateExpressions.Add(Expression.Call(d.Method, DelegateCall.Value))
                    Else
                        Dim n = ClosureFieldIndices(k)
                        Dim DelegateType = d.GetType()
                        Dim DelegateFunc = Expression.ConvertChecked(AccessClosure(n), DelegateType)
                        DelegateExpressions.Add(Expression.Invoke(DelegateFunc, DelegateCall.Value))
                    End If
                    k += 1
                Next
            End With
            Return New DelegateExpressionContext With {.ClosureParam = ClosureParam, .Closure = Closure, .DelegateExpressions = DelegateExpressions.ToArray()}
        End Function
        Public Function CreateDelegate(ByVal ClosureParam As ParameterExpression, ByVal Closure As Object(), ByVal Expr As LambdaExpression) As [Delegate]
            Dim FunctionLambda = Expr
            If Closure IsNot Nothing Then
                FunctionLambda = Expression.Lambda(FunctionLambda, New ParameterExpression() {ClosureParam})
            End If

            Dim Compiled = FunctionLambda.Compile()
            If Closure IsNot Nothing Then
                Compiled = DirectCast(DirectCast(Compiled, Func(Of Object(), [Delegate]))(Closure), [Delegate])
            End If
            Return Compiled
        End Function
        <Extension()> Public Function Compose(ByVal InnerFunction As [Delegate], ByVal OuterMethod As [Delegate]) As [Delegate]
            Dim D = InnerFunction.Method.GetParameters.Single.ParameterType
            Dim MI = InnerFunction.Method.ReturnType
            Dim MO = OuterMethod.Method.GetParameters.Single.ParameterType

            Dim iParam = Expression.Parameter(InnerFunction.GetType(), "<>_i")
            Dim oParam = Expression.Parameter(OuterMethod.GetType(), "<>_o")

            Dim vParam = Expression.Parameter(D, "<>_v")
            Dim InnerLambda As LambdaExpression
            If MI Is MO Then
                InnerLambda = Expression.Lambda(Expression.Invoke(oParam, Expression.Invoke(iParam, vParam)), vParam)
            Else
                InnerLambda = Expression.Lambda(Expression.Invoke(oParam, Expression.ConvertChecked(Expression.Invoke(iParam, vParam), MO)), vParam)
            End If
            Dim OuterLambda = Expression.Lambda(InnerLambda, iParam, oParam)
            Return OuterLambda.Compile().StaticDynamicInvoke(Of [Delegate], [Delegate], [Delegate])(InnerFunction, OuterMethod)
        End Function
        <Extension()> Public Function Curry(ByVal Method As [Delegate], ByVal ParamArray Parameters As Object()) As [Delegate]
            Dim ProvidedParameters = Method.Method.GetParameters().Take(Parameters.Length).Select(Function(p) Expression.Parameter(p.ParameterType, p.Name)).ToArray()
            Dim NotProvidedParameters = Method.Method.GetParameters().Skip(Parameters.Length).Select(Function(p) Expression.Parameter(p.ParameterType, p.Name)).ToArray()
            Dim AllParameters = ProvidedParameters.Concat(NotProvidedParameters).ToArray
            Dim mParam = Expression.Parameter(Method.GetType(), "<>_m")
            Dim InnerLambda = Expression.Lambda(Expression.Invoke(mParam, AllParameters), NotProvidedParameters)
            Dim OuterLambda = Expression.Lambda(InnerLambda, (New ParameterExpression() {mParam}).Concat(ProvidedParameters).ToArray())
            Dim OuterDelegate = OuterLambda.Compile()
            Dim ParamObjects = (New Object() {Method}).Concat(Parameters).ToArray()
            Return OuterDelegate.StaticDynamicInvoke(Of Object(), [Delegate])(ParamObjects)
        End Function
        <Extension()> Public Function AdaptFunction(ByVal Method As [Delegate], ByVal ReturnType As Type, ByVal ParamArray RequiredParameterTypes As Type()) As [Delegate]
            Dim Parameters = Method.Method.GetParameters().Zip(RequiredParameterTypes, Function(p, r) New With {.InnerType = p.ParameterType, .OuterType = r, .OuterParamExpr = Expression.Parameter(r, p.Name)}).ToArray

            Dim ClosureParam As ParameterExpression = Nothing
            ClosureParam = Expression.Parameter(GetType([Delegate]), "<>_Closure")
            Dim ConvertExpressions As New List(Of Expression)
            For Each p In Parameters
                ConvertExpressions.Add(Expression.ConvertChecked(p.OuterParamExpr, p.InnerType))
            Next
            Dim Ret = Expression.ConvertChecked(Expression.Invoke(Expression.ConvertChecked(ClosureParam, Method.GetType()), ConvertExpressions), ReturnType)
            Dim InnerLambda = Expression.Lambda(Ret, Parameters.Select(Function(p) p.OuterParamExpr).ToArray())
            Dim OuterLambda = Expression.Lambda(Expression.ConvertChecked(InnerLambda, GetType([Delegate])), ClosureParam)

            Dim OuterDelegate = DirectCast(OuterLambda.Compile(), Func(Of [Delegate], [Delegate]))
            Return OuterDelegate(Method)
        End Function
        <Extension()> Public Function AdaptFunction(Of TReturn)(ByVal Method As [Delegate]) As Func(Of TReturn)
            Return DirectCast(AdaptFunction(Method, GetType(TReturn)), Func(Of TReturn))
        End Function
        <Extension()> Public Function AdaptFunction(Of T, TReturn)(ByVal Method As [Delegate]) As Func(Of T, TReturn)
            Return DirectCast(AdaptFunction(Method, GetType(TReturn), GetType(T)), Func(Of T, TReturn))
        End Function
        <Extension()> Public Function AdaptFunction(Of T1, T2, TReturn)(ByVal Method As [Delegate]) As Func(Of T1, T2, TReturn)
            Return DirectCast(AdaptFunction(Method, GetType(TReturn), GetType(T1), GetType(T2)), Func(Of T1, T2, TReturn))
        End Function
        <Extension()> Public Function AdaptFunction(Of T1, T2, T3, TReturn)(ByVal Method As [Delegate]) As Func(Of T1, T2, T3, TReturn)
            Return DirectCast(AdaptFunction(Method, GetType(TReturn), GetType(T1), GetType(T2), GetType(T3)), Func(Of T1, T2, T3, TReturn))
        End Function
        <Extension()> Public Function AdaptFunction(Of T1, T2, T3, T4, TReturn)(ByVal Method As [Delegate]) As Func(Of T1, T2, T3, T4, TReturn)
            Return DirectCast(AdaptFunction(Method, GetType(TReturn), GetType(T1), GetType(T2), GetType(T3)), Func(Of T1, T2, T3, T4, TReturn))
        End Function
        <Extension()> Public Function StaticDynamicInvoke(Of TReturn)(ByVal Method As [Delegate]) As TReturn
            Return Method.AdaptFunction(Of TReturn)()()
        End Function
        <Extension()> Public Function StaticDynamicInvoke(Of T, TReturn)(ByVal Method As [Delegate], ByVal v As T) As TReturn
            Return Method.AdaptFunction(Of T, TReturn)()(v)
        End Function
        <Extension()> Public Function StaticDynamicInvoke(Of T1, T2, TReturn)(ByVal Method As [Delegate], ByVal v1 As T1, ByVal v2 As T2) As TReturn
            Return Method.AdaptFunction(Of T1, T2, TReturn)()(v1, v2)
        End Function
        <Extension()> Public Function StaticDynamicInvoke(Of T1, T2, T3, TReturn)(ByVal Method As [Delegate], ByVal v1 As T1, ByVal v2 As T2, ByVal v3 As T3) As TReturn
            Return Method.AdaptFunction(Of T1, T2, T3, TReturn)()(v1, v2, v3)
        End Function
        <Extension()> Public Function StaticDynamicInvoke(Of T1, T2, T3, T4, TReturn)(ByVal Method As [Delegate], ByVal v1 As T1, ByVal v2 As T2, ByVal v3 As T3, ByVal v4 As T4) As TReturn
            Return Method.AdaptFunction(Of T1, T2, T3, T4, TReturn)()(v1, v2, v3, v4)
        End Function
    End Module
End Namespace
