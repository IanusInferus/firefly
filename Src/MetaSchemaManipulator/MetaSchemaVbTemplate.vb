'==========================================================================
'
'  File:        MetaSchemaVbTemplate.vb
'  Location:    Firefly.MetaSchemaManipulator <Visual Basic .Net>
'  Description: 元类型结构VB模板模板
'  Version:     2011.03.16.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System

Public Class MetaSchemaVbTemplate
    Public Keywords As String()
    Public PrimitiveMappings As PrimitiveMapping()
    Public Templates As Template()
End Class

Public Class PrimitiveMapping
    Public Name As String
    Public PlatformName As String
End Class

Public Class Template
    Public Name As String
    Public Value As String
End Class
