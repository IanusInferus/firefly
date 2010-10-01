'==========================================================================
'
'  File:        StringDescape.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 字符串转义语法糖
'  Version:     2010.10.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports Firefly.TextEncoding

''' <summary>
''' 字符串转义
''' 用于使用 "转义字符串".Descape 和 "格式".Formats(...) 语法
''' </summary>
Public Module StringDescape

    ''' <summary>字符串反转义函数</summary>
    ''' <remarks>
    ''' \0 与null \u0000 匹配
    ''' \a 与响铃（警报）\u0007 匹配 
    ''' \b 与退格符 \u0008 匹配
    ''' \f 与换页符 \u000C 匹配
    ''' \n 与换行符 \u000A 匹配
    ''' \r 与回车符 \u000D 匹配
    ''' \t 与 Tab 符 \u0009 匹配 
    ''' \v 与垂直 Tab 符 \u000B 匹配
    ''' \x?? 与 \u00?? 匹配
    ''' \u???? 与对应的UTF16字符对应
    ''' \U????? 与对应的UTF32字符对应
    ''' </remarks>
    <Extension()> Public Function Descape(ByVal This As String) As String
        Dim m = r.Match(This)
        If Not m.Success Then Throw New InvalidCastException

        Dim ss As New SortedList(Of Integer, String)
        For Each c As Capture In m.Groups.Item("SingleEscape").Captures
            ss.Add(c.Index, SingleEscapeDict(c.Value))
        Next
        For Each c As Capture In m.Groups.Item("UnicodeEscape").Captures
            ss.Add(c.Index, ChrQ(Int32.Parse(c.Value, Globalization.NumberStyles.HexNumber)))
        Next
        For Each c As Capture In m.Groups.Item("ErrorEscape").Captures
            Throw New ArgumentException("ErrorEscape: Ch " & (c.Index + 1) & " " & c.Value)
        Next
        For Each c As Capture In m.Groups.Item("Normal").Captures
            ss.Add(c.Index, c.Value)
        Next

        Dim sb As New StringBuilder
        For Each s In ss.Values
            sb.Append(s)
        Next

        Return sb.ToString
    End Function

    ''' <summary>字符串转义函数</summary>
    ''' <remarks>
    ''' \0 与null \u0000 匹配
    ''' \a 与响铃（警报）\u0007 匹配 
    ''' \b 与退格符 \u0008 匹配
    ''' \f 与换页符 \u000C 匹配
    ''' \n 与换行符 \u000A 匹配
    ''' \r 与回车符 \u000D 匹配
    ''' \t 与 Tab 符 \u0009 匹配 
    ''' \v 与垂直 Tab 符 \u000B 匹配
    ''' \u???? 与对应的UTF16字符对应(只转义\u0000-\u001F和\u007F中除上述字符的字符)
    ''' \U????? 与对应的UTF32字符对应 不出现
    ''' </remarks>
    <Extension()> Public Function Escape(ByVal This As String) As String
        Dim l As New List(Of Char32)
        For Each c In This.ToUTF32
            Select Case c.Value
                Case &H5C
                    l.AddRange("\\".ToUTF32)
                Case &H0
                    l.AddRange("\0".ToUTF32)
                Case &H7
                    l.AddRange("\a".ToUTF32)
                Case &H8
                    l.AddRange("\b".ToUTF32)
                Case &HC
                    l.AddRange("\f".ToUTF32)
                Case &HA
                    l.AddRange("\n".ToUTF32)
                Case &HD
                    l.AddRange("\r".ToUTF32)
                Case &H9
                    l.AddRange("\t".ToUTF32)
                Case &HB
                    l.AddRange("\v".ToUTF32)
                Case &H0 To &H1F, &H7F
                    l.AddRange("\u{0:X4}".Formats(c.Value).ToUTF32)
                Case Else
                    l.Add(c)
            End Select
        Next
        Return l.ToUTF16B
    End Function

    ''' <summary>将指定的 String 中的格式项替换为指定的 Object 实例的值的文本等效项。</summary>
    <Extension()> Public Function Formats(ByVal This As String, ByVal arg0 As Object) As String
        Return String.Format(This, arg0)
    End Function
    ''' <summary>将指定的 String 中的格式项替换为两个指定的 Object 实例的值的文本等效项。</summary>
    <Extension()> Public Function Formats(ByVal This As String, ByVal arg0 As Object, ByVal arg1 As Object) As String
        Return String.Format(This, arg0, arg1)
    End Function
    ''' <summary>将指定的 String 中的格式项替换为三个指定的 Object 实例的值的文本等效项。</summary>
    <Extension()> Public Function Formats(ByVal This As String, ByVal arg0 As Object, ByVal arg1 As Object, ByVal arg2 As Object) As String
        Return String.Format(This, arg0, arg1, arg2)
    End Function
    ''' <summary>将指定 String 中的格式项替换为指定数组中相应 Object 实例的值的文本等效项。</summary>
    <Extension()> Public Function Formats(ByVal This As String, ByVal ParamArray args As Object()) As String
        Return String.Format(This, args)
    End Function
    ''' <summary>将指定 String 中的格式项替换为指定数组中相应 Object 实例的值的文本等效项。指定的参数提供区域性特定的格式设置信息。</summary>
    <Extension()> Public Function Formats(ByVal This As String, ByVal provider As IFormatProvider, ByVal ParamArray args As Object()) As String
        Return String.Format(provider, This, args)
    End Function


    Private ReadOnly Property SingleEscapeDict() As Dictionary(Of String, String)
        Get
            Static d As Dictionary(Of String, String)
            If d IsNot Nothing Then Return d
            d = New Dictionary(Of String, String)
            d.Add("\", "\") 'backslash
            d.Add("0", ChrQ(0)) 'null
            d.Add("a", ChrQ(7)) 'alert (beep)
            d.Add("b", ChrQ(8)) 'backspace
            d.Add("f", ChrQ(&HC)) 'form feed
            d.Add("n", ChrQ(&HA)) 'newline (lf)
            d.Add("r", ChrQ(&HD)) 'carriage return (cr) 
            d.Add("t", ChrQ(9)) 'horizontal tab 
            d.Add("v", ChrQ(&HB)) 'vertical tab
            Return d
        End Get
    End Property
    Private ReadOnly Property SingleEscapes() As String
        Get
            Static s As String
            If s IsNot Nothing Then Return s
            Dim Chars As New List(Of String)
            For Each c In "\0abfnrtv"
                Chars.Add(Regex.Escape(c))
            Next
            s = "\\(?<SingleEscape>" & String.Join("|", Chars.ToArray) & ")"
            Return s
        End Get
    End Property
    Private UnicodeEscapes As String = "\\U(?<UnicodeEscape>[0-9A-Fa-f]{5})|\\u(?<UnicodeEscape>[0-9A-Fa-f]{4})|\\x(?<UnicodeEscape>[0-9A-Fa-f]{2})"
    Private ErrorEscapes As String = "(?<ErrorEscape>\\)"
    Private Normal As String = "(?<Normal>.|\r|\n)"

    Private r As New Regex("^" & "(" & SingleEscapes & "|" & UnicodeEscapes & "|" & ErrorEscapes & "|" & Normal & ")*" & "$", RegexOptions.ExplicitCapture)
End Module
