'==========================================================================
'
'  File:        Enumerators.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 枚举器
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic

''' <summary>映射式枚举器</summary>
Public Class MappedEnumerator(Of TKey, TValue)
    Implements IEnumerator(Of TValue)

    Private BaseEnumerator As IEnumerator(Of TKey)
    Private Mapping As Func(Of TKey, TValue)

    Public Sub New(ByVal BaseEnumerator As IEnumerator(Of TKey), ByVal Mapping As Func(Of TKey, TValue))
        Me.BaseEnumerator = BaseEnumerator
        Me.Mapping = Mapping
    End Sub

    Public ReadOnly Property Current() As TValue Implements IEnumerator(Of TValue).Current
        Get
            Return Mapping(BaseEnumerator.Current)
        End Get
    End Property

    Private ReadOnly Property GetEnumeratorNonGeneric() As Object Implements IEnumerator.Current
        Get
            Return Current()
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Return BaseEnumerator.MoveNext
    End Function

    Public Sub Reset() Implements IEnumerator.Reset
        BaseEnumerator.Reset()
    End Sub

#Region " IDisposable 支持 "
    Private disposedValue As Boolean = False '检测冗余的调用
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                '释放其他状态(托管对象)。
                BaseEnumerator.Dispose()
            End If

            '释放您自己的状态(非托管对象)。
            '将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        '不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

''' <summary>Zip式枚举器</summary>
Public Class ZippedEnumerator(Of TKeyA, TKeyB, TValue)
    Implements IEnumerator(Of TValue)

    Private BaseEnumeratorA As IEnumerator(Of TKeyA)
    Private BaseEnumeratorB As IEnumerator(Of TKeyB)
    Private Zipping As Func(Of TKeyA, TKeyB, TValue)

    Public Sub New(ByVal BaseEnumeratorA As IEnumerator(Of TKeyA), ByVal BaseEnumeratorB As IEnumerator(Of TKeyB), ByVal Zipping As Func(Of TKeyA, TKeyB, TValue))
        Me.BaseEnumeratorA = BaseEnumeratorA
        Me.BaseEnumeratorB = BaseEnumeratorB
        Me.Zipping = Zipping
    End Sub

    Public ReadOnly Property Current() As TValue Implements IEnumerator(Of TValue).Current
        Get
            Return Zipping(BaseEnumeratorA.Current, BaseEnumeratorB.Current)
        End Get
    End Property

    Private ReadOnly Property GetEnumeratorNonGeneric() As Object Implements IEnumerator.Current
        Get
            Return Current()
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Dim ResultA = BaseEnumeratorA.MoveNext
        Dim ResultB = BaseEnumeratorB.MoveNext
        If ResultA <> ResultB Then Throw New InvalidOperationException
        Return ResultA
    End Function

    Public Sub Reset() Implements IEnumerator.Reset
        BaseEnumeratorA.Reset()
        BaseEnumeratorB.Reset()
    End Sub

#Region " IDisposable 支持 "
    Private disposedValue As Boolean = False '检测冗余的调用
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                '释放其他状态(托管对象)。
                BaseEnumeratorA.Dispose()
                BaseEnumeratorB.Dispose()
            End If

            '释放您自己的状态(非托管对象)。
            '将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        '不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

''' <summary>用于转换泛型Enumerator到泛型IEnumerable</summary>
Public Class EnumeratorEnumerable(Of T)
    Implements IEnumerable(Of T)

    Private BaseEnumerator As IEnumerator(Of T)

    Public Sub New(ByVal BaseEnumerator As IEnumerator(Of T))
        Me.BaseEnumerator = BaseEnumerator
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return BaseEnumerator
    End Function

    Private Function GetEnumeratorNonGeneric() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class
