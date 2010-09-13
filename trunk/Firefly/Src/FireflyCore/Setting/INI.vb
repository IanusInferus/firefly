'==========================================================================
'
'  File:        INI.vb
'  Location:    Firefly.Setting <Visual Basic .Net>
'  Description: INI控制类及相关
'  Created:     2004.10.31.09:33:47(GMT+08:00)
'  Version:     0.6 2010.02.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Option Compare Text

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting

Namespace Setting
    ''' <summary>INI控制类</summary>
    ''' <remarks>
    ''' 本类管理INI文件
    ''' 注意 本类初始化时会从文件读取数据,但没有文件也可
    ''' 注意 相同的键只保留后者
    ''' 注意 写函数
    '''
    ''' 注意 本类的字符串支持字符转义
    ''' @ 在字符串前可取消字符转义
    ''' { } 可用表示多行文字 此时自动禁止转义 {必须在有等号那行 }必须是那行的最后一个除空格以外的字符
    ''' $ 在字符串前表示字符串的值从后面的外部文件得到 此时自动禁止转义
    ''' 若@{$连用只有前面的起作用
    ''' ; # // 用作单行注释
    ''' /* */ 用作多行注释
    ''' 没有等号和节格式的行不处理 不推荐作为注释
    ''' \a 与响铃（警报）\u0007 匹配 
    ''' \b 与退格符 \u0008 匹配
    ''' \t 与 Tab 符 \u0009 匹配 
    ''' \r 与回车符 \u000D 匹配
    ''' \v 与垂直 Tab 符 \u000B 匹配
    ''' \f 与换页符 \u000C 匹配
    ''' \n 与换行符 \u000A 匹配
    ''' \x?? 与 Chr(??) 匹配
    ''' \x2F 与 左斜杠符 / 匹配
    ''' \x5C 与 右斜杠符 \ 匹配
    '''
    ''' 本类使用ReadValue来读值 如果没有读出返回False  用New INI(FILE_NAME)得到的实例会自动调用此函数
    ''' 本类使用WriteValue来写入值到内存
    ''' 本类使用ReadFromFile将从文件添入值 如果没有文件可用返回False
    ''' 本类使用WriteToFile将所有改变写入文件 如果没有写入返回False
    ''' </remarks>
    Public Class Ini
        Private FilePath As String
        Private Encoding As System.Text.Encoding

        Private Sections As New List(Of String)
        Private SectionDict As New Dictionary(Of String, Section)(StringComparer.OrdinalIgnoreCase)

        Private Class Section
            Public Entries As New List(Of String)
            Public EntryDict As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        End Class

        Public Sub New()
            FilePath = ""
            Encoding = TextEncoding.WritingDefault
        End Sub
        Public Sub New(ByVal Path As String)
            Me.New(Path, True, Nothing)
        End Sub
        Public Sub New(ByVal Path As String, ByVal Read As Boolean)
            Me.New(Path, Read, Nothing)
        End Sub
        Public Sub New(ByVal Path As String, ByVal Read As Boolean, ByVal Encoding As System.Text.Encoding)
            FilePath = Path
            Me.Encoding = Encoding
            If Read Then ReadFromFile(Path)
        End Sub
        Public Function ReadFromFile(ByVal Path As String) As Boolean
            Return ReadFromFile(Path, Nothing)
        End Function
        Public Function ReadFromFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding) As Boolean
            If Encoding Is Nothing Then Encoding = TextEncoding.Default

            Dim Line As String()
            Try
                Line = Regex.Split(Txt.ReadFile(Path, Encoding), "\r\n|\n")
            Catch
                Return False
            End Try

            Dim CurrentSection As Section = Nothing
            For n As Integer = 0 To Line.GetUpperBound(0)
                '处理节和键
                Dim TempLine = Line(n).Split(New Char() {"="c}, 2)
                TempLine(0) = TempLine(0).Trim(" "c)
                If TempLine.Length > 1 Then
                    TempLine(1) = TempLine(1).TrimStart(" "c)
                End If
                If TempLine.GetLength(0) = 1 Then
                    ' 处理节
                    If TempLine(0).StartsWith("[") AndAlso TempLine(0).EndsWith("]") Then
                        Dim SectionName = TempLine(0).TrimStart("["c).TrimEnd("]"c)
                        If Not SectionDict.ContainsKey(SectionName) Then
                            Sections.Add(SectionName)
                            CurrentSection = New Section
                            SectionDict.Add(SectionName, CurrentSection)
                        Else
                            CurrentSection = SectionDict(SectionName)
                        End If
                    End If
                Else
                    If TempLine(0) = "" Then Continue For
                    If TempLine(0).Contains(";") Then Continue For
                    If TempLine(0).Contains("#") Then Continue For
                    If TempLine(0).Contains("//") Then Continue For
                    If TempLine(0).Contains("/*") Then Continue For
                    If TempLine(0).Contains("*/") Then Continue For
                    If TempLine(1).StartsWith("{") Then
                        Dim s As String = TempLine(1).Remove(0, 1)
                        If s <> "" Then s = s & Environment.NewLine
                        While Not Line(n).EndsWith("}")
                            n += 1
                            If n > Line.GetUpperBound(0) Then Exit While
                            s &= Line(n) & Environment.NewLine
                        End While
                        If s.EndsWith("}" & Environment.NewLine) Then s = s.Substring(0, s.Length - 1 - Environment.NewLine.Length)
                        If s.EndsWith(Environment.NewLine) Then s = s.Substring(0, s.Length - Environment.NewLine.Length)
                        TempLine(1) = "@" & s
                    ElseIf TempLine(1).StartsWith("$") Then
                        Try
                            TempLine(1) = Txt.ReadFile(TempLine(1).Remove(0, 1).Trim(" "c), Encoding)
                        Catch
                            Continue For
                        End Try
                    Else
                        '除去"/*"到"*/"的注释
                        Dim Index As Integer = TempLine(1).IndexOf("/*")
                        If Not Index < 0 Then
                            Dim Index2 As Integer = TempLine(1).IndexOf("*/")
                            If Index2 > Index Then
                                TempLine(1) = TempLine(1).Substring(0, Index - 1 + 1) & TempLine(1).Substring(Index2 + 2)
                            Else
                                TempLine(1) = TempLine(1).Substring(0, Index - 1 + 1)
                                n += 1
                                While Line(n).IndexOf("*/") < 0
                                    n += 1
                                    If n > Line.GetUpperBound(0) Then Exit For
                                End While
                                TempLine(1) &= Line(n).Substring(Line(n).IndexOf("*/") + 2)
                            End If
                        End If
                        '除去单行注释
                        TempLine(1) = TempLine(1).Split(";"c)(0)
                        TempLine(1) = TempLine(1).Split("#"c)(0)
                        Index = TempLine(1).IndexOf("//")
                        If Not (Index < 0) Then TempLine(1) = TempLine(1).Substring(0, Index)
                    End If
                    If Not TempLine(1).StartsWith("@") Then TempLine(1) = TempLine(1).Trim(" "c)

                    '处理键
                    Dim Key = TempLine(0)
                    Dim Value = TempLine(1)
                    If CurrentSection.EntryDict.ContainsKey(Key) Then
                        CurrentSection.EntryDict(Key) = Value
                    Else
                        CurrentSection.Entries.Add(Key)
                        CurrentSection.EntryDict.Add(Key, Value)
                    End If
                End If
            Next
            Return True
        End Function

        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As String) As Boolean
            If Not SectionDict.ContainsKey(SectionName) Then Return False
            Dim CurrentSection = SectionDict(SectionName)

            If Not CurrentSection.EntryDict.ContainsKey(Key) Then Return False
            Dim TempString As String = CurrentSection.EntryDict(Key)

            If Not TempString.StartsWith("@") Then
                TempString = TempString.Descape
                Value = TempString
            Else
                Value = TempString.Remove(0, 1)
            End If
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Decimal) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Decimal.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Single) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Single.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Double) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Double.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Boolean) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Boolean.Parse(TempString)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Byte) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Byte.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As SByte) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = SByte.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Int16) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Int16.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As UInt16) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = UInt16.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Int32) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Int32.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As UInt32) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = UInt32.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As Int64) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = Int64.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As UInt64) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Try
                Value = UInt64.Parse(TempString, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As String()) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            Dim Temp As String()
            If Not TempString.StartsWith("@") Then
                If TempString = "" Then Return True
                Temp = TempString.Split(","c)
                If Value Is Nothing OrElse Value.GetUpperBound(0) <> Temp.GetUpperBound(0) Then Value = New String(Temp.GetUpperBound(0)) {}
                For n As Integer = 0 To Math.Min(Value.GetUpperBound(0), Temp.GetUpperBound(0))
                    Temp(n) = Temp(n).Descape
                    Value(n) = Temp(n)
                Next
            Else
                TempString = TempString.Remove(0, 1)
                If TempString = "" Then Return True
                Temp = TempString.Split(","c)
                If Value Is Nothing OrElse Value.GetUpperBound(0) <> Temp.GetUpperBound(0) Then Value = New String(Temp.GetUpperBound(0)) {}
                For n As Integer = 0 To Math.Min(Value.GetUpperBound(0), Temp.GetUpperBound(0))
                    Value(n) = Temp(n).TrimStart(ChrQ(13)).TrimStart(ChrQ(10)).TrimEnd(ChrQ(10)).TrimEnd(ChrQ(13))
                Next
            End If
            Return True
        End Function
        Public Function ReadValue(ByVal SectionName As String, ByVal Key As String, ByRef Value As String(,)) As Boolean
            Dim TempString As String = Nothing
            If Not ReadValue(SectionName, Key, TempString) Then Return False
            If Value Is Nothing Then Return False
            Dim TempLine As String()
            Dim Temp As String()
            If Not TempString.StartsWith("@") Then
                If TempString = "" Then Return True
                TempLine = Regex.Split(TempString, "\r\n|\n")
                For n As Integer = 0 To Math.Min(Value.GetUpperBound(1), TempLine.GetUpperBound(0))
                    Temp = TempLine(n).Split(","c)
                    For m As Integer = 0 To Math.Min(Value.GetUpperBound(0), Temp.GetUpperBound(0))
                        Temp(m) = Temp(m).Descape
                        Value(m, n) = Temp(m)
                    Next
                Next
            Else
                TempString = TempString.Remove(0, 1)
                If TempString = "" Then Return True
                TempLine = Regex.Split(TempString, "\r\n|\n")
                For n As Integer = 0 To Math.Min(Value.GetUpperBound(1), TempLine.GetUpperBound(0))
                    Temp = TempLine(n).Split(","c)
                    For m As Integer = 0 To Math.Min(Value.GetUpperBound(0), Temp.GetUpperBound(0))
                        Value(m, n) = Temp(m).TrimStart(ChrQ(13)).TrimStart(ChrQ(10)).TrimEnd(ChrQ(10)).TrimEnd(ChrQ(13))
                    Next
                Next
            End If
            Return True
        End Function
        Public Sub WriteValue(ByVal SectionName As String, ByVal Key As String, ByVal Value As String, ByVal TransferMeaning As Boolean)
            Dim CurrentSection As Section
            If SectionDict.ContainsKey(SectionName) Then
                CurrentSection = SectionDict(SectionName)
            Else
                Sections.Add(SectionName)
                CurrentSection = New Section
                SectionDict.Add(SectionName, CurrentSection)
            End If

            Dim TempString As String = ""
            If Value <> "" Then TempString = Value
            If TransferMeaning Then
                TempString = TempString.Replace(";", "\x3B")
                TempString = TempString.Replace("#", "\x23")
                TempString = TempString.Replace("//", "\x2F\x2F")
                TempString = TempString.Replace("/*", "\x2F\x2A")
                TempString = TempString.Replace("*/", "\x2A\x2F")
                TempString = TempString.Replace("{", "\x7B")
                TempString = TempString.Replace("}", "\x7D")
                TempString = TempString.Replace("@", "\x40")
                TempString = TempString.Replace("$", "\x24")
                TempString = TempString.Replace("\", "\x5C")
                TempString = TempString.Replace("\a".Descape, "\a")
                TempString = TempString.Replace("\b".Descape, "\b")
                TempString = TempString.Replace("\t".Descape, "\t")
                TempString = TempString.Replace("\r".Descape, "\r")
                TempString = TempString.Replace("\v".Descape, "\v")
                TempString = TempString.Replace("\f".Descape, "\f")
                TempString = TempString.Replace("\n".Descape, "\n")
                TempString = TempString.Replace("\b".Descape, "\b")
            Else
                TempString = "@" & TempString
            End If

            Key = Key.Trim(" "c)
            If CurrentSection.EntryDict.ContainsKey(Key) Then
                CurrentSection.EntryDict(Key) = TempString
            Else
                CurrentSection.Entries.Add(Key)
                CurrentSection.EntryDict.Add(Key, TempString)
            End If
        End Sub
        Public Sub WriteValue(ByVal SectionName As String, ByVal Key As String, ByVal Value As String)
            If Value = "" OrElse Not (Value.Contains("\") OrElse Value.Contains("/")) Then
                WriteValue(SectionName, Key, Value, True)
            Else
                WriteValue(SectionName, Key, Value, False)
            End If
        End Sub
        Public Sub WriteValue(ByVal SectionName As String, ByVal Key As String, ByVal Value As String())
            If Value Is Nothing Then Return
            WriteValue(SectionName, Key, String.Join(",", Value))
        End Sub
        Public Sub WriteValue(ByVal SectionName As String, ByVal Key As String, ByVal Value As String(,))
            If Value Is Nothing Then Return
            WriteValue(SectionName, Key, String.Join(Environment.NewLine, (From y In Enumerable.Range(0, Value.GetLength(1)) Select String.Join(",", (From x In Enumerable.Range(0, Value.GetLength(0)) Select Value(x, y)).ToArray)).ToArray), False)
        End Sub
        Public Function WriteToFile(Optional ByVal Comment As String = "") As Boolean
            Dim Lines As New List(Of String)
            For Each SectionName In Sections
                Dim CurrentSection = SectionDict(SectionName)
                Lines.Add("[" & SectionName & "]")
                For Each Key In CurrentSection.Entries
                    Dim Temp = CurrentSection.EntryDict(Key)
                    If Temp.StartsWith("@") AndAlso Temp.Contains(Lf) Then
                        Lines.Add(Key & " = " & "{" & Environment.NewLine & Temp.Remove(0, 1) & Environment.NewLine & "}")
                    Else
                        Lines.Add(Key & " = " & Temp)
                    End If
                Next
                Lines.Add("")
            Next

            Dim StringToWrite = String.Join(Environment.NewLine, Lines.ToArray)

            If Comment <> "" Then
                Dim TempString As String = Comment
                TempString = TempString.Replace("\a".Descape, "\a")
                TempString = TempString.Replace("\b".Descape, "\b")
                TempString = TempString.Replace("\t".Descape, "\t")
                TempString = TempString.Replace("\v".Descape, "\v")
                TempString = TempString.Replace("\f".Descape, "\f")
                TempString = TempString.Replace("\b".Descape, "\b")
                StringToWrite = Comment & Environment.NewLine & StringToWrite
            End If

            Dim Encoding = Me.Encoding
            If Encoding Is Nothing Then Encoding = TextEncoding.WritingDefault
            Try
                Dim dir As String = GetFileDirectory(FilePath)
                If dir <> "" AndAlso Not IO.Directory.Exists(dir) Then IO.Directory.CreateDirectory(dir)
                Txt.WriteFile(FilePath, Encoding, StringToWrite)
            Catch
                Return False
            End Try
            Return True
        End Function
        Public Sub SetFileName(ByVal Path As String)
            FilePath = Path
        End Sub
    End Class

    ''' <summary>INI本地化类</summary>
    ''' <remarks>
    ''' 本类管理以INI形式存储的本地化字符串
    ''' 需要的本地化文件格式样例如下：
    ''' 
    ''' Program.en.ini
    ''' [en]
    ''' Text1 = Hello
    ''' Text2 = Welldone
    ''' Text3 = Color
    ''' 
    ''' [en-GB]
    ''' Text3 = Colour
    ''' 
    ''' Program.zh-CN.ini
    ''' [zh-CN]
    ''' Text1 = 你好
    ''' Text2 = 棒极了
    ''' Text3 = 颜色
    ''' 
    ''' </remarks>
    Public Class IniLocalization
        Protected LanRes As Ini
        Protected LanFull As String
        ReadOnly Property LanguageIndentiySignFull() As String
            Get
                Return LanFull
            End Get
        End Property
        Protected LanParent As String
        ReadOnly Property LanguageIndentiySignParent() As String
            Get
                Return LanParent
            End Get
        End Property
        Protected LanNative As String
        ReadOnly Property LanguageIndentiySignNative() As String
            Get
                Return LanNative
            End Get
        End Property
        Protected LanDefault As String
        ReadOnly Property DefaultLanguageIndentiySign() As String
            Get
                Return LanDefault
            End Get
        End Property
        Sub New(ByVal LanguageFileMainName As String, ByVal Language As String, Optional ByVal DefaultLanguage As String = "en", Optional ByVal Encoding As System.Text.Encoding = Nothing)
            LanFull = Language
            LanDefault = DefaultLanguage

            If LanFull = "" Then LanFull = Globalization.CultureInfo.InstalledUICulture.Name
            Dim Index As Integer = LanFull.IndexOf("-")
            If Index < 0 Then
                LanParent = LanFull
                LanNative = ""
                LanParent = LanParent.ToLower
                LanFull = LanParent
            Else
                LanParent = LanFull.Substring(0, Index)
                LanNative = LanFull.Substring(Index + 1)
                LanParent = LanParent.ToLower
                LanNative = LanNative.ToUpper
                LanFull = LanParent & "-" & LanNative
            End If

            LanRes = New Ini
            LanRes.ReadFromFile(LanguageFileMainName & "." & DefaultLanguage & ".ini", Encoding)
            LanRes.ReadFromFile(LanguageFileMainName & "." & LanParent & ".ini", Encoding)
            LanRes.ReadFromFile(LanguageFileMainName & "." & LanFull & ".ini", Encoding)
        End Sub
        Sub ReadValue(ByVal Key As String, ByRef Value As String)
            LanRes.ReadValue(LanDefault, Key, Value)
            LanRes.ReadValue(LanParent, Key, Value)
            LanRes.ReadValue(LanFull, Key, Value)
        End Sub
    End Class
End Namespace
