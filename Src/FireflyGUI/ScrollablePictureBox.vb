'==========================================================================
'
'  File:        ScrollablePictureBox.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 带滚动条的图形框
'  Version:     2009.07.02.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Drawing
Imports System.ComponentModel
Imports Firefly.Glyphing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class ScrollablePictureBox
    Inherits System.Windows.Forms.UserControl

#Region " 窗体代码 "
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
    Public WithEvents PictureBox As System.Windows.Forms.PictureBox

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PictureBox = New System.Windows.Forms.PictureBox
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox
        '
        Me.PictureBox.BackColor = System.Drawing.Color.White
        Me.PictureBox.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox.Name = "PictureBox"
        Me.PictureBox.Size = New System.Drawing.Size(200, 100)
        Me.PictureBox.TabIndex = 0
        Me.PictureBox.TabStop = False
        '
        'GlyphBox
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.Controls.Add(Me.PictureBox)
        Me.Name = "GlyphBox"
        Me.Size = New System.Drawing.Size(200, 100)
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
#End Region

    <Category("Appearance")> _
    Public Property Image() As Image
        Get
            If PictureBox IsNot Nothing Then Return PictureBox.Image
            Return Nothing
        End Get
        Set(ByVal Value As Image)
            If PictureBox Is Nothing Then Return
            PictureBox.Image = Value
            If Image Is Nothing Then
                PictureBox.Width = 0
                PictureBox.Height = 0
            Else
                PictureBox.Width = Image.Width
                PictureBox.Height = Image.Height
            End If
        End Set
    End Property


End Class
