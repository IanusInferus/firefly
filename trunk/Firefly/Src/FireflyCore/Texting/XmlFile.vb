'==========================================================================
'
'  File:        XmlFile.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: Xml读写
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Linq
Imports System.Text
Imports System.Runtime.CompilerServices
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
        Public Shared Function ReadFile(ByVal Path As String, ByVal Setting As XmlReaderSettings) As XElement
            Using s = Streams.OpenReadable(Path)
                Return ReadFile(s, Setting)
            End Using
        End Function
        Public Shared Function ReadFile(ByVal s As IReadableStream) As XElement
            Return ReadFile(s, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal s As IReadableStream, ByVal Setting As XmlReaderSettings) As XElement
            Using r = XmlReader.Create(s.ToUnsafeStream, Setting)
                Return XElement.Load(r, LoadOptions.PreserveWhitespace Or LoadOptions.SetLineInfo)
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding) As XElement
            Return ReadFile(Path, Encoding, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Setting As XmlReaderSettings) As XElement
            Using sr = Txt.CreateTextReader(Path, Encoding)
                Return ReadFile(sr, Setting)
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As XElement
            Return ReadFile(Reader, DefaultReaderSetting)
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader, ByVal Setting As XmlReaderSettings) As XElement
            Using r = XmlReader.Create(Reader, Setting)
                Return XElement.Load(r, LoadOptions.PreserveWhitespace Or LoadOptions.SetLineInfo)
            End Using
        End Function

        Private Shared DefaultWriterSetting As New XmlWriterSettings With {.CheckCharacters = True, .Indent = True, .NamespaceHandling = NamespaceHandling.OmitDuplicates}
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement)
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As Encoding, ByVal Value As XElement)
            Using sw = Txt.CreateTextWriter(Path, Encoding)
                WriteFile(sw, Value)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As XElement, ByVal Setting As XmlWriterSettings)
            Using sw = Txt.CreateTextWriter(Path, Setting.Encoding)
                WriteFile(sw, Value, Setting)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement)
            Dim Setting = DefaultWriterSetting.Clone()
            Setting.Encoding = Writer.Encoding
            WriteFile(Writer, Value, Setting)
        End Sub
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As XElement, ByVal Setting As XmlWriterSettings)
            Using w = XmlWriter.Create(Writer, Setting)
                Value.Save(w)
            End Using
        End Sub
    End Class
    Public Module XmlOperations
        <Extension()> Public Function Reduce(x As XElement) As XElement
            Dim n As New XElement(x.Name)
            For Each a In x.Attributes
                n.SetAttributeValue(a.Name, a.Value)
            Next
            For Each e In x.Elements
                n.Add(Reduce(e))
            Next
            If Not (x.IsEmpty OrElse x.HasElements) Then
                n.Value = x.Value
            End If
            Return n
        End Function
    End Module
End Namespace
