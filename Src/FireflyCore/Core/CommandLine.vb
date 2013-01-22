'==========================================================================
'
'  File:        CommandLine.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 控制台
'  Version:     2013.01.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Firefly.TextEncoding

Public NotInheritable Class CommandLine
    Private Sub New()
    End Sub

    Public Class CommandLineOption
        Public Name As String
        Public Arguments As String()
    End Class

    Public Class CommandLineArguments
        Public Arguments As String()
        Public Options As CommandLineOption()
    End Class

    Private Shared Function DescapeQuota(ByVal s As String) As String
        If s = "" Then Return ""
        If s.StartsWith("""") AndAlso s.EndsWith("""") Then
            Return s.Substring(1, s.Length - 2).Replace("""""", """")
        Else
            Return s
        End If
    End Function

    Private Shared Function SplitCmdLineWithChar(ByVal CmdLine As String, ByVal r As Regex, ByVal c As Char32, ByVal SuppressFirst As Boolean) As IEnumerable(Of String)
        Dim argv As New List(Of String)
        Dim SuppressedFirst As Boolean = Not SuppressFirst
        Dim NextStart = 0
        For Each arg As Match In r.Matches(CmdLine)
            If arg.Index <> NextStart Then
                If Not CmdLine.Substring(NextStart, arg.Index - NextStart).ToUTF32.All(Function(ch) ch = c) Then Throw New InvalidOperationException
            End If
            NextStart = arg.Index + arg.Length
            If Not SuppressedFirst Then
                SuppressedFirst = True
                Continue For
            End If
            If arg.Success Then
                Dim m = arg.Value
                argv.Add(DescapeQuota(m))
            Else
                Throw New InvalidOperationException
            End If
        Next
        If CmdLine.Length <> NextStart Then
            If Not CmdLine.Substring(NextStart, CmdLine.Length - NextStart).ToUTF32.All(Function(ch) ch = c) Then Throw New InvalidOperationException
        End If

        Return argv
    End Function

    Public Shared Function ParseCmdLine(ByVal CmdLine As String, ByVal SuppressFirst As Boolean) As CommandLineArguments
        Dim argv = SplitCmdLineWithChar(CmdLine, New Regex("(""[^""]*""|([^"" ])+)+", RegexOptions.ExplicitCapture), " "c, SuppressFirst)

        Dim Arguments As New List(Of String)
        Dim Options As New List(Of CommandLineOption)

        For Each arg In argv
            If arg.StartsWith("/") Then
                Dim OptionLine = arg.Substring(1)
                Dim Name As String
                Dim ParameterLine As String
                Dim Index = OptionLine.IndexOf(":")
                If Index >= 0 Then
                    Name = DescapeQuota(OptionLine.Substring(0, Index))
                    ParameterLine = OptionLine.Substring(Index + 1)
                Else
                    Name = DescapeQuota(OptionLine)
                    ParameterLine = ""
                End If
                Options.Add(New CommandLineOption With {.Name = Name, .Arguments = SplitCmdLineWithChar(ParameterLine, New Regex("(""[^""]*""|([^"",])+)+", RegexOptions.ExplicitCapture), ","c, False).ToArray})
            Else
                Arguments.Add(arg)
            End If
        Next

        Return New CommandLineArguments With {.Arguments = Arguments.ToArray, .Options = Options.ToArray}
    End Function

    Public Shared Function GetCmdLine() As CommandLineArguments
        If Environment.GetEnvironmentVariable("ComSpec") <> "" Then
            Return ParseCmdLine(Environment.CommandLine, True)
        Else
            Dim Args = Environment.GetCommandLineArgs()
            Dim l As New List(Of String)
            l.Add("""""")
            For Each a In Args.Skip(1)
                If a.StartsWith("//") Then
                    l.Add("""" & a.Substring(2).Replace("""", """""") & """")
                ElseIf a.StartsWith("/") Then
                    l.Add(a)
                ElseIf a.Contains("""") OrElse a.Contains(" ") Then
                    l.Add("""" & a.Replace("""", """""") & """")
                ElseIf a = "" Then
                    l.Add("""""")
                Else
                    l.Add(a)
                End If
            Next
            Dim CommandLine = String.Join(" ", l.ToArray())
            Return ParseCmdLine(CommandLine, True)
        End If
    End Function
End Class
