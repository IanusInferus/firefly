'==========================================================================
'
'  File:        FileDialogInterop.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 扩展文件对话框类，Win7下存在兼容性问题，已过时，请使用FilePicker
'  Version:     2009.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On

Imports System
Imports System.Text
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Public Module FileDialogInterop
    Public Delegate Function WndEnumProc(ByVal hWnd As IntPtr, ByVal lParam As Int32) As Boolean

    Public Declare Unicode Function SendMessage Lib "user32.dll" Alias "SendMessageW" (ByVal hWnd As HandleRef, ByVal msg As UInt32, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    Public Declare Unicode Function SendMessage Lib "user32.dll" Alias "SendMessageW" (ByVal hWnd As HandleRef, ByVal msg As UInt32, ByVal wParam As IntPtr, ByVal lParam As StringBuilder) As IntPtr

    Public Declare Function GetParent Lib "user32.dll" (ByVal hWnd As HandleRef) As IntPtr
    Public Declare Unicode Function GetClassName Lib "user32.dll" Alias "GetClassNameW" (ByVal hWnd As HandleRef, ByVal param As StringBuilder, ByVal length As Int32) As Int32
    Public Declare Function GetDlgCtrlID Lib "user32.dll" Alias "GetClassNameW" (ByVal hWnd As HandleRef) As Int32
    Public Declare Function EnumChildWindows Lib "user32.dll" (ByVal hWndParent As HandleRef, ByVal lpEnumFunc As WndEnumProc, ByVal lParam As IntPtr) As Boolean

    Public Function IntPtrToItemIdList(ByVal pNativeData As IntPtr) As Byte()()
        Dim IdList As New List(Of Byte())()
        While True
            Dim Length As Int16 = Marshal.ReadInt16(pNativeData)
            pNativeData = New IntPtr(pNativeData.ToInt64() + 2)
            If Length = 0 Then
                Exit While
            End If
            Length -= 2S
            Dim ID As Byte() = New Byte(Length - 1) {}
            For i As Int32 = 0 To Length - 1
                ID(i) = Marshal.ReadByte(pNativeData)
                pNativeData = New IntPtr(pNativeData.ToInt64() + 1)
            Next
            IdList.Add(ID)
        End While
        Return IdList.ToArray()
    End Function
End Module

<StructLayout(LayoutKind.Sequential)> _
Public Structure LVITEM
    Public mask As UInt32
    Public iItem As Int32
    Public iSubItem As Int32
    Public state As UInt32
    Public stateMask As UInt32
    Public pszText As IntPtr
    Public cchTextMax As Int32
    Public iImage As Int32
    Public lParam As IntPtr
    Public iIndent As Int32
    Public iGroupId As Int32
End Structure

Public Enum CommonDialogMessages
    CDM_FIRST = (1024 + 100)
    CDM_GETSPEC = (CDM_FIRST + 0)
    CDM_GETFILEPATH = (CDM_FIRST + 1)
    CDM_GETFOLDERPATH = (CDM_FIRST + 2)
    CDM_GETFOLDERIDLIST = (CDM_FIRST + 3)
    CDM_SETCONTROLTEXT = (CDM_FIRST + 4)
    CDM_HIDECONTROL = (CDM_FIRST + 5)
    CDM_SETDEFEXT = (CDM_FIRST + 6)
End Enum

Public Enum ListViewMessages As UInt32
    LVM_FIRST = 4096
    ' ListView messages 
    LVM_GETITEMCOUNT = (LVM_FIRST + 4)
    LVM_GETITEM = (LVM_FIRST + 75)
End Enum

'Windows Messages 
Public Enum Msg
    WM_NULL = 0
    WM_CREATE = 1
    WM_DESTROY = 2
    WM_MOVE = 3
    WM_SIZE = 5
    WM_ACTIVATE = 6
    WM_SETFOCUS = 7
    WM_KILLFOCUS = 8
    WM_ENABLE = 10
    WM_SETREDRAW = 11
    WM_SETTEXT = 12
    WM_GETTEXT = 13
    WM_GETTEXTLENGTH = 14
    WM_PAINT = 15
    WM_CLOSE = 16
    WM_QUERYENDSESSION = 17
    WM_QUIT = 18
    WM_QUERYOPEN = 19
    WM_ERASEBKGND = 20
    WM_SYSCOLORCHANGE = 21
    WM_ENDSESSION = 22
    WM_SHOWWINDOW = 24
    WM_CTLCOLOR = 25
    WM_WININICHANGE = 26
    WM_SETTINGCHANGE = 26
    WM_DEVMODECHANGE = 27
    WM_ACTIVATEAPP = 28
    WM_FONTCHANGE = 29
    WM_TIMECHANGE = 30
    WM_CANCELMODE = 31
    WM_SETCURSOR = 32
    WM_MOUSEACTIVATE = 33
    WM_CHILDACTIVATE = 34
    WM_QUEUESYNC = 35
    WM_GETMINMAXINFO = 36
    WM_PAINTICON = 38
    WM_ICONERASEBKGND = 39
    WM_NEXTDLGCTL = 40
    WM_SPOOLERSTATUS = 42
    WM_DRAWITEM = 43
    WM_MEASUREITEM = 44
    WM_DELETEITEM = 45
    WM_VKEYTOITEM = 46
    WM_CHARTOITEM = 47
    WM_SETFONT = 48
    WM_GETFONT = 49
    WM_SETHOTKEY = 50
    WM_GETHOTKEY = 51
    WM_QUERYDRAGICON = 55
    WM_COMPAREITEM = 57
    WM_GETOBJECT = 61
    WM_COMPACTING = 65
    WM_COMMNOTIFY = 68
    WM_WINDOWPOSCHANGING = 70
    WM_WINDOWPOSCHANGED = 71
    WM_POWER = 72
    WM_COPYDATA = 74
    WM_CANCELJOURNAL = 75
    WM_NOTIFY = 78
    WM_INPUTLANGCHANGEREQUEST = 80
    WM_INPUTLANGCHANGE = 81
    WM_TCARD = 82
    WM_HELP = 83
    WM_USERCHANGED = 84
    WM_NOTIFYFORMAT = 85
    WM_CONTEXTMENU = 123
    WM_STYLECHANGING = 124
    WM_STYLECHANGED = 125
    WM_DISPLAYCHANGE = 126
    WM_GETICON = 127
    WM_SETICON = 128
    WM_NCCREATE = 129
    WM_NCDESTROY = 130
    WM_NCCALCSIZE = 131
    WM_NCHITTEST = 132
    WM_NCPAINT = 133
    WM_NCACTIVATE = 134
    WM_GETDLGCODE = 135
    WM_SYNCPAINT = 136
    WM_NCMOUSEMOVE = 160
    WM_NCLBUTTONDOWN = 161
    WM_NCLBUTTONUP = 162
    WM_NCLBUTTONDBLCLK = 163
    WM_NCRBUTTONDOWN = 164
    WM_NCRBUTTONUP = 165
    WM_NCRBUTTONDBLCLK = 166
    WM_NCMBUTTONDOWN = 167
    WM_NCMBUTTONUP = 168
    WM_NCMBUTTONDBLCLK = 169
    WM_NCXBUTTONDOWN = 171
    WM_NCXBUTTONUP = 172
    WM_NCXBUTTONDBLCLK = 173
    WM_KEYDOWN = 256
    WM_KEYUP = 257
    WM_CHAR = 258
    WM_DEADCHAR = 259
    WM_SYSKEYDOWN = 260
    WM_SYSKEYUP = 261
    WM_SYSCHAR = 262
    WM_SYSDEADCHAR = 263
    WM_KEYLAST = 264
    WM_IME_STARTCOMPOSITION = 269
    WM_IME_ENDCOMPOSITION = 270
    WM_IME_COMPOSITION = 271
    WM_IME_KEYLAST = 271
    WM_INITDIALOG = 272
    WM_COMMAND = 273
    WM_SYSCOMMAND = 274
    WM_TIMER = 275
    WM_HSCROLL = 276
    WM_VSCROLL = 277
    WM_INITMENU = 278
    WM_INITMENUPOPUP = 279
    WM_MENUSELECT = 287
    WM_MENUCHAR = 288
    WM_ENTERIDLE = 289
    WM_MENURBUTTONUP = 290
    WM_MENUDRAG = 291
    WM_MENUGETOBJECT = 292
    WM_UNINITMENUPOPUP = 293
    WM_MENUCOMMAND = 294
    WM_CTLCOLORMSGBOX = 306
    WM_CTLCOLOREDIT = 307
    WM_CTLCOLORLISTBOX = 308
    WM_CTLCOLORBTN = 309
    WM_CTLCOLORDLG = 310
    WM_CTLCOLORSCROLLBAR = 311
    WM_CTLCOLORSTATIC = 312
    WM_MOUSEMOVE = 512
    WM_LBUTTONDOWN = 513
    WM_LBUTTONUP = 514
    WM_LBUTTONDBLCLK = 515
    WM_RBUTTONDOWN = 516
    WM_RBUTTONUP = 517
    WM_RBUTTONDBLCLK = 518
    WM_MBUTTONDOWN = 519
    WM_MBUTTONUP = 520
    WM_MBUTTONDBLCLK = 521
    WM_MOUSEWHEEL = 522
    WM_XBUTTONDOWN = 523
    WM_XBUTTONUP = 524
    WM_XBUTTONDBLCLK = 525
    WM_PARENTNOTIFY = 528
    WM_ENTERMENULOOP = 529
    WM_EXITMENULOOP = 530
    WM_NEXTMENU = 531
    WM_SIZING = 532
    WM_CAPTURECHANGED = 533
    WM_MOVING = 534
    WM_DEVICECHANGE = 537
    WM_MDICREATE = 544
    WM_MDIDESTROY = 545
    WM_MDIACTIVATE = 546
    WM_MDIRESTORE = 547
    WM_MDINEXT = 548
    WM_MDIMAXIMIZE = 549
    WM_MDITILE = 550
    WM_MDICASCADE = 551
    WM_MDIICONARRANGE = 552
    WM_MDIGETACTIVE = 553
    WM_MDISETMENU = 560
    WM_ENTERSIZEMOVE = 561
    WM_EXITSIZEMOVE = 562
    WM_DROPFILES = 563
    WM_MDIREFRESHMENU = 564
    WM_IME_SETCONTEXT = 641
    WM_IME_NOTIFY = 642
    WM_IME_CONTROL = 643
    WM_IME_COMPOSITIONFULL = 644
    WM_IME_SELECT = 645
    WM_IME_CHAR = 646
    WM_IME_REQUEST = 648
    WM_IME_KEYDOWN = 656
    WM_IME_KEYUP = 657
    WM_MOUSEHOVER = 673
    WM_MOUSELEAVE = 675
    WM_CUT = 768
    WM_COPY = 769
    WM_PASTE = 770
    WM_CLEAR = 771
    WM_UNDO = 772
    WM_RENDERFORMAT = 773
    WM_RENDERALLFORMATS = 774
    WM_DESTROYCLIPBOARD = 775
    WM_DRAWCLIPBOARD = 776
    WM_PAINTCLIPBOARD = 777
    WM_VSCROLLCLIPBOARD = 778
    WM_SIZECLIPBOARD = 779
    WM_ASKCBFORMATNAME = 780
    WM_CHANGECBCHAIN = 781
    WM_HSCROLLCLIPBOARD = 782
    WM_QUERYNEWPALETTE = 783
    WM_PALETTEISCHANGING = 784
    WM_PALETTECHANGED = 785
    WM_HOTKEY = 786
    WM_PRINT = 791
    WM_PRINTCLIENT = 792
    WM_THEME_CHANGED = 794
    WM_HANDHELDFIRST = 856
    WM_HANDHELDLAST = 863
    WM_AFXFIRST = 864
    WM_AFXLAST = 895
    WM_PENWINFIRST = 896
    WM_PENWINLAST = 911
    WM_APP = 32768
    WM_USER = 1024
    WM_REFLECT = WM_USER + 7168
End Enum
