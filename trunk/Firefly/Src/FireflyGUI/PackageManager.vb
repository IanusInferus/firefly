'==========================================================================
'
'  File:        PackageManager.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: Package文件管理器
'  Version:     2010.03.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.Packaging
Imports Firefly.Setting
Imports Firefly.GUI.ExceptionHandler

Public Class PackageManager
    Inherits System.Windows.Forms.Form

#Region " 窗体代码 "
    <System.Diagnostics.DebuggerStepThrough()> _
    Public Sub New()
        MyBase.New()

        '该调用是 Windows 窗体设计器所必需的。
        InitializeComponent()

        '在 InitializeComponent() 调用之后添加任何初始化

    End Sub

    '窗体重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    Public WithEvents FileListView As System.Windows.Forms.ListView
    Public WithEvents FileName As System.Windows.Forms.ColumnHeader
    Public WithEvents FileLength As System.Windows.Forms.ColumnHeader
    Public WithEvents Offset As System.Windows.Forms.ColumnHeader
    Public WithEvents FileType As System.Windows.Forms.ColumnHeader
    Public WithEvents MainMenu As System.Windows.Forms.MainMenu
    Public WithEvents Menu_File As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_OpenPackage As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_Spliter As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_Exit As System.Windows.Forms.MenuItem
    Public WithEvents Menu_About As System.Windows.Forms.MenuItem
    Public WithEvents Menu_About_About As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_RecentFiles As System.Windows.Forms.MenuItem
    Public WithEvents Path As System.Windows.Forms.TextBox
    Public WithEvents Spliter As System.Windows.Forms.SplitContainer
    Public WithEvents Spliter2 As System.Windows.Forms.SplitContainer
    Public WithEvents Mask As System.Windows.Forms.TextBox
    Public Shadows WithEvents ContextMenu As System.Windows.Forms.ContextMenu
    Public WithEvents ContextMenu_Extract As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_Close As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_ReplacePackage As System.Windows.Forms.MenuItem
    Public WithEvents ContextMenu_CopyLength As System.Windows.Forms.MenuItem
    Public WithEvents ContextMenu_CopyAddress As System.Windows.Forms.MenuItem
    Public WithEvents Menu_File_Create As System.Windows.Forms.MenuItem
    Friend WithEvents Menu_File_Log As System.Windows.Forms.MenuItem
    Public WithEvents ContextMenu_CopyPath As System.Windows.Forms.MenuItem
    <System.Diagnostics.DebuggerStepThrough()> _
    Protected Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim ListViewItem1 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem("")
        Me.FileListView = New System.Windows.Forms.ListView
        Me.FileName = New System.Windows.Forms.ColumnHeader
        Me.FileLength = New System.Windows.Forms.ColumnHeader
        Me.Offset = New System.Windows.Forms.ColumnHeader
        Me.FileType = New System.Windows.Forms.ColumnHeader
        Me.MainMenu = New System.Windows.Forms.MainMenu(Me.components)
        Me.Menu_File = New System.Windows.Forms.MenuItem
        Me.Menu_File_OpenPackage = New System.Windows.Forms.MenuItem
        Me.Menu_File_ReplacePackage = New System.Windows.Forms.MenuItem
        Me.Menu_File_Create = New System.Windows.Forms.MenuItem
        Me.Menu_File_Log = New System.Windows.Forms.MenuItem
        Me.Menu_File_Close = New System.Windows.Forms.MenuItem
        Me.Menu_File_Spliter = New System.Windows.Forms.MenuItem
        Me.Menu_File_RecentFiles = New System.Windows.Forms.MenuItem
        Me.Menu_File_Exit = New System.Windows.Forms.MenuItem
        Me.Menu_About = New System.Windows.Forms.MenuItem
        Me.Menu_About_About = New System.Windows.Forms.MenuItem
        Me.Path = New System.Windows.Forms.TextBox
        Me.Spliter = New System.Windows.Forms.SplitContainer
        Me.Spliter2 = New System.Windows.Forms.SplitContainer
        Me.Mask = New System.Windows.Forms.TextBox
        Me.ContextMenu = New System.Windows.Forms.ContextMenu
        Me.ContextMenu_Extract = New System.Windows.Forms.MenuItem
        Me.ContextMenu_CopyPath = New System.Windows.Forms.MenuItem
        Me.ContextMenu_CopyLength = New System.Windows.Forms.MenuItem
        Me.ContextMenu_CopyAddress = New System.Windows.Forms.MenuItem
        Me.Spliter.Panel1.SuspendLayout()
        Me.Spliter.Panel2.SuspendLayout()
        Me.Spliter.SuspendLayout()
        Me.Spliter2.Panel1.SuspendLayout()
        Me.Spliter2.Panel2.SuspendLayout()
        Me.Spliter2.SuspendLayout()
        Me.SuspendLayout()
        '
        'FileListView
        '
        Me.FileListView.AllowDrop = True
        Me.FileListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.FileName, Me.FileLength, Me.Offset, Me.FileType})
        Me.FileListView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FileListView.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem1})
        Me.FileListView.Location = New System.Drawing.Point(0, 0)
        Me.FileListView.Name = "FileListView"
        Me.FileListView.Size = New System.Drawing.Size(543, 393)
        Me.FileListView.TabIndex = 0
        Me.FileListView.UseCompatibleStateImageBehavior = False
        Me.FileListView.View = System.Windows.Forms.View.Details
        '
        'FileName
        '
        Me.FileName.Text = "文件名"
        Me.FileName.Width = 268
        '
        'FileLength
        '
        Me.FileLength.Text = "文件长度"
        Me.FileLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.FileLength.Width = 98
        '
        'Offset
        '
        Me.Offset.Text = "偏移量"
        Me.Offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.Offset.Width = 76
        '
        'FileType
        '
        Me.FileType.Text = "文件类型"
        Me.FileType.Width = 69
        '
        'MainMenu
        '
        Me.MainMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.Menu_File, Me.Menu_About})
        '
        'Menu_File
        '
        Me.Menu_File.Index = 0
        Me.Menu_File.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.Menu_File_OpenPackage, Me.Menu_File_ReplacePackage, Me.Menu_File_Create, Me.Menu_File_Log, Me.Menu_File_Close, Me.Menu_File_Spliter, Me.Menu_File_RecentFiles, Me.Menu_File_Exit})
        Me.Menu_File.Text = "文件(&F)"
        '
        'Menu_File_OpenPackage
        '
        Me.Menu_File_OpenPackage.Index = 0
        Me.Menu_File_OpenPackage.Text = "打开包文件(&O)..."
        '
        'Menu_File_ReplacePackage
        '
        Me.Menu_File_ReplacePackage.Index = 1
        Me.Menu_File_ReplacePackage.Text = "替换文件(&R)..."
        '
        'Menu_File_Create
        '
        Me.Menu_File_Create.Index = 2
        Me.Menu_File_Create.Text = "创建包文件(&E)..."
        '
        'Menu_File_Log
        '
        Me.Menu_File_Log.Index = 3
        Me.Menu_File_Log.Text = "生成日志(&L)..."
        '
        'Menu_File_Close
        '
        Me.Menu_File_Close.Index = 4
        Me.Menu_File_Close.Text = "关闭(&C)"
        '
        'Menu_File_Spliter
        '
        Me.Menu_File_Spliter.Index = 5
        Me.Menu_File_Spliter.Text = "-"
        '
        'Menu_File_RecentFiles
        '
        Me.Menu_File_RecentFiles.Enabled = False
        Me.Menu_File_RecentFiles.Index = 6
        Me.Menu_File_RecentFiles.Text = "最近的文件(&F)"
        '
        'Menu_File_Exit
        '
        Me.Menu_File_Exit.Index = 7
        Me.Menu_File_Exit.Text = "退出(&X)"
        '
        'Menu_About
        '
        Me.Menu_About.Index = 1
        Me.Menu_About.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.Menu_About_About})
        Me.Menu_About.Text = "关于(&A)"
        '
        'Menu_About_About
        '
        Me.Menu_About_About.Index = 0
        Me.Menu_About_About.Text = "关于(&A)..."
        '
        'Path
        '
        Me.Path.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Path.Location = New System.Drawing.Point(0, 0)
        Me.Path.Name = "Path"
        Me.Path.ReadOnly = True
        Me.Path.Size = New System.Drawing.Size(516, 21)
        Me.Path.TabIndex = 0
        '
        'Spliter
        '
        Me.Spliter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Spliter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.Spliter.IsSplitterFixed = True
        Me.Spliter.Location = New System.Drawing.Point(0, 0)
        Me.Spliter.Name = "Spliter"
        Me.Spliter.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'Spliter.Panel1
        '
        Me.Spliter.Panel1.Controls.Add(Me.Spliter2)
        Me.Spliter.Panel1MinSize = 20
        '
        'Spliter.Panel2
        '
        Me.Spliter.Panel2.Controls.Add(Me.FileListView)
        Me.Spliter.Panel2MinSize = 20
        Me.Spliter.Size = New System.Drawing.Size(543, 417)
        Me.Spliter.SplitterDistance = 22
        Me.Spliter.SplitterWidth = 2
        Me.Spliter.TabIndex = 2
        '
        'Spliter2
        '
        Me.Spliter2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Spliter2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.Spliter2.IsSplitterFixed = True
        Me.Spliter2.Location = New System.Drawing.Point(0, 0)
        Me.Spliter2.Name = "Spliter2"
        '
        'Spliter2.Panel1
        '
        Me.Spliter2.Panel1.Controls.Add(Me.Path)
        '
        'Spliter2.Panel2
        '
        Me.Spliter2.Panel2.Controls.Add(Me.Mask)
        Me.Spliter2.Size = New System.Drawing.Size(543, 22)
        Me.Spliter2.SplitterDistance = 516
        Me.Spliter2.SplitterWidth = 2
        Me.Spliter2.TabIndex = 2
        '
        'Mask
        '
        Me.Mask.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Mask.Location = New System.Drawing.Point(0, 0)
        Me.Mask.Name = "Mask"
        Me.Mask.Size = New System.Drawing.Size(25, 21)
        Me.Mask.TabIndex = 0
        Me.Mask.Text = "*"
        '
        'ContextMenu
        '
        Me.ContextMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.ContextMenu_Extract, Me.ContextMenu_CopyPath, Me.ContextMenu_CopyLength, Me.ContextMenu_CopyAddress})
        '
        'ContextMenu_Extract
        '
        Me.ContextMenu_Extract.Index = 0
        Me.ContextMenu_Extract.Text = "解压(&E)..."
        '
        'ContextMenu_CopyPath
        '
        Me.ContextMenu_CopyPath.Index = 1
        Me.ContextMenu_CopyPath.Text = "复制文件路径(&P)" & Global.Microsoft.VisualBasic.ChrW(9) & "Ctrl+Q"
        '
        'ContextMenu_CopyLength
        '
        Me.ContextMenu_CopyLength.Index = 2
        Me.ContextMenu_CopyLength.Text = "复制文件长度(&L)" & Global.Microsoft.VisualBasic.ChrW(9) & "Ctrl+W"
        '
        'ContextMenu_CopyAddress
        '
        Me.ContextMenu_CopyAddress.Index = 3
        Me.ContextMenu_CopyAddress.Text = "复制偏移量(&A)" & Global.Microsoft.VisualBasic.ChrW(9) & "Ctrl+E"
        '
        'PackageManager
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(543, 417)
        Me.Controls.Add(Me.Spliter)
        Me.Menu = Me.MainMenu
        Me.Name = "PackageManager"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "文件包管理器"
        Me.Spliter.Panel1.ResumeLayout(False)
        Me.Spliter.Panel2.ResumeLayout(False)
        Me.Spliter.ResumeLayout(False)
        Me.Spliter2.Panel1.ResumeLayout(False)
        Me.Spliter2.Panel1.PerformLayout()
        Me.Spliter2.Panel2.ResumeLayout(False)
        Me.Spliter2.Panel2.PerformLayout()
        Me.Spliter2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

#Region " 设置 "
    Protected Opt As INI
    Protected LanFull As String
    Protected Title As String = ExceptionInfo.AssemblyProduct
    Protected Readme As String = "{0}\r\n{1}\r\n".Descape.Formats(ExceptionInfo.AssemblyProduct, ExceptionInfo.AssemblyCopyright)
    Protected INISettingNotice As String = "包文件管理器初始化配置文件" & Environment.NewLine & "在不了解此文件用法的时候请不要编辑此文件。"
    Protected RecentFiles(5) As String
    Public Property ProgramTitle() As String
        Get
            Return Title
        End Get
        Set(ByVal Value As String)
            Title = Value
        End Set
    End Property
    Public Property ProgramReadme() As String
        Get
            Return Readme
        End Get
        Set(ByVal Value As String)
            Readme = Value
        End Set
    End Property
    Public Property ProgramINISettingNotice() As String
        Get
            Return INISettingNotice
        End Get
        Set(ByVal Value As String)
            INISettingNotice = Value
        End Set
    End Property

    Protected Sub PackageManager_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IO.Directory.Exists("..\Ini") Then
            Opt = New Ini(String.Format("..\Ini\{0}.ini", ExceptionInfo.AssemblyProduct))
        Else
            Opt = New Ini(String.Format("{0}.ini", ExceptionInfo.AssemblyProduct))
        End If
        LoadOpt()
        Me.Text = Title

        Dim ImageList As New ImageList()
        ImageList.Images.Add(My.Resources.File)
        ImageList.Images.Add(My.Resources.Directory)
        ImageList.ColorDepth = ColorDepth.Depth32Bit
        FileListView.SmallImageList = ImageList
        FileListView.ContextMenu = ContextMenu

        RefreshRecent()
    End Sub
    Protected Sub PackageManager_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        If Not pfClosed Then pf.Close()
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory
        SaveOpt()
        Opt.WriteToFile("/*" & Environment.NewLine & INISettingNotice & Environment.NewLine & "*/" & Environment.NewLine)
    End Sub

    Protected Sub LoadOpt()
        LanFull = ""
        Opt.ReadValue("Option", "CurrentCulture", LanFull)
        Opt.ReadValue("Option", "Recent0", RecentFiles(0))
        Opt.ReadValue("Option", "Recent1", RecentFiles(1))
        Opt.ReadValue("Option", "Recent2", RecentFiles(2))
        Opt.ReadValue("Option", "Recent3", RecentFiles(3))
        Opt.ReadValue("Option", "Recent4", RecentFiles(4))
        Opt.ReadValue("Option", "Recent5", RecentFiles(5))
    End Sub
    Protected Sub SaveOpt()
        Opt.WriteValue("Option", "CurrentCulture", LanFull)
        Opt.WriteValue("Option", "Recent0", RecentFiles(0))
        Opt.WriteValue("Option", "Recent1", RecentFiles(1))
        Opt.WriteValue("Option", "Recent2", RecentFiles(2))
        Opt.WriteValue("Option", "Recent3", RecentFiles(3))
        Opt.WriteValue("Option", "Recent4", RecentFiles(4))
        Opt.WriteValue("Option", "Recent5", RecentFiles(5))
    End Sub
#End Region

    Protected pf As PackageBase
    Protected pfCurDirDB As FileDB
    Protected pfClosed As Boolean = True
    Protected TempDir As String = IO.Path.GetTempPath & "\$PackageManager"

    Protected Sub PackageManager_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If IO.Directory.Exists(TempDir) Then
            Try
                IO.Directory.Delete(TempDir, True)
            Catch
            End Try
        End If
    End Sub

    Protected Sub RefreshList()
        If pfCurDirDB Is Nothing Then Return
        Dim Item As ListViewItem
        Dim n As Integer = 0
        Dim Sorter As New List(Of ListViewItem)
        Dim FileMask As String = Mask.Text
        For Each f As FileDB In pfCurDirDB.SubFile
            Select Case f.Type
                Case FileDB.FileType.Directory
                    If f.Address > 0 Then
                        Item = New ListViewItem(New String() {f.TitleName, "", f.Address.ToString("X8"), "", n, 0}, 1)
                    Else
                        Item = New ListViewItem(New String() {f.TitleName, "", "", "", n, 0}, 1)
                    End If
                Case Else
                    Item = Nothing
            End Select
            If Item IsNot Nothing Then Sorter.Add(Item)
            n += 1
        Next
        n = 0
        For Each f As FileDB In pfCurDirDB.SubFile
            Select Case f.Type
                Case FileDB.FileType.File
                    If IsMatchFileMask(f.Name, FileMask) Then
                        Item = New ListViewItem(New String() {f.TitleName, f.Length, f.Address.ToString("X8"), GetExtendedFileName(f.Name), n, 1}, 0)
                    Else
                        Item = Nothing
                    End If
                Case Else
                    Item = Nothing
            End Select
            If Item IsNot Nothing Then Sorter.Add(Item)
            n += 1
        Next
        If FileListViewMajorCompareeIndex <> -1 Then Sorter.Sort(AddressOf Comparison)

        With FileListView.Items
            .Clear()
            If pfCurDirDB.ParentFileDB IsNot Nothing Then
                .Add(New ListViewItem(New String() {"..", "", "", "", -1, 0}, 1))
            End If
            .AddRange(Sorter.ToArray)
        End With
    End Sub
    Protected FileListViewMajorCompareeIndex As Integer = -1

    Protected Function Comparison(ByVal x As ListViewItem, ByVal y As ListViewItem) As Integer
        If x.SubItems(5).Text < y.SubItems(5).Text Then Return -1
        If x.SubItems(5).Text > y.SubItems(5).Text Then Return 1

        Select Case FileListViewMajorCompareeIndex
            Case 0, 3
                If x.SubItems(FileListViewMajorCompareeIndex).Text < y.SubItems(FileListViewMajorCompareeIndex).Text Then Return -1
                If x.SubItems(FileListViewMajorCompareeIndex).Text > y.SubItems(FileListViewMajorCompareeIndex).Text Then Return 1
            Case 1, 2
                If CInt(x.SubItems(5).Text) <> 0 Then
                    If x.SubItems(FileListViewMajorCompareeIndex).Text = "" Then
                        If y.SubItems(FileListViewMajorCompareeIndex).Text = "" Then
                            Return 0
                        Else
                            Return -1
                        End If
                    ElseIf y.SubItems(FileListViewMajorCompareeIndex).Text = "" Then
                        Return 1
                    End If
                    If x.SubItems(FileListViewMajorCompareeIndex).Text.Length < y.SubItems(FileListViewMajorCompareeIndex).Text.Length Then Return -1
                    If x.SubItems(FileListViewMajorCompareeIndex).Text.Length > y.SubItems(FileListViewMajorCompareeIndex).Text.Length Then Return 1
                    If x.SubItems(FileListViewMajorCompareeIndex).Text < y.SubItems(FileListViewMajorCompareeIndex).Text Then Return -1
                    If x.SubItems(FileListViewMajorCompareeIndex).Text > y.SubItems(FileListViewMajorCompareeIndex).Text Then Return 1
                End If
        End Select

        If x.SubItems(0).Text < y.SubItems(0).Text Then Return -1
        If x.SubItems(0).Text > y.SubItems(0).Text Then Return 1

        If x.SubItems(2).Text < y.SubItems(2).Text Then Return -1
        If x.SubItems(2).Text > y.SubItems(2).Text Then Return 1
        Return 0
    End Function

    Protected Friend Overridable Sub DoOpenPackage(ByVal TypeIndex As Integer, ByVal PackagePath As String)
        If Not pfClosed Then pf.Close()
        pf = PackageRegister.Open(TypeIndex, PackagePath)
        pfCurDirDB = pf.Root
        Path.Text = pfCurDirDB.Name & "\"
        Path.Text = Path.Text.TrimStart("\")
        RefreshList()
        AddRecent(PackagePath & "," & TypeIndex)
        pfClosed = False
        Me.Text = Title & " - " & PackagePath
    End Sub

    Protected Friend Overridable Sub DoCreatePackage(ByVal WritableTypeIndex As Integer, ByVal PackagePath As String, ByVal Directory As String)
        If Not pfClosed Then pf.Close()
        pfCurDirDB = Nothing
        FileListView.Items.Clear()
        Path.Text = ""
        pfClosed = True
        Application.DoEvents()

        pf = PackageRegister.Create(WritableTypeIndex, PackagePath, Directory)
        pfCurDirDB = pf.Root
        Path.Text = pfCurDirDB.Name & "\"
        Path.Text = Path.Text.TrimStart("\")
        RefreshList()
        pfClosed = False
        Me.Text = Title & " - " & PackagePath
    End Sub

    Protected Friend Overridable Sub DoClose()
        If Not pfClosed Then pf.Close()
        pfCurDirDB = Nothing
        FileListView.Items.Clear()
        Path.Text = ""
        pfClosed = True
        Me.Text = Title
    End Sub

    Protected Sub Menu_File_OpenPackage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_OpenPackage.Click
        Dim Filter As String = PackageRegister.GetFilter
        If Filter = "" Then
            PopupInfo("不存在可以打开的包类型。")
            Return
        End If

        Static d As FilePicker
        If d Is Nothing Then d = New FilePicker(False)
        d.Filter = Filter
        d.ModeSelection = FilePicker.ModeSelectionEnum.File
        d.CheckFileExists = True
        If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
            DoOpenPackage(d.CurrentFilterIndex, d.FilePath)
        End If
    End Sub

    Protected Sub Menu_File_ReplacePackage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_ReplacePackage.Click
        If pf Is Nothing Then
            PopupInfo("当前没有打开的包。")
            Return
        End If

        Static d As FilePicker
        If d Is Nothing Then d = New FilePicker(False)
        d.Filter = "*.*(*.*)|*.*"
        d.ModeSelection = FilePicker.ModeSelectionEnum.FileWithFolder
        d.Multiselect = True
        If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
            If pfClosed Then Throw New InvalidDataException("包文件未打开")
            Dim l As New System.Text.StringBuilder
            Dim Files As New List(Of FileDB)
            Dim Paths As New List(Of String)
            For Each BinPath As String In d.FilePaths
                Try
                    Files.Add(pfCurDirDB.SubFileNameRef(GetFileName(BinPath)))
                    Paths.Add(BinPath)
                Catch ex As Exception
                    l.AppendLine(ex.Message)
                End Try
            Next
            pf.Replace(Files, Paths, Mask.Text)
            If l.Length <> 0 Then
                Throw New Exception(l.ToString)
            Else
                PopupInfo("完成")
            End If
            RefreshList()
        End If
    End Sub

    Protected Sub Menu_File_Create_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_Create.Click
        Dim WritableFilter As String = PackageRegister.GetWritableFilter
        If WritableFilter = "" Then
            PopupInfo("不存在可以创建的包类型。")
            Return
        End If

        Static d2 As FilePicker
        If d2 Is Nothing Then d2 = New FilePicker(True)
        d2.Filter = WritableFilter
        d2.ModeSelection = FilePicker.ModeSelectionEnum.File
        If d2.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Static d As FilePicker
            If d Is Nothing Then
                d = New FilePicker(False)
                d.ModeSelection = FilePicker.ModeSelectionEnum.Folder
            End If
            If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
                DoCreatePackage(d2.CurrentFilterIndex, d2.FilePath, d.FilePath)
            End If
        End If
    End Sub

    Protected Sub Menu_File_Log_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_Log.Click
        If pfClosed Then
            PopupInfo("没有打开的文件。")
            Return
        End If

        Static d As FilePicker
        If d Is Nothing Then d = New FilePicker(True)
        d.Filter = "文本文件(*.txt)|*.txt"
        d.ModeSelection = FilePicker.ModeSelectionEnum.File
        If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim sl As New List(Of String)
            Dim Count = 20
            For Each i As ListViewItem In FileListView.Items
                Count = Max(Count, i.SubItems(0).Text.Length)
            Next
            For Each i As ListViewItem In FileListView.Items
                sl.Add(("{0, -" & Count & "} {1,10} {2,10} {3,4}").Formats(i.SubItems(0).Text, i.SubItems(1).Text, i.SubItems(2).Text, i.SubItems(3).Text))
            Next
            Texting.Txt.WriteFile(d.FilePath, TextEncoding.UTF16, String.Join(System.Environment.NewLine, sl.ToArray))
        End If
    End Sub

    Protected Sub Menu_File_Close_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_Close.Click
        DoClose()
    End Sub

    Protected Sub Menu_File_Exit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_File_Exit.Click
        Me.Close()
    End Sub

    Protected d As FilePicker

    Protected Sub FileListView_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles FileListView.ItemActivate
        If pfClosed Then Return
        Dim FocusedItem As ListViewItem = FileListView.FocusedItem
        If FocusedItem.SubItems.Count < 6 Then Return
        Dim n As Integer = FocusedItem.SubItems(4).Text
        If FocusedItem.SubItems(5).Text = 1 Then
            If d Is Nothing Then
                d = New FilePicker(True)
                d.ModeSelection = FilePicker.ModeSelectionEnum.Folder
            End If
            If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
                Dim Files As New List(Of FileDB)
                Dim Paths As New List(Of String)
                For Each Item As ListViewItem In FileListView.SelectedItems
                    Dim k As Integer = Item.SubItems(4).Text
                    If k < 0 Then Continue For
                    Files.Add(pfCurDirDB.SubFile(k))
                    Paths.Add(GetPath(d.FilePath, pfCurDirDB.SubFile(k).Name))
                Next
                pf.Extract(Files, Paths, Mask.Text)
            End If
        Else
            If n < 0 Then
                pfCurDirDB = pfCurDirDB.ParentFileDB
                Path.Text = Path.Text.Substring(0, Path.Text.Length - 1)
                If Path.Text.Contains("\") Then
                    Path.Text = Path.Text.Substring(0, Path.Text.LastIndexOf("\")) & "\"
                Else
                    Path.Text = ""
                End If
            Else
                pfCurDirDB = pfCurDirDB.SubFile(n)
                Path.Text = (GetPath(Path.Text, pfCurDirDB.Name) & "\")
            End If
            Path.Text = Path.Text.TrimStart("\")
            RefreshList()
        End If
    End Sub

    Protected Sub ContextMenu_Extract_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContextMenu_Extract.Click
        If pfClosed Then Return
        Dim FocusedItem As ListViewItem = FileListView.FocusedItem
        If FocusedItem.SubItems.Count < 6 Then Return
        Dim n As Integer = FocusedItem.SubItems(4).Text
        If d Is Nothing Then
            d = New FilePicker(True)
            d.ModeSelection = FilePicker.ModeSelectionEnum.Folder
        End If
        If d.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim Files As New List(Of FileDB)
            Dim Paths As New List(Of String)
            For Each Item As ListViewItem In FileListView.SelectedItems
                Dim k As Integer = Item.SubItems(4).Text
                If k < 0 Then Continue For
                Files.Add(pfCurDirDB.SubFile(k))
                Paths.Add(GetPath(d.FilePath, pfCurDirDB.SubFile(k).Name))
            Next
            pf.Extract(Files, Paths, Mask.Text)
        End If
    End Sub
    Protected Sub ContextMenu_CopyPath_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContextMenu_CopyPath.Click
        If pfClosed Then Return
        Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
        If Focused.Count >= 4 Then
            Dim n As Integer = Focused(4).Text
            If n < 0 Then Return
            Dim c = pfCurDirDB.SubFile(n)
            Dim Path As String = c.Name
            While True
                c = c.ParentFileDB
                If c Is Nothing Then Exit While
                Path = GetPath(c.Name, Path)
            End While
            Clipboard.SetText(Path)
        End If
    End Sub
    Protected Sub ContextMenu_CopyLength_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContextMenu_CopyLength.Click
        If pfClosed Then Return
        Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
        If Focused.Count >= 1 Then Clipboard.SetText(Focused(1).Text)
    End Sub
    Protected Sub ContextMenu_CopyPosition_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContextMenu_CopyAddress.Click
        If pfClosed Then Return
        Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
        If Focused.Count >= 2 Then Clipboard.SetText(Focused(2).Text)
    End Sub

    Protected Sub FileListView_ItemDrag(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemDragEventArgs) Handles FileListView.ItemDrag
        Dim Data As New DataObject

        Dim TempList As New Collections.Specialized.StringCollection
        Dim Files As New List(Of FileDB)
        Dim Paths As New List(Of String)
        For Each Item As ListViewItem In FileListView.SelectedItems
            If Item.SubItems.Count < 4 Then Continue For
            Dim n As Integer = Item.SubItems(4).Text
            If n < 0 Then Continue For
            Files.Add(pfCurDirDB.SubFile(n))
            Paths.Add(GetPath(TempDir, pfCurDirDB.SubFile(n).Name))
            TempList.Add(TempDir & "\" & pfCurDirDB.SubFile(n).Name)
        Next
        pf.Extract(Files, Paths, Mask.Text)
        Data.SetFileDropList(TempList)

        FileListView.DoDragDrop(Data, DragDropEffects.Move)
    End Sub

    Private Sub FileListView_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles FileListView.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub FileListView_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles FileListView.DragDrop
        Try
            If pf Is Nothing Then
                PopupInfo("当前没有打开的包。")
                Return
            End If
            Dim Names As New Dictionary(Of String, Integer)
            Dim l As New System.Text.StringBuilder
            Dim Files As New List(Of FileDB)
            Dim Paths As New List(Of String)
            For Each File As String In e.Data.GetData(DataFormats.FileDrop)
                Try
                    Dim FileDB = pfCurDirDB.SubFileNameRef(GetFileName(File))
                    Files.Add(FileDB)
                    Paths.Add(File)
                    Names.Add(FileDB.Name, 0)
                Catch ex As Exception
                    l.AppendLine(ex.Message)
                End Try
            Next
            pf.Replace(Files, Paths, Mask.Text)
            If l.Length <> 0 Then
                PopupException(New Exception(l.ToString))
            End If
            RefreshList()
            FileListView.Focus()
            For Each Item As ListViewItem In FileListView.Items
                If Item.SubItems.Count < 4 Then Continue For
                If Names.ContainsKey(Item.SubItems(0).Text) Then
                    Item.Selected = True
                    Item.Focused = True
                End If
            Next
        Catch ex As Exception
            PopupException(ex)
        End Try
    End Sub

    Protected Sub Mask_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Mask.TextChanged
        If pfClosed Then Return
        Dim Tick As Integer = Environment.TickCount
        While Environment.TickCount - Tick < 2000
            Application.DoEvents()
        End While
        RefreshList()
    End Sub

    Protected Sub Menu_About_About_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Menu_About_About.Click
        MessageBox.Show(Readme, Title)
    End Sub

    Protected Sub AddRecent(ByVal s As String)
        For n As Integer = 0 To 5
            If RecentFiles(n) = s Then
                For m As Integer = n To 4
                    RecentFiles(m) = RecentFiles(m + 1)
                Next
                RecentFiles(5) = Nothing
                Exit For
            End If
        Next
        For n As Integer = 4 To 0 Step -1
            RecentFiles(n + 1) = RecentFiles(n)
        Next
        RecentFiles(0) = s
        RefreshRecent()
    End Sub
    Protected Sub RemoveRecent(ByVal Index As Integer)
        RecentFiles(Index) = Nothing
        For m As Integer = Index To 4
            RecentFiles(m) = RecentFiles(m + 1)
        Next
        RecentFiles(5) = Nothing
        RefreshRecent()
    End Sub
    Protected Sub RefreshRecent()
        Dim c As Menu.MenuItemCollection = Menu_File_RecentFiles.MenuItems
        c.Clear()
        For n As Integer = 0 To 5
            If RecentFiles(n) = Nothing Then
                Exit For
            End If
            Dim i As New MenuItem("&" & n & " " & RecentFiles(n))
            c.Add(i)
            AddHandler i.Click, AddressOf RecentFilesHandler
        Next
        Menu_File_RecentFiles.Enabled = (c.Count <> 0)
    End Sub
    Protected Sub RecentFilesHandler(ByVal sender As Object, ByVal e As EventArgs)
        Dim Success = False
        Dim r As MenuItem = CType(sender, MenuItem)
        Try
            Dim Path As String = r.Text.Substring(3)
            If Not r.Text.Contains(",") Then RemoveRecent(r.Text.Substring(1, 1))
            Dim TypeIndex As Integer = 0
            If Path.Contains(",") Then
                TypeIndex = Path.Substring(Path.LastIndexOf(",") + 1)
                Path = Path.Substring(0, Path.LastIndexOf(","))
            End If
            If Not pfClosed Then pf.Close()
            pf = PackageRegister.Open(TypeIndex, Path)
            pfCurDirDB = pf.Root
            Me.Path.Text = pfCurDirDB.Name & "\"
            Me.Path.Text = Me.Path.Text.TrimStart("\")
            RefreshList()
            AddRecent(Path & "," & TypeIndex)
            pfClosed = False
            Me.Text = Title & " - " & Path
            Success = True
        Finally
            If Not Success Then
                If Not System.Diagnostics.Debugger.IsAttached Then RemoveRecent(r.Text.Substring(1, 1))
            End If
        End Try
    End Sub

    Protected Sub FileListView_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles FileListView.KeyUp
        Select Case e.KeyData
            Case Keys.Back
                If pfClosed Then Return
                If pfCurDirDB.ParentFileDB Is Nothing Then Return
                pfCurDirDB = pfCurDirDB.ParentFileDB
                Path.Text = Path.Text.Substring(0, Path.Text.Length - 1)
                If Path.Text.Contains("\") Then
                    Path.Text = Path.Text.Substring(0, Path.Text.LastIndexOf("\")) & "\"
                Else
                    Path.Text = ""
                End If
                Path.Text = Path.Text.TrimStart("\")
                RefreshList()
            Case Keys.Control Or Keys.A
                FileListView.BeginUpdate()
                For Each Item As ListViewItem In FileListView.Items
                    Item.Selected = True
                Next
                If FileListView.Items(0).SubItems(0).Text = ".." Then FileListView.Items(0).Selected = False
                FileListView.EndUpdate()
            Case Keys.Control Or Keys.Q
                If pfClosed Then Return
                Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
                If Focused.Count >= 4 Then
                    Dim n As Integer = Focused(4).Text
                    If n < 0 Then Return
                    Dim c = pfCurDirDB.SubFile(n)
                    Dim Path As String = c.Name
                    While True
                        c = c.ParentFileDB
                        If c Is Nothing Then Exit While
                        Path = GetPath(c.Name, Path)
                    End While
                    Clipboard.SetText(Path)
                End If
            Case Keys.Control Or Keys.W
                If pfClosed Then Return
                Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
                If Focused.Count >= 1 Then Clipboard.SetText(Focused(1).Text)
            Case Keys.Control Or Keys.E
                If pfClosed Then Return
                Dim Focused As ListViewItem.ListViewSubItemCollection = FileListView.FocusedItem.SubItems
                If Focused.Count >= 2 Then Clipboard.SetText(Focused(2).Text)
        End Select
    End Sub

    Protected Sub FileListView_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles FileListView.ColumnClick
        FileListViewMajorCompareeIndex = e.Column
        RefreshList()
    End Sub
End Class
