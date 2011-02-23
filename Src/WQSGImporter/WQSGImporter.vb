'==========================================================================
'
'  File:        WQSGImporter.vb
'  Location:    Firefly.WQSGImporter <Visual Basic .Net>
'  Description: WQSG文本导入器
'  Version:     2011.02.23.
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

Public Module WQSGImporter

    Public Enum NewLineStyle
        None = 0
        Lf = 1
        CrLf = 3
    End Enum

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
        Dim CmdLine = CommandLine.GetCmdLine()
        Dim argv = CmdLine.Arguments
        Dim PaddingBytes As List(Of Byte()) = Nothing
        Dim IgnoreNonExistFile As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "p"
                    PaddingBytes = New List(Of Byte())
                    For Each s In opt.Arguments
                        If s.Length Mod 2 <> 0 Then Throw New ArgumentException("""{0}""的长度应为偶数".Formats(s))
                        Dim Bytes As New List(Of Byte)
                        For n = 0 To (s.Length \ 2) - 1
                            Bytes.Add(Integer.Parse(s.Substring(n * 2, 2), Globalization.NumberStyles.HexNumber))
                        Next
                        PaddingBytes.Add(Bytes.ToArray)
                    Next
                Case "i"
                    IgnoreNonExistFile = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Length
            Case 3
                WQSGImport(argv(0), GetEncoding(argv(1)), Integer.Parse(argv(2), Globalization.NumberStyles.HexNumber), , PaddingBytes, IgnoreNonExistFile)
            Case 4
                Dim s As NewLineStyle = System.Enum.Parse(GetType(NewLineStyle), argv(3), True)
                WQSGImport(argv(0), GetEncoding(argv(1)), Integer.Parse(argv(2), Globalization.NumberStyles.HexNumber), s, PaddingBytes, IgnoreNonExistFile)
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("WQSG文本导入器")
        Console.WriteLine("Firefly.WQSGImporter，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("WQSGImporter <Pattern> <Encoding> [<PaddingByte> [<NewLineStyle>]] [/p:<PaddingBytes>] [/I]")
        Console.WriteLine("Pattern 被导入文件的文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("Encoding 导入时用的编码，编码名称、代码页或者编码文件")
        Console.WriteLine("PaddingByte 导入时用的填充字节，应由两位十六进制数组成，如00")
        Console.WriteLine("NewLineStyle 换行风格，CrLf 将\n反转义为CrLf(默认) Lf 将\n反转义为Lf None 不将\n反转义")
        Console.WriteLine("/p:<PaddingBytes> 导入时用的填充字节串，应由偶数位十六进制数组成，不要包含空格，如00或8140。可用逗号分隔主要填充字节串和备用填充字节串，如8140,20")
        Console.WriteLine("使用/p:<PaddingBytes>将忽略前面的<PaddingByte>")
        Console.WriteLine("/I 如果被导入文件对应的文本不存在，跳过该文件")
        Console.WriteLine("导入时用的文本文件是由被导入文件的文件名连接.txt获得，文本文件编码应为UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一。")
        Console.WriteLine("编码若由名称或者代码页指定，则使用System.Text.Encoding.GetEncoding获得。")
        Console.WriteLine("编码若由编码文件(*.tbl)指定，则使用该编码文件生成一个编码，支持1-4字节编码，不支持破字节编码(如UTF-7)。")
        Console.WriteLine("若文件有BOM，则总识别BOM(UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF))，因此这几种带BOM的编码的文件等不受SourceEncoding影响。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("WQSGImporter "".*?\.bin"" Shift-JIS 00 CrLf /p:8140,20 /I")
        Console.WriteLine("将所有扩展名为bin的文件的.txt文件中的WQSG文本按Shift-JIS编码导入到bin文件中，并将每句字符不足的文本用00补足，\n认为是CrLf。")
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

    Public Sub WQSGImport(ByVal Pattern As String, ByVal Encoding As System.Text.Encoding, ByVal PaddingByte As Byte, Optional ByVal Style As NewLineStyle = NewLineStyle.CrLf, Optional ByVal PaddingBytes As IEnumerable(Of Byte()) = Nothing, Optional ByVal IgnoreNonExistFile As Boolean = False)
        Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
        Dim CurrentDir = System.Environment.CurrentDirectory
        For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
            If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)

            Dim FileOutputedFlag = False

            Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
            If Match.Success Then
                Dim BinPath = f
                Dim WQSGPath = f & ".txt"
                If IgnoreNonExistFile AndAlso Not File.Exists(WQSGPath) Then Continue For
                Dim Triples = WQSG.ReadFile(WQSGPath)
                Using s = Streams.OpenResizable(BinPath)
                    For Each t In Triples
                        s.Position = t.Offset
                        Dim Text = t.Text
                        Select Case Style
                            Case NewLineStyle.None
                                Text = Text.Replace(CrLf, Lf).Replace(Lf, "\n")
                            Case NewLineStyle.Lf
                                Text = Text.Replace(CrLf, Lf)
                            Case NewLineStyle.CrLf
                                Text = Text.Replace(CrLf, Lf).Replace(Lf, CrLf)
                        End Select
                        Dim Bytes = Encoding.GetBytes(Text)
                        If Bytes.Length = t.Length Then
                            s.Write(Bytes)
                        ElseIf Bytes.Length < t.Length Then
                            If PaddingBytes Is Nothing Then
                                s.Write(Bytes.Extend(t.Length, PaddingByte))
                            Else
                                Dim l = Bytes.ToList
                                For Each b In PaddingBytes
                                    While l.Count + b.Length <= t.Length
                                        l.AddRange(b)
                                    End While
                                Next
                                If l.Count < t.Length Then
                                    If Not FileOutputedFlag Then
                                        Console.WriteLine("来自{0}的错误：".Formats(f))
                                        FileOutputedFlag = True
                                    End If
                                    Console.WriteLine("{0:X8}空{1}字节，不适合填充。".Formats(t.Offset, t.Length - Bytes.Length))
                                Else
                                    s.Write(l.ToArray)
                                End If
                            End If
                        Else
                            If Not FileOutputedFlag Then
                                Console.WriteLine("来自{0}的错误：".Formats(f))
                                FileOutputedFlag = True
                            End If
                            Console.WriteLine("{0:X8}超长{1}字节。".Formats(t.Offset, Bytes.Length - t.Length))
                        End If
                    Next
                End Using
            End If
        Next
    End Sub
End Module
