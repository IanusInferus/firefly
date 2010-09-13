<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FontGen
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FontGen))
        Me.Label_FontName = New System.Windows.Forms.Label()
        Me.ComboBox_FontName = New System.Windows.Forms.ComboBox()
        Me.CheckBox_Bold = New System.Windows.Forms.CheckBox()
        Me.CheckBox_Italic = New System.Windows.Forms.CheckBox()
        Me.CheckBox_Underline = New System.Windows.Forms.CheckBox()
        Me.CheckBox_Strikeout = New System.Windows.Forms.CheckBox()
        Me.NumericUpDown_Size = New System.Windows.Forms.NumericUpDown()
        Me.Label_Size = New System.Windows.Forms.Label()
        Me.PictureBox_Preview = New System.Windows.Forms.PictureBox()
        Me.CheckBox_DoubleSample = New System.Windows.Forms.CheckBox()
        Me.Label_PhysicalWidth = New System.Windows.Forms.Label()
        Me.NumericUpDown_PhysicalWidth = New System.Windows.Forms.NumericUpDown()
        Me.Label_PhysicalHeight = New System.Windows.Forms.Label()
        Me.NumericUpDown_PhysicalHeight = New System.Windows.Forms.NumericUpDown()
        Me.PictureBox_Preview2x = New System.Windows.Forms.PictureBox()
        Me.SplitContainer_Main = New System.Windows.Forms.SplitContainer()
        Me.Label_DrawOffsetX = New System.Windows.Forms.Label()
        Me.Label_DrawOffsetY = New System.Windows.Forms.Label()
        Me.NumericUpDown_DrawOffsetX = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_DrawOffsetY = New System.Windows.Forms.NumericUpDown()
        Me.Label_VirtualOffsetX = New System.Windows.Forms.Label()
        Me.Label_VirtualOffsetY = New System.Windows.Forms.Label()
        Me.NumericUpDown_VirtualOffsetX = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_VirtualOffsetY = New System.Windows.Forms.NumericUpDown()
        Me.Label_VirtualDeltaWidth = New System.Windows.Forms.Label()
        Me.Label_VirtualDeltaHeight = New System.Windows.Forms.Label()
        Me.NumericUpDown_VirtualDeltaWidth = New System.Windows.Forms.NumericUpDown()
        Me.NumericUpDown_VirtualDeltaHeight = New System.Windows.Forms.NumericUpDown()
        Me.Button_Generate = New System.Windows.Forms.Button()
        Me.FileSelectBox_File = New Firefly.GUI.FileSelectBox()
        Me.Button_CmdToClipboard = New System.Windows.Forms.Button()
        Me.CheckBox_AnchorLeft = New System.Windows.Forms.CheckBox()
        CType(Me.NumericUpDown_Size, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox_Preview, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_PhysicalWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_PhysicalHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox_Preview2x, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer_Main.Panel1.SuspendLayout()
        Me.SplitContainer_Main.Panel2.SuspendLayout()
        Me.SplitContainer_Main.SuspendLayout()
        CType(Me.NumericUpDown_DrawOffsetX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_DrawOffsetY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_VirtualOffsetX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_VirtualOffsetY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_VirtualDeltaWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_VirtualDeltaHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label_FontName
        '
        Me.Label_FontName.AutoSize = True
        Me.Label_FontName.Location = New System.Drawing.Point(13, 13)
        Me.Label_FontName.Name = "Label_FontName"
        Me.Label_FontName.Size = New System.Drawing.Size(53, 12)
        Me.Label_FontName.TabIndex = 0
        Me.Label_FontName.Text = "字体名称"
        '
        'ComboBox_FontName
        '
        Me.ComboBox_FontName.FormattingEnabled = True
        Me.ComboBox_FontName.Location = New System.Drawing.Point(15, 29)
        Me.ComboBox_FontName.Name = "ComboBox_FontName"
        Me.ComboBox_FontName.Size = New System.Drawing.Size(120, 20)
        Me.ComboBox_FontName.TabIndex = 1
        Me.ComboBox_FontName.Text = "宋体"
        '
        'CheckBox_Bold
        '
        Me.CheckBox_Bold.AutoSize = True
        Me.CheckBox_Bold.Location = New System.Drawing.Point(15, 94)
        Me.CheckBox_Bold.Name = "CheckBox_Bold"
        Me.CheckBox_Bold.Size = New System.Drawing.Size(48, 16)
        Me.CheckBox_Bold.TabIndex = 2
        Me.CheckBox_Bold.Text = "加粗"
        Me.CheckBox_Bold.UseVisualStyleBackColor = True
        '
        'CheckBox_Italic
        '
        Me.CheckBox_Italic.AutoSize = True
        Me.CheckBox_Italic.Location = New System.Drawing.Point(78, 94)
        Me.CheckBox_Italic.Name = "CheckBox_Italic"
        Me.CheckBox_Italic.Size = New System.Drawing.Size(48, 16)
        Me.CheckBox_Italic.TabIndex = 2
        Me.CheckBox_Italic.Text = "斜体"
        Me.CheckBox_Italic.UseVisualStyleBackColor = True
        '
        'CheckBox_Underline
        '
        Me.CheckBox_Underline.AutoSize = True
        Me.CheckBox_Underline.Location = New System.Drawing.Point(15, 116)
        Me.CheckBox_Underline.Name = "CheckBox_Underline"
        Me.CheckBox_Underline.Size = New System.Drawing.Size(60, 16)
        Me.CheckBox_Underline.TabIndex = 2
        Me.CheckBox_Underline.Text = "下划线"
        Me.CheckBox_Underline.UseVisualStyleBackColor = True
        '
        'CheckBox_Strikeout
        '
        Me.CheckBox_Strikeout.AutoSize = True
        Me.CheckBox_Strikeout.Location = New System.Drawing.Point(78, 116)
        Me.CheckBox_Strikeout.Name = "CheckBox_Strikeout"
        Me.CheckBox_Strikeout.Size = New System.Drawing.Size(60, 16)
        Me.CheckBox_Strikeout.TabIndex = 2
        Me.CheckBox_Strikeout.Text = "删除线"
        Me.CheckBox_Strikeout.UseVisualStyleBackColor = True
        '
        'NumericUpDown_Size
        '
        Me.NumericUpDown_Size.Location = New System.Drawing.Point(15, 67)
        Me.NumericUpDown_Size.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_Size.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_Size.Name = "NumericUpDown_Size"
        Me.NumericUpDown_Size.Size = New System.Drawing.Size(120, 21)
        Me.NumericUpDown_Size.TabIndex = 3
        Me.NumericUpDown_Size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.NumericUpDown_Size.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'Label_Size
        '
        Me.Label_Size.AutoSize = True
        Me.Label_Size.Location = New System.Drawing.Point(13, 52)
        Me.Label_Size.Name = "Label_Size"
        Me.Label_Size.Size = New System.Drawing.Size(53, 12)
        Me.Label_Size.TabIndex = 0
        Me.Label_Size.Text = "字体大小"
        '
        'PictureBox_Preview
        '
        Me.PictureBox_Preview.BackColor = System.Drawing.Color.White
        Me.PictureBox_Preview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox_Preview.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox_Preview.Name = "PictureBox_Preview"
        Me.PictureBox_Preview.Size = New System.Drawing.Size(479, 129)
        Me.PictureBox_Preview.TabIndex = 4
        Me.PictureBox_Preview.TabStop = False
        '
        'CheckBox_DoubleSample
        '
        Me.CheckBox_DoubleSample.AutoSize = True
        Me.CheckBox_DoubleSample.Location = New System.Drawing.Point(15, 138)
        Me.CheckBox_DoubleSample.Name = "CheckBox_DoubleSample"
        Me.CheckBox_DoubleSample.Size = New System.Drawing.Size(60, 16)
        Me.CheckBox_DoubleSample.TabIndex = 2
        Me.CheckBox_DoubleSample.Text = "2x采样"
        Me.CheckBox_DoubleSample.UseVisualStyleBackColor = True
        '
        'Label_PhysicalWidth
        '
        Me.Label_PhysicalWidth.AutoSize = True
        Me.Label_PhysicalWidth.Location = New System.Drawing.Point(13, 157)
        Me.Label_PhysicalWidth.Name = "Label_PhysicalWidth"
        Me.Label_PhysicalWidth.Size = New System.Drawing.Size(53, 12)
        Me.Label_PhysicalWidth.TabIndex = 0
        Me.Label_PhysicalWidth.Text = "物理宽度"
        '
        'NumericUpDown_PhysicalWidth
        '
        Me.NumericUpDown_PhysicalWidth.Location = New System.Drawing.Point(15, 171)
        Me.NumericUpDown_PhysicalWidth.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_PhysicalWidth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_PhysicalWidth.Name = "NumericUpDown_PhysicalWidth"
        Me.NumericUpDown_PhysicalWidth.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_PhysicalWidth.TabIndex = 3
        Me.NumericUpDown_PhysicalWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.NumericUpDown_PhysicalWidth.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'Label_PhysicalHeight
        '
        Me.Label_PhysicalHeight.AutoSize = True
        Me.Label_PhysicalHeight.Location = New System.Drawing.Point(76, 157)
        Me.Label_PhysicalHeight.Name = "Label_PhysicalHeight"
        Me.Label_PhysicalHeight.Size = New System.Drawing.Size(53, 12)
        Me.Label_PhysicalHeight.TabIndex = 0
        Me.Label_PhysicalHeight.Text = "物理高度"
        '
        'NumericUpDown_PhysicalHeight
        '
        Me.NumericUpDown_PhysicalHeight.Location = New System.Drawing.Point(78, 172)
        Me.NumericUpDown_PhysicalHeight.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_PhysicalHeight.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_PhysicalHeight.Name = "NumericUpDown_PhysicalHeight"
        Me.NumericUpDown_PhysicalHeight.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_PhysicalHeight.TabIndex = 3
        Me.NumericUpDown_PhysicalHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.NumericUpDown_PhysicalHeight.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'PictureBox_Preview2x
        '
        Me.PictureBox_Preview2x.BackColor = System.Drawing.Color.White
        Me.PictureBox_Preview2x.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox_Preview2x.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox_Preview2x.Name = "PictureBox_Preview2x"
        Me.PictureBox_Preview2x.Size = New System.Drawing.Size(479, 258)
        Me.PictureBox_Preview2x.TabIndex = 4
        Me.PictureBox_Preview2x.TabStop = False
        '
        'SplitContainer_Main
        '
        Me.SplitContainer_Main.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer_Main.Location = New System.Drawing.Point(141, 46)
        Me.SplitContainer_Main.Name = "SplitContainer_Main"
        Me.SplitContainer_Main.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer_Main.Panel1
        '
        Me.SplitContainer_Main.Panel1.Controls.Add(Me.PictureBox_Preview)
        '
        'SplitContainer_Main.Panel2
        '
        Me.SplitContainer_Main.Panel2.Controls.Add(Me.PictureBox_Preview2x)
        Me.SplitContainer_Main.Size = New System.Drawing.Size(479, 391)
        Me.SplitContainer_Main.SplitterDistance = 129
        Me.SplitContainer_Main.TabIndex = 5
        '
        'Label_DrawOffsetX
        '
        Me.Label_DrawOffsetX.AutoSize = True
        Me.Label_DrawOffsetX.Location = New System.Drawing.Point(13, 196)
        Me.Label_DrawOffsetX.Name = "Label_DrawOffsetX"
        Me.Label_DrawOffsetX.Size = New System.Drawing.Size(59, 12)
        Me.Label_DrawOffsetX.TabIndex = 0
        Me.Label_DrawOffsetX.Text = "绘制X偏移"
        '
        'Label_DrawOffsetY
        '
        Me.Label_DrawOffsetY.AutoSize = True
        Me.Label_DrawOffsetY.Location = New System.Drawing.Point(76, 196)
        Me.Label_DrawOffsetY.Name = "Label_DrawOffsetY"
        Me.Label_DrawOffsetY.Size = New System.Drawing.Size(59, 12)
        Me.Label_DrawOffsetY.TabIndex = 0
        Me.Label_DrawOffsetY.Text = "绘制Y偏移"
        '
        'NumericUpDown_DrawOffsetX
        '
        Me.NumericUpDown_DrawOffsetX.Location = New System.Drawing.Point(15, 211)
        Me.NumericUpDown_DrawOffsetX.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_DrawOffsetX.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_DrawOffsetX.Name = "NumericUpDown_DrawOffsetX"
        Me.NumericUpDown_DrawOffsetX.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_DrawOffsetX.TabIndex = 3
        Me.NumericUpDown_DrawOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'NumericUpDown_DrawOffsetY
        '
        Me.NumericUpDown_DrawOffsetY.Location = New System.Drawing.Point(78, 211)
        Me.NumericUpDown_DrawOffsetY.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_DrawOffsetY.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_DrawOffsetY.Name = "NumericUpDown_DrawOffsetY"
        Me.NumericUpDown_DrawOffsetY.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_DrawOffsetY.TabIndex = 3
        Me.NumericUpDown_DrawOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label_VirtualOffsetX
        '
        Me.Label_VirtualOffsetX.AutoSize = True
        Me.Label_VirtualOffsetX.Location = New System.Drawing.Point(13, 235)
        Me.Label_VirtualOffsetX.Name = "Label_VirtualOffsetX"
        Me.Label_VirtualOffsetX.Size = New System.Drawing.Size(59, 12)
        Me.Label_VirtualOffsetX.TabIndex = 0
        Me.Label_VirtualOffsetX.Text = "虚拟X偏移"
        '
        'Label_VirtualOffsetY
        '
        Me.Label_VirtualOffsetY.AutoSize = True
        Me.Label_VirtualOffsetY.Location = New System.Drawing.Point(76, 235)
        Me.Label_VirtualOffsetY.Name = "Label_VirtualOffsetY"
        Me.Label_VirtualOffsetY.Size = New System.Drawing.Size(59, 12)
        Me.Label_VirtualOffsetY.TabIndex = 0
        Me.Label_VirtualOffsetY.Text = "虚拟Y偏移"
        '
        'NumericUpDown_VirtualOffsetX
        '
        Me.NumericUpDown_VirtualOffsetX.Location = New System.Drawing.Point(15, 250)
        Me.NumericUpDown_VirtualOffsetX.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_VirtualOffsetX.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_VirtualOffsetX.Name = "NumericUpDown_VirtualOffsetX"
        Me.NumericUpDown_VirtualOffsetX.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_VirtualOffsetX.TabIndex = 3
        Me.NumericUpDown_VirtualOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'NumericUpDown_VirtualOffsetY
        '
        Me.NumericUpDown_VirtualOffsetY.Location = New System.Drawing.Point(78, 250)
        Me.NumericUpDown_VirtualOffsetY.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_VirtualOffsetY.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_VirtualOffsetY.Name = "NumericUpDown_VirtualOffsetY"
        Me.NumericUpDown_VirtualOffsetY.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_VirtualOffsetY.TabIndex = 3
        Me.NumericUpDown_VirtualOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Label_VirtualDeltaWidth
        '
        Me.Label_VirtualDeltaWidth.AutoSize = True
        Me.Label_VirtualDeltaWidth.Location = New System.Drawing.Point(13, 274)
        Me.Label_VirtualDeltaWidth.Name = "Label_VirtualDeltaWidth"
        Me.Label_VirtualDeltaWidth.Size = New System.Drawing.Size(65, 12)
        Me.Label_VirtualDeltaWidth.TabIndex = 0
        Me.Label_VirtualDeltaWidth.Text = "虚拟宽度差"
        '
        'Label_VirtualDeltaHeight
        '
        Me.Label_VirtualDeltaHeight.AutoSize = True
        Me.Label_VirtualDeltaHeight.Location = New System.Drawing.Point(76, 274)
        Me.Label_VirtualDeltaHeight.Name = "Label_VirtualDeltaHeight"
        Me.Label_VirtualDeltaHeight.Size = New System.Drawing.Size(65, 12)
        Me.Label_VirtualDeltaHeight.TabIndex = 0
        Me.Label_VirtualDeltaHeight.Text = "虚拟高度差"
        '
        'NumericUpDown_VirtualDeltaWidth
        '
        Me.NumericUpDown_VirtualDeltaWidth.Location = New System.Drawing.Point(15, 289)
        Me.NumericUpDown_VirtualDeltaWidth.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_VirtualDeltaWidth.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_VirtualDeltaWidth.Name = "NumericUpDown_VirtualDeltaWidth"
        Me.NumericUpDown_VirtualDeltaWidth.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_VirtualDeltaWidth.TabIndex = 3
        Me.NumericUpDown_VirtualDeltaWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'NumericUpDown_VirtualDeltaHeight
        '
        Me.NumericUpDown_VirtualDeltaHeight.Location = New System.Drawing.Point(78, 289)
        Me.NumericUpDown_VirtualDeltaHeight.Maximum = New Decimal(New Integer() {65536, 0, 0, 0})
        Me.NumericUpDown_VirtualDeltaHeight.Minimum = New Decimal(New Integer() {65535, 0, 0, -2147483648})
        Me.NumericUpDown_VirtualDeltaHeight.Name = "NumericUpDown_VirtualDeltaHeight"
        Me.NumericUpDown_VirtualDeltaHeight.Size = New System.Drawing.Size(57, 21)
        Me.NumericUpDown_VirtualDeltaHeight.TabIndex = 3
        Me.NumericUpDown_VirtualDeltaHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'Button_Generate
        '
        Me.Button_Generate.Location = New System.Drawing.Point(15, 345)
        Me.Button_Generate.Name = "Button_Generate"
        Me.Button_Generate.Size = New System.Drawing.Size(120, 23)
        Me.Button_Generate.TabIndex = 7
        Me.Button_Generate.Text = "生成"
        Me.Button_Generate.UseVisualStyleBackColor = True
        '
        'FileSelectBox_File
        '
        Me.FileSelectBox_File.AutoSize = True
        Me.FileSelectBox_File.Filter = "tbl码表文件(*.tbl)|*.tbl|fd字符描述文件(*.fd)|*.fd|字符文件(*.txt)|*.txt"
        Me.FileSelectBox_File.LabelText = "tbl/fd/字符文件"
        Me.FileSelectBox_File.Location = New System.Drawing.Point(141, 13)
        Me.FileSelectBox_File.Name = "FileSelectBox_File"
        Me.FileSelectBox_File.Path = ""
        Me.FileSelectBox_File.Size = New System.Drawing.Size(479, 27)
        Me.FileSelectBox_File.SplitterDistance = 100
        Me.FileSelectBox_File.TabIndex = 6
        '
        'Button_CmdToClipboard
        '
        Me.Button_CmdToClipboard.Location = New System.Drawing.Point(15, 316)
        Me.Button_CmdToClipboard.Name = "Button_CmdToClipboard"
        Me.Button_CmdToClipboard.Size = New System.Drawing.Size(120, 23)
        Me.Button_CmdToClipboard.TabIndex = 8
        Me.Button_CmdToClipboard.Text = "传命令行到剪贴板"
        Me.Button_CmdToClipboard.UseVisualStyleBackColor = True
        '
        'CheckBox_AnchorLeft
        '
        Me.CheckBox_AnchorLeft.AutoSize = True
        Me.CheckBox_AnchorLeft.Location = New System.Drawing.Point(78, 138)
        Me.CheckBox_AnchorLeft.Name = "CheckBox_AnchorLeft"
        Me.CheckBox_AnchorLeft.Size = New System.Drawing.Size(60, 16)
        Me.CheckBox_AnchorLeft.TabIndex = 2
        Me.CheckBox_AnchorLeft.Text = "左对齐"
        Me.CheckBox_AnchorLeft.UseVisualStyleBackColor = True
        '
        'FontGen
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(632, 446)
        Me.Controls.Add(Me.Button_CmdToClipboard)
        Me.Controls.Add(Me.Button_Generate)
        Me.Controls.Add(Me.FileSelectBox_File)
        Me.Controls.Add(Me.SplitContainer_Main)
        Me.Controls.Add(Me.NumericUpDown_VirtualDeltaHeight)
        Me.Controls.Add(Me.NumericUpDown_VirtualDeltaWidth)
        Me.Controls.Add(Me.NumericUpDown_VirtualOffsetY)
        Me.Controls.Add(Me.NumericUpDown_VirtualOffsetX)
        Me.Controls.Add(Me.NumericUpDown_DrawOffsetY)
        Me.Controls.Add(Me.NumericUpDown_DrawOffsetX)
        Me.Controls.Add(Me.NumericUpDown_PhysicalHeight)
        Me.Controls.Add(Me.NumericUpDown_PhysicalWidth)
        Me.Controls.Add(Me.NumericUpDown_Size)
        Me.Controls.Add(Me.Label_VirtualDeltaHeight)
        Me.Controls.Add(Me.CheckBox_Strikeout)
        Me.Controls.Add(Me.Label_VirtualOffsetY)
        Me.Controls.Add(Me.CheckBox_Underline)
        Me.Controls.Add(Me.Label_DrawOffsetY)
        Me.Controls.Add(Me.Label_VirtualDeltaWidth)
        Me.Controls.Add(Me.CheckBox_Italic)
        Me.Controls.Add(Me.Label_VirtualOffsetX)
        Me.Controls.Add(Me.Label_PhysicalHeight)
        Me.Controls.Add(Me.Label_DrawOffsetX)
        Me.Controls.Add(Me.CheckBox_Bold)
        Me.Controls.Add(Me.Label_PhysicalWidth)
        Me.Controls.Add(Me.ComboBox_FontName)
        Me.Controls.Add(Me.Label_Size)
        Me.Controls.Add(Me.Label_FontName)
        Me.Controls.Add(Me.CheckBox_AnchorLeft)
        Me.Controls.Add(Me.CheckBox_DoubleSample)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "FontGen"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "字库图片生成器"
        CType(Me.NumericUpDown_Size, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox_Preview, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_PhysicalWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_PhysicalHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox_Preview2x, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer_Main.Panel1.ResumeLayout(False)
        Me.SplitContainer_Main.Panel2.ResumeLayout(False)
        Me.SplitContainer_Main.ResumeLayout(False)
        CType(Me.NumericUpDown_DrawOffsetX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_DrawOffsetY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_VirtualOffsetX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_VirtualOffsetY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_VirtualDeltaWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_VirtualDeltaHeight, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label_FontName As System.Windows.Forms.Label
    Friend WithEvents ComboBox_FontName As System.Windows.Forms.ComboBox
    Friend WithEvents CheckBox_Bold As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_Italic As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_Underline As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox_Strikeout As System.Windows.Forms.CheckBox
    Friend WithEvents NumericUpDown_Size As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label_Size As System.Windows.Forms.Label
    Friend WithEvents PictureBox_Preview As System.Windows.Forms.PictureBox
    Friend WithEvents CheckBox_DoubleSample As System.Windows.Forms.CheckBox
    Friend WithEvents Label_PhysicalWidth As System.Windows.Forms.Label
    Friend WithEvents NumericUpDown_PhysicalWidth As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label_PhysicalHeight As System.Windows.Forms.Label
    Friend WithEvents NumericUpDown_PhysicalHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents PictureBox_Preview2x As System.Windows.Forms.PictureBox
    Friend WithEvents SplitContainer_Main As System.Windows.Forms.SplitContainer
    Friend WithEvents Label_DrawOffsetX As System.Windows.Forms.Label
    Friend WithEvents Label_DrawOffsetY As System.Windows.Forms.Label
    Friend WithEvents NumericUpDown_DrawOffsetX As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumericUpDown_DrawOffsetY As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label_VirtualOffsetX As System.Windows.Forms.Label
    Friend WithEvents Label_VirtualOffsetY As System.Windows.Forms.Label
    Friend WithEvents NumericUpDown_VirtualOffsetX As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumericUpDown_VirtualOffsetY As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label_VirtualDeltaWidth As System.Windows.Forms.Label
    Friend WithEvents Label_VirtualDeltaHeight As System.Windows.Forms.Label
    Friend WithEvents NumericUpDown_VirtualDeltaWidth As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumericUpDown_VirtualDeltaHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents FileSelectBox_File As Firefly.GUI.FileSelectBox
    Friend WithEvents Button_Generate As System.Windows.Forms.Button
    Friend WithEvents Button_CmdToClipboard As System.Windows.Forms.Button
    Friend WithEvents CheckBox_AnchorLeft As System.Windows.Forms.CheckBox
End Class
