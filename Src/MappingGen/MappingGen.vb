'==========================================================================
'
'  File:        MappingGen.vb
'  Location:    Firefly.MappingGen <Visual Basic .Net>
'  Description: 字符映射表生成器
'  Version:     2010.12.01.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Public Module MappingGen

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

    Public Sub DisplayInfo()
        Console.WriteLine("字符映射表生成器")
        Console.WriteLine("Firefly.MappingGen，按BSD许可证分发")
        Console.WriteLine("F.R.C.")
        Console.WriteLine("")
        Console.WriteLine("本字符映射表生成器按照一个原始的日文字符映射表和一个简体汉文字符表，生成最接近的新字符映射表。")
        Console.WriteLine("最接近是指：")
        Console.WriteLine("如果新映射表中有原映射表中的字符，那么码点一致。")
        Console.WriteLine("如果新映射表中有原映射表中的字符的简体形式而没有原始字符，那么码点一致。")
        Console.WriteLine("")
        Console.WriteLine("用法:")
        Console.WriteLine("MappingGen <SourceTblFile> <Charfile> <TargetTblFile> [/G] [/N] FixCode*")
        Console.WriteLine("FixCode ::= /fixcode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("SourceTblFile 原始日文字符映射表，tbl文件。")
        Console.WriteLine("CharFile 字符文件，包含所有新字符编码表中所需要的字符。")
        Console.WriteLine("TargetTblFile 目标字符映射表，tbl文件，UTF-16编码。")
        Console.WriteLine("/G 表明简体字比原字更优先。")
        Console.WriteLine("/N 表明不占用原始映射表的无字符码位。")
        Console.WriteLine("/fixcode 固定该编码范围内(包含两边界)字符。")
        Console.WriteLine("编码文件本身，应以UTF-16、GB18030、UTF-8、UTF-32、UTF-16B、UTF-32B之一编码。")
        Console.WriteLine("注意：请事先备份原文件，本程序会直接修改原文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("MappingGen Shift-JIS.tbl Char.txt FakeShift-JIS.tbl")
        Console.WriteLine("将Shift-JIS.tbl中的日文编码，以最接近的方式加入Char.txt中的内容，然后生成新的字符映射表FakeShift-JIS.tbl。")
        Console.WriteLine("")
        Console.WriteLine("高级用法:")
        Console.WriteLine("MappingGen (Add|AddNew|Replace|RemoveCode|Remove|Nullify|RemoveEmptyCode|SortCode|Save)*")
        Console.WriteLine("Add ::= /add:<Source tbl>")
        Console.WriteLine("AddNew ::= /addnew:<Source tbl>")
        Console.WriteLine("Replace ::= [/G] [/N] FixCode* /replace:<Source Charfile>")
        Console.WriteLine("FixCode ::= /fixcode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("RemoveCode ::= /removecode:<Lower:Hex>,<Upper:Hex>")
        Console.WriteLine("Remove ::= /remove:<Source Charfile>")
        Console.WriteLine("Nullify ::= /nullify:<Source Charfile>")
        Console.WriteLine("RemoveEmptyCode ::= /removeemptycode")
        Console.WriteLine("SortCode ::= /sortcode")
        Console.WriteLine("Save ::= /save:<Target tbl>")
        Console.WriteLine("/add 增加编码源，覆盖已存在编码。")
        Console.WriteLine("/addnew 增加编码源，不覆盖已存在编码。")
        Console.WriteLine("/replace 替换字符，按照一般用法进行。")
        Console.WriteLine("/removecode 删除编码。")
        Console.WriteLine("/remove 将在该字符源中的字符的编码删除。")
        Console.WriteLine("/nullify 将在该字符源中的字符置空。")
        Console.WriteLine("/removeemptycode 将没有字符的编码去除。")
        Console.WriteLine("/sortcode 按编码排序。")
        Console.WriteLine("/save 保存字形到fd文件。")
        Console.WriteLine("")
        Console.WriteLine("示例:")
        Console.WriteLine("MappingGen /add:Shift-JIS.tbl /replace:Char.txt /save:FakeShift-JIS.tbl")
    End Sub

    Public Class StringCodeComparer
        Inherits EqualityComparer(Of StringCode)

        Public Overloads Overrides Function Equals(ByVal x As StringCode, ByVal y As StringCode) As Boolean
            If x.HasCodes AndAlso y.HasCodes Then Return x.Codes = y.Codes
            Return x.Equals(y)
        End Function

        Public Overloads Overrides Function GetHashCode(ByVal obj As StringCode) As Integer
            If obj.HasCodes Then Return obj.Codes.GetHashCode()
            Return obj.GetHashCode()
        End Function
    End Class

    Public Function MainInner() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()

        For Each opt In CmdLine.Options
            Select Case opt.Name.ToLower
                Case "?", "help"
                    DisplayInfo()
                    Return 0
            End Select
        Next

        If CmdLine.Arguments.Count = 0 AndAlso CmdLine.Options.Count = 0 Then
            DisplayInfo()
            Return 0
        End If

        Select Case CmdLine.Arguments.Count
            Case 0
                Dim StringCodes As IEnumerable(Of StringCode) = New StringCode() {}
                Dim PriorG As Boolean = False
                Dim NoEmpty As Boolean = False
                Dim FixCodeRanges As New List(Of RangeUInt64)
                For Each opt In CmdLine.Options
                    Select Case opt.Name.ToLower
                        Case "g"
                            PriorG = True
                        Case "n"
                            NoEmpty = True
                        Case "fixcode"
                            Dim arg = opt.Arguments
                            Select Case arg.Length
                                Case 2
                                    Dim l = UInt64.Parse(arg(0), Globalization.NumberStyles.HexNumber)
                                    Dim u = UInt64.Parse(arg(1), Globalization.NumberStyles.HexNumber)
                                    FixCodeRanges.Add(New RangeUInt64(l, u))
                                Case Else
                                    Throw New ArgumentException(opt.Name & ":" & String.Join(",", opt.Arguments))
                            End Select
                        Case Else
                            Dim argv = opt.Arguments
                            Select Case opt.Name.ToLower
                                Case "add"
                                    Select Case argv.Length
                                        Case 1
                                            Dim sc = TblCharMappingFile.ReadFile(argv(0))
                                            StringCodes = StringCodes.Except(sc, New StringCodeComparer).Concat(sc)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "addnew"
                                    Select Case argv.Length
                                        Case 1
                                            Dim sc = TblCharMappingFile.ReadFile(argv(0))
                                            StringCodes = StringCodes.Concat(sc.Except(StringCodes, New StringCodeComparer))
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "replace"
                                    Select Case argv.Length
                                        Case 1
                                            Dim cList = From c In Txt.ReadFile(argv(0)).ToUTF32.Distinct Select (c.ToString())
                                            StringCodes = GenerateMapping(StringCodes, cList, PriorG, NoEmpty, FixCodeRanges)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "removecode"
                                    Select Case argv.Length
                                        Case 2
                                            Dim l = UInt64.Parse(argv(0), Globalization.NumberStyles.HexNumber)
                                            Dim u = UInt64.Parse(argv(1), Globalization.NumberStyles.HexNumber)
                                            StringCodes = StringCodes.Where(Function(sc) (Not sc.HasCodes) OrElse sc.Code < l OrElse sc.Code > u)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "remove"
                                    Select Case argv.Length
                                        Case 1
                                            Dim cSet = New HashSet(Of String)(From c In Txt.ReadFile(argv(0)).ToUTF32.Distinct Select (c.ToString()))
                                            StringCodes = StringCodes.Where(Function(sc) Not (sc.HasUnicodes AndAlso cSet.Contains(sc.UnicodeString)))
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "nullify"
                                    Select Case argv.Length
                                        Case 1
                                            Dim cSet = New HashSet(Of String)(From c In Txt.ReadFile(argv(0)).ToUTF32.Distinct Select (c.ToString()))
                                            StringCodes = StringCodes.Select(
                                                    Function(sc)
                                                        If sc.HasUnicodes AndAlso cSet.Contains(sc.UnicodeString) Then
                                                            Return StringCode.FromCodes(sc.Codes)
                                                        Else
                                                            Return sc
                                                        End If
                                                    End Function
                                                    )
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "removeemptycode"
                                    Select Case argv.Length
                                        Case 0
                                            StringCodes = StringCodes.Where(Function(sc) sc.HasUnicodes)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case "sortcode"
                                    StringCodes = StringCodes.OrderBy(Function(sc) sc.Codes.Count).ThenBy(Function(sc) sc.Codes)
                                Case "save"
                                    Select Case argv.Length
                                        Case 1
                                            TblCharMappingFile.WriteFile(argv(0), StringCodes)
                                        Case Else
                                            Throw New ArgumentException(String.Join(",", opt.Arguments))
                                    End Select
                                Case Else
                                    Throw New ArgumentException(opt.Name)
                            End Select
                            PriorG = False
                            NoEmpty = False
                            FixCodeRanges = New List(Of RangeUInt64)
                    End Select
                Next
            Case 3
                Dim argv = CmdLine.Arguments
                Dim PriorG As Boolean = False
                Dim NoEmpty As Boolean = False
                Dim FixCodeRanges As New List(Of RangeUInt64)
                For Each opt In CmdLine.Options
                    Select Case opt.Name.ToLower
                        Case "g"
                            PriorG = True
                        Case "n"
                            NoEmpty = True
                        Case "fixcode"
                            Dim arg = opt.Arguments
                            Select Case arg.Length
                                Case 2
                                    Dim l = UInt64.Parse(arg(0), Globalization.NumberStyles.HexNumber)
                                    Dim u = UInt64.Parse(arg(1), Globalization.NumberStyles.HexNumber)
                                    FixCodeRanges.Add(New RangeUInt64(l, u))
                                Case Else
                                    Throw New ArgumentException(opt.Name & ":" & String.Join(",", opt.Arguments))
                            End Select
                        Case "removecode"
                            Throw New ArgumentException("removecode选项已改为fixcode选项")
                        Case Else
                            Throw New ArgumentException(opt.Name)
                    End Select
                Next
                Dim StringCodes = TblCharMappingFile.ReadFile(argv(0))
                Dim cList = From c In Txt.ReadFile(argv(1)).ToUTF32.Distinct Select (c.ToString())
                StringCodes = GenerateMapping(StringCodes, cList, PriorG, NoEmpty, FixCodeRanges)
                TblCharMappingFile.WriteFile(argv(2), StringCodes)
            Case Else
                DisplayInfo()
                Return -1
        End Select
        Return 0
    End Function

    <Extension()> Private Function Code(ByVal This As StringCode) As UInt64
        If This.Codes Is Nothing Then Throw New ArgumentNullException
        If This.Codes.Count = 0 Then Throw New ArgumentNullException
        If This.Codes.Count > 8 Then Throw New NotSupportedException

        Dim i As UInt64 = 0
        For Each b In This.Codes
            i = (i << 8) Or b
        Next
        Return i
    End Function

    Public Function GenerateMapping(ByVal StringCodes As IEnumerable(Of StringCode), ByVal Chars As IEnumerable(Of String), ByVal PriorG As Boolean, ByVal NoEmpty As Boolean, ByVal FixCodeRanges As IEnumerable(Of RangeUInt64)) As IEnumerable(Of StringCode)
        Dim SourceStringCodes = StringCodes.ToArray
        Dim Source = SourceStringCodes.Select(Function(sc, i) New KeyValuePair(Of Integer, StringCode)(i, sc)).ToArray

        Dim SourceDict As New Dictionary(Of String, KeyValuePair(Of Integer, StringCode))
        For Each p In Source
            Dim c = p.Value
            If c.HasUnicodes AndAlso Not SourceDict.ContainsKey(c.UnicodeString) Then
                SourceDict.Add(c.UnicodeString, p)
            End If
        Next
        Dim LookupWithUnicode =
                Function(u As String)
                    If SourceDict.ContainsKey(u) Then Return SourceDict(u)
                    Return Nothing
                End Function

        Dim Target = SourceStringCodes.ToArray

        Dim UsedStringCodes As New HashSet(Of StringCode)
        Dim NewChars As New List(Of String)

        For Each r In FixCodeRanges
            For Each p In Source
                Dim StringCode = p.Value
                If Not UsedStringCodes.Contains(StringCode) AndAlso StringCode.HasCodes AndAlso r.Contain(StringCode.Code) Then
                    Target(p.Key) = StringCode
                    UsedStringCodes.Add(StringCode)
                End If
            Next
        Next

        For Each c In Chars
            Dim p = LookupWithUnicode(c)
            Dim StringCode = p.Value
            If StringCode IsNot Nothing AndAlso UsedStringCodes.Contains(StringCode) Then Continue For
            NewChars.Add(c)
        Next
        Chars = NewChars.ToArray
        NewChars.Clear()

        If NoEmpty Then
            For Each p In Source
                Dim StringCode = p.Value
                If Not UsedStringCodes.Contains(StringCode) AndAlso Not StringCode.HasUnicodes Then
                    Target(p.Key) = StringCode
                    UsedStringCodes.Add(StringCode)
                End If
            Next
        End If

        If PriorG Then
            For Each c In Chars
                Dim p = LookupWithUnicode(HanziConverter.G2JOneOnOne(c))
                Dim StringCodeJ = p.Value
                If StringCodeJ IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCodeJ) Then
                    Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCodeJ.CodeString)
                    Target(p.Key) = NewStringCode
                    UsedStringCodes.Add(StringCodeJ)
                    Continue For
                End If
                NewChars.Add(c)
            Next
            Chars = NewChars.ToArray
            NewChars.Clear()

            For Each c In Chars
                Dim p = LookupWithUnicode(c)
                Dim StringCode = p.Value
                If StringCode IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCode) Then
                    Target(p.Key) = StringCode
                    UsedStringCodes.Add(StringCode)
                    Continue For
                End If
                NewChars.Add(c)
            Next
            Chars = NewChars.ToArray
            NewChars.Clear()
        Else
            For Each c In Chars
                Dim p = LookupWithUnicode(c)
                Dim StringCode = p.Value
                If StringCode IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCode) Then
                    Target(p.Key) = StringCode
                    UsedStringCodes.Add(StringCode)
                    Continue For
                End If
                NewChars.Add(c)
            Next
            Chars = NewChars.ToArray
            NewChars.Clear()

            For Each c In Chars
                Dim p = LookupWithUnicode(HanziConverter.G2JOneOnOne(c))
                Dim StringCodeJ = p.Value
                If StringCodeJ IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCodeJ) Then
                    Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCodeJ.CodeString)
                    Target(p.Key) = NewStringCode
                    UsedStringCodes.Add(StringCodeJ)
                    Continue For
                End If
                NewChars.Add(c)
            Next
            Chars = NewChars.ToArray
            NewChars.Clear()
        End If

        For Each c In Chars
            Dim p = LookupWithUnicode(HanziConverter.T2JOneOnOne(c))
            Dim StringCodeJ = p.Value
            If StringCodeJ IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCodeJ) Then
                Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCodeJ.CodeString)
                Target(p.Key) = NewStringCode
                UsedStringCodes.Add(StringCodeJ)
                Continue For
            End If
            NewChars.Add(c)
        Next
        Chars = NewChars.ToArray
        NewChars.Clear()

        For Each c In Chars
            Dim p = LookupWithUnicode(HanziConverter.J2GOneOnOne(c))
            Dim StringCodeJ = p.Value
            If StringCodeJ IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCodeJ) Then
                Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCodeJ.CodeString)
                Target(p.Key) = NewStringCode
                UsedStringCodes.Add(StringCodeJ)
                Continue For
            End If
            NewChars.Add(c)
        Next
        Chars = NewChars.ToArray
        NewChars.Clear()

        For Each c In Chars
            Dim p = LookupWithUnicode(HanziConverter.J2TOneOnOne(c))
            Dim StringCodeJ = p.Value
            If StringCodeJ IsNot Nothing AndAlso Not UsedStringCodes.Contains(StringCodeJ) Then
                Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCodeJ.CodeString)
                Target(p.Key) = NewStringCode
                UsedStringCodes.Add(StringCodeJ)
                Continue For
            End If
            NewChars.Add(c)
        Next
        Chars = NewChars.ToArray
        NewChars.Clear()

        Dim LeftCount As Integer = (From p In Source Where Not UsedStringCodes.Contains(p.Value)).Count

        If Chars.Count > LeftCount Then
            Throw New InvalidDataException("字符数超出{0}个".Formats(Chars.Count - LeftCount))
        End If

        Dim k = Source.Count - 1
        For Each c In Chars.Reverse
            While UsedStringCodes.Contains(Source(k).Value)
                k -= 1
            End While
            Dim p = Source(k)
            Dim StringCode = p.Value
            Dim NewStringCode = StringCode.FromUnicodeStringAndCodeString(c, StringCode.CodeString)
            Target(p.Key) = NewStringCode
            k -= 1
        Next

        Return Target
    End Function
End Module
