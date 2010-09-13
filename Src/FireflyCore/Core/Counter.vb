'==========================================================================
'
'  File:        Counter.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 计数器
'  Version:     2009.11.21.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic

Public Class Counter
    Public Tick As Integer = 0
    Public Sub Count()
        Tick += 1
    End Sub
End Class
