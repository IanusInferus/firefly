'==========================================================================
'
'  File:        FilePicker.vb
'  Location:    Firefly.GUI <Visual Basic .Net>
'  Description: 文件选取对话框
'  Version:     2009.12.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Windows.Forms

''' <summary>文件选取对话框，用于统一打开和保存单个和多个文件和文件夹，可替代OpenFileDialog、SaveFileDialog、FolderBrowserDialog三个对话框。</summary>
Partial Public Class FilePicker
    Private InitialDirectoryValue As String
    Public Property InitialDirectory() As String
        Get
            Return InitialDirectoryValue
        End Get
        Set(ByVal Value As String)
            If Directory.Exists(Value) Then
                InitialDirectoryValue = Value
                CurrentDirectory = Value
            End If
        End Set
    End Property

    Private CurrentDirectoryValue As String
    Public Property CurrentDirectory() As String
        Get
            Return CurrentDirectoryValue
        End Get
        Set(ByVal Value As String)
            Value = GetDirectoryPathWithTailingSeparator(GetAbsolutePath(Value, CurrentDirectoryValue))
            If Directory.Exists(Value) Then
                Dim Previous = CurrentDirectoryValue
                Try
                    CurrentDirectoryValue = Value
                    ComboBox_Directory.Text = Value
                    RefreshList()
                Catch ex As UnauthorizedAccessException
                    MessageBox.Show("无法访问{0}。\r\n拒绝访问。".Descape.Formats(CurrentDirectoryValue), "位置不可用", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    CurrentDirectoryValue = Previous
                    ComboBox_Directory.Text = Previous
                    RefreshList()
                End Try
            End If
        End Set
    End Property

    Private FilterValues As New List(Of KeyValuePair(Of String, String()))
    Private FilterIndexValue As Integer = -1
    Public Property Filter() As String
        Get
            Return String.Join("|", (From p In FilterValues Select p.Key & "|" & String.Join(";", p.Value)).ToArray)
        End Get
        Set(ByVal Value As String)
            If Value = "" OrElse Not Value.Contains("|") Then
                FilterValues.Clear()
                FilterIndexValue = -1
                Return
            End If
            Dim r = Value.Split("|")
            If r.Length Mod 2 <> 0 Then Throw New ArgumentException
            FilterValues = Enumerable.Range(0, r.Length \ 2).Select(Function(i) New KeyValuePair(Of String, String())(r(i * 2), r(i * 2 + 1).Split(";"))).ToList
            FilterIndexValue = 0
        End Set
    End Property

    Public Property CurrentFilterIndex() As Integer
        Get
            Return FilterIndexValue
        End Get
        Set(ByVal Value As Integer)
            FilterIndexValue = Value
            If FilterIndexValue >= 0 AndAlso FilterIndexValue < FilterValues.Count Then
                ComboBox_Filter.SelectedIndex = FilterIndexValue
            End If
        End Set
    End Property

    Protected IsSaveDialogValue As Boolean = False
    Public Property IsSaveDialog() As Boolean
        Get
            Return IsSaveDialogValue
        End Get
        Set(ByVal Value As Boolean)
            IsSaveDialogValue = Value
            If Me.Title <> Me.Name Then Return
            If Value Then
                Me.Title = "另存为.."
            Else
                Me.Title = "打开"
            End If
        End Set
    End Property

    Public Enum ModeSelectionEnum
        File = 1
        Folder = 2
        FileWithFolder = 3
    End Enum

    Protected ModeSelectionValue As ModeSelectionEnum = ModeSelectionEnum.File
    Public Property ModeSelection() As ModeSelectionEnum
        Get
            Return ModeSelectionValue
        End Get
        Set(ByVal Value As ModeSelectionEnum)
            Select Case Value
                Case ModeSelectionEnum.File, ModeSelectionEnum.FileWithFolder, ModeSelectionEnum.Folder
                    ModeSelectionValue = Value
                Case Else
                    Throw New ArgumentException
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

    Private MultiselectValue As Boolean = False
    Public Property Multiselect() As Boolean
        Get
            Return MultiselectValue
        End Get
        Set(ByVal Value As Boolean)
            MultiselectValue = Value
            FileListView.MultiSelect = Value
        End Set
    End Property

    Private FilePathValue As String = ""
    Public Property FilePath() As String
        Get
            Return FilePathValue
        End Get
        Set(ByVal Value As String)
            FilePathValue = Value
            CurrentDirectory = GetFileDirectory(Value)
            ComboBox_FileName.Text = GetFileName(Value)
        End Set
    End Property

    Private FileNamesValue As String() = New String() {}
    Public ReadOnly Property FileNames() As String()
        Get
            Return FileNamesValue.ToArray
        End Get
    End Property

    Public ReadOnly Property FilePaths() As String()
        Get
            Dim d = CurrentDirectory
            Return (From f In FileNamesValue Select GetAbsolutePath(f, d)).ToArray
        End Get
    End Property

    Private CheckFileExistsValue As Boolean = True
    Public Property CheckFileExists() As Boolean
        Get
            Return CheckFileExistsValue
        End Get
        Set(ByVal Value As Boolean)
            CheckFileExistsValue = Value
        End Set
    End Property

    Private CheckPathExistsValue As Boolean = True
    Public Property CheckPathExists() As Boolean
        Get
            Return CheckPathExistsValue
        End Get
        Set(ByVal Value As Boolean)
            CheckPathExistsValue = Value
        End Set
    End Property

    Public Property Title() As String
        Get
            Return Me.Text
        End Get
        Set(ByVal Value As String)
            Me.Text = Value
        End Set
    End Property

    Private ValidateNamesValue As Boolean = True
    Public Property ValidateNames() As Boolean
        Get
            Return ValidateNamesValue
        End Get
        Set(ByVal Value As Boolean)
            ValidateNamesValue = Value
        End Set
    End Property
End Class
