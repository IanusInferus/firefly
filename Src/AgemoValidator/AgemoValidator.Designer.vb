<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AgemoValidator
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AgemoValidator))
        Me.ListBox_Files = New System.Windows.Forms.ListBox
        Me.Label_Tip = New System.Windows.Forms.Label
        Me.TextBox_Output = New System.Windows.Forms.TextBox
        Me.Button_Validate = New System.Windows.Forms.Button
        Me.CheckBox_AutoRemove = New System.Windows.Forms.CheckBox
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
        Me.CheckBox_EnforceUTF16 = New System.Windows.Forms.CheckBox
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListBox_Files
        '
        Me.ListBox_Files.AllowDrop = True
        Me.ListBox_Files.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListBox_Files.ItemHeight = 12
        Me.ListBox_Files.Location = New System.Drawing.Point(0, 0)
        Me.ListBox_Files.Name = "ListBox_Files"
        Me.ListBox_Files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.ListBox_Files.Size = New System.Drawing.Size(302, 400)
        Me.ListBox_Files.TabIndex = 0
        '
        'Label_Tip
        '
        Me.Label_Tip.AutoSize = True
        Me.Label_Tip.Location = New System.Drawing.Point(10, 15)
        Me.Label_Tip.Name = "Label_Tip"
        Me.Label_Tip.Size = New System.Drawing.Size(155, 12)
        Me.Label_Tip.TabIndex = 0
        Me.Label_Tip.Text = "将待验证Agemo文本拖入此框"
        '
        'TextBox_Output
        '
        Me.TextBox_Output.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBox_Output.Location = New System.Drawing.Point(0, 0)
        Me.TextBox_Output.Multiline = True
        Me.TextBox_Output.Name = "TextBox_Output"
        Me.TextBox_Output.ReadOnly = True
        Me.TextBox_Output.Size = New System.Drawing.Size(302, 400)
        Me.TextBox_Output.TabIndex = 0
        '
        'Button_Validate
        '
        Me.Button_Validate.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Validate.Location = New System.Drawing.Point(545, 10)
        Me.Button_Validate.Name = "Button_Validate"
        Me.Button_Validate.Size = New System.Drawing.Size(75, 23)
        Me.Button_Validate.TabIndex = 3
        Me.Button_Validate.Text = "验证"
        Me.Button_Validate.UseVisualStyleBackColor = True
        '
        'CheckBox_AutoRemove
        '
        Me.CheckBox_AutoRemove.AutoSize = True
        Me.CheckBox_AutoRemove.Checked = True
        Me.CheckBox_AutoRemove.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox_AutoRemove.Location = New System.Drawing.Point(176, 14)
        Me.CheckBox_AutoRemove.Name = "CheckBox_AutoRemove"
        Me.CheckBox_AutoRemove.Size = New System.Drawing.Size(132, 16)
        Me.CheckBox_AutoRemove.TabIndex = 1
        Me.CheckBox_AutoRemove.Text = "自动去除已验证文本"
        Me.CheckBox_AutoRemove.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer1.Location = New System.Drawing.Point(12, 36)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.ListBox_Files)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.TextBox_Output)
        Me.SplitContainer1.Size = New System.Drawing.Size(608, 400)
        Me.SplitContainer1.SplitterDistance = 302
        Me.SplitContainer1.TabIndex = 4
        '
        'CheckBox_EnforceUTF16
        '
        Me.CheckBox_EnforceUTF16.AutoSize = True
        Me.CheckBox_EnforceUTF16.Checked = True
        Me.CheckBox_EnforceUTF16.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox_EnforceUTF16.Location = New System.Drawing.Point(318, 14)
        Me.CheckBox_EnforceUTF16.Name = "CheckBox_EnforceUTF16"
        Me.CheckBox_EnforceUTF16.Size = New System.Drawing.Size(180, 16)
        Me.CheckBox_EnforceUTF16.TabIndex = 2
        Me.CheckBox_EnforceUTF16.Text = "检查是否UTF16(Unicode)编码"
        Me.CheckBox_EnforceUTF16.UseVisualStyleBackColor = True
        '
        'AgemoValidator
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(632, 446)
        Me.Controls.Add(Me.CheckBox_EnforceUTF16)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.CheckBox_AutoRemove)
        Me.Controls.Add(Me.Button_Validate)
        Me.Controls.Add(Me.Label_Tip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "AgemoValidator"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Agemo文本验证器"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ListBox_Files As System.Windows.Forms.ListBox
    Friend WithEvents Label_Tip As System.Windows.Forms.Label
    Friend WithEvents TextBox_Output As System.Windows.Forms.TextBox
    Friend WithEvents Button_Validate As System.Windows.Forms.Button
    Friend WithEvents CheckBox_AutoRemove As System.Windows.Forms.CheckBox
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents CheckBox_EnforceUTF16 As System.Windows.Forms.CheckBox
End Class
