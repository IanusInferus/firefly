'==========================================================================
'
'  File:        RangeAnalyzer.vb
'  Location:    Firefly.RangeAnalyzer <Visual Basic .Net>
'  Description: 范围分析器
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.GUI

Public Class RangeAnalyzer

    Public Shared Sub Application_ThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        ExceptionHandler.PopupException(e.Exception, New StackTrace(4, True))
    End Sub

    Public Shared Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                ExceptionHandler.PopupException(ex)
                Return -1
            End Try
        End If
    End Function

    Public Shared Function MainInner() As Integer
        If Debugger.IsAttached Then
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException)
            Return MainWindow()
        Else
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
            Try
                AddHandler Application.ThreadException, AddressOf Application_ThreadException
                Return MainWindow()
            Catch ex As Exception
                ExceptionHandler.PopupException(ex)
                Return -1
            Finally
                RemoveHandler Application.ThreadException, AddressOf Application_ThreadException
            End Try
        End If
    End Function

    Public Shared Function MainWindow() As Integer
        Application.EnableVisualStyles()
        Application.Run(New RangeAnalyzer)
        Return 0
    End Function

    Private Shared ReadOnly Property Dict() As Dictionary(Of String, Func(Of StreamEx, Decimal, Decimal, Decimal, String))
        Get
            Static d As Dictionary(Of String, Func(Of StreamEx, Decimal, Decimal, Decimal, String))
            If d Is Nothing Then
                d = New Dictionary(Of String, Func(Of StreamEx, Decimal, Decimal, Decimal, String))(StringComparer.OrdinalIgnoreCase)
                d.Add("SByte", AddressOf GetRangeSByte)
                d.Add("Int16", AddressOf GetRangeInt16)
                d.Add("Int32", AddressOf GetRangeInt32)
                d.Add("Int64", AddressOf GetRangeInt64)
                d.Add("Byte", AddressOf GetRangeByte)
                d.Add("UInt16", AddressOf GetRangeUInt16)
                d.Add("UInt32", AddressOf GetRangeUInt32)
                d.Add("UInt64", AddressOf GetRangeUInt64)
                d.Add("Int16B", AddressOf GetRangeInt16B)
                d.Add("Int32B", AddressOf GetRangeInt32B)
                d.Add("Int64B", AddressOf GetRangeInt64B)
                d.Add("UInt16B", AddressOf GetRangeUInt16B)
                d.Add("UInt32B", AddressOf GetRangeUInt32B)
                d.Add("UInt64B", AddressOf GetRangeUInt64B)
            End If
            Return d
        End Get
    End Property


    Private Sub Button_GetRange_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_GetRange.Click
        If Not File.Exists(FileSelectBox_File.Path) Then
            MessageBox.Show("未选择文件", Me.Text, MessageBoxButtons.OK)
            Return
        End If
        Using s = StreamEx.CreateReadable(FileSelectBox_File.Path, FileMode.Open, FileShare.ReadWrite)
            If Dict.ContainsKey(ComboBox_Type.Text) Then
                Dim f = Dict(ComboBox_Type.Text)
                TextBox_Result.Text = f(s, NumericUpDown_StartPosition.Value, NumericUpDown_EndPosition.Value, NumericUpDown_Step.Value)
            Else
                TextBox_Result.Text = "没有这种类型"
            End If
        End Using
    End Sub

    Private Shared Function GetRangeSByte(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = SByte.MaxValue
        Dim Upper = SByte.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = CUS(s.ReadByte)
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X2}-0x{1:X2}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt16(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int16.MaxValue
        Dim Upper = Int16.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt16
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X4}-0x{1:X4}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt32(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int32.MaxValue
        Dim Upper = Int32.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt32
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X8}-0x{1:X8}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt64(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int64.MaxValue
        Dim Upper = Int64.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt64
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X16}-0x{1:X16}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeByte(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Byte.MaxValue
        Dim Upper = Byte.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadByte
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X2}-0x{1:X2}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt16(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt16.MaxValue
        Dim Upper = UInt16.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadUInt16
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X4}-0x{1:X4}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt32(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt32.MaxValue
        Dim Upper = UInt32.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = CSU(s.ReadInt32)
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X8}-0x{1:X8}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt64(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt64.MaxValue
        Dim Upper = UInt64.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = CSU(s.ReadInt64)
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X16}-0x{1:X16}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt16B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int16.MaxValue
        Dim Upper = Int16.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt16B
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X4}-0x{1:X4}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt32B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int32.MaxValue
        Dim Upper = Int32.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt32B
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X8}-0x{1:X8}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeInt64B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = Int64.MaxValue
        Dim Upper = Int64.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadInt64B
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X16}-0x{1:X16}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt16B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt16.MaxValue
        Dim Upper = UInt16.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = s.ReadUInt16B
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X4}-0x{1:X4}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt32B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt32.MaxValue
        Dim Upper = UInt32.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = CSU(s.ReadInt32B)
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X8}-0x{1:X8}]".Formats(Lower, Upper)
        End If
    End Function

    Private Shared Function GetRangeUInt64B(ByVal s As IReadableSeekableStream, ByVal PStart As Decimal, ByVal PEnd As Decimal, ByVal PStep As Decimal) As String
        If PStep <= 0 Then Throw New ArgumentOutOfRangeException("Step")
        Dim Lower = UInt64.MaxValue
        Dim Upper = UInt64.MinValue
        For p = PStart To PEnd - 1 Step PStep
            s.Position = p
            Dim v = CSU(s.ReadInt64B)
            Lower = Min(Lower, v)
            Upper = Max(Upper, v)
        Next
        If Lower > Upper Then
            Return ""
        Else
            Return "[0x{0:X16}-0x{1:X16}]".Formats(Lower, Upper)
        End If
    End Function
End Class
