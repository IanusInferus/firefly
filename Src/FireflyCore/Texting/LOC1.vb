'==========================================================================
'
'  File:        LOC1.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: LOC文件格式类(版本1)(图形文本)
'  Version:     2010.09.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text
Imports Firefly
Imports Firefly.Imaging
Imports Firefly.TextEncoding
Imports Firefly.Glyphing

Namespace Texting
    ''' <summary>图形文本文件类</summary>
    Public Class LOC1
        Public Shared ReadOnly IdentifierCompression As String = "LOCC"
        Public Shared ReadOnly Identifier As String = "LOC1"

        Protected FontLibValue As FontLib
        Protected GlyphMapValue As GlyphMap
        Protected CharGlyphDictValue As New Dictionary(Of CharCode, Int32)
        Protected TextValue As CharCode()()

        ''' <summary>字库。不要修改该字库，可以考虑创建LOC的新实例，以使得LOC的内部状态正常。</summary>
        Public ReadOnly Property FontLib() As FontLib
            Get
                Return FontLibValue
            End Get
        End Property

        ''' <summary>字形图。不要修改该字形图，可以考虑创建LOC的新实例，以使得LOC的内部状态正常。</summary>
        Public ReadOnly Property GlyphMap() As GlyphMap
            Get
                Return GlyphMapValue
            End Get
        End Property

        ''' <summary>字符码点-字形号映射。不要修改，可以考虑创建LOC的新实例，以使得LOC的内部状态正常。</summary>
        Public ReadOnly Property CharGlyphDict() As Dictionary(Of CharCode, Int32)
            Get
                Return CharGlyphDictValue
            End Get
        End Property

        ''' <summary>文本。不要修改该文本，可以考虑创建LOC的新实例，以使得LOC的内部状态正常。</summary>
        Public ReadOnly Property Text() As CharCode()()
            Get
                Return TextValue
            End Get
        End Property


        ''' <summary>已重载。创建新的图形文本类。</summary>
        Public Sub New(ByVal FontLib As FontLib, ByVal GlyphMap As GlyphMap, ByVal CharGlyphDict As Dictionary(Of CharCode, Int32), ByVal Text As CharCode()())
            Me.FontLibValue = FontLib
            Me.GlyphMapValue = GlyphMap
            Me.CharGlyphDictValue = CharGlyphDict
            Me.TextValue = Text
        End Sub

        ''' <summary>生成图形文本文件。生成32位BMP。若需其他BMP，可仿照此函数生成。所有字库字符大小不得大于最大的字符宽度和高度。</summary>
        Public Shared Function GenerateLOC(ByVal FontLib As FontLib, ByVal Text As CharCode()()) As LOC1
            Dim CharGlyphDict As New Dictionary(Of CharCode, Integer)
            Dim GlyphWidth = 0
            Dim GlyphHeight = 0
            Dim GlyphCount = 0
            For Each c In FontLib.CharCodes
                If FontLib.HasGlyph(c) Then
                    Dim Glyph = FontLib.Item(c)
                    If Glyph.PhysicalWidth > GlyphWidth Then GlyphWidth = Glyph.PhysicalWidth
                    If Glyph.PhysicalHeight > GlyphHeight Then GlyphHeight = Glyph.PhysicalHeight

                    CharGlyphDict.Add(c, GlyphCount)
                    GlyphCount += 1
                End If
            Next

            Dim g As New GlyphMap(GlyphCount, GlyphWidth, GlyphHeight)
            For Each Pair In CharGlyphDict
                Dim Glyph = FontLib(Pair.Key)
                Dim Block = Glyph.Block
                g.WidthTable(Pair.Value) = CByte(Glyph.VirtualBox.Width)
                Dim r = g.GetGlyphPhysicalBox(Pair.Value)
                If Block.GetLength(0) > r.Width Then Throw New InvalidDataException
                If Block.GetLength(1) > r.Height Then Throw New InvalidDataException
                g.Bmp.SetRectangle(r.X, r.Y, Block)
            Next

            Return New LOC1(FontLib, g, CharGlyphDict, Text)
        End Function

        Public Shared Function GenerateEmptyLOC(ByVal Count As Integer)
            Return LOC1.GenerateLOC(New FontLib, (From i In Enumerable.Range(0, Count) Select New CharCode() {}).ToArray)
        End Function

        ''' <summary>已重载。从流读取图形文本文件。</summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId:="CharInfoDBLength")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId:="TextInfoDBLength")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId:="TextLength")> _
        Public Sub New(ByVal sp As ZeroPositionStreamPasser)
            Dim s As StreamEx = sp.GetStream
            Dim Close As Boolean = False
            Try
                Dim Id As String = s.ReadSimpleString(4)
                If Id = IdentifierCompression Then
                    Using gz As New PartialStreamEx(s, 4, s.Length - 4)
                        Using gzDec As New IO.Compression.GZipStream(gz.ToUnsafeStream, IO.Compression.CompressionMode.Decompress, False)
                            s = New StreamEx
                            While True
                                Dim b As Int32 = gzDec.ReadByte
                                If b = -1 Then Exit While
                                s.WriteByte(CByte(b))
                            End While
                        End Using
                    End Using
                    s.Position = 0

                    Close = True
                    Id = s.ReadSimpleString(4)
                End If

                If Id <> Identifier Then Throw New InvalidDataException


                Dim NumSection As Int32 = s.ReadInt32

                Dim FontLibSectionAddress As Int32
                Dim FontLibSectionLength As Int32
                If NumSection >= 1 Then
                    FontLibSectionAddress = s.ReadInt32
                    FontLibSectionLength = s.ReadInt32
                End If
                Dim TextAddress As Int32
                Dim TextLength As Int32
                If NumSection >= 2 Then
                    TextAddress = s.ReadInt32
                    TextLength = s.ReadInt32
                End If


                If NumSection <= 0 Then Return
                s.Position = FontLibSectionAddress
                If FontLibSectionLength > 0 Then
                    Dim CharCount As Int32 = s.ReadInt32
                    Dim CharCode = New CharCode(CharCount - 1) {}
                    Dim CharInfoDBLength As Int32 = s.ReadInt32 '暂时不用
                    For n As Integer = 0 To CharCount - 1
                        Dim Index = s.ReadInt32
                        If Index <> n Then Throw New InvalidDataException

                        Dim GlyphIndex = s.ReadInt32
                        Dim Unicode = s.ReadInt32
                        Dim Code = s.ReadInt32

                        CharCode(n) = New CharCode(Unicode, Code)

                        If GlyphIndex <> -1 Then CharGlyphDictValue.Add(CharCode(n), GlyphIndex)
                    Next

                    FontLibValue = New FontLib(CharCode)

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
                    Dim Bitmap As Bmp = Nothing
                    If BitmapLength > 0 Then
                        Using BitmapStream As New PartialStreamEx(s, s.Position, BitmapLength)
                            Dim ms As New StreamEx
                            ms.WriteFromStream(BitmapStream, BitmapStream.Length)
                            ms.Position = 0
                            Bitmap = Bmp.Open(ms)
                            s.Position += BitmapLength - BitmapStream.Position
                        End Using
                    End If
                    GlyphMapValue = New GlyphMap(GlyphCount, GlyphWidth, GlyphHeight, WidthTable, Bitmap)

                    For Each c In CharCode
                        If CharGlyphDictValue.ContainsKey(c) Then
                            Dim r = GlyphMapValue.GetGlyphPhysicalBox(CharGlyphDictValue(c))
                            FontLibValue(c) = New Glyph With {.c = c.ToStringCode, .Block = GlyphMapValue.Bmp.GetRectangleAsARGB(r.X, r.Y, r.Width, r.Height), .VirtualBox = GlyphMapValue.GetGlyphVirtualBox(CharGlyphDictValue(c))}
                        End If
                    Next
                End If


                If NumSection <= 1 Then Return
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
                TextValue = New CharCode(TextCount - 1)() {}
                For n As Integer = 0 To TextCount - 1
                    Dim Original = TextCharIndex(n)
                    Dim SingleText = New CharCode(TextCharIndex(n).Length - 1) {}
                    For k As Integer = 0 To TextCharIndex(n).Length - 1
                        SingleText(k) = FontLibValue.CharCodes(Original(k))
                    Next
                    TextValue(n) = SingleText
                Next
            Finally
                If Close Then s.Close()
            End Try
        End Sub

        ''' <summary>写入图形文本文件到流。</summary>
        Public Sub WriteToFile(ByVal sp As ZeroPositionStreamPasser, Optional ByVal Compress As Boolean = False)
            Dim f As StreamEx = sp.GetStream
            Using s As New StreamEx
                s.WriteSimpleString(Identifier, 4)
                Dim NumSection As Integer = 2
                s.WriteInt32(NumSection)


                Dim FontLibSectionAddress As Int32
                Dim FontLibSectionLength As Int32
                Dim TextAddress As Int32
                Dim TextLength As Int32
                s.Position += 8 * NumSection
                s.Position = ((s.Position + 15) \ 16) * 16


                FontLibSectionAddress = CInt(s.Position)
                Dim CodeIndexDict As New Dictionary(Of CharCode, Int32)
                Dim CharCount = FontLibValue.CharCount
                Dim CharCodes = FontLibValue.CharCodes
                For n = 0 To CharCount - 1
                    CodeIndexDict.Add(CharCodes(n), n)
                Next
                If FontLibValue IsNot Nothing Then
                    s.WriteInt32(CharCount)
                    s.WriteInt32(16)
                    For n As Integer = 0 To CharCount - 1
                        Dim c = CharCodes(n)
                        s.WriteInt32(n)
                        If FontLibValue.HasGlyph(c) Then
                            s.WriteInt32(CharGlyphDictValue(c))
                        Else
                            s.WriteInt32(-1)
                        End If
                        s.WriteInt32(c.Unicode)
                        s.WriteInt32(c.Code)
                    Next

                    With GlyphMapValue
                        s.WriteInt32(.GlyphCount)
                        s.WriteInt32(.GlyphWidth)
                        s.WriteInt32(.GlyphHeight)
                        If .WidthTable Is Nothing Then
                            s.WriteInt32(0)
                        Else
                            s.WriteInt32(.WidthTable.Length)
                            s.Write(.WidthTable)
                        End If
                        s.Position = ((s.Position + 15) \ 16) * 16
                        If .GlyphCount = 0 Then
                            s.WriteInt32(0)
                        Else
                            Using ms As New StreamEx
                                .Bmp.SaveTo(ms)
                                ms.Position = 0
                                s.WriteInt32(CInt(ms.Length))
                                s.WriteFromStream(ms, ms.Length)
                            End Using
                        End If
                    End With
                End If
                FontLibSectionLength = CInt(s.Position) - FontLibSectionAddress
                s.Position = ((s.Position + 15) \ 16) * 16


                TextAddress = CInt(s.Position)
                Dim TextCount As Int32 = 0
                If TextValue IsNot Nothing Then TextCount = TextValue.Length
                s.WriteInt32(TextCount)
                s.WriteInt32(8)

                Dim TextInfoAddress As Int32() = New Int32(TextCount - 1) {}
                Dim TextInfoLength As Int32() = New Int32(TextCount - 1) {}
                Dim TextIndexPosition As Int32 = CInt(s.Position)
                s.Position += 8 * TextCount
                For n As Integer = 0 To TextCount - 1
                    Dim Original = TextValue(n)
                    Dim TextCharIndex = New Int32(Original.Length - 1) {}
                    For k As Integer = 0 To Original.Length - 1
                        TextCharIndex(k) = CodeIndexDict(Original(k))
                    Next
                    Dim VLECode As Byte() = VariableLengthEncode(TextCharIndex)
                    TextInfoAddress(n) = CInt(s.Position) - TextAddress
                    TextInfoLength(n) = VLECode.Length
                    s.Write(VLECode)
                Next
                Dim TextEndPosition = s.Position
                s.Position = TextIndexPosition
                For n As Integer = 0 To TextCount - 1
                    s.WriteInt32(TextInfoAddress(n))
                    s.WriteInt32(TextInfoLength(n))
                Next
                s.Position = TextEndPosition
                TextLength = CInt(s.Position) - TextAddress
                s.Position = ((s.Position + 15) \ 16) * 16


                s.SetLength(s.Position)


                Dim Position = s.Position
                s.Position = 8
                s.WriteInt32(FontLibSectionAddress)
                s.WriteInt32(FontLibSectionLength)
                s.WriteInt32(TextAddress)
                s.WriteInt32(TextLength)
                s.Position = Position

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

        Protected Shared Function VariableLengthEncode(ByVal Original As Int32()) As Byte()
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
        Protected Shared Function VariableLengthDecode(ByVal Encoded As Byte()) As Int32()
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

        Public Function GetGlyphText() As GlyphText
            Return New GlyphText(Me)
        End Function
    End Class

    ''' <summary>字形图片。</summary>
    Public Class GlyphMap
        Protected GlyphCountValue As Integer

        ''' <summary>字形数量。</summary>
        Public Property GlyphCount() As Integer
            Get
                Return GlyphCountValue
            End Get
            Set(ByVal Value As Integer)
                If Value < 0 Then Throw New ArgumentOutOfRangeException
                GlyphCountValue = Value
            End Set
        End Property

        Protected GlyphWidthValue As Integer
        ''' <summary>字形的最大宽度。</summary>
        Public ReadOnly Property GlyphWidth() As Integer
            Get
                Return GlyphWidthValue
            End Get
        End Property

        Protected GlyphHeightValue As Integer
        ''' <summary>字形的高度。</summary>
        Public ReadOnly Property GlyphHeight() As Integer
            Get
                Return GlyphHeightValue
            End Get
        End Property

        Protected WidthTableValue As Byte()
        ''' <summary>字形宽度表。</summary>
        Public Property WidthTable() As Byte()
            Get
                Return WidthTableValue
            End Get
            Set(ByVal Value As Byte())
                If Value Is Nothing Then Throw New ArgumentNullException
                If Value.Length <> GlyphCount Then Throw New ArgumentException
                WidthTableValue = Value
            End Set
        End Property

        Protected BmpValue As Bmp
        ''' <summary>图片。</summary>
        Public ReadOnly Property Bmp() As Bmp
            Get
                Return BmpValue
            End Get
        End Property

        Protected NumGlyphInLineValue As Integer
        ''' <summary>每行的字形数。</summary>
        Public ReadOnly Property NumGlyphInLine() As Integer
            Get
                Return NumGlyphInLineValue
            End Get
        End Property

        ''' <summary>图片宽度。</summary>
        Public ReadOnly Property Width() As Integer
            Get
                Return BmpValue.Width
            End Get
        End Property
        ''' <summary>图片高度。</summary>
        Public ReadOnly Property Height() As Integer
            Get
                Return BmpValue.Height
            End Get
        End Property

        ''' <summary>已重载。构造字形图片。自动创建默认的宽度表，且所有值均初始化为GlyphWidth。如果传入空的Bmp，会自动按GlyphCount创建适当大小的32位Bmp。</summary>
        Public Sub New(ByVal GlyphCount As Integer, ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, Optional ByVal Bmp As Bmp = Nothing)
            Me.New(GlyphCount, GlyphWidth, GlyphHeight, New Byte(GlyphCount - 1) {}, Bmp)
            For n = 0 To WidthTable.Length - 1
                WidthTable(n) = CByte(GlyphWidth)
            Next
        End Sub

        ''' <summary>已重载。构造字形图片。如果传入空的Bmp，会自动按GlyphCount创建适当大小的32位Bmp。</summary>
        Public Sub New(ByVal GlyphCount As Integer, ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, ByVal WidthTable As Byte(), Optional ByVal Bmp As Bmp = Nothing)
            If GlyphCount < 0 Then Throw New ArgumentOutOfRangeException
            If GlyphWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If GlyphHeight <= 0 Then Throw New ArgumentOutOfRangeException
            If WidthTable Is Nothing Then Throw New ArgumentNullException
            If WidthTable.Length <> GlyphCount Then Throw New ArgumentException
            Me.GlyphCountValue = GlyphCount
            Me.GlyphWidthValue = GlyphWidth
            Me.GlyphHeightValue = GlyphHeight
            Me.WidthTableValue = WidthTable
            If Bmp IsNot Nothing Then
                NumGlyphInLineValue = Bmp.Width \ GlyphWidth
                Me.BmpValue = Bmp
            Else
                Const BitmapWidth As Integer = 512
                NumGlyphInLineValue = BitmapWidth \ GlyphWidth
                Dim NumGlyphLine As Integer = (GlyphCount + NumGlyphInLineValue - 1) \ NumGlyphInLineValue
                If NumGlyphLine <= 0 Then NumGlyphInLineValue = 1
                Dim Bitmap As New Bmp(BitmapWidth, GlyphHeight * NumGlyphLine, 32)
                Me.BmpValue = Bitmap
            End If
        End Sub

        ''' <summary>获取字形的正方形位置。</summary>
        Public Function GetGlyphVirtualBox(ByVal GlyphIndex As Integer) As System.Drawing.Rectangle
            If GlyphIndex < 0 OrElse GlyphIndex > GlyphCount Then Throw New InvalidDataException
            Return New Rectangle(0, 0, WidthTable(GlyphIndex), GlyphHeight)
        End Function

        ''' <summary>获取字形的正方形位置。</summary>
        Public Function GetGlyphPhysicalBox(ByVal GlyphIndex As Integer) As System.Drawing.Rectangle
            If GlyphIndex < 0 OrElse GlyphIndex > GlyphCount Then Throw New InvalidDataException
            Dim x As Integer = (GlyphIndex Mod NumGlyphInLine) * GlyphWidth
            Dim y As Integer = (GlyphIndex \ NumGlyphInLine) * GlyphHeight
            Return New Rectangle(x, y, GlyphWidth, GlyphHeight)
        End Function
    End Class

    ''' <summary>图形文本类</summary>
    Public Class GlyphText

        Protected FontLibValue As FontLib
        ''' <summary>字库。</summary>
        Public ReadOnly Property FontLib() As FontLib
            Get
                Return FontLibValue
            End Get
        End Property

        Protected TextValue As CharCode()()
        ''' <summary>文本。</summary>
        Public Property Text() As CharCode()()
            Get
                Return TextValue
            End Get
            Set(ByVal Value As CharCode()())
                If Value Is Nothing Then Throw New ArgumentNullException
                TextValue = Value
            End Set
        End Property

        Protected GlyphWidthValue As Integer
        ''' <summary>字形的默认宽度(最大宽度)。</summary>
        Public ReadOnly Property GlyphWidth() As Integer
            Get
                Return GlyphWidthValue
            End Get
        End Property

        Protected GlyphHeightValue As Integer
        ''' <summary>字形的高度。</summary>
        Public ReadOnly Property GlyphHeight() As Integer
            Get
                Return GlyphHeightValue
            End Get
        End Property

        ''' <summary>已重载。从字库、默认字形大小和文本码点创建实例。</summary>
        Public Sub New(ByVal FontLib As FontLib, ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, ByVal Text As CharCode()())
            If FontLib Is Nothing Then Throw New ArgumentNullException
            If GlyphWidth <= 0 Then Throw New ArgumentOutOfRangeException
            If GlyphHeight <= 0 Then Throw New ArgumentOutOfRangeException
            If Text Is Nothing Then Throw New ArgumentNullException
            Me.FontLibValue = FontLib
            Me.GlyphWidthValue = GlyphWidth
            Me.GlyphHeightValue = GlyphHeight
            Me.TextValue = Text
        End Sub

        ''' <summary>已重载。从默认字形大小和文本创建实例。</summary>
        Public Sub New(ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, ByVal Text As String())
            Dim CharCodeText = New CharCode(Text.Length - 1)() {}
            For n = 0 To Text.Length - 1
                CharCodeText(n) = CharCodeString.FromString16(Text(n))
            Next
            Me.FontLibValue = New FontLib(CharCodeString.FromString32(GetEncodingString32FromText(Text)))
            For n = 0 To Text.Length - 1
                Dim t = CharCodeText(n)
                For k = 0 To t.Length - 1
                    t(n) = FontLibValue.LookupWithUnicode(t(n).Unicode)
                Next
            Next
            Me.GlyphWidthValue = GlyphWidth
            Me.GlyphHeightValue = GlyphHeight
            Me.TextValue = CharCodeText
        End Sub

        ''' <summary>已重载。从图形文本文件创建实例。若图形文本文件不包含FontLib或GlyphMap，则会抛出异常。</summary>
        Public Sub New(ByVal LOC As LOC1)
            Me.New(LOC.FontLib, LOC.GlyphMap.GlyphWidth, LOC.GlyphMap.GlyphHeight, LOC.Text)
        End Sub

        ''' <summary>为字符添加字形。</summary>
        Public Delegate Function DrawGlyph(ByVal c As String) As Int32(,)

        ''' <summary>已重载。使用指定字体为所有无字形非控制Unicode字符添加字形。</summary>
        Public Overridable Sub DrawGlyphForAllChar(ByVal DrawGlyph As DrawGlyph)
            For Each c In FontLibValue.CharCodes
                If c.IsControlChar Then Continue For
                If FontLibValue.HasGlyph(c) Then Continue For
                If Not c.HasUnicode Then Continue For
                Dim Block = DrawGlyph(c.Character)
                FontLibValue(c) = New Glyph With {.c = c.ToStringCode, .Block = Block, .VirtualBox = New Rectangle(0, 0, Block.GetLength(0), Block.GetLength(1))}
            Next
        End Sub
        ''' <summary>已重载。使用指定字体为所有无字形非控制Unicode字符添加字形。</summary>
        Public Overridable Sub DrawGlyphForAllChar(ByVal Font As Font)
            For Each c In FontLibValue.CharCodes
                If c.IsControlChar Then Continue For
                If FontLibValue.HasGlyph(c) Then Continue For
                If Not c.HasUnicode Then Continue For
                Using Bitmap As New Bitmap(GlyphWidthValue, GlyphHeightValue, PixelFormat.Format32bppArgb)
                    Using g = Graphics.FromImage(Bitmap)
                        Dim Width As Integer = Round(g.MeasureStringWidth(c.Character, Font))
                        Width = Min(Width, GlyphWidthValue)
                        g.DrawString(c.Character, Font, Brushes.Black, 0, 0, StringFormat.GenericTypographic)
                        Dim Block = Bitmap.GetRectangle(0, 0, Width, GlyphHeight)
                        FontLibValue(c) = New Glyph With {.c = c.ToStringCode, .Block = Block, .VirtualBox = New Rectangle(0, 0, Block.GetLength(0), Block.GetLength(1))}
                    End Using
                End Using
            Next
        End Sub

        ''' <summary>生成图形文本文件。</summary>
        Public Overridable Function GenerateLOC() As LOC1
            Return LOC1.GenerateLOC(FontLibValue, TextValue)
        End Function

        ''' <summary>绘制CharInfo表示的文本。</summary>
        Public Overridable Function GetBitmap(ByVal TextIndex As Integer, Optional ByVal Space As Integer = 0, Optional ByVal PhoneticDictionary As Dictionary(Of String, String) = Nothing) As Bitmap
            Dim EnablePhonetic As Boolean = PhoneticDictionary IsNot Nothing

            Dim GlyphText As CharCode() = TextValue(TextIndex)
            If GlyphText Is Nothing OrElse GlyphText.Length = 0 Then Return Nothing

            Dim Size As Integer = GlyphHeight
            Dim GetWidth = Function(Width As Integer) Width + Space
            If PhoneticDictionary IsNot Nothing Then
                GetWidth = Function(Width As Integer) CInt(Round((Width + Space) * 1.4))
            End If

            Dim Lines As New List(Of CharCode())
            Dim Line As New List(Of CharCode)
            Dim MaxLineWidth As Integer
            Dim LineWidth As Integer
            For Each c In GlyphText
                If c.Unicode = 10 Then
                    Lines.Add(Line.ToArray)
                    Line.Clear()
                    If MaxLineWidth < LineWidth Then MaxLineWidth = LineWidth
                    LineWidth = 0
                ElseIf FontLib.HasGlyph(c) Then
                    Line.Add(c)
                    LineWidth += GetWidth(FontLib(c).VirtualBox.Width)
                Else
                    Line.Add(c)
                    LineWidth += GetWidth(GlyphWidth)
                End If
            Next
            Lines.Add(Line.ToArray)
            Line.Clear()
            If MaxLineWidth < LineWidth Then MaxLineWidth = LineWidth
            LineWidth = 0


            Using ZHFont As New Drawing.Font("宋体", Size, FontStyle.Regular, GraphicsUnit.Pixel)
                Using PFont As New Drawing.Font("宋体", (Size * 2) \ 3, FontStyle.Regular, GraphicsUnit.Pixel)
                    Using JPFont As New Drawing.Font("MingLiU", Size, FontStyle.Regular, GraphicsUnit.Pixel)

                        Dim PadX As Integer = 5
                        Dim PadY As Integer = 5
                        Dim Bitmap As Bitmap
                        If EnablePhonetic Then
                            Bitmap = New Bitmap(MaxLineWidth + PadX * 2, GetWidth(Size) * 2 * Lines.Count + PadY * 2)
                        Else
                            Bitmap = New Bitmap(MaxLineWidth + PadX * 2, GetWidth(Size) * Lines.Count + PadY * 2)
                        End If
                        Using g As Graphics = Graphics.FromImage(Bitmap)
                            g.Clear(Color.White)

                            Dim x As Integer = PadX
                            Dim y As Integer = PadY

                            For Each GlyphLine In Lines
                                If EnablePhonetic Then y += GetWidth(Size)

                                Dim ControlCode As New List(Of Char32)
                                Dim ControlCodeMode = False

                                For Each c In GlyphLine
                                    If c.HasUnicode Then
                                        Dim ch As Char32 = c.Unicode
                                        Select Case ch
                                            Case "<", "{"
                                                If Not ControlCodeMode Then
                                                    ControlCodeMode = True
                                                    ControlCode.Add(ch)
                                                End If
                                                Continue For
                                            Case ">", "}"
                                                If ControlCodeMode Then
                                                    ControlCodeMode = False
                                                    ControlCode.Add(ch)

                                                    Dim s As String = ControlCode.ToUTF16B
                                                    g.DrawString(s, ZHFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                                    x += g.MeasureStringWidth(s, ZHFont) + 1

                                                    ControlCode.Clear()
                                                End If
                                                Continue For
                                            Case Else
                                                If ControlCodeMode Then
                                                    ControlCode.Add(ch)
                                                    Continue For
                                                Else
                                                End If
                                        End Select
                                    End If

                                    If c.IsControlChar OrElse Not ((FontLibValue.HasGlyph(c) OrElse c.HasUnicode OrElse c.HasCode)) Then
                                        g.FillRectangle(Brushes.Gray, New Rectangle(x, y, GlyphWidth, GlyphHeight))
                                        x += GetWidth(GlyphWidth)
                                    ElseIf FontLib.HasGlyph(c) Then
                                        Dim Width = FontLib(c).VirtualBox.Width
                                        Dim Glyph = FontLibValue(c)
                                        If Glyph.VirtualBox.Width <= 0 OrElse Glyph.VirtualBox.Height <= 0 Then

                                        End If
                                        Using SrcImage As New Bitmap(Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                            SrcImage.SetRectangle(0, 0, Glyph.Block)
                                            Dim SrcRect As New Rectangle(0, 0, Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                            Dim DestRect As New Rectangle(x, y, Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                            g.DrawImage(SrcImage, DestRect, SrcRect, GraphicsUnit.Pixel)
                                            '下面这句因为.Net Framework 2.0内部错误源矩形会向右偏移1像素
                                            'g.DrawImage(SrcImage, x, y, SrcRect, GraphicsUnit.Pixel)
                                        End Using

                                        If c.HasUnicode Then
                                            Dim ch As String = c.Character
                                            If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                                Dim s = PhoneticDictionary(ch)
                                                Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                                If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                            End If
                                        End If
                                        x += GetWidth(Width)
                                    Else
                                        Dim ch As String = c.Character
                                        If (c.Unicode >= &H3040) AndAlso (c.Unicode < &H3100) Then
                                            Dim Width = g.MeasureStringWidth(ch, JPFont)
                                            g.DrawString(ch, JPFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                            If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                                Dim s = PhoneticDictionary(ch)
                                                Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                                If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                            End If
                                            x += GetWidth(Width)
                                        Else
                                            Dim Width = g.MeasureStringWidth(ch, ZHFont)
                                            g.DrawString(ch, ZHFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                            If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                                Dim s = PhoneticDictionary(ch)
                                                Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                                If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                            End If
                                            x += GetWidth(Width)
                                        End If
                                    End If
                                Next

                                y += GetWidth(Size)
                                x = PadX
                            Next
                        End Using

                        Return Bitmap
                    End Using
                End Using
            End Using
        End Function
        ''' <summary>得到字符文字的普通字符串，只有已映射有Unicode的字符才会转换。</summary>
        Public Overridable Function Parse(ByVal TextIndex As Integer) As String
            Dim GlyphText As CharCode() = TextValue(TextIndex)
            Dim sb As New System.Text.StringBuilder
            For Each ci In GlyphText
                If ci.HasUnicode Then
                    sb.Append(ci.Character)
                End If
            Next
            Return sb.ToString.UnifyNewLineToCrLf
        End Function
    End Class

    ''' <summary>字库，字符码点到字形的映射。</summary>
    Public Class FontLib
        Protected Code As New List(Of CharCode)
        Protected Dict As New Dictionary(Of CharCode, IGlyph)
        Protected UnicodeDict As New Dictionary(Of Int32, CharCode)
        Protected CodeDict As New Dictionary(Of Int32, CharCode)

        ''' <summary>已重载。构造空字库。</summary>
        Public Sub New()
        End Sub

        ''' <summary>已重载。从字符码点构造字库。后面的字符若与前面重复，不会覆盖。</summary>
        Public Sub New(ByVal CharCodes As ICollection(Of CharCode))
            If CharCodes Is Nothing Then Throw New ArgumentNullException

            For Each c In CharCodes
                If Dict.ContainsKey(c) Then Continue For
                Code.Add(c)

                If c.HasUnicode AndAlso Not UnicodeDict.ContainsKey(c.Unicode) Then UnicodeDict.Add(c.Unicode, c)
                If c.HasCode AndAlso Not CodeDict.ContainsKey(c.Code) Then CodeDict.Add(c.Code, c)
            Next
        End Sub

        ''' <summary>已重载。从字符码点和字形构造字库。后面的字符若与前面重复，不会覆盖。</summary>
        ''' <remarks>字符码点和字形的数量要一致。</remarks>
        Public Sub New(ByVal CharCodes As IList(Of CharCode), ByVal CharGlyph As IList(Of IGlyph))
            If CharCodes Is Nothing Then Throw New ArgumentNullException
            If CharGlyph Is Nothing Then Throw New ArgumentNullException
            If CharCodes.Count <> CharGlyph.Count Then Throw New ArgumentException

            For n = 0 To CharCodes.Count - 1
                Dim c = CharCodes(n)
                Dim g = CharGlyph(n)
                If Dict.ContainsKey(c) Then Continue For
                Code.Add(c)
                Dict.Add(c, g)

                If c.HasUnicode AndAlso Not UnicodeDict.ContainsKey(c.Unicode) Then UnicodeDict.Add(c.Unicode, c)
                If c.HasCode AndAlso Not CodeDict.ContainsKey(c.Code) Then CodeDict.Add(c.Code, c)
            Next
        End Sub

        ''' <summary>字符数量。</summary>
        Public ReadOnly Property CharCount() As Integer
            Get
                If Code Is Nothing Then Return 0
                Return Code.Count
            End Get
        End Property

        ''' <summary>字符码点。</summary>
        Public ReadOnly Property CharCodes() As List(Of CharCode)
            Get
                Return New List(Of CharCode)(Code)
            End Get
        End Property

        ''' <summary>字符码点-字形映射。</summary>
        Public Function GetDict() As Dictionary(Of CharCode, IGlyph)
            Return New Dictionary(Of CharCode, IGlyph)(Dict)
        End Function

        ''' <summary>获得Unicode-字符码点映射。</summary>
        Public Function GetUnicodeDict() As Dictionary(Of Int32, CharCode)
            Return New Dictionary(Of Int32, CharCode)(UnicodeDict)
        End Function

        ''' <summary>获得自定义码点-字符码点映射。</summary>
        Public Function GetCodeDict() As Dictionary(Of Int32, CharCode)
            Return New Dictionary(Of Int32, CharCode)(CodeDict)
        End Function

        ''' <summary>从Unicode查找字符码点。</summary>
        Public Function LookupWithUnicode(ByVal Unicode As Int32) As CharCode
            If Unicode = -1 Then Return Nothing
            If UnicodeDict.ContainsKey(Unicode) Then Return UnicodeDict(Unicode)
            Return Nothing
        End Function

        ''' <summary>从自定义码点查找字符码点。</summary>
        Public Function LookupWithCode(ByVal Code As Int32) As CharCode
            If Code = -1 Then Return Nothing
            If CodeDict.ContainsKey(Code) Then Return CodeDict(Code)
            Return Nothing
        End Function

        ''' <summary>从字符(UTF-16B)查找字符码点。</summary>
        Public Function LookupWithChar(ByVal c As String) As CharCode
            Return LookupWithUnicode(Char32.FromString(c))
        End Function

        ''' <summary>添加字符，仅码点。后面的字符若与前面重复，不会覆盖。</summary>
        Public Sub Add(ByVal CharCode As CharCode)
            If Dict.ContainsKey(CharCode) Then Throw New InvalidOperationException

            Code.Add(CharCode)
            Dict.Add(CharCode, Nothing)

            If CharCode.HasUnicode AndAlso Not UnicodeDict.ContainsKey(CharCode.Unicode) Then UnicodeDict.Add(CharCode.Unicode, CharCode)
            If CharCode.HasCode AndAlso Not CodeDict.ContainsKey(CharCode.Code) Then CodeDict.Add(CharCode.Code, CharCode)
        End Sub

        ''' <summary>添加字符，从码点和字形。后面的字符若与前面重复，不会覆盖。</summary>
        Public Sub Add(ByVal CharCode As CharCode, ByVal CharGlyph As IGlyph)
            If Dict.ContainsKey(CharCode) Then Throw New InvalidOperationException

            Code.Add(CharCode)
            Dict.Add(CharCode, CharGlyph)

            If CharCode.HasUnicode AndAlso Not UnicodeDict.ContainsKey(CharCode.Unicode) Then UnicodeDict.Add(CharCode.Unicode, CharCode)
            If CharCode.HasCode AndAlso Not CodeDict.ContainsKey(CharCode.Code) Then CodeDict.Add(CharCode.Code, CharCode)
        End Sub

        ''' <summary>移除字符。</summary>
        Public Sub Remove(ByVal CharCode As CharCode)
            Code.Remove(CharCode)
            If Dict.ContainsKey(CharCode) Then Dict.Remove(CharCode)

            If CharCode.HasUnicode AndAlso UnicodeDict.ContainsKey(CharCode.Unicode) AndAlso UnicodeDict(CharCode.Unicode) Is CharCode Then UnicodeDict.Remove(CharCode.Unicode)
            If CharCode.HasCode AndAlso CodeDict.ContainsKey(CharCode.Code) AndAlso CodeDict(CharCode.Code) Is CharCode Then CodeDict.Remove(CharCode.Code)
        End Sub

        ''' <summary>移除字符。</summary>
        Public Sub RemoveMany(ByVal CharCodes As IEnumerable(Of CharCode))
            Dim NewCode As New List(Of CharCode)
            Dim CharCodeDict As New HashSet(Of CharCode)(CharCodes)
            For Each c In Code
                If CharCodeDict.Contains(c) Then Continue For
                NewCode.Add(c)
            Next
            Code = NewCode
            For Each CharCode In CharCodes
                If Dict.ContainsKey(CharCode) Then Dict.Remove(CharCode)

                If CharCode.HasUnicode AndAlso UnicodeDict.ContainsKey(CharCode.Unicode) AndAlso UnicodeDict(CharCode.Unicode) Is CharCode Then UnicodeDict.Remove(CharCode.Unicode)
                If CharCode.HasCode AndAlso CodeDict.ContainsKey(CharCode.Code) AndAlso CodeDict(CharCode.Code) Is CharCode Then CodeDict.Remove(CharCode.Code)
            Next
        End Sub

        ''' <summary>获取或设置指定字形。</summary>
        Default Public Property Item(ByVal CharCode As CharCode) As IGlyph
            Get
                Return Dict(CharCode)
            End Get
            Set(ByVal Value As IGlyph)
                Dict(CharCode) = Value
            End Set
        End Property

        ''' <summary>指示指定字符码点是否存在字形。</summary>
        Public ReadOnly Property HasGlyph(ByVal CharCode As CharCode) As Boolean
            Get
                Return Dict.ContainsKey(CharCode) AndAlso Dict(CharCode) IsNot Nothing
            End Get
        End Property

        ''' <summary>获取字库中的所有Unicode字符。</summary>
        Public Function GetEncodingString() As String
            Dim sb As New StringBuilder
            For Each c In CharCodes
                If c.HasUnicode Then sb.Append(Char32.ToString(c.Unicode))
            Next
            Return sb.ToString
        End Function
    End Class

    ''' <summary>字符码点值对，可用于码点转换。值均用Int32存储。</summary>
    Public Class CharCode
        Implements IEquatable(Of CharCode)

        ''' <summary>Unicode字符，UTF-32。</summary>
        Public Unicode As Char32 = -1
        ''' <summary>码点形式的自定义码点。</summary>
        Public Code As Int32 = -1
        ''' <summary>自定义码点的字节长度。</summary>
        Public CodeLength As Integer = -1

        ''' <summary>已重载。创建字符码点值对的实例。</summary>
        Public Sub New()
            Me.Unicode = -1
            Me.Code = -1
            Me.CodeLength = 0
        End Sub
        ''' <summary>已重载。创建字符码点值对的实例。</summary>
        ''' <param name="UniChar">Unicode字符，-1表示不存在。</param>
        ''' <param name="Code">自定义码点，-1表示不存在。</param>
        ''' <param name="CodeLength">自定义码点的字节长度，只能为-1、0、1、2、3、4。其中-1表示不明确，0表示码点不存在。</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal UniChar As Char32, ByVal Code As Int32, Optional ByVal CodeLength As Integer = -1)
            Me.Unicode = UniChar
            Me.Code = Code
            If CodeLength < -1 OrElse CodeLength > 4 Then Throw New ArgumentException
            Me.CodeLength = CodeLength
        End Sub

        ''' <summary>创建字符码点值对的实例。</summary>
        Public Shared Function FromNothing() As CharCode
            Return New CharCode(-1, -1, 0)
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="UniChar">Unicode字符。</param>
        Public Shared Function FromUniChar(ByVal UniChar As Char32) As CharCode
            Return New CharCode(UniChar, -1, 0)
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Unicode">Unicode码。</param>
        Public Shared Function FromUnicode(ByVal Unicode As Int32) As CharCode
            Return New CharCode(Unicode, -1, 0)
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="Code">自定义码点，-1表示不存在。</param>
        ''' <param name="CodeLength">自定义码点的字节长度，只能为-1、0、1、2、3、4。其中-1表示不明确，0表示码点不存在。</param>
        Public Shared Function FromCode(ByVal Code As Int32, Optional ByVal CodeLength As Integer = -1) As CharCode
            Return New CharCode(-1, Code, CodeLength)
        End Function

        ''' <summary>创建字符码点值对的实例。</summary>
        ''' <param name="CodeString">自定义码点的字符串形式，""表示不存在。</param>
        Public Shared Function FromCodeString(ByVal CodeString As String) As CharCode
            If CodeString = "" Then
                Return New CharCode(-1, -1, 0)
            Else
                Return New CharCode(-1, Integer.Parse(CodeString, Globalization.NumberStyles.HexNumber), (CodeString.Length + 1) \ 2)
            End If
        End Function

        Public Function ToStringCode() As StringCode
            If HasUnicode And HasCode Then
                Return StringCode.FromUnicodeStringAndCodeString(Character, CodeString)
            ElseIf HasUnicode Then
                Return StringCode.FromUnicodeString(Character)
            ElseIf HasCode Then
                Return StringCode.FromCodeString(CodeString)
            Else
                Return StringCode.FromNothing
            End If
        End Function

        ''' <summary>字符。</summary>
        Public Property Character() As String
            Get
                If HasUnicode Then Return Unicode.ToString
                Throw New InvalidOperationException
            End Get
            Set(ByVal Value As String)
                Unicode = Char32.FromString(Value)
            End Set
        End Property

        ''' <summary>指示是否是控制符。</summary>
        Public Overridable ReadOnly Property IsControlChar() As Boolean
            Get
                If HasUnicode Then Return Unicode >= 0 AndAlso Unicode <= &H1F
                Return False
            End Get
        End Property

        ''' <summary>指示是否是换行符。</summary>
        Public Overridable ReadOnly Property IsNewLine() As Boolean
            Get
                Return Unicode = 10
            End Get
        End Property

        ''' <summary>指示是否已建立映射。</summary>
        Public ReadOnly Property IsCodeMappable() As Boolean
            Get
                Return HasUnicode AndAlso HasCode
            End Get
        End Property

        ''' <summary>指示Unicode是否存在。</summary>
        Public ReadOnly Property HasUnicode() As Boolean
            Get
                Return Unicode <> -1
            End Get
        End Property

        ''' <summary>指示自定义码点是否存在。</summary>
        Public ReadOnly Property HasCode() As Boolean
            Get
                Return CodeLength <> 0
            End Get
        End Property

        ''' <summary>生成显示用字符串。</summary>
        Public Overrides Function ToString() As String
            Dim List As New List(Of String)
            If HasUnicode Then
                List.Add(String.Format("U+{0:X4}", Unicode.Value))
                If Not IsControlChar Then List.Add(String.Format("""{0}""", Unicode.ToString))
            End If
            If HasCode Then List.Add(String.Format("Code = {0}", CodeString()))

            Return "CharCode{" & String.Join(", ", List.ToArray) & "}"
        End Function

        ''' <summary>自定义码点的字符串形式。</summary>
        Public Property CodeString() As String
            Get
                Select Case CodeLength
                    Case Is > 0
                        Return Code.ToString("X" & (CodeLength * 2))
                    Case 0
                        Return ""
                    Case Else
                        Select Case Code
                            Case 0 To &HFF
                                Return Code.ToString("X2")
                            Case &H100 To &HFFFF
                                Return Code.ToString("X4")
                            Case &H10000 To &HFFFFFF
                                Return Code.ToString("X6")
                            Case Else
                                Return Code.ToString("X8")
                        End Select
                End Select
            End Get
            Set(ByVal Value As String)
                If Value = "" Then
                    Code = -1
                    CodeLength = 0
                Else
                    Code = Integer.Parse(Value, Globalization.NumberStyles.HexNumber)
                    CodeLength = (Value.Length + 1) \ 2
                End If
            End Set
        End Property

        ''' <summary>比较两个字符码点是否相等。</summary>
        Public Overloads Function Equals(ByVal other As CharCode) As Boolean Implements System.IEquatable(Of CharCode).Equals
            Return Unicode = other.Unicode AndAlso Code = other.Code
        End Function

        ''' <summary>比较两个字符码点是否相等。</summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If Me Is obj Then Return True
            If obj Is Nothing Then Return False
            Dim c = TryCast(obj, CharCode)
            If c Is Nothing Then Return False
            Return Equals(c)
        End Function

        ''' <summary>获取字符码点的HashCode。</summary>
        Public Overrides Function GetHashCode() As Integer
            Return Unicode.Value Xor ((Code << 16) Or ((Code >> 16) And &HFFFF))
        End Function
    End Class

    ''' <summary>字符码点值对字符串。</summary>
    Public Module CharCodeString
        ''' <summary>转换UTF-32字符串到CharCode()。</summary>
        Public Function FromString32(ByVal s As Char32()) As CharCode()
            Dim CharCodes = New CharCode(s.Length - 1) {}
            For n = 0 To s.Length - 1
                CharCodes(n) = CharCode.FromUniChar(s(n))
            Next
            Return CharCodes
        End Function

        ''' <summary>转换UTF-16 Big-Endian字符串到UTF-32字符串。</summary>
        Public Function FromString16(ByVal s As String) As CharCode()
            Dim cl As New List(Of CharCode)

            For n As Integer = 0 To s.Length - 1
                Dim c As Char = s(n)
                Dim H As Int32 = AscQ(c)
                If H >= &HD800 AndAlso H <= &HDBFF Then
                    cl.Add(CharCode.FromUniChar(Char32.FromString(c & s(n + 1))))
                    n += 1
                Else
                    cl.Add(CharCode.FromUniChar(c))
                End If
            Next

            Return cl.ToArray
        End Function
    End Module
End Namespace
