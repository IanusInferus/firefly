//==========================================================================
//
//  File:        Program.cs
//  Location:    Firefly.Examples <Visual C#>
//  Description: 文件包管理器示例
//  Version:     2010.11.16.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Firefly;
using Firefly.Packaging;
using Firefly.GUI;

namespace PackageManager
{
    public static class Program
    {

        public static void Application_ThreadException(Object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ExceptionHandler.PopupException(e.Exception, new StackTrace(4, true));
        }

        [STAThread]
        public static int Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            try
            {
                Application.ThreadException += Application_ThreadException;
                return MainWindow();
            }
            catch (Exception ex)
            {
                ExceptionHandler.PopupException(ex, "发生以下异常:", "Examples.PackageManager");
                return -1;
            }
            finally
            {
                Application.ThreadException -= Application_ThreadException;
            }
        }

        public static int MainWindow()
        {
            //在这里添加所有需要的文件包类型
            PackageRegister.Register(DAT.Filter, DAT.Open, null);
            PackageRegister.Register(ISO.Filter, ISO.Open, null);

            Application.EnableVisualStyles();
            Application.Run(new Firefly.GUI.PackageManager());

            return 0;
        }
    }
}
