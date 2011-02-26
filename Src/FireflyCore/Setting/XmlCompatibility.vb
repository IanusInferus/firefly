'==========================================================================
'
'  File:        XmlCompatibility.vb
'  Location:    Firefly.Setting <Visual Basic .Net>
'  Description: Xml读写兼容支持，用于兼容System.Xml.Serialization.XmlSerializer
'  Version:     2011.02.27.
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
Imports System.Globalization
Imports Firefly
Imports Firefly.Texting
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText

Namespace Setting
    ''' <summary>Xml读写兼容支持，用于兼容System.Xml.Serialization.XmlSerializer</summary>
    Public NotInheritable Class XmlCompatibility
        Private Sub New()
        End Sub

        Public Shared Function ReadFile(Of T)(ByVal Reader As StreamReader) As T
            Dim xs As New XmlSerializer
            xs.PutReader(XmlCompatibility.Base64StringToByteArray)
            xs.PutReader(XmlCompatibility.StringToDateTime)

            Dim Root As XElement
            Using r = XmlReader.Create(Reader)
                Root = XElement.Load(r)
            End Using

            Return xs.Read(Of T)(Root)
        End Function

        Public Shared Sub WriteFile(Of T)(ByVal Writer As StreamWriter, ByVal Value As T)
            Dim xs As New XmlSerializer
            xs.PutWriter(XmlCompatibility.BooleanToString)
            xs.PutWriter(XmlCompatibility.ByteArrayToBase64String)
            xs.PutWriter(XmlCompatibility.DateTimeToString)
            xs.PutWriterTranslator(New CompatibleXElementToStringRangeTranslator)

            Dim Root = xs.Write(Of T)(Value)
            Root.SetAttributeValue(XNamespace.Xmlns + "xsi", xsi)
            Root.SetAttributeValue(XNamespace.Xmlns + "xsd", xsd)

            Dim Setting = New XmlWriterSettings With {.Encoding = Writer.Encoding, .Indent = True, .OmitXmlDeclaration = False}
            Using w = XmlWriter.Create(Writer, Setting)
                Root.Save(w)
            End Using
        End Sub

        Public Shared ReadOnly BooleanToString As Func(Of Boolean, String) =
            Function(b As Boolean) As String
                If b Then
                    Return "true"
                Else
                    Return "false"
                End If
            End Function

        Public Shared ReadOnly ByteArrayToBase64String As Func(Of Byte(), String) =
            Function(byteArray As Byte()) As String
                If byteArray Is Nothing Then Return Nothing
                Return Convert.ToBase64String(byteArray)
            End Function

        Public Shared ReadOnly Base64StringToByteArray As Func(Of String, Byte()) =
            Function(byteArrayString As String) Convert.FromBase64String(byteArrayString)

        Public Shared ReadOnly DateTimeToString As Func(Of DateTime, String) =
            Function(d As DateTime) d.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz")

        Public Shared ReadOnly StringToDateTime As Func(Of String, DateTime) =
            Function(s As String) DateTime.ParseExact(s, AllDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite Or DateTimeStyles.AllowTrailingWhite Or DateTimeStyles.NoCurrentDateDefault)

        Private Shared AllDateTimeFormats As String() = New String() {
            "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz",
            "yyyy",
            "---dd",
            "---ddZ",
            "---ddzzzzzz",
            "--MM-dd",
            "--MM-ddZ",
            "--MM-ddzzzzzz",
            "--MM--",
            "--MM--Z",
            "--MM--zzzzzz",
            "yyyy-MM",
            "yyyy-MMZ",
            "yyyy-MMzzzzzz",
            "yyyyzzzzzz",
            "yyyy-MM-dd",
            "yyyy-MM-ddZ",
            "yyyy-MM-ddzzzzzz",
            "HH:mm:ss",
            "HH:mm:ss.f",
            "HH:mm:ss.ff",
            "HH:mm:ss.fff",
            "HH:mm:ss.ffff",
            "HH:mm:ss.fffff",
            "HH:mm:ss.ffffff",
            "HH:mm:ss.fffffff",
            "HH:mm:ssZ",
            "HH:mm:ss.fZ",
            "HH:mm:ss.ffZ",
            "HH:mm:ss.fffZ",
            "HH:mm:ss.ffffZ",
            "HH:mm:ss.fffffZ",
            "HH:mm:ss.ffffffZ",
            "HH:mm:ss.fffffffZ",
            "HH:mm:sszzzzzz",
            "HH:mm:ss.fzzzzzz",
            "HH:mm:ss.ffzzzzzz",
            "HH:mm:ss.fffzzzzzz",
            "HH:mm:ss.ffffzzzzzz",
            "HH:mm:ss.fffffzzzzzz",
            "HH:mm:ss.ffffffzzzzzz",
            "HH:mm:ss.fffffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.f",
            "yyyy-MM-ddTHH:mm:ss.ff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.ffff",
            "yyyy-MM-ddTHH:mm:ss.fffff",
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fffffff",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fZ",
            "yyyy-MM-ddTHH:mm:ss.ffZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffffZ",
            "yyyy-MM-ddTHH:mm:ss.ffffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:sszzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.fffffzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.ffffffzzzzzz"
        }

        Private Shared xsi As XNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")
        Private Shared xsd As XNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema")
        Private Shared ReadOnly Property PrimitiveToXmlDataType As Dictionary(Of Type, XName)
            Get
                Static Dict As Dictionary(Of Type, XName)
                If Dict Is Nothing Then
                    Dim d As New Dictionary(Of Type, XName)
                    d.Add(GetType(Byte), "byte")
                    d.Add(GetType(UInt16), "unsignedShort")
                    d.Add(GetType(UInt32), "unsignedInt")
                    d.Add(GetType(UInt64), "unsignedLong")
                    d.Add(GetType(SByte), "byte")
                    d.Add(GetType(Int16), "short")
                    d.Add(GetType(Int32), "int")
                    d.Add(GetType(Int64), "long")
                    d.Add(GetType(Single), "float")
                    d.Add(GetType(Double), "double")
                    d.Add(GetType(Boolean), "boolean")
                    d.Add(GetType(String), "string")
                    d.Add(GetType(Decimal), "decimal")
                    Dict = d
                End If
                Return Dict
            End Get
        End Property

        Public Class CompatibleXElementToStringRangeTranslator
            Implements IProjectorToProjectorRangeTranslator(Of XElement, String)

            Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, XElement) Implements IProjectorToProjectorRangeTranslator(Of XElement, String).TranslateProjectorToProjectorRange
                Dim TypeName As XName
                If PrimitiveToXmlDataType.ContainsKey(GetType(D)) Then
                    TypeName = PrimitiveToXmlDataType(GetType(D))
                Else
                    TypeName = GetTypeFriendlyName(GetType(D))
                End If
                Return Function(v)
                           Dim s = Projector(v)
                           Return New XElement(TypeName, s)
                       End Function
            End Function
        End Class
    End Class
End Namespace
