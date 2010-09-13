'==========================================================================
'
'  File:        N32.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 32位非负整数环
'  Version:     2010.05.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Globalization

<DebuggerDisplay("{v}")>
Public Structure N32
    Implements IFormattable
    Implements IEquatable(Of N32)
    Implements IComparable(Of N32)

    <DebuggerBrowsable(DebuggerBrowsableState.Never)>
    Public Shared ReadOnly MinValue As N32 = New N32(0)
    <DebuggerBrowsable(DebuggerBrowsableState.Never)>
    Public Shared ReadOnly MaxValue As N32 = New N32(&HFFFFFFFFUI)

    <DebuggerBrowsable(DebuggerBrowsableState.Never)>
    Private v As UInt32

    Public Sub New(ByVal Value As UInt32)
        v = Value
    End Sub

    Public Sub New(ByVal Value As Int32)
        v = CSU(Value)
    End Sub

    <DebuggerBrowsable(DebuggerBrowsableState.Never)>
    Public Property Value As UInt32
        Get
            Return v
        End Get
        Set(ByVal val As UInt32)
            v = val
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return v.ToString()
    End Function
    Public Overloads Function ToString(ByVal Format As String) As String
        Return v.ToString(Format)
    End Function
    Public Overloads Function ToString(ByVal Provider As IFormatProvider) As String
        Return v.ToString(Provider)
    End Function
    Public Overloads Function ToString(ByVal Format As String, ByVal Provider As IFormatProvider) As String Implements System.IFormattable.ToString
        Return v.ToString(Format, Provider)
    End Function
    Public Shared Function Parse(ByVal s As String) As N32
        Return UInt32.Parse(s)
    End Function
    Public Shared Function Parse(ByVal s As String, ByVal Style As NumberStyles) As N32
        Return UInt32.Parse(s, Style)
    End Function
    Public Shared Function Parse(ByVal s As String, ByVal Provider As IFormatProvider) As N32
        Return UInt32.Parse(s, Provider)
    End Function
    Public Shared Function Parse(ByVal s As String, ByVal Style As NumberStyles, ByVal Provider As IFormatProvider) As N32
        Return UInt32.Parse(s, Style, Provider)
    End Function
    Public Shared Function TryParse(ByVal s As String, <Out()> ByRef Result As N32) As Boolean
        Return UInt32.TryParse(s, Result)
    End Function
    Public Shared Function TryParse(ByVal s As String, ByVal Style As NumberStyles, ByVal Provider As IFormatProvider, <Out()> ByRef Result As N32) As Boolean
        Return UInt32.TryParse(s, Style, Provider, Result)
    End Function

    Public Shared Widening Operator CType(ByVal Value As UInt32) As N32
        Return New N32(Value)
    End Operator
    Public Shared Widening Operator CType(ByVal Value As N32) As UInt32
        Return Value.v
    End Operator
    Public Shared Narrowing Operator CType(ByVal Value As Int32) As N32
        Return New N32(Value)
    End Operator
    Public Shared Narrowing Operator CType(ByVal Value As N32) As Int32
        Return CUS(Value.v)
    End Operator

    Public Shared Operator Not(ByVal R As N32) As N32
        Return Not R.v
    End Operator
    Public Shared Operator And(ByVal L As N32, ByVal R As N32) As N32
        Return L.v And R.v
    End Operator
    Public Shared Operator Or(ByVal L As N32, ByVal R As N32) As N32
        Return L.v Or R.v
    End Operator
    Public Shared Operator Xor(ByVal L As N32, ByVal R As N32) As N32
        Return L.v Xor R.v
    End Operator
    Public Shared Operator <<(ByVal L As N32, ByVal R As Integer) As N32
        Return L.v.SHL(R)
    End Operator
    Public Shared Operator >>(ByVal L As N32, ByVal R As Integer) As N32
        Return L.v.SHR(R)
    End Operator

    Public Shared Operator +(ByVal R As N32) As N32
        Return R
    End Operator
    Public Shared Operator -(ByVal R As N32) As N32
        Return Not (R - New N32(1))
    End Operator
    Public Shared Operator +(ByVal L As N32, ByVal R As N32) As N32
        Return CUInt((CLng(L.v) + CLng(R.v)) And &HFFFFFFFFL)
    End Operator
    Public Shared Operator -(ByVal L As N32, ByVal R As N32) As N32
        Return CUInt((CLng(L.v) - CLng(R.v)) And &HFFFFFFFFL)
    End Operator
    Public Shared Operator *(ByVal L As N32, ByVal R As N32) As N32
        Return CUInt((CLng(L.v) * CLng(R.v)) And &HFFFFFFFFL)
    End Operator
    Public Shared Operator \(ByVal L As N32, ByVal R As N32) As N32
        Return L.v \ R.v
    End Operator
    Public Shared Operator ^(ByVal L As N32, ByVal R As Integer) As N32
        If R < 0 Then Throw New ArgumentOutOfRangeException
        Dim v As N32 = 1UI
        Dim pow As N32 = L
        For n = 0 To 31
            If R = 0 Then Exit For
            If (R And (1 << n)) <> 0 Then
                v *= pow
                R = R And Not (1 << n)
            End If
            pow = pow * pow
        Next
        Return v
    End Operator

    Public Shared Operator =(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v = R.v
    End Operator
    Public Shared Operator <>(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v <> R.v
    End Operator
    Public Shared Operator >=(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v >= R.v
    End Operator
    Public Shared Operator <=(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v <= R.v
    End Operator
    Public Shared Operator >(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v > R.v
    End Operator
    Public Shared Operator <(ByVal L As N32, ByVal R As N32) As Boolean
        Return L.v < R.v
    End Operator

    Public Overrides Function Equals(ByVal Other As Object) As Boolean
        Return (TypeOf Other Is N32) AndAlso (v = CType(Other, N32).v)
    End Function
    Public Overloads Function Equals(ByVal Other As N32) As Boolean Implements IEquatable(Of N32).Equals
        Return v.Equals(Other.v)
    End Function
    Public Function CompareTo(ByVal Other As N32) As Integer Implements IComparable(Of N32).CompareTo
        Return v.CompareTo(Other.v)
    End Function
End Structure
