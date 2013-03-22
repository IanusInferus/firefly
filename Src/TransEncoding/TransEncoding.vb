'==========================================================================
'
'  File:        TransEncoding.vb
'  Location:    Firefly.TransEncoding <Visual Basic .Net>
'  Description: 编码转换器
'  Version:     2013.03.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.Streaming
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

        Dim NoBom As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "nobom"
                    NoBom = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Length
            Case Is >= 2
                TransEncoding(argv(0), GetEncoding(argv(1)), argv.Skip(2).Select(Function(e) GetEncoding(e)).ToArray(), NoBom)
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
        Console.WriteLine("TransEncoding <Pattern> <TargetEncoding> <SourceEncoding>* [/nobom]")
        Console.WriteLine("Pattern 文本的文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("TargetEncoding 目标编码，编码名称、代码页或者编码文件")
        Console.WriteLine("SourceEncoding 原始编码，编码名称、代码页或者编码文件，可以指定多个，一个读取出错时则按后面一个尝试，若不指定，表示编码必须通过BOM识别")
        Console.WriteLine("/nobom 生成的文件中没有BOM")
        Console.WriteLine("编码若由名称或者代码页指定，则使用Encoding.GetEncoding获得。")
        Console.WriteLine("编码若由编码文件(*.tbl)指定，则使用该编码文件生成一个编码，支持1-4字节编码，不支持破字节编码(如UTF-7)。")
        Console.WriteLine("若文件有BOM，则总识别BOM(UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF))，因此这几种带BOM的编码的文件等不受SourceEncoding影响。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("注意：请事先备份原文件，本程序会直接修改原文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("TransEncoding "".*?\.txt"" UTF-16 Shift-JIS")
        Console.WriteLine("将所有扩展名为txt的文件的编码从Shift-JIS转换为UTF-16。")
    End Sub

    Public Function GetEncoding(ByVal NameOrCodePageOrFile As String) As Encoding
        If File.Exists(NameOrCodePageOrFile) Then
            Dim e = TblCharMappingFile.ReadAsEncoding(NameOrCodePageOrFile)
            e.EncoderFallback = EncoderFallback.ExceptionFallback
            e.DecoderFallback = DecoderFallback.ExceptionFallback
            Return e
        Else
            Dim CodePage As Integer
            If Integer.TryParse(NameOrCodePageOrFile, CodePage) Then
                Return Encoding.GetEncoding(CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
            Else
                Return Encoding.GetEncoding(NameOrCodePageOrFile, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
            End If
        End If
    End Function

    Public Sub TransEncoding(ByVal Pattern As String, ByVal TargetEncoding As Encoding, ByVal SourceEncodings As Encoding(), Optional ByVal NoBom As Boolean = False)
        Dim Count = 0
        Dim WrittenCount = 0
        Dim SkipCount = 0

        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                Dim DetectedEncoding = Txt.GetEncodingByBOM(f)
                Dim HasBOM = DetectedEncoding IsNot Nothing
                Dim NeedBOM = Not NoBom
                Dim TryEncodings As Encoding()
                If DetectedEncoding IsNot Nothing Then
                    TryEncodings = (New Encoding() {DetectedEncoding}).Concat(SourceEncodings).ToArray()
                Else
                    TryEncodings = SourceEncodings
                End If

                Dim t As String = Nothing
                For Each e In TryEncodings
                    Try
                        Using s = Txt.CreateTextReader(f, e, True)
                            If Not s.EndOfStream Then
                                t = s.ReadToEnd()
                            Else
                                t = ""
                            End If
                        End Using
                    Catch ex As DecoderFallbackException
                        Continue For
                    End Try
                    DetectedEncoding = e
                    Exit For
                Next
                If t Is Nothing Then
                    Console.WriteLine("编码无法识别: {0}".Formats(f))
                    SkipCount += 1
                ElseIf (Not IsSameIntrinsic(DetectedEncoding, TargetEncoding)) OrElse HasBOM <> NeedBOM Then
                    Dim Bytes As Byte() = Nothing
                    Try
                        Using ms = Streams.CreateMemoryStream()
                            Using sw = Txt.CreateTextWriter(ms.AsNewWriting(), TargetEncoding, NeedBOM)
                                Txt.WriteFile(sw, t)
                                sw.Flush()
                                ms.Position = 0
                                Bytes = ms.Read(ms.Length)
                            End Using
                        End Using
                    Catch ex As EncoderFallbackException
                    End Try
                    If Bytes Is Nothing Then
                        Console.WriteLine("文件内容无法编码: {0}".Formats(f))
                        SkipCount += 1
                    Else
                        Try
                            Using fs = Streams.CreateWritable(f)
                                fs.Write(Bytes)
                            End Using
                        Catch ex As Exception
                            Console.WriteLine("文件写入失败: {0}".Formats(f))
                        End Try
                        WrittenCount += 1
                    End If
                Else
                    SkipCount += 1
                End If
                Count += 1
            End If
        Next

        Console.WriteLine("共处理了{0}个文件，写入{1}个文件，跳过{2}个文件。".Formats(Count, WrittenCount, SkipCount))
    End Sub
End Module
