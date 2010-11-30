'==========================================================================
'
'  File:        Txt.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 文本文件格式
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Text
Imports Firefly.TextEncoding
Imports Firefly.Streaming

Namespace Texting
    Public NotInheritable Class Txt
        Private Sub New()
        End Sub

        ''' <summary>已重载。检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM，如果失败，返回空。</summary>
        Public Shared Function GetEncodingByBOM(ByVal sp As ZeroPositionStreamPasser) As Encoding
            Dim s = sp.GetStream
            If s.Length >= 4 Then
                s.Position = 0
                Dim BOM As Int32 = s.ReadInt32B
                If BOM = &HFFFE0000 Then Return UTF32
                If BOM = &HFEFF Then Return UTF32B
                If BOM = &H84319533 Then Return GB18030
            End If
            If s.Length >= 3 Then
                s.Position = 0
                Dim BOM As Int32 = s.ReadUInt16B
                BOM = (BOM << 8) Or s.ReadByte
                If BOM = &HEFBBBF Then Return UTF8
            End If
            If s.Length >= 2 Then
                s.Position = 0
                Dim BOM As UInt16 = s.ReadUInt16B
                If BOM = &HFFFEUS Then Return UTF16
                If BOM = &HFEFFUS Then Return UTF16B
            End If
            Return Nothing
        End Function
        ''' <summary>已重载。检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM，如果失败，返回空。</summary>
        Public Shared Function GetEncodingByBOM(ByVal Path As String) As Encoding
            Using s As New StreamEx(Path, FileMode.Open, FileAccess.Read)
                Return GetEncodingByBOM(s)
            End Using
        End Function
        ''' <summary>已重载。检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM，如果失败，返回默认编码。</summary>
        Public Shared Function GetEncoding(ByVal sp As ZeroPositionStreamPasser, ByVal DefaultEncoding As Encoding) As Encoding
            Dim Encoding = GetEncodingByBOM(sp)
            If Encoding IsNot Nothing Then Return Encoding
            Return DefaultEncoding
        End Function
        ''' <summary>已重载。检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM，如果失败，返回默认编码。</summary>
        Public Shared Function GetEncoding(ByVal Path As String, ByVal DefaultEncoding As Encoding) As Encoding
            Using s As New StreamEx(Path, FileMode.Open, FileAccess.Read)
                Return GetEncoding(s, DefaultEncoding)
            End Using
        End Function
        ''' <summary>已重载。检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM，如果失败，返回系统默认编码(GB2312会被替换为GB18030)。</summary>
        Public Shared Function GetEncoding(ByVal Path As String) As Encoding
            Return GetEncoding(Path, TextEncoding.Default)
        End Function

        ''' <param name="DetectEncodingFromByteOrderMarks">如果为真，将检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM。</param>
        Public Shared Function CreateTextReader(ByVal sp As ZeroPositionStreamPasser, ByVal Encoding As Encoding, Optional ByVal DetectEncodingFromByteOrderMarks As Boolean = True) As StreamReader
            Dim s = sp.GetStream
            If DetectEncodingFromByteOrderMarks Then
                If s.Length >= 4 Then
                    s.Position = 0
                    Dim BOM As Int32 = s.ReadInt32B
                    If BOM = &HFFFE0000 Then Return New StreamReader((New PartialStreamEx(s, 4, s.Length - 4, True)).ToUnsafeStream, TextEncoding.UTF32, False)
                    If BOM = &HFEFF Then Return New StreamReader((New PartialStreamEx(s, 4, s.Length - 4, True)).ToUnsafeStream, TextEncoding.UTF32B, False)
                    If BOM = &H84319533 Then Return New StreamReader((New PartialStreamEx(s, 4, s.Length - 4, True)).ToUnsafeStream, TextEncoding.GB18030, False)
                End If
                If s.Length >= 3 Then
                    s.Position = 0
                    Dim BOM As Int32 = s.ReadUInt16B
                    BOM = (BOM << 8) Or s.ReadByte
                    If BOM = &HEFBBBF Then Return New StreamReader((New PartialStreamEx(s, 3, s.Length - 3, True)).ToUnsafeStream, TextEncoding.UTF8, False)
                End If
                If s.Length >= 2 Then
                    s.Position = 0
                    Dim BOM As UInt16 = s.ReadUInt16B
                    If BOM = &HFFFEUS Then Return New StreamReader((New PartialStreamEx(s, 2, s.Length - 2, True)).ToUnsafeStream, TextEncoding.UTF16, False)
                    If BOM = &HFEFFUS Then Return New StreamReader((New PartialStreamEx(s, 2, s.Length - 2, True)).ToUnsafeStream, TextEncoding.UTF16B, False)
                End If
                s.Position = 0
                Return New StreamReader(s.ToUnsafeStream, Encoding, True)
            Else
                Return New StreamReader(s.ToUnsafeStream, Encoding, False)
            End If
        End Function
        ''' <param name="DetectEncodingFromByteOrderMarks">如果为真，将检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM。</param>
        Public Shared Function CreateTextReader(ByVal Path As String, ByVal Encoding As Encoding, Optional ByVal DetectEncodingFromByteOrderMarks As Boolean = True) As StreamReader
            Return CreateTextReader(New StreamEx(Path, FileMode.Open, FileAccess.Read), Encoding, DetectEncodingFromByteOrderMarks)
        End Function
        Public Shared Function CreateTextReader(ByVal Path As String) As StreamReader
            Return CreateTextReader(Path, TextEncoding.Default, True)
        End Function

        Public Shared Function ReadFile(ByVal Reader As StreamReader) As String
            Dim s = Reader
            If Not s.EndOfStream Then Return s.ReadToEnd
            Return ""
        End Function
        ''' <param name="DetectEncodingFromByteOrderMarks">如果为真，将检查UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码的BOM。</param>
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As Encoding, Optional ByVal DetectEncodingFromByteOrderMarks As Boolean = True) As String
            Using s = CreateTextReader(Path, Encoding, DetectEncodingFromByteOrderMarks)
                If Not s.EndOfStream Then Return s.ReadToEnd
            End Using
            Return ""
        End Function
        Public Shared Function ReadFile(ByVal Path As String) As String
            Return ReadFile(Path, TextEncoding.Default)
        End Function

        ''' <param name="WithByteOrderMarks">如果为真，将为UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码写入BOM。</param>
        Public Shared Function CreateTextWriter(ByVal sp As ZeroLengthStreamPasser, ByVal Encoding As Encoding, Optional ByVal WithByteOrderMarks As Boolean = True) As StreamWriter
            Dim s = sp.GetStream
            If WithByteOrderMarks Then
                If Encoding Is UTF16 Then
                    s.WriteByte(&HFF)
                    s.WriteByte(&HFE)
                ElseIf Encoding Is GB18030 Then
                    s.WriteInt32B(&H84319533)
                ElseIf Encoding Is UTF8 Then
                    s.WriteByte(&HEF)
                    s.WriteByte(&HBB)
                    s.WriteByte(&HBF)
                ElseIf Encoding Is UTF32 Then
                    s.WriteByte(&HFF)
                    s.WriteByte(&HFE)
                    s.WriteByte(0)
                    s.WriteByte(0)
                ElseIf Encoding Is UTF16B Then
                    s.WriteByte(&HFE)
                    s.WriteByte(&HFF)
                ElseIf Encoding Is UTF32B Then
                    s.WriteByte(0)
                    s.WriteByte(0)
                    s.WriteByte(&HFE)
                    s.WriteByte(&HFF)
                End If
            End If
            Return New StreamWriter(s, New EncodingNoPreambleWrapper(Encoding))
        End Function
        ''' <param name="WithByteOrderMarks">如果为真，将为UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码写入BOM。</param>
        Public Shared Function CreateTextWriter(ByVal Path As String, ByVal Encoding As System.Text.Encoding, Optional ByVal WithByteOrderMarks As Boolean = True) As StreamWriter
            Return CreateTextWriter(New StreamEx(Path, FileMode.Create, FileAccess.ReadWrite), Encoding, WithByteOrderMarks)
        End Function
        Public Shared Function CreateTextWriter(ByVal Path As String) As StreamWriter
            Return CreateTextWriter(Path, TextEncoding.WritingDefault, True)
        End Function

        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal Value As String)
            Dim s = Writer
            s.Write(Value)
        End Sub
        ''' <param name="WithByteOrderMarks">如果为真，将为UTF-16(FF FE)、GB18030(84 31 95 33)、UTF-8(EF BB BF)、UTF-32(FF FE 00 00)、UTF-16B(FE FF)、UTF-32B(00 00 FE FF)这六种编码写入BOM。</param>
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding, ByVal Value As String, Optional ByVal WithByteOrderMarks As Boolean = True)
            Using s = CreateTextWriter(Path, Encoding, WithByteOrderMarks)
                s.Write(Value)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As String)
            WriteFile(Path, TextEncoding.WritingDefault, Value)
        End Sub
    End Class
End Namespace
