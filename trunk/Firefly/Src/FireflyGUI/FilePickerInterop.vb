'==========================================================================
'
'  File:        FilePickerInterop.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 文件选取对话框 - Win32调用包装
'  Version:     2009.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Runtime.InteropServices

Public NotInheritable Class FilePickerInterop
    Public Shared Function GetAssociatedIcon(ByVal Path As String, ByVal large As Boolean) As Icon
        Dim info As New SHFILEINFO(True)
        Dim cbFileInfo As Integer = Marshal.SizeOf(info)
        Dim flags As SHGFI

        If large Then
            flags = SHGFI.Icon Or SHGFI.LargeIcon
        Else
            flags = SHGFI.Icon Or SHGFI.SmallIcon
        End If

        SHGetFileInfo(Path, 0, info, CUInt(cbFileInfo), flags)

        If info.hIcon = IntPtr.Zero Then Return Nothing
        Dim i = Icon.FromHandle(info.hIcon).Clone
        DestroyIcon(info.hIcon)

        Return i
    End Function
    Public Shared Function GetTypeName(ByVal Path As String) As String
        Dim info As New SHFILEINFO(True)
        Dim cbFileInfo As Integer = Marshal.SizeOf(info)
        Dim flags As SHGFI
        flags = SHGFI.TypeName

        SHGetFileInfo(Path, 0, info, CUInt(cbFileInfo), flags)

        Return info.szTypeName
    End Function

    <DllImport("shell32.dll", CharSet:=CharSet.Ansi)> _
    Private Shared Function SHGetFileInfo(ByVal pszPath As String, ByVal dwFileAttributes As UInt32, <Out()> ByRef psfi As SHFILEINFO, ByVal cbfileInfo As UInteger, ByVal uFlags As SHGFI) As IntPtr
    End Function
    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function DestroyIcon(ByVal hIcon As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Const MAX_PATH As Integer = 260
    Private Const MAX_TYPE As Integer = 80

    Private Structure SHFILEINFO
        Public Sub New(ByVal b As Boolean)
            hIcon = IntPtr.Zero
            iIcon = 0
            dwAttributes = 0
            szDisplayName = ""
            szTypeName = ""
        End Sub

        Public hIcon As IntPtr
        Public iIcon As Integer
        Public dwAttributes As UInt32
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_TYPE)> _
        Public szTypeName As String
    End Structure

    <Flags()> _
    Enum SHGFI As UInteger
        ''' <summary>get icon</summary>
        Icon = &H100

        ''' <summary>get display name</summary>
        DisplayName = &H200

        ''' <summary>get type name</summary>
        TypeName = &H400

        ''' <summary>get attributes</summary>
        Attributes = &H800

        ''' <summary>get icon location</summary>
        IconLocation = &H1000

        ''' <summary>return exe type</summary>
        ExeType = &H2000

        ''' <summary>get system icon index</summary>
        SysIconIndex = &H4000

        ''' <summary>put a link overlay on icon</summary>
        LinkOverlay = &H8000

        ''' <summary>show icon in selected state</summary>
        Selected = &H10000

        ''' <summary>get only specified attributes</summary>
        Attr_Specified = &H20000

        ''' <summary>get large icon</summary>
        LargeIcon = &H0

        ''' <summary>get small icon</summary>
        SmallIcon = &H1

        ''' <summary>get open icon</summary>
        OpenIcon = &H2

        ''' <summary>get shell size icon</summary>
        ShellIconize = &H4

        ''' <summary>pszPath is a pidl</summary>
        PIDL = &H8

        ''' <summary>use passed dwFileAttribute</summary>
        UseFileAttributes = &H10

        ''' <summary>apply the appropriate overlays</summary>
        AddOverlays = &H20

        ''' <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
        OverlayIndex = &H40
    End Enum
End Class
