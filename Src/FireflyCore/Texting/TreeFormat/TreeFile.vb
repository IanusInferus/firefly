'==========================================================================
'
'  File:        TreeFile.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: Tree文件格式 - 版本2
'  Version:     2012.07.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Diagnostics
Imports Firefly.Streaming
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Mapping.MetaSchema

Namespace Texting.TreeFormat
    Public NotInheritable Class TreeFile
        Public Shared Function ReadRaw(ByVal Path As String, ByVal ParseSetting As TreeFormatParseSetting) As TreeFormatParseResult
            Return ReadRaw(Path, TextEncoding.Default, ParseSetting)
        End Function
        Public Shared Function ReadRaw(ByVal Path As String, ByVal Encoding As Encoding, ByVal ParseSetting As TreeFormatParseSetting) As TreeFormatParseResult
            Using sr = Txt.CreateTextReader(Path, Encoding)
                Dim t = Txt.ReadFile(sr)
                Dim tfp As New TreeFormatSyntaxParser(ParseSetting, t, Path)
                Try
                    Return tfp.Parse()
                Catch ex As InvalidOperationException
                    Throw New Syntax.InvalidSyntaxException("", New Syntax.FileTextRange With {.Text = tfp.Text, .Range = Opt(Of Syntax.TextRange).Empty}, ex)
                End Try
            End Using
        End Function
        Public Shared Function ReadRaw(ByVal Reader As StreamReader, ByVal ParseSetting As TreeFormatParseSetting) As TreeFormatParseResult
            Dim t = Txt.ReadFile(Reader)
            Dim tfp As New TreeFormatSyntaxParser(ParseSetting, t)
            Return tfp.Parse()
        End Function

        Public Shared Function ReadDirect(ByVal Path As String, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As TreeFormatResult
            Return ReadDirect(Path, TextEncoding.Default, ParseSetting, EvaluateSetting)
        End Function
        Public Shared Function ReadDirect(ByVal Path As String, ByVal Encoding As Encoding, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As TreeFormatResult
            Dim pr = ReadRaw(Path, Encoding, ParseSetting)
            Dim tfe As New TreeFormatEvaluator(EvaluateSetting, pr)
            Return tfe.Evaluate()
        End Function
        Public Shared Function ReadDirect(ByVal Reader As StreamReader, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As TreeFormatResult
            Dim pr = ReadRaw(Reader, ParseSetting)
            Dim tfe As New TreeFormatEvaluator(EvaluateSetting, pr)
            Return tfe.Evaluate()
        End Function

        Public Shared Function ReadFile(ByVal Path As String, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As XElement
            Return ReadFile(Path, TextEncoding.Default, ParseSetting, EvaluateSetting)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As XElement
            Dim er = ReadDirect(Path, Encoding, ParseSetting, EvaluateSetting)
            Return XmlInterop.TreeToXml(er)
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader, ByVal ParseSetting As TreeFormatParseSetting, ByVal EvaluateSetting As TreeFormatEvaluateSetting) As XElement
            Dim er = ReadDirect(Reader, ParseSetting, EvaluateSetting)
            Return XmlInterop.TreeToXml(er)
        End Function

        Public Shared Function ReadFile(ByVal Path As String) As XElement
            Return ReadFile(Path, TextEncoding.Default)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As XElement
            Dim er = ReadDirect(Path, Encoding, New TreeFormatParseSetting, New TreeFormatEvaluateSetting)
            Return XmlInterop.TreeToXml(er)
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As XElement
            Dim er = ReadDirect(Reader, New TreeFormatParseSetting, New TreeFormatEvaluateSetting)
            Return XmlInterop.TreeToXml(er)
        End Function

        Public Shared Sub WriteRaw(ByVal Path As String, ByVal Value As Syntax.Forest)
            WriteRaw(Path, TextEncoding.WritingDefault, Value)
        End Sub
        Public Shared Sub WriteRaw(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As Syntax.Forest)
            Using sw = Txt.CreateTextWriter(Path, Encoding)
                WriteRaw(sw, Value)
            End Using
        End Sub
        Public Shared Sub WriteRaw(ByVal Writer As StreamWriter, ByVal Value As Syntax.Forest)
            Dim w As New TreeFormatSyntaxWriter(Writer)
            w.Write(Value)
        End Sub

        Public Shared Sub WriteDirect(ByVal Path As String, ByVal Value As Semantics.Forest)
            WriteDirect(Path, TextEncoding.WritingDefault, Value)
        End Sub
        Public Shared Sub WriteDirect(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As Semantics.Forest)
            Using sw = Txt.CreateTextWriter(Path, Encoding)
                WriteDirect(sw, Value)
            End Using
        End Sub
        Public Shared Sub WriteDirect(ByVal Writer As StreamWriter, ByVal Value As Semantics.Forest)
            Dim w As New TreeFormatWriter(Writer)
            w.Write(Value)
        End Sub

        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement)
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As XElement)
            Using sw = Txt.CreateTextWriter(Path, Encoding)
                WriteFile(sw, Value)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement)
            Dim v = XmlInterop.XmlToTree(Value)
            WriteDirect(Writer, v.Value)
        End Sub
    End Class
End Namespace
