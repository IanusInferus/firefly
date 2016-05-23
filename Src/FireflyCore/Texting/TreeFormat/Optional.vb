'==========================================================================
'
'  File:        Opt.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 可选值
'  Version:     2016.05.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat
    Public Enum OptionalTag
        NotHasValue = 0
        HasValue = 1
    End Enum
    <TaggedUnion>
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure [Optional](Of T)
        <Tag> Public _Tag As OptionalTag

        Public NotHasValue As Unit
        Public HasValue As T

        Public Shared Function CreateNotHasValue() As [Optional](Of T)
            Return New [Optional](Of T) With {._Tag = OptionalTag.NotHasValue, .NotHasValue = New Unit()}
        End Function
        Public Shared Function CreateHasValue(ByVal Value As T) As [Optional](Of T)
            Return New [Optional](Of T) With {._Tag = OptionalTag.HasValue, .HasValue = Value}
        End Function

        Public ReadOnly Property OnNotHasValue As Boolean
            Get
                Return _Tag = OptionalTag.NotHasValue
            End Get
        End Property
        Public ReadOnly Property OnHasValue As Boolean
            Get
                Return _Tag = OptionalTag.HasValue
            End Get
        End Property

        Public Shared ReadOnly Property Empty As [Optional](Of T)
            Get
                Return CreateNotHasValue()
            End Get
        End Property
        Public Shared Widening Operator CType(ByVal v As T) As [Optional](Of T)
            If v Is Nothing Then Return CreateNotHasValue()
            Return CreateHasValue(v)
        End Operator
        Public Shared Narrowing Operator CType(ByVal v As [Optional](Of T)) As T
            If v.OnNotHasValue Then Throw New InvalidOperationException()
            Return v.HasValue
        End Operator
        Public Shared Operator =(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Shared Operator =(ByVal Left As [Optional](Of T) ?, ByVal Right As [Optional](Of T) ?) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As [Optional](Of T) ?, ByVal Right As [Optional](Of T) ?) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing Then Return Equals(Me, Nothing)
            If obj.GetType() <> GetType([Optional](Of T)) Then Return False
            Dim o = CType(obj, [Optional](Of T))
            Return Equals(Me, o)
        End Function
        Public Overrides Function GetHashCode() As Int32
            If OnNotHasValue Then Return 0
            Return HasValue.GetHashCode()
        End Function

        Private Overloads Shared Function Equals(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            If Left.OnNotHasValue AndAlso Right.OnNotHasValue Then Return True
            If Left.OnNotHasValue OrElse Right.OnNotHasValue Then Return False
            Return Left.HasValue.Equals(Right.HasValue)
        End Function
        Private Overloads Shared Function Equals(ByVal Left As [Optional](Of T) ?, ByVal Right As [Optional](Of T) ?) As Boolean
            If (Not Left.HasValue OrElse Left.Value.OnNotHasValue) AndAlso (Not Right.HasValue OrElse Right.Value.OnNotHasValue) Then Return True
            If Not Left.HasValue OrElse Left.Value.OnNotHasValue OrElse Not Right.HasValue OrElse Right.Value.OnNotHasValue Then Return False
            Return Equals(Left.Value, Right.Value)
        End Function

        Public ReadOnly Property Value() As T
            Get
                If OnHasValue Then
                    Return HasValue
                Else
                    Throw New InvalidOperationException()
                End If
            End Get
        End Property
        Public Function ValueOrDefault(ByVal [Default] As T) As T
            If OnHasValue Then
                Return HasValue
            Else
                Return [Default]
            End If
        End Function

        Public Overrides Function ToString() As String
            If OnHasValue Then
                Return HasValue.ToString()
            Else
                Return "-"
            End If
        End Function
    End Structure

End Namespace
