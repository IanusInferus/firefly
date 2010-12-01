'==========================================================================
'
'  File:        Main.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: Kung Fu Panda lxb文件导入导出器
'  Version:     2010.12.01.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Public Module Main
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
        Dim argv = CommandLine.GetCmdLine.Arguments

        For Each f In argv
            Dim Dir = GetFileDirectory(f)
            Dim FileName = GetFileName(f)
            If IsMatchFileMask(FileName, "*.lxb") Then
                Dim Pairs = LXB.Read(f)
                Agemo.WriteFile(ChangeExtension(f, "idx"), UTF16, From p In Pairs Select p.Key.ToString("X8"))
                Agemo.WriteFile(ChangeExtension(f, "txt"), UTF16, From p In Pairs Select p.Value)
            ElseIf IsMatchFileMask(FileName, "*.txt") Then
                Dim Keys = Agemo.ReadFile(ChangeExtension(f, "idx"))
                Dim Values = Agemo.ReadFile(f)
                Dim Pairs = Keys.Select(Function(s, i) New KeyValuePair(Of Int32, String)(Int32.Parse(s, Globalization.NumberStyles.HexNumber), Values(i)))
                LXB.Write(ChangeExtension(f, "lxb"), Pairs)
            End If
        Next

        Return 0
    End Function
End Module
