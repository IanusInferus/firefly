'==========================================================================
'
'  File:        Program.vb
'  Location:    Firefly.Examples <Visual Basic .Net>
'  Description: 文件包管理器示例
'  Version:     2009.07.07.
'  Author:      F.R.C.
'  Copyright(C) Public Domain
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.Packaging

Public Module Program

    Public Sub Main(ByVal argv As String())
#If Not DEBUG Then
        Try
#End If
        '在这里添加所有需要的文件包类型
        PackageRegister.Register(DAT.Filter, AddressOf DAT.Open)
        PackageRegister.Register(ISO.Filter, AddressOf ISO.Open)

        Application.EnableVisualStyles()
        Application.Run(New GUI.PackageManager())
#If Not DEBUG Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex, "发生以下异常:", My.Application.Info.ProductName)
        End Try
#End If
    End Sub
End Module
