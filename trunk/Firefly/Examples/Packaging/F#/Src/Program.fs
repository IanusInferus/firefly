//==========================================================================
//
//  File:        Program.fs
//  Location:    Firefly.Examples <Visual F#>
//  Description: 文件包管理器示例
//  Version:     2010.02.18.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

open System
open System.IO
open System.Windows.Forms
open System.Reflection
open Firefly
open Firefly.Packaging
open PackageManager

[<STAThread()>]
try
    //在这里添加所有需要的文件包类型
    PackageRegister.Register(DAT.Filter, DAT.Open, null)
    PackageRegister.Register(ISO.Filter, ISO.Open, null)

    Application.EnableVisualStyles()
    Application.Run(new GUI.PackageManager())
with
| ex ->
    ExceptionHandler.PopupException(ex, "发生以下异常:", "Examples.PackageManager")
