'==========================================================================
'
'  File:        MetaProgramming.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 元编程
'  Version:     2010.11.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Module MetaProgramming
    <Extension()> Public Function IsListType(ByVal PhysicalType As Type) As Boolean
        Return PhysicalType.GetInterfaces().Any(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of )))
    End Function
    <Extension()> Public Function GetListElementType(ByVal PhysicalType As Type) As Type
        Return PhysicalType.GetInterfaces().Where(Function(t) t.IsGenericType AndAlso t.GetGenericTypeDefinition Is GetType(ICollection(Of ))).Single.GetGenericArguments()(0)
    End Function
    <Extension()> Public Function MakeGenericTypeFromDummy(ByVal Type As Type, ByVal Mapping As Func(Of Type, Type)) As Type
        Dim Mapped = Mapping(Type)
        If Mapped IsNot Type Then Return Mapped
        If Type.IsGenericTypeDefinition Then Throw New ArgumentException
        If Type.IsGenericType Then
            Return Type.GetGenericTypeDefinition.MakeGenericType(Type.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(Mapping)).ToArray)
        End If
        If Type.IsArray Then
            Dim ElementType = Type.GetElementType()
            Dim n = Type.GetArrayRank()
            If n = 1 Then Return ElementType.MakeGenericTypeFromDummy(Mapping).MakeArrayType()
            Return ElementType.MakeGenericTypeFromDummy(Mapping).MakeArrayType(n)
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
        If Method.IsGenericMethodDefinition Then Throw New ArgumentException
        Return Method.GetGenericMethodDefinition().MakeGenericMethod(Method.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(Mapping)).ToArray)
    End Function
    <Extension()> Public Function MakeGenericMethodFromDummy(ByVal Method As MethodInfo, ByVal DummyType As Type, ByVal RealType As Type) As MethodInfo
        If Method.IsGenericMethodDefinition Then Throw New ArgumentException
        Return Method.GetGenericMethodDefinition().MakeGenericMethod(Method.GetGenericArguments().Select(Function(t) t.MakeGenericTypeFromDummy(DummyType, RealType)).ToArray)
    End Function
    <Extension()> Public Function MakeDelegateMethodFromDummy(ByVal m As [Delegate], ByVal Mapping As Func(Of Type, Type)) As [Delegate]
        Dim Target = m.Target
        Dim Method = m.Method
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

    Public Class DummyType
    End Class
End Module
