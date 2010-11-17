'==========================================================================
'
'  File:        CommonTranslators.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 公共适配器
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

'用于适配DataEntry的版本1和版本2
Public Class DataEntryVersion1To2Translator
    Implements IProjectorToProjectorRangeTranslator(Of DataEntry, DataEntryVersion1) 'Reader
    Implements IProjectorToProjectorDomainTranslator(Of DataEntry, DataEntryVersion1) 'Writer

    Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, DataEntryVersion1)) As Func(Of D, DataEntry) Implements IProjectorToProjectorRangeTranslator(Of DataEntry, DataEntryVersion1).TranslateProjectorToProjectorRange
        Return Function(DomainValue)
                   Dim v1 = Projector(DomainValue)
                   Return New DataEntry With {.Name = v1.Name, .Data = v1.Data, .Attribute = ""}
               End Function
    End Function
    Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of DataEntryVersion1, R)) As Func(Of DataEntry, R) Implements IProjectorToProjectorDomainTranslator(Of DataEntry, DataEntryVersion1).TranslateProjectorToProjectorDomain
        Return Function(v2)
                   Dim v1 = New DataEntryVersion1 With {.Name = v2.Name, .Data = v2.Data}
                   Return Projector(v1)
               End Function
    End Function
End Class
