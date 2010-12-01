'==========================================================================
'
'  File:        CollectionOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 集合操作
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

''' <summary>数组操作</summary>
Public Module CollectionOperations
    Public Function CreatePair(Of TKey, TValue)(ByVal Key As TKey, ByVal Value As TValue) As KeyValuePair(Of TKey, TValue)
        Return New KeyValuePair(Of TKey, TValue)(Key, Value)
    End Function

    ''' <summary>
    ''' 返回数组的扩展数组。
    ''' </summary>
    ''' <param name="This">数组</param>
    ''' <param name="Length">新长度</param>
    ''' <param name="Value">初始值</param>
    <Extension()> Public Function Extend(Of T)(ByVal This As T(), ByVal Length As Integer, ByVal Value As T) As T()
        If This.Length > Length Then Throw New ArgumentOutOfRangeException
        Dim newBytes As T() = New T(Length - 1) {}
        Array.Copy(This, newBytes, Min(This.Length, Length))
        For n = Min(This.Length, Length) To Length - 1
            newBytes(n) = Value
        Next
        Return newBytes
    End Function

    ''' <summary>
    ''' 对指定数组的每个元素执行指定操作。
    ''' </summary>
    ''' <typeparam name="T">数组元素的类型。</typeparam>
    ''' <param name="This">从零开始的一维 Array，要对其元素执行操作。</param>
    ''' <param name="Action">要对 array 的每个元素执行的 Action(Of T)。</param>
    <Extension()> Public Sub ForEach(Of T)(ByVal This As T(), ByVal Action As Action(Of T))
        Array.ForEach(This, Action)
    End Sub

    ''' <summary>
    ''' 返回字典的值或者默认值
    ''' </summary>
    <Extension()> Public Function ItemOrDefault(Of TKey, TValue)(ByVal This As IDictionary(Of TKey, TValue), ByVal Key As TKey, ByVal DefaultValue As TValue) As TValue
        Dim ReturnValue As TValue = Nothing
        If This.TryGetValue(Key, ReturnValue) Then
            Return ReturnValue
        Else
            Return DefaultValue
        End If
    End Function
End Module
