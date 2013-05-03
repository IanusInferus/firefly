'==========================================================================
'
'  File:        AbstractBitmap.vb
'  Location:    Firefly.Imaging <Visual Basic .Net>
'  Description: 抽象位图
'  Version:     2013.05.03.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System

Namespace Imaging
    Public NotInheritable Class AbstractBitmap(Of T)
        Private WidthValue As Integer
        Private HeightValue As Integer
        Private Elements As T()
        Public Sub New(ByVal Width As Integer, ByVal Height As Integer)
            WidthValue = Width
            HeightValue = Height
            Elements = New T(Width * Height - 1) {}
        End Sub

        Public ReadOnly Property Width As Integer
            Get
                Return WidthValue
            End Get
        End Property
        Public ReadOnly Property Height As Integer
            Get
                Return HeightValue
            End Get
        End Property

        Public ReadOnly Property Data As T()
            Get
                Return Elements
            End Get
        End Property

        Default Public Property Pixel(ByVal x As Integer, ByVal y As Integer) As T
            Get
                If x < 0 OrElse x > WidthValue - 1 OrElse y < 0 OrElse y > HeightValue - 1 Then Return Nothing
                Return Elements(x + y * WidthValue)
            End Get
            Set(ByVal Value As T)
                If x < 0 OrElse x > WidthValue - 1 OrElse y < 0 OrElse y > HeightValue - 1 Then Return
                Elements(x + y * WidthValue) = Value
            End Set
        End Property

        Public Property Rectangle(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As T()
            Get
                Return GetRectangle(x, y, w, h)
            End Get
            Set(ByVal Value As T())
                SetRectangle(x, y, w, h, Value)
            End Set
        End Property

        Public Function GetRectangle(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As T()
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r = New T(w * h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i + j * w) = Elements((x + i) + (y + j) * WidthValue)
                Next
            Next
            Return r
        End Function

        Public Sub SetRectangle(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer, ByVal r As T())
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            If r.Length <> w * h Then Throw New ArgumentException
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = r(i + j * w)
                Next
            Next
        End Sub

        Public Function GetRectangle2(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As T(,)
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r = New T(w - 1, h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i, j) = Elements((x + i) + (y + j) * WidthValue)
                Next
            Next
            Return r
        End Function

        Public Sub SetRectangle2(ByVal x As Integer, ByVal y As Integer, ByVal r As T(,))
            Dim w = r.GetLength(0)
            Dim h = r.GetLength(1)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = r(i, j)
                Next
            Next
        End Sub

        Public Function GetBitmap(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As AbstractBitmap(Of T)
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r As New AbstractBitmap(Of T)(w, h)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i, j) = Elements((x + i) + (y + j) * WidthValue)
                Next
            Next
            Return r
        End Function

        Public Sub SetBitmap(ByVal x As Integer, ByVal y As Integer, ByVal r As AbstractBitmap(Of T))
            Dim w = r.Width
            Dim h = r.Height
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = r(i, j)
                Next
            Next
        End Sub

        Public Function GetRectangle(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer, ByVal Map As Func(Of T, M)) As M()
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r = New M(w * h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i + j * w) = Map(Elements((x + i) + (y + j) * WidthValue))
                Next
            Next
            Return r
        End Function

        Public Sub SetRectangle(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer, ByVal Map As Func(Of M, T), ByVal r As M())
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            If r.Length <> w * h Then Throw New ArgumentException
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = Map(r(i + j * w))
                Next
            Next
        End Sub

        Public Function GetRectangle2(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer, ByVal Map As Func(Of T, M)) As M(,)
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r = New M(w - 1, h - 1) {}
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i, j) = Map(Elements((x + i) + (y + j) * WidthValue))
                Next
            Next
            Return r
        End Function

        Public Sub SetRectangle2(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal Map As Func(Of M, T), ByVal r As M(,))
            Dim w = r.GetLength(0)
            Dim h = r.GetLength(1)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = Map(r(i, j))
                Next
            Next
        End Sub

        Public Function GetBitmap(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer, ByVal Map As Func(Of T, M)) As AbstractBitmap(Of M)
            If w < 0 OrElse h < 0 Then Throw New ArgumentException
            Dim r As New AbstractBitmap(Of M)(w, h)
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    r(i, j) = Map(Elements((x + i) + (y + j) * WidthValue))
                Next
            Next
            Return r
        End Function

        Public Sub SetBitmap(Of M)(ByVal x As Integer, ByVal y As Integer, ByVal Map As Func(Of M, T), ByVal r As AbstractBitmap(Of M))
            Dim w = r.Width
            Dim h = r.Height
            Dim jb = Max(0, -y)
            Dim je = Min(y + h, HeightValue) - y
            Dim ib = Max(0, -x)
            Dim ie = Min(x + w, WidthValue) - x
            For j = jb To je - 1
                For i = ib To ie - 1
                    Elements((x + i) + (y + j) * WidthValue) = Map(r(i, j))
                Next
            Next
        End Sub
    End Class
End Namespace
