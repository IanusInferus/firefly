'==========================================================================
'
'  File:        ByteTextSearch.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 基于字节的正则表达式搜索
'  Version:     2009.10.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly.TextEncoding

Namespace Texting
    Public NotInheritable Class ByteTextSearch
        Private Sub New()
        End Sub

        Public Shared Function EncodeAsByteString(ByVal Input As Byte()) As String
            Dim sb = New Char32(Input.Length - 1) {}
            For n = 0 To Input.Length - 1
                sb(n) = ChrQ(Input(n))
            Next
            Return sb.ToUTF16B
        End Function

        Public Shared Function DecodeFromByteString(ByVal Input As String) As Byte()
            Dim bb = New Byte(Input.Length - 1) {}
            For n = 0 To Input.Length - 1
                bb(n) = AscQ(Input(n))
            Next
            Return bb
        End Function

        Public Shared Function EncodingRangeToRegexPattern(ByVal ByteRanges As Range()) As String
            Dim l As New List(Of String)
            For Each r In ByteRanges
                l.Add(String.Format("\x{0:X2}-\x{1:X2}", r.Lower, r.Upper))
            Next
            Return "[" & String.Join("", l.ToArray) & "]"
        End Function
        Public Shared Function EncodingRangeToRegexPattern(ByVal FirstByteRanges As Range(), ByVal SecondByteRanges As Range()) As String
            Return EncodingRangeToRegexPattern(FirstByteRanges) & EncodingRangeToRegexPattern(SecondByteRanges)
        End Function
        Public Shared Function EncodingRangeToRegexPattern(ByVal FirstByteRanges As Range(), ByVal SecondByteRanges As Range(), ByVal ThirdByteRanges As Range()) As String
            Return EncodingRangeToRegexPattern(FirstByteRanges) & EncodingRangeToRegexPattern(SecondByteRanges) & EncodingRangeToRegexPattern(ThirdByteRanges)
        End Function
        Public Shared Function EncodingRangeToRegexPattern(ByVal FirstByteRanges As Range(), ByVal SecondByteRanges As Range(), ByVal ThirdByteRanges As Range(), ByVal ForthByteRanges As Range()) As String
            Return EncodingRangeToRegexPattern(FirstByteRanges) & EncodingRangeToRegexPattern(SecondByteRanges) & EncodingRangeToRegexPattern(ThirdByteRanges) & EncodingRangeToRegexPattern(ForthByteRanges)
        End Function

        Public Shared Function MatchAll(ByVal Encoding As Encoding, ByVal Input As Byte(), ByVal Pattern As String, Optional ByVal Options As RegexOptions = RegexOptions.ExplicitCapture) As WQSG.Triple()
            Dim t As New List(Of WQSG.Triple)
            Dim InputStr = EncodeAsByteString(Input)
            Dim Matches = Regex.Matches(InputStr, Pattern, Options)
            For Each m As Match In Matches
                Dim Text = Encoding.GetChars(DecodeFromByteString(m.Value))
                t.Add(New WQSG.Triple With {.Offset = m.Index, .Length = m.Length, .Text = Text})
            Next
            Return t.ToArray
        End Function
    End Class
End Namespace
