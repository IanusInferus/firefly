'==========================================================================
'
'  File:        VBValidator.vb
'  Location:    Firefly.VBValidator <Visual Basic .Net>
'  Description: VB文本验证工具
'  Version:     2013.01.20.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Public Module VBValidator

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
        Dim argv = CmdLine.Arguments

        Dim RepairByVal As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "repairbyval"
                    RepairByVal = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Length
            Case 0
                Validate(RepairByVal)
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("VB文本验证工具")
        Console.WriteLine("Firefly.VBValidator，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("VBValidator [/repairbyval]")
        Console.WriteLine("/repairbyval 自动修复ByVal缺失问题。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("VBValidator")
        Console.WriteLine("将验证当前目录下所有.vb文件。")
    End Sub

    Public Sub Validate(ByVal RepairByVal As Boolean)
        For Each f In Directory.EnumerateFiles(Environment.CurrentDirectory, "*.vb", SearchOption.AllDirectories)
            Dim Lines = Txt.ReadFile(f).UnifyNewLineToLf.Split(Lf)
            Dim NewLines As New List(Of String)
            Dim Changed As Boolean = False
            Dim ErrorLines As New List(Of KeyValuePair(Of Integer, String))
            For n = 0 To Lines.Length - 1
                Dim k = n
                Dim Line = Lines(n)
                If Line.Contains(" Sub ") OrElse Line.Contains(" Function ") Then
                    If Line.Contains(" Public ") OrElse Line.Contains(" Protected ") OrElse Line.Contains(" Friend ") OrElse Line.Contains(" Private ") Then
                        Dim ForEachParam =
                            Sub(ParamsText As String)
                                If ParamsText = "" Then Return
                                Dim l As New List(Of Char32)
                                Dim Level = 0
                                Dim Quote = False
                                For Each c In (ParamsText & ",").ToUTF32
                                    If Quote Then
                                        If c = """"c Then
                                            Quote = False
                                        End If
                                    Else
                                        Select Case c
                                            Case """"c
                                                Quote = True
                                            Case "("c
                                                Level += 1
                                            Case ")"c
                                                Level -= 1
                                            Case ","c
                                                If Level = 0 Then
                                                    Dim Param = l.ToArray().ToUTF16B.Trim(" "c)
                                                    l.Clear()
                                                    If Param = "" Then Continue For
                                                    If Param.Contains("ByVal") OrElse Param.Contains("ByRef") Then Continue For
                                                    ErrorLines.Add(CreatePair(k, "ByValError"))
                                                    Line = Line.Replace(Param, "ByVal " & Param)
                                                    Changed = True
                                                    Continue For
                                                End If
                                            Case Else
                                        End Select
                                    End If
                                    l.Add(c)
                                Next
                            End Sub

                        Dim ForEachParentheses =
                            Sub(LineText As String)
                                If LineText = "" Then Return
                                Dim l As New List(Of Char32)
                                Dim Level = 0
                                For Each c In (LineText & ",").ToUTF32
                                    Select Case c
                                        Case "("c
                                            If Level = 0 Then
                                                Level += 1
                                                Continue For
                                            Else
                                                Level += 1
                                            End If
                                        Case ")"c
                                            Level -= 1

                                            If Level = 0 Then
                                                Dim Params = l.ToArray().ToUTF16B.Trim(" "c)
                                                l.Clear()
                                                If Params.StartsWith("Of ") Then Continue For
                                                ForEachParam(Params)
                                                Exit For
                                            End If
                                        Case Else
                                    End Select
                                    If Level > 0 Then
                                        l.Add(c)
                                    End If
                                Next
                            End Sub

                        ForEachParentheses(Line)
                    End If
                End If
                NewLines.Add(Line)
            Next

            If ErrorLines.Count > 0 Then
                Console.WriteLine("Error in {0}".Formats(f))
                For Each p In ErrorLines
                    Console.WriteLine("  {0} : {1}".Formats(p.Key + 1, p.Value))
                Next
            End If
            If RepairByVal AndAlso Changed Then
                Console.WriteLine("按任意键修正。")
                Console.ReadKey()
                Txt.WriteFile(f, String.Join(CrLf, NewLines.ToArray))
            End If
        Next
    End Sub
End Module
