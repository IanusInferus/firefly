'==========================================================================
'
'  File:        Opt.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 可选值
'  Version:     2013.03.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure Opt(Of T)
        Public Property HasValue As Boolean
        Public Property Value As T

        Public Shared Function CreateNotHasValue() As Opt(Of T)
            Return New Opt(Of T) With {.HasValue = False}
        End Function
        Public Shared Function CreateHasValue(ByVal Value As T) As Opt(Of T)
            Return New Opt(Of T) With {.HasValue = True, .Value = Value}
        End Function

        Public ReadOnly Property OnNotHasValue As Boolean
            Get
                Return Not HasValue
            End Get
        End Property
        Public ReadOnly Property OnHasValue As Boolean
            Get
                Return HasValue
            End Get
        End Property

        Public Shared ReadOnly Property Empty As Opt(Of T)
            Get
                Return CreateNotHasValue()
            End Get
        End Property
        Public Shared Widening Operator CType(ByVal v As T) As Opt(Of T)
            If v Is Nothing Then
                Return CreateNotHasValue()
            Else
                Return CreateHasValue(v)
            End If
        End Operator
        Public Shared Narrowing Operator CType(ByVal v As Opt(Of T)) As T
            If Not v.HasValue Then
                Throw New InvalidOperationException
            End If
            Return v.Value
        End Operator
        Public Shared Operator =(ByVal Left As Opt(Of T), ByVal Right As Opt(Of T)) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As Opt(Of T), ByVal Right As Opt(Of T)) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Shared Operator =(ByVal Left As Opt(Of T)?, ByVal Right As Opt(Of T)?) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As Opt(Of T)?, ByVal Right As Opt(Of T)?) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Overrides Function Equals(ByVal Obj As Object) As Boolean
            Return Equals(Me, Obj)
        End Function
        Public Overrides Function GetHashCode() As Integer
            If Not HasValue Then Return 0
            Return Value.GetHashCode()
        End Function

        Private Overloads Shared Function Equals(ByVal Left As Opt(Of T), ByVal Right As Opt(Of T)) As Boolean
            If Left.OnNotHasValue AndAlso Right.OnNotHasValue Then
                Return True
            End If
            If Left.OnNotHasValue OrElse Right.OnNotHasValue Then
                Return False
            End If
            Return Left.Value.Equals(Right.Value)
        End Function
        Private Overloads Shared Function Equals(ByVal Left As Opt(Of T)?, ByVal Right As Opt(Of T)?) As Boolean
            If (Not Left.HasValue OrElse Left.Value.OnNotHasValue) AndAlso (Not Right.HasValue OrElse Right.Value.OnNotHasValue) Then
                Return True
            End If
            If Not Left.HasValue OrElse Left.Value.OnNotHasValue OrElse Not Right.HasValue OrElse Right.Value.OnNotHasValue Then
                Return False
            End If
            Return Equals(Left.Value, Right.Value)
        End Function

        Public Function ValueOrDefault(ByVal [Default] As T) As T
            If OnHasValue Then
                Return Value
            Else
                Return [Default]
            End If
        End Function

        Public Overrides Function ToString() As String
            If HasValue Then
                Return "Opt{" & Value.ToString() & "}"
            Else
                Return "Opt{}"
            End If
        End Function
    End Structure
End Namespace
