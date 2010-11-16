//==========================================================================
//
//  File:        Program.fs
//  Location:    Firefly.Examples <Visual F#>
//  Description: 文件包管理器示例
//  Version:     2010.11.16.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

open System
open System.IO
open System.Windows.Forms
open System.Reflection
open System.Diagnostics
open Firefly
open Firefly.Packaging
open Firefly.GUI
open PackageManager

let Application_ThreadException_Handler =
    new System.Threading.ThreadExceptionEventHandler(
        fun sender e -> ExceptionHandler.PopupException(e.Exception, new StackTrace(4, true))
    )

let ProtectedMain (innerMain : unit -> int) : int =
    Application.SetUnhandledExceptionMode UnhandledExceptionMode.CatchException
    try
        try
            Application.ThreadException.AddHandler Application_ThreadException_Handler
            innerMain ()
        with
        | ex ->
            ExceptionHandler.PopupException (ex, "发生以下异常:", "Examples.PackageManager")
            -1
    finally
        Application.ThreadException.RemoveHandler Application_ThreadException_Handler

let MainWindow () : int =
    //在这里添加所有需要的文件包类型
    PackageRegister.Register (DAT.Filter, DAT.Open, null)
    PackageRegister.Register (ISO.Filter, ISO.Open, null)

    Application.EnableVisualStyles ()
    Application.Run (new GUI.PackageManager ())

    0

[<STAThread()>]
MainWindow |> ProtectedMain |> ignore
