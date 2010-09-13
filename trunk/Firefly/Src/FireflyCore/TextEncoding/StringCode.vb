'==========================================================================
'
'  File:        StringCode.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: 字符串码点信息
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Diagnostics
Imports System.Runtime.CompilerServices

Namespace TextEncoding

    ''' <summary>字符串码点值对，可用于码点转换。</summary>
    <DebuggerDisplay("{ToString()}")> _
    Public Class StringCode
        Implements IEquatable(Of StringCode)

        Private UnicodesValue As StringEx(Of Char32) = Nothing
        Private CodesValue As StringEx(Of Byte) = Nothing

        ''' <summary>Unicode字符串。</summary>
        Public Property Unicodes As StringEx(Of Char32)
            Get
                Return UnicodesValue
            End Get
            Set(ByVal Value As StringEx(Of Char32))
                UnicodesValue = Value
            End Set
        End Property

        ''' <summary>自定义编码串。</summary>
        Public Property Codes As StringEx(Of Byte)
            Get
                Return CodesValue
            End Get
            Set(ByVal Value As StringEx(Of Byte))
                CodesValue = Value
            End Set
        End Property

        ''' <summary>指示Unicode字符串是否存在。</summary>
        Public ReadOnly Property HasUnicodes() As Boolean
            Get
                Return Unicodes IsNot Nothing
            End Get
        End Property

        ''' <summary>指示自定义编码串是否存在。</summary>
        Public ReadOnly Property HasCodes() As Boolean
            Get
                Return Codes IsNot Nothing
            End Get
        End Property

        ''' <summary>Unicode字符串的长度。</summary>
        Public Function UnicodeCount() As Integer
            Return Unicodes.Count
        End Function

        ''' <summary>自定义编码串的长度。</summary>
        Public Function CodeCount() As Integer
            Return Codes.Count
        End Function

        ''' <summary>Unicode字符串的UTF-16B形式。</summary>
        Public Property UnicodeString As String
            Get
                If Unicodes Is Nothing Then
                    Return Nothing
                Else
                    Return Unicodes.ToUTF16B
                End If
            End Get
            Set(ByVal Value As String)
                If Value Is Nothing Then
                    Unicodes = Nothing
                Else
                    Unicodes = New StringEx(Of Char32)(Value.ToUTF32)
                End If
            End Set
        End Property

        ''' <summary>自定义编码串的字符形式。</summary>
        Public Property CodeString() As String
            Get
                If Codes Is Nothing Then
                    Return Nothing
                Else
                    Return String.Join("", (From c In Codes Select c.ToString("X2")).ToArray)
                End If
            End Get
            Set(ByVal Value As String)
                If Value Is Nothing Then
                    Codes = Nothing
                Else
                    Dim v = Value.ToUTF32
                    If v.Count Mod 2 <> 0 Then Throw New ArgumentException

                    Dim Bytes = New Byte((v.Count \ 2) - 1) {}
                    For n = 0 To Bytes.Length - 1
                        Bytes(n) = Byte.Parse(v.SubArray(n * 2, 2).ToUTF16B, Globalization.NumberStyles.HexNumber)
                    Next

                    Codes = New StringEx(Of Byte)(Bytes)
                End If
            End Set
        End Property

        ''' <summary>创建字符码点值对的实例。</summary>
        Public Shared Function FromNothing() As StringCode
            Return New StringCode
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Unicodes">Unicode字符串。</param>
        ''' <param name="Codes">自定义编码串。</param>
        Public Shared Function FromUnicodesAndCodes(ByVal Unicodes As StringEx(Of Char32), ByVal Codes As StringEx(Of Byte)) As StringCode
            Return New StringCode With {.Unicodes = Unicodes, .Codes = Codes}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="UnicodeString">Unicode字符串的UTF-16B形式。</param>
        ''' <param name="CodeString">自定义编码串的字符形式。</param>
        Public Shared Function FromUnicodeStringAndCodeString(ByVal UnicodeString As String, ByVal CodeString As String) As StringCode
            Return New StringCode With {.UnicodeString = UnicodeString, .CodeString = CodeString}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Unicodes">Unicode字符串。</param>
        Public Shared Function FromUnicodes(ByVal Unicodes As StringEx(Of Char32)) As StringCode
            Return New StringCode With {.Unicodes = Unicodes}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Codes">自定义编码串。</param>
        Public Shared Function FromCodes(ByVal Codes As StringEx(Of Byte)) As StringCode
            Return New StringCode With {.Codes = Codes}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="UnicodeString">Unicode字符串的UTF-16B形式。</param>
        Public Shared Function FromUnicodeString(ByVal UnicodeString As String) As StringCode
            Return New StringCode With {.UnicodeString = UnicodeString}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="CodeString">自定义编码串的字符形式。</param>
        Public Shared Function FromCodeString(ByVal CodeString As String) As StringCode
            Return New StringCode With {.CodeString = CodeString}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="UnicodeChar">Unicode字符。</param>
        Public Shared Function FromUnicodeChar(ByVal UnicodeChar As Char32) As StringCode
            Return New StringCode With {.Unicodes = New StringEx(Of Char32)(New Char32() {UnicodeChar})}
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Unicode">Unicode码。</param>
        Public Shared Function FromUnicode(ByVal Unicode As Int32) As StringCode
            Return FromUnicodeChar(New Char32(Unicode))
        End Function

        ''' <summary>指示是否是控制符。</summary>
        Public Overridable ReadOnly Property IsControlChar() As Boolean
            Get
                If HasUnicodes Then
                    If Unicodes.Count = 1 Then
                        Return Unicodes(0) >= 0 AndAlso Unicodes(0) <= &H1F
                    End If
                End If
                Return False
            End Get
        End Property

        ''' <summary>指示是否是换行符。</summary>
        Public Overridable ReadOnly Property IsNewLine() As Boolean
            Get
                If HasUnicodes Then
                    If Unicodes.Count = 1 Then
                        Return Unicodes(0) = Lf
                    End If
                End If
                Return False
            End Get
        End Property

        ''' <summary>指示是否已建立映射。</summary>
        Public ReadOnly Property IsCodeMappable() As Boolean
            Get
                Return HasUnicodes AndAlso HasCodes
            End Get
        End Property

        ''' <summary>生成显示用字符串。</summary>
        Public Overrides Function ToString() As String
            Dim List As New List(Of String)
            If HasUnicodes Then
                List.Add(String.Join(" ", (From c In Unicodes Select String.Format("U+{0:X4}", c.Value)).ToArray))
                If Not IsControlChar Then List.Add(String.Format("""{0}""", UnicodeString))
            End If
            If HasCodes Then List.Add(String.Format("Code = {0}", CodeString()))

            Return "StringCode{" & String.Join(", ", List.ToArray) & "}"
        End Function

        ''' <summary>比较两个字符码点是否相等。</summary>
        Public Overloads Function Equals(ByVal other As StringCode) As Boolean Implements System.IEquatable(Of StringCode).Equals
            Return Unicodes = other.Unicodes AndAlso Codes = other.Codes
        End Function

        ''' <summary>比较两个字符码点是否相等。</summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If Me Is obj Then Return True
            If obj Is Nothing Then Return False
            Dim c = TryCast(obj, StringCode)
            If c Is Nothing Then Return False
            Return Equals(c)
        End Function

        ''' <summary>获取字符码点的HashCode。</summary>
        Public Overrides Function GetHashCode() As Integer
            Dim UniocdesHash = 0
            If HasUnicodes Then
                UniocdesHash = Unicodes.GetHashCode()
            End If
            Dim CodesHash = 0
            If HasCodes Then
                CodesHash = Codes.GetHashCode()
            End If
            Return UniocdesHash Xor ((CodesHash << 16) Or ((CodesHash >> 16) And &HFFFF))
        End Function
    End Class

    ''' <summary>字符码点值对字符串。</summary>
    Public Module StringCodeString
        ''' <summary>转换UTF-32字符串到StringCode()。</summary>
        Public Function FromString32(ByVal s As IEnumerable(Of Char32)) As StringCode()
            Return (From c In s Select StringCode.FromUnicodeChar(c)).ToArray
        End Function

        ''' <summary>转换UTF-16 Big-Endian字符串到UTF-32字符串。</summary>
        Public Function FromString16(ByVal s As String) As StringCode()
            Return FromString32(s.ToUTF32)
        End Function

        ''' <summary>转换UTF-32字符串到UTF-16 Big-Endian字符串。</summary>
        <Extension()> Public Function ToString16(ByVal s As IEnumerable(Of StringCode)) As String
            Return String.Join("", (From c In s Select c.Unicodes.ToUTF16B).ToArray)
        End Function
    End Module
End Namespace
