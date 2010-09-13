'==========================================================================
'
'  File:        FileDialogEx.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 扩展文件对话框类，Win7下存在兼容性问题，已过时，请使用FilePicker
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Reflection

Public Class FileDialogEx

    <Obsolete("Win7下存在兼容性问题，已过时，请使用FilePicker")> _
    Public Sub New(Optional ByVal IsSaveDialog As Boolean = False)
        Me.Size = New Size(0, 0)
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(0, -Screen.PrimaryScreen.Bounds.Height * 16)
        Me.SetStyle(ControlStyles.EnableNotifyMessage, True)
        Me.ShowInTaskbar = False

        IsSaveDialogValue = IsSaveDialog
        If IsSaveDialog Then
            SaveDialogValue = New SaveFileDialog
        Else
            OpenDialogValue = New OpenFileDialog
        End If

        AutoUpgradeEnabled = False
        Multiselect = False
        ValidateNames = False
    End Sub

#Region " 修改过的属性 "

    Protected IsSaveDialogValue As Boolean
    Public ReadOnly Property IsSaveDialog() As Boolean
        Get
            Return IsSaveDialogValue
        End Get
    End Property

    Public Enum ModeSelectionEnum
        File = 1
        Folder = 2
        FileWithFolder = 3
    End Enum

    Protected InnerFilter As String
    Protected ModeSelectionValue As ModeSelectionEnum = ModeSelectionEnum.FileWithFolder
    Public Property ModeSelection() As ModeSelectionEnum
        Get
            Return ModeSelectionValue
        End Get
        Set(ByVal Value As ModeSelectionEnum)
            Select Case Value
                Case ModeSelectionEnum.File, ModeSelectionEnum.FileWithFolder
                    If ModeSelectionValue = ModeSelectionEnum.Folder Then
                        Dialog.Filter = InnerFilter
                    End If
                    ModeSelectionValue = Value
                Case ModeSelectionEnum.Folder
                    InnerFilter = Dialog.Filter
                    Dialog.Filter = "-| "
                    ModeSelectionValue = Value
                Case Else
                    Throw New IO.InvalidDataException
            End Select
        End Set
    End Property

    Public ReadOnly Property CanSelectFiles() As Boolean
        Get
            Return ModeSelectionValue And ModeSelectionEnum.File
        End Get
    End Property

    Public ReadOnly Property CanSelectFolders() As Boolean
        Get
            Return ModeSelectionValue And ModeSelectionEnum.Folder
        End Get
    End Property

    Private MultiselectValue As Boolean
    Public Property Multiselect() As Boolean
        Get
            Return MultiselectValue
        End Get
        Set(ByVal Value As Boolean)
            MultiselectValue = Value
            If IsSaveDialog Then
                Dim mi As MethodInfo = GetType(FileDialog).GetMethod("SetOption", BindingFlags.Instance Or BindingFlags.InvokeMethod Or BindingFlags.NonPublic Or BindingFlags.Public)
                'Friend Sub FileDialog.SetOption(ByVal [option] As Integer, ByVal value As Boolean)
                mi.Invoke(SaveDialogValue, New Object() {&H200, Value})
                'If Value Then
                '    Throw New NotSupportedException("Win7 has disabled this ability.")
                'End If
            Else
                OpenDialogValue.Multiselect = Value
            End If
        End Set
    End Property

    Public Property FileName() As String
        Get
            Return Dialog.FileName
        End Get
        Set(ByVal Value As String)
            Dialog.FileName = Value
        End Set
    End Property

    Private FileNamesValue As New List(Of String)
    Public ReadOnly Property FileNames() As String()
        Get
            Return FileNamesValue.ToArray
        End Get
    End Property

    ''' <summary>仅当作为OpenFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public ReadOnly Property SafeFileName() As String
        Get
            If IsSaveDialog Then Throw New NotSupportedException
            Return OpenDialogValue.SafeFileName
        End Get
    End Property

    Private SafeFileNamesValue As New List(Of String)
    ''' <summary>仅当作为OpenFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public ReadOnly Property SafeFileNames() As String()
        Get
            If IsSaveDialog Then Throw New NotSupportedException
            Return SafeFileNamesValue.ToArray
        End Get
    End Property

#End Region

#Region " FileDialog公有方法 "

    Public Property AddExtension() As Boolean
        Get
            Return Dialog.AddExtension
        End Get
        Set(ByVal Value As Boolean)
            Dialog.AddExtension = Value
        End Set
    End Property

    Public Property AutoUpgradeEnabled() As Boolean
        Get
            Return Dialog.AutoUpgradeEnabled
        End Get
        Set(ByVal Value As Boolean)
            Dialog.AutoUpgradeEnabled = Value
        End Set
    End Property

    Public Property CheckFileExists() As Boolean
        Get
            Return Dialog.CheckFileExists
        End Get
        Set(ByVal Value As Boolean)
            Dialog.CheckFileExists = Value
        End Set
    End Property

    Public Property CheckPathExists() As Boolean
        Get
            Return Dialog.CheckPathExists
        End Get
        Set(ByVal Value As Boolean)
            Dialog.CheckPathExists = Value
        End Set
    End Property

    Public ReadOnly Property CustomPlaces() As FileDialogCustomPlacesCollection
        Get
            Return Dialog.CustomPlaces
        End Get
    End Property

    Public Property DefaultExt() As String
        Get
            Return Dialog.DefaultExt
        End Get
        Set(ByVal Value As String)
            Dialog.DefaultExt = Value
        End Set
    End Property

    Public Property DereferenceLinks() As Boolean
        Get
            Return Dialog.DereferenceLinks
        End Get
        Set(ByVal Value As Boolean)
            Dialog.DereferenceLinks = Value
        End Set
    End Property

    Public Property Filter() As String
        Get
            If CanSelectFolders AndAlso Not CanSelectFiles Then
                Throw New InvalidOperationException
            End If
            Return Dialog.Filter
        End Get
        Set(ByVal Value As String)
            If CanSelectFolders AndAlso Not CanSelectFiles Then
                Throw New InvalidOperationException
            End If
            Dialog.Filter = Value
        End Set
    End Property

    Public Property FilterIndex() As Integer
        Get
            Return Dialog.FilterIndex
        End Get
        Set(ByVal Value As Integer)
            Dialog.FilterIndex = Value
        End Set
    End Property

    Public Property InitialDirectory() As String
        Get
            Return Dialog.InitialDirectory
        End Get
        Set(ByVal Value As String)
            Dialog.InitialDirectory = Value
        End Set
    End Property

    Public Function OpenFile() As System.IO.Stream
        If IsSaveDialog Then
            Return SaveDialogValue.OpenFile
        Else
            Return OpenDialogValue.OpenFile
        End If
    End Function

    Public Sub Reset()
        Dialog.Reset()
        Multiselect = False
    End Sub

    Public Property RestoreDirectory() As Boolean
        Get
            Return Dialog.RestoreDirectory
        End Get
        Set(ByVal Value As Boolean)
            Dialog.RestoreDirectory = Value
        End Set
    End Property

    Public Property ShowHelp() As Boolean
        Get
            Return Dialog.ShowHelp
        End Get
        Set(ByVal Value As Boolean)
            Dialog.ShowHelp = Value
        End Set
    End Property

    Public Property SupportMultiDottedExtensions() As Boolean
        Get
            Return Dialog.SupportMultiDottedExtensions
        End Get
        Set(ByVal Value As Boolean)
            Dialog.SupportMultiDottedExtensions = Value
        End Set
    End Property

    Public Property Title() As String
        Get
            Return Dialog.Title
        End Get
        Set(ByVal Value As String)
            Dialog.Title = Value
        End Set
    End Property

    Public Property ValidateNames() As Boolean
        Get
            Return Dialog.ValidateNames
        End Get
        Set(ByVal Value As Boolean)
            Dialog.ValidateNames = Value
        End Set
    End Property

#End Region

#Region " OpenFileDialog专有方法 "

    ''' <summary>仅当作为OpenFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public Property ReadOnlyChecked() As Boolean
        Get
            If IsSaveDialog Then Throw New NotSupportedException
            Return OpenDialogValue.ReadOnlyChecked
        End Get
        Set(ByVal Value As Boolean)
            If IsSaveDialog Then Throw New NotSupportedException
            OpenDialogValue.ReadOnlyChecked = Value
        End Set
    End Property

    ''' <summary>仅当作为OpenFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public Property ShowReadOnly() As Boolean
        Get
            If IsSaveDialog Then Throw New NotSupportedException
            Return OpenDialogValue.ShowReadOnly
        End Get
        Set(ByVal Value As Boolean)
            If IsSaveDialog Then Throw New NotSupportedException
            OpenDialogValue.ShowReadOnly = Value
        End Set
    End Property

#End Region

#Region " SaveFileDialog专有方法 "

    ''' <summary>仅当作为SaveFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public Property CreatePrompt() As Boolean
        Get
            If Not IsSaveDialog Then Throw New NotSupportedException
            Return SaveDialogValue.CreatePrompt
        End Get
        Set(ByVal Value As Boolean)
            If Not IsSaveDialog Then Throw New NotSupportedException
            SaveDialogValue.CreatePrompt = Value
        End Set
    End Property

    ''' <summary>仅当作为SaveFileDialog时有效，否则会抛出NotSupportedException异常</summary>
    Public Property OverwritePrompt() As Boolean
        Get
            If Not IsSaveDialog Then Throw New NotSupportedException
            Return SaveDialogValue.OverwritePrompt
        End Get
        Set(ByVal Value As Boolean)
            If Not IsSaveDialog Then Throw New NotSupportedException
            SaveDialogValue.OverwritePrompt = Value
        End Set
    End Property

#End Region

#Region " 事件参数 "

    Public Class SelectionChangedEventArgs
        Inherits System.EventArgs

        Public Sub New(ByVal FolderPath As String, ByVal FilePath As String, ByVal FileNames As String())
            FolderPathValue = FolderPath
            FilePathValue = FilePath
            FileNamesValue = FileNames
        End Sub

        Protected FolderPathValue As String
        Public ReadOnly Property FolderPath() As String
            Get
                Return FolderPathValue
            End Get
        End Property

        Protected FilePathValue As String
        Public ReadOnly Property FilePath() As String
            Get
                Return FilePathValue
            End Get
        End Property

        Protected FileNamesValue As String()
        Public ReadOnly Property FileNames() As String()
            Get
                Return FileNamesValue
            End Get
        End Property
    End Class

    Public Class ButtonOpenClickEventArgs
        Inherits SelectionChangedEventArgs

        Public Sub New(ByVal FolderPath As String, ByVal FilePath As String, ByVal FileNames As String(), ByVal Cancel As Boolean)
            MyBase.New(FolderPath, FilePath, FileNames)
            CancelValue = Cancel
        End Sub

        Protected CancelValue As Boolean
        Public Property Cancel() As Boolean
            Get
                Return CancelValue
            End Get
            Set(ByVal Value As Boolean)
                CancelValue = Value
            End Set
        End Property
    End Class
#End Region

#Region " 事件 "

    Private FileOKValue As Boolean = False
    Protected Sub mNativeDialog_ButtonOpenClick(ByVal sender As Object, ByVal e As ButtonOpenClickEventArgs) Handles mNativeDialog.ButtonOpenClick
        'If Not (EnableSelectFolders Or (IsSaveDialog AndAlso MultiselectValue)) Then Return

        Dialog.FileName = e.FilePath
        FileNamesValue.Clear()
        For Each f In e.FileNames
            FileNamesValue.Add(IO.Path.Combine(e.FolderPath, f))
        Next
        SafeFileNamesValue.Clear()
        SafeFileNamesValue.AddRange(e.FileNames)

        FileOKValue = True

        Dim b As New System.ComponentModel.CancelEventArgs
        RaiseEvent FileOk(Me, b)
        e.Cancel = b.Cancel

        If b.Cancel Then
            FileOKValue = False
        End If
    End Sub

    Public Event SelectionChangedClick(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
    Protected Sub mNativeDialog_SelectionChangedClick(ByVal sender As Object, ByVal e As SelectionChangedEventArgs) Handles mNativeDialog.SelectionChangedClick
        Dialog.FileName = e.FilePath
        FileNamesValue.Clear()
        For Each f In e.FileNames
            FileNamesValue.Add(IO.Path.Combine(e.FolderPath, f))
        Next
        SafeFileNamesValue.Clear()
        SafeFileNamesValue.AddRange(e.FileNames)

        RaiseEvent SelectionChangedClick(sender, e)
    End Sub

    Public Event FileOk(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
    Protected CancelValue As Boolean
    Protected Sub OpenDialogValue_FileOk(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles OpenDialogValue.FileOk
        If CanSelectFolders Then
            e.Cancel = CancelValue
        Else
            RaiseEvent FileOk(Me, e)
        End If
    End Sub
    Protected Sub SaveDialogValue_FileOk(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles SaveDialogValue.FileOk
        If CanSelectFolders OrElse (IsSaveDialog AndAlso MultiselectValue) Then
            e.Cancel = CancelValue
        Else
            RaiseEvent FileOk(Me, e)
        End If
    End Sub

    Public Event HelpRequest(ByVal sender As Object, ByVal e As System.EventArgs)
    Protected Sub OpenDialogValue_HelpRequest(ByVal sender As Object, ByVal e As System.EventArgs) Handles OpenDialogValue.HelpRequest
        RaiseEvent HelpRequest(Me, e)
    End Sub
    Protected Sub SaveDialogValue_HelpRequest(ByVal sender As Object, ByVal e As System.EventArgs) Handles SaveDialogValue.HelpRequest
        RaiseEvent HelpRequest(Me, e)
    End Sub

#End Region

End Class
