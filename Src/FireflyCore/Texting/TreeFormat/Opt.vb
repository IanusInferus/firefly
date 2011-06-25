'==========================================================================
'
'  File:        Opt.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 可选值
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Class Opt(Of T)
        Public Property HasValue As Boolean
        Public Property Value As T

        Public Shared ReadOnly Property Empty As Opt(Of T)
            Get
                Static v As New Opt(Of T) With {.HasValue = False, .Value = Nothing}
                Return v
            End Get
        End Property

        Public Shared Widening Operator CType(ByVal v As T) As Opt(Of T)
            Return New Opt(Of T) With {.HasValue = True, .Value = v}
        End Operator

        Public Overrides Function ToString() As String
            If HasValue Then
                Return "Opt{" & Value.ToString() & "}"
            Else
                Return "Opt{}"
            End If
        End Function

        Public Overrides Function Equals(Obj As Object) As Boolean
            Dim o = TryCast(Obj, Opt(Of T))
            If o Is Nothing Then Return False
            If (Not HasValue) AndAlso (Not o.HasValue) Then Return True
            Return Value.Equals(o.Value)
        End Function

        Public Overrides Function GetHashCode() As Integer
            If Not HasValue Then Return 0
            Return Value.GetHashCode()
        End Function
    End Class
End Namespace
