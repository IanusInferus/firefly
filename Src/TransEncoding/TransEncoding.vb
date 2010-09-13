'==========================================================================
'
'  File:        TransEncoding.vb
'  Location:    Firefly.TransEncoding <Visual Basic .Net>
'  Description: 编码转换器
'  Version:     2010.08.28.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Public Module TransEncoding

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
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Length
            Case 2
                TransEncoding(argv(0), GetEncoding(argv(1)))
            Case 3
                TransEncoding(argv(0), GetEncoding(argv(1)), GetEncoding(argv(2)))
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("编码转换器")
        Console.WriteLine("Firefly.TransEncoding，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("TransEncoding <Pattern> <TargetEncoding> [<SourceEncoding>]")
        Console.WriteLine("Pattern 文本的文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("TargetEncoding 目标编码，编码名称、代码页或者编码文件")
        Console.WriteLine("SourceEncoding 原始编码，编码名称、代码页或者编码文件，可不指定，默认为GB18030")
        Console.WriteLine("编码若由名称或者代码页指定，则使用System.Text.Encoding.GetEncoding获得。")
        Console.WriteLine("编码若由编码文件(*.tbl)指定，则使用该编码文件生成一个编码，支持1-4字节编码，不支持破字节编码(如UTF-7)。")
        Console.WriteLine("若文件有BOM，则总识别BOM(UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF))，因此这几种带BOM的编码的文件等不受SourceEncoding影响。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("注意：请事先备份原文件，本程序会直接修改原文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("TransEncoding "".*?\.txt"" UTF-16 Shift-JIS")
        Console.WriteLine("将所有扩展名为txt的文件的编码从Shift-JIS转换为UTF-16。")
    End Sub

    Public Function GetEncoding(ByVal NameOrCodePageOrFile As String) As System.Text.Encoding
        If File.Exists(NameOrCodePageOrFile) Then
            Return TblCharMappingFile.ReadAsEncoding(NameOrCodePageOrFile)
        Else
            Dim CodePage As Integer
            If Integer.TryParse(NameOrCodePageOrFile, CodePage) Then
                Return System.Text.Encoding.GetEncoding(CodePage)
            Else
                Return System.Text.Encoding.GetEncoding(NameOrCodePageOrFile)
            End If
        End If
    End Function

    Public Sub TransEncoding(ByVal Pattern As String, ByVal TargetEncoding As System.Text.Encoding, Optional ByVal SourceEncoding As System.Text.Encoding = Nothing)
        If SourceEncoding Is Nothing Then SourceEncoding = TextEncoding.Default
        Dim Count = 0
        Dim WrittenCount = 0

        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                Dim t As String = ""
                Dim DetectedEncoding = Txt.GetEncoding(f, SourceEncoding)
                Using s = Txt.CreateTextReader(f, DetectedEncoding, True)
                    If Not s.EndOfStream Then
                        t = s.ReadToEnd
                    End If
                End Using
                If DetectedEncoding IsNot TargetEncoding Then
                    Txt.WriteFile(f, TargetEncoding, t)
                    WrittenCount += 1
                End If
                Count += 1
            End If
        Next

        Console.WriteLine("共处理了{0}个文件，写入{1}个文件。".Formats(Count, WrittenCount))
    End Sub
End Module
