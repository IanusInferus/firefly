'==========================================================================
'
'  File:        ExceptionHandler.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 异常处理器
'  Version:     2010.09.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.IO
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Firefly.ExceptionInfo

Public NotInheritable Class ExceptionHandler
    Private Sub New()
    End Sub

    Public Shared Sub PopupInfo(ByVal s As String)
        MessageDialog.Show(s, AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Public Shared Sub PopupException(ByVal ex As Exception)
        Dim Info As String = GetExceptionInfo(ex, New StackTrace(2, True))
        MessageDialog.Show(DebugTip, Info, AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
    Public Shared Sub PopupException(ByVal ex As Exception, ByVal ParentTrace As StackTrace)
        Dim Info As String = GetExceptionInfo(ex, ParentTrace)
        MessageDialog.Show(DebugTip, Info, AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
    Public Shared Sub PopupException(ByVal ex As Exception, ByVal DebugTip As String, ByVal Title As String)
        Dim Info As String = GetExceptionInfo(ex, New StackTrace(2, True))
        MessageDialog.Show(DebugTip, Info, Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
    Public Shared Sub PopupException(ByVal ex As Exception, ByVal ParentTrace As StackTrace, ByVal DebugTip As String, ByVal Title As String)
        Dim Info As String = GetExceptionInfo(ex, ParentTrace)
        MessageDialog.Show(DebugTip, Info, Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Public Shared DebugTip As String = "程序出现错误"
    Public Shared LogPath As String = AssemblyName & ".log"
    Public Shared CurrentFilePath As String = ""
    Public Shared CurrentSection As String = ""
    Private Shared sw As TextWriter
    Private Shared Sub WriteLineDirect(ByVal s As String)
        System.Diagnostics.Debug.WriteLine(s)
        If sw Is Nothing Then sw = TextWriter.Synchronized(Texting.Txt.CreateTextWriter(LogPath))
        sw.WriteLine(s)
        sw.Flush()
    End Sub
    Public Shared Sub WriteLine(ByVal s As String)
        s = GetIndexedText(s)
        WriteLineDirect(s)
    End Sub
    Public Shared Sub WriteWarning(ByVal s As String)
        s = GetIndexedText(s)
        WriteLineDirect(s)
    End Sub
    Public Shared Sub WriteWarning(ByVal ex As Exception)
        WriteWarning(ex.ToString)
    End Sub
    Public Shared Sub WriteError(ByVal s As String)
        s = GetIndexedText(s)
        WriteLineDirect(s)
    End Sub
    Public Shared Sub WriteError(ByVal ex As Exception)
        WriteError(GetExceptionInfo(ex, New StackTrace(2, True)))
    End Sub
    Private Shared Function GetIndexedText(ByVal s As String) As String
        If CurrentFilePath = "" AndAlso CurrentSection = "" Then
            Return s
        ElseIf CurrentSection = "" Then
            Return String.Format("{0} {1}", CurrentFilePath, s)
        Else
            Return String.Format("{0}:{1} {2}", CurrentFilePath, CurrentSection, s)
        End If
    End Function
End Class
