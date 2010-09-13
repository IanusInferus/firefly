'==========================================================================
'
'  File:        NumericOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 数值操作
'  Version:     2010.05.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Runtime.CompilerServices

''' <summary>数值操作</summary>
Public Module NumericOperations
    Public Function Max(Of T As IComparable)(ByVal a As T, ByVal b As T) As T
        If Not (a Is Nothing) Then
            If a.CompareTo(b) >= 0 Then Return a
        End If
        Return b
    End Function
    Public Function Min(Of T As IComparable)(ByVal a As T, ByVal b As T) As T
        If Not (a Is Nothing) Then
            If a.CompareTo(b) >= 0 Then Return b
        End If
        Return a
    End Function
    Public Function Max(Of T As IComparable)(ByVal a As T, ByVal ParamArray b As T()) As T
        Dim ret As T = a
        For Each x As T In b
            If Not (x Is Nothing) Then
                If x.CompareTo(ret) >= 0 Then ret = x
            End If
        Next
        Return ret
    End Function
    Public Function Min(Of T As IComparable)(ByVal a As T, ByVal ParamArray b As T()) As T
        Dim ret As T = a
        For Each x As T In b
            If Not (ret Is Nothing) Then
                If ret.CompareTo(x) >= 0 Then ret = x
            End If
        Next
        Return ret
    End Function
    Public Function Exchange(Of T)(ByRef a As T, ByRef b As T) As T
        Dim Temp As T
        Temp = a
        a = b
        b = Temp
    End Function
    <Extension()> Public Function [Mod](ByVal This As Integer, ByVal m As Integer) As Integer
        Dim Result = This Mod m
        If Result < 0 Xor m < 0 Then Result += m
        Return Result
    End Function
End Module
