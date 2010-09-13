'==========================================================================
'
'  File:        Encoding.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: 编码
'  Version:     2009.12.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Text
Imports System.Runtime.CompilerServices

Namespace TextEncoding
    Public Class EncodingNoPreambleWrapper
        Inherits Encoding

        Private BaseEncoding As Encoding
        Public Sub New(ByVal BaseEncoding As Encoding)
            Me.BaseEncoding = BaseEncoding
        End Sub

        Public Overrides Function GetDecoder() As Decoder
            Return BaseEncoding.GetDecoder()
        End Function
        Public Overrides Function GetEncoder() As Encoder
            Return BaseEncoding.GetEncoder()
        End Function
        Public Overrides Function GetPreamble() As Byte()
            Return New Byte() {}
        End Function

        Public Overrides Function GetByteCount(ByVal chars() As Char, ByVal index As Integer, ByVal count As Integer) As Integer
            Return BaseEncoding.GetByteCount(chars, index, count)
        End Function
        Public Overrides Function GetBytes(ByVal chars() As Char, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal bytes() As Byte, ByVal byteIndex As Integer) As Integer
            Return BaseEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex)
        End Function
        Public Overrides Function GetCharCount(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer) As Integer
            Return BaseEncoding.GetCharCount(bytes, index, count)
        End Function
        Public Overrides Function GetChars(ByVal bytes() As Byte, ByVal byteIndex As Integer, ByVal byteCount As Integer, ByVal chars() As Char, ByVal charIndex As Integer) As Integer
            Return BaseEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex)
        End Function
        Public Overrides Function GetMaxByteCount(ByVal charCount As Integer) As Integer
            Return BaseEncoding.GetMaxByteCount(charCount)
        End Function
        Public Overrides Function GetMaxCharCount(ByVal byteCount As Integer) As Integer
            Return BaseEncoding.GetMaxCharCount(byteCount)
        End Function

        Public Overrides Function Clone() As Object
            Return MyBase.Clone()
        End Function
        Public Overrides Function Equals(ByVal value As Object) As Boolean
            Dim e = TryCast(value, EncodingNoPreambleWrapper)
            If e IsNot Nothing Then Return BaseEncoding.Equals(e.BaseEncoding)
            Return BaseEncoding.Equals(value)
        End Function
        Public Overrides Function GetHashCode() As Integer
            Return BaseEncoding.GetHashCode()
        End Function
        Public Overrides Function ToString() As String
            Return BaseEncoding.ToString()
        End Function

        Public Overrides ReadOnly Property BodyName() As String
            Get
                Return BaseEncoding.BodyName
            End Get
        End Property
        Public Overrides ReadOnly Property CodePage() As Integer
            Get
                Return BaseEncoding.CodePage
            End Get
        End Property
        Public Overrides ReadOnly Property EncodingName() As String
            Get
                Return BaseEncoding.EncodingName
            End Get
        End Property
        Public Overrides Function GetByteCount(ByVal chars() As Char) As Integer
            Return BaseEncoding.GetByteCount(chars)
        End Function
        Public Overrides Function GetByteCount(ByVal s As String) As Integer
            Return BaseEncoding.GetByteCount(s)
        End Function
        Public Overrides Function GetBytes(ByVal chars() As Char) As Byte()
            Return BaseEncoding.GetBytes(chars)
        End Function
        Public Overrides Function GetBytes(ByVal chars() As Char, ByVal index As Integer, ByVal count As Integer) As Byte()
            Return BaseEncoding.GetBytes(chars, index, count)
        End Function
        Public Overrides Function GetBytes(ByVal s As String) As Byte()
            Return BaseEncoding.GetBytes(s)
        End Function
        Public Overrides Function GetBytes(ByVal s As String, ByVal charIndex As Integer, ByVal charCount As Integer, ByVal bytes() As Byte, ByVal byteIndex As Integer) As Integer
            Return BaseEncoding.GetBytes(s, charIndex, charCount, bytes, byteIndex)
        End Function
        Public Overrides Function GetCharCount(ByVal bytes() As Byte) As Integer
            Return BaseEncoding.GetCharCount(bytes)
        End Function
        Public Overrides Function GetChars(ByVal bytes() As Byte) As Char()
            Return BaseEncoding.GetChars(bytes)
        End Function
        Public Overrides Function GetChars(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer) As Char()
            Return BaseEncoding.GetChars(bytes, index, count)
        End Function
        Public Overrides Function GetString(ByVal bytes() As Byte) As String
            Return BaseEncoding.GetString(bytes)
        End Function
        Public Overrides Function GetString(ByVal bytes() As Byte, ByVal index As Integer, ByVal count As Integer) As String
            Return BaseEncoding.GetString(bytes, index, count)
        End Function
        Public Overrides ReadOnly Property HeaderName() As String
            Get
                Return BaseEncoding.HeaderName
            End Get
        End Property
        Public Overrides Function IsAlwaysNormalized(ByVal form As NormalizationForm) As Boolean
            Return BaseEncoding.IsAlwaysNormalized(form)
        End Function
        Public Overrides ReadOnly Property IsBrowserDisplay() As Boolean
            Get
                Return BaseEncoding.IsBrowserDisplay
            End Get
        End Property
        Public Overrides ReadOnly Property IsBrowserSave() As Boolean
            Get
                Return BaseEncoding.IsBrowserSave
            End Get
        End Property
        Public Overrides ReadOnly Property IsMailNewsDisplay() As Boolean
            Get
                Return BaseEncoding.IsMailNewsDisplay
            End Get
        End Property
        Public Overrides ReadOnly Property IsMailNewsSave() As Boolean
            Get
                Return BaseEncoding.IsMailNewsSave
            End Get
        End Property
        Public Overrides ReadOnly Property IsSingleByte() As Boolean
            Get
                Return BaseEncoding.IsSingleByte
            End Get
        End Property
        Public Overrides ReadOnly Property WebName() As String
            Get
                Return BaseEncoding.WebName
            End Get
        End Property
        Public Overrides ReadOnly Property WindowsCodePage() As Integer
            Get
                Return BaseEncoding.WindowsCodePage
            End Get
        End Property
    End Class

    Public Module TextEncoding
        Private DefaultValue As Encoding
        Public Property [Default]() As Encoding
            Get
                If DefaultValue Is Nothing Then
                    DefaultValue = Encoding.Default
                    If DefaultValue Is GB2312 Then
                        Try
                            DefaultValue = GB18030
                        Catch
                        End Try
                    End If
                End If
                Return DefaultValue
            End Get
            Set(ByVal Value As Encoding)
                DefaultValue = Value
            End Set
        End Property

        Private WritingDefaultValue As Encoding
        Public Property WritingDefault() As Encoding
            Get
                If WritingDefaultValue Is Nothing Then
                    WritingDefaultValue = UTF16
                End If
                Return WritingDefaultValue
            End Get
            Set(ByVal Value As Encoding)
                WritingDefaultValue = Value
            End Set
        End Property

        Public ReadOnly Property ASCII() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("ASCII", New EncoderExceptionFallback(), New DecoderExceptionFallback()) '20127
                Return e
            End Get
        End Property

        Public ReadOnly Property UTF8() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("UTF-8") '65001
                Return e
            End Get
        End Property

        Public ReadOnly Property UTF16() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("UTF-16") '1200
                Return e
            End Get
        End Property

        Public ReadOnly Property UTF16B() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("UTF-16BE") '1201
                Return e
            End Get
        End Property

        Public ReadOnly Property UTF32() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("UTF-32") '12000
                Return e
            End Get
        End Property

        Public ReadOnly Property UTF32B() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("UTF-32BE") '12001
                Return e
            End Get
        End Property

        Public ReadOnly Property GB18030() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("GB18030") '54936
                Return e
            End Get
        End Property

        Public ReadOnly Property GB2312() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("GB2312") '936
                Return e
            End Get
        End Property

        Public ReadOnly Property Big5() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("Big5") '950
                Return e
            End Get
        End Property

        Public ReadOnly Property ShiftJIS() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("Shift-JIS") '932
                Return e
            End Get
        End Property

        Public ReadOnly Property ISO8859_1() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("ISO-8859-1") '28591
                Return e
            End Get
        End Property

        Public ReadOnly Property Windows1252() As Encoding
            Get
                Static e As Encoding = Nothing
                If e Is Nothing Then e = Encoding.GetEncoding("Windows-1252") '1252
                Return e
            End Get
        End Property

        ''' <summary>
        ''' 将指定字节数组中的所有字节解码为一组字符。
        ''' </summary>
        ''' <param name="This">编码。</param>
        ''' <param name="Bytes">包含要解码的字节序列的字节数组。</param>
        ''' <returns>一个字节数组，包含对指定的字节序列进行解码的结果。</returns>
        <Extension()> Public Function GetString32(ByVal This As Encoding, ByVal Bytes As Byte()) As Char32()
            Return String32.FromUTF16B(This.GetChars(Bytes))
        End Function

        ''' <summary>
        ''' 映射。
        ''' </summary>
        ''' <typeparam name="D">定义域。</typeparam>
        ''' <typeparam name="R">值域。</typeparam>
        Public Delegate Function Mapping(Of D, R)(ByVal d As D) As R
    End Module
End Namespace
