'==========================================================================
'
'  File:        Opt.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: 可选值
'  Version:     2019.04.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Diagnostics
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat
    Public Enum OptionalTag
        None = 0
        Some = 1
    End Enum
    <TaggedUnion>
    <Record(), DebuggerDisplay("{ToString()}")>
    Public Structure [Optional](Of T)
        <Tag> Public _Tag As OptionalTag

        Public None As Unit
        Public Some As T

        Public Shared Function CreateNone() As [Optional](Of T)
            Return New [Optional](Of T) With {._Tag = OptionalTag.None, .None = New Unit()}
        End Function
        Public Shared Function CreateSome(ByVal Value As T) As [Optional](Of T)
            Return New [Optional](Of T) With {._Tag = OptionalTag.Some, .Some = Value}
        End Function

        Public ReadOnly Property OnNone As Boolean
            Get
                Return _Tag = OptionalTag.None
            End Get
        End Property
        Public ReadOnly Property OnSome As Boolean
            Get
                Return _Tag = OptionalTag.Some
            End Get
        End Property

        Public Shared ReadOnly Property Empty As [Optional](Of T)
            Get
                Return CreateNone()
            End Get
        End Property
        Public Shared Widening Operator CType(ByVal v As T) As [Optional](Of T)
            If v Is Nothing Then Return CreateNone()
            Return CreateSome(v)
        End Operator
        Public Shared Narrowing Operator CType(ByVal v As [Optional](Of T)) As T
            If v.OnNone Then Throw New InvalidOperationException()
            Return v.Some
        End Operator
        Public Shared Operator =(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Shared Operator =(ByVal Left As [Optional](Of T)?, ByVal Right As [Optional](Of T)?) As Boolean
            Return Equals(Left, Right)
        End Operator
        Public Shared Operator <>(ByVal Left As [Optional](Of T)?, ByVal Right As [Optional](Of T)?) As Boolean
            Return Not Equals(Left, Right)
        End Operator
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing Then Return Equals(Me, Nothing)
            If obj.GetType() <> GetType([Optional](Of T)) Then Return False
            Dim o = CType(obj, [Optional](Of T))
            Return Equals(Me, o)
        End Function
        Public Overrides Function GetHashCode() As Int32
            If OnNone Then Return 0
            Return Some.GetHashCode()
        End Function

        Private Overloads Shared Function Equals(ByVal Left As [Optional](Of T), ByVal Right As [Optional](Of T)) As Boolean
            If Left.OnNone AndAlso Right.OnNone Then Return True
            If Left.OnNone OrElse Right.OnNone Then Return False
            Return Left.Some.Equals(Right.Some)
        End Function
        Private Overloads Shared Function Equals(ByVal Left As [Optional](Of T)?, ByVal Right As [Optional](Of T)?) As Boolean
            If (Not Left.HasValue OrElse Left.Value.OnNone) AndAlso (Not Right.HasValue OrElse Right.Value.OnNone) Then Return True
            If Not Left.HasValue OrElse Left.Value.OnNone OrElse Not Right.HasValue OrElse Right.Value.OnNone Then Return False
            Return Equals(Left.Value, Right.Value)
        End Function

        Public ReadOnly Property Value() As T
            Get
                If OnSome Then
                    Return Some
                Else
                    Throw New InvalidOperationException()
                End If
            End Get
        End Property
        Public Function ValueOrDefault(ByVal [Default] As T) As T
            If OnSome Then
                Return Some
            Else
                Return [Default]
            End If
        End Function

        Public Overrides Function ToString() As String
            If OnSome Then
                Return Some.ToString()
            Else
                Return "-"
            End If
        End Function
    End Structure

End Namespace
