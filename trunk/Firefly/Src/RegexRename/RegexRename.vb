'==========================================================================
'
'  File:        RegexRename.vb
'  Location:    Firefly.RegexRename <Visual Basic .Net>
'  Description: 正则表达式文件重命名工具
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly

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
        Dim argv = CmdLine.Arguments
        Dim NoCheck As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "y"
                    NoCheck = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next

        If argv.Length = 2 Then
            If NoCheck Then
                Rename(argv(0), argv(1))
            Else
                If Check(argv(0), argv(1)) Then
                    Rename(argv(0), argv(1))
                End If
            End If
        Else
            DisplayInfo()
            Return -1
        End If
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("正则表达式重命名文件工具")
        Console.WriteLine("Firefly.RegexRename，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("RegexRename <Pattern> <Replace> [/Y]")
        Console.WriteLine("Pattern 输入模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("Replace 替换模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("/Y 无需确认")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("RegexRename ""(?<Name>\d+)\.en"" ""${Name}.txt""")
        Console.WriteLine("将所有主文件名为数字、扩展名为en的文件的扩展名修改为txt。")
    End Sub

    Public Function Check(ByVal Pattern As String, ByVal Replace As String) As Boolean
        Dim Count = 0
        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                If Count = 0 Then
                    Console.WriteLine("前三个文件的转换结果如下：")
                End If
                Console.WriteLine(String.Format("{0} -> {1}", f, Path.Combine(Path.GetDirectoryName(f), Match.Result(Replace))))
                Count += 1
            End If
            If Count = 3 Then Exit For
        Next
        If Count = 0 Then
            Console.WriteLine("没有发现任何匹配。")
            Return False
        Else
            Console.Write("输入N或按Ctrl+C终止重命名，按其他任意键开始重命名...")
            Dim Key = Console.ReadKey.Key
            Console.WriteLine()
            If Key = ConsoleKey.N Then Return False
            Return True
        End If
    End Function

    Public Sub Rename(ByVal Pattern As String, ByVal Replace As String)
        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                File.Move(f, Path.Combine(Path.GetDirectoryName(f), Match.Result(Replace)))
            End If
        Next
    End Sub
End Module
