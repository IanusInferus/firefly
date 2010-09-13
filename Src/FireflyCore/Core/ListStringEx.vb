'==========================================================================
'
'  File:        ListStringEx.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 列表式泛型串
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' 列表式泛型串，特性类似于字符串，但是类型参数不是字符，创建后不可改变，可作为容器类的键使用
''' </summary>
Public Class ListStringEx(Of T)
    Implements ICollection(Of T), IEquatable(Of ListStringEx(Of T)), IComparable(Of ListStringEx(Of T)), ICloneable

    Private Internal As IList(Of T)
    Private HashCode As Int32

    ''' <summary>考虑到效率原因，不会复制数据，而是直接引用数据，因此传入的数组不得改变</summary>
    Public Sub New(ByVal Data As IList(Of T))
        If Data Is Nothing Then Throw New ArgumentNullException
        Internal = Data

        Dim hash As Integer = 1315423911
        For Each v In Internal
            hash = ((hash << 5) Xor v.GetHashCode Xor ((hash >> 2) And &H3FFFFFFF))
        Next
        HashCode = hash
    End Sub

    ''' <summary>已重载。考虑到效率原因，不会复制数据，而是直接引用数据，因此传入的数组不得改变</summary>
    Public Sub New(ByVal Data As IList(Of T), ByVal HashCode As Int32)
        If Data Is Nothing Then Throw New ArgumentNullException
        Internal = Data

        Me.HashCode = HashCode
    End Sub

    Public Overloads Function Equals(ByVal other As ListStringEx(Of T)) As Boolean Implements System.IEquatable(Of ListStringEx(Of T)).Equals
        If Me Is other Then Return True
        If other Is Nothing Then Return False
        With other
            If Internal.Count <> other.Internal.Count Then Return False
            Dim ec = EqualityComparer(Of T).Default
            Using e As New ZippedEnumerator(Of T, T, Boolean)(Me.GetEnumerator, other.GetEnumerator, Function(a, b) ec.Equals(a, b))
                Return (New EnumeratorEnumerable(Of Boolean)(e)).All(Function(b) b)
            End Using
        End With
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If Me Is obj Then Return True
        If obj Is Nothing Then Return False
        Dim s = TryCast(obj, ListStringEx(Of T))
        If s Is Nothing Then Return False
        Return Equals(s)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return HashCode
    End Function

    Public Function CompareTo(ByVal other As ListStringEx(Of T)) As Integer Implements System.IComparable(Of ListStringEx(Of T)).CompareTo
        If other Is Nothing Then Return Integer.MinValue
        With other
            Dim Left As IEnumerable(Of T) = Me
            Dim Right As IEnumerable(Of T) = other
            Dim LeftCount = Left.Count
            Dim RightCount = Right.Count
            Dim r = LeftCount - RightCount
            If r < 0 Then
                Right = Right.Take(LeftCount)
            ElseIf r > 0 Then
                Left = Left.Take(RightCount)
            End If
            Dim c = Comparer(Of T).Default
            Using e As New ZippedEnumerator(Of T, T, Integer)(Left.GetEnumerator, Right.GetEnumerator, Function(a, b) c.Compare(a, b))
                Dim r2 = (New EnumeratorEnumerable(Of Integer)(e)).FirstOrDefault(Function(i) i <> 0)
                If r2 <> 0 Then Return r2
                Return r
            End Using
        End With
    End Function

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return Internal.GetEnumerator()
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

    Public Function Contains(ByVal Item As T) As Boolean Implements ICollection(Of T).Contains
        Return Internal.Contains(Item)
    End Function

    Public Sub CopyTo(ByVal Array() As T, ByVal ArrayIndex As Integer) Implements ICollection(Of T).CopyTo
        For n = 0 To Internal.Count - 1
            Array(ArrayIndex + n) = Internal(n)
        Next
    End Sub

    Public ReadOnly Property Count() As Integer Implements ICollection(Of T).Count
        Get
            Return Internal.Count
        End Get
    End Property

    Public Function Reverse() As ListStringEx(Of T)
        Return Reverse(0, Internal.Count)
    End Function

    Public Function Reverse(ByVal Index As Integer, ByVal Count As Integer) As ListStringEx(Of T)
        If Index < 0 OrElse Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        If Index + Count > Internal.Count Then Throw New ArgumentOutOfRangeException
        Dim Result = Internal.ToArray
        For i = 0 To Count \ 2 - 1
            Exchange(Result(Index + i), Internal(Index + Count - 1 - i))
        Next
        Return New ListStringEx(Of T)(Result)
    End Function

    Public Shared Widening Operator CType(ByVal l As ListStringEx(Of T)) As StringEx(Of T)
        Return New StringEx(Of T)(l.Internal, l.HashCode)
    End Operator

    Public Function IndexOf(ByVal Value As T) As Integer
        Return IndexOf(Value, 0, Internal.Count)
    End Function

    Public Function IndexOf(ByVal Value As T, ByVal Index As Integer) As Integer
        Return IndexOf(Value, Index, Internal.Count - Index)
    End Function

    Public Function IndexOf(ByVal Value As T, ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        If Index + Count > Internal.Count Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index To Index + Count - 1
            If ec.Equals(Me(p), Value) Then
                Return p
            End If
        Next
        Return -1
    End Function

    Public Function IndexOf(ByVal Value As ListStringEx(Of T)) As Integer
        Return IndexOf(Value, 0, Internal.Count)
    End Function

    Public Function IndexOf(ByVal Value As ListStringEx(Of T), ByVal Index As Integer) As Integer
        Return IndexOf(Value, Index, Internal.Count - Index)
    End Function

    Public Function IndexOf(ByVal Value As ListStringEx(Of T), ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        If Index + Count > Internal.Count Then Throw New ArgumentOutOfRangeException
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
        Return LastIndexOf(Value, Internal.Count - 1, Internal.Count)
    End Function

    Public Function LastIndexOf(ByVal Value As T, ByVal Index As Integer) As Integer
        Return LastIndexOf(Value, Index, Index + 1)
    End Function

    Public Function LastIndexOf(ByVal Value As T, ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        If Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        Dim ec = EqualityComparer(Of T).Default
        For p As Integer = Index To Index - Count + 1 Step -1
            If ec.Equals(Me(p), Value) Then
                Return p
            End If
        Next
        Return -1
    End Function

    Public Function LastIndexOf(ByVal Value As ListStringEx(Of T)) As Integer
        Return LastIndexOf(Value, Internal.Count - 1, Internal.Count)
    End Function

    Public Function LastIndexOf(ByVal Value As ListStringEx(Of T), ByVal Index As Integer) As Integer
        Return LastIndexOf(Value, Index, Index + 1)
    End Function

    Public Function LastIndexOf(ByVal Value As ListStringEx(Of T), ByVal Index As Integer, ByVal Count As Integer) As Integer
        If Index < 0 OrElse Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
        If Index >= Internal.Count Then Throw New ArgumentOutOfRangeException
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

    Public Shared Operator =(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        If Left Is Nothing AndAlso Right Is Nothing Then Return True
        If Left Is Nothing OrElse Right Is Nothing Then Return False
        Return Left.Equals(Right)
    End Operator

    Public Shared Operator <>(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        If Left Is Nothing AndAlso Right Is Nothing Then Return False
        If Left Is Nothing OrElse Right Is Nothing Then Return True
        Return Not Left.Equals(Right)
    End Operator

    Public Shared Operator >=(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        If Right Is Nothing Then Return True
        If Left Is Nothing Then Return False
        Return Left.CompareTo(Right) >= 0
    End Operator

    Public Shared Operator <=(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        If Left Is Nothing Then Return True
        If Right Is Nothing Then Return False
        Return Left.CompareTo(Right) <= 0
    End Operator

    Public Shared Operator >(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        Return Not (Left <= Right)
    End Operator

    Public Shared Operator <(ByVal Left As ListStringEx(Of T), ByVal Right As ListStringEx(Of T)) As Boolean
        Return Not (Left >= Right)
    End Operator

    Public Function Substring(ByVal Index As Integer, ByVal Length As Integer) As ListStringEx(Of T)
        Return New ListStringEx(Of T)(New ListPart(Of T)(Internal, Index, Length))
    End Function

    Public Function Substring(ByVal Index As Integer) As ListStringEx(Of T)
        Return New ListStringEx(Of T)(New ListPart(Of T)(Internal, Index, Internal.Count - Index))
    End Function
End Class
