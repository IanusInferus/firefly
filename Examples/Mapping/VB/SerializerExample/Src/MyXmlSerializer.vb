'==========================================================================
'
'  File:        MyXmlSerializer.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 自定义XML序列化器
'  Version:     2011.02.23.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText

''' <summary>自定义序列化器，简便起见，这里直接序列化到字节数组，而不是流</summary>
Public Class MyXmlSerializer
    Public Sub New()
    End Sub

    '版本1用序列化器
    Private SerializerVersion1Value As XmlSerializer
    Private ReadOnly Property SerializerVersion1 As XmlSerializer
        Get
            If SerializerVersion1Value Is Nothing Then
                SerializerVersion1Value = New XmlSerializer
                SerializerVersion1Value.PutReaderTranslator(New ByteArrayCodec)
                SerializerVersion1Value.PutReaderTranslator(New DataEntryVersion1To2Translator)
                SerializerVersion1Value.PutWriterTranslator(New ByteArrayCodec)
                SerializerVersion1Value.PutWriterTranslator(New DataEntryVersion1To2Translator)
            End If
            Return SerializerVersion1Value
        End Get
    End Property

    '版本2用序列化器
    Private SerializerVersion2Value As XmlSerializer
    Private ReadOnly Property SerializerVersion2 As XmlSerializer
        Get
            If SerializerVersion2Value Is Nothing Then
                SerializerVersion2Value = New XmlSerializer
                SerializerVersion2Value.PutReaderTranslator(New ByteArrayCodec)
                SerializerVersion2Value.PutWriterTranslator(New ByteArrayCodec)
            End If
            Return SerializerVersion2Value
        End Get
    End Property

    ''' <summary>读取版本1或2</summary>
    Public Function Read(ByVal Element As XElement) As DataObject
        Dim SchemaType = Element.@<SchemaType>
        If SchemaType <> "MyDataFormat" Then Throw New InvalidDataException("数据不是MYDF格式数据")
        Dim Version = Integer.Parse(Element.@<Version>)

        Select Case Version
            Case 1
                Return SerializerVersion1.Read(Of DataObject)(Element)
            Case 2
                Return SerializerVersion2.Read(Of DataObject)(Element)
            Case Else
                Throw New NotSupportedException("Unknown Version")
        End Select
    End Function

    ''' <summary>写入版本1</summary>
    Public Function WriteVersion1(ByVal Obj As DataObject) As XElement
        Dim Element = SerializerVersion1.Write(Obj)
        Element.@<SchemaType> = "MyDataFormat"
        Element.@<Version> = 1
        Return Element
    End Function

    ''' <summary>写入版本2</summary>
    Public Function Write(ByVal Obj As DataObject) As XElement
        Dim Element = SerializerVersion2.Write(Obj)
        Element.@<SchemaType> = "MyDataFormat"
        Element.@<Version> = 2
        Return Element
    End Function

    '用于将字节数组转换为字符串处理
    Private Class ByteArrayCodec
        Implements IProjectorToProjectorRangeTranslator(Of Byte(), String) 'Reader
        Implements IProjectorToProjectorDomainTranslator(Of Byte(), String) 'Writer

        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, String)) As Func(Of D, Byte()) Implements IProjectorToProjectorRangeTranslator(Of Byte(), String).TranslateProjectorToProjectorRange
            Return Function(k) Regex.Split(Projector(k).Trim(" \t\r\n".Descape.ToCharArray), "( |\t|\r|\n)+", RegexOptions.ExplicitCapture).Select(Function(s) Byte.Parse(s, Globalization.NumberStyles.HexNumber)).ToArray
        End Function
        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of String, R)) As Func(Of Byte(), R) Implements IProjectorToProjectorDomainTranslator(Of Byte(), String).TranslateProjectorToProjectorDomain
            Return Function(ba) Projector(String.Join(" ", (ba.Select(Function(b) b.ToString("X2")).ToArray)))
        End Function
    End Class
End Class
