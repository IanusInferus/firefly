'==========================================================================
'
'  File:        BruteForceExporter.vb
'  Location:    Firefly.BruteForceExporter <Visual Basic .Net>
'  Description: 暴力文本导出器
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Texting
Imports Firefly.Setting

Public Class Macro
    Public Key As String
    Public Value As String
End Class

Public Module BruteForceExporter

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
        Return 0
    End Function

    Public Function MainInner() As Integer
        LoadSetting()

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
            Case 4
                BruteForceExport(argv(0), argv(1), argv(2), GetEncoding(argv(3)))
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("暴力文本导出器")
        Console.WriteLine("Firefly.BruteForceExporter，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("BruteForceExporter <FileNamePattern> <TextPattern> <TextPatternExclude> <Encoding>")
        Console.WriteLine("FileNamePattern 文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("TextPattern 字节文本模式，正则表达式和自定义标记，如果文本模式有捕获组，则会导出各匹配组，否则导出整个模式的匹配。")
        Console.WriteLine("TextPatternExclude 字节文本排除模式，正则表达式和自定义标记，对匹配的文本模式进行排除，为""""表示不排除。请注意匹配整个匹配要在模式前后加^$符号。")
        Console.WriteLine("Encoding 目标编码，编码名称、代码页或者tbl编码文件。")
        Console.WriteLine("编码若由名称或者代码页指定，则使用System.Text.Encoding.GetEncoding获得。")
        Console.WriteLine("编码若由编码文件(*.tbl)指定，则使用该编码文件生成一个编码，支持1-4字节编码，不支持破字节编码(如UTF-7)。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("BruteForceExporter "".*?\.bin"" ""\p{MonoBasicLatin}{5,}|\p{MonoBasicLatinLetter}{3,}"" ""^\x20*$"" ASCII")
        Console.WriteLine("从所有扩展名为bin的文件中导出符合""五个或以上的基本拉丁字符或者三个或以上的基本拉丁字母""模式的字节组，排除全部是空格的组，并作为ASCII编码文本识别，以UTF-16编码保存为原文件名连接"".txt""的结果。")
    End Sub

    Public Macros As Macro()

    Public Sub LoadSetting()
        Dim CD = Environment.CurrentDirectory
        Environment.CurrentDirectory = GetFileDirectory(System.Reflection.Assembly.GetExecutingAssembly.Location)

        Dim XmlPath As String
        If IO.Directory.Exists("..\Ini") Then
            XmlPath = String.Format("..\Ini\{0}.xml", My.Application.Info.ProductName)
        Else
            XmlPath = String.Format("{0}.xml", My.Application.Info.ProductName)
        End If
        If IO.File.Exists(XmlPath) Then
            Macros = Xml.ReadFile(Of Macro())(XmlPath)
        Else
            Dim l As New List(Of Macro)
            l.Add(New Macro With {.Key = "\p{MonoBasicLatin}", .Value = "[\x20-\x7E]"})
            l.Add(New Macro With {.Key = "\p{MonoBasicLatinLetter}", .Value = "[\x41-\x5A\x61-\x7A]"})
            l.Add(New Macro With {.Key = "\p{DiBasicLatin}", .Value = "[\x20-\x7E][\x00-\x00]"})
            l.Add(New Macro With {.Key = "\p{DiBasicLatinLetter}", .Value = "[\x41-\x5A\x61-\x7A][\x00-\x00]"})
            l.Add(New Macro With {.Key = "\p{DiShiftJIS}", .Value = "[\x81-\x9F][\x40-\x7E\x80-\xFC]"})
            l.Add(New Macro With {.Key = "\p{DiShiftJIS_Standard}", .Value = "[\x81-\x98][\x40-\x7E\x80-\xFC]"})
            l.Add(New Macro With {.Key = "\p{DiTriTetraUTF8}", .Value = "[\xC0-\xDF][\x80-\xBF]|[\xE0-\xEF][\x80-\xBF][\x80-\xBF]|[\xF0-\xF7][\x80-\xBF][\x80-\xBF][\x80-\xBF]"})
            Macros = l.ToArray
            Xml.WriteFile(XmlPath, Macros)
        End If

        Environment.CurrentDirectory = CD
    End Sub

    Public Function FindAllString(ByVal b As Byte(), ByVal Pattern As String, ByVal PatternExclude As String, ByVal Encoding As System.Text.Encoding) As IEnumerable(Of WQSG.Triple)
        For Each m In Macros
            Pattern = Pattern.Replace(m.Key, "(" & m.Value & ")")
        Next
        Dim r As New Regex(Pattern, RegexOptions.ExplicitCapture)
        Dim re As Regex = Nothing
        If PatternExclude <> "" Then
            For Each m In Macros
                PatternExclude = PatternExclude.Replace(m.Key, "(" & m.Value & ")")
            Next
            re = New Regex(PatternExclude, RegexOptions.ExplicitCapture)
        End If
        Dim Groups = r.GetGroupNames().Except(New String() {"0"})
        Dim EnableExtraction = (Groups.Count >= 1)

        Dim t As New List(Of WQSG.Triple)
        Dim Input = ByteTextSearch.EncodeAsByteString(b)
        Dim Matches = r.Matches(Input)
        If EnableExtraction Then
            For Each ma As Match In Matches
                For Each m As Group In ma.Groups
                    If TypeOf m Is Match Then Continue For

                    Dim MatchedText = m.Value
                    If re.Match(MatchedText).Success Then Continue For
                    Dim Bytes = ByteTextSearch.DecodeFromByteString(MatchedText)
                    Dim Text = Encoding.GetChars(Bytes)
                    t.Add(New WQSG.Triple With {.Offset = m.Index, .Length = m.Length, .Text = Text})
                Next
            Next
        Else
            For Each m As Match In Matches
                Dim MatchedText = m.Value
                If re IsNot Nothing AndAlso re.Match(MatchedText).Success Then Continue For
                Dim Bytes = ByteTextSearch.DecodeFromByteString(MatchedText)
                Dim Text = Encoding.GetChars(Bytes)
                t.Add(New WQSG.Triple With {.Offset = m.Index, .Length = m.Length, .Text = Text})
            Next
        End If
        Return t.ToArray
    End Function

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

    Public Sub BruteForceExport(ByVal FileNamePattern As String, ByVal TextPattern As String, ByVal TextPatternExclude As String, ByVal Encoding As System.Text.Encoding)
        Dim Regex As New Regex("^" & FileNamePattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                Dim BinFilePath = f
                Dim WQSGPath = f & ".txt"

                Using BinFile = StreamEx.CreateReadable(BinFilePath, IO.FileMode.Open)
                    If BinFile.Length <= &H1000000 Then
                        Dim b = BinFile.Read(BinFile.Length)
                        Dim t = FindAllString(b, TextPattern, TextPatternExclude, Encoding)
                        WQSG.WriteFile(WQSGPath, UTF16, t)
                    Else
                        Dim t As New List(Of WQSG.Triple)
                        For n = 0 To (BinFile.Length + &H1000000 - 1) \ &H1000000 - 1
                            BinFile.Position = n * &H1000000
                            Dim b = BinFile.Read(Min(&H1100000, BinFile.Length - BinFile.Position))
                            Dim tn = FindAllString(b, TextPattern, TextPatternExclude, Encoding)
                            For Each t0 In tn
                                If t0.Offset < &H1000000 Then t.Add(New WQSG.Triple With {.Offset = t0.Offset + n * &H1000000, .Length = t0.Length, .Text = t0.Text})
                            Next
                        Next
                        WQSG.WriteFile(WQSGPath, UTF16, t)
                    End If
                End Using
            End If
        Next
    End Sub
End Module
