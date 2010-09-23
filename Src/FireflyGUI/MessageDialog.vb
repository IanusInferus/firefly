'==========================================================================
'
'  File:        MessageDialog.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 进度显示框
'  Version:     2010.09.23.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Media

Public Class MessageDialog
    Private ImageValue As Bitmap
    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        ImageValue = New Bitmap(PictureBox_Icon.Width, PictureBox_Icon.Height)
        Message = ""
        Information = ""
        Caption = ""
        Buttons = MessageBoxButtons.OK
        Icon = MessageBoxIcon.None
        DefaultButton = MessageBoxDefaultButton.Button1
    End Sub

    Private Sub MessageDialog_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If Disposing Then
            If ImageValue IsNot Nothing Then
                ImageValue.Dispose()
                ImageValue = Nothing
            End If
        End If
    End Sub

    Public Property Message As String
        Get
            Return Label_Message.Text
        End Get
        Set(ByVal Value As String)
            Label_Message.Text = Value
        End Set
    End Property

    Private InformationVisible As Boolean = True
    Public Property Information As String
        Get
            Return TextBox_Information.Text
        End Get
        Set(ByVal Value As String)
            If Value = "" Then
                InformationVisible = False
            Else
                InformationVisible = True
            End If
            TextBox_Information.Text = Value
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

    Private IconValue As MessageBoxIcon
    Private IconVisible As Boolean = False
    Public Shadows Property Icon As MessageBoxIcon
        Get
            Return IconValue
        End Get
        Set(ByVal Value As MessageBoxIcon)
            Using g = Graphics.FromImage(ImageValue)
                g.Clear(PictureBox_Icon.BackColor)

                Dim rect = New Rectangle(0, 0, PictureBox_Icon.Width, PictureBox_Icon.Height)

                Select Case Value
                    Case MessageBoxIcon.None
                    Case MessageBoxIcon.Question
                        g.DrawIcon(SystemIcons.Question, rect)
                    Case MessageBoxIcon.Error, MessageBoxIcon.Stop, MessageBoxIcon.Hand
                        g.DrawIcon(SystemIcons.Error, rect)
                    Case MessageBoxIcon.Warning, MessageBoxIcon.Exclamation
                        g.DrawIcon(SystemIcons.Warning, rect)
                    Case MessageBoxIcon.Information, MessageBoxIcon.Asterisk
                        g.DrawIcon(SystemIcons.Information, rect)
                    Case Else
                        Throw New ArgumentException
                End Select
            End Using
            PictureBox_Icon.Image = ImageValue
            IconValue = Value

            If Value = MessageBoxIcon.None Then
                IconVisible = False
            Else
                IconVisible = True
            End If
        End Set
    End Property

    Private DefaultButtonValue As MessageBoxDefaultButton
    Public Property DefaultButton As MessageBoxDefaultButton
        Get
            Return DefaultButtonValue
        End Get
        Set(ByVal Value As MessageBoxDefaultButton)
            Select Case Value
                Case MessageBoxDefaultButton.Button1
                Case MessageBoxDefaultButton.Button2
                Case MessageBoxDefaultButton.Button3
                Case Else
                    Throw New ArgumentException
            End Select
            DefaultButtonValue = Value
        End Set
    End Property

    Private Function IIf(Of T)(ByVal Flag As Boolean, ByVal TrueValue As T, ByVal FalseValue As T) As t
        If Flag Then
            Return TrueValue
        Else
            Return FalseValue
        End If
    End Function

    Public Sub UpdateLayout()
        PictureBox_Icon.Visible = IconVisible
        TextBox_Information.Visible = InformationVisible

        Dim WorkingAreaSize = SystemInformation.WorkingArea
        Dim MaximumFormSize = New Size(WorkingAreaSize.Width * 2 \ 3, WorkingAreaSize.Height)

        Dim IconWidth = PictureBox_Icon.Width
        Dim IconHeight = PictureBox_Icon.Height

        Dim CaptionHeight = SystemInformation.CaptionHeight
        Dim OuterBorderSize = SystemInformation.BorderSize
        Dim InnerBorderSize = New Size(20, 20)
        Dim ButtonHeight = Button3.Height
        Dim Gap = 10

        Dim MinimumFormSize = New Size(300, 300)
        Dim MinimumMessageSize = New Size(Max(200, MinimumFormSize.Width - OuterBorderSize.Width * 2 - InnerBorderSize.Width * 2 - IIf(IconVisible, IconWidth + Gap, 0)), Max(50, IconHeight))
        Dim MinimumInformationSize = New Size(200, 50)

        Dim MaximumMessageWidth = MaximumFormSize.Width - OuterBorderSize.Width * 2 - InnerBorderSize.Width * 2 - IIf(IconVisible, IconWidth + Gap, 0)
        Dim MaximumMessageHeight = MaximumFormSize.Height - OuterBorderSize.Height * 2 - InnerBorderSize.Height * 2 - CaptionHeight - Gap - ButtonHeight - IIf(InformationVisible, MinimumInformationSize.Height + Gap, 0)
        Dim MaximumMessageSize = New Size(MaximumMessageWidth, MaximumMessageHeight)

        Dim PreferredMessageSize = Label_Message.GetPreferredSize(MaximumMessageSize)
        If PreferredMessageSize.Width > MaximumMessageSize.Width Then PreferredMessageSize.Width = MaximumMessageSize.Width
        If PreferredMessageSize.Width < MinimumMessageSize.Width Then PreferredMessageSize.Width = MinimumMessageSize.Width
        If PreferredMessageSize.Height > MaximumMessageSize.Height Then PreferredMessageSize.Height = MaximumMessageSize.Height
        If PreferredMessageSize.Height < MinimumMessageSize.Height Then PreferredMessageSize.Height = MinimumMessageSize.Height

        Dim PreferredInformationSize As Size
        If InformationVisible Then
            Dim MaximumInformationWidth = MaximumMessageWidth
            Dim MaximumInformationHeight = MaximumFormSize.Height - OuterBorderSize.Height * 2 - InnerBorderSize.Height * 2 - CaptionHeight - Gap - ButtonHeight - PreferredMessageSize.Height - Gap
            If MaximumInformationHeight < MinimumInformationSize.Height Then
                PreferredMessageSize.Height -= MinimumInformationSize.Height - MaximumInformationHeight
                MaximumInformationHeight = MinimumInformationSize.Height
            End If
            Dim MaximumInformationSize = New Size(MaximumInformationWidth, MaximumInformationHeight)
            PreferredInformationSize = TextBox_Information.GetPreferredSize(MaximumInformationSize)
            If PreferredInformationSize.Width > MaximumInformationSize.Width Then PreferredInformationSize.Width = MaximumFormSize.Width
            If PreferredInformationSize.Width < MinimumInformationSize.Width Then PreferredInformationSize.Width = MinimumInformationSize.Width
            If PreferredInformationSize.Height > MaximumFormSize.Height Then PreferredInformationSize.Height = MaximumInformationSize.Height
            If PreferredInformationSize.Height < MinimumInformationSize.Height Then PreferredInformationSize.Height = MinimumInformationSize.Height
        Else
            PreferredInformationSize = Size.Empty
        End If

        Dim MessageSize = New Size(Max(PreferredMessageSize.Width, PreferredInformationSize.Width), PreferredMessageSize.Height)
        Dim InformationSize = New Size(MessageSize.Width, PreferredInformationSize.Height)

        Dim MessageLocation = New Point(InnerBorderSize.Width + IIf(IconVisible, IconWidth + Gap, 0), InnerBorderSize.Height)
        Dim InformationLocation = New Point(MessageLocation.X, MessageLocation.Y + MessageSize.Height + Gap)

        Dim ButtonY = IIf(InformationVisible, InformationLocation.Y + InformationSize.Height + Gap, InformationLocation.Y)

        Dim FormWidth = OuterBorderSize.Width * 2 + InnerBorderSize.Width * 2 + IIf(IconVisible, IconWidth + Gap, 0) + MessageSize.Width
        Dim FormHeight = OuterBorderSize.Height * 2 + InnerBorderSize.Height * 2 + CaptionHeight + Gap + ButtonHeight + MessageSize.Height + IIf(InformationVisible, InformationSize.Height + Gap, 0)

        Me.Width = FormWidth
        Me.Height = FormHeight
        Label_Message.Location = MessageLocation
        Label_Message.Size = MessageSize
        If InformationVisible Then
            TextBox_Information.Location = InformationLocation
            TextBox_Information.Size = InformationSize
        End If
        Button1.Location = New Point(Button1.Location.X, ButtonY)
        Button2.Location = New Point(Button2.Location.X, ButtonY)
        Button3.Location = New Point(Button3.Location.X, ButtonY)

        Button1.Hide()
        Button2.Hide()
        Button3.Hide()
        Button1.Text = ""
        Button2.Text = ""
        Button3.Text = ""
        CancelButton = Nothing

        Select Case Buttons
            Case MessageBoxButtons.OK
                Button3.Show()
                Button3.Text = "确定(&O)"
            Case MessageBoxButtons.OKCancel
                Button2.Show()
                Button2.Text = "确定(&O)"
                Button3.Show()
                Button3.Text = "取消"
                CancelButton = Button3
            Case MessageBoxButtons.AbortRetryIgnore
                Button1.Show()
                Button1.Text = "中止(&A)"
                Button2.Show()
                Button2.Text = "重试(&R)"
                Button3.Show()
                Button3.Text = "忽略(&I)"
            Case MessageBoxButtons.YesNoCancel
                Button1.Show()
                Button1.Text = "是(&Y)"
                Button2.Show()
                Button2.Text = "否(&N)"
                Button3.Show()
                Button3.Text = "取消"
                CancelButton = Button3
            Case MessageBoxButtons.YesNo
                Button2.Show()
                Button2.Text = "是(&Y)"
                Button3.Show()
                Button3.Text = "否(&N)"
            Case MessageBoxButtons.RetryCancel
                Button2.Show()
                Button2.Text = "重试(&R)"
                Button3.Show()
                Button3.Text = "取消"
                CancelButton = Button3
            Case Else
                Throw New InvalidOperationException
        End Select

        Select Case DefaultButton
            Case MessageBoxDefaultButton.Button1
                Select Case Buttons
                    Case MessageBoxButtons.OK
                        Button3.Select()
                    Case MessageBoxButtons.OKCancel
                        Button2.Select()
                    Case MessageBoxButtons.AbortRetryIgnore
                        Button1.Select()
                    Case MessageBoxButtons.YesNoCancel
                        Button1.Select()
                    Case MessageBoxButtons.YesNo
                        Button2.Select()
                    Case MessageBoxButtons.RetryCancel
                        Button2.Select()
                    Case Else
                        Throw New InvalidOperationException
                End Select
            Case MessageBoxDefaultButton.Button2
                Select Case Buttons
                    Case MessageBoxButtons.OK
                        Throw New InvalidOperationException
                    Case MessageBoxButtons.OKCancel
                        Button3.Select()
                    Case MessageBoxButtons.AbortRetryIgnore
                        Button2.Select()
                    Case MessageBoxButtons.YesNoCancel
                        Button2.Select()
                    Case MessageBoxButtons.YesNo
                        Button3.Select()
                    Case MessageBoxButtons.RetryCancel
                        Button3.Select()
                    Case Else
                        Throw New InvalidOperationException
                End Select
            Case MessageBoxDefaultButton.Button3
                Select Case Buttons
                    Case MessageBoxButtons.OK
                        Throw New InvalidOperationException
                    Case MessageBoxButtons.OKCancel
                        Throw New InvalidOperationException
                    Case MessageBoxButtons.AbortRetryIgnore
                        Button3.Select()
                    Case MessageBoxButtons.YesNoCancel
                        Button3.Select()
                    Case MessageBoxButtons.YesNo
                        Throw New InvalidOperationException
                    Case MessageBoxButtons.RetryCancel
                        Throw New InvalidOperationException
                    Case Else
                        Throw New InvalidOperationException
                End Select
            Case Else
                Throw New InvalidOperationException
        End Select
    End Sub

    Public Sub PlaySound()
        Select Case Icon
            Case MessageBoxIcon.None
            Case MessageBoxIcon.Question
                SystemSounds.Question.Play()
            Case MessageBoxIcon.Error, MessageBoxIcon.Stop, MessageBoxIcon.Hand
                SystemSounds.Hand.Play()
            Case MessageBoxIcon.Warning, MessageBoxIcon.Exclamation
                SystemSounds.Exclamation.Play()
            Case MessageBoxIcon.Information, MessageBoxIcon.Asterisk
                SystemSounds.Asterisk.Play()
            Case Else
                Throw New ArgumentException
        End Select
    End Sub

    Public Shadows Function ShowDialog() As DialogResult
        UpdateLayout()
        PlaySound()
        Return MyBase.ShowDialog()
    End Function

    Public Shadows Sub Show()
        UpdateLayout()
        PlaySound()
        MyBase.Show()
    End Sub

    Public Shared Shadows Function Show(ByVal Message As String, ByVal Information As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon, ByVal DefaultButton As MessageBoxDefaultButton) As DialogResult
        Using d As New MessageDialog With {.Message = Message, .Information = Information, .Caption = Caption, .Buttons = Buttons, .Icon = Icon, .DefaultButton = DefaultButton}
            Return d.ShowDialog()
        End Using
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Information As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon) As DialogResult
        Return Show(Message, Information, Caption, Buttons, Icon, MessageBoxDefaultButton.Button1)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Information As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons) As DialogResult
        Return Show(Message, Information, Caption, Buttons, MessageBoxIcon.None)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Information As String, ByVal Caption As String) As DialogResult
        Return Show(Message, Information, Caption, MessageBoxButtons.OK)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon, ByVal DefaultButton As MessageBoxDefaultButton) As DialogResult
        Return Show(Message, Nothing, Caption, Buttons, Icon, Buttons)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons, ByVal Icon As MessageBoxIcon) As DialogResult
        Return Show(Message, Caption, Buttons, Icon, MessageBoxDefaultButton.Button1)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Caption As String, ByVal Buttons As MessageBoxButtons) As DialogResult
        Return Show(Message, Caption, Buttons, MessageBoxIcon.None)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String, ByVal Caption As String) As DialogResult
        Return Show(Message, Caption, MessageBoxButtons.OK)
    End Function
    Public Shared Shadows Function Show(ByVal Message As String) As DialogResult
        Return Show(Message, "")
    End Function

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Select Case Buttons
            Case MessageBoxButtons.OK
                Throw New InvalidOperationException
            Case MessageBoxButtons.OKCancel
                Throw New InvalidOperationException
            Case MessageBoxButtons.AbortRetryIgnore
                DialogResult = DialogResult.Abort
            Case MessageBoxButtons.YesNoCancel
                DialogResult = DialogResult.Yes
            Case MessageBoxButtons.YesNo
                Throw New InvalidOperationException
            Case MessageBoxButtons.RetryCancel
                Throw New InvalidOperationException
            Case Else
                Throw New InvalidOperationException
        End Select
        Close()
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Select Case Buttons
            Case MessageBoxButtons.OK
                Throw New InvalidOperationException
            Case MessageBoxButtons.OKCancel
                DialogResult = DialogResult.OK
            Case MessageBoxButtons.AbortRetryIgnore
                DialogResult = DialogResult.Retry
            Case MessageBoxButtons.YesNoCancel
                DialogResult = DialogResult.No
            Case MessageBoxButtons.YesNo
                DialogResult = DialogResult.Yes
            Case MessageBoxButtons.RetryCancel
                DialogResult = DialogResult.Retry
            Case Else
                Throw New InvalidOperationException
        End Select
        Close()
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Select Case Buttons
            Case MessageBoxButtons.OK
                DialogResult = DialogResult.OK
            Case MessageBoxButtons.OKCancel
                DialogResult = DialogResult.Cancel
            Case MessageBoxButtons.AbortRetryIgnore
                DialogResult = DialogResult.Ignore
            Case MessageBoxButtons.YesNoCancel
                DialogResult = DialogResult.Cancel
            Case MessageBoxButtons.YesNo
                DialogResult = DialogResult.No
            Case MessageBoxButtons.RetryCancel
                DialogResult = DialogResult.Cancel
            Case Else
                Throw New InvalidOperationException
        End Select
        Close()
    End Sub
End Class
