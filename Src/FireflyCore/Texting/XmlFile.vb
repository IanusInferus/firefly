'==========================================================================
'
'  File:        XmlFile.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: Xml读写
'  Version:     2011.03.16.
'  Copyright:   F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Linq
Imports System.Text
Imports Firefly
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText
Imports Firefly.Streaming
Imports Firefly.Texting

Namespace Texting
    Public NotInheritable Class XmlFile
        Private Sub New()
        End Sub

        Private Shared DefaultReaderSetting As New XmlReaderSettings With {.CheckCharacters = False}
        Public Shared Function ReadFile(ByVal Path As String) As XElement
            Return ReadFile(Path, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, Setting As XmlReaderSettings) As XElement
            Using s = Streams.OpenReadable(Path)
                Return ReadFile(s, Setting)
            End Using
        End Function
        Public Shared Function ReadFile(ByVal s As IReadableStream) As XElement
            Return ReadFile(s, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal s As IReadableStream, Setting As XmlReaderSettings) As XElement
            Using r = XmlReader.Create(s.ToUnsafeStream, Setting)
                Return XElement.Load(r, LoadOptions.PreserveWhitespace Or LoadOptions.SetLineInfo)
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As XElement
            Return ReadFile(Path, Encoding, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding, Setting As XmlReaderSettings) As XElement
            Using s = Streams.OpenReadable(Path)
                Using sr = Txt.CreateTextReader(s.AsNewReading, Encoding)
                    Return ReadFile(sr, Setting)
                End Using
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As XElement
            Return ReadFile(Reader, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader, Setting As XmlReaderSettings) As XElement
            Using r = XmlReader.Create(Reader, Setting)
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
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement, Setting As XmlWriterSettings)
            Using tw = Txt.CreateTextWriter(Path, Setting.Encoding)
                WriteFile(tw, Value, Setting)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement)
            Dim Setting = New XmlWriterSettings With {.Encoding = Writer.Encoding, .Indent = True, .OmitXmlDeclaration = False, .CheckCharacters = False}
            WriteFile(Writer, Value, Setting)
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement, Setting As XmlWriterSettings)
            Using w = XmlWriter.Create(Writer, Setting)
                Value.Save(w)
            End Using
        End Sub
    End Class
End Namespace
