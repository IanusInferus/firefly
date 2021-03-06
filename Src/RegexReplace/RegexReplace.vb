﻿'==========================================================================
'
'  File:        RegexReplace.vb
'  Location:    Firefly.RegexReplace <Visual Basic .Net>
'  Description: 正则表达式字符串替换工具
'  Version:     2015.12.08.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.Texting

Public Module RegexRename

    Public Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                Return -1
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()
        Dim args = CmdLine.Arguments
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
            End Select
        Next

        If args.Length = 1 Then
            If CmdLine.Options.Length > 0 Then
                Dim FilePath = args(0)
                Dim Encoding = Txt.GetEncoding(FilePath)
                Dim Text = Txt.ReadFile(FilePath)
                Dim EscapeReplacement = False
                For Each opt In CmdLine.Options
                    Dim argv = opt.Arguments
                    Select Case opt.Name.ToLower
                        Case "resc"
                            EscapeReplacement = True
                        Case "replace"
                            Select Case argv.Length
                                Case 2
                                    Dim Pattern = argv(0)
                                    Dim Replacement = argv(1)
                                    If EscapeReplacement Then
                                        Replacement = Replacement.Descape()
                                    End If
                                    Text = Replace(Text, Pattern, Replacement)
                                Case Else
                                    Throw New ArgumentException(opt.Name & ":" & String.Join(",", opt.Arguments))
                            End Select
                        Case Else
                            Throw New ArgumentException(opt.Name)
                    End Select
                Next
                Txt.WriteFile(FilePath, Encoding, Text)
            End If
        Else
            DisplayInfo()
            Return -1
        End If
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("正则表达式字符串替换工具")
        Console.WriteLine("Firefly.RegexReplace，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("RegexReplace <FilePath> [/resc] Replace*")
        Console.WriteLine("FilePath 欲替换字符串的文件，注意替换时会直接覆盖原文件")
        Console.WriteLine("/resc 转义替换模式，使得替换模式中可以表达换行等字符")
        Console.WriteLine("Replace ::= /replace:<Pattern>,<Replacement>")
        Console.WriteLine("Pattern 匹配模式，已打开多行模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("Replacement 替换模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("RegexReplace Test.txt /replace:""a+"",""""")
        Console.WriteLine("将Test.txt中的所有aa...a去除掉。")
    End Sub

    Public Function Replace(ByVal Text As String, ByVal Pattern As String, ByVal Replacement As String) As String
        Dim Regex As New Regex(Pattern, RegexOptions.ExplicitCapture Or RegexOptions.Multiline)
        Return Regex.Replace(Text, Replacement)
    End Function
End Module
