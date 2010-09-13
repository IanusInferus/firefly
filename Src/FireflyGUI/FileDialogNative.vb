'==========================================================================
'
'  File:        FileDialogNative.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 扩展文件对话框类，Win7下存在兼容性问题，已过时，请使用FilePicker
'               本文件使用了http://www.codeproject.com/KB/dialog/OpenFileDialogEx.aspx中提到的方法
'               已知问题1：快捷方式等隐藏扩展名文件无法正确处理
'               已知问题2：按回车时无法自动进入文件夹
'  Version:     2009.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

Partial Public Class FileDialogEx
    Inherits Form
    Protected WithEvents OpenDialogValue As OpenFileDialog
    Protected WithEvents SaveDialogValue As SaveFileDialog

    Protected Friend SetIntializeFileNameToNull As Boolean = False

    Protected WithEvents mNativeDialog As FileDialogBase = Nothing
    Protected mDialogHandle As IntPtr = IntPtr.Zero
    Protected Overloads Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = Msg.WM_ACTIVATE Then
            If mDialogHandle <> m.LParam Then
                Dim ClassNameSB As New StringBuilder(256)
                FileDialogInterop.GetClassName(New HandleRef(Me, m.LParam), ClassNameSB, ClassNameSB.Capacity)
                Dim ClassName = ClassNameSB.ToString

                If ClassName = "#32770" Then
                    mDialogHandle = m.LParam
                    If mNativeDialog IsNot Nothing Then
                        SetIntializeFileNameToNull = mNativeDialog.SetIntializeFileNameToNull
                        mNativeDialog.Dispose()
                    End If
                    mNativeDialog = New FileDialogBase(mDialogHandle, Me)
                    mNativeDialog.SetIntializeFileNameToNull = SetIntializeFileNameToNull
                End If
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    Protected ReadOnly Property Dialog() As FileDialog
        Get
            If IsSaveDialog Then Return SaveDialogValue
            Return OpenDialogValue
        End Get
    End Property

    Public Shadows Function ShowDialog() As DialogResult
        Dim returnDialogResult As DialogResult = DialogResult.Cancel
        FileOKValue = False
        Me.Show()
        Dim HideFileExt As Integer = CInt(Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced").GetValue("HideFileExt", 1))
        If HideFileExt <> 0 Then
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", True).SetValue("HideFileExt", 0)
        End If

        If Me.FileName.EndsWith(Path.DirectorySeparatorChar) Then
            Me.FileName = Path.Combine(Me.FileName, Path.GetFileName(Me.FileName.TrimEnd(Path.DirectorySeparatorChar)))
            mNativeDialog.SetIntializeFileNameToNull = True
        End If

        returnDialogResult = Dialog.ShowDialog(Me)

        If HideFileExt <> 0 Then
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", True).SetValue("HideFileExt", 1)
        End If

        Me.Hide()

        If FileOKValue Then Return Windows.Forms.DialogResult.OK
        Return returnDialogResult
    End Function

    Protected Class FileDialogBase
        Inherits NativeWindow
        Implements IDisposable

        Protected mSourceControl As FileDialogEx
        Protected mFileDialogHandle As IntPtr
        Protected mListViewHandle As IntPtr
        Protected mEditFileNameHandle As IntPtr

        Protected Friend SetIntializeFileNameToNull As Boolean = False

        Public Sub New(ByVal Handle As IntPtr, ByVal SourceControl As FileDialogEx)
            mFileDialogHandle = Handle
            mSourceControl = SourceControl
            AssignHandle(mFileDialogHandle)
        End Sub

        Public Event ButtonOpenClick(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
        Public Event SelectionChangedClick(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)

        Protected Function FileDialogEnumWindowCallBack(ByVal hWnd As IntPtr, ByVal lParam As Integer) As Boolean
            Dim ClassNameSB As New StringBuilder(256)
            FileDialogInterop.GetClassName(New HandleRef(Me, hWnd), ClassNameSB, ClassNameSB.Capacity)
            Dim ClassName = ClassNameSB.ToString
            Dim ControlId As Int32 = GetDlgCtrlID(New HandleRef(Me, hWnd))

            If ClassName = "SysListView32" Then
                mListViewHandle = hWnd
            ElseIf ClassName = "Edit" Then
                mEditFileNameHandle = hWnd
            End If

            Return True
        End Function

        Protected Sub FileDialogEnumWindow()
            FileDialogInterop.EnumChildWindows(New HandleRef(Me, mFileDialogHandle), AddressOf FileDialogEnumWindowCallBack, New IntPtr(0))
        End Sub

        Protected Function GetFilePath() As String
            Dim Length As IntPtr = FileDialogInterop.SendMessage(New HandleRef(Me, mFileDialogHandle), CUInt(CommonDialogMessages.CDM_GETFILEPATH), IntPtr.Zero, IntPtr.Zero)
            If Length.ToInt32 < 0 Then Return ""

            Dim FilePath As StringBuilder = New StringBuilder(Length.ToInt32)
            FileDialogInterop.SendMessage(New HandleRef(Me, mFileDialogHandle), CUInt(CommonDialogMessages.CDM_GETFILEPATH), New IntPtr(FilePath.Capacity), FilePath)
            Dim t = FilePath.ToString
            If t.Contains("""") Then
                Dim a = t.IndexOf("""")
                Dim b = t.IndexOf("""", a + 1)
                t = t.Substring(0, a) & t.Substring(a + 1, b - a - 1)
            End If
            Return t
        End Function

        Protected Function GetFolderPath() As String
            Dim Length As IntPtr = FileDialogInterop.SendMessage(New HandleRef(Me, mFileDialogHandle), CUInt(CommonDialogMessages.CDM_GETFOLDERPATH), IntPtr.Zero, IntPtr.Zero)
            If Length.ToInt32 < 0 Then Return ""

            Dim FolderPath As StringBuilder = New StringBuilder(Length.ToInt32)
            FileDialogInterop.SendMessage(New HandleRef(Me, mFileDialogHandle), CUInt(CommonDialogMessages.CDM_GETFOLDERPATH), New IntPtr(FolderPath.Capacity), FolderPath)
            Return FolderPath.ToString
        End Function

        Protected Function GetFileNames() As String()
            Dim Length As IntPtr = FileDialogInterop.SendMessage(New HandleRef(Me, mListViewHandle), ListViewMessages.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero)

            Dim FolderPath As String = ""
            If Not mSourceControl.CanSelectFolders Then
                FolderPath = GetFolderPath()
            End If

            Dim FileNames As New List(Of String)
            Dim FileName As String = ""
            For n = 0 To CInt(Length.ToInt64 - 1)
                Dim NamePtr As IntPtr = Marshal.AllocHGlobal(2048 * 4)
                Dim Item As New LVITEM() With {.mask = &HFFFFFFFFUI, .iItem = n, .iSubItem = 0, .state = 0, .stateMask = &HFFFFFFFFUI, .pszText = NamePtr, .cchTextMax = 2048}
                Dim p As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(LVITEM)) * 4)
                Marshal.StructureToPtr(Item, p, True)
                Dim Successful As IntPtr = SendMessage(New HandleRef(Me, mListViewHandle), CInt(ListViewMessages.LVM_GETITEM), New IntPtr(Marshal.SizeOf(GetType(LVITEM))), p)
                Item = DirectCast(Marshal.PtrToStructure(p, GetType(LVITEM)), LVITEM)
                Marshal.FreeHGlobal(p)
                Dim Name As String = ""
                If Item.pszText <> New IntPtr(-1) Then
                    Name = DirectCast(Marshal.PtrToStringUni(NamePtr), String)
                End If
                Marshal.FreeHGlobal(NamePtr)

                If Successful <> IntPtr.Zero Then Continue For

                'state
                'LVIS_FOCUSED 0x0001
                'LVIS_SELECTED 0x0002
                Dim Focused As Boolean = CBool(Item.state And 1)
                Dim Selected As Boolean = CBool(Item.state And 2)

                If Name <> "" Then
                    'System.Console.WriteLine("Cap " & Name & " " & Focused & " " & Selected)
                    If Not mSourceControl.CanSelectFiles Then
                        If File.Exists(Path.Combine(FolderPath, Name)) Then Continue For
                    End If
                    If Not mSourceControl.CanSelectFolders Then
                        If Directory.Exists(Path.Combine(FolderPath, Name)) Then Continue For
                    End If

                    If Selected Then FileNames.Add(Name)
                    If Focused Then FileName = Name
                Else
                    'System.Console.WriteLine("Cap " & Focused & " " & Selected)
                End If
            Next

            If FileName <> "" AndAlso FileNames.Contains(FileName) Then
                FileNames.Remove(FileName)
                FileNames.Insert(0, FileName)
            End If

            'If FileNames.Count = 0 Then
            '    Console.WriteLine("Cap " & FileNames.Count & " " & Length.ToInt64)
            'End If

            Return FileNames.ToArray
        End Function

        Protected Function GetPathFromFolderPath(ByVal FilePath As String, ByVal FolderPath As String) As String
            If FilePath = FolderPath AndAlso Not FilePath.EndsWith(Path.DirectorySeparatorChar) Then
                If FilePath = "" Then Return FilePath
                Return FilePath & Path.DirectorySeparatorChar
            End If
            Return FilePath
        End Function

        Protected Function GetEditFileNameText() As String
            Dim TextLength As IntPtr = FileDialogInterop.SendMessage(New HandleRef(Me, mEditFileNameHandle), CUInt(Msg.WM_GETTEXTLENGTH), IntPtr.Zero, IntPtr.Zero)
            Dim Text As StringBuilder = New StringBuilder(TextLength.ToInt32 + 2)
            FileDialogInterop.SendMessage(New HandleRef(Me, mEditFileNameHandle), CUInt(Msg.WM_GETTEXT), New IntPtr(Text.Capacity), Text)

            Return Text.ToString
        End Function

        Protected Function GetEditFileNames() As String()
            Dim Text = GetEditFileNameText()
            If Text Is Nothing OrElse Text.Trim = "" Then Return New String() {}
            If Not Text.Contains("""") Then
                Return New String() {Text}
            End If

            Dim FileNames As New List(Of String)
            Dim FileName As New StringBuilder
            Dim Odd As Boolean = False
            For Each c In Text
                If c = """" Then
                    Odd = Not Odd
                    If Not Odd Then
                        FileNames.Add(FileName.ToString)
                        FileName = New StringBuilder
                    End If
                ElseIf Odd Then
                    FileName.Append(c)
                End If
            Next
            If Odd Then Throw New InvalidCastException

            Return FileNames.ToArray
        End Function

        Protected Sub SetEditFileNames(ByVal FileNames As String())
            Dim sb As StringBuilder
            If FileNames IsNot Nothing Then
                If FileNames.Length = 1 Then
                    sb = New StringBuilder(FileNames(0))
                Else
                    Dim Names As String() = CType(FileNames.Clone, String())
                    For n = 0 To Names.Length - 1
                        Names(n) = """" & Names(n) & """"
                    Next

                    sb = New StringBuilder(String.Join(" ", Names))
                End If
            Else
                sb = New StringBuilder
            End If
            FileDialogInterop.SendMessage(New HandleRef(Me, mEditFileNameHandle), CUInt(Msg.WM_SETTEXT), IntPtr.Zero, sb)
        End Sub

        Protected Enum AfterButtonOpenClicked
            None = 0
            RefreshSelection = 1
            InternalMessageHandling = 2
        End Enum

        Protected Function OnButtonOpenClicked(ByRef m As Message) As AfterButtonOpenClicked
            'If mSourceControl.EnableSelectFolders OrElse (mSourceControl.IsSaveDialog AndAlso mSourceControl.MultiselectValue) Then
            FileDialogEnumWindow()

            Dim FilePath = GetFilePath()
            Dim FolderPath = GetFolderPath()
            Dim FileNames As String() = Nothing

            Try
                FileNames = GetEditFileNames()
            Catch ex As Exception
                Return AfterButtonOpenClicked.RefreshSelection Or AfterButtonOpenClicked.InternalMessageHandling
            End Try

            If FileNames.Length = 0 Then
                If Not mSourceControl.CanSelectFolders Then Return AfterButtonOpenClicked.RefreshSelection Or AfterButtonOpenClicked.InternalMessageHandling

                FilePath = GetPathFromFolderPath(FilePath, FolderPath)
                FileNames = New String() {FolderPath}
                Dim b As New ButtonOpenClickEventArgs(FolderPath, FilePath, FileNames, False)
                RaiseEvent ButtonOpenClick(Me, b)
                mSourceControl.CancelValue = b.Cancel
                If Not b.Cancel Then
                    MyBase.WndProc(New Message With {.Msg = Msg.WM_COMMAND, .WParam = New IntPtr(2), .LParam = m.LParam, .HWnd = m.HWnd, .Result = IntPtr.Zero})
                    Return AfterButtonOpenClicked.None
                End If
            End If

            FilePath = GetPathFromFolderPath(FilePath, FolderPath)

            Dim Invailds As Char() = Path.GetInvalidPathChars()
            For Each f In FileNames
                For Each c In Invailds
                    If f = "" OrElse f.Contains(c) Then Return AfterButtonOpenClicked.None
                Next
            Next

            If mSourceControl.CheckPathExists Then
                For Each f In FileNames
                    If f = "" Then Return AfterButtonOpenClicked.None
                    If Not Directory.Exists(Path.Combine(FolderPath, Path.GetDirectoryName(f))) Then Return AfterButtonOpenClicked.None
                Next
            End If

            If mSourceControl.CheckFileExists Then
                Select Case mSourceControl.ModeSelection
                    Case ModeSelectionEnum.File
                        For Each f In FileNames
                            If Not File.Exists(Path.Combine(FolderPath, f)) Then Return AfterButtonOpenClicked.None
                        Next
                    Case ModeSelectionEnum.Folder
                        For Each f In FileNames
                            If Not Directory.Exists(Path.Combine(FolderPath, f)) Then Return AfterButtonOpenClicked.None
                        Next
                    Case ModeSelectionEnum.FileWithFolder
                        For Each f In FileNames
                            If Not (File.Exists(Path.Combine(FolderPath, f)) OrElse Directory.Exists(Path.Combine(FolderPath, f))) Then Return AfterButtonOpenClicked.None
                        Next
                End Select
            End If

            Dim e As New ButtonOpenClickEventArgs(FolderPath, FilePath, FileNames, False)
            RaiseEvent ButtonOpenClick(Me, e)
            mSourceControl.CancelValue = e.Cancel
            If Not e.Cancel Then
                MyBase.WndProc(New Message With {.Msg = Msg.WM_COMMAND, .WParam = New IntPtr(2), .LParam = m.LParam, .HWnd = m.HWnd, .Result = IntPtr.Zero})
                Return AfterButtonOpenClicked.None
            End If
            'End If

            Return AfterButtonOpenClicked.RefreshSelection Or AfterButtonOpenClicked.InternalMessageHandling
        End Function

        Protected Overloads Overrides Sub WndProc(ByRef m As Message)
            If m.Msg = Msg.WM_COMMAND Then
                Dim ControlId As Int32 = CInt(m.WParam.ToInt64 And &HFFFFL)
                Dim Notification As Int32 = CInt((m.WParam.ToInt64 >> 16) And &HFFFFL)
                If ControlId = 1 AndAlso Notification = 0 Then 'ButtonOpen, BN_CLICKED
                    'System.Console.WriteLine("{0} : {1}, {2}, {3}", "FileDialogBase", DirectCast(m.Msg, Msg), m.LParam, m.WParam)
                    'System.Console.WriteLine(" {0}, {1}", ControlId, Notification)

                    Dim r = OnButtonOpenClicked(m)
                    Select Case r
                        Case AfterButtonOpenClicked.None
                            Return
                        Case AfterButtonOpenClicked.RefreshSelection Or AfterButtonOpenClicked.InternalMessageHandling
                        Case AfterButtonOpenClicked.InternalMessageHandling
                            MyBase.WndProc(m)
                            Return
                        Case Else
                            Throw New InvalidDataException
                    End Select
                End If
            End If

            Select Case m.Msg
                Case 1324
                    FileDialogEnumWindow()

                    Dim FilePath As String
                    Dim FolderPath As String
                    Dim FileNames As String()
                    Try
                        FilePath = GetFilePath()
                        FolderPath = GetFolderPath()
                        FileNames = GetFileNames()
                        If FileNames.Length = 0 AndAlso FilePath <> FolderPath Then
                            FileNames = New String() {GetFileName(FilePath)}
                        End If
                        FilePath = GetPathFromFolderPath(FilePath, FolderPath)
                    Catch ex As Exception
                        Exit Select
                    End Try

                    FileDialogEnumWindow()

                    If SetIntializeFileNameToNull Then
                        SetEditFileNames(New String() {})
                        SetIntializeFileNameToNull = False
                    ElseIf mSourceControl.MultiselectValue Then
                        SetEditFileNames(FileNames)
                    Else
                        If FileNames.Length > 1 Then
                            SetEditFileNames(New String() {Path.GetFileName(FilePath)})
                        ElseIf FileNames.Length > 0 Then
                            SetEditFileNames(FileNames)
                        End If
                    End If

                    Dim b As New SelectionChangedEventArgs(FolderPath, FilePath, FileNames)
                    RaiseEvent SelectionChangedClick(Me, b)
            End Select
            MyBase.WndProc(m)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Static Disposed As Boolean = False
            If Not Disposed Then
                ReleaseHandle()
                Disposed = True
            End If
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Class
