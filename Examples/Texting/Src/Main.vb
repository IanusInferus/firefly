'==========================================================================
'
'  File:        Main.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: Kung Fu Panda lxb文件导入导出器
'  Version:     2009.08.24.
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
    Public Sub Main(ByVal argv As String())
#If Not DEBUG Then
        Try
#End If
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
#If Not DEBUG Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex, "发生以下异常:", My.Application.Info.ProductName)
        End Try
#End If
    End Sub
End Module
