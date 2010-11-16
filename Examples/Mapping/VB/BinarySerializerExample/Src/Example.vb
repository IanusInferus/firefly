'==========================================================================
'
'  File:        Example.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 二进制序列化器示例
'  Version:     2010.11.17.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Mapping

Public Class DataEntryVersion1
    Public Name As String
    Public Data As Byte()
End Class

Public Class DataEntry
    Public Name As String
    Public Data As Byte()
    Public Attribute As String
End Class

Public Class ImmutableDataEntry(Of T)
    Public ReadOnly Property Name As String
        Get
            Return NameValue
        End Get
    End Property
    Public ReadOnly Property Data As T
        Get
            Return DataValue
        End Get
    End Property

    Private NameValue As String
    Private DataValue As T
    Public Sub New(ByVal Name As String, ByVal Data As T)
        Me.NameValue = Name
        Me.DataValue = Data
    End Sub
End Class

Public Class DataObject
    Public DataEntries As New Dictionary(Of String, DataEntry)
    Public ImmutableDataEntries As New Dictionary(Of String, ImmutableDataEntry(Of Byte()))
End Class

Public Module Example
    Public Sub Execute()
        Dim bs As New BinarySerializer
        bs.PutReaderTranslator(New StringAndBytesTranslator)
        bs.PutWriterTranslator(New StringAndBytesTranslator)

        Dim Obj As New DataObject
        Obj.DataEntries.Add("DataEntry1", New DataEntry With {.Name = "DataEntry1", .Data = New Byte() {1, 2, 3, 4, 5}, .Attribute = "Version2Only"})
        Obj.DataEntries.Add("DataEntry2", New DataEntry With {.Name = "DataEntry2", .Data = New Byte() {6, 7, 8, 9, 10}, .Attribute = "Version2Only"})
        Obj.ImmutableDataEntries.Add("ImmutableDataEntry1", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry1", New Byte() {1, 2, 3, 4, 5}))
        Obj.ImmutableDataEntries.Add("ImmutableDataEntry2", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry2", New Byte() {6, 7, 8, 9, 10}))

        Dim Version1Bytes = WriteVersion1(Obj)
        Dim Version2Bytes = WriteVersion2(Obj)

        Dim Version1RoundTripped = Read(Version1Bytes)
        Dim Version2RoundTripped = Read(Version2Bytes)

        Stop
    End Sub

    Public Function Read(ByVal Bytes As Byte()) As DataObject
        Dim bs As New BinarySerializer
        bs.PutReaderTranslator(New StringAndBytesTranslator)

        Using s As New StreamEx
            s.Write(Bytes)
            s.Position = 0

            Dim Version = s.ReadUInt32

            If Version = 1 Then
                bs.PutReaderTranslator(New DataEntryVersion1To2Translator)
            End If

            Return bs.Read(Of DataObject)(s)
        End Using
    End Function

    Public Function WriteVersion1(ByVal Obj As DataObject) As Byte()
        Dim bs As New BinarySerializer
        bs.PutWriterTranslator(New StringAndBytesTranslator)
        bs.PutWriterTranslator(New DataEntryVersion1To2Translator)

        Using s As New StreamEx
            Dim Version = 1
            s.WriteUInt32(Version)
            bs.Write(s, Obj)

            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function

    Public Function WriteVersion2(ByVal Obj As DataObject) As Byte()
        Dim bs As New BinarySerializer
        bs.PutWriterTranslator(New StringAndBytesTranslator)

        Using s As New StreamEx
            Dim Version = 2
            s.WriteUInt32(Version)
            bs.Write(s, Obj)

            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function
End Module

Public Class StringAndBytesTranslator
    Implements IProjectorToProjectorRangeTranslator(Of String, Byte()) 'Reader
    Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()) 'Writer

    Public Function TranslateProjectorToProjector(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
        Return Function(v) UTF16.GetString(Projector(v))
    End Function
    Public Function TranslateAggregatorToAggregator(Of R)(ByVal Aggregator As Action(Of Byte(), R)) As Action(Of String, R) Implements IAggregatorToAggregatorDomainTranslator(Of String, Byte()).TranslateAggregatorToAggregatorDomain
        Return Sub(s, v) Aggregator(UTF16.GetBytes(s), v)
    End Function
End Class

Public Class DataEntryVersion1To2Translator
    Implements IProjectorToProjectorRangeTranslator(Of DataEntry, DataEntryVersion1) 'Reader
    Implements IAggregatorToAggregatorDomainTranslator(Of DataEntry, DataEntryVersion1) 'Writer

    Public Function TranslateProjectorToProjector(Of D)(ByVal Projector As Func(Of D, DataEntryVersion1)) As Func(Of D, DataEntry) Implements IProjectorToProjectorRangeTranslator(Of DataEntry, DataEntryVersion1).TranslateProjectorToProjectorRange
        Return Function(DomainValue)
                   Dim v1 = Projector(DomainValue)
                   Return New DataEntry With {.Name = v1.Name, .Data = v1.Data, .Attribute = ""}
               End Function
    End Function
    Public Function TranslateAggregatorToAggregator(Of R)(ByVal Aggregator As Action(Of DataEntryVersion1, R)) As Action(Of DataEntry, R) Implements IAggregatorToAggregatorDomainTranslator(Of DataEntry, DataEntryVersion1).TranslateAggregatorToAggregatorDomain
        Return Sub(v2, RangeValue)
                   Dim v1 = New DataEntryVersion1 With {.Name = v2.Name, .Data = v2.Data}
                   Aggregator(v1, RangeValue)
               End Sub
    End Function
End Class
