'==========================================================================
'
'  File:        CharAdder.vb
'  Location:    Firefly.CharAdder <Visual Basic .Net>
'  Description: 字符入库器
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Public Module CharAdder

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
        Dim RemoveUnicodeRanges As New List(Of Range)
        Dim IgnoreExistingBaseFile As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "removeunicode"
                    Dim arg = opt.Arguments
                    Select Case arg.Length
                        Case 2
                            Dim l = Int32.Parse(arg(0), Globalization.NumberStyles.HexNumber)
                            Dim u = Int32.Parse(arg(1), Globalization.NumberStyles.HexNumber)
                            RemoveUnicodeRanges.Add(New Range(l, u))
                        Case Else
                            Throw New ArgumentException(opt.Name & ":" & String.Join(",", opt.Arguments))
                    End Select
                Case "i"
                    IgnoreExistingBaseFile = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Length
            Case 2
                AddChar(argv(0), argv(1), "", RemoveUnicodeRanges, IgnoreExistingBaseFile)
            Case 3
                AddChar(argv(0), argv(1), argv(2), RemoveUnicodeRanges, IgnoreExistingBaseFile)
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("字符入库器")
        Console.WriteLine("Firefly.CharAdder，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("CharAdder <Pattern> <CharFile> [<ExcludeFile>] (RemoveUnicode)* [/I]")
        Console.WriteLine("RemoveUnicode ::= /removeunicode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("Pattern 文本的文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("CharFile 字符库文件")
        Console.WriteLine("ExcludeFile 字符排除库文件")
        Console.WriteLine("/removeunicode 移除该Unicode范围内(包含两边界)字符，Unicode的范围包括扩展平面")
        Console.WriteLine("/I 忽略已有字符库文件中的字符")
        Console.WriteLine("注意：文本文件编码仅支持GB18030(GB2312)和带BOM的Unicode系编码。生成的结果保存为UTF-16编码。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("CharAdder "".*?\.txt"" Char.txt Exclude.txt")
        Console.WriteLine("从所有扩展名为txt的文件中提取字符，排除掉Exclude.txt中的字符，加到Char.txt的最后。")
    End Sub

    Public Sub AddChar(ByVal Pattern As String, ByVal BaseFile As String, ByVal ExcludeFile As String, ByVal RemoveUnicodeRanges As List(Of Range), ByVal IgnoreExistingBaseFile As Boolean)
        Dim g As New EncodingStringGenerator
        g.PushExclude(Cr)
        g.PushExclude(Lf)

        If File.Exists(ExcludeFile) Then
            g.PushExclude(Txt.ReadFile(ExcludeFile, TextEncoding.Default))
        End If

        Dim LibString As String = ""
        If Not IgnoreExistingBaseFile Then
            If File.Exists(BaseFile) Then
                LibString = Txt.ReadFile(BaseFile, TextEncoding.Default)
                g.PushExclude(LibString)
            End If
        End If

        For Each c In New Indexer(RemoveUnicodeRanges)
            g.PushExclude(c)
        Next

        Dim Count = 0

        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(Path.GetFileName(f))
            If Match.Success Then
                g.PushText(Txt.ReadFile(f, TextEncoding.Default))
                Count += 1
            End If
        Next

        LibString &= g.GetLibString

        Using BaseWriter As New StreamWriter(BaseFile, False, System.Text.Encoding.Unicode)
            BaseWriter.Write(LibString.ToString)
        End Using

        Console.WriteLine("共处理了{0}个文件。".Formats(Count))
    End Sub
End Module
