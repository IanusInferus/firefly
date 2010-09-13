//==========================================================================
//
//  File:        Program.cs
//  Location:    Firefly.Examples <Visual C#>
//  Description: 文件包管理器示例
//  Version:     2009.07.07.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firefly;
using Firefly.Packaging;

namespace PackageManager {
    static class Program {
        [STAThread]
        static void Main() {
            try {
                //在这里添加所有需要的文件包类型
                PackageRegister.Register(DAT.Filter, DAT.Open, null);
                PackageRegister.Register(ISO.Filter, ISO.Open, null);

                Application.EnableVisualStyles();
                Application.Run(new Firefly.GUI.PackageManager());
            }
            catch (Exception ex) {
                ExceptionHandler.PopupException(ex, "发生以下异常:", "Examples.PackageManager");
            }
        }
    }
}
