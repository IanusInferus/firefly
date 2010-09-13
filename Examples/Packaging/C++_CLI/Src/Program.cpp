//==========================================================================
//
//  File:        Program.cpp
//  Location:    Firefly.Examples <Visual C++/CLI>
//  Description: �ļ���������ʾ��
//  Version:     2009.07.07.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
//
//==========================================================================

#include "stdafx.h"
#include "DAT.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace Firefly;
using namespace Firefly::Packaging;

[STAThreadAttribute]
int main(array<System::String ^> ^args) {
    try {
        //���������������Ҫ���ļ�������
        PackageRegister::Register(DAT::Filter, gcnew PackageRegister::PackageOpenWithPath(DAT::Open), nullptr);
        PackageRegister::Register(ISO::Filter, gcnew PackageRegister::PackageOpenWithPath(ISO::Open), nullptr);

        Application::EnableVisualStyles();
        Application::SetCompatibleTextRenderingDefault(false); 
        Application::Run(gcnew Firefly::GUI::PackageManager());
    }
    catch (Exception^ ex) {
        ExceptionHandler::PopupException(ex, "���������쳣:", "Examples.PackageManager");
    }
	return 0;
}
