'==========================================================================
'
'  File:        XmlFile.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: Xml读写
'  Version:     2011.03.06.
'  Copyright:   F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Linq
Imports System.Text
Imports Firefly
Imports Firefly.Texting
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText

Namespace Texting
    Public NotInheritable Class XmlFile
        Private Sub New()
        End Sub

        Public Shared Function ReadFile(ByVal Path As String) As XElement
            Return ReadFile(Path, TextEncoding.Default)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As XElement
            Using s As New FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(s, Encoding, True)
                    Return ReadFile(sr)
                End Using
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As XElement
            Using r = XmlReader.Create(Reader)
                Return XElement.Load(r, LoadOptions.PreserveWhitespace Or LoadOptions.SetLineInfo)
            End Using
        End Function

        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement)
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As XElement)
            Using tw = Txt.CreateTextWriter(Path, Encoding)
                WriteFile(tw, Value)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement)
            Dim Setting = New XmlWriterSettings With {.Encoding = Writer.Encoding, .Indent = True, .OmitXmlDeclaration = False}
            Using w = XmlWriter.Create(Writer, Setting)
                Value.Save(w)
            End Using
        End Sub
    End Class
End Namespace
