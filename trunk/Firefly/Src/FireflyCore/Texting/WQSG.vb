'==========================================================================
'
'  File:        WQSG.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: WQSGText文本格式
'  Version:     2010.08.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports Firefly.TextEncoding

Namespace Texting
    Public Module WQSGExt
        <Extension()> Public Function GetTexts(ByVal This As WQSG.Triple()) As String()
            Return WQSG.GetTexts(This)
        End Function
    End Module
    Public NotInheritable Class WQSG
        Private Sub New()
        End Sub

        Public Class Triple
            Public Offset As Integer
            Public Length As Integer
            Public Text As String
        End Class
        Public Shared Function GetTexts(ByVal This As Triple()) As String()
            Dim Texts As String() = New String(This.Length - 1) {}
            For n = 0 To This.Length - 1
                Texts(n) = This(n).Text
            Next
            Return Texts
        End Function
        Private Shared Function ReadFile(ByVal Reader As StreamReader, ByVal FormatError As Func(Of Integer, String)) As Triple()
            Dim l As New List(Of Triple)
            Dim LineNumber As Integer = 1
            Dim s = Reader
            Dim r As New Regex("^(?<offset>.*?),(?<length>.*?),(?<text>.*)$", RegexOptions.ExplicitCapture)
            While Not s.EndOfStream
                Dim Line = s.ReadLine
                If Line = "" Then
                    LineNumber += 1
                    Continue While
                End If
                Dim Match = r.Match(Line)
                If Match.Success Then
                    Dim t As New Triple
                    If Not Integer.TryParse(Match.Result("${offset}"), Globalization.NumberStyles.HexNumber, Nothing, t.Offset) Then Throw New InvalidDataException(FormatError(LineNumber))
                    If Not Integer.TryParse(Match.Result("${length}"), Globalization.NumberStyles.Integer, Nothing, t.Length) Then Throw New InvalidDataException(FormatError(LineNumber))
                    t.Text = Match.Result("${text}").Replace("\n", CrLf)
                    l.Add(t)
                Else
                    Throw New InvalidDataException(FormatError(LineNumber))
                End If
                LineNumber += 1
            End While
            Return l.ToArray
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As Triple()
            Return ReadFile(Reader, Function(LineNumber) String.Format("{0} : 格式错误。", LineNumber))
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As Triple()
            Using s = Txt.CreateTextReader(Path, Encoding, True)
                Return ReadFile(s, Function(LineNumber) String.Format("{0}({1}) : 格式错误。", Path, LineNumber))
            End Using
        End Function
        Public Shared Function VerifyFile(ByVal Reader As StreamReader, <System.Runtime.InteropServices.Out()> ByRef LogText As String, ByVal FormatError As Func(Of Integer, String)) As Boolean
            Dim Log As New List(Of String)
            Dim LineNumber As Integer = 1
            Dim s = Reader
            Dim r As New Regex("^(?<offset>.*?),(?<length>.*?),(?<text>.*)$", RegexOptions.ExplicitCapture)
            While Not s.EndOfStream
                Dim Line = s.ReadLine
                If Line.Trim = "" Then
                    LineNumber += 1
                    Continue While
                End If
                Dim Match = r.Match(Line)
                If Match.Success Then
                    Dim t As New Triple
                    If Not Integer.TryParse(Match.Result("${offset}"), Globalization.NumberStyles.HexNumber, Nothing, t.Offset) Then Log.Add(FormatError(LineNumber))
                    If Not Integer.TryParse(Match.Result("${length}"), Globalization.NumberStyles.Integer, Nothing, t.Length) Then Log.Add(FormatError(LineNumber))
                    t.Text = Match.Result("${text}").Replace("\n", CrLf)
                Else
                    Log.Add(FormatError(LineNumber))
                End If
                LineNumber += 1
            End While
            LogText = String.Join(System.Environment.NewLine, Log.ToArray)
            Return Log.Count = 0
        End Function
        Public Shared Function VerifyFile(ByVal Reader As StreamReader, <System.Runtime.InteropServices.Out()> ByRef LogText As String) As Boolean
            Return VerifyFile(Reader, LogText, Function(LineNumber) String.Format("{0} : 格式错误。", LineNumber))
        End Function
        Public Shared Function VerifyFile(ByVal Path As String, ByVal Encoding As Encoding, <System.Runtime.InteropServices.Out()> ByRef LogText As String) As Boolean
            Using s = Txt.CreateTextReader(Path, Encoding, True)
                Return VerifyFile(s, LogText, Function(LineNumber) String.Format("{0} : 格式错误。", LineNumber))
            End Using
        End Function
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As IEnumerable(Of Triple))
            Dim s = Writer
            Dim n = 0
            For Each v In Value
                s.WriteLine(String.Format("{0},{1},{2}", v.Offset.ToString("X8"), v.Length, v.Text.Replace(CrLf, Lf).Replace(Lf, "\n")))
                s.WriteLine()
                n += 1
            Next
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As IEnumerable(Of Triple))
            Using s = Txt.CreateTextWriter(Path, Encoding, True)
                WriteFile(s, Value)
            End Using
        End Sub
        Public Shared Function ReadFile(ByVal Path As String) As Triple()
            Return ReadFile(Path, TextEncoding.Default)
        End Function
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As IEnumerable(Of Triple))
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
    End Class
End Namespace
