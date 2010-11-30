'==========================================================================
'
'  File:        LOC.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: LOC文件格式类(版本2)(图形文本)
'  Version:     2010.11.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Drawing
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Imaging
Imports Firefly.Glyphing

Namespace Texting
    Public Class LOCText
        Public Font As IEnumerable(Of IGlyph)
        Public Text As IEnumerable(Of IEnumerable(Of StringCode))
    End Class

    ''' <summary>LOC文件格式类(版本2)(图形文本)</summary>
    ''' <remarks>本类兼容LOC版本1格式的读取，但不能生成LOC版本1格式的文件。</remarks>
    Public Class LOC
        Private Sub New()
        End Sub

        Private Shared ReadOnly IdentifierCompression As String = "LOCC"
        Private Shared ReadOnly Identifier1 As String = "LOC1"
        Private Shared ReadOnly Identifier2 As String = "LOC2"

        Private Shared ReadOnly IdentifierFd As String = "FD"
        Private Shared ReadOnly IdentifierBmp As String = "BMP"
        Private Shared ReadOnly IdentifierAgemo As String = "AGEM"

        Public Shared Function ReadFile(ByVal sp As ZeroPositionStreamPasser) As LOCText
            Dim sInput = sp.GetStream

            Dim Identifier As String = sInput.ReadSimpleString(4)
            Using s As New StreamEx
                If Identifier = IdentifierCompression Then
                    Using gz As New PartialStreamEx(sInput, 4, sInput.Length - 4)
                        Using gzDec As New IO.Compression.GZipStream(gz.ToUnsafeStream, IO.Compression.CompressionMode.Decompress, False)
                            While True
                                Dim b As Int32 = gzDec.ReadByte
                                If b = -1 Then Exit While
                                s.WriteByte(CByte(b))
                            End While
                        End Using
                    End Using
                Else
                    sInput.Position = 0
                    s.WriteFromStream(sInput, sInput.Length)
                End If
                s.Position = 0

                Identifier = s.ReadSimpleString(4)

                Select Case Identifier
                    Case Identifier1
                        Dim NumSection As Int32 = s.ReadInt32
                        If NumSection < 2 Then Throw New InvalidDataException

                        Dim FontLibSectionAddress = s.ReadInt32
                        Dim FontLibSectionLength = s.ReadInt32
                        Dim TextAddress = s.ReadInt32
                        Dim TextLength = s.ReadInt32

                        Dim Font As New List(Of IGlyph)
                        Dim Text As New List(Of IEnumerable(Of StringCode))

                        Dim CharGlyphDictValue As New Dictionary(Of StringCode, Int32)

                        s.Position = FontLibSectionAddress
                        If FontLibSectionLength <= 0 Then Throw New InvalidDataException

                        Dim CharCount As Int32 = s.ReadInt32
                        Dim CharCode = New StringCode(CharCount - 1) {}
                        Dim CharInfoDBLength As Int32 = s.ReadInt32 '暂时不用
                        For n As Integer = 0 To CharCount - 1
                            Dim Index = s.ReadInt32
                            If Index <> n Then Throw New InvalidDataException

                            Dim GlyphIndex = s.ReadInt32
                            Dim Unicode = s.ReadInt32
                            Dim Code = s.ReadInt32

                            CharCode(n) = StringCode.FromUnicodesAndCodes(UnicodeInt32ToUnicodes(Unicode), CodeInt32ToCodes(Code))

                            If GlyphIndex <> -1 Then CharGlyphDictValue.Add(CharCode(n), GlyphIndex)
                        Next

                        Dim GlyphCount = s.ReadInt32
                        Dim GlyphWidth = s.ReadInt32
                        Dim GlyphHeight = s.ReadInt32
                        Dim WidthTableLength As Int32 = s.ReadInt32
                        Dim WidthTable = New Byte(GlyphCount - 1) {}
                        If WidthTableLength > 0 Then
                            For n As Integer = 0 To GlyphCount - 1
                                WidthTable(n) = s.ReadByte
                            Next
                            s.Position += WidthTableLength - GlyphCount
                        End If
                        s.Position = ((s.Position + 15) \ 16) * 16
                        Dim BitmapLength As Int32 = s.ReadInt32
                        If BitmapLength > 0 Then
                            Dim HoldPosition = s.Position
                            Using BitmapStream As New PartialStreamEx(s, s.Position, BitmapLength)
                                Using Bitmap = Bmp.Open(BitmapStream)
                                    Dim NumGlyphInLine = Bitmap.Width \ GlyphWidth
                                    Dim GetGlyphVirtualBox = Function(GlyphIndex As Integer) New Rectangle(0, 0, WidthTable(GlyphIndex), GlyphHeight)
                                    Dim GetGlyphPhysicalBox = Function(GlyphIndex As Integer) New Rectangle((GlyphIndex Mod NumGlyphInLine) * GlyphWidth, (GlyphIndex \ NumGlyphInLine) * GlyphHeight, GlyphWidth, GlyphHeight)

                                    For Each c In CharCode
                                        If CharGlyphDictValue.ContainsKey(c) Then
                                            Dim r = GetGlyphPhysicalBox(CharGlyphDictValue(c))
                                            Font.Add(New Glyph With {.c = c, .Block = Bitmap.GetRectangleAsARGB(r.X, r.Y, r.Width, r.Height), .VirtualBox = GetGlyphVirtualBox(CharGlyphDictValue(c))})
                                        End If
                                    Next
                                End Using
                            End Using
                            s.Position = HoldPosition + BitmapLength
                        End If

                        s.Position = TextAddress
                        Dim TextCount As Int32 = s.ReadInt32
                        Dim TextInfoDBLength As Int32 = s.ReadInt32 '暂时不用

                        Dim TextInfoAddress As Int32() = New Int32(TextCount - 1) {}
                        Dim TextInfoLength As Int32() = New Int32(TextCount - 1) {}
                        For n As Integer = 0 To TextCount - 1
                            TextInfoAddress(n) = s.ReadInt32
                            TextInfoLength(n) = s.ReadInt32
                        Next
                        Dim TextCharIndex = New Int32(TextCount - 1)() {}
                        For n As Integer = 0 To TextCount - 1
                            s.Position = TextAddress + TextInfoAddress(n)
                            Dim VLEData As Byte() = s.Read(TextInfoLength(n))
                            TextCharIndex(n) = VariableLengthDecode(VLEData)
                        Next
                        For n As Integer = 0 To TextCount - 1
                            Dim Original = TextCharIndex(n)
                            Dim SingleText = New StringCode(TextCharIndex(n).Length - 1) {}
                            For k As Integer = 0 To TextCharIndex(n).Length - 1
                                SingleText(k) = CharCode(Original(k))
                            Next
                            Text.Add(SingleText)
                        Next

                        Return New LOCText With {.Font = Font, .Text = Text}
                    Case Identifier2
                        Dim NumSection = s.ReadInt32
                        Dim StreamDict As New Dictionary(Of String, StreamEx)
                        Try
                            For n = 0 To NumSection - 1
                                Dim SectionIdentifier = s.ReadSimpleString(4)
                                Dim Address = s.ReadInt32
                                Dim Length = s.ReadInt32

                                Select Case SectionIdentifier
                                    Case IdentifierFd, IdentifierBmp, "AGEM"
                                        Dim HoldPosition = s.Position
                                        s.Position = Address
                                        Dim SectionStream As New StreamEx
                                        Try
                                            SectionStream.WriteFromStream(s, Length)
                                            SectionStream.Position = 0
                                        Finally
                                            StreamDict.Add(SectionIdentifier, SectionStream)
                                        End Try
                                        s.Position = HoldPosition
                                    Case Else

                                End Select
                            Next

                            Dim Font As IEnumerable(Of IGlyph) = Nothing
                            Dim IsContainFD = StreamDict.ContainsKey(IdentifierFd)
                            Dim IsContainBMP = StreamDict.ContainsKey(IdentifierBmp)
                            If IsContainFD OrElse IsContainBMP Then
                                If IsContainFD AndAlso IsContainBMP Then
                                    Dim FdStream = StreamDict(IdentifierFd)
                                    Dim BmpStream = StreamDict(IdentifierBmp)
                                    Using FdReader = Txt.CreateTextReader(FdStream, TextEncoding.Default)
                                        Using ImageReader As New BmpFontImageReader(BmpStream)
                                            Font = FdGlyphDescriptionFile.ReadFont(FdReader, ImageReader)
                                        End Using
                                    End Using
                                Else
                                    Throw New InvalidDataException
                                End If
                            End If

                            Dim Text As IEnumerable(Of IEnumerable(Of StringCode)) = Nothing
                            If StreamDict.ContainsKey(IdentifierAgemo) Then
                                Dim AgemoStream = StreamDict(IdentifierAgemo)
                                Using AgemoReader = Txt.CreateTextReader(AgemoStream, TextEncoding.Default)
                                    Dim AgemoText = Agemo.ReadFile(AgemoReader)
                                    Text = (From str In AgemoText Select Descape(str)).ToArray
                                End Using
                            End If

                            Return New LOCText With {.Font = Font, .Text = Text}
                        Finally
                            For Each sPair In StreamDict
                                Try
                                    sPair.Value.Dispose()
                                Catch
                                End Try
                            Next
                        End Try
                    Case Else
                        Throw New NotSupportedException
                End Select
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Path As String) As LOCText
            Using s As New StreamEx(Path, FileMode.Open, FileAccess.Read)
                Return ReadFile(s)
            End Using
        End Function

        Public Shared Sub WriteFile(ByVal sp As ZeroLengthStreamPasser, ByVal Value As LOCText, Optional ByVal Compress As Boolean = False)
            Dim f As StreamEx = sp.GetStream
            Using s As New StreamEx
                Dim Font = Value.Font
                Dim Text = Value.Text

                Dim StreamList As New List(Of KeyValuePair(Of String, StreamEx))
                Try
                    If Font IsNot Nothing Then
                        Dim FdStream As New StreamEx
                        Dim BmpStream As New StreamEx
                        Try
                            Using FdProtecter As New PartialStreamEx(FdStream, 0, Int64.MaxValue, FdStream.Length)
                                Using FdWriter = Txt.CreateTextWriter(FdProtecter, TextEncoding.UTF16)
                                    Using ImageProtecter As New PartialStreamEx(BmpStream, 0, Int64.MaxValue, BmpStream.Length)
                                        Using ImageWriter As New BmpFontImageWriter(ImageProtecter)
                                            FdGlyphDescriptionFile.WriteFont(FdWriter, Font, ImageWriter)
                                        End Using
                                    End Using
                                End Using
                            End Using
                        Finally
                            StreamList.Add(New KeyValuePair(Of String, StreamEx)(IdentifierFd, FdStream))
                            StreamList.Add(New KeyValuePair(Of String, StreamEx)(IdentifierBmp, BmpStream))
                        End Try
                    End If

                    If Text IsNot Nothing Then
                        Dim AgemoText = (From str In Text Select Escape(str)).ToArray
                        Dim AgemoStream As New StreamEx
                        Try
                            Using AgemoProtecter As New PartialStreamEx(AgemoStream, 0, Int64.MaxValue, AgemoStream.Length)
                                Using AgemoWriter = Txt.CreateTextWriter(AgemoProtecter, TextEncoding.UTF16)
                                    Agemo.WriteFile(AgemoWriter, AgemoText)
                                End Using
                            End Using
                        Finally
                            StreamList.Add(New KeyValuePair(Of String, StreamEx)(IdentifierAgemo, AgemoStream))
                        End Try
                    End If

                    Dim NumSection = StreamList.Count
                    Dim Address = 8 + 12 * NumSection
                    Address = ((Address + 15) \ 16) * 16
                    s.SetLength(Address)
                    s.WriteSimpleString(Identifier2, 4)
                    s.WriteInt32(NumSection)
                    For Each sPair In StreamList
                        Dim ss = sPair.Value
                        s.WriteSimpleString(sPair.Key, 4)
                        s.WriteInt32(Address)
                        Dim Length = ss.Length
                        Dim Space = ((Length + 15) \ 16) * 16
                        s.WriteInt32(Length)

                        Dim HoldPosition = s.Position
                        s.Position = Address
                        ss.Position = 0
                        s.WriteFromStream(ss, Length)
                        For n = Length To Space - 1
                            s.WriteByte(0)
                        Next
                        s.Position = HoldPosition

                        Address += Space
                    Next
                Finally
                    For Each sPair In StreamList
                        Try
                            sPair.Value.Dispose()
                        Catch
                        End Try
                    Next
                End Try

                s.Position = 0
                If Not Compress Then
                    f.WriteFromStream(s, s.Length)
                Else
                    f.WriteSimpleString(IdentifierCompression, 4)
                    Using ps As New PartialStreamEx(f, 4, Int64.MaxValue)
                        Using gz As New IO.Compression.GZipStream(ps, Compression.CompressionMode.Compress, True)
                            gz.Write(s.Read(CInt(s.Length)), 0, CInt(s.Length))
                        End Using
                    End Using
                End If
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As LOCText, Optional ByVal Compress As Boolean = False)
            Using s As New StreamEx(Path, FileMode.Create, FileAccess.ReadWrite)
                WriteFile(s, Value, Compress)
            End Using
        End Sub

        Private Shared Function Descape(ByVal This As String) As IEnumerable(Of StringCode)
            Dim m = r.Match(This)
            If Not m.Success Then Throw New InvalidCastException

            Dim ss As New SortedList(Of Integer, String)
            Dim sg As New Dictionary(Of Integer, StringCode)
            For Each c As Capture In m.Groups.Item("SingleEscape").Captures
                ss.Add(c.Index, SingleEscapeDict(c.Value))
            Next
            For Each c As Capture In m.Groups.Item("CustomCodeEscape").Captures
                ss.Add(c.Index, Nothing)
                sg.Add(c.Index, StringCode.FromCodeString(c.Value))
            Next
            For Each c As Capture In m.Groups.Item("UnicodeEscape").Captures
                ss.Add(c.Index, ChrQ(Int32.Parse(c.Value, Globalization.NumberStyles.HexNumber)))
            Next
            For Each c As Capture In m.Groups.Item("ErrorEscape").Captures
                Throw New ArgumentException("ErrorEscape: Ch " & (c.Index + 1) & " " & c.Value)
            Next
            For Each c As Capture In m.Groups.Item("Normal").Captures
                ss.Add(c.Index, c.Value)
            Next

            Dim l As New List(Of StringCode)
            Dim sl As New List(Of String)
            For Each p In ss
                If p.Value IsNot Nothing Then
                    sl.Add(p.Value)
                Else
                    If sl.Count > 0 Then
                        Dim s = String.Join("", sl.ToArray)
                        l.AddRange(From c In s.ToUTF32 Select StringCode.FromUnicodeChar(c))
                        sl.Clear()
                    End If
                    l.Add(sg(p.Key))
                End If
            Next
            If sl.Count > 0 Then
                Dim s = String.Join("", sl.ToArray)
                l.AddRange(From c In s.ToUTF32 Select StringCode.FromUnicodeChar(c))
                sl.Clear()
            End If

            Return l
        End Function
        Private Shared Function Escape(ByVal This As IEnumerable(Of StringCode)) As String
            Dim l As New List(Of String)
            For Each c In This
                If c.HasUnicodes Then
                    l.Add(c.UnicodeString.Replace("\", "\\"))
                ElseIf c.HasCodes Then
                    l.Add("\g{" & c.CodeString & "}")
                Else
                    Throw New InvalidDataException
                End If
            Next
            Return String.Join("", l.ToArray)
        End Function

        Private Shared ReadOnly Property SingleEscapeDict() As Dictionary(Of String, String)
            Get
                Static d As Dictionary(Of String, String)
                If d IsNot Nothing Then Return d
                d = New Dictionary(Of String, String)
                d.Add("\", "\") 'backslash
                d.Add("0", ChrQ(0)) 'null
                d.Add("a", ChrQ(7)) 'alert (beep)
                d.Add("b", ChrQ(8)) 'backspace
                d.Add("f", ChrQ(&HC)) 'form feed
                d.Add("n", ChrQ(&HA)) 'newline (lf)
                d.Add("r", ChrQ(&HD)) 'carriage return (cr) 
                d.Add("t", ChrQ(9)) 'horizontal tab 
                d.Add("v", ChrQ(&HB)) 'vertical tab
                Return d
            End Get
        End Property
        Private Shared ReadOnly Property SingleEscapes() As String
            Get
                Static s As String
                If s IsNot Nothing Then Return s
                Dim Chars As New List(Of String)
                For Each c In "\0abfnrtv"
                    Chars.Add(Regex.Escape(c))
                Next
                s = "\\(?<SingleEscape>" & String.Join("|", Chars.ToArray) & ")"
                Return s
            End Get
        End Property
        Private Shared CustomCodeEscapes As String = "\\g\{(?<CustomCodeEscape>[0-9A-Fa-f]+)\}"
        Private Shared UnicodeEscapes As String = "\\U(?<UnicodeEscape>[0-9A-Fa-f]{5})|\\u(?<UnicodeEscape>[0-9A-Fa-f]{4})|\\x(?<UnicodeEscape>[0-9A-Fa-f]{2})"
        Private Shared ErrorEscapes As String = "(?<ErrorEscape>\\)"
        Private Shared Normal As String = "(?<Normal>.|\r|\n)"

        Private Shared r As New Regex("^" & "(" & SingleEscapes & "|" & CustomCodeEscapes & "|" & UnicodeEscapes & "|" & ErrorEscapes & "|" & Normal & ")*" & "$", RegexOptions.ExplicitCapture)

        Private Shared Function VariableLengthEncode(ByVal Original As Int32()) As Byte()
            Dim l As New List(Of Byte)
            For Each i In Original
                While (i And Not &H7F) <> 0
                    l.Add(CByte((i And &H7F) Or &H80))
                    i >>= 7
                End While
                l.Add(CByte(i))
            Next
            Return l.ToArray
        End Function
        Private Shared Function VariableLengthDecode(ByVal Encoded As Byte()) As Int32()
            Dim l As New List(Of Int32)
            Dim i As Int32
            Dim p As Int32
            For Each b In Encoded
                i = i Or ((CInt(b) And &H7F) << p)
                If (b And &H80) <> 0 Then
                    p += 7
                Else
                    p = 0
                    l.Add(i)
                    i = 0
                End If
            Next
            Return l.ToArray
        End Function

        Private Shared Function CodeInt32ToCodes(ByVal Code As Int32) As StringEx(Of Byte)
            Select Case Code
                Case -1
                    Return Nothing
                Case 0 To &HFF
                    Return New StringEx(Of Byte)(New Byte() {Code.Bits(7, 0)})
                Case &H100 To &HFFFF
                    Return New StringEx(Of Byte)(New Byte() {Code.Bits(7, 0), Code.Bits(15, 8)})
                Case &H10000 To &HFFFFFF
                    Return New StringEx(Of Byte)(New Byte() {Code.Bits(7, 0), Code.Bits(15, 8), Code.Bits(23, 16)})
                Case Else
                    Return New StringEx(Of Byte)(New Byte() {Code.Bits(7, 0), Code.Bits(15, 8), Code.Bits(23, 16), Code.Bits(31, 24)})
            End Select
        End Function
        Private Shared Function CodesToCodeInt32(ByVal Codes As StringEx(Of Byte)) As Int32
            If Codes Is Nothing Then Return -1
            Select Case Codes.Count
                Case 0
                    Return -1
                Case 1
                    Return Codes(0)
                Case 2
                    Return ConcatBits(Codes(1), 8, Codes(0), 8)
                Case 3
                    Return ConcatBits(Codes(2), 8, Codes(1), 8, Codes(0), 8)
                Case 4
                    Return ConcatBits(Codes(3), 8, Codes(2), 8, Codes(1), 8, Codes(0), 8)
                Case Else
                    Throw New ArgumentException
            End Select
        End Function

        Private Shared Function UnicodeInt32ToUnicodes(ByVal Unicode As Int32) As StringEx(Of Char32)
            If Unicode = -1 Then Return Nothing
            Return New StringEx(Of Char32)(New Char32() {New Char32(Unicode)})
        End Function
        Private Shared Function UnicodesToUnicodeInt32(ByVal Unicodes As StringEx(Of Char32)) As Int32
            If Unicodes Is Nothing Then Return -1
            If Unicodes.Count = 0 Then Return -1
            If Unicodes.Count <> 1 Then Throw New ArgumentException
            Return Unicodes(0)
        End Function
    End Class
End Namespace
