'==========================================================================
'
'  File:        DataModel.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 数据模型
'  Version:     2010.11.17.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic

'版本1的DataEntry
Public Class DataEntryVersion1
    Public Name As String
    Public Data As Byte()
End Class

'当前版本(版本2)的DataEntry
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
