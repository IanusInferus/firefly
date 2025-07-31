'==========================================================================
'
'  File:        FileSelectBox.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 文件选取框
'  Version:     2025.07.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel
Imports Firefly

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class FileSelectBox
    Inherits UserControl

    'UserControl 重写 Dispose，以清理组件列表。
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
    Friend WithEvents Button As System.Windows.Forms.Button
    Friend WithEvents TextBox As System.Windows.Forms.TextBox
    Friend WithEvents Label As System.Windows.Forms.Label
    Public WithEvents SplitContainer As System.Windows.Forms.SplitContainer
    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Button = New System.Windows.Forms.Button
        Me.TextBox = New System.Windows.Forms.TextBox
        Me.Label = New System.Windows.Forms.Label
        Me.SplitContainer = New System.Windows.Forms.SplitContainer
        Me.SplitContainer.Panel1.SuspendLayout()
        Me.SplitContainer.Panel2.SuspendLayout()
        Me.SplitContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'Button
        '
        Me.Button.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button.Location = New System.Drawing.Point(296, 2)
        Me.Button.Name = "Button"
        Me.Button.Size = New System.Drawing.Size(34, 23)
        Me.Button.TabIndex = 0
        Me.Button.Text = "..."
        Me.Button.UseVisualStyleBackColor = True
        '
        'TextBox
        '
        Me.TextBox.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.TextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories
        Me.TextBox.Location = New System.Drawing.Point(3, 3)
        Me.TextBox.Margin = New System.Windows.Forms.Padding(0)
        Me.TextBox.Name = "TextBox"
        Me.TextBox.Size = New System.Drawing.Size(290, 21)
        Me.TextBox.TabIndex = 1
        '
        'Label
        '
        Me.Label.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label.AutoSize = True
        Me.Label.Location = New System.Drawing.Point(3, 7)
        Me.Label.Name = "Label"
        Me.Label.Size = New System.Drawing.Size(35, 12)
        Me.Label.TabIndex = 2
        Me.Label.Text = "Label"
        '
        'SplitContainer
        '
        Me.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer.IsSplitterFixed = True
        Me.SplitContainer.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer.Name = "SplitContainer"
        '
        'SplitContainer.Panel1
        '
        Me.SplitContainer.Panel1.Controls.Add(Me.Label)
        '
        'SplitContainer.Panel2
        '
        Me.SplitContainer.Panel2.Controls.Add(Me.TextBox)
        Me.SplitContainer.Panel2.Controls.Add(Me.Button)
        Me.SplitContainer.Size = New System.Drawing.Size(379, 27)
        Me.SplitContainer.SplitterDistance = 45
        Me.SplitContainer.SplitterWidth = 1
        Me.SplitContainer.TabIndex = 3
        '
        'FileSelectBox
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.Controls.Add(Me.SplitContainer)
        Me.Name = "FileSelectBox"
        Me.Size = New System.Drawing.Size(379, 27)
        Me.SplitContainer.Panel1.ResumeLayout(False)
        Me.SplitContainer.Panel1.PerformLayout()
        Me.SplitContainer.Panel2.ResumeLayout(False)
        Me.SplitContainer.Panel2.PerformLayout()
        Me.SplitContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private Sub TextBox_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox.KeyUp
        If e.KeyData = Keys.Enter Then
            RaiseEvent EnterPressed(Me, New EventArgs)
        End If
    End Sub

    Public Sub PopupDialog()
        Dim CurrentDirectory = Environment.CurrentDirectory
        With TextBox
            Static d As FilePicker
            If d Is Nothing Then d = New FilePicker(IsSaveDialogValue)
            If d.IsSaveDialog <> IsSaveDialogValue Then d = New FilePicker(IsSaveDialogValue)
            d.Multiselect = Multiselect
            d.ModeSelection = ModeSelectionValue

            Dim dir As String = GetFileDirectory(.Text)
            If IO.Directory.Exists(dir) Then
                d.FilePath = .Text.TrimEnd("\"c).TrimEnd("/"c)
            End If
            d.Filter = FilterValue
            If d.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Dim T As String = GetRelativePath(d.FilePath, CurrentDirectory)
                If T <> "" AndAlso d.FilePath <> "" AndAlso T.Length < d.FilePath.Length Then
                    .Text = T
                Else
                    .Text = d.FilePath
                End If
            End If
        End With
        Environment.CurrentDirectory = CurrentDirectory
    End Sub

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button.Click
        PopupDialog()
    End Sub

    Private FilterValue As String = "(*.*)|*.*"
    <Category("Appearance")> _
    Public Property Filter() As String
        Get
            Return FilterValue
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Throw New ArgumentException
            Dim n As Integer
            Dim i As Integer
            While True
                i = Value.IndexOf("|", i + 1)
                If i < 0 Then Exit While
                n += 1
            End While
            If (n And 1) = 0 Then Throw New ArgumentException
            FilterValue = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property LabelText() As String
        Get
            Return Label.Text
        End Get
        Set(ByVal Value As String)
            Label.Text = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property Path() As String
        Get
            Return TextBox.Text
        End Get
        Set(ByVal Value As String)
            TextBox.Text = Value
        End Set
    End Property

    <Category("Appearance")> _
    Public Property SplitterDistance() As Integer
        Get
            Return SplitContainer.SplitterDistance
        End Get
        Set(ByVal Value As Integer)
            SplitContainer.SplitterDistance = Value
        End Set
    End Property

    Private IsSaveDialogValue As Boolean = False
    <Category("Behavior"), DefaultValue(False)> _
    Public Property IsSaveDialog() As Boolean
        Get
            Return IsSaveDialogValue
        End Get
        Set(ByVal Value As Boolean)
            IsSaveDialogValue = Value
        End Set
    End Property

    Private MultiselectValue As Boolean = False
    <Category("Behavior"), DefaultValue(False)> _
    Public Property Multiselect() As Boolean
        Get
            Return MultiselectValue
        End Get
        Set(ByVal Value As Boolean)
            MultiselectValue = Value
        End Set
    End Property

    Protected ModeSelectionValue As FilePicker.ModeSelectionEnum = FilePicker.ModeSelectionEnum.FileWithFolder
    <Category("Behavior"), DefaultValue(GetType(FilePicker.ModeSelectionEnum), "FileWithFolder")>
    Public Property ModeSelection() As FilePicker.ModeSelectionEnum
        Get
            Return ModeSelectionValue
        End Get
        Set(ByVal Value As FilePicker.ModeSelectionEnum)
            Select Case Value
                Case FilePicker.ModeSelectionEnum.File, FilePicker.ModeSelectionEnum.Folder, FilePicker.ModeSelectionEnum.FileWithFolder
                    ModeSelectionValue = Value
                Case Else
                    Throw New IO.InvalidDataException
            End Select
        End Set
    End Property

    Public Event EnterPressed(ByVal sender As Object, ByVal e As EventArgs)
End Class
