'==========================================================================
'
'  File:        ArrayOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 数组操作
'  Version:     2009.01.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices

''' <summary>数组操作</summary>
Public Module ArrayOperations
    ''' <summary>
    ''' 已重载。获取数组的子数组。
    ''' </summary>
    ''' <param name="This">数组</param>
    ''' <param name="StartIndex">起始索引</param>
    ''' <param name="Length">长度</param>
    <Extension()> Public Function SubArray(Of T)(ByVal This As T(), ByVal StartIndex As Integer, ByVal Length As Integer) As T()
        Dim s = New T(Length - 1) {}
        Array.Copy(This, StartIndex, s, 0, Length)
        Return s
    End Function

    ''' <summary>
    ''' 已重载。获取数组的子数组。
    ''' </summary>
    ''' <param name="This">数组</param>
    ''' <param name="StartIndex">起始索引</param>
    <Extension()> Public Function SubArray(Of T)(ByVal This As T(), ByVal StartIndex As Integer) As T()
        Return SubArray(This, StartIndex, This.Length - StartIndex)
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
    ''' 判断数组是否元素完全相等。
    ''' </summary>
    <Extension()> Public Function ArrayEqual(Of T)(ByVal a As T(), ByVal b As T()) As Boolean
        If a.Length <> b.Length Then Return False
        For n = 0 To a.Length - 1
            If Not a(n).Equals(b(n)) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' 判断数组是否元素完全相等。
    ''' </summary>
    <Extension()> Public Function ArrayEqual(Of T)(ByVal a As T(), ByVal b As T(), ByVal bIndex As Integer, ByVal bCount As Integer) As Boolean
        If a.Length <> bCount Then Return False
        For n = 0 To a.Length - 1
            If Not a(n).Equals(b(bIndex + n)) Then Return False
        Next
        Return True
    End Function
End Module
