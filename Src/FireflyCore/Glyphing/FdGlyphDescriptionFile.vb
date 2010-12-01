'==========================================================================
'
'  File:        FdGlyphDescriptionFile.vb
'  Location:    Firefly.TextEncoding <Visual Basic .Net>
'  Description: fd字形描述文件
'  Version:     2010.10.30.
'  Copyright(C) F.R.C.
'
'==========================================================================

Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Drawing
Imports System.IO
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Texting
Imports Firefly.Imaging

Namespace Glyphing
    Public NotInheritable Class FdGlyphDescriptionFile
        Private Sub New()
        End Sub

        Private Shared Function ReadFile(ByVal Reader As StreamReader, ByVal GetFormatException As Func(Of Integer, Exception)) As IEnumerable(Of GlyphDescriptor)
            Dim d As New List(Of GlyphDescriptor)
            Dim s = Reader
            Dim r As New Regex("^U\+(?<Unicode>[0-9A-Fa-f]+)$", RegexOptions.ExplicitCapture)
            Dim LineNumber As Integer = 1
            While Not s.EndOfStream
                Dim Line = s.ReadLine
                If Line.Trim <> "" Then
                    Dim Values = Line.Split(","c)
                    If Values.Length <> 10 Then Throw GetFormatException(LineNumber)

                    Dim Unicodes As New List(Of Char32)
                    If Not Regex.Match(Values(0), "^ *$").Success Then
                        For Each p In Regex.Split(Values(0), " +")
                            Dim m = r.Match(p)
                            If Not m.Success Then Throw GetFormatException(LineNumber)
                            Dim Unicode = Integer.Parse(m.Result("${Unicode}"), Globalization.NumberStyles.HexNumber)
                            Unicodes.Add(Unicode)
                        Next
                    End If
                    Dim Code = Values(1)
                    Dim c As StringCode
                    If Code <> "" Then
                        c = StringCode.FromUnicodeStringAndCodeString(Unicodes.ToUTF16B, Code)
                    Else
                        c = StringCode.FromUnicodeString(Unicodes.ToUTF16B)
                    End If

                    Dim PhysicalBox As New Rectangle(Integer.Parse(Values(2), Globalization.NumberStyles.Integer), Integer.Parse(Values(3), Globalization.NumberStyles.Integer), Integer.Parse(Values(4), Globalization.NumberStyles.Integer), Integer.Parse(Values(5), Globalization.NumberStyles.Integer))
                    Dim VirtualBox As New Rectangle(Integer.Parse(Values(6), Globalization.NumberStyles.Integer), Integer.Parse(Values(7), Globalization.NumberStyles.Integer), Integer.Parse(Values(8), Globalization.NumberStyles.Integer), Integer.Parse(Values(9), Globalization.NumberStyles.Integer))

                    d.Add(New GlyphDescriptor With {.c = c, .PhysicalBox = PhysicalBox, .VirtualBox = VirtualBox})
                End If
                LineNumber += 1
            End While
            Return d
        End Function
        Public Shared Function ReadFile(ByVal Reader As StreamReader) As IEnumerable(Of GlyphDescriptor)
            Return ReadFile(Reader, Function(LineNumber) New InvalidTextFormatException(LineNumber))
        End Function
        Public Shared Function ReadFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding) As IEnumerable(Of GlyphDescriptor)
            Using s = Txt.CreateTextReader(Path, Encoding, True)
                Return ReadFile(s, Function(LineNumber) New InvalidTextFormatException(Path, LineNumber))
            End Using
        End Function
        Public Shared Function ReadFile(ByVal Path As String) As IEnumerable(Of GlyphDescriptor)
            Return ReadFile(Path, TextEncoding.Default)
        End Function
        Public Shared Sub WriteFile(ByVal Writer As StreamWriter, ByVal GlyphDescriptors As IEnumerable(Of GlyphDescriptor))
            Dim s = Writer
            For Each d In GlyphDescriptors
                Dim Unicode = ""
                If d.c.HasUnicodes Then Unicode = String.Join(" ", (From u In d.c.UnicodeString.ToUTF32 Select "U+{0:X4}".Formats(u.Value)).ToArray)
                Dim Code = ""
                If d.c.HasCodes Then Code = d.c.CodeString

                s.WriteLine(String.Join(",", (From o In New Object() {Unicode, Code, d.PhysicalBox.X, d.PhysicalBox.Y, d.PhysicalBox.Width, d.PhysicalBox.Height, d.VirtualBox.X, d.VirtualBox.Y, d.VirtualBox.Width, d.VirtualBox.Height} Select (o.ToString)).ToArray))
            Next
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal Encoding As System.Text.Encoding, ByVal GlyphDescriptors As IEnumerable(Of GlyphDescriptor))
            Using s = Texting.Txt.CreateTextWriter(Path, Encoding, True)
                WriteFile(s, GlyphDescriptors)
            End Using
        End Sub
        Public Shared Sub WriteFile(ByVal Path As String, ByVal GlyphDescriptors As IEnumerable(Of GlyphDescriptor))
            WriteFile(Path, TextEncoding.WritingDefault, GlyphDescriptors)
        End Sub

        Public Shared Function ReadFont(ByVal Reader As StreamReader, ByVal ImageReader As IImageReader) As IEnumerable(Of IGlyph)
            Dim GlyphDescriptors = ReadFile(Reader)
            ImageReader.Load()
            Dim l As New List(Of IGlyph)
            For Each d In GlyphDescriptors
                l.Add(New Glyph With {.c = d.c, .Block = ImageReader.GetRectangleAsARGB(d.PhysicalBox.X, d.PhysicalBox.Y, d.PhysicalBox.Width, d.PhysicalBox.Height), .VirtualBox = d.VirtualBox})
            Next
            Return l
        End Function
        Public Shared Function ReadFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal ImageReader As IImageReader) As IEnumerable(Of IGlyph)
            Dim GlyphDescriptors = ReadFile(FdPath, Encoding)
            ImageReader.Load()
            Dim l As New List(Of IGlyph)
            For Each d In GlyphDescriptors
                l.Add(New Glyph With {.c = d.c, .Block = ImageReader.GetRectangleAsARGB(d.PhysicalBox.X, d.PhysicalBox.Y, d.PhysicalBox.Width, d.PhysicalBox.Height), .VirtualBox = d.VirtualBox})
            Next
            Return l
        End Function
        Public Shared Function ReadFont(ByVal FdPath As String, ByVal ImageReader As IImageReader) As IEnumerable(Of IGlyph)
            Return ReadFont(FdPath, TextEncoding.Default, ImageReader)
        End Function
        Public Shared Function ReadFont(ByVal FdPath As String) As IEnumerable(Of IGlyph)
            Dim Encoding = TextEncoding.Default
            Dim BmpPath = ChangeExtension(FdPath, "bmp")
            Using ImageReader As New BmpFontImageFileReader(BmpPath)
                Return ReadFont(FdPath, Encoding, ImageReader)
            End Using
        End Function
        Public Shared Sub WriteFont(ByVal Writer As StreamWriter, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal GlyphDescriptors As IEnumerable(Of GlyphDescriptor), ByVal ImageWriter As IImageWriter, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim gdl = GlyphDescriptors.ToArray
            If gl.Length <> gdl.Length Then Throw New ArgumentException("GlyphsAndGlyphDescriptorsCountNotMatch")
            Dim PicWidth = Width
            Dim PicHeight = Height

            ImageWriter.Create(PicWidth, PicHeight)
            For GlyphIndex = 0 To gl.Count - 1
                Dim g = gl(GlyphIndex)
                Dim gd = gdl(GlyphIndex)
                Dim x As Integer = gd.PhysicalBox.X
                Dim y As Integer = gd.PhysicalBox.Y
                ImageWriter.SetRectangleFromARGB(x, y, g.Block)
            Next
            WriteFile(Writer, GlyphDescriptors)
        End Sub
        Public Shared Sub WriteFont(ByVal Writer As StreamWriter, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal GlyphArranger As IGlyphArranger, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim PicWidth = Width
            Dim PicHeight = Height

            Dim GlyphDescriptors = GlyphArranger.GetGlyphArrangement(gl, PicWidth, PicHeight)
            Dim gdl = GlyphDescriptors.ToArray
            If gl.Length <> gdl.Length Then Throw New InvalidOperationException("NumGlyphTooMuch: NumGlyph={0} MaxNumGlyph={1}".Formats(gl.Count, GlyphDescriptors.Count))

            WriteFont(Writer, Glyphs, GlyphDescriptors, ImageWriter, Width, Height)
        End Sub
        Public Shared Sub WriteFont(ByVal Writer As StreamWriter, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim PicWidth = Width
            Dim PicHeight = Height

            WriteFont(Writer, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal Writer As StreamWriter, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal Width As Integer)
            Dim gl = Glyphs.ToArray
            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim PicWidth = Width
            Dim PicHeight = ga.GetPreferredHeight(gl, Width)

            WriteFont(Writer, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal Writer As StreamWriter, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter)
            Dim gl = Glyphs.ToArray
            If gl.Count = 0 Then
                ImageWriter.Create(0, 0)
                ImageWriter.SetRectangleFromARGB(0, 0, New Int32(,) {})
                Return
            End If

            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim Size = ga.GetPreferredSize(gl)
            Dim PicWidth = Size.Width
            Dim PicHeight = Size.Height

            WriteFont(Writer, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal GlyphDescriptors As IEnumerable(Of GlyphDescriptor), ByVal ImageWriter As IImageWriter, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim gdl = GlyphDescriptors.ToArray
            If gl.Length <> gdl.Length Then Throw New ArgumentException("GlyphsAndGlyphDescriptorsCountNotMatch")
            Dim PicWidth = Width
            Dim PicHeight = Height

            ImageWriter.Create(PicWidth, PicHeight)
            For GlyphIndex = 0 To gl.Count - 1
                Dim g = gl(GlyphIndex)
                Dim gd = gdl(GlyphIndex)
                Dim x As Integer = gd.PhysicalBox.X
                Dim y As Integer = gd.PhysicalBox.Y
                ImageWriter.SetRectangleFromARGB(x, y, g.Block)
            Next
            WriteFile(FdPath, Encoding, GlyphDescriptors)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal GlyphArranger As IGlyphArranger, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim PicWidth = Width
            Dim PicHeight = Height

            Dim GlyphDescriptors = GlyphArranger.GetGlyphArrangement(gl, PicWidth, PicHeight)
            Dim gdl = GlyphDescriptors.ToArray
            If gl.Length <> gdl.Length Then Throw New InvalidOperationException("NumGlyphTooMuch: NumGlyph={0} MaxNumGlyph={1}".Formats(gl.Count, GlyphDescriptors.Count))

            WriteFont(FdPath, Encoding, Glyphs, GlyphDescriptors, ImageWriter, Width, Height)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal Width As Integer, ByVal Height As Integer)
            Dim gl = Glyphs.ToArray
            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim PicWidth = Width
            Dim PicHeight = Height

            WriteFont(FdPath, Encoding, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter, ByVal Width As Integer)
            Dim gl = Glyphs.ToArray
            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim PicWidth = Width
            Dim PicHeight = ga.GetPreferredHeight(gl, Width)

            WriteFont(FdPath, Encoding, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Encoding As System.Text.Encoding, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal ImageWriter As IImageWriter)
            Dim gl = Glyphs.ToArray
            Dim PhysicalWidth As Integer = (From g In gl Select (g.PhysicalWidth)).Max
            Dim PhysicalHeight As Integer = (From g In gl Select (g.PhysicalHeight)).Max
            Dim ga As New GlyphArranger(PhysicalWidth, PhysicalHeight)
            Dim Size = ga.GetPreferredSize(gl)
            Dim PicWidth = Size.Width
            Dim PicHeight = Size.Height

            WriteFont(FdPath, Encoding, gl, ImageWriter, ga, PicWidth, PicHeight)
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal BitPerPixel As Integer, ByVal Palette As Int32(), ByVal Quantize As Func(Of Int32, Byte))
            Dim BmpPath = ChangeExtension(FdPath, "bmp")
            Dim Encoding = TextEncoding.WritingDefault
            Using ImageWriter As New BmpFontImageFileWriter(BmpPath, CShort(BitPerPixel), Palette, Quantize)
                WriteFont(FdPath, Encoding, Glyphs, ImageWriter)
            End Using
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Glyphs As IEnumerable(Of IGlyph), ByVal BitPerPixel As Integer)
            Dim BmpPath = ChangeExtension(FdPath, "bmp")
            Dim Encoding = TextEncoding.WritingDefault
            Using ImageWriter As New BmpFontImageFileWriter(BmpPath, CShort(BitPerPixel))
                WriteFont(FdPath, Encoding, Glyphs, ImageWriter)
            End Using
        End Sub
        Public Shared Sub WriteFont(ByVal FdPath As String, ByVal Glyphs As IEnumerable(Of IGlyph))
            Dim BmpPath = ChangeExtension(FdPath, "bmp")
            Dim Encoding = TextEncoding.WritingDefault
            Using ImageWriter As New BmpFontImageFileWriter(BmpPath)
                WriteFont(FdPath, Encoding, Glyphs, ImageWriter)
            End Using
        End Sub
    End Class

    Public NotInheritable Class BmpFontImageReader
        Implements IImageReader

        Private sp As NewReadingStreamPasser
        Private b As Bmp

        Public Sub New(ByVal sp As NewReadingStreamPasser)
            Me.sp = sp
        End Sub

        Public Sub Load() Implements IImageReader.Load
            If b IsNot Nothing Then Throw New InvalidOperationException
            b = Bmp.Open(sp)
        End Sub
        Public Function GetRectangleAsARGB(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As Integer(,) Implements IImageReader.GetRectangleAsARGB
            Return b.GetRectangleAsARGB(x, y, w, h)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If b IsNot Nothing Then
                b.Dispose()
                b = Nothing
            End If
        End Sub
    End Class

    Public NotInheritable Class BmpFontImageFileReader
        Implements IImageReader

        Private s As IReadableSeekableStream
        Private r As BmpFontImageReader

        Public Sub New(ByVal Path As String)
            s = StreamEx.CreateReadable(Path, FileMode.Open)
            r = New BmpFontImageReader(s.AsNewReading)
        End Sub

        Public Sub Load() Implements IImageReader.Load
            r.Load()
        End Sub
        Public Function GetRectangleAsARGB(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer) As Integer(,) Implements IImageReader.GetRectangleAsARGB
            Return r.GetRectangleAsARGB(x, y, w, h)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If r IsNot Nothing Then
                r.Dispose()
                r = Nothing
            End If
            If s IsNot Nothing Then
                s.Dispose()
                s = Nothing
            End If
        End Sub
    End Class

    Public NotInheritable Class BmpFontImageWriter
        Implements IImageWriter

        Private sp As NewWritingStreamPasser
        Private b As Bmp
        Private BitPerPixel As Integer
        Private Palette As Int32()
        Private Quantize As Func(Of Int32, Byte)

        Public Sub New(ByVal sp As NewWritingStreamPasser)
            Me.New(sp, 8)
        End Sub
        Public Sub New(ByVal sp As NewWritingStreamPasser, ByVal BitPerPixel As Integer)
            Me.sp = sp
            Me.BitPerPixel = BitPerPixel
            Select Case BitPerPixel
                Case 2
                    Me.BitPerPixel = 4
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Dim r = 255 \ ((1 << BitPerPixel) - 1)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(r * i), 8, CByte(r * i), 8, CByte(r * i), 8)).ToArray.Extend(16, 0)
                    Quantize = Function(ARGB As Int32) CByte(GetGray(ARGB) >> (8 - BitPerPixel))
                Case 8
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(i), 8, CByte(i), 8, CByte(i), 8)).ToArray
                    Quantize = GetGray
                Case Is <= 8
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Dim r = 255 \ ((1 << BitPerPixel) - 1)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(r * i), 8, CByte(r * i), 8, CByte(r * i), 8)).ToArray
                    Quantize = Function(ARGB As Int32) CByte(GetGray(ARGB) >> (8 - BitPerPixel))
                Case Else
            End Select
        End Sub
        Public Sub New(ByVal sp As NewWritingStreamPasser, ByVal BitPerPixel As Integer, ByVal Palette As Int32())
            Me.sp = sp
            Me.BitPerPixel = BitPerPixel
            Me.Palette = Palette
        End Sub
        Public Sub New(ByVal sp As NewWritingStreamPasser, ByVal BitPerPixel As Integer, ByVal Palette As Int32(), ByVal Quantize As Func(Of Int32, Byte))
            Me.sp = sp
            Me.BitPerPixel = BitPerPixel
            Me.Palette = Palette
            Me.Quantize = Quantize
        End Sub

        Public Sub Create(ByVal w As Integer, ByVal h As Integer) Implements IImageWriter.Create
            b = New Bmp(sp, w, h, CShort(BitPerPixel))
            If Palette IsNot Nothing Then b.Palette = Palette
        End Sub

        Public Sub SetRectangleFromARGB(ByVal x As Integer, ByVal y As Integer, ByVal a(,) As Integer) Implements IImageWriter.SetRectangleFromARGB
            If Quantize Is Nothing Then
                b.SetRectangleFromARGB(x, y, a)
            Else
                b.SetRectangleFromARGB(x, y, a, Quantize)
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If b IsNot Nothing Then
                b.Dispose()
                b = Nothing
            End If
        End Sub
    End Class

    Public NotInheritable Class BmpFontImageFileWriter
        Implements IImageWriter

        Private BmpPath As String
        Private b As Bmp
        Private BitPerPixel As Integer
        Private Palette As Int32()
        Private Quantize As Func(Of Int32, Byte)

        Public Sub New(ByVal Path As String)
            Me.New(Path, 8)
        End Sub
        Public Sub New(ByVal Path As String, ByVal BitPerPixel As Integer)
            BmpPath = Path
            Me.BitPerPixel = BitPerPixel
            Select Case BitPerPixel
                Case 2
                    Me.BitPerPixel = 4
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Dim r = 255 \ ((1 << BitPerPixel) - 1)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(r * i), 8, CByte(r * i), 8, CByte(r * i), 8)).ToArray.Extend(16, 0)
                    Quantize = Function(ARGB As Int32) CByte(GetGray(ARGB) >> (8 - BitPerPixel))
                Case 8
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(i), 8, CByte(i), 8, CByte(i), 8)).ToArray
                    Quantize = GetGray
                Case Is <= 8
                    Dim GetGray = Function(ARGB As Int32) CByte((((ARGB And &HFF0000) >> 16) + ((ARGB And &HFF00) >> 8) + (ARGB And &HFF) + 2) \ 3)
                    Dim r = 255 \ ((1 << BitPerPixel) - 1)
                    Palette = (From i In Enumerable.Range(0, 1 << BitPerPixel) Select ConcatBits(CByte(&HFF), 8, CByte(r * i), 8, CByte(r * i), 8, CByte(r * i), 8)).ToArray
                    Quantize = Function(ARGB As Int32) CByte(GetGray(ARGB) >> (8 - BitPerPixel))
                Case Else
            End Select
        End Sub
        Public Sub New(ByVal Path As String, ByVal BitPerPixel As Integer, ByVal Palette As Int32())
            BmpPath = Path
            Me.BitPerPixel = BitPerPixel
            Me.Palette = Palette
        End Sub
        Public Sub New(ByVal Path As String, ByVal BitPerPixel As Integer, ByVal Palette As Int32(), ByVal Quantize As Func(Of Int32, Byte))
            BmpPath = Path
            Me.BitPerPixel = BitPerPixel
            Me.Palette = Palette
            Me.Quantize = Quantize
        End Sub

        Public Sub Create(ByVal w As Integer, ByVal h As Integer) Implements IImageWriter.Create
            b = New Bmp(BmpPath, w, h, CShort(BitPerPixel))
            If Palette IsNot Nothing Then b.Palette = Palette
        End Sub

        Public Sub SetRectangleFromARGB(ByVal x As Integer, ByVal y As Integer, ByVal a(,) As Integer) Implements IImageWriter.SetRectangleFromARGB
            If Quantize Is Nothing Then
                b.SetRectangleFromARGB(x, y, a)
            Else
                b.SetRectangleFromARGB(x, y, a, Quantize)
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If b IsNot Nothing Then
                b.Dispose()
                b = Nothing
            End If
        End Sub
    End Class
End Namespace
