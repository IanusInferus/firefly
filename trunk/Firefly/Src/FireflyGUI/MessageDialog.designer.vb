<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MessageDialog
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MessageDialog))
        Me.PictureBox_Icon = New System.Windows.Forms.PictureBox()
        Me.TextBox_Information = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Label_Message = New System.Windows.Forms.Label()
        CType(Me.PictureBox_Icon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox_Icon
        '
        resources.ApplyResources(Me.PictureBox_Icon, "PictureBox_Icon")
        Me.PictureBox_Icon.Name = "PictureBox_Icon"
        Me.PictureBox_Icon.TabStop = False
        '
        'TextBox_Information
        '
        resources.ApplyResources(Me.TextBox_Information, "TextBox_Information")
        Me.TextBox_Information.BackColor = System.Drawing.SystemColors.Control
        Me.TextBox_Information.Name = "TextBox_Information"
        Me.TextBox_Information.ReadOnly = True
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        resources.ApplyResources(Me.Button3, "Button3")
        Me.Button3.Name = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Label_Message
        '
        resources.ApplyResources(Me.Label_Message, "Label_Message")
        Me.Label_Message.Name = "Label_Message"
        '
        'MessageDialog
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Label_Message)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox_Information)
        Me.Controls.Add(Me.PictureBox_Icon)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "MessageDialog"
        Me.ShowIcon = False
        CType(Me.PictureBox_Icon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents Button1 As System.Windows.Forms.Button
    Private WithEvents Button2 As System.Windows.Forms.Button
    Private WithEvents Button3 As System.Windows.Forms.Button
    Private WithEvents PictureBox_Icon As System.Windows.Forms.PictureBox
    Private WithEvents TextBox_Information As System.Windows.Forms.TextBox
    Friend WithEvents Label_Message As System.Windows.Forms.Label
End Class
