'==========================================================================
'
'  File:        Indexer.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 离散索引器
'  Version:     2010.09.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO

''' <summary>
''' 离散索引器，用于表示离散整数区间，并提供遍历离散整数区间的函数与枚举器支持。
''' 支持使用For Each语法遍历区间内的所有点。
''' </summary>
Public Class Indexer
    Implements IEnumerator(Of Integer)
    Implements IEnumerable(Of Integer)
    Protected Descriptor As New SortedList(Of Integer, Range)
    Protected Value As Integer
    Protected Position As Integer

    Public Sub New(ByVal Descriptors As ICollection(Of Range))
        For Each d As Range In Descriptors
            If d.Lower = Integer.MinValue Then Throw New InvalidDataException
            Descriptor.Add(d.Lower, d)
        Next
        Value = Integer.MinValue
        Position = 0
    End Sub
    Public Sub AddDescriptor(ByVal d As Range)
        If d.Lower = Integer.MinValue Then Throw New InvalidDataException
        Descriptor.Add(d.Lower, d)
        Position = 0
    End Sub
    Public Sub RemoveDescriptor(ByVal d As Range)
        Descriptor.Remove(d.Lower)
        Position = 0
    End Sub

    Public Function Contain(ByVal i As Integer) As Boolean
        If Descriptor.Count = 0 Then Return False
        Dim U As Integer = Descriptor.Count - 1
        Dim M As Integer = U \ 2
        While U > 0
            If Descriptor.Keys(M) > i Then
                U = M
                M = U \ 2
            Else
                Exit While
            End If
        End While
        U = M
        For n = U To 0 Step -1
            If Descriptor(Descriptor.Keys(n)).Contain(i) Then Return True
        Next
        Return False
    End Function

    Public ReadOnly Property Current() As Integer Implements System.Collections.Generic.IEnumerator(Of Integer).Current
        Get
            Return Value
        End Get
    End Property
    Private ReadOnly Property CurrentNonGeneric() As Object Implements System.Collections.IEnumerator.Current
        Get
            Return Value
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
        If Descriptor.Count = 0 Then Return False
        Dim v = Value + 1
        While v >= Descriptor.Values(Position).Lower + Descriptor.Values(Position).Count
            Position += 1
            If Position >= Descriptor.Count Then Return False
        End While
        If v < Descriptor.Values(Position).Lower Then v = Descriptor.Values(Position).Lower
        Value = v
        Return True
    End Function

    Public Sub SetBefore(ByVal Index As Integer)
        Value = Index - 1
    End Sub

    Public Sub Reset() Implements System.Collections.IEnumerator.Reset
        If Descriptor.Count < 0 Then Throw New InvalidOperationException
        Value = Integer.MinValue
        Position = 0
    End Sub

    Private disposedValue As Boolean = False
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            Descriptor = Nothing
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Integer) Implements System.Collections.Generic.IEnumerable(Of Integer).GetEnumerator
        Return Me
    End Function

    Private Function GetEnumeratorNonGeneric() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return Me
    End Function
End Class

''' <summary>范围，离散索引器描述器，用于表示离散索引器中的一段连续整数区间</summary>
Public Class Range
    Public Lower As Integer
    Public Upper As Integer
    Public Property Count() As Integer
        Get
            Return Upper - Lower + 1
        End Get
        Set(ByVal Value As Integer)
            Upper = Lower + Value - 1
        End Set
    End Property
    Public Sub New(ByVal Lower As Integer, ByVal Upper As Integer)
        If Upper < Lower Then Throw New ArgumentOutOfRangeException
        Me.Lower = Lower
        Me.Upper = Upper
    End Sub
    Public Function Contain(ByVal i As Integer) As Boolean
        Return (i >= Lower) AndAlso (i <= Upper)
    End Function
End Class

''' <summary>范围，离散索引器描述器，用于表示离散索引器中的一段连续整数区间</summary>
Public Class RangeInt64
    Public Lower As Int64
    Public Upper As Int64
    Public Property Count() As Int64
        Get
            Return Upper - Lower + 1
        End Get
        Set(ByVal Value As Int64)
            Upper = Lower + Value - 1
        End Set
    End Property
    Public Sub New(ByVal Lower As Int64, ByVal Upper As Int64)
        If Upper < Lower Then Throw New ArgumentOutOfRangeException
        Me.Lower = Lower
        Me.Upper = Upper
    End Sub
    Public Function Contain(ByVal i As Int64) As Boolean
        Return (i >= Lower) AndAlso (i <= Upper)
    End Function
End Class

''' <summary>范围，离散索引器描述器，用于表示离散索引器中的一段连续整数区间</summary>
Public Class RangeUInt64
    Public Lower As UInt64
    Public Upper As UInt64
    Public Property Count() As UInt64
        Get
            Return Upper - Lower + 1UL
        End Get
        Set(ByVal Value As UInt64)
            Upper = Lower + Value - 1UL
        End Set
    End Property
    Public Sub New(ByVal Lower As UInt64, ByVal Upper As UInt64)
        If Upper < Lower Then Throw New ArgumentOutOfRangeException
        Me.Lower = Lower
        Me.Upper = Upper
    End Sub
    Public Function Contain(ByVal i As UInt64) As Boolean
        Return (i >= Lower) AndAlso (i <= Upper)
    End Function
End Class
