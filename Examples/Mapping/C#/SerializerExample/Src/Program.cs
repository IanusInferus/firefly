//==========================================================================
//
//  File:        Program.cs
//  Location:    Firefly.Examples <Visual C#>
//  Description: 二进制序列化器示例
//  Version:     2010.11.17.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

using System;
using System.IO;
using Firefly;

namespace PackageManager
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return MainWindow();
            }
            else
            {
                try
                {
                    return MainWindow();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex));
                    return -1;
                }
            }
        }

        public static int MainWindow()
        {
            Example.Execute();
            return 0;
        }
    }
}
