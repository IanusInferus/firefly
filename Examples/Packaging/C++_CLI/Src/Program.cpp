//==========================================================================
//
//  File:        Program.cpp
//  Location:    Firefly.Examples <Visual C++/CLI>
//  Description: 文件包管理器示例
//  Version:     2010.11.16.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

#include "stdafx.h"
#include "DAT.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Diagnostics;
using namespace Firefly;
using namespace Firefly::Packaging;
using namespace Firefly::GUI;

void Application_ThreadException(Object^ sender, System::Threading::ThreadExceptionEventArgs^ e)
{
    ExceptionHandler::PopupException(e->Exception, gcnew StackTrace(4, true));
}

int MainWindow();

[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
    System::Threading::ThreadExceptionEventHandler^ Application_ThreadException_Handler = gcnew System::Threading::ThreadExceptionEventHandler(Application_ThreadException);

    Application::SetUnhandledExceptionMode(UnhandledExceptionMode::CatchException);
    try
    {
        Application::ThreadException += Application_ThreadException_Handler;
        return MainWindow();
    }
    catch (Exception^ ex)
    {
        ExceptionHandler::PopupException(ex, L"发生以下异常:", L"Examples.PackageManager");
        return -1;
    }
    finally
    {
        Application::ThreadException -= Application_ThreadException_Handler;
    }
}

int MainWindow()
{
    //在这里添加所有需要的文件包类型
    PackageRegister::Register(DAT::Filter, gcnew PackageRegister::PackageOpenWithPath(DAT::Open), nullptr);
    PackageRegister::Register(ISO::Filter, gcnew PackageRegister::PackageOpenWithPath(ISO::Open), nullptr);

    Application::EnableVisualStyles();
    Application::SetCompatibleTextRenderingDefault(false); 
    Application::Run(gcnew Firefly::GUI::PackageManager());

    return 0;
}
