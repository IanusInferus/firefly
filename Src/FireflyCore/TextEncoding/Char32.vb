'==========================================================================
'
'  File:        Char32.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: UTF-32 字符
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Diagnostics
Imports System.Runtime.CompilerServices

Namespace TextEncoding

    ''' <summary>UTF-32字符。</summary>
    <DebuggerDisplay("{ToDisplayString()}")> _
    Public Structure Char32
        Implements IEquatable(Of Char32), IComparable(Of Char32)

        Private Unicode As Int32
        Public Sub New(ByVal Unicode As Int32)
            Me.Unicode = Unicode
        End Sub

        ''' <summary>UTF-32值。</summary>
        Public ReadOnly Property Value() As Int32
            Get
                Return Unicode
            End Get
        End Property

        ''' <summary>生成显示用字符串。</summary>
        Public Function ToDisplayString() As String
            Dim List As New List(Of String)
            List.Add(String.Format("U+{0:X4}", Unicode))
            If Not IsControlChar Then List.Add(String.Format("""{0}""", ToString))

            Return "Char32{" & String.Join(", ", List.ToArray) & "}"
        End Function

        ''' <summary>已重载。将UTF-16 Big-Endian转换成Unicode(UTF-32)。</summary>
        Public Overrides Function ToString() As String
            Return ToString(Me)
        End Function


        ''' <summary>指示是否是控制符。</summary>
        Private ReadOnly Property IsControlChar() As Boolean
            Get
                Return Unicode >= 0 AndAlso Unicode <= &H1F
            End Get
        End Property

        ''' <summary>已重载。将Unicode(UTF-32)转换成UTF-16 Big-Endian。</summary>
        Public Overloads Shared Function ToString(ByVal c As Char32) As String
            If c.Unicode >= 0 AndAlso c.Unicode < &H10000 Then
                Return ChrW(c.Unicode)
            ElseIf c.Unicode >= &H10000 AndAlso c.Unicode < &H10FFFF Then
                Dim S0 As Integer
                Dim S1 As Integer
                SplitBits(S1, 10, S0, 10, c.Unicode - &H10000)
                Dim L As Int32 = ConcatBits(&H37, 6, S0, 10) '110111
                Dim H As Int32 = ConcatBits(&H36, 6, S1, 10) '110110
                Return ChrW(H) & ChrW(L)
            Else
                Throw New InvalidDataException
            End If
        End Function

        ''' <summary>将UTF-16 Big-Endian转换成Unicode(UTF-32)。</summary>
        Public Shared Function FromString(ByVal UTF16B As String) As Char32
            If UTF16B = "" Then Return New Char32(0)
            Dim H As Int32 = AscW(UTF16B(0))
            If H >= &HD800 AndAlso H <= &HDBFF Then
                If UTF16B.Length <> 2 Then Throw New InvalidDataException
                Dim L As Int32 = AscW(UTF16B(1))
                If L < &HDC00 OrElse L > &HDFFF Then Throw New InvalidDataException
                Return New Char32(ConcatBits(H.Bits(9, 0), 10, L.Bits(9, 0), 10) + &H10000)
            Else
                If UTF16B.Length <> 1 Then Throw New InvalidDataException
                Return New Char32(H)
            End If
        End Function

        ''' <summary>转换UTF-32字符到32位整数。</summary>
        Public Shared Widening Operator CType(ByVal c As Char32) As Int32
            Return c.Unicode
        End Operator

        ''' <summary>转换32位整数到UTF-32字符。</summary>
        Public Shared Widening Operator CType(ByVal c As Int32) As Char32
            Return New Char32(c)
        End Operator

        ''' <summary>转换UTF-16 Big-Endian字符到UTF-32字符。</summary>
        Public Shared Widening Operator CType(ByVal c As Char) As Char32
            Return FromString(c)
        End Operator

        ''' <summary>转换Uncode(UTF-32)字符到转换UTF-16 Big-Endian字符。</summary>
        Public Shared Narrowing Operator CType(ByVal c As Char32) As Char
            Dim l = c.ToString()
            If l.Length > 1 Then Throw New ArgumentOutOfRangeException
            Return l(0)
        End Operator

        ''' <summary>转换UTF-16 Big-Endian字符串到UTF-32字符。</summary>
        Public Shared Narrowing Operator CType(ByVal c As String) As Char32
            Return FromString(c)
        End Operator

        ''' <summary>转换UTF-32字符到转换UTF-16 Big-Endian字符串。</summary>
        Public Shared Widening Operator CType(ByVal c As Char32) As String
            Return c.ToString()
        End Operator

        ''' <summary>比较两个字符是否相等。</summary>
        Public Overloads Function Equals(ByVal other As Char32) As Boolean Implements System.IEquatable(Of Char32).Equals
            Return Unicode = other.Unicode
        End Function

        ''' <summary>比较两个字符是否相等。</summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing Then Return False
            If Not TypeOf obj Is Char32 Then Return False
            Return Equals(CType(obj, Char32))
        End Function

        ''' <summary>获取字符的HashCode。</summary>
        Public Overrides Function GetHashCode() As Integer
            Return Unicode
        End Function

        ''' <summary>比较两个字符的大小。</summary>
        Public Function CompareTo(ByVal other As Char32) As Integer Implements System.IComparable(Of Char32).CompareTo
            Return Unicode.CompareTo(other.Unicode)
        End Function

        Public Shared Operator =(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return l.Equals(r)
        End Operator

        Public Shared Operator <>(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return Not l.Equals(r)
        End Operator

        Public Shared Operator <(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return l.CompareTo(r) < 0
        End Operator

        Public Shared Operator <=(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return l.CompareTo(r) <= 0
        End Operator

        Public Shared Operator >(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return l.CompareTo(r) > 0
        End Operator

        Public Shared Operator >=(ByVal l As Char32, ByVal r As Char32) As Boolean
            Return l.CompareTo(r) >= 0
        End Operator

        Public Shared Operator &(ByVal l As Char32, ByVal r As Char32) As String
            Return CStr(l) & CStr(r)
        End Operator

        Public Shared Operator &(ByVal l As Char32, ByVal r As String) As String
            Return CStr(l) & r
        End Operator

        Public Shared Operator &(ByVal l As String, ByVal r As Char32) As String
            Return l & CStr(r)
        End Operator

        Public Shared Operator &(ByVal l As Char32, ByVal r As Char32()) As String
            Return CStr(l) & r.ToUTF16B
        End Operator

        Public Shared Operator &(ByVal l As Char32(), ByVal r As Char32) As String
            Return l.ToUTF16B & CStr(r)
        End Operator
    End Structure

    ''' <summary>UTF-32字符串，即Char32()。</summary>
    Public Module String32
        ''' <summary>UTF-32数值转到UTF-32字符。</summary>
        Public Function ChrQ(ByVal u As Int32) As Char32
            Return CType(u, Char32)
        End Function

        ''' <summary>UTF-32字符转到UTF-32数值。</summary>
        Public Function AscQ(ByVal c As Char32) As Int32
            Return CType(c, Int32)
        End Function

        ''' <summary>转换UTF-16 Big-Endian字符串到Uncode(UTF-32)字符串。</summary>
        Public Function FromUTF16B(ByVal s As String) As Char32()
            Dim cl As New List(Of Char32)

            For n As Integer = 0 To s.Length - 1
                Dim c As Char = s(n)
                Dim H As Int32 = AscW(c)
                If H >= &HD800 AndAlso H <= &HDBFF Then
                    cl.Add(CType(c & s(n + 1), Char32))
                    n += 1
                Else
                    cl.Add(CType(c, Char32))
                End If
            Next

            Return cl.ToArray
        End Function

        ''' <summary>转换Uncode(UTF-32)字符串到UTF-16 Big-Endian字符串。</summary>
        <Extension()> Public Function ToUTF16B(ByVal s As IEnumerable(Of Char32)) As String
            Dim sb As New StringBuilder

            For Each c In s
                sb.Append(c.ToString)
            Next

            Return sb.ToString
        End Function
    End Module

    ''' <summary>UTF-16字符串，即String。</summary>
    Public Module String16
        ''' <summary>UTF-16数值转到UTF-16字符。</summary>
        Public Function ChrW(ByVal u As Int16) As Char
            Return Convert.ToChar(CSU(u))
        End Function

        ''' <summary>UTF-16数值转到UTF-16字符。</summary>
        Public Function ChrW(ByVal u As UInt16) As Char
            Return Convert.ToChar(u)
        End Function

        ''' <summary>UTF-32数值转到UTF-16字符。</summary>
        Public Function ChrW(ByVal u As Int32) As Char
            Return Convert.ToChar(u)
        End Function

        ''' <summary>UTF-16字符转到UTF-16数值。</summary>
        Public Function AscW(ByVal c As Char) As UInt16
            Return Convert.ToUInt16(c)
        End Function

        ''' <summary>转换UTF-16 Big-Endian字符串到Uncode(UTF-32)字符串。</summary>
        <Extension()> Public Function ToUTF32(ByVal s As String) As Char32()
            Return String32.FromUTF16B(s)
        End Function

        ''' <summary>转换Uncode(UTF-32)字符串到UTF-16 Big-Endian字符串。</summary>
        Public Function FromUTF32(ByVal s As IEnumerable(Of Char32)) As String
            Return String32.ToUTF16B(s)
        End Function

        ''' <summary>统一换行符为回车换行。</summary>
        <Extension()> Public Function UnifyNewLineToCrLf(ByVal s As String) As String
            Return s.Replace(CrLf, Lf).Replace(Cr, Lf).Replace(Lf, CrLf)
        End Function

        ''' <summary>统一换行符为换行。</summary>
        <Extension()> Public Function UnifyNewLineToLf(ByVal s As String) As String
            Return s.Replace(CrLf, Lf).Replace(Cr, Lf)
        End Function

        ''' <summary>从当前 String 对象移除数组中指定的一个字符的所有前导匹配项。</summary>
        <Extension()> Public Function TrimStart(ByVal s As String, ByVal c As Char32) As String
            Dim s32 = s.ToUTF32
            For n = s32.Length - 1 To 0 Step -1
                If s32(n) <> c Then
                    Return s32.SubArray(0, n + 1).ToUTF16B
                End If
            Next
            Return ""
        End Function

        ''' <summary>从当前 String 对象移除数组中指定的一个字符的所有尾部匹配项。</summary>
        <Extension()> Public Function TrimEnd(ByVal s As String, ByVal c As Char32) As String
            Dim s32 = s.ToUTF32
            For n = 0 To s32.Length - 1
                If s32(n) <> c Then
                    Return s32.SubArray(n).ToUTF16B
                End If
            Next
            Return ""
        End Function

        ''' <summary>从当前 String 对象移除一个指定字符的所有前导匹配项和尾部匹配项。</summary>
        <Extension()> Public Function Trim(ByVal s As String, ByVal c As Char32) As String
            Return s.TrimStart(c).TrimEnd(c)
        End Function
    End Module
End Namespace
