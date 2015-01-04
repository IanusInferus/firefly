'==========================================================================
'
'  File:        ExceptionInfo.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 异常信息
'  Version:     2015.01.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Diagnostics
Imports System.Reflection

Public NotInheritable Class ExceptionInfo
    Private Sub New()
    End Sub

    Public Shared ReadOnly Property AssemblyName As String
        Get
            Return Assembly.GetEntryAssembly.GetName.Name
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyTitle As String
        Get
            Dim Attributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), True)
            If Attributes.Length >= 1 Then
                Dim Str = DirectCast(Attributes(0), AssemblyTitleAttribute).Title
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyDescription As String
        Get
            Dim Attributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyDescriptionAttribute), True)
            If Attributes.Length >= 1 Then
                Dim Str = DirectCast(Attributes(0), AssemblyDescriptionAttribute).Description
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyDescriptionOrTitle As String
        Get
            Dim Description = AssemblyDescription
            If Description <> "" Then Return Description

            Dim Title = AssemblyTitle
            If Title <> "" Then Return Title

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyCompany As String
        Get
            Dim Attributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyCompanyAttribute), True)
            If Attributes.Length >= 1 Then
                Dim Str = DirectCast(Attributes(0), AssemblyCompanyAttribute).Company
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyProduct As String
        Get
            Dim DescriptionAttributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyProductAttribute), True)
            If DescriptionAttributes.Length >= 1 Then
                Dim Str = DirectCast(DescriptionAttributes(0), AssemblyProductAttribute).Product
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyCopyright As String
        Get
            Dim DescriptionAttributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyCopyrightAttribute), True)
            If DescriptionAttributes.Length >= 1 Then
                Dim Str = DirectCast(DescriptionAttributes(0), AssemblyCopyrightAttribute).Copyright
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyTrademark As String
        Get
            Dim DescriptionAttributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyTrademarkAttribute), True)
            If DescriptionAttributes.Length >= 1 Then
                Dim Str = DirectCast(DescriptionAttributes(0), AssemblyTrademarkAttribute).Trademark
                If Str <> "" Then Return Str
            End If

            Return ""
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyVersion As String
        Get
            Return Assembly.GetEntryAssembly.GetName().Version.ToString()
        End Get
    End Property
    Public Shared ReadOnly Property AssemblyFileVersion As String
        Get
            Dim DescriptionAttributes = Assembly.GetEntryAssembly.GetCustomAttributes(GetType(AssemblyFileVersionAttribute), True)
            If DescriptionAttributes.Length >= 1 Then
                Dim Str = DirectCast(DescriptionAttributes(0), AssemblyFileVersionAttribute).Version
                If Str <> "" Then Return Str
            End If

            Return AssemblyVersion
        End Get
    End Property

    Private Shared Sub GetExceptionInfoWithoutParent(ByVal ex As Exception, ByVal msg As StringBuilder, ByVal Level As Integer)
        If ex.InnerException IsNot Nothing AndAlso ex.InnerException IsNot ex AndAlso Level < 3 Then
            GetExceptionInfoWithoutParent(ex.InnerException, msg, Level + 1)
            msg.AppendLine(New String("-"c, 20))
        End If
        msg.AppendLine(String.Format("{0}:" & System.Environment.NewLine & "{1}", ex.GetType.FullName, ex.Message))
        msg.AppendLine()
        msg.Append(GetStackTrace(ex))
    End Sub
    Public Shared Function GetExceptionInfo(ByVal ex As Exception) As String
        Return GetExceptionInfo(ex, New StackTrace(2, True))
    End Function
    Public Shared Function GetExceptionInfo(ByVal ex As Exception, ByVal ParentTrace As StackTrace) As String
        Dim msg As New StringBuilder
        GetExceptionInfoWithoutParent(ex, msg, 0)
        If ParentTrace IsNot Nothing Then msg.AppendLine(GetStackTrace(ParentTrace))
        Return msg.ToString
    End Function
    Public Shared Function GetStackTrace(ByVal ex As Exception, ByVal ParentTrace As StackTrace) As String
        Return GetStackTrace(ex) & GetStackTrace(ParentTrace)
    End Function
    Public Shared Function GetStackTrace(ByVal ex As Exception) As String
        'mono中ExceptionDispatchInfo捕捉到的调用栈都存储在captured_traces中
        Dim f = GetType(Exception).GetField("captured_traces", BindingFlags.NonPublic Or BindingFlags.Instance)
        If f IsNot Nothing Then
            Dim captured_traces = TryCast(f.GetValue(ex), StackTrace())
            If captured_traces IsNot Nothing Then
                Return String.Join(New String("-"c, 20) & System.Environment.NewLine, captured_traces.Concat(New StackTrace() {New StackTrace(ex, True)}).Select(Function(s) GetStackTrace(s)))
            End If
        End If

        Return GetStackTrace(New StackTrace(ex, True))
    End Function
    Public Shared Function GetStackTrace(ByVal Trace As StackTrace) As String
        If Trace Is Nothing Then Return Nothing
        If Trace.FrameCount = 0 Then Return ""
        Dim sb As StringBuilder = New StringBuilder()
        For Each Frame In Trace.GetFrames
            sb.AppendLine(StackFrameToString(Frame))
        Next
        Return sb.ToString()
    End Function
    Public Shared Function StackFrameToString(ByVal Frame As StackFrame) As String
        Dim mi As MemberInfo = Frame.GetMethod()
        Dim Params As New List(Of String)
        For Each param In DirectCast(mi, MethodBase).GetParameters()
            If param.Name = "" Then
                Params.Add(param.ParameterType.Name)
            Else
                Params.Add(param.ParameterType.Name & " " & param.Name)
            End If
        Next

        Dim Pos As New List(Of String)
        If Frame.GetFileLineNumber > 0 Then Pos.Add(String.Format("Line {0}", Frame.GetFileLineNumber))
        If Frame.GetFileColumnNumber > 0 Then Pos.Add(String.Format("Column {0}", Frame.GetFileColumnNumber))
        If Frame.GetILOffset <> StackFrame.OFFSET_UNKNOWN Then Pos.Add(String.Format("IL {0:X4}", Frame.GetILOffset))
        If Frame.GetNativeOffset <> StackFrame.OFFSET_UNKNOWN Then Pos.Add(String.Format("N {0:X6}", Frame.GetNativeOffset))

        Dim l As New List(Of String)
        If mi.DeclaringType IsNot Nothing Then l.Add("{0}.".Formats(mi.DeclaringType.FullName))
        l.Add(mi.Name)
        l.Add("({0})".Formats(String.Join(", ", Params.ToArray)))
        If Frame.GetFileName <> "" Then l.Add(" {0}".Formats(Frame.GetFileName))
        If Pos.Count > 0 Then
            l.Add(" : ")
            l.Add(String.Join(", ", Pos.ToArray))
        End If
        Return String.Join("", l.ToArray)
    End Function
End Class
