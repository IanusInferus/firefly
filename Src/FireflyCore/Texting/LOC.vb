'==========================================================================
'
'  File:        LOC.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: LOC文件格式类(版本2)(图形文本)
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Glyphing

Namespace Texting
    Public Class LOCText
        Public Font As IEnumerable(Of IGlyph)
        Public Text As IEnumerable(Of IEnumerable(Of StringCode))
    End Class

    Public Enum LOCVersion
        LOC1
        LOC2
    End Enum

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
                        Throw New NotImplementedException
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

        Public Shared Sub WriteFile(ByVal sp As ZeroLengthStreamPasser, ByVal Value As LOCText, Optional ByVal Compress As Boolean = False, Optional ByVal Version As LOCVersion = LOCVersion.LOC2)
            Dim f As StreamEx = sp.GetStream
            Using s As New StreamEx
                Select Case Version
                    Case LOCVersion.LOC1
                        Throw New NotImplementedException
                    Case LOCVersion.LOC2
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
                    Case Else
                        Throw New NotSupportedException
                End Select

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
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Value As LOCText, Optional ByVal Compress As Boolean = False, Optional ByVal Version As LOCVersion = LOCVersion.LOC2)
            Using s As New StreamEx(Path, FileMode.Create, FileAccess.ReadWrite)
                WriteFile(s, Value, Compress, Version)
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
                ElseIf sl.Count > 0 Then
                    Dim s = String.Join("", sl.ToArray)
                    l.AddRange(From c In s.ToUTF32 Select StringCode.FromUnicodeChar(c))
                    sl.Clear()
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
                If c.HasCodes Then
                    l.Add("\g{" & c.CodeString & "}")
                Else
                    l.Add(c.UnicodeString)
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
        Private Shared Normal As String = "(?<Normal>.)"

        Private Shared r As New Regex("^" & "(" & SingleEscapes & "|" & CustomCodeEscapes & "|" & UnicodeEscapes & "|" & ErrorEscapes & "|" & Normal & ")*" & "$", RegexOptions.ExplicitCapture)
    End Class
End Namespace
