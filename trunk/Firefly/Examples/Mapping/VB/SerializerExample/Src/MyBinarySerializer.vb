'==========================================================================
'
'  File:        MyBinarySerializer.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 自定义二进制序列化器
'  Version:     2011.02.23.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.TextEncoding
Imports Firefly.Mapping

''' <summary>自定义二进制序列化器，简便起见，这里直接序列化到字节数组，而不是流</summary>
Public Class MyBinarySerializer
    Public Sub New()
    End Sub

    '版本1用序列化器
    Private SerializerVersion1Value As BinarySerializer
    Private ReadOnly Property SerializerVersion1 As BinarySerializer
        Get
            If SerializerVersion1Value Is Nothing Then
                SerializerVersion1Value = New BinarySerializer
                SerializerVersion1Value.PutReaderTranslator(New StringCodec)
                SerializerVersion1Value.PutReaderTranslator(New DataEntryVersion1To2Translator)
                SerializerVersion1Value.PutWriterTranslator(New StringCodec)
                SerializerVersion1Value.PutWriterTranslator(New DataEntryVersion1To2Translator)
            End If
            Return SerializerVersion1Value
        End Get
    End Property

    '版本2用序列化器
    Private SerializerVersion2Value As BinarySerializer
    Private ReadOnly Property SerializerVersion2 As BinarySerializer
        Get
            If SerializerVersion2Value Is Nothing Then
                SerializerVersion2Value = New BinarySerializer
                SerializerVersion2Value.PutReaderTranslator(New StringCodec)
                SerializerVersion2Value.PutWriterTranslator(New StringCodec)
            End If
            Return SerializerVersion2Value
        End Get
    End Property

    ''' <summary>读取版本1或2</summary>
    Public Function Read(ByVal Bytes As Byte()) As DataObject
        Using s = Streams.CreateMemoryStream()
            s.Write(Bytes)
            s.Position = 0

            If s.ReadSimpleString(4) <> "MYDF" Then Throw New InvalidDataException("数据不是MYDF格式数据")
            Dim Version = s.ReadUInt32

            Select Case Version
                Case 1
                    Return SerializerVersion1.Read(Of DataObject)(s)
                Case 2
                    Return SerializerVersion2.Read(Of DataObject)(s)
                Case Else
                    Throw New NotSupportedException("Unknown Version")
            End Select
        End Using
    End Function

    ''' <summary>写入版本1</summary>
    Public Function WriteVersion1(ByVal Obj As DataObject) As Byte()
        Using s = Streams.CreateMemoryStream()
            Dim Version = 1
            s.WriteSimpleString("MYDF", 4)
            s.WriteUInt32(Version)
            SerializerVersion1.Write(s, Obj)

            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function

    ''' <summary>写入版本2</summary>
    Public Function Write(ByVal Obj As DataObject) As Byte()
        Using s = Streams.CreateMemoryStream()
            Dim Version = 2
            s.WriteSimpleString("MYDF", 4)
            s.WriteUInt32(Version)
            SerializerVersion2.Write(s, Obj)

            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function

    '用于将字符串转换为字节数组处理
    Private Class StringCodec
        Implements IProjectorToProjectorRangeTranslator(Of String, Byte()) 'Reader
        Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()) 'Writer

        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
            Return Function(v) UTF16.GetString(Projector(v))
        End Function
        Public Function TranslateAggregatorToAggregatorDomain(Of R)(ByVal Aggregator As Action(Of Byte(), R)) As Action(Of String, R) Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()).TranslateAggregatorToAggregatorDomain
            Return Sub(s, v) Aggregator(UTF16.GetBytes(s), v)
        End Function
    End Class
End Class
