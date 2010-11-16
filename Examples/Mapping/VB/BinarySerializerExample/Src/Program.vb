'==========================================================================
'
'  File:        Program.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 二进制序列化器示例
'  Version:     2010.11.16.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.IO
Imports Firefly

Public Module Program
    Public Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                Return -1
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Example.Execute()
        Return 0
    End Function
End Module
