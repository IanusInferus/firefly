'==========================================================================
'
'  File:        Result.vb
'  Location:    Firefly.Texting.TreeFormat <Visual Basic .Net>
'  Description: Tree文件解析结果
'  Version:     2011.06.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic

Namespace Texting.TreeFormat
    Public Class TreeFormatResult
        Public Value As Semantics.Forest
        Public Positions As Dictionary(Of Object, Syntax.FileTextRange)
    End Class
End Namespace
