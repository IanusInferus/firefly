'==========================================================================
'
'  File:        Plain.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 纯文本格式
'  Version:     2010.08.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly.TextEncoding

Namespace Texting
    Public NotInheritable Class Plain
        Private Sub New()
        End Sub

        Public Shared Function ReadFile(ByVal Reader As StreamReader) As String()
            Dim s = Reader
            Dim l As New List(Of String)
            While Not s.EndOfStream
                Dim Line As String = s.ReadLine
                If Line <> "" Then
                    Line = Line.Replace("\n", ChrQ(13) & ChrQ(10))
                    Line = Line.Replace("\x5C", "\")
                End If
                l.Add(Line)
            End While
            Return l.ToArray
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding) As String()
            Using s = Txt.CreateTextReader(Path, Encoding, True)
                Return ReadFile(s)
            End Using
        End Function
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As IEnumerable(Of String))
            Dim s = Writer
            Dim n = 0
            For Each v In Value
                If v <> "" Then
                    Dim Line As String = v
                    If Line <> "" Then Line = Line.Replace("\", "\x5C")
                    If Line <> "" Then Line = Line.Replace(ChrQ(13) & ChrQ(10), ChrQ(10)).Replace(ChrQ(10), "\n")
                    s.WriteLine(Line)
                Else
                    s.WriteLine()
                End If
                n += 1
            Next
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding, ByVal Value As IEnumerable(Of String))
            Using s = Txt.CreateTextWriter(Path, Encoding, True)
                WriteFile(s, Value)
            End Using
        End Sub
        Public Shared Function ReadFile(ByVal Path As String) As String()
            Return ReadFile(Path, TextEncoding.Default)
        End Function
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As IEnumerable(Of String))
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
    End Class
End Namespace
