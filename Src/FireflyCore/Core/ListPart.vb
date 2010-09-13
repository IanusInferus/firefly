'==========================================================================
'
'  File:        ListPart.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 列表片
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' 列表片，列表的一部分
''' </summary>
Public NotInheritable Class ListPart(Of T)
    Implements IList(Of T), ICloneable

    Private Internal As IList(Of T)
    Private InternalOffset As Integer
    Private InternalLength As Integer

    ''' <summary>已重载。考虑到效率原因，不会复制数据，而是直接引用数据，因此传入的数组不得改变</summary>
    Public Sub New(ByVal Data As IList(Of T))
        If Data Is Nothing Then Throw New ArgumentNullException
        Internal = Data
        InternalOffset = 0
        InternalLength = Data.Count
    End Sub
    ''' <summary>已重载。考虑到效率原因，不会复制数据，而是直接引用数据，因此传入的数组不得改变</summary>
    Public Sub New(ByVal Data As IList(Of T), ByVal Offset As Integer, ByVal Length As Integer)
        If Data Is Nothing Then Throw New ArgumentNullException
        If Offset < 0 OrElse Offset >= Data.Count Then Throw New ArgumentOutOfRangeException
        Internal = Data
        InternalOffset = Offset
        InternalLength = Length
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return New MappedEnumerator(Of Integer, T)(Enumerable.Range(InternalOffset, InternalLength).GetEnumerator, Function(i) Internal(i))
    End Function

    Private Function GetEnumeratorNonGeneric() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Public Function Clone() As Object Implements System.ICloneable.Clone
        Return Me.MemberwiseClone
    End Function

    Private ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of T).IsReadOnly
        Get
            Return True
        End Get
    End Property
    Private Sub Add(ByVal Item As T) Implements ICollection(Of T).Add
        Throw New NotSupportedException
    End Sub
    Private Sub Clear() Implements ICollection(Of T).Clear
        Throw New NotSupportedException
    End Sub
    Private Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
        Throw New NotSupportedException
    End Function
    Private Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
        Throw New NotSupportedException
    End Sub
    Private Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
        Throw New NotSupportedException
    End Sub

    Public Function Contains(ByVal Item As T) As Boolean Implements ICollection(Of T).Contains
        Return IndexOf(Item) >= 0
    End Function

    Public Sub CopyTo(ByVal Array() As T, ByVal ArrayIndex As Integer) Implements ICollection(Of T).CopyTo
        For n = 0 To InternalLength - 1
            Array(ArrayIndex + n) = Internal(InternalOffset + n)
        Next
    End Sub

    Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count
        Get
            Return InternalLength
        End Get
    End Property

    Default Public Property Value(ByVal Index As Integer) As T Implements IList(Of T).Item
        Get
            If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
            Return Internal(InternalOffset + Index)
        End Get
        Set(ByVal Item As T)
            If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
            Internal(InternalOffset + Index) = Item
        End Set
    End Property

    Public Sub Reverse()
        Reverse(0, InternalLength)
    End Sub

    Public Sub Reverse(ByVal Index As Integer, ByVal Count As Integer)
        If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        If Index + Count > InternalLength Then Throw New ArgumentOutOfRangeException
        For i = 0 To Count \ 2 - 1
            Exchange(Internal(Index + i), Internal(Index + Count - 1 - i))
        Next
    End Sub

    Public Function IndexOf(ByVal Value As T) As Integer Implements IList(Of T).IndexOf
        Return IndexOf(Value, 0, InternalLength)
    End Function

    Public Function IndexOf(ByVal Value As T, ByVal Index As Integer) As Integer
        Return IndexOf(Value, Index, InternalLength - Index)
    End Function

    Public Function IndexOf(ByVal Value As T, ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        If Index + Count > InternalLength Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index To Index + Count - 1
            If ec.Equals(Me(p), Value) Then
                Return p
            End If
        Next
        Return -1
    End Function

    Public Function IndexOf(ByVal Value As IList(Of T)) As Integer
        Return IndexOf(Value, 0, InternalLength)
    End Function

    Public Function IndexOf(ByVal Value As IList(Of T), ByVal Index As Integer) As Integer
        Return IndexOf(Value, Index, InternalLength - Index)
    End Function

    Public Function IndexOf(ByVal Value As IList(Of T), ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        If Index + Count > InternalLength Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index To Index + Count - Value.Count
            Dim Flag As Boolean = True
            For k As Integer = 0 To Value.Count - 1
                If Not ec.Equals(Me(p + k), Value(k)) Then
                    Flag = False
                    Exit For
                End If
            Next
            If Flag Then Return p
        Next
        Return -1
    End Function

    Public Function LastIndexOf(ByVal Value As T) As Integer
        Return LastIndexOf(Value, InternalLength - 1, InternalLength)
    End Function

    Public Function LastIndexOf(ByVal Value As T, ByVal Index As Integer) As Integer
        Return LastIndexOf(Value, Index, Index + 1)
    End Function

    Public Function LastIndexOf(ByVal Value As T, ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        If Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index To Index - Count + 1 Step -1
            If ec.Equals(Me(p), Value) Then
                Return p
            End If
        Next
        Return -1
    End Function

    Public Function LastIndexOf(ByVal Value As IList(Of T)) As Integer
        Return LastIndexOf(Value, InternalLength - 1, InternalLength)
    End Function

    Public Function LastIndexOf(ByVal Value As IList(Of T), ByVal Index As Integer) As Integer
        Return LastIndexOf(Value, Index, Index + 1)
    End Function

    Public Function LastIndexOf(ByVal Value As IList(Of T), ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        If Index >= InternalLength Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index - Value.Count + 1 To Index - Count + 1 Step -1
            Dim Flag As Boolean = True
            For k As Integer = 0 To Value.Count - 1
                If Not ec.Equals(Me(p + k), Value(k)) Then
                    Flag = False
                    Exit For
                End If
            Next
            If Flag Then Return p
        Next
        Return -1
    End Function
End Class
