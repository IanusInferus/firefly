'==========================================================================
'
'  File:        NumericOperations.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 数值操作
'  Version:     2010.09.30.
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
        Dim r = This Mod m
        If (r < 0 AndAlso m > 0) OrElse (r > 0 AndAlso m < 0) Then r += m
        Return r
    End Function
    <Extension()> Public Function [Mod](ByVal This As Long, ByVal m As Long) As Long
        Dim r = This Mod m
        If (r < 0 AndAlso m > 0) OrElse (r > 0 AndAlso m < 0) Then r += m
        Return r
    End Function
    <Extension()> Public Function Div(ByVal This As Integer, ByVal b As Integer) As Integer
        If b = 0 Then Throw New DivideByZeroException()
        Dim r = This.Mod(b)
        If This > 0 AndAlso r < 0 Then
            If (This - Integer.MaxValue > r) Then Return (This - Math.Abs(b) - r) \ b + Math.Sign(b)
        ElseIf This < 0 AndAlso r > 0 Then
            If (This - Integer.MinValue < r) Then Return (This + Math.Abs(b) - r) \ b - Math.Sign(b)
        End If
        Return (This - r) \ b
    End Function
    <Extension()> Public Function Div(ByVal This As Long, ByVal b As Long) As Long
        If b = 0 Then Throw New DivideByZeroException()
        Dim r = This.Mod(b)
        If This > 0 AndAlso r < 0 Then
            If (This - Long.MaxValue > r) Then Return (This - Math.Abs(b) - r) \ b + Math.Sign(b)
        ElseIf This < 0 AndAlso r > 0 Then
            If (This - Long.MinValue < r) Then Return (This + Math.Abs(b) - r) \ b - Math.Sign(b)
        End If
        Return (This - r) \ b
    End Function
    <Extension()> Public Function CeilToMultipleOf(ByVal This As Integer, ByVal n As Integer) As Integer
        Return (This + n - 1).Div(n) * n
    End Function
    <Extension()> Public Function CeilToMultipleOf(ByVal This As Long, ByVal n As Long) As Long
        Return (This + n - 1).Div(n) * n
    End Function
End Module
