'==========================================================================
'
'  File:        CommandLine.vb
'  Location:    Firefly.Core <Visual Basic .Net>
'  Description: 控制台
'  Version:     2016.04.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
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

    Private Shared Function DescapeQuote(ByVal s As String) As String
        If s.Length < 2 Then Return s
        If s.StartsWith("""") AndAlso s.EndsWith("""") Then
            Return s.Substring(1, s.Length - 2).Replace("""""", """")
        Else
            Return s
        End If
    End Function

    Private Shared Function SplitCmdLineWithSpace(ByVal CmdLine As String) As List(Of String)
        Dim l As New List(Of String)
        Dim a As New List(Of Char32)
        Dim InQuote = False
        For Each c In CmdLine.ToUTF32()
            If c = " "c AndAlso Not InQuote Then
                If a.Count = 0 Then Continue For
                l.Add(DescapeQuote(a.ToUTF16B()))
                a.Clear()
                Continue For
            End If
            If c = """"c Then
                InQuote = Not InQuote
            End If
            a.Add(c)
        Next
        If a.Count <> 0 Then
            l.Add(DescapeQuote(a.ToUTF16B()))
        End If
        Return l
    End Function
    Private Shared Function SplitCmdLineWithChar(ByVal CmdLine As String, ByVal Splitter As Char32) As List(Of String)
        Dim l As New List(Of String)
        Dim a As New List(Of Char32)
        Dim InQuote = False
        For Each c In CmdLine.ToUTF32()
            If c = Splitter AndAlso Not InQuote Then
                l.Add(DescapeQuote(a.ToUTF16B()))
                a.Clear()
                Continue For
            End If
            If c = """"c Then
                InQuote = Not InQuote
            End If
            a.Add(c)
        Next
        l.Add(DescapeQuote(a.ToUTF16B()))
        Return l
    End Function

    Public Shared Function ParseCmdLine(ByVal CmdLine As String, ByVal SuppressFirst As Boolean) As CommandLineArguments
        Dim argv = SplitCmdLineWithSpace(CmdLine).Skip(1).ToList()

        Dim Arguments As New List(Of String)
        Dim Options As New List(Of CommandLineOption)

        For Each arg In argv
            If arg.StartsWith("/") Then
                Dim OptionLine = arg.Substring(1)
                Dim Name As String
                Dim ParameterLine As String
                Dim Index = OptionLine.IndexOf(":")
                If Index >= 0 Then
                    Name = DescapeQuote(OptionLine.Substring(0, Index))
                    ParameterLine = OptionLine.Substring(Index + 1)
                Else
                    Name = DescapeQuote(OptionLine)
                    ParameterLine = ""
                End If
                Options.Add(New CommandLineOption With {.Name = Name, .Arguments = SplitCmdLineWithChar(ParameterLine, ","c).ToArray})
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
