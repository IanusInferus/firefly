'==========================================================================
'
'  File:        LOC.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: LOC文件格式类(版本2)(图形文本)
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Glyphing

Namespace Texting
    Public Class LOCText
        Public Font As IEnumerable(Of IGlyph)
        Public Text As IEnumerable(Of IEnumerable(Of StringCode))
    End Class

    Public Class LOC
        Private Sub New()
        End Sub

        'TODO
        Public Function ReadText(ByVal FdReader As StreamReader, ByVal ImageReader As IImageReader, ByVal AgemoReader As StreamReader) As LOCText
            Dim Font = FdGlyphDescriptionFile.ReadFont(FdReader, ImageReader)
            Dim Text = (From s In Agemo.ReadFile(AgemoReader) Select Descape(s)).ToArray
            Return New LOCText With {.Font = Font, .Text = Text}
        End Function

        Private Function Descape(ByVal This As String) As IEnumerable(Of StringCode)
            Dim m = r.Match(This)
            If Not m.Success Then Throw New InvalidCastException

            Dim ss As New SortedList(Of Integer, String)
            Dim sg As New Dictionary(Of Integer, StringCode)
            For Each c As Capture In m.Groups.Item("SingleEscape").Captures
                ss.Add(c.Index, SingleEscapeDict(c.Value))
            Next
            For Each c As Capture In m.Groups.Item("CustomCodeEscape").Captures
                ss.Add(c.Index, Nothing)
                sg.Add(c.Index, StringCode.FromCodeString(c.Value))
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

            Dim l As New List(Of StringCode)
            Dim sl As New List(Of String)
            For Each p In ss
                If p.Value IsNot Nothing Then
                    sl.Add(p.Value)
                ElseIf sl.Count > 0 Then
                    Dim s = String.Join("", sl.ToArray)
                    l.AddRange(From c In s.ToUTF32 Select StringCode.FromUnicodeChar(c))
                    sl.Clear()
                    l.Add(sg(p.Key))
                End If
            Next
            If sl.Count > 0 Then
                Dim s = String.Join("", sl.ToArray)
                l.AddRange(From c In s.ToUTF32 Select StringCode.FromUnicodeChar(c))
                sl.Clear()
            End If

            Return l
        End Function
        Private Function Escape(ByVal This As IEnumerable(Of StringCode)) As String
            Dim l As New List(Of String)
            For Each c In This
                If c.HasCodes Then
                    l.Add("\g{" & c.CodeString & "}")
                Else
                    l.Add(c.UnicodeString)
                End If
            Next
            Return String.Join("", l.ToArray)
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
        Private CustomCodeEscapes As String = "\\g\{(?<CustomCodeEscape>[0-9A-Fa-f]+)\}"
        Private UnicodeEscapes As String = "\\U(?<UnicodeEscape>[0-9A-Fa-f]{5})|\\u(?<UnicodeEscape>[0-9A-Fa-f]{4})|\\x(?<UnicodeEscape>[0-9A-Fa-f]{2})"
        Private ErrorEscapes As String = "(?<ErrorEscape>\\)"
        Private Normal As String = "(?<Normal>.)"

        Private r As New Regex("^" & "(" & SingleEscapes & "|" & CustomCodeEscapes & "|" & UnicodeEscapes & "|" & ErrorEscapes & "|" & Normal & ")*" & "$", RegexOptions.ExplicitCapture)
    End Class
End Namespace
