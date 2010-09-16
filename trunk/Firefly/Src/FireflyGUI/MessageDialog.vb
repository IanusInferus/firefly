'==========================================================================
'
'  File:        MessageDialog.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 进度显示框
'  Version:     2008.11.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Windows.Forms

Public Class MessageDialog
    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        Buttons = MessageBoxButtons.OK
    End Sub

    Public Shadows Property Text As String
        Get
            Return Label_Message.Text
        End Get
        Set(ByVal Value As String)
            Label_Message.Text = Value
        End Set
    End Property

    Public Property Caption As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal Value As String)
            MyBase.Text = Value
        End Set
    End Property

    Private ButtonsValue As MessageBoxButtons = MessageBoxButtons.OK
    Public Property Buttons As MessageBoxButtons
        Get
            Return ButtonsValue
        End Get
        Set(ByVal Value As MessageBoxButtons)
            Select Case Value
                Case MessageBoxButtons.OK
                Case MessageBoxButtons.OKCancel
                Case MessageBoxButtons.AbortRetryIgnore
                Case MessageBoxButtons.YesNoCancel
                Case MessageBoxButtons.YesNo
                Case MessageBoxButtons.RetryCancel
                Case Else
                    Throw New ArgumentException
            End Select
            ButtonsValue = Value
        End Set
    End Property


    Public Shared Shadows Function Show(ByVal Text As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon, ByVal DefaultButton As MessageBoxDefaultButton) As DialogResult
        Return (New MessageDialog).ShowDialog()
    End Function
    Public Shared Shadows Function Show(ByVal Text As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon) As DialogResult
        Return Show(Text, Caption, Buttons, Icon, MessageBoxDefaultButton.Button1)
    End Function
    Public Shared Shadows Function Show(ByVal Text As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons) As DialogResult
        Return Show(Text, Caption, Buttons, MessageBoxIcon.None)
    End Function
    Public Shared Shadows Function Show(ByVal Text As String, ByVal Caption As String) As DialogResult
        Return Show(Text, Caption, MessageBoxButtons.OK)
    End Function
    Public Shared Shadows Function Show(ByVal Text As String) As DialogResult
        Return Show(Text, "")
    End Function
End Class