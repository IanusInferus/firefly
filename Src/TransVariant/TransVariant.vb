'==========================================================================
'
'  File:        TransVariant.vb
'  Location:    Firefly.TransVariant <Visual Basic .Net>
'  Description: 简繁日汉字异体字转换器
'  Version:     2010.09.11.
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
Imports Firefly.Texting

Public Module TransVariant

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
        'GenerateSubDatabase()
        'GenerateTables()
        'Return

        Dim CmdLine = CommandLine.GetCmdLine()
        Dim argv = CmdLine.Arguments
        Dim Punctuation As Boolean = False
        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
                Case "p"
                    Punctuation = True
                Case Else
                    Throw New ArgumentException(opt.Name)
            End Select
        Next
        Select Case argv.Count
            Case 3
                TransVariant(argv(0), Nothing, System.Enum.Parse(GetType(HanziSource), argv(1), True), System.Enum.Parse(GetType(HanziSource), argv(2), True), Punctuation)
            Case 4
                TransVariant(argv(0), GetEncoding(argv(1)), System.Enum.Parse(GetType(HanziSource), argv(2), True), System.Enum.Parse(GetType(HanziSource), argv(3), True), Punctuation)
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("简繁日汉字异体字转换器")
        Console.WriteLine("Firefly.TransVariant，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("本转换器仅转换一一对应字，不做按词判断字转换和异体字、异型词转换。")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("TransVariant <Pattern> [<Encoding>] <SourceHanziSource> <TargetHanziSource> [/P]")
        Console.WriteLine("Pattern 文本的文件名模式，参考 MSDN - 正则表达式 [.NET Framework]")
        Console.WriteLine("Encoding 原始编码，编码名称、代码页或者编码文件，可不指定，默认为GB18030")
        Console.WriteLine("编码若由名称或者代码页指定，则使用System.Text.Encoding.GetEncoding获得。")
        Console.WriteLine("编码若由编码文件(*.tbl)指定，则使用该编码文件生成一个编码，支持1-4字节编码，不支持破字节编码(如UTF-7)。")
        Console.WriteLine("若文件有BOM，则总识别BOM(UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF))，因此这几种带BOM的编码的文件等不受SourceEncoding影响。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("SourceHanziSource 原始汉字源，简繁日中的一种，分别为G、T、J。")
        Console.WriteLine("TargetHanziSource 目标汉字源，简繁日中的一种，分别为G、T、J。")
        Console.WriteLine("/P 转换标点，如间隔符(·)。")
        Console.WriteLine("注意：请事先备份原文件，本程序会直接修改原文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("TransVariant "".*?\.txt"" GB18030 J G")
        Console.WriteLine("将所有扩展名为txt、编码为GB18030的文件，其中的汉字由日体汉字转为简体汉字。")
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

    Public Enum HanziSource
        G = 1
        T = 2
        J = 4
    End Enum

    Public Sub TransVariant(ByVal Pattern As String, ByVal Encoding As System.Text.Encoding, ByVal SourceHanziSource As HanziSource, ByVal TargetHanziSource As HanziSource, ByVal Punctuation As Boolean)
        Dim Src2TarDict As Dictionary(Of Char32, Char32) = Nothing
        Dim Tar2SrcDict As Dictionary(Of Char32, Char32) = Nothing
        Select Case SourceHanziSource
            Case HanziSource.G
                Select Case TargetHanziSource
                    Case HanziSource.G

                    Case HanziSource.T
                        Src2TarDict = HanziConverter.G2T_Dict
                        Tar2SrcDict = HanziConverter.T2G_Dict
                    Case HanziSource.J
                        Src2TarDict = HanziConverter.G2J_Dict
                        Tar2SrcDict = HanziConverter.J2G_Dict
                End Select
            Case HanziSource.T
                Select Case TargetHanziSource
                    Case HanziSource.G
                        Src2TarDict = HanziConverter.T2G_Dict
                        Tar2SrcDict = HanziConverter.G2T_Dict
                    Case HanziSource.T

                    Case HanziSource.J
                        Src2TarDict = HanziConverter.T2J_Dict
                        Tar2SrcDict = HanziConverter.J2T_Dict
                End Select
            Case HanziSource.J
                Select Case TargetHanziSource
                    Case HanziSource.G
                        Src2TarDict = HanziConverter.J2G_Dict
                        Tar2SrcDict = HanziConverter.G2J_Dict
                    Case HanziSource.T
                        Src2TarDict = HanziConverter.J2T_Dict
                        Tar2SrcDict = HanziConverter.T2J_Dict
                    Case HanziSource.J

                End Select
        End Select
        Select Case SourceHanziSource
            Case HanziSource.G, HanziSource.T
                Select Case TargetHanziSource
                    Case HanziSource.J
                        If Not Src2TarDict.ContainsKey("·") Then Src2TarDict.Add("·", "・")
                        If Not Tar2SrcDict.ContainsKey("・") Then Tar2SrcDict.Add("・", "·")
                End Select
            Case HanziSource.J
                Select Case TargetHanziSource
                    Case HanziSource.G, HanziSource.T
                        If Not Src2TarDict.ContainsKey("・") Then Src2TarDict.Add("・", "·")
                        If Not Tar2SrcDict.ContainsKey("·") Then Tar2SrcDict.Add("·", "・")
                End Select
        End Select

        Dim Count = 0
        If Src2TarDict IsNot Nothing AndAlso Tar2SrcDict IsNot Nothing Then
            Dim Regex As New Regex("^" & Pattern & "$", RegexOptions.ExplicitCapture)
            Dim CurrentDir = System.Environment.CurrentDirectory
            For Each f In Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
                If f.StartsWith(".\") OrElse f.StartsWith("./") Then f = f.Substring(2)
                Dim Match = Regex.Match(GetRelativePath(f, CurrentDir))
                If Match.Success Then
                    Dim s As String = ""
                    Dim sr As StreamReader = Nothing
                    Dim CurrentEncoding As System.Text.Encoding
                    Try
                        If Encoding Is Nothing Then
                            sr = Txt.CreateTextReader(f, TextEncoding.Default)
                        Else
                            sr = Txt.CreateTextReader(f, Encoding)
                        End If
                        If Not sr.EndOfStream Then
                            s = sr.ReadToEnd
                            s = HanziConverter.TableConvertOneOnOne(s, Src2TarDict, Tar2SrcDict)
                        End If
                        CurrentEncoding = sr.CurrentEncoding
                    Finally
                        If sr IsNot Nothing Then sr.Dispose()
                    End Try
                    Txt.WriteFile(f, CurrentEncoding, s)
                    Count += 1
                End If
            Next
        End If
        Console.WriteLine("共处理了{0}个文件。".Formats(Count))
    End Sub

    Public Sub GenerateSubDatabase()
        Dim uhdb As New UniHanDatabase
        uhdb.Load("Unihan_IRGSources.txt", AddressOf HanziVariantTableGen.IsToLoad)
        uhdb.Load("Unihan_Variants.txt", AddressOf HanziVariantTableGen.IsToLoad)

        Using sw = Txt.CreateTextWriter("UnihanS.txt", UTF16)
            For Each t In uhdb.GetTriples
                sw.WriteLine("U+{0:X4}\t{1}\t{2}".Descape.Formats(t.Unicode.Value, t.FieldType, t.Value))
            Next
        End Using

        Using sw = Txt.CreateTextWriter("UnihanView.txt", UTF16)
            For Each t In uhdb.GetTriples
                sw.WriteLine("{0}\t{1}\t{2}".Descape.Formats(t.Unicode.ToDisplayString, t.FieldType, t.Value))
            Next
        End Using
    End Sub

    Public Sub GenerateTables()
        Dim g As New HanziVariantTableGen("UnihanS.txt")

        Using sr = Txt.CreateTextWriter("GT.txt", UTF16)
            Dim Table = g.GetGTMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using

        Using sr = Txt.CreateTextWriter("TG.txt", UTF16)
            Dim Table = g.GetTGMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using

        Using sr = Txt.CreateTextWriter("JT.txt", UTF16)
            Dim Table = g.GetJTMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using

        Using sr = Txt.CreateTextWriter("TJ.txt", UTF16)
            Dim Table = g.GetTJMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using
        Using sr = Txt.CreateTextWriter("GJ.txt", UTF16)
            Dim Table = g.GetGJMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using

        Using sr = Txt.CreateTextWriter("JG.txt", UTF16)
            Dim Table = g.GetJGMap()
            sr.WriteLine((From p In Table Select p.Key).ToUTF16B)
            sr.WriteLine((From p In Table Select p.Value).ToUTF16B)
        End Using

        Dim Text = g.GenerateTranslateTables()
    End Sub

End Module
