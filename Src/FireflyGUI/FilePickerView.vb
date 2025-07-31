'==========================================================================
'
'  File:        FilePickerView.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 文件选取对话框 - 界面
'  Version:     2025.07.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

Public Class FilePicker
    Private Class Item
        Public Name As String
        Public Type As Integer
    End Class

    Private DirectoryList As New List(Of String)
    Private Sub RefreshDirectoryList()
        Dim DirectoryText = ComboBox_Directory.Text
        DirectoryList.Clear()
        ComboBox_Directory.Items.Clear()
        DirectoryList.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
        If Threading.Thread.CurrentThread.CurrentCulture.Name = "zh-CN" Then
            ComboBox_Directory.Items.Add("桌面")
        Else
            ComboBox_Directory.Items.Add("Desktop")
        End If
        For Each DrivePath In Environment.GetLogicalDrives

            DirectoryList.Add(DrivePath)
            Dim VolumeLabel = ""
            Try
                Dim d As New DriveInfo(DrivePath)
                If d.IsReady Then
                    VolumeLabel = d.VolumeLabel
                End If
            Catch
            End Try
            If VolumeLabel <> "" Then
                ComboBox_Directory.Items.Add("{0} ({1})".Formats(GetDirectoryPathWithoutTailingSeparator(DrivePath), VolumeLabel))
            Else
                ComboBox_Directory.Items.Add(GetDirectoryPathWithoutTailingSeparator(DrivePath))
            End If
        Next
        ComboBox_Directory.Text = DirectoryText
    End Sub

    Private Sub ComboBox_Directory_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox_Directory.SelectedIndexChanged
        ComboBox_Directory.Text = DirectoryList(ComboBox_Directory.SelectedIndex)
        CurrentDirectory = ComboBox_Directory.Text
    End Sub

    Private ImageList As New ImageList()
    Private IconDict As New Dictionary(Of Integer, Icon)
    Private Sorter As New List(Of ListViewItem)
    Private Sub FillItem(ByVal i As Integer)
        Dim Item = Sorter(i)
        If IconDict.ContainsKey(i) Then Return

        Dim f = GetAbsolutePath(Item.SubItems(0).Text, CurrentDirectory)
        Dim Icon As Icon = Nothing
        Icon = FilePickerInterop.GetAssociatedIcon(f, False)
        If Icon IsNot Nothing Then
            Dim Index As Integer
            ImageList.Images.Add(Icon)
            Index = IconDict.Count
            IconDict.Add(i, Icon)
            Item.ImageIndex = Index
        End If
        Dim TypeName = FilePickerInterop.GetTypeName(f)
        Item.SubItems(2).Text = TypeName
    End Sub

    Private Sub RefreshList()
        If Not Directory.Exists(CurrentDirectory) Then Return
        For Each p In IconDict
            p.Value.Dispose()
        Next
        ImageList = New ImageList()
        IconDict = New Dictionary(Of Integer, Icon)
        ImageList.ColorDepth = ColorDepth.Depth32Bit
        FileListView.SmallImageList = ImageList

        FileListView.VirtualListSize = 0

        Dim RootItems = New Item() {New Item With {.Name = "..", .Type = -1}}
        Dim DirectoryItems = Directory.GetDirectories(CurrentDirectory).OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase).Select(Function(f) New Item With {.Name = f, .Type = 0})
        Dim FileMasks = New String() {}
        If FilterIndexValue >= 0 AndAlso FilterIndexValue < FilterValues.Count Then
            FileMasks = FilterValues(FilterIndexValue).Value
        End If
        Dim FileNamePredicate = Function(FileName As String) FileMasks.Select(Function(m) Function(f As String) IsMatchFileMask(f, m)).Any(Function(p) p(FileName))
        Dim FileItems = Directory.GetFiles(CurrentDirectory).Where(Function(FilePath) FileNamePredicate(GetFileName(FilePath))).OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase).Select(Function(f) New Item With {.Name = f, .Type = 1})
        Dim Items = RootItems.Concat(DirectoryItems).Concat(FileItems).ToArray

        Dim n As Integer = 0
        Sorter = New List(Of ListViewItem)
        For Each p In Items
            Dim f = GetAbsolutePath(p.Name, CurrentDirectory)
            If f = "" Then Continue For
            Dim fi As New FileInfo(f)
            Dim TypeName = ""
            If Directory.Exists(f) Then
                Dim Item = New ListViewItem(New String() {GetFileName(p.Name), "", TypeName, fi.LastWriteTime, fi.CreationTime, n, p.Type})
                Sorter.Add(Item)
            Else
                Dim Item = New ListViewItem(New String() {GetFileName(p.Name), fi.Length, TypeName, fi.LastWriteTime, fi.CreationTime, n, p.Type})
                Sorter.Add(Item)
            End If
            n += 1
        Next
        If FileListViewMajorCompareeIndex <> -1 Then Sorter.Sort(AddressOf Comparison)

        FileListView.VirtualListSize = Sorter.Count
    End Sub

    Private Sub FileListView_RetrieveVirtualItem(ByVal sender As Object, ByVal e As System.Windows.Forms.RetrieveVirtualItemEventArgs) Handles FileListView.RetrieveVirtualItem
        FillItem(e.ItemIndex)
        e.Item = Sorter(e.ItemIndex)
    End Sub

    Private FileListViewMajorCompareeIndex As Integer = -1

    Private Function Comparison(ByVal x As ListViewItem, ByVal y As ListViewItem) As Integer
        Dim OrderSeq As New List(Of Integer)
        OrderSeq.Add(FileListView.Columns.Count + 1)
        Dim r = Enumerable.Range(0, FileListView.Columns.Count)
        If r.Contains(FileListViewMajorCompareeIndex) Then
            OrderSeq.Add(FileListViewMajorCompareeIndex)
            OrderSeq.AddRange(r.Except(New Integer() {FileListViewMajorCompareeIndex}))
        Else
            OrderSeq.AddRange(r)
        End If
        OrderSeq.Add(FileListView.Columns.Count)

        For Each c In OrderSeq
            Select Case c
                Case 1
                    If x.SubItems(c).Text.Length < y.SubItems(c).Text.Length Then Return -1
                    If x.SubItems(c).Text.Length > y.SubItems(c).Text.Length Then Return 1
            End Select
            If x.SubItems(c).Text < y.SubItems(c).Text Then Return -1
            If x.SubItems(c).Text > y.SubItems(c).Text Then Return 1
        Next
        Return 0
    End Function

    Private Sub FileListView_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles FileListView.ColumnClick
        FileListViewMajorCompareeIndex = e.Column
        RefreshList()
    End Sub

    Private Sub RefreshFilterList()
        ComboBox_Filter.Items.Clear()
        If FilterValues.Count > 0 Then
            ComboBox_Filter.Items.AddRange((From f In FilterValues Select f.Key).ToArray)
            If FilterIndexValue >= 0 AndAlso FilterIndexValue < FilterValues.Count Then
                ComboBox_Filter.SelectedIndex = FilterIndexValue
            End If
        End If
    End Sub

    Private Sub ComboBox_Filter_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox_Filter.SelectedIndexChanged
        If ComboBox_Filter.SelectedIndex <> FilterIndexValue Then
            FilterIndexValue = ComboBox_Filter.SelectedIndex
            RefreshList()
        End If
    End Sub

    Public Sub New(Optional ByVal IsSaveDialog As Boolean = False)
        ' 此调用是 Windows 窗体设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        If Threading.Thread.CurrentThread.CurrentCulture.Name = "zh-CN" Then
            Me.Button_Select.Text = "选定(&S)"
            Me.Button_Cancel.Text = "取消"
            Me.Button_Enter.Text = "进入(&E)"
            Me.ColumnHeader_Name.Text = "名称"
            Me.ColumnHeader_Length.Text = "大小"
            Me.ColumnHeader_Type.Text = "类型"
            Me.ColumnHeader_ModifyTime.Text = "修改时间"
            Me.ColumnHeader_CreateTime.Text = "创建时间"
            Me.Label_FileName.Text = "文件名(&N):"
            Me.Label_Filter.Text = "文件类型(&T):"
            Me.Label_Directory.Text = "查找范围(&I):"
        Else
            Me.Button_Select.Text = "&Select"
            Me.Button_Cancel.Text = "Cancel"
            Me.Button_Enter.Text = "&Enter"
            Me.ColumnHeader_Name.Text = "Name"
            Me.ColumnHeader_Length.Text = "Length"
            Me.ColumnHeader_Type.Text = "Type"
            Me.ColumnHeader_ModifyTime.Text = "Modify Time"
            Me.ColumnHeader_CreateTime.Text = "Create Time"
            Me.Label_FileName.Text = "File &Name:"
            Me.Label_Filter.Text = "File &Type:"
            Me.Label_Directory.Text = "&In:"
        End If

        'FileListView.ContextMenu = ContextMenu

        InitialDirectory = System.Environment.CurrentDirectory
        CurrentDirectory = InitialDirectory
        If Threading.Thread.CurrentThread.CurrentCulture.Name = "zh-CN" Then
            Filter = "所有文件(*.*)|*.*"
        Else
            Filter = "All files(*.*)|*.*"
        End If
        Me.IsSaveDialog = IsSaveDialog
        ModeSelection = ModeSelectionEnum.File
        Multiselect = False
        CheckFileExists = True
        CheckPathExists = True
        ValidateNames = True
    End Sub

    Private Sub FilePicker_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        RefreshDirectoryList()
        RefreshFilterList()
        RefreshList()
    End Sub

    Private Sub FilePicker_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        LastSourceControl = ComboBox_FileName
        ComboBox_FileName.Focus()
    End Sub

    Public Shadows Sub Hide()
        Me.Owner = Nothing
        MyBase.Hide()
    End Sub

    Private Sub HideSelf()
        Me.Hide()
        If Me.Parent IsNot Nothing Then Me.Parent.Focus()
    End Sub

    Private LastSourceControl As Control

    Private Sub Button_Enter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Enter.Click
        If LastSourceControl Is ComboBox_Directory Then
            CurrentDirectory = ComboBox_Directory.Text
        ElseIf LastSourceControl Is ComboBox_FileName Then
            CurrentDirectory = ComboBox_FileName.Text
        ElseIf LastSourceControl Is FileListView Then
            Dim FocusedItem As ListViewItem = FileListView.FocusedItem
            If FocusedItem Is Nothing Then Return
            If FocusedItem.SubItems.Count <> FileListView.Columns.Count + 2 Then Return
            Select Case CInt(FocusedItem.SubItems(FileListView.Columns.Count + 1).Text)
                Case -1
                    CurrentDirectory = GetFileDirectory(GetDirectoryPathWithoutTailingSeparator(CurrentDirectory))
                Case 0
                    CurrentDirectory = GetAbsolutePath(FocusedItem.SubItems(0).Text, CurrentDirectory)
            End Select
        End If
    End Sub

    Private Function ExistNode(ByVal Path As String) As Boolean
        If ModeSelection And ModeSelectionEnum.File Then
            If File.Exists(Path) Then Return True
        End If
        If ModeSelection And ModeSelectionEnum.Folder Then
            If Directory.Exists(Path) Then Return True
        End If
        Return False
    End Function

    Private Function CheckComboBox_FileName(ByVal PopCheckFileExistBox As Boolean) As Boolean
        Dim Path = GetAbsolutePath(ComboBox_FileName.Text, CurrentDirectory)
        If CheckPathExists Then
            If Not Directory.Exists(GetFileDirectory(Path)) Then Return False
        End If
        If CheckFileExists Then
            If IsSaveDialog Then
                If ExistNode(Path) Then
                    If PopCheckFileExistBox Then
                        Dim dr = MessageBox.Show(If(Threading.Thread.CurrentThread.CurrentCulture.Name = "zh-CN", "{0} 已存在。\r\n要替换吗？", "{0} exists.\r\nDo you want to overwrite?").Descape.Formats(Path), If(Threading.Thread.CurrentThread.CurrentCulture.Name = "zh-CN", "确认另存为", "Confirm Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                        If dr <> System.Windows.Forms.DialogResult.Yes Then Return False
                    End If
                Else
                    If ValidateNames Then
                        For Each c In IO.Path.GetInvalidPathChars
                            If Path.Contains(c) Then Return False
                        Next
                    End If
                End If
            Else
                If Not ExistNode(Path) Then Return False
            End If
        End If
        Return True
    End Function

    Private FileNameHistorySet As New HashSet(Of String)
    Private Sub Button_Select_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Select.Click
        If LastSourceControl Is ComboBox_FileName Then
            If CheckComboBox_FileName(True) Then
                If Not FileNameHistorySet.Contains(ComboBox_FileName.Text) Then
                    FileNameHistorySet.Add(ComboBox_FileName.Text)
                    ComboBox_Directory.Items.Add(ComboBox_FileName.Text)
                End If
                Dim Path = GetAbsolutePath(ComboBox_FileName.Text, CurrentDirectory)
                FilePathValue = Path
                FileNamesValue = New String() {GetRelativePath(Path, CurrentDirectory)}
                DialogResult = System.Windows.Forms.DialogResult.OK
                Me.HideSelf()
            End If
        ElseIf LastSourceControl Is FileListView Then
            Dim l As New List(Of String)
            Dim f As String = ""

            For Each r As ListViewItem In Sorter
                If Not r.Selected Then Continue For
                If r.SubItems.Count = FileListView.Columns.Count + 2 Then
                    Select Case CInt(r.SubItems(FileListView.Columns.Count + 1).Text)
                        Case -1
                        Case 0
                            If ModeSelection And ModeSelectionEnum.File Then
                                l.Add(r.SubItems(0).Text)
                            End If
                        Case Else
                            If ModeSelection And ModeSelectionEnum.Folder Then
                                l.Add(r.SubItems(0).Text)
                            End If
                    End Select
                End If
                If r.Focused Then
                    f = r.SubItems(0).Text
                End If
            Next
            If f = "" AndAlso l.Count > 0 Then f = l(0)
            FilePathValue = GetAbsolutePath(f, CurrentDirectory)
            FileNamesValue = l.ToArray
            DialogResult = System.Windows.Forms.DialogResult.OK
            Me.HideSelf()
        End If
    End Sub

    Private Sub ProcessReturn(ByVal sender As Object, ByVal e As System.EventArgs)
        If ComboBox_Directory.Focused AndAlso Button_Enter.Enabled Then
            Button_Enter_Click(sender, e)
        ElseIf ComboBox_FileName.Focused AndAlso Button_Select.Enabled Then
            Button_Select_Click(sender, e)
        ElseIf ComboBox_FileName.Focused AndAlso Button_Enter.Enabled Then
            Button_Enter_Click(sender, e)
        ElseIf FileListView.Focused AndAlso Button_Enter.Enabled Then
            Button_Enter_Click(sender, e)
        ElseIf FileListView.Focused AndAlso Button_Select.Enabled Then
            Button_Select_Click(sender, e)
        End If
    End Sub

    Private Sub FileListView_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles FileListView.Enter
        LastSourceControl = FileListView
    End Sub

    Private Sub FileListView_ItemActivate(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FileListView.ItemActivate
        LastSourceControl = FileListView
        ProcessReturn(sender, e)
    End Sub

    Private Sub ComboBox_Directory_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox_Directory.Enter
        LastSourceControl = ComboBox_Directory
        Button_Enter.Enabled = True
    End Sub

    Private Sub ComboBox_FileName_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox_FileName.Enter
        LastSourceControl = ComboBox_FileName
        ComboBox_FileName_TextChanged(sender, e)
    End Sub

    Private Sub FileListView_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles FileListView.KeyDown
        Select Case e.KeyData
            Case Keys.Control Or Keys.A
                If Multiselect Then
                    For Each r As ListViewItem In Sorter
                        If r.SubItems.Count = FileListView.Columns.Count + 2 Then
                            If CInt(r.SubItems(FileListView.Columns.Count + 1).Text) = -1 Then
                                r.Selected = False
                            Else
                                r.Selected = True
                            End If
                        End If
                    Next
                End If
            Case Keys.Back
                CurrentDirectory = GetFileDirectory(GetDirectoryPathWithoutTailingSeparator(CurrentDirectory))
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub

    Private Sub FileListView_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FileListView.SelectedIndexChanged
        Timer.Stop()
        Dim FocusedItem As ListViewItem = FileListView.FocusedItem
        If FocusedItem Is Nothing Then Return
        If FocusedItem.SubItems.Count <> FileListView.Columns.Count + 2 Then Return
        Select Case CInt(FocusedItem.SubItems(FileListView.Columns.Count + 1).Text)
            Case -1, 0
                Button_Enter.Enabled = True
            Case Else
                Button_Enter.Enabled = False
        End Select
        Dim Exist As Boolean = False
        For Each r As ListViewItem In Sorter
            If Not r.Selected Then Continue For
            If r.SubItems.Count = FileListView.Columns.Count + 2 Then
                Exist = True
                Select Case CInt(r.SubItems(FileListView.Columns.Count + 1).Text)
                    Case -1
                        Button_Select.Enabled = False
                        Return
                    Case 0
                        If ModeSelection = ModeSelectionEnum.File Then
                            Button_Select.Enabled = False
                            Return
                        End If
                    Case Else
                        If ModeSelection = ModeSelectionEnum.Folder Then
                            Button_Select.Enabled = False
                            Return
                        End If
                End Select
            End If
        Next
        Button_Select.Enabled = Exist
    End Sub

    Private Sub ComboBox_FileName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox_FileName.TextChanged
        Box_TextChanged(sender, e)
    End Sub

    Private WithEvents Timer As New Timer
    Friend IMECompositing As Integer = 0
    Private Block As Integer = 0
    Private Sub Box_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer.Tick
        If System.Threading.Interlocked.CompareExchange(IMECompositing, -1, -1) Then Return
        System.Threading.Interlocked.Exchange(Block, -1)
        Timer.Stop()
        If ComboBox_FileName.Focused Then
            Timer.Interval = 500
            Timer.Start()
        End If
        Button_Select.Enabled = CheckComboBox_FileName(False)
        System.Threading.Interlocked.Exchange(Block, 0)
    End Sub
    Private Sub Box_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        If System.Threading.Interlocked.CompareExchange(Block, -1, -1) Then Return
        Timer.Stop()
        Timer.Interval = 500
        Timer.Start()
    End Sub

    Private Sub FilePicker_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyData
            Case Keys.Return
                ProcessReturn(sender, e)
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub

    Private Sub Button_Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Cancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.HideSelf()
    End Sub
End Class
