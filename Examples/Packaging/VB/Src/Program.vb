'==========================================================================
'
'  File:        Program.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 文件包管理器示例
'  Version:     2010.11.16.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Windows.Forms
Imports System.Diagnostics
Imports Firefly
Imports Firefly.Packaging
Imports Firefly.GUI

Public Module Program

    Public Sub Application_ThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        ExceptionHandler.PopupException(e.Exception, New StackTrace(4, True))
    End Sub

    <STAThread()>
    Public Function Main() As Integer
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        Try
            AddHandler Application.ThreadException, AddressOf Application_ThreadException
            Return MainWindow()
        Catch ex As Exception
            ExceptionHandler.PopupException(ex, "发生以下异常:", "Examples.PackageManager")
            Return -1
        Finally
            RemoveHandler Application.ThreadException, AddressOf Application_ThreadException
        End Try
    End Function

    Public Function MainWindow() As Integer
        '在这里添加所有需要的文件包类型
        PackageRegister.Register(DAT.Filter, AddressOf DAT.Open)
        PackageRegister.Register(ISO.Filter, AddressOf ISO.Open)

        Application.EnableVisualStyles()
        Application.Run(New GUI.PackageManager())

        Return 0
    End Function
End Module
