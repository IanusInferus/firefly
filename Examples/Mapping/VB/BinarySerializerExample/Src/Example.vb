'==========================================================================
'
'  File:        Example.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 二进制序列化器示例
'  Version:     2010.11.16.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Firefly
Imports Firefly.Mapping

Public Class DataEntry
    Public Name As String
    Public Data As Byte()
End Class

Public Class ImmutableDataEntry(Of T)
    Public ReadOnly Property Name As String
        Get
            Return NameValue
        End Get
    End Property
    Public ReadOnly Property Data As T
        Get
            Return Data
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

        Dim obj As New DataObject
        obj.DataEntries.Add("DataEntry1", New DataEntry With {.Name = "DataEntry1", .Data = New Byte() {1, 2, 3, 4, 5}})
        obj.DataEntries.Add("DataEntry2", New DataEntry With {.Name = "DataEntry2", .Data = New Byte() {6, 7, 8, 9, 10}})
        obj.ImmutableDataEntries.Add("ImmutableDataEntry1", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry1", New Byte() {1, 2, 3, 4, 5}))
        obj.ImmutableDataEntries.Add("ImmutableDataEntry2", New ImmutableDataEntry(Of Byte())("ImmutableDataEntry2", New Byte() {6, 7, 8, 9, 10}))

        Dim Bytes As Byte()

        Using s As New StreamEx
            bs.Write(s, obj)

            s.Position = 0
            Bytes = s.Read(s.Length)
        End Using

        Dim DeserializedObject As DataObject
        Using s As New StreamEx
            s.Write(Bytes)

            s.Position = 0
            DeserializedObject = bs.Read(Of DataObject)(s)
        End Using

        Stop
    End Sub
End Module
