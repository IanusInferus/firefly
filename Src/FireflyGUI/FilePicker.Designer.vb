<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FilePicker
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Button_Select = New System.Windows.Forms.Button
        Me.Button_Cancel = New System.Windows.Forms.Button
        Me.Button_Enter = New System.Windows.Forms.Button
        Me.FileListView = New System.Windows.Forms.ListView
        Me.ColumnHeader_Name = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader_Length = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader_Type = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader_ModifyTime = New System.Windows.Forms.ColumnHeader
        Me.ColumnHeader_CreateTime = New System.Windows.Forms.ColumnHeader
        Me.ComboBox_FileName = New System.Windows.Forms.ComboBox
        Me.Label_FileName = New System.Windows.Forms.Label
        Me.ComboBox_Filter = New System.Windows.Forms.ComboBox
        Me.Label_Filter = New System.Windows.Forms.Label
        Me.ComboBox_Directory = New System.Windows.Forms.ComboBox
        Me.Label_Directory = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'Button_Select
        '
        Me.Button_Select.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Select.Location = New System.Drawing.Point(415, 216)
        Me.Button_Select.Name = "Button_Select"
        Me.Button_Select.Size = New System.Drawing.Size(67, 21)
        Me.Button_Select.TabIndex = 6
        Me.Button_Select.Text = "选定(&S)"
        '
        'Button_Cancel
        '
        Me.Button_Cancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button_Cancel.Location = New System.Drawing.Point(415, 242)
        Me.Button_Cancel.Name = "Button_Cancel"
        Me.Button_Cancel.Size = New System.Drawing.Size(67, 21)
        Me.Button_Cancel.TabIndex = 9
        Me.Button_Cancel.Text = "取消"
        '
        'Button_Enter
        '
        Me.Button_Enter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Enter.Location = New System.Drawing.Point(415, 11)
        Me.Button_Enter.Name = "Button_Enter"
        Me.Button_Enter.Size = New System.Drawing.Size(67, 21)
        Me.Button_Enter.TabIndex = 2
        Me.Button_Enter.Text = "进入(&E)"
        '
        'FileListView
        '
        Me.FileListView.AllowColumnReorder = True
        Me.FileListView.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FileListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader_Name, Me.ColumnHeader_Length, Me.ColumnHeader_Type, Me.ColumnHeader_ModifyTime, Me.ColumnHeader_CreateTime})
        Me.FileListView.FullRowSelect = True
        Me.FileListView.Location = New System.Drawing.Point(14, 38)
        Me.FileListView.Name = "FileListView"
        Me.FileListView.ShowGroups = False
        Me.FileListView.Size = New System.Drawing.Size(468, 171)
        Me.FileListView.TabIndex = 3
        Me.FileListView.UseCompatibleStateImageBehavior = False
        Me.FileListView.View = System.Windows.Forms.View.Details
        Me.FileListView.VirtualMode = True
        '
        'ColumnHeader_Name
        '
        Me.ColumnHeader_Name.Text = "名称"
        Me.ColumnHeader_Name.Width = 300
        '
        'ColumnHeader_Length
        '
        Me.ColumnHeader_Length.Text = "大小"
        Me.ColumnHeader_Length.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'ColumnHeader_Type
        '
        Me.ColumnHeader_Type.Text = "类型"
        Me.ColumnHeader_Type.Width = 110
        '
        'ColumnHeader_ModifyTime
        '
        Me.ColumnHeader_ModifyTime.Text = "修改时间"
        Me.ColumnHeader_ModifyTime.Width = 110
        '
        'ColumnHeader_CreateTime
        '
        Me.ColumnHeader_CreateTime.Text = "创建时间"
        Me.ColumnHeader_CreateTime.Width = 110
        '
        'ComboBox_FileName
        '
        Me.ComboBox_FileName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_FileName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.ComboBox_FileName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem
        Me.ComboBox_FileName.FormattingEnabled = True
        Me.ComboBox_FileName.Location = New System.Drawing.Point(112, 216)
        Me.ComboBox_FileName.Name = "ComboBox_FileName"
        Me.ComboBox_FileName.Size = New System.Drawing.Size(297, 20)
        Me.ComboBox_FileName.TabIndex = 5
        '
        'Label_FileName
        '
        Me.Label_FileName.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label_FileName.AutoSize = True
        Me.Label_FileName.Location = New System.Drawing.Point(12, 219)
        Me.Label_FileName.Name = "Label_FileName"
        Me.Label_FileName.Size = New System.Drawing.Size(65, 12)
        Me.Label_FileName.TabIndex = 4
        Me.Label_FileName.Text = "文件名(&N):"
        '
        'ComboBox_Filter
        '
        Me.ComboBox_Filter.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_Filter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_Filter.FormattingEnabled = True
        Me.ComboBox_Filter.Location = New System.Drawing.Point(112, 243)
        Me.ComboBox_Filter.Name = "ComboBox_Filter"
        Me.ComboBox_Filter.Size = New System.Drawing.Size(297, 20)
        Me.ComboBox_Filter.TabIndex = 8
        '
        'Label_Filter
        '
        Me.Label_Filter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label_Filter.AutoSize = True
        Me.Label_Filter.Location = New System.Drawing.Point(12, 246)
        Me.Label_Filter.Name = "Label_Filter"
        Me.Label_Filter.Size = New System.Drawing.Size(77, 12)
        Me.Label_Filter.TabIndex = 7
        Me.Label_Filter.Text = "文件类型(&T):"
        '
        'ComboBox_Directory
        '
        Me.ComboBox_Directory.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.ComboBox_Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories
        Me.ComboBox_Directory.FormattingEnabled = True
        Me.ComboBox_Directory.Location = New System.Drawing.Point(112, 12)
        Me.ComboBox_Directory.Name = "ComboBox_Directory"
        Me.ComboBox_Directory.Size = New System.Drawing.Size(297, 20)
        Me.ComboBox_Directory.TabIndex = 1
        '
        'Label_Directory
        '
        Me.Label_Directory.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label_Directory.AutoSize = True
        Me.Label_Directory.Location = New System.Drawing.Point(12, 15)
        Me.Label_Directory.Name = "Label_Directory"
        Me.Label_Directory.Size = New System.Drawing.Size(77, 12)
        Me.Label_Directory.TabIndex = 0
        Me.Label_Directory.Text = "查找范围(&I):"
        '
        'FilePicker
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Button_Cancel
        Me.ClientSize = New System.Drawing.Size(494, 275)
        Me.Controls.Add(Me.Button_Enter)
        Me.Controls.Add(Me.Label_Filter)
        Me.Controls.Add(Me.Label_Directory)
        Me.Controls.Add(Me.Label_FileName)
        Me.Controls.Add(Me.ComboBox_Filter)
        Me.Controls.Add(Me.ComboBox_Directory)
        Me.Controls.Add(Me.ComboBox_FileName)
        Me.Controls.Add(Me.FileListView)
        Me.Controls.Add(Me.Button_Cancel)
        Me.Controls.Add(Me.Button_Select)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FilePicker"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "FilePicker"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button_Select As System.Windows.Forms.Button
    Friend WithEvents Button_Cancel As System.Windows.Forms.Button
    Friend WithEvents Button_Enter As System.Windows.Forms.Button
    Friend WithEvents FileListView As System.Windows.Forms.ListView
    Friend WithEvents ComboBox_FileName As System.Windows.Forms.ComboBox
    Friend WithEvents Label_FileName As System.Windows.Forms.Label
    Friend WithEvents ComboBox_Filter As System.Windows.Forms.ComboBox
    Friend WithEvents Label_Filter As System.Windows.Forms.Label
    Friend WithEvents ComboBox_Directory As System.Windows.Forms.ComboBox
    Friend WithEvents Label_Directory As System.Windows.Forms.Label
    Friend WithEvents ColumnHeader_Name As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader_Length As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader_Type As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader_ModifyTime As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader_CreateTime As System.Windows.Forms.ColumnHeader

End Class
