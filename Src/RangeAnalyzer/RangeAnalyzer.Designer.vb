<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RangeAnalyzer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RangeAnalyzer))
        Me.NumericUpDown_StartPosition = New System.Windows.Forms.NumericUpDown
        Me.NumericUpDown_EndPosition = New System.Windows.Forms.NumericUpDown
        Me.NumericUpDown_Step = New System.Windows.Forms.NumericUpDown
        Me.Button_GetRange = New System.Windows.Forms.Button
        Me.TextBox_Result = New System.Windows.Forms.TextBox
        Me.Label_StartPosition = New System.Windows.Forms.Label
        Me.Label_EndPosition = New System.Windows.Forms.Label
        Me.Label_Step = New System.Windows.Forms.Label
        Me.ComboBox_Type = New System.Windows.Forms.ComboBox
        Me.Label_Type = New System.Windows.Forms.Label
        Me.FileSelectBox_File = New Firefly.GUI.FileSelectBox
        CType(Me.NumericUpDown_StartPosition, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_EndPosition, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_Step, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'NumericUpDown_StartPosition
        '
        Me.NumericUpDown_StartPosition.Hexadecimal = True
        Me.NumericUpDown_StartPosition.Location = New System.Drawing.Point(15, 61)
        Me.NumericUpDown_StartPosition.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.NumericUpDown_StartPosition.Name = "NumericUpDown_StartPosition"
        Me.NumericUpDown_StartPosition.Size = New System.Drawing.Size(120, 21)
        Me.NumericUpDown_StartPosition.TabIndex = 2
        Me.NumericUpDown_StartPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'NumericUpDown_EndPosition
        '
        Me.NumericUpDown_EndPosition.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.NumericUpDown_EndPosition.Hexadecimal = True
        Me.NumericUpDown_EndPosition.Location = New System.Drawing.Point(152, 61)
        Me.NumericUpDown_EndPosition.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.NumericUpDown_EndPosition.Name = "NumericUpDown_EndPosition"
        Me.NumericUpDown_EndPosition.Size = New System.Drawing.Size(120, 21)
        Me.NumericUpDown_EndPosition.TabIndex = 4
        Me.NumericUpDown_EndPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'NumericUpDown_Step
        '
        Me.NumericUpDown_Step.Location = New System.Drawing.Point(15, 107)
        Me.NumericUpDown_Step.Maximum = New Decimal(New Integer() {2147483647, 0, 0, 0})
        Me.NumericUpDown_Step.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_Step.Name = "NumericUpDown_Step"
        Me.NumericUpDown_Step.Size = New System.Drawing.Size(120, 21)
        Me.NumericUpDown_Step.TabIndex = 6
        Me.NumericUpDown_Step.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.NumericUpDown_Step.Value = New Decimal(New Integer() {36, 0, 0, 0})
        '
        'Button_GetRange
        '
        Me.Button_GetRange.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_GetRange.Location = New System.Drawing.Point(197, 132)
        Me.Button_GetRange.Name = "Button_GetRange"
        Me.Button_GetRange.Size = New System.Drawing.Size(75, 23)
        Me.Button_GetRange.TabIndex = 9
        Me.Button_GetRange.Text = "获取范围"
        Me.Button_GetRange.UseVisualStyleBackColor = True
        '
        'TextBox_Result
        '
        Me.TextBox_Result.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBox_Result.Location = New System.Drawing.Point(15, 164)
        Me.TextBox_Result.Name = "TextBox_Result"
        Me.TextBox_Result.ReadOnly = True
        Me.TextBox_Result.Size = New System.Drawing.Size(257, 21)
        Me.TextBox_Result.TabIndex = 10
        '
        'Label_StartPosition
        '
        Me.Label_StartPosition.AutoSize = True
        Me.Label_StartPosition.Location = New System.Drawing.Point(13, 46)
        Me.Label_StartPosition.Name = "Label_StartPosition"
        Me.Label_StartPosition.Size = New System.Drawing.Size(83, 12)
        Me.Label_StartPosition.TabIndex = 1
        Me.Label_StartPosition.Text = "起始位置(HEX)"
        '
        'Label_EndPosition
        '
        Me.Label_EndPosition.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label_EndPosition.AutoSize = True
        Me.Label_EndPosition.Location = New System.Drawing.Point(150, 46)
        Me.Label_EndPosition.Name = "Label_EndPosition"
        Me.Label_EndPosition.Size = New System.Drawing.Size(83, 12)
        Me.Label_EndPosition.TabIndex = 3
        Me.Label_EndPosition.Text = "终止位置(HEX)"
        '
        'Label_Step
        '
        Me.Label_Step.AutoSize = True
        Me.Label_Step.Location = New System.Drawing.Point(13, 92)
        Me.Label_Step.Name = "Label_Step"
        Me.Label_Step.Size = New System.Drawing.Size(59, 12)
        Me.Label_Step.TabIndex = 5
        Me.Label_Step.Text = "步长(DEC)"
        '
        'ComboBox_Type
        '
        Me.ComboBox_Type.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_Type.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.ComboBox_Type.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.ComboBox_Type.FormattingEnabled = True
        Me.ComboBox_Type.Items.AddRange(New Object() {"SByte", "Int16", "Int32", "Int64", "Byte", "UInt16", "UInt32", "UInt64", "Int16B", "Int32B", "Int64B", "UInt16B", "UInt32B", "UInt64B"})
        Me.ComboBox_Type.Location = New System.Drawing.Point(152, 106)
        Me.ComboBox_Type.Name = "ComboBox_Type"
        Me.ComboBox_Type.Size = New System.Drawing.Size(120, 20)
        Me.ComboBox_Type.TabIndex = 8
        Me.ComboBox_Type.Text = "Int32"
        '
        'Label_Type
        '
        Me.Label_Type.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label_Type.AutoSize = True
        Me.Label_Type.Location = New System.Drawing.Point(150, 92)
        Me.Label_Type.Name = "Label_Type"
        Me.Label_Type.Size = New System.Drawing.Size(29, 12)
        Me.Label_Type.TabIndex = 7
        Me.Label_Type.Text = "类型"
        '
        'FileSelectBox_File
        '
        Me.FileSelectBox_File.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FileSelectBox_File.AutoSize = True
        Me.FileSelectBox_File.Filter = "(*.*)|*.*"
        Me.FileSelectBox_File.LabelText = "文件"
        Me.FileSelectBox_File.Location = New System.Drawing.Point(12, 12)
        Me.FileSelectBox_File.ModeSelection = Firefly.GUI.FilePicker.ModeSelectionEnum.File
        Me.FileSelectBox_File.Name = "FileSelectBox_File"
        Me.FileSelectBox_File.Path = ""
        Me.FileSelectBox_File.Size = New System.Drawing.Size(260, 27)
        Me.FileSelectBox_File.SplitterDistance = 45
        Me.FileSelectBox_File.TabIndex = 0
        '
        'RangeAnalyzer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 195)
        Me.Controls.Add(Me.ComboBox_Type)
        Me.Controls.Add(Me.Label_EndPosition)
        Me.Controls.Add(Me.Label_Type)
        Me.Controls.Add(Me.Label_Step)
        Me.Controls.Add(Me.Label_StartPosition)
        Me.Controls.Add(Me.Button_GetRange)
        Me.Controls.Add(Me.TextBox_Result)
        Me.Controls.Add(Me.FileSelectBox_File)
        Me.Controls.Add(Me.NumericUpDown_Step)
        Me.Controls.Add(Me.NumericUpDown_EndPosition)
        Me.Controls.Add(Me.NumericUpDown_StartPosition)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(300, 230)
        Me.MinimumSize = New System.Drawing.Size(300, 230)
        Me.Name = "RangeAnalyzer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "范围分析器"
        CType(Me.NumericUpDown_StartPosition, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_EndPosition, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_Step, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents NumericUpDown_StartPosition As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumericUpDown_EndPosition As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumericUpDown_Step As System.Windows.Forms.NumericUpDown
    Friend WithEvents FileSelectBox_File As Firefly.GUI.FileSelectBox
    Friend WithEvents Button_GetRange As System.Windows.Forms.Button
    Friend WithEvents TextBox_Result As System.Windows.Forms.TextBox
    Friend WithEvents Label_StartPosition As System.Windows.Forms.Label
    Friend WithEvents Label_EndPosition As System.Windows.Forms.Label
    Friend WithEvents Label_Step As System.Windows.Forms.Label
    Friend WithEvents ComboBox_Type As System.Windows.Forms.ComboBox
    Friend WithEvents Label_Type As System.Windows.Forms.Label

End Class
